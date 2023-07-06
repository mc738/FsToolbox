namespace FsToolbox.Yaml

open YamlDotNet.RepresentationModel

module Say =


    let getValue (i: YamlScalarNode) =
        ()

    let test (i: YamlNode) =
        
        
        match i.NodeType with
        | YamlNodeType.Scalar ->
            
            
            ""

    let hello name =
        printfn "Hello %s" name