namespace FsToolbox.GameDevelopment.Cameras

open System
open System.Numerics
open FsToolbox.GameDevelopment.Core

[<AutoOpen>]
module Core =
    
    type Camera() as this =

        let mutable near = 0.1f
        let mutable far = 1000f
        
        let mutable fov = MathF.PI / 4f
        
        let mutable position = Vector3.Zero
        let mutable front = Vector3.UnitZ
        let mutable up = Vector3.UnitY
        let mutable yaw = 0f // -90f
        let mutable pitch = 0f
        let mutable zoom = 0f //45f
        let mutable aspectRatio = 600f / 800f
        let mutable target = Vector3.Zero
        let mutable right = Vector3.Zero
        let mutable forward = Vector3.UnitZ

        let mutable viewMatrix = Matrix4x4.Identity

        let mutable projectionMatrix = Matrix4x4.Identity

        //let mutable frustum = Frustum.Default

        let mutable isDirty = false

        member _.Position = position
        member _.Front = front
        member _.Up = up
        member _.Yaw = yaw
        member _.Pitch = pitch
        member _.Zoom = zoom
        member _.AspectRatio = aspectRatio
        member _.Target = target

        member _.Right = right

        member _.Forward = forward

        member _.ViewMatrix = viewMatrix

        member _.ProjectionMatrix = projectionMatrix

        //member _.Frustum = frustum

        member _.Near = near
        
        member _.Far = far
        
        member _.FieldOfView = fov
        
        member _.SetPosition(p) = position <- p

        member _.ModifyPosition(x: float32, y: float32, z: float32) =
            position <- position + right * x + up * y + forward * z

        member _.ModifyPosition(vec3: Vector3) = position <- position + vec3

        (*
        member _.GetFrustum() =
            
            let zNear = near
            let zFar = far
            let fovY = fov
            
            let halfVSide = zFar * MathF.Tan(fovY * 0.5f)
            let halfHSide = halfVSide * aspectRatio
            let frontMultFar = zFar * this.Forward

            { Left = FrustumPlane.FromPointAndNormal(this.Position, Vector3.Cross(this.Up, frontMultFar + this.Right * halfHSide), false)
              Right = FrustumPlane.FromPointAndNormal(this.Position, Vector3.Cross(frontMultFar - this.Right * halfHSide, this.Up), false)
              Bottom = FrustumPlane.FromPointAndNormal(this.Position, Vector3.Cross(frontMultFar + this.Up * halfVSide, this.Right), false)
              Top = FrustumPlane.FromPointAndNormal(this.Position, Vector3.Cross(this.Right, frontMultFar - this.Up * halfVSide), false)
              Near = FrustumPlane.FromPointAndNormal(this.Position + zNear * this.Forward,  this.Forward, false)
              Far = FrustumPlane.FromPointAndNormal(this.Position + frontMultFar, -this.Forward, false) }
         *)

        /// <summary>
        /// Raycast for a point on the cameras view straight ahead into the world.
        /// </summary>
        /// <param name="origin"></param>
        /// <param name="screen"></param>
        member camera.Raycast(origin: Vector2, screen: Rectangle) =
            Ray.CreateFromScreenRaycast(origin, screen, camera.ProjectionMatrix, camera.ViewMatrix)

        //member this.ModifyTarget(x: float32, y: float32, z: float32) =
        //    target <- target + right * x + up * y + forward * z

        (*
        member _.ModifyDirection(xOffset: float32, yOffset: float32) =
            yaw <- yaw + xOffset
            pitch <- Math.Clamp(pitch - yOffset, -89f, 89f)

            // Separate variable for clarity for now.
            // Though see if this is needed.
            let mutable newDirection = Vector3.Zero
            newDirection.X <- MathF.Cos(Math.degreesToRadians yaw) * Math.degreesToRadians (pitch)
            newDirection.Y <- MathF.Sin(Math.degreesToRadians (pitch))
            newDirection.Z <- MathF.Sin(Math.degreesToRadians yaw) * Math.degreesToRadians (pitch)

            //direction <- newDirection

            front <- Vector3.Normalize(newDirection)
        *)

        member _.SetFront(f) =
            front <- f
            isDirty <- true

        member _.SetUp(u) =
            up <- u
            isDirty <- true

        member _.SetYaw(y) =
            yaw <- y
            isDirty <- true

        member _.ModifyYaw(y) =
            yaw <- yaw + y
            isDirty <- true

        member _.SetPitch(p) =
            pitch <- p
            isDirty <- true

        member _.ModifyPitch(p) =
            pitch <- pitch + p
            isDirty <- true

        member _.SetZoom(z) =
            zoom <- z
            isDirty <- true

        member _.ModifyZoom(z) =
            zoom <- zoom + z
            isDirty <- true

        member _.SetAspectRatio(ar) =
            aspectRatio <- ar
            isDirty <- true

        /// <summary>
        /// Commit any changes to the camera, this will update the basis and generate new view and projection matrices.
        /// This is intended to be called priority to rending. Normally you should not have to call this manually.
        /// </summary>
        [<CompilerMessage("This method is designed for internal consumption.", 10005)>]
        member _.Commit() =
            //if isDirty then
            this.UpdateBasis()

            projectionMatrix <- Matrix4x4.CreatePerspectiveFieldOfView(fov, aspectRatio, near, far)

            viewMatrix <-
                Matrix4x4.CreateLookAt(
                    position, // Pos
                    position + forward, // target
                    up
                )

            //frustum <- this.GetFrustum()

        [<CompilerMessage("This method is designed for internal consumption.", 10005)>]
        member camera.UpdateBasis() =
            forward <-
                Vector3.Normalize(
                    Vector3(
                        // Swap for sin yaw--------|
                        //MathF.Cos(camera.Pitch) * MathF.Cos(camera.Yaw),
                        MathF.Cos(camera.Pitch) * MathF.Sin(camera.Yaw),
                        MathF.Sin(camera.Pitch),
                        // Swap for cos yaw--------|
                        //MathF.Cos(camera.Pitch) * MathF.Sin(camera.Yaw)
                        MathF.Cos(camera.Pitch) * MathF.Cos(camera.Yaw)
                    )
                )
                
                
            // Flip for -z forwards
            forward <- -forward

            right <- Vector3.Normalize(Vector3.Cross(camera.Forward, Vector3(0f, 1f, 0f)))

            up <- Vector3.Cross(camera.Right, camera.Forward)

        member _.LookAt(newPosition: Vector3) =

            position <- newPosition + Vector3(0f, 0f, -5f)

    module FreeLookCamera =

        let move (camera: Camera) (moveVec: Vector3) (speed: float32) (dt: float32) =
            camera.ModifyPosition(camera.Forward * moveVec.Z * speed * dt) // <- camera.Position +
            camera.ModifyPosition(camera.Right * moveVec.X * speed * dt)
            camera.ModifyPosition(camera.Up * moveVec.Y * speed * dt)

        let updatePitchAndYaw (camera: Camera) (delta: Vector2) (sensitivity: float32) =
            // This is flipped for OpenGL, DX, doesn't flip.
            camera.ModifyYaw(-1f * delta.X * sensitivity)
            
            // *-1 for flip (DX)
            camera.ModifyPitch(delta.Y * sensitivity)
            camera.SetPitch(Math.Clamp(camera.Pitch, -1.55f, 1.55f))
        
        ()

