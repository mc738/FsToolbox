namespace FsToolbox.Yaml

open System.IO
open System.Reflection
open FsToolbox.Core.Results
open YamlDotNet.Core
open YamlDotNet.Core.Events
open YamlDotNet.RepresentationModel

[<AutoOpen>]
module Common =

    open System


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
            // PERFORMANCE To increase performance this call could be memorised.
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

    let getPropertyValue (name: string) (node: YamlNode) =
        match node.NodeType with
        | YamlNodeType.Mapping ->
            let n = node :?> YamlMappingNode

            match n.Children.TryGetValue(YamlScalarNode(name)) with
            | true, nv -> Some nv
            | false, _ -> None
        | YamlNodeType.Alias ->
            // NOTE could this be handled?
            None
        | _ -> None

    let tryGetScalarNode (node: YamlNode) =
        match node.NodeType with
        | YamlNodeType.Scalar -> node :?> YamlScalarNode |> Some
        | YamlNodeType.Alias ->
            // NOTE could this be handled?
            None
        | _ -> None

    let tryGetString (node: YamlNode) =
        tryGetScalarNode node |> Option.map (fun n -> n.Value)

    let tryGetByte (node: YamlNode) =
        tryGetScalarNode node
        |> Option.bind (fun n ->
            match Byte.TryParse n.Value with
            | true, v -> Some v
            | false, _ -> None)
        
    let tryGetBoolean (node: YamlNode) =
        tryGetScalarNode node
        |> Option.bind (fun n ->
            match n.Value.ToLower() with
            | "yes"
            | "ok"
            | "true"
            | "1" -> Some true
            | "no"
            | "false"
            | "0" -> Some false
            | _ -> None)
    
    let tryGetInt (node: YamlNode) =
        tryGetScalarNode node
        |> Option.bind (fun n ->
            match Int32.TryParse n.Value with
            | true, v -> Some v
            | false, _ -> None)

    let tryGetIntProperty (name: string) (node: YamlNode) =
        getPropertyValue name node |> Option.bind tryGetInt
