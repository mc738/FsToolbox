namespace FsToolbox.GameDevelopment.Maths

open System
open System.Numerics
open System.Runtime.InteropServices

[<AutoOpen>]
module Types =

    type Int2 =
        val mutable X: int
        val mutable Y: int

        new(x, y) = { X = x; Y = y }

        static member inline (+)(a: Int2, b: Int2) = Int2(a.X + b.X, a.Y + b.Y)

        static member inline (-)(a: Int2, b: Int2) = Int2(a.X - b.X, a.Y - b.Y)

        static member inline (*)(a: Int2, b: Int2) = Int2(a.X * b.X, a.Y * b.Y)

        static member inline (/)(a: Int2, b: Int2) = Int2(a.X / b.X, a.Y / b.Y)

        static member inline Dot(a: Int2, b: Int2) = a.X * b.X + a.Y * b.Y

        member this.ToArray() = [| this.X; this.Y |]

        member this.Deconstruct() = (this.X, this.Y)

        //member this.ToFloat2() = Float2(this.X |> float32, this.Y |> float32)

        member this.Length() =
            MathF.Sqrt(float32 (this.X * this.X + this.Y * this.Y))

        member this.LengthSquared() = this.X * this.X + this.Y * this.Y

    [<Struct; StructLayout(LayoutKind.Sequential)>]
    type Float2 =
        val mutable X: float32
        val mutable Y: float32

        new(x, y) = { X = x; Y = y }


        // Maths ops
        static member inline (+)(a: Float2, b: Float2) = Float2(a.X + b.X, a.Y + b.Y)

        static member inline (-)(a: Float2, b: Float2) = Float2(a.X - b.X, a.Y - b.Y)

        static member inline (*)(a: Float2, b: Float2) = Float2(a.X * b.X, a.Y * b.Y)

        static member inline (/)(a: Float2, b: Float2) = Float2(a.X / b.X, a.Y / b.Y)

        static member inline Dot(a: Float2, b: Float2) = a.X * b.X + a.Y * b.Y

        member this.ToArray() = [| this.X; this.Y |]

        member this.Deconstruct() = (this.X, this.Y)

    [<Struct; StructLayout(LayoutKind.Sequential)>]
    type Float3 =
        val mutable X: float32
        val mutable Y: float32
        val mutable Z: float32

        new(x, y, z) = { X = x; Y = y; Z = z }

        new(float2: Float2, z: float32) = { X = float2.X; Y = float2.Y; Z = z }

        static member Zero = Float3(0f, 0f, 0f)

        static member One = Float3(1f, 1f, 1f)

        // Maths ops
        static member inline (+)(a: Float3, b: Float3) = Float3(a.X + b.X, a.Y + b.Y, a.Z + b.Z)

        static member inline (-)(a: Float3, b: Float3) = Float3(a.X - b.X, a.Y - b.Y, a.Z - b.Z)

        static member inline (*)(a: Float3, b: Float3) = Float3(a.X * b.X, a.Y * b.Y, a.Z * b.Z)

        static member inline (*)(a: Float3, v: float32) = Float3(a.X * v, a.Y * v, a.Z * v)

        static member inline (/)(a: Float3, b: Float3) = Float3(a.X / b.X, a.Y / b.Y, a.Z / b.Z)

        static member inline Dot(a: Float3, b: Float3) = a.X * b.X + a.Y * b.Y + a.Z * b.Z

        static member inline Cross(a: Float3, b: Float3) =
            Float3(a.Y * b.Z - a.Z * b.Y, a.Z * b.X - a.X * b.Z, a.X * b.Y - a.Y * b.X)

        member this.XY = Float2(this.X, this.Y)

        member this.XZ = Float2(this.X, this.Z)

        member this.YZ = Float2(this.Y, this.Z)

        member this.ToArray() = [| this.X; this.Y; this.Z |]

    type Polygon3 =
        val mutable A: Float3
        val mutable B: Float3
        val mutable C: Float3

        new(a, b, c) = { A = a; B = b; C = c }

    [<Struct; StructLayout(LayoutKind.Sequential)>]
    type Float3Ray =
        val mutable Origin: Float3
        val mutable Direction: Float3

        new(origin, direction) =
            { Origin = origin
              Direction = direction }


        /// <summary>
        /// Get the inverse direction of the ray.
        /// This is a method because it involves a calculation.
        /// </summary>
        member inline ray.InverseDirection() =
            Float3(1f / ray.Direction.X, 1f / ray.Direction.Y, 1f / ray.Direction.Z)

    [<Struct; StructLayout(LayoutKind.Sequential)>]
    type Float3AABB =
        val mutable Min: Float3
        val mutable Max: Float3
        new(min, max) = { Min = min; Max = max }

        member inline aabb.TestIntersect(ray: Float3Ray) =
            let mutable tmin = (aabb.Min.X - ray.Origin.X) / ray.Direction.X
            let mutable tmax = (aabb.Max.X - ray.Origin.X) / ray.Direction.X

            if tmin > tmax then
                let tmp = tmin
                tmin <- tmax
                tmax <- tmp

            let mutable tymin = (aabb.Min.Y - ray.Origin.Y) / ray.Direction.Y
            let mutable tymax = (aabb.Max.Y - ray.Origin.Y) / ray.Direction.Y

            if tymin > tymax then
                let tmp = tymin
                tymin <- tymax
                tymax <- tmp

            if (tmin > tymax) || (tymin > tmax) then
                ValueNone
            else

                tmin <- MathF.Max(tmin, tymin)

                tmax <- MathF.Min(tmax, tymax)

                let mutable tzmin = (aabb.Min.Z - ray.Origin.Z) / ray.Direction.Z
                let mutable tzmax = (aabb.Max.Z - ray.Origin.Z) / ray.Direction.Z

                if tzmin > tzmax then
                    let tmp = tzmin
                    tzmin <- tzmax
                    tzmax <- tmp

                if (tmin > tzmax) || (tzmin > tmax) then
                    ValueNone
                else
                    tmin <- MathF.Max(tmin, tzmin)

                    if tzmin > tmin then
                        tmin <- tzmin

                    tmax <- MathF.Min(tmax, tzmax)

                    if tzmax < tmax then
                        tmax <- tzmax

                    if tmin >= 0.0f then ValueSome tmin else ValueNone
