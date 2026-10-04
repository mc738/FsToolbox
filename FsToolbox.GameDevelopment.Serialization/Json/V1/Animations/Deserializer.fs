namespace FsToolbox.GameDevelopment.Serialization.Json.V1.Animations

open System.Numerics
open System.Text.Json
open FsToolbox.Core
open FsToolbox.GameDevelopment.Animations.Armatures
open FsToolbox.GameDevelopment.Animations.Clips
open FsToolbox.GameDevelopment.Serialization.Json.V1

module Deserializer =

    let rec deserializeArmatureBone (root: JsonElement) =
        match
            Json.tryGetIntProperty "nodeId" root,
            Json.tryGetStringProperty "name" root,
            Json.tryGetArrayProperty "children" root
        with
        | None, _, _ -> JsonReadError.MissingProperty "nodeId" |> Error
        | _, None, _ -> JsonReadError.MissingProperty "name" |> Error
        | _, _, None -> JsonReadError.MissingProperty "children" |> Error
        | Some nodeId, Some name, Some children ->
            let childrenResult = ResizeArray<ArmatureBone>()
            let mutable error: JsonReadError option = None

            for child in children do
                if error.IsNone then
                    match deserializeArmatureBone child with
                    | Ok c -> childrenResult.Add c
                    | Error e -> error <- Some e

            match error with
            | Some e -> Error e
            | None ->
                Ok(
                    { NodeId = nodeId
                      Name = name
                      Children = childrenResult |> List.ofSeq }
                    : ArmatureBone
                )

    let deserializeArmatureJoint (root: JsonElement) =
        match
            Json.tryGetIntProperty "jointIndex" root,
            Json.tryGetIntProperty "nodeIndex" root,
            Json.tryGetProperty "inverseBindMatrix" root
            |> Option.map Deserializer.deserializeMatrix4x4
            |> Option.defaultValue (Error(JsonReadError.MissingProperty "inverseBindMatrix")),
            Json.tryGetProperty "defaultTranslation" root
            |> Option.map Deserializer.deserializeVector3
            |> Option.defaultValue (Error(JsonReadError.MissingProperty "defaultTranslation")),
            Json.tryGetProperty "defaultRotation" root
            |> Option.map Deserializer.deserializeQuaternion
            |> Option.defaultValue (Error(JsonReadError.MissingProperty "defaultRotation")),
            Json.tryGetProperty "defaultScale" root
            |> Option.map Deserializer.deserializeVector3
            |> Option.defaultValue (Error(JsonReadError.MissingProperty "scale"))
        with
        | None, _, _, _, _, _ -> JsonReadError.MissingProperty "jointIndex" |> Error
        | _, None, _, _, _, _ -> JsonReadError.MissingProperty "nodeIndex" |> Error
        | _, _, Error e, _, _, _ -> Error e
        | _, _, _, Error e, _, _ -> Error e
        | _, _, _, _, Error e, _ -> Error e
        | _, _, _, _, _, Error e -> Error e
        | Some ji, Some ni, Ok ibm, Ok dt, Ok dr, Ok ds ->

            ({ JointIndex = ji
               NodeIndex = ni
               InverseBindMatrix = ibm
               DefaultTranslation = dt
               DefaultRotation = dr
               DefaultScale = ds }
            : ArmatureJoint)
            |> Ok

    let deserializeArmature (root: JsonElement) =
        match Json.tryGetArrayProperty "rootBones" root, Json.tryGetArrayProperty "joints" root with
        | None, _ -> JsonReadError.MissingProperty "rootBones" |> Error
        | _, None -> JsonReadError.MissingProperty "joints" |> Error
        | Some bones, Some joints ->
            let mutable error: JsonReadError option = None

            let rootBoneResults = ResizeArray<ArmatureBone>()
            let jointsResults = ResizeArray<int * ArmatureJoint>()

            for bone in bones do
                if error.IsNone then
                    match deserializeArmatureBone bone with
                    | Ok b -> rootBoneResults.Add b
                    | Error e -> error <- Some e

            for joint in joints do
                if error.IsNone then
                    match Json.tryGetIntProperty "key" joint, Json.tryGetProperty "value" joint with
                    | None, _ -> error <- Some(JsonReadError.MissingProperty "key")
                    | _, None -> error <- Some(JsonReadError.MissingProperty "value")
                    | Some key, Some value ->
                        match deserializeArmatureJoint value with
                        | Ok j -> jointsResults.Add(key, j)
                        | Error e -> error <- Some e

            Ok(
                { RootBones = []
                  Joints = failwith "todo" }
                : Armature
            )

    let deserializeAnimationChannel (root: JsonElement) =
        match
            Json.tryGetIntProperty "nodeId" root,
            Json.tryGetArrayProperty "translationKeyframes" root,
            Json.tryGetArrayProperty "rotationKeyframes" root,
            Json.tryGetArrayProperty "scaleKeyframes" root
        with
        | None, _, _, _ -> JsonReadError.MissingProperty "nodeId" |> Error
        | _, None, _, _ -> JsonReadError.MissingProperty "translationKeyframes" |> Error
        | _, _, None, _ -> JsonReadError.MissingProperty "rotationKeyframes" |> Error
        | _, _, _, None -> JsonReadError.MissingProperty "scaleKeyframes" |> Error
        | Some ni, Some tkf, Some rkf, Some skf ->
            let mutable error: JsonReadError option = None

            let translationKeyframes = ResizeArray<Keyframe<Vector3>>()
            let rotationKeyframes = ResizeArray<Keyframe<Quaternion>>()
            let scaleKeyframes = ResizeArray<Keyframe<Vector3>>()

            for keyframe in tkf do
                if error.IsNone then
                    match
                        Json.tryGetSingleProperty "time" keyframe,
                        Json.tryGetProperty "value" keyframe
                        |> Option.map Deserializer.deserializeVector3
                        |> Option.defaultValue (Error(JsonReadError.MissingProperty "value"))
                    with
                    | None, _ -> error <- Some(JsonReadError.MissingProperty "time")
                    | _, Error e -> error <- Some e
                    | Some time, Ok value -> translationKeyframes.Add({ Time = time; Value = value })

            for keyframe in rkf do
                if error.IsNone then
                    match
                        Json.tryGetSingleProperty "time" keyframe,
                        Json.tryGetProperty "value" keyframe
                        |> Option.map Deserializer.deserializeQuaternion
                        |> Option.defaultValue (Error(JsonReadError.MissingProperty "value"))
                    with
                    | None, _ -> error <- Some(JsonReadError.MissingProperty "time")
                    | _, Error e -> error <- Some e
                    | Some time, Ok value -> rotationKeyframes.Add({ Time = time; Value = value })

            for keyframe in skf do
                if error.IsNone then
                    match
                        Json.tryGetSingleProperty "time" keyframe,
                        Json.tryGetProperty "value" keyframe
                        |> Option.map Deserializer.deserializeVector3
                        |> Option.defaultValue (Error(JsonReadError.MissingProperty "value"))
                    with
                    | None, _ -> error <- Some(JsonReadError.MissingProperty "time")
                    | _, Error e -> error <- Some e
                    | Some time, Ok value -> scaleKeyframes.Add({ Time = time; Value = value })

            Ok(
                { NodeId = 0
                  TranslationKeyframes = translationKeyframes.ToArray()
                  RotationKeyframes = rotationKeyframes.ToArray()
                  ScaleKeyframes = scaleKeyframes.ToArray() }
                : AnimationChannel
            )

    let deserializeAnimationEvent (root: JsonElement) =
        match Json.tryGetSingleProperty "time" root, Json.tryGetStringProperty "name" root with
        | None, _ -> JsonReadError.MissingProperty "time" |> Error
        | _, None -> JsonReadError.MissingProperty "name" |> Error
        | Some time, Some name -> Ok({ Time = time; Name = name })

    let deserializeAnimationClip (root: JsonElement) =
        match Json.tryGetStringProperty "name" root, Json.tryGetArrayProperty "tracks" root with
        | None, _ -> JsonReadError.MissingProperty "name" |> Error
        | _, None -> JsonReadError.MissingProperty "tracks" |> Error
        | Some name, Some tracks ->
            ({ Name = ""
               Duration = failwith "todo"
               IsLoop = failwith "todo"
               Channels = failwith "todo"
               Events = failwith "todo" }
            : AnimationClip)
            |> Ok
