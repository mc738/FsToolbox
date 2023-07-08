namespace FsToolbox.Yaml

open System.Text.Json
open YamlDotNet.RepresentationModel

module Conversion =
    
    
    let jsonElementToYamlNode (element: JsonElement) =
        match element.ValueKind with
        | JsonValueKind.Array -> ()
        | JsonValueKind.Undefined -> failwith "todo"
        | JsonValueKind.Object -> failwith "todo"
        | JsonValueKind.String ->
            
            YamlScalarNode()
            
            failwith "todo"
        | JsonValueKind.Number -> failwith "todo"
        | JsonValueKind.True -> failwith "todo"
        | JsonValueKind.False -> failwith "todo"
        | JsonValueKind.Null -> failwith "todo"
        
        
    
    
    ()

