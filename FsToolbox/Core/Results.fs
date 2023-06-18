namespace FsToolbox.Core.Results

open System
open Microsoft.FSharp.Core.LanguagePrimitives

type FailureResult =
    { Message: string
      DisplayMessage: string
      Exception: exn option }

    static member Aggregate(failures: FailureResult seq, displayMessage: string) =
        let exceptions = failures |> Seq.choose (fun f -> f.Exception) |> List.ofSeq

        { Message =
            "The following failures occurred"
            :: (failures |> List.ofSeq |> List.map (fun f -> f.Message))
            |> String.concat Environment.NewLine
          DisplayMessage = displayMessage
          Exception =
            match exceptions.IsEmpty with
            | true -> None
            | false -> AggregateException(exceptions) :> exn |> Some }

[<RequireQualifiedAccess>]
module FailureResult =

    let separateResults<'T> (results: Result<'T, FailureResult> seq) =
        results
        |> Seq.fold
            (fun (ok, errors) r ->
                match r with
                | Ok v -> v :: ok, errors
                | Error e -> ok, e :: errors)
            ([], [])
        |> fun (ok, errors) -> ok |> List.rev, errors |> List.rev

    let separateResultsAsync<'T> (results: Async<Result<'T, FailureResult> seq>) =
        async {
            let! r = results

            return
                r
                |> Seq.fold
                    (fun (ok, errors) r ->
                        match r with
                        | Ok v -> v :: ok, errors
                        | Error e -> ok, e :: errors)
                    ([], [])
                |> fun (ok, errors) -> ok |> List.rev, errors |> List.rev
        }


    let unwrap<'T> (result: Result<'T, FailureResult>) =
        match result with
        | Ok v -> v
        | Error e ->
            match e.Exception with
            | Some ex -> raise ex
            | None -> failwith e.Message

[<RequireQualifiedAccess>]
type FetchResult<'T> =
    | Success of 'T
    | Failure of FailureResult

    member fr.ToResult() =
        match fr with
        | FetchResult.Success r -> Ok r
        | FetchResult.Failure f -> Error f

[<RequireQualifiedAccess>]
type CreateResult<'T> =
    | Success of 'T
    | Failure of FailureResult

    member ar.ToResult() =
        match ar with
        | CreateResult.Success r -> Ok r
        | CreateResult.Failure f -> Error f

[<RequireQualifiedAccess>]
type UpdateResult<'T> =
    | Success of 'T
    | Failure of FailureResult

    member ur.ToResult() =
        match ur with
        | UpdateResult.Success r -> Ok r
        | UpdateResult.Failure f -> Error f

[<RequireQualifiedAccess>]
type ActionResult<'T> =
    | Success of 'T
    | Failure of FailureResult

    member ar.ToResult() =
        match ar with
        | ActionResult.Success r -> Ok r
        | ActionResult.Failure f -> Error f

[<RequireQualifiedAccess>]
module FetchResult =

    let defaultWith<'T> (fn: unit -> 'T) (result: FetchResult<'T>) =
        match result with
        | FetchResult.Success r -> r
        | FetchResult.Failure _ -> fn ()

    let defaultValue<'T> (value: 'T) (result: FetchResult<'T>) = defaultWith (fun _ -> value) result

    let map<'T, 'U> (fn: 'T -> 'U) (result: FetchResult<'T>) =
        match result with
        | FetchResult.Success r -> fn r |> FetchResult.Success
        | FetchResult.Failure f -> FetchResult.Failure f

    let mapFailure<'T> (fn: unit -> 'T) (result: FetchResult<'T>) =
        match result with
        | FetchResult.Success r -> FetchResult.Success r
        | FetchResult.Failure _ -> fn () |> FetchResult.Success

    let bind<'T, 'U> (fn: 'T -> FetchResult<'U>) (result: FetchResult<'T>) =
        match result with
        | FetchResult.Success r -> fn r
        | FetchResult.Failure f -> FetchResult.Failure f

    let bindFailure<'T> (fn: unit -> FetchResult<'T>) (result: FetchResult<'T>) =
        match result with
        | FetchResult.Success r -> FetchResult.Success r
        | FetchResult.Failure _ -> fn ()

    let combine<'T1, 'T2, 'U> (result2: FetchResult<'T2>) (result1: FetchResult<'T1>) =
        match result1, result2 with
        | FetchResult.Success v1, FetchResult.Success v2 -> FetchResult.Success(v1, v2)
        | FetchResult.Failure f, _ -> FetchResult.Failure f
        | _, FetchResult.Failure f -> FetchResult.Failure f

    let chain<'T1, 'T2, 'U> (chainFn: 'T1 -> 'T2 -> 'U) (result2: FetchResult<'T2>) (result1: FetchResult<'T1>) =
        match result1, result2 with
        | FetchResult.Success v1, FetchResult.Success v2 -> chainFn v1 v2 |> FetchResult.Success
        | FetchResult.Failure f, _ -> FetchResult.Failure f
        | _, FetchResult.Failure f -> FetchResult.Failure f

    let append<'T1, 'T2, 'T3, 'U> (result2: FetchResult<'T3>) (result1: FetchResult<'T1 * 'T2>) =
        match result1, result2 with
        | FetchResult.Success(v1, v2), FetchResult.Success v3 -> FetchResult.Success(v1, v2, v3)
        | FetchResult.Failure f, _ -> FetchResult.Failure f
        | _, FetchResult.Failure f -> FetchResult.Failure f

    /// <summary>
    ///     Merge to fetch results, the second one is based of the first ones result value.
    ///     For example, fetch a user then us the user's company id to fetch a company.
    ///     The results are merged via a merge function that takes both result values
    ///     and produces a new one.
    /// </summary>
    let merge<'T1, 'T2, 'U> (mergeFn: 'T1 -> 'T2 -> 'U) (result2: 'T1 -> FetchResult<'T2>) (result: FetchResult<'T1>) =
        // QUESTION would this make more sense to be `pipe`?
        match result with
        | FetchResult.Success v1 ->
            match result2 v1 with
            | FetchResult.Success v2 -> mergeFn v1 v2 |> FetchResult.Success
            | FetchResult.Failure f -> FetchResult.Failure f
        | FetchResult.Failure f -> FetchResult.Failure f

    /// <summary>
    ///     A wrapper around `merge`, with a merge function that takes both result values and combines them into a tuple.
    /// </summary>
    let pipe<'T1, 'T2, 'U> (result2: 'T1 -> FetchResult<'T2>) (result: FetchResult<'T1>) =
        // QUESTION would this make more sense to be `merge`?
        merge (fun v1 v2 -> v1, v2) <| result2 <| result

    let toResult<'T> (fetchResult: FetchResult<'T>) = fetchResult.ToResult()

    let toResult2<'T1, 'T2> (result1: FetchResult<'T1>) (result2: FetchResult<'T2>) =
        match result1.ToResult(), result2.ToResult() with
        | Ok r1, Ok r2 -> Ok(r1, r2)
        | Error e, _
        | _, Error e -> Error e

    let toResult3<'T1, 'T2, 'T3> (result1: FetchResult<'T1>) (result2: FetchResult<'T2>) (result3: FetchResult<'T3>) =
        match result1.ToResult(), result2.ToResult(), result3.ToResult() with
        | Ok r1, Ok r2, Ok r3 -> Ok(r1, r2, r3)
        | Error e, _, _
        | _, Error e, _
        | _, _, Error e -> Error e

    let fromResult<'T> (result: Result<'T, FailureResult>) =
        match result with
        | Ok v -> FetchResult.Success v
        | Error f -> FetchResult.Failure f

    let toOptionOrElse<'T> (fn: unit -> 'T option) (result: FetchResult<'T>) =
        match result with
        | FetchResult.Success v -> Some v
        | FetchResult.Failure _ -> fn ()

    let toOption<'T> (result: FetchResult<'T>) = toOptionOrElse (fun _ -> None) result

    let mapToOption<'T, 'U> (fn: 'T -> FetchResult<'U>) (value: 'T option) =
        value |> Option.bind (fun v -> fn v |> toOption)

    let bindToAction<'T, 'U> (actionFn: 'T -> ActionResult<'U>) (result: FetchResult<'T>) =
        match result with
        | FetchResult.Success v -> actionFn v
        | FetchResult.Failure f -> ActionResult.Failure f

    let mapToAction<'T, 'U> (actionFn: 'T -> 'U) (result: FetchResult<'T>) =
        match result with
        | FetchResult.Success v -> actionFn v |> ActionResult.Success
        | FetchResult.Failure f -> ActionResult.Failure f

    let bindToCreate<'T, 'U> (createFn: 'T -> CreateResult<'U>) (result: FetchResult<'T>) =
        match result with
        | FetchResult.Success v -> createFn v
        | FetchResult.Failure f -> CreateResult.Failure f

    let mapToCreate<'T, 'U> (createFn: 'T -> CreateResult<'U>) (result: FetchResult<'T>) =
        match result with
        | FetchResult.Success v -> createFn v
        | FetchResult.Failure f -> CreateResult.Failure f

    let bindToUpdate<'T, 'U> (createFn: 'T -> UpdateResult<'U>) (result: FetchResult<'T>) =
        match result with
        | FetchResult.Success v -> createFn v
        | FetchResult.Failure f -> UpdateResult.Failure f

    let mapToUpdate<'T, 'U> (createFn: 'T -> UpdateResult<'U>) (result: FetchResult<'T>) =
        match result with
        | FetchResult.Success v -> createFn v
        | FetchResult.Failure f -> UpdateResult.Failure f

    let toActionResult<'T> (result: FetchResult<'T>) =
        match result with
        | FetchResult.Success v -> ActionResult.Success v
        | FetchResult.Failure f -> ActionResult.Failure f

    let fromActionResult<'T> (result: ActionResult<'T>) =
        match result with
        | ActionResult.Success v -> FetchResult.Success v
        | ActionResult.Failure f -> FetchResult.Failure f

    let toCreateResult<'T> (result: FetchResult<'T>) =
        match result with
        | FetchResult.Success v -> CreateResult.Success v
        | FetchResult.Failure f -> CreateResult.Failure f

    let fromCreateResult<'T> (result: CreateResult<'T>) =
        match result with
        | CreateResult.Success v -> FetchResult.Success v
        | CreateResult.Failure f -> FetchResult.Failure f

    let toUpdateResult<'T> (result: FetchResult<'T>) =
        match result with
        | FetchResult.Success v -> UpdateResult.Success v
        | FetchResult.Failure f -> UpdateResult.Failure f

    let fromUpdateResult<'T> (result: UpdateResult<'T>) =
        match result with
        | UpdateResult.Success v -> FetchResult.Success v
        | UpdateResult.Failure f -> FetchResult.Failure f

    let iter<'T> (fn: 'T -> unit) (result: FetchResult<'T>) =
        match result with
        | FetchResult.Success v -> fn v
        | FetchResult.Failure _ -> ()

    let orElse<'T> (ifFailure: FetchResult<'T>) (result: FetchResult<'T>) =
        match result with
        | FetchResult.Success _ -> result
        | FetchResult.Failure _ -> ifFailure

    let orElseWith<'T> (fn: unit -> FetchResult<'T>) (result: FetchResult<'T>) =
        match result with
        | FetchResult.Success _ -> result
        | FetchResult.Failure _ -> fn ()

    let unzipResults (results: FetchResult<'T> seq) =
        // NOTE would resize arrays be better for this?
        results
        |> Seq.fold
            (fun (ok, errors) r ->
                match r with
                | FetchResult.Success v -> v :: ok, errors
                | FetchResult.Failure f -> ok, f :: errors)
            ([], [])
        |> fun (ok, errors) -> ok |> List.rev, errors |> List.rev
        
    let aggregateResults<'T> (errorDisplayMessage: string) (results: FetchResult<'T> seq) =
        results
        |> unzipResults
        |> fun (ok, err) ->
            FetchResult.Success ok,

            (match err.IsEmpty with
             | true -> None
             | false ->
                 FailureResult.Aggregate(err, errorDisplayMessage)
                 |> FetchResult.Failure
                 |> Some)

[<RequireQualifiedAccess>]
module ActionResult =

    let defaultWith<'T> (fn: unit -> 'T) (result: ActionResult<'T>) =
        match result with
        | ActionResult.Success r -> r
        | ActionResult.Failure _ -> fn ()

    let defaultValue<'T> (value: 'T) (result: ActionResult<'T>) = defaultWith (fun _ -> value) result

    let map<'T, 'U> (fn: 'T -> 'U) (result: ActionResult<'T>) =
        match result with
        | ActionResult.Success r -> fn r |> ActionResult.Success
        | ActionResult.Failure f -> ActionResult.Failure f

    let mapFailure<'T> (fn: unit -> 'T) (result: ActionResult<'T>) =
        match result with
        | ActionResult.Success r -> ActionResult.Success r
        | ActionResult.Failure _ -> fn () |> ActionResult.Success

    let bind<'T, 'U> (fn: 'T -> ActionResult<'U>) (result: ActionResult<'T>) =
        match result with
        | ActionResult.Success r -> fn r
        | ActionResult.Failure f -> ActionResult.Failure f

    let bindFailure<'T> (fn: unit -> ActionResult<'T>) (result: ActionResult<'T>) =
        match result with
        | ActionResult.Success r -> ActionResult.Success r
        | ActionResult.Failure _ -> fn ()

    let combine<'T1, 'T2, 'U> (result2: ActionResult<'T2>) (result1: ActionResult<'T1>) =
        match result1, result2 with
        | ActionResult.Success v1, ActionResult.Success v2 -> ActionResult.Success(v1, v2)
        | ActionResult.Failure f, _ -> ActionResult.Failure f
        | _, ActionResult.Failure f -> ActionResult.Failure f

    let chain<'T1, 'T2, 'U> (chainFn: 'T1 -> 'T2 -> 'U) (result2: ActionResult<'T2>) (result1: ActionResult<'T1>) =
        match result1, result2 with
        | ActionResult.Success v1, ActionResult.Success v2 -> chainFn v1 v2 |> ActionResult.Success
        | ActionResult.Failure f, _ -> ActionResult.Failure f
        | _, ActionResult.Failure f -> ActionResult.Failure f

    let append<'T1, 'T2, 'T3, 'U> (result2: ActionResult<'T3>) (result1: ActionResult<'T1 * 'T2>) =
        match result1, result2 with
        | ActionResult.Success(v1, v2), ActionResult.Success v3 -> ActionResult.Success(v1, v2, v3)
        | ActionResult.Failure f, _ -> ActionResult.Failure f
        | _, ActionResult.Failure f -> ActionResult.Failure f

    /// <summary>
    ///     Merge to fetch results, the second one is based of the first ones result value.
    /// </summary>
    let merge<'T1, 'T2, 'U> (mergeFn: 'T1 -> 'T2 -> 'U) (result2: 'T1 -> ActionResult<'T2>) (result: ActionResult<'T1>) =
        // QUESTION would this make more sense to be `pipe`?
        match result with
        | ActionResult.Success v1 ->
            match result2 v1 with
            | ActionResult.Success v2 -> mergeFn v1 v2 |> ActionResult.Success
            | ActionResult.Failure f -> ActionResult.Failure f
        | ActionResult.Failure f -> ActionResult.Failure f

    /// <summary>
    ///     A wrapper around `merge`, with a merge function that takes both result values and combines them into a tuple.
    /// </summary>
    let pipe<'T1, 'T2, 'U> (result2: 'T1 -> ActionResult<'T2>) (result: ActionResult<'T1>) =
        // QUESTION would this make more sense to be `merge`?
        merge (fun v1 v2 -> v1, v2) <| result2 <| result

    let toResult<'T> (fetchResult: ActionResult<'T>) = fetchResult.ToResult()

    let toResult2<'T1, 'T2> (result1: ActionResult<'T1>) (result2: ActionResult<'T2>) =
        match result1.ToResult(), result2.ToResult() with
        | Ok r1, Ok r2 -> Ok(r1, r2)
        | Error e, _
        | _, Error e -> Error e

    let toResult3<'T1, 'T2, 'T3> (result1: ActionResult<'T1>) (result2: ActionResult<'T2>) (result3: ActionResult<'T3>) =
        match result1.ToResult(), result2.ToResult(), result3.ToResult() with
        | Ok r1, Ok r2, Ok r3 -> Ok(r1, r2, r3)
        | Error e, _, _
        | _, Error e, _
        | _, _, Error e -> Error e

    let fromResult<'T> (result: Result<'T, FailureResult>) =
        match result with
        | Ok v -> ActionResult.Success v
        | Error f -> ActionResult.Failure f

    let toOptionOrElse<'T> (fn: unit -> 'T option) (result: ActionResult<'T>) =
        match result with
        | ActionResult.Success v -> Some v
        | ActionResult.Failure _ -> fn ()

    let toOption<'T> (result: ActionResult<'T>) = toOptionOrElse (fun _ -> None) result

    let mapToOption<'T, 'U> (fn: 'T -> ActionResult<'U>) (value: 'T option) =
        value |> Option.bind (fun v -> fn v |> toOption)

    let bindToFetch<'T, 'U> (fetchFn: 'T -> FetchResult<'U>) (result: ActionResult<'T>) =
        match result with
        | ActionResult.Success v -> fetchFn v
        | ActionResult.Failure f -> FetchResult.Failure f

    let mapToFetch<'T, 'U> (fetchFn: 'T -> 'U) (result: ActionResult<'T>) =
        match result with
        | ActionResult.Success v -> fetchFn v |> FetchResult.Success
        | ActionResult.Failure f -> FetchResult.Failure f

    let bindToCreate<'T, 'U> (createFn: 'T -> CreateResult<'U>) (result: ActionResult<'T>) =
        match result with
        | ActionResult.Success v -> createFn v
        | ActionResult.Failure f -> CreateResult.Failure f

    let mapToCreate<'T, 'U> (createFn: 'T -> CreateResult<'U>) (result: ActionResult<'T>) =
        match result with
        | ActionResult.Success v -> createFn v
        | ActionResult.Failure f -> CreateResult.Failure f

    let bindToUpdate<'T, 'U> (createFn: 'T -> UpdateResult<'U>) (result: ActionResult<'T>) =
        match result with
        | ActionResult.Success v -> createFn v
        | ActionResult.Failure f -> UpdateResult.Failure f

    let mapToUpdate<'T, 'U> (createFn: 'T -> UpdateResult<'U>) (result: ActionResult<'T>) =
        match result with
        | ActionResult.Success v -> createFn v
        | ActionResult.Failure f -> UpdateResult.Failure f

    let toFetchResult<'T> (result: ActionResult<'T>) =
        match result with
        | ActionResult.Success v -> FetchResult.Success v
        | ActionResult.Failure f -> FetchResult.Failure f

    let fromFetchResult<'T> (result: FetchResult<'T>) =
        match result with
        | FetchResult.Success v -> ActionResult.Success v
        | FetchResult.Failure f -> ActionResult.Failure f

    let toCreateResult<'T> (result: ActionResult<'T>) =
        match result with
        | ActionResult.Success v -> CreateResult.Success v
        | ActionResult.Failure f -> CreateResult.Failure f

    let fromCreateResult<'T> (result: CreateResult<'T>) =
        match result with
        | CreateResult.Success v -> ActionResult.Success v
        | CreateResult.Failure f -> ActionResult.Failure f

    let toUpdateResult<'T> (result: ActionResult<'T>) =
        match result with
        | ActionResult.Success v -> UpdateResult.Success v
        | ActionResult.Failure f -> UpdateResult.Failure f

    let fromUpdateResult<'T> (result: UpdateResult<'T>) =
        match result with
        | UpdateResult.Success v -> ActionResult.Success v
        | UpdateResult.Failure f -> ActionResult.Failure f

    let iter<'T> (fn: 'T -> unit) (result: ActionResult<'T>) =
        match result with
        | ActionResult.Success v -> fn v
        | ActionResult.Failure _ -> ()

    let orElse<'T> (ifFailure: ActionResult<'T>) (result: ActionResult<'T>) =
        match result with
        | ActionResult.Success _ -> result
        | ActionResult.Failure _ -> ifFailure

    let orElseWith<'T> (fn: unit -> ActionResult<'T>) (result: ActionResult<'T>) =
        match result with
        | ActionResult.Success _ -> result
        | ActionResult.Failure _ -> fn ()

    let unzipResults (results: ActionResult<'T> seq) =
        // NOTE would resize arrays be better for this?
        results
        |> Seq.fold
            (fun (ok, errors) r ->
                match r with
                | ActionResult.Success v -> v :: ok, errors
                | ActionResult.Failure f -> ok, f :: errors)
            ([], [])
        |> fun (ok, errors) -> ok |> List.rev, errors |> List.rev
        
    let aggregateResults<'T> (errorDisplayMessage: string) (results: ActionResult<'T> seq) =
        results
        |> unzipResults
        |> fun (ok, err) ->
            ActionResult.Success ok,

            (match err.IsEmpty with
             | true -> None
             | false ->
                 FailureResult.Aggregate(err, errorDisplayMessage)
                 |> ActionResult.Failure
                 |> Some)
            
[<RequireQualifiedAccess>]
module CreateResult =

    let defaultWith<'T> (fn: unit -> 'T) (result: CreateResult<'T>) =
        match result with
        | CreateResult.Success r -> r
        | CreateResult.Failure _ -> fn ()

    let defaultValue<'T> (value: 'T) (result: CreateResult<'T>) = defaultWith (fun _ -> value) result

    let map<'T, 'U> (fn: 'T -> 'U) (result: CreateResult<'T>) =
        match result with
        | CreateResult.Success r -> fn r |> CreateResult.Success
        | CreateResult.Failure f -> CreateResult.Failure f

    let mapFailure<'T> (fn: unit -> 'T) (result: CreateResult<'T>) =
        match result with
        | CreateResult.Success r -> CreateResult.Success r
        | CreateResult.Failure _ -> fn () |> CreateResult.Success

    let bind<'T, 'U> (fn: 'T -> CreateResult<'U>) (result: CreateResult<'T>) =
        match result with
        | CreateResult.Success r -> fn r
        | CreateResult.Failure f -> CreateResult.Failure f

    let bindFailure<'T> (fn: unit -> CreateResult<'T>) (result: CreateResult<'T>) =
        match result with
        | CreateResult.Success r -> CreateResult.Success r
        | CreateResult.Failure _ -> fn ()

    let combine<'T1, 'T2, 'U> (result2: CreateResult<'T2>) (result1: CreateResult<'T1>) =
        match result1, result2 with
        | CreateResult.Success v1, CreateResult.Success v2 -> CreateResult.Success(v1, v2)
        | CreateResult.Failure f, _ -> CreateResult.Failure f
        | _, CreateResult.Failure f -> CreateResult.Failure f

    let chain<'T1, 'T2, 'U> (chainFn: 'T1 -> 'T2 -> 'U) (result2: CreateResult<'T2>) (result1: CreateResult<'T1>) =
        match result1, result2 with
        | CreateResult.Success v1, CreateResult.Success v2 -> chainFn v1 v2 |> CreateResult.Success
        | CreateResult.Failure f, _ -> CreateResult.Failure f
        | _, CreateResult.Failure f -> CreateResult.Failure f

    let append<'T1, 'T2, 'T3, 'U> (result2: CreateResult<'T3>) (result1: CreateResult<'T1 * 'T2>) =
        match result1, result2 with
        | CreateResult.Success(v1, v2), CreateResult.Success v3 -> CreateResult.Success(v1, v2, v3)
        | CreateResult.Failure f, _ -> CreateResult.Failure f
        | _, CreateResult.Failure f -> CreateResult.Failure f

    /// <summary>
    ///     Merge to create results, the second one is based of the first ones result value.
    /// </summary>
    let merge<'T1, 'T2, 'U> (mergeFn: 'T1 -> 'T2 -> 'U) (result2: 'T1 -> CreateResult<'T2>) (result: CreateResult<'T1>) =
        // QUESTION would this make more sense to be `pipe`?
        match result with
        | CreateResult.Success v1 ->
            match result2 v1 with
            | CreateResult.Success v2 -> mergeFn v1 v2 |> CreateResult.Success
            | CreateResult.Failure f -> CreateResult.Failure f
        | CreateResult.Failure f -> CreateResult.Failure f

    /// <summary>
    ///     A wrapper around `merge`, with a merge function that takes both result values and combines them into a tuple.
    /// </summary>
    let pipe<'T1, 'T2, 'U> (result2: 'T1 -> CreateResult<'T2>) (result: CreateResult<'T1>) =
        // QUESTION would this make more sense to be `merge`?
        merge (fun v1 v2 -> v1, v2) <| result2 <| result

    let toResult<'T> (result: CreateResult<'T>) = result.ToResult()

    let toResult2<'T1, 'T2> (result1: CreateResult<'T1>) (result2: CreateResult<'T2>) =
        match result1.ToResult(), result2.ToResult() with
        | Ok r1, Ok r2 -> Ok(r1, r2)
        | Error e, _
        | _, Error e -> Error e

    let toResult3<'T1, 'T2, 'T3> (result1: CreateResult<'T1>) (result2: CreateResult<'T2>) (result3: CreateResult<'T3>) =
        match result1.ToResult(), result2.ToResult(), result3.ToResult() with
        | Ok r1, Ok r2, Ok r3 -> Ok(r1, r2, r3)
        | Error e, _, _
        | _, Error e, _
        | _, _, Error e -> Error e

    let fromResult<'T> (result: Result<'T, FailureResult>) =
        match result with
        | Ok v -> CreateResult.Success v
        | Error f -> CreateResult.Failure f

    let toOptionOrElse<'T> (fn: unit -> 'T option) (result: CreateResult<'T>) =
        match result with
        | CreateResult.Success v -> Some v
        | CreateResult.Failure _ -> fn ()

    let toOption<'T> (result: CreateResult<'T>) = toOptionOrElse (fun _ -> None) result

    let mapToOption<'T, 'U> (fn: 'T -> CreateResult<'U>) (value: 'T option) =
        value |> Option.bind (fun v -> fn v |> toOption)

    let bindToFetch<'T, 'U> (fetchFn: 'T -> FetchResult<'U>) (result: CreateResult<'T>) =
        match result with
        | CreateResult.Success v -> fetchFn v
        | CreateResult.Failure f -> FetchResult.Failure f

    let mapToFetch<'T, 'U> (fetchFn: 'T -> 'U) (result: CreateResult<'T>) =
        match result with
        | CreateResult.Success v -> fetchFn v |> FetchResult.Success
        | CreateResult.Failure f -> FetchResult.Failure f

    let bindToAction<'T, 'U> (actionFn: 'T -> ActionResult<'U>) (result: CreateResult<'T>) =
        match result with
        | CreateResult.Success v -> actionFn v
        | CreateResult.Failure f -> ActionResult.Failure f

    let mapToAction<'T, 'U> (actionFn: 'T -> ActionResult<'U>) (result: CreateResult<'T>) =
        match result with
        | CreateResult.Success v -> actionFn v
        | CreateResult.Failure f -> ActionResult.Failure f

    let bindToUpdate<'T, 'U> (createFn: 'T -> UpdateResult<'U>) (result: CreateResult<'T>) =
        match result with
        | CreateResult.Success v -> createFn v
        | CreateResult.Failure f -> UpdateResult.Failure f

    let mapToUpdate<'T, 'U> (createFn: 'T -> UpdateResult<'U>) (result: CreateResult<'T>) =
        match result with
        | CreateResult.Success v -> createFn v
        | CreateResult.Failure f -> UpdateResult.Failure f

    let toFetchResult<'T> (result: CreateResult<'T>) =
        match result with
        | CreateResult.Success v -> FetchResult.Success v
        | CreateResult.Failure f -> FetchResult.Failure f

    let fromFetchResult<'T> (result: FetchResult<'T>) =
        match result with
        | FetchResult.Success v -> CreateResult.Success v
        | FetchResult.Failure f -> CreateResult.Failure f

    let toActionResult<'T> (result: CreateResult<'T>) =
        match result with
        | CreateResult.Success v -> ActionResult.Success v
        | CreateResult.Failure f -> ActionResult.Failure f

    let fromActionResult<'T> (result: ActionResult<'T>) =
        match result with
        | ActionResult.Success v -> CreateResult.Success v
        | ActionResult.Failure f -> CreateResult.Failure f

    let toUpdateResult<'T> (result: CreateResult<'T>) =
        match result with
        | CreateResult.Success v -> UpdateResult.Success v
        | CreateResult.Failure f -> UpdateResult.Failure f

    let fromUpdateResult<'T> (result: UpdateResult<'T>) =
        match result with
        | UpdateResult.Success v -> CreateResult.Success v
        | UpdateResult.Failure f -> CreateResult.Failure f

    let iter<'T> (fn: 'T -> unit) (result: CreateResult<'T>) =
        match result with
        | CreateResult.Success v -> fn v
        | CreateResult.Failure _ -> ()

    let orElse<'T> (ifFailure: CreateResult<'T>) (result: CreateResult<'T>) =
        match result with
        | CreateResult.Success _ -> result
        | CreateResult.Failure _ -> ifFailure

    let orElseWith<'T> (fn: unit -> CreateResult<'T>) (result: CreateResult<'T>) =
        match result with
        | CreateResult.Success _ -> result
        | CreateResult.Failure _ -> fn ()

    let unzipResults (results: CreateResult<'T> seq) =
        // NOTE would resize arrays be better for this?
        results
        |> Seq.fold
            (fun (ok, errors) r ->
                match r with
                | CreateResult.Success v -> v :: ok, errors
                | CreateResult.Failure f -> ok, f :: errors)
            ([], [])
        |> fun (ok, errors) -> ok |> List.rev, errors |> List.rev
        
    let aggregateResults<'T> (errorDisplayMessage: string) (results: CreateResult<'T> seq) =
        results
        |> unzipResults
        |> fun (ok, err) ->
            ActionResult.Success ok,

            (match err.IsEmpty with
             | true -> None
             | false ->
                 FailureResult.Aggregate(err, errorDisplayMessage)
                 |> ActionResult.Failure
                 |> Some)
            
[<RequireQualifiedAccess>]
module UpdateResult =

    let defaultWith<'T> (fn: unit -> 'T) (result: UpdateResult<'T>) =
        match result with
        | UpdateResult.Success r -> r
        | UpdateResult.Failure _ -> fn ()

    let defaultValue<'T> (value: 'T) (result: UpdateResult<'T>) = defaultWith (fun _ -> value) result

    let map<'T, 'U> (fn: 'T -> 'U) (result: UpdateResult<'T>) =
        match result with
        | UpdateResult.Success r -> fn r |> UpdateResult.Success
        | UpdateResult.Failure f -> UpdateResult.Failure f

    let mapFailure<'T> (fn: unit -> 'T) (result: UpdateResult<'T>) =
        match result with
        | UpdateResult.Success r -> UpdateResult.Success r
        | UpdateResult.Failure _ -> fn () |> UpdateResult.Success

    let bind<'T, 'U> (fn: 'T -> UpdateResult<'U>) (result: UpdateResult<'T>) =
        match result with
        | UpdateResult.Success r -> fn r
        | UpdateResult.Failure f -> UpdateResult.Failure f

    let bindFailure<'T> (fn: unit -> UpdateResult<'T>) (result: UpdateResult<'T>) =
        match result with
        | UpdateResult.Success r -> UpdateResult.Success r
        | UpdateResult.Failure _ -> fn ()

    let combine<'T1, 'T2, 'U> (result2: UpdateResult<'T2>) (result1: UpdateResult<'T1>) =
        match result1, result2 with
        | UpdateResult.Success v1, UpdateResult.Success v2 -> UpdateResult.Success(v1, v2)
        | UpdateResult.Failure f, _ -> UpdateResult.Failure f
        | _, UpdateResult.Failure f -> UpdateResult.Failure f

    let chain<'T1, 'T2, 'U> (chainFn: 'T1 -> 'T2 -> 'U) (result2: UpdateResult<'T2>) (result1: UpdateResult<'T1>) =
        match result1, result2 with
        | UpdateResult.Success v1, UpdateResult.Success v2 -> chainFn v1 v2 |> UpdateResult.Success
        | UpdateResult.Failure f, _ -> UpdateResult.Failure f
        | _, UpdateResult.Failure f -> UpdateResult.Failure f

    let append<'T1, 'T2, 'T3, 'U> (result2: CreateResult<'T3>) (result1: CreateResult<'T1 * 'T2>) =
        match result1, result2 with
        | CreateResult.Success(v1, v2), CreateResult.Success v3 -> CreateResult.Success(v1, v2, v3)
        | CreateResult.Failure f, _ -> CreateResult.Failure f
        | _, CreateResult.Failure f -> CreateResult.Failure f

    /// <summary>
    ///     Merge to create results, the second one is based of the first ones result value.
    /// </summary>
    let merge<'T1, 'T2, 'U> (mergeFn: 'T1 -> 'T2 -> 'U) (result2: 'T1 -> CreateResult<'T2>) (result: CreateResult<'T1>) =
        // QUESTION would this make more sense to be `pipe`?
        match result with
        | CreateResult.Success v1 ->
            match result2 v1 with
            | CreateResult.Success v2 -> mergeFn v1 v2 |> CreateResult.Success
            | CreateResult.Failure f -> CreateResult.Failure f
        | CreateResult.Failure f -> CreateResult.Failure f

    /// <summary>
    ///     A wrapper around `merge`, with a merge function that takes both result values and combines them into a tuple.
    /// </summary>
    let pipe<'T1, 'T2, 'U> (result2: 'T1 -> CreateResult<'T2>) (result: CreateResult<'T1>) =
        // QUESTION would this make more sense to be `merge`?
        merge (fun v1 v2 -> v1, v2) <| result2 <| result

    let toResult<'T> (result: CreateResult<'T>) = result.ToResult()

    let toResult2<'T1, 'T2> (result1: CreateResult<'T1>) (result2: CreateResult<'T2>) =
        match result1.ToResult(), result2.ToResult() with
        | Ok r1, Ok r2 -> Ok(r1, r2)
        | Error e, _
        | _, Error e -> Error e

    let toResult3<'T1, 'T2, 'T3> (result1: CreateResult<'T1>) (result2: CreateResult<'T2>) (result3: CreateResult<'T3>) =
        match result1.ToResult(), result2.ToResult(), result3.ToResult() with
        | Ok r1, Ok r2, Ok r3 -> Ok(r1, r2, r3)
        | Error e, _, _
        | _, Error e, _
        | _, _, Error e -> Error e

    let fromResult<'T> (result: Result<'T, FailureResult>) =
        match result with
        | Ok v -> CreateResult.Success v
        | Error f -> CreateResult.Failure f

    let toOptionOrElse<'T> (fn: unit -> 'T option) (result: CreateResult<'T>) =
        match result with
        | CreateResult.Success v -> Some v
        | CreateResult.Failure _ -> fn ()

    let toOption<'T> (result: CreateResult<'T>) = toOptionOrElse (fun _ -> None) result

    let mapToOption<'T, 'U> (fn: 'T -> CreateResult<'U>) (value: 'T option) =
        value |> Option.bind (fun v -> fn v |> toOption)

    let bindToFetch<'T, 'U> (fetchFn: 'T -> FetchResult<'U>) (result: CreateResult<'T>) =
        match result with
        | CreateResult.Success v -> fetchFn v
        | CreateResult.Failure f -> FetchResult.Failure f

    let mapToFetch<'T, 'U> (fetchFn: 'T -> 'U) (result: CreateResult<'T>) =
        match result with
        | CreateResult.Success v -> fetchFn v |> FetchResult.Success
        | CreateResult.Failure f -> FetchResult.Failure f

    let bindToAction<'T, 'U> (actionFn: 'T -> ActionResult<'U>) (result: CreateResult<'T>) =
        match result with
        | CreateResult.Success v -> actionFn v
        | CreateResult.Failure f -> ActionResult.Failure f

    let mapToAction<'T, 'U> (actionFn: 'T -> ActionResult<'U>) (result: CreateResult<'T>) =
        match result with
        | CreateResult.Success v -> actionFn v
        | CreateResult.Failure f -> ActionResult.Failure f

    let bindToUpdate<'T, 'U> (createFn: 'T -> UpdateResult<'U>) (result: CreateResult<'T>) =
        match result with
        | CreateResult.Success v -> createFn v
        | CreateResult.Failure f -> UpdateResult.Failure f

    let mapToUpdate<'T, 'U> (createFn: 'T -> UpdateResult<'U>) (result: CreateResult<'T>) =
        match result with
        | CreateResult.Success v -> createFn v
        | CreateResult.Failure f -> UpdateResult.Failure f

    let toFetchResult<'T> (result: CreateResult<'T>) =
        match result with
        | CreateResult.Success v -> FetchResult.Success v
        | CreateResult.Failure f -> FetchResult.Failure f

    let fromFetchResult<'T> (result: FetchResult<'T>) =
        match result with
        | FetchResult.Success v -> CreateResult.Success v
        | FetchResult.Failure f -> CreateResult.Failure f

    let toActionResult<'T> (result: CreateResult<'T>) =
        match result with
        | CreateResult.Success v -> ActionResult.Success v
        | CreateResult.Failure f -> ActionResult.Failure f

    let fromActionResult<'T> (result: ActionResult<'T>) =
        match result with
        | ActionResult.Success v -> CreateResult.Success v
        | ActionResult.Failure f -> CreateResult.Failure f

    let toUpdateResult<'T> (result: CreateResult<'T>) =
        match result with
        | CreateResult.Success v -> UpdateResult.Success v
        | CreateResult.Failure f -> UpdateResult.Failure f

    let fromUpdateResult<'T> (result: UpdateResult<'T>) =
        match result with
        | UpdateResult.Success v -> CreateResult.Success v
        | UpdateResult.Failure f -> CreateResult.Failure f

    let iter<'T> (fn: 'T -> unit) (result: CreateResult<'T>) =
        match result with
        | CreateResult.Success v -> fn v
        | CreateResult.Failure _ -> ()

    let orElse<'T> (ifFailure: CreateResult<'T>) (result: CreateResult<'T>) =
        match result with
        | CreateResult.Success _ -> result
        | CreateResult.Failure _ -> ifFailure

    let orElseWith<'T> (fn: unit -> CreateResult<'T>) (result: CreateResult<'T>) =
        match result with
        | CreateResult.Success _ -> result
        | CreateResult.Failure _ -> fn ()

    let unzipResults (results: CreateResult<'T> seq) =
        // NOTE would resize arrays be better for this?
        results
        |> Seq.fold
            (fun (ok, errors) r ->
                match r with
                | CreateResult.Success v -> v :: ok, errors
                | CreateResult.Failure f -> ok, f :: errors)
            ([], [])
        |> fun (ok, errors) -> ok |> List.rev, errors |> List.rev
        
    let aggregateResults<'T> (errorDisplayMessage: string) (results: CreateResult<'T> seq) =
        results
        |> unzipResults
        |> fun (ok, err) ->
            ActionResult.Success ok,

            (match err.IsEmpty with
             | true -> None
             | false ->
                 FailureResult.Aggregate(err, errorDisplayMessage)
                 |> ActionResult.Failure
                 |> Some)