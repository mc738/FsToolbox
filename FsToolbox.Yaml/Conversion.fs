namespace FsToolbox.Yaml

open System.Text.Json
open FsToolbox.Core
open YamlDotNet.RepresentationModel

module Conversion =


    let jsonElementToYamlNode (element: JsonElement) =
        let rec handler (el: JsonElement) =
            match element.ValueKind with
            | JsonValueKind.Array ->
                element.EnumerateArray()
                |> List.ofSeq
                // NOTE Is List.choose correct here? Would it be better to fail on an error?
                |> List.choose handler
                |> fun els -> YamlSequenceNode(els |> Seq.ofList) :> YamlNode |> Some
            | JsonValueKind.Undefined -> None
            | JsonValueKind.Object -> failwith "todo"
            | JsonValueKind.String -> Json.tryGetString el |> Option.map (fun s -> YamlScalarNode s :> YamlNode)
            | JsonValueKind.Number -> failwith "todo"
            | JsonValueKind.True -> failwith "todo"
            | JsonValueKind.False -> failwith "todo"
            | JsonValueKind.Null -> failwith "todo"

        handler element



    ()
