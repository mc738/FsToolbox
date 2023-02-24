namespace FsToolbox.Core

open System
open System.IO
open System.Security.Cryptography
open System.Text

[<RequireQualifiedAccess>]
module Rsa =
    
    /// Create a base64 encoded RSA SHA256 signature for a value based on private RSA details.
    let createRsaSignature privateRsaXml (value: string) =
        use rsa = new RSACryptoServiceProvider()
        rsa.FromXmlString(privateRsaXml)
        use ms = new MemoryStream(value |> Encoding.UTF8.GetBytes)
        
        rsa.SignData(ms, "SHA256") |> Convert.ToBase64String
    
    /// Verify a base64 encoded RSA SHA256 signature based on public RSA details.
    let verifyRsaSignature (publicRsaXml) (value: string) (signature: string) =
        use rsa = new RSACryptoServiceProvider()
        rsa.FromXmlString(publicRsaXml)

        rsa.VerifyData(value |> Encoding.UTF8.GetBytes, "SHA256", signature |> Convert.FromBase64String)