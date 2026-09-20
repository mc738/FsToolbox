namespace FsToolbox.OpenGL.Types

open System
open System.Runtime.InteropServices
open Silk.NET.OpenGL

type VertexBufferObject(gl: GL, data: Span<byte>, bufferType: BufferTargetARB) as this =

        let mutable handle = 0u

        do
            handle <- gl.GenBuffer()
            this.Bind()

            //use bufferPtr = fixed data
            //let voidPtr = bufferPtr |> NativePtr.toVoidPtr

            // Line 1
            //gl.BufferData(bufferType, (data.Length * sizeof<'TDataType>) |> unativeint, voidPtr, BufferUsageARB.StaticDraw)

            // Line 2
            gl.BufferData(
                bufferType,
                MemoryMarshal.CreateReadOnlySpan(&data.[0], data.Length),
                BufferUsageARB.StaticDraw
            )

        interface IDisposable with
            member this.Dispose() = gl.DeleteBuffer(handle)

        member _.Bind() = gl.BindBuffer(bufferType, handle)

        member _.Update(data: Span<float32>) =
            gl.BufferData(
                bufferType,
                MemoryMarshal.CreateReadOnlySpan(&data.[0], data.Length),
                // Because the data has been updated, we set it to dynamic draw.
                BufferUsageARB.DynamicDraw
            )
            
type IndexBufferObject(gl: GL, data: Span<uint32>, bufferType: BufferTargetARB) as this =

        let mutable handle = 0u

        do
            handle <- gl.GenBuffer()
            this.Bind()

            //use bufferPtr = fixed data
            //let voidPtr = bufferPtr |> NativePtr.toVoidPtr

            // Line 1
            //gl.BufferData(bufferType, (data.Length * sizeof<'TDataType>) |> unativeint, voidPtr, BufferUsageARB.StaticDraw)

            // Line 2
            gl.BufferData(
                bufferType,
                MemoryMarshal.CreateReadOnlySpan(&data.[0], data.Length),
                BufferUsageARB.StaticDraw
            )

        interface IDisposable with
            member this.Dispose() = gl.DeleteBuffer(handle)

        member _.Bind() = gl.BindBuffer(bufferType, handle)

        member _.Update(data: Span<uint>) =
            gl.BufferData(
                bufferType,
                MemoryMarshal.CreateReadOnlySpan(&data.[0], data.Length),
                // Because the data has been updated, we set it to dynamic draw.
                BufferUsageARB.DynamicDraw
            )

type VertexArrayObject
        (gl: GL, vertexBuffer: VertexBufferObject, indexBuffer: IndexBufferObject) as this =
        let mutable handle = 0u

        do
            handle <- gl.GenVertexArray()
            this.Bind()
            vertexBuffer.Bind()
            indexBuffer.Bind()

        interface IDisposable with
            member this.Dispose() = gl.DeleteVertexArray(handle)

        member _.VertexAttributePointer
            (index: uint, count: int, pointerType: VertexAttribPointerType, vertexSize: uint, offset: int)
            =
            let size, isInt =
                match pointerType with
                | VertexAttribPointerType.Byte -> sizeof<byte>, true
                | VertexAttribPointerType.UnsignedByte -> sizeof<sbyte>, true
                | VertexAttribPointerType.Short -> sizeof<int16>, true
                | VertexAttribPointerType.UnsignedShort -> sizeof<uint16>, true
                | VertexAttribPointerType.Int -> sizeof<int>, true
                | VertexAttribPointerType.UnsignedInt -> sizeof<uint>, true
                | VertexAttribPointerType.Float -> sizeof<float32>, false
                | VertexAttribPointerType.Double -> sizeof<float>, false
                | VertexAttribPointerType.HalfFloat -> sizeof<float32> / 2, false
                | VertexAttribPointerType.Fixed -> failwith "todo"
                | VertexAttribPointerType.Int64Arb -> failwith "todo"
                | VertexAttribPointerType.UnsignedInt64Arb -> failwith "todo"
                | VertexAttribPointerType.UnsignedInt2101010Rev -> failwith "todo"
                | VertexAttribPointerType.UnsignedInt10f11f11fRev -> failwith "todo"
                | VertexAttribPointerType.Int2101010Rev -> failwith "todo"

            //let strideBytes = nativeint vertexSize * nativeint sizeof<float32>
            
            if isInt then
                gl.VertexAttribIPointer(
                    index,
                    count,
                    VertexAttribIType.Int,
                    vertexSize * (size |> uint),
                    nativeint (offset * size)
                )

                gl.EnableVertexAttribArray(index)
            else
                gl.VertexAttribPointer(
                    index,
                    count,
                    pointerType,
                    false,
                    vertexSize * (size |> uint),
                    nativeint (offset * size)
                )

                gl.EnableVertexAttribArray(index)

        member _.Bind() = gl.BindVertexArray(handle)

