namespace FsToolbox.Core

open System

[<RequireQualifiedAccess>]
module Json =

    open System.Text.Json

    let deserialize<'T> (json: string) =
        attempt (fun _ -> JsonSerializer.Deserialize<'T>(json))

    let serialize<'T> (value: 'T) =
        attempt (fun _ -> JsonSerializer.Serialize<'T>(value))

    let tryLoad<'T> path =
        attempt (fun _ -> FileIO.readText path |> Result.bind deserialize<'T>)

    let tryOpenDocument (json: string) =
        attempt (fun _ -> JsonDocument.Parse json)

    let tryLoadDocument path =
        FileIO.readText path |> Result.bind tryOpenDocument

    let tryGetProperty (name: string) (element: JsonElement) =
        match element.TryGetProperty name with
        | true, prop -> Some prop
        | false, _ -> None

    let tryGetBool (element: JsonElement) =
        match element.ValueKind with
        | JsonValueKind.True -> Some true
        | JsonValueKind.False -> Some false
        | _ -> None

    let tryGetByte (element: JsonElement) =
        match element.TryGetByte() with
        | true, b -> Some b
        | false, _ -> None

    let tryGetDecimal (element: JsonElement) =
        match element.TryGetDecimal() with
        | true, d -> Some d
        | false, _ -> None

    let tryGetDouble (element: JsonElement) =
        match element.TryGetDouble() with
        | true, d -> Some d
        | false, _ -> None

    let tryGetGuid (element: JsonElement) =
        match element.TryGetGuid() with
        | true, g -> Some g
        | false, _ -> None

    let tryGetInt16 (element: JsonElement) =
        match element.TryGetInt16() with
        | true, i -> Some i
        | false, _ -> None

    let tryGetInt (element: JsonElement) =
        match element.TryGetInt32() with
        | true, i -> Some i
        | false, _ -> None

    let tryGetInt64 (element: JsonElement) =
        match element.TryGetInt64() with
        | true, i -> Some i
        | false, _ -> None

    let tryGetSingle (element: JsonElement) =
        match element.TryGetSingle() with
        | true, s -> Some s
        | false, _ -> None

    let tryGetDateTime (element: JsonElement) =
        match element.TryGetDateTime() with
        | true, d -> Some d
        | false, _ -> None

    let tryGetSByte (element: JsonElement) =
        match element.TryGetSByte() with
        | true, s -> Some s
        | false, _ -> None

    let tryGetUInt16 (element: JsonElement) =
        match element.TryGetUInt16() with
        | true, u -> Some u
        | false, _ -> None

    let tryGetUInt (element: JsonElement) =
        match element.TryGetUInt32() with
        | true, u -> Some u
        | false, _ -> None

    let tryGetUInt64 (element: JsonElement) =
        match element.TryGetUInt64() with
        | true, u -> Some u
        | false, _ -> None

    let tryGetBytesFromBase64 (element: JsonElement) =
        match element.TryGetBytesFromBase64() with
        | true, b -> Some b
        | false, _ -> None

    let tryGetString (element: JsonElement) =
        match element.ValueKind with
        | JsonValueKind.String -> element.GetString() |> Some
        | _ -> None

    let getString (element: JsonElement) = element.GetString()

    /// Try and get a string array from a JsonElement.
    /// If the element is not a array, None is returned.
    let tryGetStringArray (element: JsonElement) =
        match element.ValueKind with
        | JsonValueKind.Array -> element.EnumerateArray() |> List.ofSeq |> List.choose (tryGetString) |> Some
        | _ -> None

    let tryGetStringProperty (name: string) (element: JsonElement) =
        match tryGetProperty name element with
        | Some p -> Some(p.GetString())
        | None -> None

    let tryGetBoolProperty (name: string) (element: JsonElement) =
        match tryGetProperty name element with
        | Some p -> Some(p.GetBoolean())
        | None -> None

    let tryGetByteProperty (name: string) (element: JsonElement) =
        match tryGetProperty name element with
        | Some p -> tryGetByte p
        | None -> None

    let tryGetDecimalProperty (name: string) (element: JsonElement) =
        match tryGetProperty name element with
        | Some p -> tryGetDecimal p
        | None -> None

    let tryGetDoubleProperty (name: string) (element: JsonElement) =
        match tryGetProperty name element with
        | Some p -> tryGetDouble p
        | None -> None

    let tryGetGuidProperty (name: string) (element: JsonElement) =
        match tryGetProperty name element with
        | Some p -> tryGetGuid p
        | None -> None

    let tryGetInt16Property (name: string) (element: JsonElement) =
        match tryGetProperty name element with
        | Some p -> tryGetInt16 p
        | None -> None

    let tryGetIntProperty (name: string) (element: JsonElement) =
        match tryGetProperty name element with
        | Some p -> tryGetInt p
        | None -> None

    let tryGetInt64Property (name: string) (element: JsonElement) =
        match tryGetProperty name element with
        | Some p -> tryGetInt64 p
        | None -> None

    let tryGetSingleProperty (name: string) (element: JsonElement) =
        match tryGetProperty name element with
        | Some p -> tryGetSingle p
        | None -> None

    let tryGetDateTimeProperty (name: string) (element: JsonElement) =
        match tryGetProperty name element with
        | Some p -> tryGetDateTime p
        | None -> None

    let tryGetSByteProperty (name: string) (element: JsonElement) =
        match tryGetProperty name element with
        | Some p -> tryGetSByte p
        | None -> None

    let tryGetUInt16Property (name: string) (element: JsonElement) =
        match tryGetProperty name element with
        | Some p -> tryGetUInt16 p
        | None -> None

    let tryGetUIntProperty (name: string) (element: JsonElement) =
        match tryGetProperty name element with
        | Some p -> tryGetUInt p
        | None -> None

    let tryGetUInt64Property (name: string) (element: JsonElement) =
        match tryGetProperty name element with
        | Some p -> tryGetUInt64 p
        | None -> None

    let tryGetBytesFromBase64Property (name: string) (element: JsonElement) =
        match tryGetProperty name element with
        | Some p -> tryGetBytesFromBase64 p
        | None -> None

    let tryGetElementsProperty (name: string) (element: JsonElement) =
        match tryGetProperty name element with
        | Some p -> Some(p.EnumerateObject() |> List.ofSeq)
        | None -> None

    let tryGetArrayProperty (name: string) (element: JsonElement) =
        match tryGetProperty name element with
        | Some p -> Some(p.EnumerateArray() |> List.ofSeq)
        | None -> None

    let tryGetIntArrayProperty (name: string) (element: JsonElement) =
        tryGetProperty name element
        |> Option.map (fun p -> p.EnumerateArray() |> List.ofSeq |> List.map (fun v -> v.GetInt32()))

    //let writeInt (writer : Utf8JsonWriter) (name : string) (value : byte) = writer.WriteNumber(name, byte)

    let propertiesToStringMap (properties: JsonProperty list) =
        properties |> List.map (fun el -> (el.Name, el.Value.GetString())) |> Map.ofList

    let propertiesToMap (properties: JsonProperty list) =
        properties |> List.map (fun p -> p.Name, p.Value) |> Map.ofList

    let writeString (writer: Utf8JsonWriter) (name: string) (value: string) = writer.WriteString(name, value)

    let writeObjectProperty (handler: Utf8JsonWriter -> unit) (name: string) (writer: Utf8JsonWriter) =
        writer.WriteStartObject(name)
        handler writer
        writer.WriteEndObject()

    [<Obsolete "Use `writeObjectProperty` instead. This is a alias to ensure backwards capability.">]
    let writePropertyObject (handler: Utf8JsonWriter -> unit) (name: string) (writer: Utf8JsonWriter) =
        writeObjectProperty handler name writer

    let writeObjectValue (handler: Utf8JsonWriter -> unit) (writer: Utf8JsonWriter) =
        writer.WriteStartObject()
        handler writer
        writer.WriteEndObject()

    [<Obsolete "Use `writeObjectValue` instead. This is a alias to ensure backwards capability.">]
    let writeObject (handler: Utf8JsonWriter -> unit) (writer: Utf8JsonWriter) = writeObjectValue handler writer

    let writeArrayValue (handler: Utf8JsonWriter -> unit) (writer: Utf8JsonWriter) =
        writer.WriteStartArray()
        handler writer
        writer.WriteEndArray()

    let writeArrayProperty (handler: Utf8JsonWriter -> unit) (name: string) (writer: Utf8JsonWriter) =
        writer.WriteStartArray(name)
        handler writer
        writer.WriteEndArray()

    [<Obsolete "Use `writeArrayProperty` instead. This is a alias to ensure backwards capability.">]
    let writeArray (handler: Utf8JsonWriter -> unit) (name: string) (writer: Utf8JsonWriter) =
        writeArrayProperty handler name writer
