namespace FsToolbox.GameDevelopment.Serialization.Json.V1.Animations

open System.Text.Json
open FsToolbox.GameDevelopment.Animations.Armatures
open FsToolbox.Core
open FsToolbox.GameDevelopment.Animations.Clips
open FsToolbox.GameDevelopment.Serialization.Json.V1

[<AutoOpen>]
module Extensions =

    type ArmatureBone with
        member this.WriteToJson(writer: Utf8JsonWriter) =
            writer
            |> Json.writeObjectValue (fun iw ->
                iw.WriteNumber("nodeId", this.NodeId)
                iw.WriteString("name", this.Name)

                iw
                |> Json.writeArrayProperty
                    (fun aw ->
                        for c in this.Children do
                            c.WriteToJson aw)
                    "children")

    type ArmatureJoint with
        member this.WriteToJson(writer: Utf8JsonWriter) =
            writer
            |> Json.writeObjectValue (fun iw ->
                iw.WriteNumber("jointIndex", this.JointIndex)
                iw.WriteNumber("nodeIndex", this.NodeIndex)

                this.InverseBindMatrix.WriteJsonProperty(iw, "inverseBindMatrix")
                this.DefaultTranslation.WriteJsonProperty(iw, "defaultTranslation")
                this.DefaultRotation.WriteJsonProperty(iw, "defaultRotation")
                this.DefaultScale.WriteJsonProperty(iw, "defaultScale"))


            ()

    type Armature with

        member this.WriteToJsonObject(writer: Utf8JsonWriter) =
            writer
            |> Json.writeObjectValue (fun w ->
                w
                |> Json.writeArrayProperty
                    (fun iw ->
                        for p in this.RootBones do
                            p.WriteToJson iw)
                    "rootBones"


                w
                |> Json.writeArrayProperty
                    (fun iw ->
                        for j in this.Joints do
                            iw
                            |> Json.writeObjectValue (fun ow ->
                                ow.WriteNumber("key", j.Key)
                                ow.WritePropertyName("value")
                                j.Value.WriteToJson ow))
                    "joints")

    type AnimationChannel with
        member this.WriteToJson(writer: Utf8JsonWriter) =
            writer
            |> Json.writeObjectValue (fun iw ->
                iw.WriteNumber("nodeId", this.NodeId)

                iw
                |> Json.writeArrayProperty
                    (fun aw ->
                        for kf in this.TranslationKeyframes do
                            aw
                            |> Json.writeObjectValue (fun ow ->
                                ow.WriteNumber("time", kf.Time)
                                kf.Value.WriteJsonProperty(ow, "value")))
                    "translationKeyframes"

                iw
                |> Json.writeArrayProperty
                    (fun aw ->
                        for kf in this.RotationKeyframes do
                            aw
                            |> Json.writeObjectValue (fun ow ->
                                ow.WriteNumber("time", kf.Time)
                                kf.Value.WriteJsonProperty(ow, "value")))
                    "rotationKeyframes"

                iw
                |> Json.writeArrayProperty
                    (fun aw ->
                        for kf in this.ScaleKeyframes do
                            aw
                            |> Json.writeObjectValue (fun ow ->
                                ow.WriteNumber("time", kf.Time)
                                kf.Value.WriteJsonProperty(ow, "value")))
                    "scaleKeyframes")

    type AnimationEvent with
        member this.WriteToJson(writer: Utf8JsonWriter) =
            writer
            |> Json.writeObjectValue (fun iw ->
                iw.WriteString("name", this.Name)
                iw.WriteNumber("time", this.Time))

    type AnimationClip with
        member this.WriteToJsonObject(writer: Utf8JsonWriter) =
            writer
            |> Json.writeObjectValue (fun w ->
                w.WriteString("name", this.Name)
                w.WriteNumber("duration", this.Duration)
                w.WriteBoolean("isLoop", this.IsLoop)

                w
                |> Json.writeArrayProperty
                    (fun iw ->
                        for c in this.Channels do
                            iw
                            |> Json.writeObjectValue (fun ow ->
                                ow.WriteNumber("key", c.Key)
                                ow.WritePropertyName("value")
                                c.Value.WriteToJson ow))
                    "channels"

                w
                |> Json.writeArrayProperty
                    (fun iw ->
                        for e in this.Events do
                            e.WriteToJson iw)
                    "events")
