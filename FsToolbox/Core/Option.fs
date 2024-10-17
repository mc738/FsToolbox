namespace FsToolbox.Core

[<RequireQualifiedAccess>]
module Option =

    let ifTrue<'TIn, 'TResult> (value: 'TResult) (option: bool option) =
        match option with
        | Some true -> Some value
        | Some false
        | None -> None

    let mapIfTrue<'TIn, 'TResult> (whenTrueTrunk: unit -> 'TResult) (option: bool option) =
        match option with
        | Some true -> whenTrueTrunk () |> Some
        | Some false
        | None -> None
        
    let bindIfTrue<'TIn, 'TResult> (whenTrueTrunk: unit -> 'TResult option) (option: bool option) =
        match option with
        | Some true -> whenTrueTrunk ()
        | Some false
        | None -> None
        
    let ifFalse<'TIn, 'TResult> (value: 'TResult) (option: bool option) =
        match option with
        | Some false -> Some value
        | Some true 
        | None -> None

    let mapIfFalse<'TIn, 'TResult> (whenTrueTrunk: unit -> 'TResult) (option: bool option) =
        match option with
        | Some false -> whenTrueTrunk () |> Some
        | Some true 
        | None -> None
        
    let bindIfFalse<'TIn, 'TResult> (whenTrueTrunk: unit -> 'TResult option) (option: bool option) =
        match option with
        | Some false -> whenTrueTrunk ()
        | Some true 
        | None -> None

    ()
