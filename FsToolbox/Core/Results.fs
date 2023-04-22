namespace FsToolbox.Core.Results

open System

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
type FetchResult<'T> =
    | Success of 'T
    | Failure of FailureResult

    member fr.ToResult() =
        match fr with
        | FetchResult.Success r -> Ok r
        | FetchResult.Failure f -> Error f

[<RequireQualifiedAccess>]
type AddResult<'T> =
    | Success of 'T
    | Failure of FailureResult

    member ar.ToResult() =
        match ar with
        | AddResult.Success r -> Ok r
        | AddResult.Failure f -> Error f

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

    let mapOption<'T, 'U> (fn: 'T -> FetchResult<'U>) (value: 'T option) =
        value |> Option.bind (fun v -> fn v |> toOption)

    let bindAction<'T, 'U> (actionFn: 'T -> ActionResult<'U>) (result: FetchResult<'T>) =
        match result with
        | FetchResult.Success v -> actionFn v
        | FetchResult.Failure f -> ActionResult.Failure f

    let mapAction<'T, 'U> (actionFn: 'T -> 'U) (result: FetchResult<'T>) =
        match result with
        | FetchResult.Success v -> actionFn v |> ActionResult.Success
        | FetchResult.Failure f -> ActionResult.Failure f

    let mapAdd<'T, 'U> (addFn: 'T -> AddResult<'U>) (result: FetchResult<'T>) =
        match result with
        | FetchResult.Success v -> addFn v
        | FetchResult.Failure f -> AddResult.Failure f

    let toActionResult<'T> (result: FetchResult<'T>) =
        match result with
        | FetchResult.Success v -> ActionResult.Success v
        | FetchResult.Failure f -> ActionResult.Failure f

    let fromActionResult<'T> (result: ActionResult<'T>) =
        match result with
        | ActionResult.Success v -> FetchResult.Success v
        | ActionResult.Failure f -> FetchResult.Failure f

[<RequireQualifiedAccess>]
module ActionResult =

    let map<'T, 'U> (fn: 'T -> 'U) (result: ActionResult<'T>) =
        match result with
        | ActionResult.Success v -> fn v |> ActionResult.Success
        | ActionResult.Failure f -> ActionResult.Failure f

    let bind<'T, 'U> (fn: 'T -> ActionResult<'U>) (result: ActionResult<'T>) =
        match result with
        | ActionResult.Success v -> fn v
        | ActionResult.Failure f -> ActionResult.Failure f

    let fromResult<'T> (result: Result<'T, FailureResult>) =
        match result with
        | Ok v -> ActionResult.Success v
        | Error f -> ActionResult.Failure f

    let iter<'T> (fn: 'T -> unit) (result: ActionResult<'T>) =
        match result with
        | ActionResult.Success v -> fn v
        | ActionResult.Failure f -> ()

    let split<'T> (results: ActionResult<'T> seq) =
        results
        |> Seq.fold
            (fun (ok, err) curr ->
                match curr with
                | ActionResult.Success v -> v :: ok, err
                | ActionResult.Failure f -> ok, f :: err)
            ([], [])
        |> fun (ok, err) -> ok |> List.rev, err |> List.rev

    let aggregateMap<'T> (errorDisplayMessage: string) (results: ActionResult<'T> seq) =
        results
        |> split
        |> fun (ok, err) ->
            ActionResult.Success ok,

            (match err.IsEmpty with
             | true -> None
             | false ->
                 FailureResult.Aggregate(err, errorDisplayMessage)
                 |> ActionResult.Failure
                 |> Some)
