namespace FsToolbox.Yaml

open System.Collections.Generic
open System.Text.Json
open FsToolbox.Core
open YamlDotNet.RepresentationModel
open YamlDotNet.Serialization

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
                    |> Option.map (fun yn -> KeyValuePair<YamlNode, YamlNode>(YamlScalarNode(jp.Name), yn)))
                |> YamlMappingNode
                |> fun n -> n :> YamlNode |> Some
            | JsonValueKind.String -> Json.tryGetString el |> Option.map (fun s -> YamlScalarNode s :> YamlNode)
            | JsonValueKind.Number ->
                //

                failwith "todo"
            | JsonValueKind.True -> failwith "todo"
            | JsonValueKind.False -> failwith "todo"
            | JsonValueKind.Null -> failwith "todo"
            | _ -> System.ArgumentOutOfRangeException() |> raise

        handler element

    let writeYamlToJson (writer: Utf8JsonWriter) (yamlNode: YamlNode) =
        let rec handler (node: YamlNode) =
            match node.NodeType with
            | YamlNodeType.Alias -> failwith "todo"
            | YamlNodeType.Mapping ->
                let n = node :?> YamlMappingNode

                Json.writeObject
                    (fun w ->
                        n.Children
                        |> Seq.iter (fun kv ->
                            match kv.Key.NodeType with
                            | YamlNodeType.Alias -> failwith "todo"
                            | YamlNodeType.Mapping -> failwith "todo"
                            | YamlNodeType.Scalar ->
                                let kn = kv.Key :?> YamlScalarNode
                                w.WritePropertyName(kn.Value)
                            | YamlNodeType.Sequence -> failwith "todo"
                            | _ -> System.ArgumentOutOfRangeException() |> raise

                            handler kv.Value))
                    writer
            | YamlNodeType.Scalar ->
                // Not perfect but,
                // use regex to get


                failwith "todo"
            | YamlNodeType.Sequence -> failwith "todo"
            | _ -> System.ArgumentOutOfRangeException() |> raise

        handler yamlNode

    let yamlToJson (yaml: string) =
        let deserializer = DeserializerBuilder().Build()

        let yamlObject: obj = deserializer.Deserialize(yaml)

        let serializer = SerializerBuilder().JsonCompatible().Build()

        serializer.Serialize(yamlObject)

    ()
