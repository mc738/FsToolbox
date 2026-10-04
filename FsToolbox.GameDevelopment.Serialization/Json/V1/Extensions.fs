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

        member this.FromJsonElement(element: JsonElement) =
            match
                Json.tryGetSingleProperty "x" element,
                Json.tryGetSingleProperty "y" element,
                Json.tryGetSingleProperty "z" element
            with
            | None, _, _ -> Result.Error(MissingProperty "x")
            | _, None, _ -> Result.Error(MissingProperty "y")
            | _, _, None -> Result.Error(MissingProperty "z")
            | Some x, Some y, Some z -> Result.Ok(Vector3(x, y, z))

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
            | Some x, Some y, Some z, Some w -> Result.Ok(Quaternion(x, y, z, w))


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

        member this.FromJsonElement(element: JsonElement) =
            let missingProperty =
                [ "m11"
                  "m12"
                  "m13"
                  "m14"
                  "m21"
                  "m22"
                  "m23"
                  "m24"
                  "m31"
                  "m32"
                  "m33"
                  "m34"
                  "m41"
                  "m42"
                  "m43"
                  "m44" ]
                |> List.fold
                    (fun (r: Result<unit, JsonReadError>) (propName: string) ->
                        match r with
                        | Result.Error e -> r
                        | Result.Ok _ ->
                            match Json.tryGetSingleProperty propName element with
                            | None -> Result.Error(MissingProperty propName)
                            | Some _ -> Ok())
                    (Result.Ok())

            match missingProperty with
            | Result.Error e -> Result.Error e
            | Result.Ok _ ->
                Matrix4x4(
                    Json.tryGetSingleProperty "m11" element |> _.Value,
                    Json.tryGetSingleProperty "m12" element |> _.Value,
                    Json.tryGetSingleProperty "m13" element |> _.Value,
                    Json.tryGetSingleProperty "m14" element |> _.Value,
                    Json.tryGetSingleProperty "m21" element |> _.Value,
                    Json.tryGetSingleProperty "m22" element |> _.Value,
                    Json.tryGetSingleProperty "m23" element |> _.Value,
                    Json.tryGetSingleProperty "m24" element |> _.Value,
                    Json.tryGetSingleProperty "m31" element |> _.Value,
                    Json.tryGetSingleProperty "m32" element |> _.Value,
                    Json.tryGetSingleProperty "m33" element |> _.Value,
                    Json.tryGetSingleProperty "m34" element |> _.Value,
                    Json.tryGetSingleProperty "m41" element |> _.Value,
                    Json.tryGetSingleProperty "m42" element |> _.Value,
                    Json.tryGetSingleProperty "m43" element |> _.Value,
                    Json.tryGetSingleProperty "m44" element |> _.Value
                )
                |> Ok

    ()


