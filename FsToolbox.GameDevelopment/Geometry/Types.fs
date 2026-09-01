namespace FsToolbox.GameDevelopment.Geometry

open System.Runtime.InteropServices

module Types =

    [<RequireQualifiedAccess>]
    type VertexAttribute =
        | Float of float32
        | Float2 of float32 * float32
        | Float3 of float32 * float32 * float32
        | Float4 of float32 * float32 * float32 * float32

        member va.GetValues() =
            match va with
            | Float f -> [| f |]
            | Float2(f, f1) -> [| f; f1 |]
            | Float3(f, f1, f2) -> [| f; f1; f2 |]
            | Float4(f, f1, f2, f3) -> [| f; f1; f2; f3 |]

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
