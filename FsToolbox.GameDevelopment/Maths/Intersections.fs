namespace FsToolbox.GameDevelopment.Maths

module Intersections =

    type RayHit =
        struct
            val mutable T: float32
            val mutable U: float32
            val mutable V: float32
            new(t, u, v) = { T = t; U = u; V = v }
        end

    type BVHNode =
        struct
            val mutable Bounds: Float3AABB
            val mutable Left: int
            val mutable Right: int
            val mutable Start: int
            val mutable Count: int

            new(b, l, r, s, c) =
                { Bounds = b
                  Left = l
                  Right = r
                  Start = s
                  Count = c }
        end

    let inline rayTriangle (orig: Float3) (dir: Float3) (tri: Polygon3) =
        let eps = 1e-6f

        let e1 = tri.B - tri.A
        let e2 = tri.C - tri.A

        let p = Float3.Cross(dir, e2)
        let det = Float3.Dot(e1, p)

        if det > -eps && det < eps then
            ValueNone
        else
            let invDet = 1f / det
            let tvec = orig - tri.A

            let u = Float3.Dot(tvec, p) * invDet

            if u < 0f || u > 1f then
                ValueNone
            else
                let q = Float3.Cross(tvec, e1)
                let v = Float3.Dot(dir, q) * invDet

                if v < 0f || u + v > 1f then
                    ValueNone
                else
                    let t = Float3.Dot(e2, q) * invDet
                    if t > eps then ValueSome(RayHit(t, u, v)) else ValueNone