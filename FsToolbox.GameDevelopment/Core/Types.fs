namespace FsToolbox.GameDevelopment.Core

open System.Numerics
open System.Runtime.InteropServices

[<AutoOpen>]
module Types =

    [<Struct; StructLayout(LayoutKind.Sequential)>]
    type Transform =
        struct
            val mutable Position: Vector3
            val mutable Rotation: Quaternion
            val mutable Scale: Vector3
        end
        
        static member Default =
            let mutable t = Transform()
            t.Position <- Vector3.Zero
            t.Rotation <- Quaternion.Identity
            t.Scale <- Vector3.One
            t

        /// <summary>
        /// Used for calls like MemoryMarshal.CreateReadOnlySpan(&this.Position.X, Transform.ElementCount) mostly.
        /// Outside of that context, it has no major meaning.
        /// </summary>
        static member ElementCount = 10

        member this.ViewMatrix =
            // Order matters
            Matrix4x4.Identity
            * Matrix4x4.CreateFromQuaternion(this.Rotation)
            * Matrix4x4.CreateScale(this.Scale)
            * Matrix4x4.CreateTranslation(this.Position)

        /// <summary>
        /// Get the transform as a raw span.
        /// This can be used when allocations don't matter
        /// </summary>
        member this.GetRaw() =
            MemoryMarshal.CreateReadOnlySpan(&this.Position.X, Transform.ElementCount)

        //member this.GetAddress() =
        //&this.Position.X

        member this.AsBytes() = MemoryMarshal.AsBytes(this.GetRaw())

    type Rectangle =
        { X: float32
          Y: float32
          Height: float32
          Width: float32 }

    type Ray =
        { Origin: Vector3
          Direction: Vector3 }

        /// <summary>
        ///
        /// </summary>
        /// <param name="origin"></param>
        /// <param name="screen"></param>
        /// <param name="projection"></param>
        /// <param name="view"></param>
        static member CreateFromScreenRaycast
            (origin: Vector2, screen: Rectangle, projection: Matrix4x4, view: Matrix4x4)
            =
            let ndcX = (2f * origin.X / (screen.Width |> float32)) - 1f
            let ndcY = 1f - (2f * origin.Y / (screen.Height |> float32))

            let nearPoint = Vector4(ndcX, ndcY, -1f, 1f)
            let farPoint = Vector4(ndcX, ndcY, 1f, 1f)

            // Inverse
            let invProj = Matrix4x4.Invert(projection) |> snd
            let invView = Matrix4x4.Invert(view) |> snd

            // View space
            let nearView = Vector4.Transform(nearPoint, invProj)
            let farView = Vector4.Transform(farPoint, invProj)

            let nearView3 = Vector3(nearView.X, nearView.Y, nearView.Z) / nearView.W
            let farView3 = Vector3(farView.X, farView.Y, farView.Z) / farView.W

            let originWorld = Vector3.Transform(nearView3, invView)
            let farWorld = Vector3.Transform(farView3, invView)

            let dir = Vector3.Normalize(farWorld - originWorld)

            { Origin = originWorld
              Direction = dir }
