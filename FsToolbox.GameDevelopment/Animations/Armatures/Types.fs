namespace FsToolbox.GameDevelopment.Animations.Armatures

open System.Numerics

[<AutoOpen>]
module Types =

    type NodeIndex = int

    type JointIndex = int

    type ArmatureBone =
        { NodeId: int
          Name: string
          Children: ArmatureBone list }

    type ArmatureJoint =
        { JointIndex: JointIndex
          NodeIndex: NodeIndex
          InverseBindMatrix: Matrix4x4
          DefaultTranslation: Vector3
          DefaultRotation: Quaternion
          DefaultScale: Vector3 }

        static member Default =
            { JointIndex = -1
              NodeIndex = -1
              InverseBindMatrix = Matrix4x4.Identity
              DefaultTranslation = Vector3.Zero
              DefaultRotation = Quaternion.Identity
              DefaultScale = Vector3.One }

    type Armature =
        {
            RootBones: ArmatureBone list
            /// <summary>
            /// A map storing joints with their respective node index.
            /// </summary>
            Joints: Map<NodeIndex, ArmatureJoint>
        }
