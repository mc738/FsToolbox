namespace FsToolbox.GameDevelopment.Geometry

open System
open System.Runtime.InteropServices
open FsToolbox.GameDevelopment.Maths

module Types =

    [<RequireQualifiedAccess>]
    type VertexAttribute =
        | Float of float32
        | Float2 of Float2
        | Float3 of Float3
        | Float4 of Float4
        | UInt of uint32
        | UInt2 of UInt2
        | UInt3 of UInt3
        | UInt4 of UInt4

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
                   yield! BitConverter.GetBytes(f4.Z) |]
            | UInt i -> BitConverter.GetBytes(i)
            | UInt2 uInt2 ->
                [| yield! BitConverter.GetBytes(uInt2.X)
                   yield! BitConverter.GetBytes(uInt2.Y) |]
            | UInt3 uInt3 ->
                [| yield! BitConverter.GetBytes(uInt3.X)
                   yield! BitConverter.GetBytes(uInt3.Y)
                   yield! BitConverter.GetBytes(uInt3.Z) |]
            | UInt4 uInt4 ->
                [| yield! BitConverter.GetBytes(uInt4.X)
                   yield! BitConverter.GetBytes(uInt4.Y)
                   yield! BitConverter.GetBytes(uInt4.Z)
                   yield! BitConverter.GetBytes(uInt4.W) |]

    [<Struct; StructLayout(LayoutKind.Sequential)>]
    type Vertex = { Attributes: VertexAttribute array }

    type VertexLayout = { Items: VertexLayoutItem list }

    and VertexLayoutItem =
        { Name: string
          ShaderName: string
          Size: int }

    type Primitive =
        { Layout: VertexLayout
          Vertices: Vertex array
          Indices: uint array }

    type Mesh = { Primitives: Primitive array }

    type Model3D = { Meshes: Mesh array }
