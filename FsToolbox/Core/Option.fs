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
        
    let bindIf

    ()
