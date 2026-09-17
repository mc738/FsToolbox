namespace FsToolbox.GameDevelopment.Geometry.Shapes

open FsToolbox.GameDevelopment.Geometry.Types
open FsToolbox.GameDevelopment.Maths

[<RequireQualifiedAccess>]
module Quad =

    type QuadVertexLayout = | Standard

    /// <summary>
    /// Vertices for XZ quad.
    /// </summary>
    /// <param name="xSize"></param>
    /// <param name="zSize"></param>
    let vertices (xSize: float32) (zSize: float32) =
        [|
           // Vert 1
           -0.5f * xSize
           0f
           -0.5f * zSize

           // Vert 2
           0.5f * xSize
           0f
           -0.5f * zSize

           // Vert 3
           0.5f * xSize
           0f
           0.5f * zSize

           // Vert 4
           -0.5f * xSize
           0f
           0.5f * zSize |]

    let indices = [| 0u; 1u; 2u; 2u; 3u; 0u |]

    let normals =
        [|
           // Vert 1
           0f
           1f
           0f

           // Vert 2
           0f
           1f
           0f

           // Vert 3
           0f
           1f
           0f

           // Vert 4
           0f
           1f
           0f |]

    let uvs =
        [| // Vert 1
           0f
           0f

           // Vert 2
           1f
           0f

           // Vert 3
           1f
           1f

           // Vert 4
           0f
           1f |]

    let groupedVertices (xSize: float32) (zSize: float32) =
        [| VertexAttribute.Float3(Float3(-0.5f * xSize, 0f, -0.5f * zSize))
           VertexAttribute.Float3(Float3(0.5f * xSize, 0f, -0.5f * zSize))
           VertexAttribute.Float3(Float3(0.5f * xSize, 0f, 0.5f * zSize))
           VertexAttribute.Float3(Float3(-0.5f * xSize, 0f, 0.5f * zSize)) |]

    let groupedUvs =
        [| VertexAttribute.Float2(Float2(0f, 0f))
           VertexAttribute.Float2(Float2(1f, 0f))
           VertexAttribute.Float2(Float2(1f, 1f))
           VertexAttribute.Float2(Float2(0f, 1f)) |]

    let groupedNormals =
        [| VertexAttribute.Float3(Float3(0f, 1f, 0f))
           VertexAttribute.Float3(Float3(0f, 1f, 0f))
           VertexAttribute.Float3(Float3(0f, 1f, 0f))
           VertexAttribute.Float3(Float3(0f, 1f, 0f)) |]

    let buildVerticesData (layout: QuadVertexLayout) (xSize: float32) (zSize: float32) =
        let verts = groupedVertices xSize zSize

        match layout with
        | Standard ->
            [| for i in 0 .. verts.Length - 1 do
                   ({ Attributes =
                       [|
                          // Vert
                          verts[i]
                          // Normals
                          groupedNormals[i]
                          // UVs
                          groupedUvs[i] |] }
                   : Vertex) |]
