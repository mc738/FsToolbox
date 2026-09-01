namespace FsToolbox.GLTF

open System.Numerics
open FsToolbox.GameDevelopment.Geometry.Types
open SharpGLTF.Memory
open SharpGLTF.Schema2

module GLTFLoader =

    [<RequireQualifiedAccess>]
    type Accessor =
        | Float of IAccessorArray<float32>
        | Vector2 of IAccessorArray<Vector2>
        | Vector3 of IAccessorArray<Vector3>
        | Vector4 of IAccessorArray<Vector4>

    [<RequireQualifiedAccess>]
    module private Operations =
        let createPrimitive (prim: SharpGLTF.Schema2.MeshPrimitive) =
            let (layoutItems, accessors) =
                [| for va in prim.VertexAccessors do
                       let f = va.Value.Format

                       let size, accessor =
                           match f.Dimensions with
                           | DimensionType.SCALAR -> 1, Accessor.Float(va.Value.AsArrayOf<float32>())
                           | DimensionType.VEC2 -> 2, Accessor.Vector2(va.Value.AsVector2Array())
                           | DimensionType.VEC3 -> 3, Accessor.Vector3(va.Value.AsVector3Array())
                           | DimensionType.VEC4 -> 4, Accessor.Vector4(va.Value.AsVector4Array())
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
                    | Accessor.Vector2 accessorArray -> accessorArray |> Seq.length
                    | Accessor.Vector3 accessorArray -> accessorArray |> Seq.length
                    | Accessor.Vector4 accessorArray -> accessorArray |> Seq.length

                [| for i in 0..length - 1 do
                       ({ Attributes =
                           [| for accessor in accessors do
                                  match accessor with
                                  | Accessor.Float accessorArray -> VertexAttribute.Float accessorArray[i]
                                  | Accessor.Vector2 accessorArray ->
                                      accessorArray[i] |> fun v -> VertexAttribute.Float2(v.X, v.Y)
                                  | Accessor.Vector3 accessorArray ->
                                      accessorArray[i] |> fun v -> VertexAttribute.Float3(v.X, v.Y, v.Z)
                                  | Accessor.Vector4 accessorArray ->
                                      accessorArray[i] |> fun v -> VertexAttribute.Float4(v.X, v.Y, v.Z, v.W) |] }) |]

            ({ Layout = ({ Items = layoutItems |> List.ofArray }: VertexLayout)
               Vertices = vertices
               Indices = prim.IndexAccessor.AsIndexArray() |> Seq.toArray }
            : Primitive)


    let loadModel (path: string) =
        let root = SharpGLTF.Schema2.ModelRoot.Load path

        ({ Meshes =
            [| for mesh in root.LogicalMeshes do
                   { Primitives = mesh.Primitives |> Seq.map Operations.createPrimitive |> Seq.toArray } |] }
        : Model3D)
