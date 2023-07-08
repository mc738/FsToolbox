namespace FsToolbox.Yaml

open System.IO
open System.Reflection
open FsToolbox.Core.Results
open YamlDotNet.Core
open YamlDotNet.Core.Events
open YamlDotNet.RepresentationModel

[<AutoOpen>]
module Common =


    let parseDocument (yaml: string) =
        use reader = new StringReader(yaml)
        let parser = Parser(reader) :> IParser

        (*
        // Move the parser forwards twice - to get to document start token.
        let rec move () =
            match nameof (parser.Current) with
            | nameof (DocumentStart) -> Ok()
            | _ ->
                match parser.MoveNext() with
                | true -> move ()
                | false -> Error "documentStart token not found"
        
        move ()
        |> Result.bind (fun _ ->
            try
                typeof<YamlDocument>
                    .GetConstructor(
                        BindingFlags.NonPublic ||| BindingFlags.Instance,
                        null,
                        [| typeof<IParser> |],
                        null
                    )
                    .Invoke([| parser |])
                :?> YamlDocument
                |> Ok
            with exn ->
                Error $"Unhandled exception while parsing yaml document: {exn}"
        )
        *)
        parser.MoveNext() |> ignore
        parser.MoveNext() |> ignore

        try
            // NOTE This is a bit of a hack. The ctor that accepts a IParser is internal. This invokes it via reflection.
            typeof<YamlDocument>
                .GetConstructor(BindingFlags.NonPublic ||| BindingFlags.Instance, null, [| typeof<IParser> |], null)
                .Invoke([| parser |])
            :?> YamlDocument
            |> Ok
        with exn ->
            ({ Message = $"Unhandled exception while parsing yaml document: {exn.Message}"
               DisplayMessage = "Error while parsing yaml document"
               Exception = Some exn }
            : FailureResult)
            |> Error
