namespace FsToolbox.Core

[<RequireQualifiedAccess>]
module Encryption =

    open System
    open System.IO
    open System.Security.Cryptography
    open Microsoft.FSharp.Collections
    open Microsoft.FSharp.Core

    let getCryptoBytes length = RandomNumberGenerator.GetBytes length

    let generateKey _ = getCryptoBytes 32

    let generateSalt _ = getCryptoBytes 16

    let encryptBytesAes key iv (data: byte array) =
        use aes = Aes.Create()

        aes.Padding <- PaddingMode.PKCS7

        let encryptor = aes.CreateEncryptor(key, iv)

        use ms = new MemoryStream()
        use cs = new CryptoStream(ms, encryptor, CryptoStreamMode.Write)

        cs.Write(ReadOnlySpan(data))
        cs.FlushFinalBlock()

        ms.ToArray()

    let decryptBytesAes key iv (cipher: byte array) =
        use aes = Aes.Create()

        aes.Padding <- PaddingMode.PKCS7

        let decryptor = aes.CreateDecryptor(key, iv)

        use ms = new MemoryStream(cipher)
        use cs = new CryptoStream(ms, decryptor, CryptoStreamMode.Read)

        Streams.readAllBytes cs

    let pack (bytes: byte array) (salt: byte array) = Array.concat [ salt; bytes ]

    let unpack (bytes: byte array) =
        match bytes.Length > 16 with
        | true -> Array.splitAt (16) bytes |> Ok
        | false -> Error "Input too short, no salt to unpack."
