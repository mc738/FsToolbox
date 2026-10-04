namespace FsToolbox.GameDevelopment.Serialization.Json.V1.Maths

open System.Text.Json
open FsToolbox.Core
open FsToolbox.GameDevelopment.Maths
open FsToolbox.GameDevelopment.Serialization
open FsToolbox.GameDevelopment.Serialization.Json.V1.Core

[<RequireQualifiedAccess>]
module Deserializer =

    let deserializeFloat2 (root: JsonElement) =
        match Json.tryGetSingleProperty "x" root, Json.tryGetSingleProperty "y" root with
        | None, _ -> MissingProperty "x" |> Error
        | _, None -> MissingProperty "y" |> Error
        | Some x, Some y -> Ok(Float2(x, y))

    let deserializeFloat3 (root: JsonElement) =
        match
            Json.tryGetSingleProperty "x" root, Json.tryGetSingleProperty "y" root, Json.tryGetSingleProperty "z" root
        with
        | None, _, _ -> MissingProperty "x" |> Error
        | _, None, _ -> MissingProperty "y" |> Error
        | _, _, None -> MissingProperty "z" |> Error
        | Some x, Some y, Some z -> Ok(Float3(x, y, z))

    let deserializeFloat4 (root: JsonElement) =
        match
            Json.tryGetSingleProperty "x" root,
            Json.tryGetSingleProperty "y" root,
            Json.tryGetSingleProperty "z" root,
            Json.tryGetSingleProperty "w" root
        with
        | None, _, _, _ -> MissingProperty "x" |> Error
        | _, None, _, _ -> MissingProperty "y" |> Error
        | _, _, None, _ -> MissingProperty "z" |> Error
        | _, _, _, None -> MissingProperty "w" |> Error
        | Some x, Some y, Some z, Some w -> Ok(Float4(x, y, z, w))


    let deserializeInt2 (root: JsonElement) =
        match Json.tryGetIntProperty "x" root, Json.tryGetIntProperty "y" root with
        | None, _ -> MissingProperty "x" |> Error
        | _, None -> MissingProperty "y" |> Error
        | Some x, Some y -> Ok(Int2(x, y))


    let deserializeInt3 (root: JsonElement) =
        match Json.tryGetIntProperty "x" root, Json.tryGetIntProperty "y" root, Json.tryGetIntProperty "z" root with
        | None, _, _ -> MissingProperty "x" |> Error
        | _, None, _ -> MissingProperty "y" |> Error
        | _, _, None -> MissingProperty "z" |> Error
        | Some x, Some y, Some z -> Ok(Int3(x, y, z))

    let deserializeInt4 (root: JsonElement) =
        match
            Json.tryGetIntProperty "x" root,
            Json.tryGetIntProperty "y" root,
            Json.tryGetIntProperty "z" root,
            Json.tryGetIntProperty "w" root
        with
        | None, _, _, _ -> MissingProperty "x" |> Error
        | _, None, _, _ -> MissingProperty "y" |> Error
        | _, _, None, _ -> MissingProperty "z" |> Error
        | _, _, _, None -> MissingProperty "w" |> Error
        | Some x, Some y, Some z, Some w -> Ok(Int4(x, y, z, w))

    let deserializeUInt2 (root: JsonElement) =
        match Json.tryGetUIntProperty "x" root, Json.tryGetUIntProperty "y" root with
        | None, _ -> MissingProperty "x" |> Error
        | _, None -> MissingProperty "y" |> Error
        | Some x, Some y -> Ok(UInt2(x, y))

    let deserializeUInt3 (root: JsonElement) =
        match Json.tryGetUIntProperty "x" root, Json.tryGetUIntProperty "y" root, Json.tryGetUIntProperty "z" root with
        | None, _, _ -> MissingProperty "x" |> Error
        | _, None, _ -> MissingProperty "y" |> Error
        | _, _, None -> MissingProperty "z" |> Error
        | Some x, Some y, Some z -> Ok(UInt3(x, y, z))

    let deserializeUInt4 (root: JsonElement) =
        match
            Json.tryGetUIntProperty "x" root,
            Json.tryGetUIntProperty "y" root,
            Json.tryGetUIntProperty "z" root,
            Json.tryGetUIntProperty "w" root
        with
        | None, _, _, _ -> MissingProperty "x" |> Error
        | _, None, _, _ -> MissingProperty "y" |> Error
        | _, _, None, _ -> MissingProperty "z" |> Error
        | _, _, _, None -> MissingProperty "w" |> Error
        | Some x, Some y, Some z, Some w -> Ok(UInt4(x, y, z, w))

    ()
