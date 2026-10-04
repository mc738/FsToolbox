namespace FsToolbox.GameDevelopment.Serialization.Json.V1.Core

[<AutoOpen>]
module Types =

    type JsonReadError =
        | MissingProperty of string
        | InvalidType of string
