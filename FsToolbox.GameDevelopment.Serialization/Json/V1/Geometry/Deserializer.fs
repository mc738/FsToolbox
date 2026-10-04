namespace FsToolbox.GameDevelopment.Serialization.Json.V1.Geometry

open System.Text.Json
open FsToolbox.Core
open FsToolbox.GameDevelopment.Geometry.Types
open FsToolbox.GameDevelopment.Maths
open FsToolbox.GameDevelopment.Serialization.Json.V1
open FsToolbox.GameDevelopment.Serialization.Json.V1.Maths

[<RequireQualifiedAccess>]
module Deserializer =

    let deserializeVertexAttribute (root: JsonElement) =
        match Json.tryGetStringProperty "type" root with
        | None -> JsonReadError.MissingProperty "type" |> Error
        | Some t ->
            match t with
            | "float" ->
                match Json.tryGetSingleProperty "value" root with
                | None -> JsonReadError.MissingProperty "value" |> Error
                | Some v -> Ok(VertexAttribute.Float v)
            | "float2" ->
                match Json.tryGetProperty "value" root with
                | None -> JsonReadError.MissingProperty "value" |> Error
                | Some v -> Deserializer.deserializeFloat2 v |> Result.map VertexAttribute.Float2
            | "float3" ->
                match Json.tryGetProperty "value" root with
                | None -> JsonReadError.MissingProperty "value" |> Error
                | Some v -> Deserializer.deserializeFloat3 v |> Result.map VertexAttribute.Float3
            | "float4" ->
                match Json.tryGetProperty "value" root with
                | None -> JsonReadError.MissingProperty "value" |> Error
                | Some v -> Deserializer.deserializeFloat4 v |> Result.map VertexAttribute.Float4
            | "int" ->
                match Json.tryGetIntProperty "value" root with
                | None -> JsonReadError.MissingProperty "value" |> Error
                | Some v -> Ok(VertexAttribute.Int v)
            | "int2" ->
                match Json.tryGetProperty "value" root with
                | None -> JsonReadError.MissingProperty "value" |> Error
                | Some v -> Deserializer.deserializeInt2 v |> Result.map VertexAttribute.Int2
            | "int3" ->
                match Json.tryGetProperty "value" root with
                | None -> JsonReadError.MissingProperty "value" |> Error
                | Some v -> Deserializer.deserializeInt3 v |> Result.map VertexAttribute.Int3
            | "int4" ->
                match Json.tryGetProperty "value" root with
                | None -> JsonReadError.MissingProperty "value" |> Error
                | Some v -> Deserializer.deserializeInt4 v |> Result.map VertexAttribute.Int4
            | t -> JsonReadError.InvalidType t |> Error

    let deserializeVertex (root: JsonElement) =
        match Json.tryGetArrayProperty "attributes" root with
        | None -> JsonReadError.MissingProperty "attributes" |> Error
        | Some attributes ->
            let results = ResizeArray<VertexAttribute>()
            let mutable error: JsonReadError option = None

            for attribute in attributes do
                if error.IsNone then
                    match deserializeVertexAttribute attribute with
                    | Ok a -> results.Add a
                    | Error e -> error <- Some e

            match error with
            | Some e -> Error e
            | None -> Ok { Attributes = results.ToArray() }

    let deserializeVertexLayoutItem (root: JsonElement) =
        match
            Json.tryGetStringProperty "name" root,
            Json.tryGetStringProperty "shaderName" root,
            Json.tryGetStringProperty "type" root
            |> Option.bind VertexAttributeEncodingType.Deserialize,
            Json.tryGetIntProperty "size" root
        with
        | None, _, _, _ -> JsonReadError.MissingProperty "name" |> Error
        | _, None, _, _ -> JsonReadError.MissingProperty "shaderName" |> Error
        | _, _, None, _ -> JsonReadError.MissingProperty "type" |> Error
        | _, _, _, None -> JsonReadError.MissingProperty "size" |> Error
        | Some name, Some shaderName, Some t, Some size ->
            ({ Name = name
               ShaderName = shaderName
               Type = t
               Size = size }
            : VertexLayoutItem)
            |> Ok

    let deserializeVertexLayout (root: JsonElement) =
        match Json.tryGetArrayProperty "items" root with
        | None -> JsonReadError.MissingProperty "items" |> Error
        | Some items ->
            let results = ResizeArray<VertexLayoutItem>()
            let mutable error: JsonReadError option = None

            for item in items do
                if error.IsNone then
                    match deserializeVertexLayoutItem item with
                    | Ok i -> results.Add i
                    | Error e -> error <- Some e

            match error with
            | Some e -> Error e
            | None -> Ok { Items = results |> List.ofSeq }

    let deserializePrimitive (root: JsonElement) =
        match
            Json.tryGetProperty "layout" root,
            Json.tryGetArrayProperty "vertices" root,
            Json.tryGetArrayProperty "indices" root
        with
        | None, _, _ -> JsonReadError.MissingProperty "layout" |> Error
        | _, None, _ -> JsonReadError.MissingProperty "vertices" |> Error
        | _, _, None -> JsonReadError.MissingProperty "indices" |> Error
        | Some layout, Some vertices, Some indices ->
            deserializeVertexLayout layout
            |> Result.bind (fun layout ->

                let verticesResult = ResizeArray<Vertex>()
                let indicesResult = ResizeArray<uint>()

                let mutable error: JsonReadError option = None

                for vertex in vertices do
                    if error.IsNone then
                        match deserializeVertex vertex with
                        | Ok v -> verticesResult.Add v
                        | Error e -> error <- Some e

                for index in indices do
                    if error.IsNone then
                        match Json.tryGetUInt index with
                        | Some v -> indicesResult.Add(v)
                        | None -> error <- Some(JsonReadError.InvalidType "indices")

                ({ Layout = layout
                   Vertices = verticesResult |> Array.ofSeq
                   Indices = indicesResult |> Array.ofSeq }
                : Primitive)
                |> Ok)

    let deserializeMesh (root: JsonElement) =
        match Json.tryGetArrayProperty "primitives" root with
        | None -> JsonReadError.MissingProperty "primitives" |> Error
        | Some primitives ->
            let results = ResizeArray<Primitive>()

            let result =
                primitives
                |> List.fold
                    (fun (r: Result<unit, JsonReadError>) (primitive: JsonElement) ->
                        match r with
                        | Result.Error e -> r
                        | Result.Ok _ ->
                            match deserializePrimitive primitive with
                            | Ok p ->
                                results.Add p
                                Ok()
                            | Error e -> Error e)
                    (Ok())

            match result with
            | Ok _ -> Ok { Primitives = results |> Array.ofSeq }
            | Error e -> Error e

    let deserializeModel3D (root: JsonElement) =
        match Json.tryGetArrayProperty "meshes" root with
        | None -> MissingProperty "meshes" |> Error
        | Some meshes ->
            let results = ResizeArray<Mesh>()

            let mutable error: JsonReadError option = None

            for mesh in meshes do
                if error.IsNone then
                    match deserializeMesh mesh with
                    | Ok m -> results.Add m
                    | Error e -> error <- Some e

            match error with
            | Some e -> Error e
            | None -> Ok { Meshes = results |> Array.ofSeq }
