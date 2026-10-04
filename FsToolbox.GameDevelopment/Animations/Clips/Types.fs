namespace FsToolbox.GameDevelopment.Animations.Clips

open System.Numerics
open System.Text.Json
open FsToolbox.Core

[<AutoOpen>]
module Types =

    type Keyframe<'T> = { Time: float32; Value: 'T }

    type AnimationChannel =
        { NodeId: int
          TranslationKeyframes: Keyframe<Vector3> array
          RotationKeyframes: Keyframe<Quaternion> array
          ScaleKeyframes: Keyframe<Vector3> array }

    type AnimationEvent =
        { Name: string
          Time: float32 }

        member this.WriteToJson(writer: Utf8JsonWriter) =
            writer
            |> Json.writeObjectValue (fun iw ->
                iw.WriteString("name", this.Name)
                iw.WriteNumber("time", this.Time))

    type AnimationClip =
        { Name: string
          Duration: float32
          IsLoop: bool
          Channels: Map<int, AnimationChannel>
          Events: AnimationEvent array }
