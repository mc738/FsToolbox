namespace FsToolbox.Core

open System
open System.IO
open System.Security.Cryptography
open System.Text

[<RequireQualifiedAccess>]
module Rsa =

    type CspBlob =
        { Private: byte array
          Public: byte array }
        
    type XmlString =
        { Private: string
          Public: string }

    type HashAlgorithm =
        | SHA256

        member ha.Serialize() =
            match ha with
            | SHA256 -> "SHA256"

    [<RequireQualifiedAccess>]
    type KeySize =
        | Default
        | Specific of int
        
        member ks.GetSize() =
            match ks with
            | Default -> 2048
            | Specific v -> v
    
    [<RequireQualifiedAccess>]
    type KeySource =
        | Csp of byte array
        //| Pem of string
        | Xml of string
        
    let generateNewCspBlob (keySize: KeySize) =
        use rsa = new RSACryptoServiceProvider(keySize.GetSize())

        ({ Private = rsa.ExportCspBlob(true)
           Public = rsa.ExportCspBlob(false) }: CspBlob)
    
    let generateNewXml (keySize: KeySize) =
        use rsa = new RSACryptoServiceProvider(keySize.GetSize())
        
        ({ Private = rsa.ToXmlString(true)
           Public = rsa.ToXmlString(false) }: XmlString)
        
       
     
            
    //let generatePem _ =
    //    use rsa = new RSACryptoServiceProvider(2048)
        
        //rsa.

    let importCspBlob (blob: byte array) (rsa: RSACryptoServiceProvider) =
        rsa.ImportCspBlob blob
        rsa

    let import (keySource: KeySource) (rsa: RSACryptoServiceProvider) =
        match keySource with
        | KeySource.Csp csp -> rsa.ImportCspBlob csp
        //| StorageType.Pem pem -> rsa.ImportFromPem pem
        | KeySource.Xml xml -> rsa.FromXmlString xml
        
        rsa
    
    let tryCreateSignature (hashAlgorithm: HashAlgorithm) (value: byte array) (rsa: RSACryptoServiceProvider) =
        match rsa.PublicOnly with
        | true -> Error "Private key is missing"
        | false ->
            use ms = new MemoryStream(value)

            rsa.SignData(ms, hashAlgorithm) |> Ok

    let createSignature (hashAlgorithm: HashAlgorithm) (value: byte array) (rsa: RSACryptoServiceProvider) =
        use ms = new MemoryStream(value)

        rsa.SignData(ms, hashAlgorithm)

    let createSHA256Signature (value: byte array) (rsa: RSACryptoServiceProvider) =
        createSignature HashAlgorithm.SHA256 value rsa

    let verifySignature
        (hashAlgorithm: HashAlgorithm)
        (value: byte array)
        (signature: byte array)
        (rsa: RSACryptoServiceProvider)
        =
        rsa.VerifyData(value, hashAlgorithm.Serialize(), signature)

    let verifySHA256Signature (value: byte array) (signature: byte array) (rsa: RSACryptoServiceProvider) =
        verifySignature HashAlgorithm.SHA256 value signature rsa

    /// Create a base64 encoded RSA SHA256 signature for a value based on private RSA details.        
    [<Obsolete("Use Rsa.createSignature instead.")>]
    let createRsaSignature privateRsaXml (value: string) =
        use rsa = new RSACryptoServiceProvider()
        rsa.FromXmlString(privateRsaXml)
        use ms = new MemoryStream(value |> Encoding.UTF8.GetBytes)

        rsa.SignData(ms, "SHA256") |> Convert.ToBase64String

    /// Verify a base64 encoded RSA SHA256 signature based on public RSA details.
    [<Obsolete("Use Rsa.createSignature instead.")>]
    let verifyRsaSignature (publicRsaXml) (value: string) (signature: string) =
        use rsa = new RSACryptoServiceProvider()
        rsa.FromXmlString(publicRsaXml)

        rsa.VerifyData(value |> Encoding.UTF8.GetBytes, "SHA256", signature |> Convert.FromBase64String)
