namespace FsToolbox.OpenGL.Geometry


open System
open System.Runtime.InteropServices
open FsToolbox.GameDevelopment.Core
open FsToolbox.GameDevelopment.Geometry.Types
open FsToolbox.OpenGL.Types
open Silk.NET.OpenGL

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

[<RequireQualifiedAccess>]
type InstancedMeshPropertyType =
    | Float of Name: string * Index: int
    | Float2 of Name: string * Index: int
    | Float3 of Name: string * Index: int
    | Float4 of Name: string * Index: int

    member imp.Size =
        match imp with
        | Float(name, index) -> 1u
        | Float2(name, index) -> 2u
        | Float3(name, index) -> 3u
        | Float4(name, index) -> 4u

type InstancedMeshItem =
    { Transform: Transform
      Properties: InstancedMeshItemProperty array }

and InstancedMeshItemProperty = { Value: float32 array }


type InstancedMeshConfiguration =
    { Properties: InstancedMeshPropertyType array
      Items: InstancedMeshItem array }

/// <summary>
/// A special type of mesh that will be rendered multiple times with one draw call.
/// This uses DrawElement internally and an index buffer.
/// </summary>
type InstancedElementMesh(vertexLayout: VertexLayout) as this =

    let mutable instancesArray = Array.empty

    let mutable vertexBuffer: VertexBufferObject option = None

    let mutable indexBuffer: IndexBufferObject option = None

    let mutable vao: VertexArrayObject option = None

    let mutable previousVertexBuffer: VertexBufferObject option = None
    let mutable previousIndexBuffer: IndexBufferObject option = None

    let mutable indicesCount = 0u

    /// <summary>
    /// Buffer to hold other properties like transforms.
    /// </summary>
    let mutable transformBuffer = Unchecked.defaultof<VertexBufferObject>

    member this.Bind() =
        match vao, indexBuffer with
        | Some vao, Some ibo ->
            vao.Bind()
            ibo.Bind()
        | None, _
        | _, None -> failwith "Fix this"

    //member this.Draw(render) =
    //    render.DrawElementsInstanced(
    //        PrimitiveType.Triangles,
    //        DrawElementsType.UnsignedInt,
    //        indicesCount,
    //        instancesArray.Length |> uint
    //    )




    member _.Build(gl: GL, vertices: Vertex array, indices: uint array, cfg: InstancedMeshConfiguration) =
        instancesArray <- cfg.Items
        indicesCount <- indices.Length |> uint
        let verticesData = ResizeArray<float32>()
        let vertexSize = vertexLayout.Items |> List.sumBy (fun i -> i.Size |> uint)

        // Deal with any previous buffers.
        match previousVertexBuffer with
        | None -> ()
        | Some pvb ->
            // This could represent a regression or other issue.
            (pvb :> IDisposable).Dispose()

            match vertexBuffer with
            | None -> ()
            | Some ib -> previousVertexBuffer <- Some ib

        match previousIndexBuffer with
        | None -> ()
        | Some pib ->
            // This could represent a regression or other issue.
            (pib :> IDisposable).Dispose()

            match indexBuffer with
            | None -> ()
            | Some ib -> previousIndexBuffer <- Some ib

        // TODO previous property buffer.

        for vertex in vertices do
            for attribute in vertex.Attributes do
                verticesData.AddRange(attribute.GetValues())

        let transformsData =
            [| for instance in instancesArray do
                   let matrix = instance.Transform.ViewMatrix

                   // Row 1
                   matrix.M11
                   matrix.M12
                   matrix.M13
                   matrix.M14
                   // Row 1
                   matrix.M21
                   matrix.M22
                   matrix.M23
                   matrix.M24
                   // Row 3
                   matrix.M31
                   matrix.M32
                   matrix.M33
                   matrix.M34
                   // Row 4
                   matrix.M41
                   matrix.M42
                   matrix.M43
                   matrix.M44

                   yield!
                       seq {
                           // Next properties
                           for property in instance.Properties do
                               yield! property.Value
                       } |]

        transformBuffer <- new VertexBufferObject(gl, transformsData.AsSpan(), BufferTargetARB.ArrayBuffer)

        // Create the buffers.
        let verts = verticesData.ToArray()

        vertexBuffer <- Some(new VertexBufferObject(gl, verts.AsSpan(), BufferTargetARB.ArrayBuffer))
        indexBuffer <- Some(new IndexBufferObject(gl, indices.AsSpan(), BufferTargetARB.ElementArrayBuffer))
        vao <- Some(new VertexArrayObject(gl, vertexBuffer.Value, indexBuffer.Value))

        // Enable attribute
        let mutable offset = 0

        for i, item in vertexLayout.Items |> Seq.indexed do
            // Currently dynamic meshes only use floats.
            vao.Value.VertexAttributePointer(i |> uint, item.Size, VertexAttribPointerType.Float, vertexSize, offset)

            offset <- offset + item.Size

        // enable the properties for the transform buffer.
        // Based on https://learnopengl.com/Advanced-OpenGL/Instancing
        transformBuffer.Bind()

        let vertexOffset = 4u * 4u + (cfg.Properties |> Array.sumBy (fun p -> p.Size))

        vao.Value.VertexAttributePointer(3u, 4, VertexAttribPointerType.Float, vertexOffset, 0)
        vao.Value.VertexAttributePointer(4u, 4, VertexAttribPointerType.Float, vertexOffset, 4)
        vao.Value.VertexAttributePointer(5u, 4, VertexAttribPointerType.Float, vertexOffset, 8)
        vao.Value.VertexAttributePointer(6u, 4, VertexAttribPointerType.Float, vertexOffset, 12)

        gl.VertexAttribDivisor(3u, 1u)
        gl.VertexAttribDivisor(4u, 1u)
        gl.VertexAttribDivisor(5u, 1u)
        gl.VertexAttribDivisor(6u, 1u)

        // Properties.
        let mutable propertyOffset = 16
        let mutable propertyIndex = 7u

        for property in cfg.Properties do
            match property with
            | InstancedMeshPropertyType.Float(name, index) ->
                vao.Value.VertexAttributePointer(
                    propertyIndex,
                    1,
                    VertexAttribPointerType.Float,
                    vertexOffset,
                    propertyOffset
                )

                propertyOffset <- propertyOffset + 1
                gl.VertexAttribDivisor(propertyIndex, 1u)
                propertyIndex <- propertyIndex + 1u
            | InstancedMeshPropertyType.Float2(name, index) ->
                vao.Value.VertexAttributePointer(
                    propertyIndex,
                    2,
                    VertexAttribPointerType.Float,
                    vertexOffset,
                    propertyOffset
                )

                propertyOffset <- propertyOffset + 2
                gl.VertexAttribDivisor(propertyIndex, 1u)
                propertyIndex <- propertyIndex + 1u
            | InstancedMeshPropertyType.Float3(name, index) ->
                vao.Value.VertexAttributePointer(
                    propertyIndex,
                    3,
                    VertexAttribPointerType.Float,
                    vertexOffset,
                    propertyOffset
                )

                propertyOffset <- propertyOffset + 3
                gl.VertexAttribDivisor(propertyIndex, 1u)
                propertyIndex <- propertyIndex + 1u
            | InstancedMeshPropertyType.Float4(name, index) ->
                vao.Value.VertexAttributePointer(
                    propertyIndex,
                    4,
                    VertexAttribPointerType.Float,
                    vertexOffset,
                    propertyOffset
                )

                propertyOffset <- propertyOffset + 4
                gl.VertexAttribDivisor(propertyIndex, 1u)
                propertyIndex <- propertyIndex + 1u

        gl.BindVertexArray(0u)

/// <summary>
/// A mesh consisting of vertices and indices.
/// </summary>
type ElementMesh(vertexLayout: VertexLayout) as this =

    let mutable vertexBuffer: VertexBufferObject option = None

    let mutable indexBuffer: IndexBufferObject option = None

    let mutable vao: VertexArrayObject option = None

    let mutable previousVertexBuffer: VertexBufferObject option = None
    let mutable previousIndexBuffer: IndexBufferObject option = None

    member this.Bind() =
        match vao, indexBuffer with
        | Some vao, Some ibo ->
            vao.Bind()
            ibo.Bind()
        | None, _
        | _, None -> failwith "Fix this"

    member _.Build(gl: GL, vertices: Vertex array, indices: uint array) =
        let verticesData = ResizeArray<float32>()
        let vertexSize = vertexLayout.Items |> List.sumBy (fun i -> i.Size |> uint)

        // Deal with any previous buffers.
        match previousVertexBuffer with
        | None -> ()
        | Some pvb ->
            // This could represent a regression or other issue.
            (pvb :> IDisposable).Dispose()

            match vertexBuffer with
            | None -> ()
            | Some ib -> previousVertexBuffer <- Some ib

        match previousIndexBuffer with
        | None -> ()
        | Some pib ->
            // This could represent a regression or other issue.
            (pib :> IDisposable).Dispose()

            match indexBuffer with
            | None -> ()
            | Some ib -> previousIndexBuffer <- Some ib

        for vertex in vertices do
            for attribute in vertex.Attributes do
                verticesData.AddRange(attribute.GetValues())

        // Create the buffers.
        let verts = verticesData.ToArray()

        vertexBuffer <- Some(new VertexBufferObject(gl, verts.AsSpan(), BufferTargetARB.ArrayBuffer))
        indexBuffer <- Some(new IndexBufferObject(gl, indices.AsSpan(), BufferTargetARB.ElementArrayBuffer))
        vao <- Some(new VertexArrayObject(gl, vertexBuffer.Value, indexBuffer.Value))

        // Enable attribute
        let mutable offset = 0

        for i, item in vertexLayout.Items |> Seq.indexed do
            // Currently dynamic meshes only use floats.
            vao.Value.VertexAttributePointer(i |> uint, item.Size, VertexAttribPointerType.Float, vertexSize, offset)

            offset <- offset + item.Size

        gl.BindVertexArray(0u)
