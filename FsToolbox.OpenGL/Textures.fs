namespace FsToolbox.OpenGL.Textures

open System
open System.IO
open System.Runtime.InteropServices
open Silk.NET.OpenGL
open StbImageSharp

type OpenGLTexture(gl: GL, data: byte array, width: int, height: int) as this =

    let mutable handle = 0u

    do
        handle <- gl.GenTexture()
        this.Bind(TextureUnit.Texture0)

        //let result = image //ImageResult.FromMemory(File.ReadAllBytes(), ColorComponents.RedGreenBlueAlpha)

        //use imgBuf = fixed result.Data
        //let imgPtr = imgBuf |> NativePtr.toVoidPtr

        //MemoryMarshal.CreateReadOnlySpan(&result.Data.[0], result.Data.Length)

        gl.TexImage2D(
            TextureTarget.Texture2D,
            0,
            InternalFormat.Rgba,
            width |> uint,
            height |> uint,
            0,
            PixelFormat.Rgba,
            PixelType.UnsignedByte,
            MemoryMarshal.CreateReadOnlySpan(&data.[0], data.Length)
        )

        this.SetParameters()

    interface IDisposable with
        member this.Dispose() = gl.DeleteTexture(handle)

    static member CreateFromFile(gl, path) =
        let result =
            ImageResult.FromMemory(File.ReadAllBytes(path), ColorComponents.RedGreenBlueAlpha)

        new OpenGLTexture(gl, result.Data, result.Width, result.Height)

    static member CreateFromBytes(gl, data: byte array, width: int, height: int) = new OpenGLTexture(gl, data, width, height)

    static member CreateFromStream(gl, stream) =
        let result = ImageResult.FromStream(stream, ColorComponents.RedGreenBlueAlpha)

        new OpenGLTexture(gl, result.Data, result.Width, result.Height)

    member _.Bind(textureSlot: TextureUnit) =
        gl.ActiveTexture(textureSlot)
        gl.BindTexture(TextureTarget.Texture2D, handle)

    member _.SetParameters() =
        let twmRepeat = (TextureWrapMode.ClampToEdge |> int)
        let minFilter = (TextureMinFilter.LinearMipmapLinear |> int)
        let magFilter = (TextureMagFilter.Linear |> int)

        gl.TexParameter(GLEnum.Texture2D, GLEnum.TextureWrapS, &twmRepeat)
        gl.TexParameter(GLEnum.Texture2D, GLEnum.TextureWrapT, &twmRepeat)
        gl.TexParameter(GLEnum.Texture2D, GLEnum.TextureMinFilter, &minFilter)
        gl.TexParameter(GLEnum.Texture2D, GLEnum.TextureMagFilter, &magFilter)
        gl.TexParameter(GLEnum.Texture2D, TextureParameterName.TextureBaseLevel, 0)
        gl.TexParameter(GLEnum.Texture2D, TextureParameterName.TextureMaxLevel, 0)

        // Generate mipmaps
        gl.GenerateMipmap(TextureTarget.Texture2D)
