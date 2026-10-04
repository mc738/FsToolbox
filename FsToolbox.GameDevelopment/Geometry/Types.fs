namespace FsToolbox.GameDevelopment.Geometry

open System
open System.IO
open System.Runtime.InteropServices
open System.Text
open System.Text.Json
open FsToolbox.GameDevelopment.Maths
open FsToolbox.Core
open FsToolbox.GameDevelopment.

module Types =

    [<RequireQualifiedAccess>]
    type VertexAttribute =
        | Float of float32
        | Float2 of Float2
        | Float3 of Float3
        | Float4 of Float4
        | Int of int32
        | Int2 of Int2
        | Int3 of Int3
        | Int4 of Int4

        member va.GetBytes() =
            match va with
            | Float f -> BitConverter.GetBytes(f)
            | Float2 f2 -> [| yield! BitConverter.GetBytes(f2.X); yield! BitConverter.GetBytes(f2.Y) |]
            | Float3 f3 ->
                [| yield! BitConverter.GetBytes(f3.X)
                   yield! BitConverter.GetBytes(f3.Y)
                   yield! BitConverter.GetBytes(f3.Z) |]
            | Float4 f4 ->
                [| yield! BitConverter.GetBytes(f4.X)
                   yield! BitConverter.GetBytes(f4.Y)
                   yield! BitConverter.GetBytes(f4.Z)
                   yield! BitConverter.GetBytes(f4.W) |]
            | Int i -> BitConverter.GetBytes(i)
            | Int2 int2 -> [| yield! BitConverter.GetBytes(int2.X); yield! BitConverter.GetBytes(int2.Y) |]
            | Int3 int3 ->
                [| yield! BitConverter.GetBytes(int3.X)
                   yield! BitConverter.GetBytes(int3.Y)
                   yield! BitConverter.GetBytes(int3.Z) |]
            | Int4 int4 ->
                [| yield! BitConverter.GetBytes(int4.X)
                   yield! BitConverter.GetBytes(int4.Y)
                   yield! BitConverter.GetBytes(int4.Z)
                   yield! BitConverter.GetBytes(int4.W) |]

    [<Struct; StructLayout(LayoutKind.Sequential)>]
    type Vertex =
        { Attributes: VertexAttribute array }

    type VertexLayout =
        { Items: VertexLayoutItem list }

    and VertexLayoutItem =
        { Name: string
          ShaderName: string
          Type: VertexAttributeEncodingType
          Size: int }

    and [<RequireQualifiedAccess>] VertexAttributeEncodingType =
        | Float
        | Int

        static member Deserialize(str: string) =
            match str.ToLower() with
            | "float32" -> Some(VertexAttributeEncodingType.Float)
            | "int32" -> Some(VertexAttributeEncodingType.Int)
            | _ -> None

        member this.Serialize() : string =
            match this with
            | Float -> "float32"
            | Int -> "int32"
            
    type Primitive =
        { Layout: VertexLayout
          Vertices: Vertex array
          Indices: uint array }

    type Mesh =
        { Primitives: Primitive array }

    type Model3D =
        { Meshes: Mesh array }
                
        static member FromJsonString(json: string) =
            let root = JsonDocument.Parse(json).RootElement
            Model3D.FromJson root

        member this.ToJsonStream(output: Stream) =
            use writer = new Utf8JsonWriter(output)

            writer
            |> Json.writeObjectValue (fun w ->
                w
                |> Json.writeArrayProperty
                    (fun iw ->
                        for p in this.Meshes do
                            p.WriteToJson iw)
                    "meshes")

            writer.Flush()

        member this.ToJsonBytes() =
            use ms = new MemoryStream()

            this.ToJsonStream(ms)

            ms.ToArray()

        member this.ToJsonString() =
            this.ToJsonBytes() |> Encoding.UTF8.GetString
