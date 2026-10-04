namespace FsToolbox.GameDevelopment.Serialization.Json.V1.Geometry

open System.Text.Json
open FsToolbox.Core
open FsToolbox.GameDevelopment.Geometry.Types
open FsToolbox.GameDevelopment.Serialization.Json.V1.Maths

[<AutoOpen>]
module rec Extensions =

    type VertexAttribute with

        member this.WriteToJsonObject(writer: Utf8JsonWriter) =
            writer
            |> Json.writeObjectValue (fun iw ->
                let typeName =
                    match this with
                    | VertexAttribute.Float f -> "float"
                    | VertexAttribute.Float2 float2 -> "float2"
                    | VertexAttribute.Float3 float3 -> "float3"
                    | VertexAttribute.Float4 float4 -> "float4"
                    | VertexAttribute.Int i -> "int"
                    | VertexAttribute.Int2 int2 -> "int2"
                    | VertexAttribute.Int3 int3 -> "int3"
                    | VertexAttribute.Int4 int4 -> "int4"

                iw.WriteString("type", typeName)

                match this with
                | VertexAttribute.Float f -> iw.WriteNumber("value", f)
                | VertexAttribute.Float2 float2 -> float2.WriteJsonProperty(iw, "value")
                | VertexAttribute.Float3 float3 -> float3.WriteJsonProperty(iw, "value")
                | VertexAttribute.Float4 float4 -> float4.WriteJsonProperty(iw, "value")
                | VertexAttribute.Int i -> iw.WriteNumber("value", i)
                | VertexAttribute.Int2 int2 -> int2.WriteJsonProperty(iw, "value")
                | VertexAttribute.Int3 int3 -> int3.WriteJsonProperty(iw, "value")
                | VertexAttribute.Int4 int4 -> int4.WriteJsonProperty(iw, "value"))

        member this.WriteJsonProperty(writer: Utf8JsonWriter, propertyName: string) =
            writer.WritePropertyName propertyName
            this.WriteToJsonObject writer

    type Vertex with

        member this.WriteToJsonObject(writer: Utf8JsonWriter) =
            // Because of the struct and layout attributes this is needed to sort errors.
            let realThis = this

            writer
            |> Json.writeObjectValue (
                Json.writeArrayProperty
                    (fun aw ->
                        for att in realThis.Attributes do
                            att.WriteToJsonObject aw)
                    "attributes"
            )

        member this.WriteJsonProperty(writer: Utf8JsonWriter, propertyName: string) =
            writer.WritePropertyName propertyName
            this.WriteToJsonObject writer

    type VertexLayout with

        member this.WriteToJsonProperty(writer: Utf8JsonWriter, propertyName: string) =
            writer
            |> Json.writeObjectProperty
                (Json.writeArrayProperty
                    (fun aw ->
                        for item in this.Items do
                            item.WriteToJsonObject aw)
                    "items")
                propertyName


    type VertexLayoutItem with
        member this.WriteToJsonObject(writer: Utf8JsonWriter) =
            writer
            |> Json.writeObjectValue (fun iw ->
                writer.WriteString("name", this.Name)
                writer.WriteString("shaderName", this.ShaderName)
                writer.WriteString("type", this.Type.Serialize())
                writer.WriteNumber("size", this.Size))

    type Primitive with
        member this.WriteToJson(writer: Utf8JsonWriter) =
            writer
            |> Json.writeObjectValue (fun writer ->
                this.Layout.WriteToJsonProperty(writer, "layout")

                writer
                |> Json.writeArrayProperty
                    (fun w ->
                        for vertex in this.Vertices do
                            vertex.WriteToJsonObject w)
                    "vertices"

                writer
                |> Json.writeArrayProperty
                    (fun w ->
                        for index in this.Indices do
                            w.WriteNumberValue index)
                    "indices"

            )

    type Mesh with
        member this.WriteToJsonObject(writer: Utf8JsonWriter) =
            writer
            |> Json.writeObjectValue (fun w ->
                w
                |> Json.writeArrayProperty
                    (fun iw ->
                        for p in this.Primitives do
                            p.WriteToJson iw)
                    "primitives"

                ())

    type Model3D with

        member this.WriteToJsonObject(writer: Utf8JsonWriter) =
            writer
            |> Json.writeObjectValue (fun w ->
                w
                |> Json.writeArrayProperty
                    (fun iw ->
                        for p in this.Meshes do
                            p.WriteToJsonObject iw)
                    "meshes")


    ()
