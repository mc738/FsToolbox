namespace FsToolbox.OpenGL

open Silk.NET.OpenGL
open FSharp.NativeInterop

#nowarn "9"

type Render(gl: GL) as this =

        let mutable drawCallCount = 0

        let zeroPtr = NativePtr.ofNativeInt<nativeint> 0n |> NativePtr.toVoidPtr

        member _.DrawElements(primitiveType: PrimitiveType, elementType: DrawElementsType, count: uint) =
            gl.DrawElements(primitiveType, count, elementType, zeroPtr)
            this.IncrementDrawCalls()
            
        member _.DrawElementsInstanced(primitiveType: PrimitiveType, elementType: DrawElementsType, count: uint, instancedCount: uint) =
            gl.DrawElementsInstanced(primitiveType, count, elementType, zeroPtr, instancedCount)
            this.IncrementDrawCalls()

        member _.DrawArrays(primitiveType: PrimitiveType, first: int, count: uint) =
            gl.DrawArrays(primitiveType, first, count)
            this.IncrementDrawCalls()

        member _.BindVertexArray(pointer: uint) = gl.BindVertexArray(pointer)

        member _.UnbindVertexArray() = this.BindVertexArray(0u)

        member private _.IncrementDrawCalls() = drawCallCount <- drawCallCount + 1

