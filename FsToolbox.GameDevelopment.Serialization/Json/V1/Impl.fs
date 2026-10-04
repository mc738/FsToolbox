namespace FsToolbox.GameDevelopment.Serialization.Json.V1

open FsToolbox.GameDevelopment.Serialization.Core

type JsonSerializer() =

    interface ITypeSerializer with
        member this.DeserializeModel(source) =

            failwith "todo"

        member this.SerializeModel(target, model) = failwith "todo"
        member this.DeserializeAnimationClip(source) = failwith "todo"
        member this.DeserializeArmature(source) = failwith "todo"
        member this.SerializeAnimationClip(target, animation) = failwith "todo"
        member this.SerializeArmature(target, animation) = failwith "todo"
