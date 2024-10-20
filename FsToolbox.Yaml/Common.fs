﻿namespace FsToolbox.Yaml

open System.Globalization
open System.IO
open System.Reflection
open FsToolbox.Core.Results
open YamlDotNet.Core
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
            FailureResult.Create($"Unhandled exception while parsing yaml document: {exn.Message}", "Error while parsing yaml document", ex = exn)
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

    let tryGetSingle (node: YamlNode) =
        tryGetScalarNode node
        |> Option.bind (fun n ->
            match Single.TryParse n.Value with
            | true, v -> Some v
            | false, _ -> None)

    let tryGetDouble (node: YamlNode) =
        tryGetScalarNode node
        |> Option.bind (fun n ->
            match Double.TryParse n.Value with
            | true, v -> Some v
            | false, _ -> None)

    let tryGetDecimal (node: YamlNode) =
        tryGetScalarNode node
        |> Option.bind (fun n ->
            match Decimal.TryParse n.Value with
            | true, v -> Some v
            | false, _ -> None)

    let tryGetInt16 (node: YamlNode) =
        tryGetScalarNode node
        |> Option.bind (fun n ->
            match Int16.TryParse n.Value with
            | true, v -> Some v
            | false, _ -> None)

    let tryGetInt (node: YamlNode) =
        tryGetScalarNode node
        |> Option.bind (fun n ->
            match Int32.TryParse n.Value with
            | true, v -> Some v
            | false, _ -> None)

    let tryGetInt64 (node: YamlNode) =
        tryGetScalarNode node
        |> Option.bind (fun n ->
            match Int64.TryParse n.Value with
            | true, v -> Some v
            | false, _ -> None)

    let tryGetDateTime (node: YamlNode) =
        tryGetScalarNode node
        |> Option.bind (fun n ->
            match DateTime.TryParse n.Value with
            | true, v -> Some v
            | false, _ -> None)

    let tryGetFormattedDateTime (format: string) (node: YamlNode) =
        tryGetScalarNode node
        |> Option.bind (fun n ->
            match
                DateTime.TryParseExact(n.Value, format, CultureInfo.InvariantCulture, DateTimeStyles.AssumeUniversal)
            with
            | true, v -> Some v
            | false, _ -> None)

    let tryGetGuid (node: YamlNode) =
        tryGetScalarNode node
        |> Option.bind (fun n ->
            match Guid.TryParse n.Value with
            | true, v -> Some v
            | false, _ -> None)

    let tryGetFormattedGuid (format: string) (node: YamlNode) =
        tryGetScalarNode node
        |> Option.bind (fun n ->
            match Guid.TryParseExact(n.Value, format) with
            | true, v -> Some v
            | false, _ -> None)


    let tryGetBoolProperty (name: string) (node: YamlNode) =
        getPropertyValue name node |> Option.bind tryGetBoolean

    let tryGetByteProperty (name: string) (node: YamlNode) =
        getPropertyValue name node |> Option.bind tryGetByte

    let tryGetSingleProperty (name: string) (node: YamlNode) =
        getPropertyValue name node |> Option.bind tryGetSingle

    let tryGetDoubleProperty (name: string) (node: YamlNode) =
        getPropertyValue name node |> Option.bind tryGetDouble

    let tryGetDecimalProperty (name: string) (node: YamlNode) =
        getPropertyValue name node |> Option.bind tryGetDecimal

    let tryGetInt16Property (name: string) (node: YamlNode) =
        getPropertyValue name node |> Option.bind tryGetInt16

    let tryGetIntProperty (name: string) (node: YamlNode) =
        getPropertyValue name node |> Option.bind tryGetInt

    let tryGetInt64Property (name: string) (node: YamlNode) =
        getPropertyValue name node |> Option.bind tryGetInt64

    let tryGetDateTimeProperty (name: string) (node: YamlNode) =
        getPropertyValue name node |> Option.bind tryGetDateTime

    let tryGetFormattedDateTimeProperty (name: string) (format) (node: YamlNode) =
        getPropertyValue name node |> Option.bind (tryGetFormattedDateTime format)
