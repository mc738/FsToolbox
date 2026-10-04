module FsToolbox.GameDevelopment.Serialization.Json.V1.Maths

open System.Text.Json
open FsToolbox.Core
open FsToolbox.GameDevelopment.Maths
open FsToolbox.GameDevelopment.Serialization.Json.V1.Core

[<AutoOpen>]
module Extensions =

    type Int2 with
        member this.WriteJsonObject(writer: Utf8JsonWriter) =
            writer
            |> Json.writeObjectValue (fun iw ->
                iw.WriteNumber("x", this.X)
                iw.WriteNumber("y", this.Y))

        member this.WriteJsonProperty(writer: Utf8JsonWriter, propertyName: string) =
            writer.WritePropertyName propertyName
            this.WriteJsonObject writer

        member this.FromJsonElement(element: JsonElement) =
            match Json.tryGetIntProperty "x" element, Json.tryGetIntProperty "y" element with
            | None, _ -> Result.Error(MissingProperty "x")
            | _, None -> Result.Error(MissingProperty "y")
            | Some x, Some y -> Result.Ok(Int2(x, y))

    type Int3 with
        member this.WriteJsonObject(writer: Utf8JsonWriter) =
            writer
            |> Json.writeObjectValue (fun iw ->
                iw.WriteNumber("x", this.X)
                iw.WriteNumber("y", this.Y)
                iw.WriteNumber("z", this.Z))

        member this.WriteJsonProperty(writer: Utf8JsonWriter, propertyName: string) =
            writer.WritePropertyName propertyName
            this.WriteJsonObject writer

        member this.FromJsonElement(element: JsonElement) =
            match
                Json.tryGetIntProperty "x" element,
                Json.tryGetIntProperty "y" element,
                Json.tryGetIntProperty "z" element
            with
            | None, _, _ -> Result.Error(MissingProperty "x")
            | _, None, _ -> Result.Error(MissingProperty "y")
            | _, _, None -> Result.Error(MissingProperty "z")
            | Some x, Some y, Some z -> Result.Ok(Int3(x, y, z))

    type Int4 with
        member this.WriteJsonObject(writer: Utf8JsonWriter) =
            writer
            |> Json.writeObjectValue (fun iw ->
                iw.WriteNumber("x", this.X)
                iw.WriteNumber("y", this.Y)
                iw.WriteNumber("z", this.Z)
                iw.WriteNumber("w", this.W))

        member this.WriteJsonProperty(writer: Utf8JsonWriter, propertyName: string) =
            writer.WritePropertyName propertyName
            this.WriteJsonObject writer

        member this.FromJsonElement(element: JsonElement) =
            match
                Json.tryGetIntProperty "x" element,
                Json.tryGetIntProperty "y" element,
                Json.tryGetIntProperty "z" element,
                Json.tryGetIntProperty "w" element
            with
            | None, _, _, _ -> Result.Error(MissingProperty "x")
            | _, None, _, _ -> Result.Error(MissingProperty "y")
            | _, _, None, _ -> Result.Error(MissingProperty "z")
            | _, _, _, None -> Result.Error(MissingProperty "w")
            | Some x, Some y, Some z, Some w -> Result.Ok(Int4(x, y, z, w))


    type UInt2 with
        member this.WriteJsonObject(writer: Utf8JsonWriter) =
            writer
            |> Json.writeObjectValue (fun iw ->
                iw.WriteNumber("x", this.X)
                iw.WriteNumber("y", this.Y))

        member this.WriteJsonProperty(writer: Utf8JsonWriter, propertyName: string) =
            writer.WritePropertyName propertyName
            this.WriteJsonObject writer

        member this.FromJsonElement(element: JsonElement) =
            match Json.tryGetUIntProperty "x" element, Json.tryGetUIntProperty "y" element with
            | None, _ -> Result.Error(MissingProperty "x")
            | _, None -> Result.Error(MissingProperty "y")
            | Some x, Some y -> Result.Ok(UInt2(x, y))

    type UInt3 with
        member this.WriteJsonObject(writer: Utf8JsonWriter) =
            writer
            |> Json.writeObjectValue (fun iw ->
                iw.WriteNumber("x", this.X)
                iw.WriteNumber("y", this.Y)
                iw.WriteNumber("z", this.Z))

        member this.WriteJsonProperty(writer: Utf8JsonWriter, propertyName: string) =
            writer.WritePropertyName propertyName
            this.WriteJsonObject writer

        member this.FromJsonElement(element: JsonElement) =
            match
                Json.tryGetUIntProperty "x" element,
                Json.tryGetUIntProperty "y" element,
                Json.tryGetUIntProperty "z" element
            with
            | None, _, _ -> Result.Error(MissingProperty "x")
            | _, None, _ -> Result.Error(MissingProperty "y")
            | _, _, None -> Result.Error(MissingProperty "z")
            | Some x, Some y, Some z -> Result.Ok(UInt3(x, y, z))

    type UInt4 with
        member this.WriteJsonObject(writer: Utf8JsonWriter) =
            writer
            |> Json.writeObjectValue (fun iw ->
                iw.WriteNumber("x", this.X)
                iw.WriteNumber("y", this.Y)
                iw.WriteNumber("z", this.Z)
                iw.WriteNumber("w", this.W))

        member this.WriteJsonProperty(writer: Utf8JsonWriter, propertyName: string) =
            writer.WritePropertyName propertyName
            this.WriteJsonObject writer

        member this.FromJsonElement(element: JsonElement) =
            match
                Json.tryGetUIntProperty "x" element,
                Json.tryGetUIntProperty "y" element,
                Json.tryGetUIntProperty "z" element,
                Json.tryGetUIntProperty "w" element
            with
            | None, _, _, _ -> Result.Error(MissingProperty "x")
            | _, None, _, _ -> Result.Error(MissingProperty "y")
            | _, _, None, _ -> Result.Error(MissingProperty "z")
            | _, _, _, None -> Result.Error(MissingProperty "w")
            | Some x, Some y, Some z, Some w -> Result.Ok(UInt4(x, y, z, w))


    type Float2 with
        member this.WriteJsonObject(writer: Utf8JsonWriter) =
            writer
            |> Json.writeObjectValue (fun iw ->
                iw.WriteNumber("x", this.X)
                iw.WriteNumber("y", this.Y))

        member this.WriteJsonProperty(writer: Utf8JsonWriter, propertyName: string) =
            writer.WritePropertyName propertyName
            this.WriteJsonObject writer

        member this.FromJsonElement(element: JsonElement) =
            match Json.tryGetSingleProperty "x" element, Json.tryGetSingleProperty "y" element with
            | None, _ -> Result.Error(MissingProperty "x")
            | _, None -> Result.Error(MissingProperty "y")
            | Some x, Some y -> Result.Ok(Float2(x, y))

    type Float3 with
        member this.WriteJsonObject(writer: Utf8JsonWriter) =
            writer
            |> Json.writeObjectValue (fun iw ->
                iw.WriteNumber("x", this.X)
                iw.WriteNumber("y", this.Y)
                iw.WriteNumber("z", this.Z))

        member this.WriteJsonProperty(writer: Utf8JsonWriter, propertyName: string) =
            writer.WritePropertyName propertyName
            this.WriteJsonObject writer

        member this.FromJsonElement(element: JsonElement) =
            match
                Json.tryGetSingleProperty "x" element,
                Json.tryGetSingleProperty "y" element,
                Json.tryGetSingleProperty "z" element
            with
            | None, _, _ -> Result.Error(MissingProperty "x")
            | _, None, _ -> Result.Error(MissingProperty "y")
            | _, _, None -> Result.Error(MissingProperty "z")
            | Some x, Some y, Some z -> Result.Ok(Float3(x, y, z))

    type Float4 with
        member this.WriteJsonObject(writer: Utf8JsonWriter) =
            writer
            |> Json.writeObjectValue (fun iw ->
                iw.WriteNumber("x", this.X)
                iw.WriteNumber("y", this.Y)
                iw.WriteNumber("z", this.Z)
                iw.WriteNumber("w", this.W))

        member this.WriteJsonProperty(writer: Utf8JsonWriter, propertyName: string) =
            writer.WritePropertyName propertyName
            this.WriteJsonObject writer

        member this.FromJsonElement(element: JsonElement) =
            match
                Json.tryGetSingleProperty "x" element,
                Json.tryGetSingleProperty "y" element,
                Json.tryGetSingleProperty "z" element,
                Json.tryGetSingleProperty "w" element
            with
            | None, _, _, _ -> Result.Error(MissingProperty "x")
            | _, None, _, _ -> Result.Error(MissingProperty "y")
            | _, _, None, _ -> Result.Error(MissingProperty "z")
            | _, _, _, None -> Result.Error(MissingProperty "w")
            | Some x, Some y, Some z, Some w -> Result.Ok(Float4(x, y, z, w))

