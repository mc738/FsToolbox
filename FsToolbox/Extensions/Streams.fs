namespace FsToolbox.Extensions

[<AutoOpen>]
module Streams =

    open System.IO
    open System.Text
    open System.Security.Cryptography
    open FsToolbox.Core
        
    type MemoryStream with

        static member FromUtf8String(str: string) = new MemoryStream(str |> Encoding.UTF8.GetBytes)
        
        static member FromBase64String(str: string) =
            new MemoryStream(str |> Conversions.fromBase64)
            
        static member FromHexString(str: string) =
            new MemoryStream(str |> Conversions.fromHex)
            
        member ms.GetSHA256Hash() = Hashing.hashStream (SHA256.Create()) ms
        
        member ms.GetUtf8String() = ms.ToArray() |> Encoding.UTF8.GetString
        
        member ms.GetBase64String() = ms.ToArray() |> Conversions.toBase64
        
        member ms.GetHexString() = ms.ToArray() |> Conversions.toHex
        