namespace FsToolbox.Yaml

open System.Collections.Generic
open System.Text.Json
open FsToolbox.Core
open YamlDotNet.RepresentationModel

module Conversions =


    let jsonElementToYamlNode (element: JsonElement) =
        let rec handler (el: JsonElement) =
            match el.ValueKind with
            | JsonValueKind.Array ->
                el.EnumerateArray()
                |> List.ofSeq
                // NOTE Is List.choose correct here? Would it be better to fail on an error?
                |> List.choose handler
                |> fun els -> YamlSequenceNode(els |> Seq.ofList) :> YamlNode |> Some
            | JsonValueKind.Undefined -> None
            | JsonValueKind.Object ->
                el.EnumerateObject()
                |> List.ofSeq 
                |> List.choose (fun jp ->
                    handler jp.Value
                    |> Option.map (fun yn ->
                        KeyValuePair<YamlNode, YamlNode>(YamlScalarNode(jp.Name), yn)))
                |> YamlMappingNode
                |> fun n -> n :> YamlNode |> Some
            | JsonValueKind.String -> Json.tryGetString el |> Option.map (fun s -> YamlScalarNode s :> YamlNode)
            | JsonValueKind.Number ->
                // 
                
                failwith "todo"
            | JsonValueKind.True -> failwith "todo"
            | JsonValueKind.False -> failwith "todo"
            | JsonValueKind.Null -> failwith "todo"

        handler element



    ()
