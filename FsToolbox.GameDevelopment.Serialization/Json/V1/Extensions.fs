namespace FsToolbox.GameDevelopment.Serialization.Json.V1

open System.Numerics
open System.Text.Json
open FsToolbox.Core

[<AutoOpen>]
module Extensions =
    
    type Vector3 with

        member this.WriteJsonObject(writer: Utf8JsonWriter) =
            writer
            |> Json.writeObjectValue (fun iw ->
                iw.WriteNumber("x", this.X)
                iw.WriteNumber("y", this.Y)
                iw.WriteNumber("z", this.Z))

        member this.WriteJsonProperty(writer: Utf8JsonWriter, propertyName: string) =
            writer.WritePropertyName propertyName
            this.WriteJsonObject writer

    type Quaternion with

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

    type Matrix4x4 with

        member this.WriteJsonObject(writer: Utf8JsonWriter) =
            writer
            |> Json.writeObjectValue (fun iw ->
                iw.WriteNumber("m11", this.M11)
                iw.WriteNumber("m12", this.M12)
                iw.WriteNumber("m13", this.M13)
                iw.WriteNumber("m14", this.M14)
                iw.WriteNumber("m21", this.M21)
                iw.WriteNumber("m22", this.M22)
                iw.WriteNumber("m23", this.M23)
                iw.WriteNumber("m24", this.M24)
                iw.WriteNumber("m31", this.M31)
                iw.WriteNumber("m32", this.M32)
                iw.WriteNumber("m33", this.M33)
                iw.WriteNumber("m34", this.M34)
                iw.WriteNumber("m41", this.M41)
                iw.WriteNumber("m42", this.M42)
                iw.WriteNumber("m43", this.M43)
                iw.WriteNumber("m44", this.M44))

        member this.WriteJsonProperty(writer: Utf8JsonWriter, propertyName: string) =
            writer.WritePropertyName propertyName
            this.WriteJsonObject writer


