﻿namespace FsToolbox.Extensions

open System.Text

[<AutoOpen>]
module Streams =

    open System.IO
    open System.Security.Cryptography
    open FsToolbox.Core
        
    type MemoryStream with

        member ms.GetSHA256Hash() = Hashing.hashStream (SHA256.Create()) ms
        
        member ms.GetUtf8String() = ms.ToArray() |> Encoding.UTF8.GetString