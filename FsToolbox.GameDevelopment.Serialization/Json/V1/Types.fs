namespace FsToolbox.GameDevelopment.Serialization.Json.V1

[<AutoOpen>]
module Types =

    type JsonReadError =
        | MissingProperty of string
        | InvalidType of string
