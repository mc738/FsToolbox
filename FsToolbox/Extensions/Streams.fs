namespace FsToolbox.Extensions

[<AutoOpen>]
module Streams =

    open System.IO
    open System.Text
    open System.Security.Cryptography
    open FsToolbox.Core
        
    type MemoryStream with

        static member FromUtf8String(str: string) = new MemoryStream(str |> Encoding.UTF8.GetBytes)
        
        member ms.GetSHA256Hash() = Hashing.hashStream (SHA256.Create()) ms
        
        member ms.GetUtf8String() = ms.ToArray() |> Encoding.UTF8.GetString
        
        