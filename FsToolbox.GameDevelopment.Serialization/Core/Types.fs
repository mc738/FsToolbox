namespace FsToolbox.GameDevelopment.Serialization.Core

open System
open System.Collections.Generic
open System.IO
open System.Text
open FsToolbox.GameDevelopment.Animations.Armatures
open FsToolbox.GameDevelopment.Animations.Clips
open FsToolbox.GameDevelopment.Geometry.Types

[<AutoOpen>]
module Types =

    [<RequireQualifiedAccess>]
    type SerializationTargetError = InvalidOperation of string

    type ISerializationTarget =
        inherit IDisposable

        abstract member GetStream: unit -> Result<Stream, SerializationTargetError>


    type FileSerializationTarget(path: string) =
        let fs = File.Create path


        interface ISerializationTarget with
            member x.Dispose() = fs.Dispose()
            member this.GetStream() = fs :> Stream |> Ok

    type MemorySerializationTarget() =
        let ms = new MemoryStream()

        interface ISerializationTarget with
            member x.Dispose() = ms.Dispose()
            member this.GetStream() = ms :> Stream |> Ok




    [<RequireQualifiedAccess>]
    type SerializationSourceError = InvalidOperation of string

    type ISerializationSource =
        inherit IDisposable

        abstract member GetAsString: unit -> Result<string, SerializationSourceError>

        abstract member GetAsBytes: unit -> Result<byte[], SerializationSourceError>

        abstract member GetAsStream: unit -> Result<Stream, SerializationSourceError>

    [<RequireQualifiedAccess>]
    type SerializationError =
        | TargetError of SerializationTargetError
        | UnhandledException of exn

    type DeserializationError =
        | SourceError of SerializationSourceError
        | UnhandledException of exn



    type FileSerializationSource(path: string) =
        let fs = File.OpenRead path


        interface ISerializationSource with
            member x.Dispose() = fs.Dispose()

            member this.GetAsBytes() =
                use ms = new MemoryStream()
                fs.CopyTo(ms)
                ms.ToArray() |> Ok

            member this.GetAsStream() = fs :> Stream |> Ok

            member this.GetAsString() =
                use ms = new MemoryStream()
                fs.CopyTo(ms)
                ms.ToArray() |> Encoding.UTF8.GetString |> Ok

    type MemorySerializationSource() =
        let ms = new MemoryStream()

        interface ISerializationSource with
            member x.Dispose() = ms.Dispose()
            member this.GetAsBytes() = ms.ToArray() |> Ok
            member this.GetAsStream() = ms :> Stream |> Ok

            member this.GetAsString() =
                ms.ToArray() |> Encoding.UTF8.GetString |> Ok


    type ITypeSerializer =

        abstract member SerializeModel: ISerializationTarget * Model3D -> Result<unit, SerializationError>

        abstract member DeserializeModel: ISerializationSource -> Result<Model3D, DeserializationError>

        abstract member SerializeArmature: ISerializationTarget * Armature -> Result<unit, SerializationError>

        abstract member DeserializeArmature: ISerializationSource -> Result<Armature, DeserializationError>

        abstract member SerializeAnimationClip: ISerializationTarget * AnimationClip -> Result<unit, SerializationError>

        abstract member DeserializeAnimationClip: ISerializationSource -> Result<AnimationClip, DeserializationError>
