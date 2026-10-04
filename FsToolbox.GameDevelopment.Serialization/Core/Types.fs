namespace FsToolbox.GameDevelopment.Serialization.Core

open System
open System.Collections.Generic
open FsToolbox.GameDevelopment.Animations.Armatures
open FsToolbox.GameDevelopment.Animations.Clips
open FsToolbox.GameDevelopment.Geometry.Types

[<AutoOpen>]
module Types =

    type ISerializationTarget =

        abstract member Serialize: obj -> string

    type ISerializationSource =

        abstract member Deserialize: string -> obj

    [<RequireQualifiedAccess>]
    type SerializationError = UnhandledException of exn

    type ITypeSerializer =

        abstract member SerializeModel: ISerializationTarget * Model3D -> unit

        abstract member DeserializeModel: ISerializationSource -> Result<Model3D, SerializationError>

        abstract member SerializeArmature: ISerializationTarget * Armature -> unit

        abstract member DeserializeArmature: ISerializationSource -> Result<Armature, SerializationError>

        abstract member SerializeAnimationClip: ISerializationTarget * AnimationClip -> unit

        abstract member DeserializeAnimationClip: ISerializationSource -> Result<AnimationClip, SerializationError>
