namespace FsToolbox.GLTF

open System.Numerics
open FsToolbox.GameDevelopment.Geometry.Types
open FsToolbox.GameDevelopment.Maths
open SharpGLTF.Memory
open SharpGLTF.Schema2

module GLTFLoader =

    [<RequireQualifiedAccess>]
    type Accessor =
        | Float of float32 array
        | Float2 of Float2 array
        | Float3 of Float3 array
        | Float4 of Float4 array
        | UInt of uint32 array
        | UInt2 of UInt2 array
        | UInt3 of UInt3 array
        | UInt4 of UInt4 array
        | Byte of byte array
        | Byte2 of byte array
        | Byte3 of byte array
        | Byte4 of byte array
        

    [<RequireQualifiedAccess>]
    module private Operations =
        let createPrimitive (prim: SharpGLTF.Schema2.MeshPrimitive) =
            let (layoutItems, accessors) =
                [| for va in prim.VertexAccessors do
                       let f = va.Value.Format

                       let size, accessor =
                           match f.Dimensions with
                           | DimensionType.SCALAR ->
                               (1,
                                match f.Encoding with
                                | EncodingType.FLOAT -> va.Value.AsArrayOf<float32>() |> Array.ofSeq |> Accessor.Float
                                | EncodingType.UNSIGNED_INT ->
                                    va.Value.AsArrayOf<uint32>() |> Array.ofSeq |> Accessor.UInt
                                | _ -> failwith $"Encoding {f.Encoding} not currently supported.")
                           | DimensionType.VEC2 ->
                               (2,
                                match f.Encoding with
                                | EncodingType.FLOAT ->
                                    va.Value.AsVector2Array()
                                    |> Array.ofSeq
                                    //|> Array.chunkBySize 2
                                    |> Array.map (fun v -> Float2(v.X, v.Y))
                                    |> Accessor.Float2
                                | EncodingType.UNSIGNED_INT ->
                                    va.Value.AsArrayOf<uint32>()
                                    |> Array.ofSeq
                                    |> Array.chunkBySize 2
                                    |> Array.map (fun v -> UInt2(v.[0], v.[1]))
                                    |> Accessor.UInt2
                                | _ -> failwith $"Encoding {f.Encoding} not currently supported.")
                           | DimensionType.VEC3 ->
                               (3,
                                match f.Encoding with
                                | EncodingType.FLOAT ->
                                    va.Value.AsVector3Array()
                                    |> Array.ofSeq
                                    //|> Array.chunkBySize 3
                                    |> Array.map (fun v -> Float3(v.X, v.Y, v.Z))
                                    |> Accessor.Float3
                                | EncodingType.UNSIGNED_INT ->
                                    va.Value.AsArrayOf<uint32>()
                                    |> Array.ofSeq
                                    |> Array.chunkBySize 3
                                    |> Array.map (fun v -> UInt3(v.[0], v.[1], v.[2]))
                                    |> Accessor.UInt3
                                | _ -> failwith $"Encoding {f.Encoding} not currently supported.")
                           | DimensionType.VEC4 ->
                               (4,
                                match f.Encoding with
                                | EncodingType.FLOAT ->
                                    va.Value.AsVector4Array()
                                    |> Array.ofSeq
                                    //|> Array.chunkBySize 4
                                    |> Array.map (fun v -> Float4(v.[0], v.[1], v.[2], v.[3]))
                                    |> Accessor.Float4
                                | EncodingType.UNSIGNED_INT ->
                                    va.Value.AsArrayOf<uint32>()
                                    |> Array.ofSeq
                                    |> Array.chunkBySize 3
                                    |> Array.map (fun v -> UInt4(v.[0], v.[1], v.[2], v.[3]))
                                    |> Accessor.UInt4
                                | _ -> failwith $"Encoding {f.Encoding} not currently supported.")
                           | DimensionType.MAT2 -> failwith "todo"
                           | DimensionType.MAT3 -> failwith "todo"
                           | DimensionType.MAT4 -> failwith "todo"
                           | DimensionType.CUSTOM -> failwith "todo"
                           | _ -> failwith "todo"

                       ({ Name = va.Key
                          ShaderName = ""
                          Size = size }
                       : VertexLayoutItem),
                       accessor

                   |]
                |> Array.unzip

            let vertices =
                let first = accessors |> Array.head
                let rest = accessors |> Array.tail

                let length =
                    match first with
                    | Accessor.Float accessorArray -> accessorArray |> Seq.length
                    | Accessor.Float2 accessorArray -> accessorArray |> Seq.length
                    | Accessor.Float3 accessorArray -> accessorArray |> Seq.length
                    | Accessor.Float4 accessorArray -> accessorArray |> Seq.length
                    | Accessor.UInt accessorArray -> accessorArray |> Seq.length
                    | Accessor.UInt2 accessorArray -> accessorArray |> Seq.length
                    | Accessor.UInt3 accessorArray -> accessorArray |> Seq.length
                    | Accessor.UInt4 accessorArray -> accessorArray |> Seq.length

                [| for i in 0 .. length - 1 do
                       ({ Attributes =
                           [| for accessor in accessors do
                                  match accessor with
                                  | Accessor.Float accessorArray -> VertexAttribute.Float accessorArray[i]
                                  | Accessor.Float2 accessorArray ->
                                      accessorArray[i] |> fun v -> VertexAttribute.Float2(Float2(v.X, v.Y))
                                  | Accessor.Float3 accessorArray ->
                                      accessorArray[i] |> fun v -> VertexAttribute.Float3(Float3(v.X, v.Y, v.Z))
                                  | Accessor.Float4 accessorArray ->
                                      accessorArray[i] |> fun v -> VertexAttribute.Float4(Float4(v.X, v.Y, v.Z, v.W))
                                  | Accessor.UInt accessorArray -> VertexAttribute.UInt accessorArray[i]
                                  | Accessor.UInt2 accessorArray ->
                                      accessorArray[i] |> fun v -> VertexAttribute.UInt2(UInt2(v.X, v.Y))
                                  | Accessor.UInt3 accessorArray ->
                                      accessorArray[i] |> fun v -> VertexAttribute.UInt3(UInt3(v.X, v.Y, v.Z))
                                  | Accessor.UInt4 accessorArray ->
                                      accessorArray[i] |> fun v -> VertexAttribute.UInt4(UInt4(v.X, v.Y, v.Z, v.W)) |] }) |]

            ({ Layout = ({ Items = layoutItems |> List.ofArray }: VertexLayout)
               Vertices = vertices
               Indices = prim.IndexAccessor.AsIndexArray() |> Seq.toArray }
            : Primitive)


    let loadRoot (path: string) = SharpGLTF.Schema2.ModelRoot.Load path

    let loadModel (path: string) =
        let root = SharpGLTF.Schema2.ModelRoot.Load path

        ({ Meshes =
            [| for mesh in root.LogicalMeshes do
                   { Primitives = mesh.Primitives |> Seq.map Operations.createPrimitive |> Seq.toArray } |] }
        : Model3D)
