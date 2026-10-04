namespace FsToolbox.GameDevelopment.Serialization

open FsToolbox.GameDevelopment.Geometry.Types


type ISerializationTarget =

    abstract member Serialize: obj -> string


type ISerializationSource =

    abstract member Deserialize: string -> obj

[<RequireQualifiedAccess>]
type SerializationError = UnhandledException of exn


type ISerializer =

    abstract member SerializeModel: ISerializationTarget * Model3D -> unit

    abstract member DeserializeModel: ISerializationSource -> Result<Model3D, SerializationError>
