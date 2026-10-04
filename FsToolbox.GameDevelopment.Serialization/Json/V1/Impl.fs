namespace FsToolbox.GameDevelopment.Serialization.Json.V1

open System.Text.Json
open FsToolbox.GameDevelopment.Serialization.Core
open FsToolbox.GameDevelopment.Serialization.Json.V1.Geometry

type JsonTypeSerializer() =

    interface ITypeSerializer with
        member this.DeserializeModel(source) =
            match source.GetAsString() with
            | Error e -> DeserializationError.SourceError e |> Error
            | Ok s ->
                match Geometry.Deserializer.deserializeModel3D (JsonDocument.Parse(s).RootElement) with
                | Ok m -> Ok m
                | Error e ->
                    // TODO make this error better.
                    DeserializationError.SourceError(SerializationSourceError.InvalidOperation(e.ToString()))
                    |> Error

        member this.SerializeModel(target, model) =
            match target.GetStream() with
            | Error e -> SerializationError.TargetError e |> Error
            | Ok s ->
                use writer = new Utf8JsonWriter(s)
                model.WriteToJsonObject writer
                writer.Flush()
                Ok()

        member this.DeserializeAnimationClip(source) = failwith "todo"
        member this.DeserializeArmature(source) = failwith "todo"
        member this.SerializeAnimationClip(target, animation) = failwith "todo"
        member this.SerializeArmature(target, animation) = failwith "todo"
