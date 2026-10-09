namespace FsToolbox.GameDevelopment.Animations.Clips

open System.Numerics
open FsToolbox.GameDevelopment.Animations.Armatures

module Operations =

    let sampleChannel<'T> (keyframes: Keyframe<'T> array) (time: float32) (interpolate: 'T * 'T * float32 -> 'T) =
        //if keyframes.Length = 0 then defaultValue
        let mutable i = 0

        while i < keyframes.Length - 1 && time > keyframes.[i + 1].Time do
            i <- i + 1

        let (ki0, ki1) = if i = keyframes.Length - 1 then i, 0 else i, i + 1

        let k0 = keyframes[ki0]
        let k1 = keyframes[ki1]

        let t = (time - k0.Time) / (k1.Time - k0.Time)

        interpolate (k0.Value, k1.Value, t)

    let calculateLocalMatrix
        (nodeIndex: NodeIndex)
        (channels: Map<NodeIndex, AnimationChannel>)
        (time: float32)
        (joint: ArmatureJoint)
        =
        match Map.tryFind nodeIndex channels with
        | None ->
            Matrix4x4.CreateScale(joint.DefaultScale)
            * Matrix4x4.CreateFromQuaternion(joint.DefaultRotation)
            * Matrix4x4.CreateTranslation(joint.DefaultTranslation)
        | Some channel ->
            let t = sampleChannel channel.TranslationKeyframes time Vector3.Lerp
            let r = sampleChannel channel.RotationKeyframes time Quaternion.Slerp
            let s = sampleChannel channel.ScaleKeyframes time Vector3.Lerp

            // System.Numerics is Row-Major, so S * R * T is correct for local layout
            Matrix4x4.CreateScale(s)
            * Matrix4x4.CreateFromQuaternion(r)
            * Matrix4x4.CreateTranslation(t)

    let rec computeGlobalMatrices
        (nodeIndex: int)
        (parentTransform: Matrix4x4)
        (channels: Map<NodeIndex, AnimationChannel>)
        (time: float32)
        (joints: Map<NodeIndex, ArmatureJoint>)
        (bone: ArmatureBone)
        (globalMatrics: outref<Map<int, Matrix4x4>>)
        =

        let joint =
            joints |> Map.tryFind nodeIndex |> Option.defaultValue ArmatureJoint.Default

        let localMatrix = calculateLocalMatrix nodeIndex channels time joint

        let globalMatrix = localMatrix * parentTransform
        //let final = joint.InverseBindMatrix * globalMatrix


        //let globalMatrix = parentTransform * localMatrix
        let final = joint.InverseBindMatrix * globalMatrix
        globalMatrics <- Map.add joint.JointIndex final globalMatrics


        for child in bone.Children do
            computeGlobalMatrices child.NodeId globalMatrix channels time joints child &globalMatrics

    let generateShaderPalette
        (armature: Armature)
        (channels: Map<NodeIndex, AnimationChannel>)
        (time: float32)
        (joints: Map<NodeIndex, ArmatureJoint>)
        =

        let mutable globalMatrices: Map<JointIndex, Matrix4x4> = Map.empty

        for rootBone in armature.RootBones do
            computeGlobalMatrices rootBone.NodeId Matrix4x4.Identity channels time joints rootBone &globalMatrices

        joints.Values
        |> Array.ofSeq
        |> Array.sortBy (fun j -> j.JointIndex)
        |> Array.map (fun joint ->
            match globalMatrices.TryFind joint.JointIndex with
            | None -> Matrix4x4.Identity
            | Some globalMatrix -> (*joint.InverseBindMatrix **) globalMatrix)
