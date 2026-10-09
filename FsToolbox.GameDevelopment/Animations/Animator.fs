namespace FsToolbox.GameDevelopment.Animations

open System
open System.Numerics
open System.Threading.Channels
open FsToolbox.GameDevelopment.Animations.Armatures
open FsToolbox.GameDevelopment.Animations.Clips

type AnimatorContext() =
    let mutable currentClip: AnimationClip option = None
    let mutable nextClip: AnimationClip option = None
    let mutable currentClipTime = 0f
    let mutable transitionTime = 0f
    let mutable totalTransitionTime = 0f

    member _.CurrentClip = currentClip
    member _.CurrentClipTime = currentClipTime
    member _.NextClip = nextClip
    member _.TransitionTime = transitionTime
    member _.TotalTime = totalTransitionTime

    member _.Tick(dt: float32) =
        currentClipTime <- currentClipTime + dt

        // First, check if any transitions are triggered.
        // If so, these should take priority.

        // However, these might also be timed.

        match currentClip with
        | None -> ()
        | Some cc ->
            if currentClipTime > cc.Duration then
                if cc.IsLoop then
                    // Loop the animation.
                    currentClipTime <- currentClipTime - cc.Duration
                    
                else 
                    currentClip <- None
                ()
            // Check for any events.

            ()

        // Next check if tr

        match nextClip with
        | None -> ()
        | Some nc ->
            transitionTime <- transitionTime + dt

            if transitionTime > totalTransitionTime then
                // Swap clips
                currentClip <- nextClip
                nextClip <- None

                // TODO check - is this correct?? It feels like it probably should be.
                currentClipTime <- transitionTime

                transitionTime <- 0f
                totalTransitionTime <- 0f
            else
                ()

            // Check for any events.
            ()

        ()

    member _.QueueAnimation(animation: AnimationClip, animationTransitionTime: float32) =
        nextClip <- Some animation
        transitionTime <- 0f
        totalTransitionTime <- animationTransitionTime

module AnimatorOperations =

    let sampleChannel<'T>
        (keyframes: Keyframe<'T> array)
        (time: float32)
        (interpolate: 'T * 'T * float32 -> 'T)
        (defaultValue: 'T)
        =
        if keyframes.Length = 0 then
            defaultValue
        else
            let mutable i = 0

            while i < keyframes.Length - 1 && time > keyframes.[i + 1].Time do
                i <- i + 1

            let (ki0, ki1) = if i = keyframes.Length - 1 then i, 0 else i, i + 1

            let k0 = keyframes[ki0]
            let k1 = keyframes[ki1]

            let t = (time - k0.Time) / (k1.Time - k0.Time)

            interpolate (k0.Value, k1.Value, t)


    let sampleTRS (nodeIndex: NodeIndex) (channels: Map<NodeIndex, AnimationChannel>) (time: float32) (joint: ArmatureJoint) =
        match Map.tryFind nodeIndex channels with
        | None ->
            {| Translation = joint.DefaultTranslation
               Rotation = joint.DefaultRotation
               Scale = joint.DefaultScale |}
        | Some channel ->
            {| Translation = sampleChannel channel.TranslationKeyframes time Vector3.Lerp joint.DefaultTranslation
               Rotation = sampleChannel channel.RotationKeyframes time Quaternion.Slerp joint.DefaultRotation
               Scale = sampleChannel channel.ScaleKeyframes time Vector3.Lerp joint.DefaultScale |}

    let calculateLocalMatrix (ctx: AnimatorContext) (nodeIndex: NodeIndex) (joint: ArmatureJoint) =

        let r =
            match ctx.CurrentClip, ctx.NextClip with
            | None, None ->
                {| Translation = joint.DefaultTranslation
                   Rotation = joint.DefaultRotation
                   Scale = joint.DefaultScale |}
            | Some cc, None -> sampleTRS nodeIndex cc.Channels ctx.CurrentClipTime joint
            | None, Some nc -> sampleTRS nodeIndex nc.Channels ctx.CurrentClipTime joint
            | Some cc, Some nc ->
                let clipA = sampleTRS nodeIndex cc.Channels ctx.CurrentClipTime joint

                let clipB = sampleTRS nodeIndex nc.Channels ctx.TransitionTime joint

                let alpha = Math.Clamp(ctx.TransitionTime / ctx.TotalTime, 0f, 1f)

                {| Translation = Vector3.Lerp(clipA.Translation, clipB.Translation, alpha)
                   Rotation = Quaternion.Slerp(clipA.Rotation, clipB.Rotation, alpha)
                   Scale = Vector3.Lerp(clipA.Scale, clipB.Scale, alpha) |}

        Matrix4x4.CreateScale(r.Scale)
        * Matrix4x4.CreateFromQuaternion(r.Rotation)
        * Matrix4x4.CreateTranslation(r.Translation)

    let rec computeGlobalMatrices
        (ctx: AnimatorContext)
        (nodeIndex: int)
        (parentTransform: Matrix4x4)
        (joints: Map<NodeIndex, ArmatureJoint>)
        (bone: ArmatureBone)
        (globalMatrics: outref<Map<int, Matrix4x4>>)
        =

        let joint =
            joints |> Map.tryFind nodeIndex |> Option.defaultValue ArmatureJoint.Default

        let localMatrix = calculateLocalMatrix ctx nodeIndex joint

        let globalMatrix = localMatrix * parentTransform
        //let final = joint.InverseBindMatrix * globalMatrix

        //let globalMatrix = parentTransform * localMatrix
        let final = joint.InverseBindMatrix * globalMatrix
        globalMatrics <- Map.add joint.JointIndex final globalMatrics

        for child in bone.Children do
            computeGlobalMatrices ctx child.NodeId globalMatrix joints child &globalMatrics

type Animator(armature: Armature) =
    let ctx = AnimatorContext()

    member _.Tick(dt: float32) = ctx.Tick(dt)
    
    member _.PlayAnimation(animation: AnimationClip, transitionTime: float32) =
        ctx.QueueAnimation(animation, transitionTime)

    member _.ComputeBoneMatrices() =

        let mutable globalMatrices: Map<JointIndex, Matrix4x4> = Map.empty

        for rootBone in armature.RootBones do
            AnimatorOperations.computeGlobalMatrices
                ctx
                rootBone.NodeId
                Matrix4x4.Identity
                armature.Joints
                rootBone
                &globalMatrices

        armature.Joints.Values
        |> Array.ofSeq
        |> Array.sortBy _.JointIndex
        |> Array.map (fun joint ->
            match globalMatrices.TryFind joint.JointIndex with
            | None -> Matrix4x4.Identity
            | Some globalMatrix -> globalMatrix)

