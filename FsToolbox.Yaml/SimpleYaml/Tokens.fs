namespace FsToolbox.Yaml.SimpleYaml

module Tokens =
    
    
    type YamlToken =
        | Whitespace
        | PropertyKey of string
        | SetMarker

