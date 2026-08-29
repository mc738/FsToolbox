namespace FsToolbox.GameDevelopment.Cameras

open System
open System.Numerics

[<RequireQualifiedAccess>]
module FreeLookCameraOperations =
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
