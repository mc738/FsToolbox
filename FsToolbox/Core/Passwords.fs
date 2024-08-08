namespace FsToolbox.Core

module Passwords =

    open System
    open System.Security.Cryptography

    let pdkdf2Bytes (key: string) (iterations: int) (hashSize: int) (password: string) (salt: byte array) =

        use pdkdf2 =
            new Rfc2898DeriveBytes(password + key, salt, iterations, HashAlgorithmName.SHA256)

        pdkdf2.GetBytes(hashSize)

    let pdkdf2 (key: string) (iterations: int) (hashSize: int) (password: string) (salt: byte array) =

        use pdkdf2 =
            new Rfc2898DeriveBytes(password + key, salt, iterations, HashAlgorithmName.SHA256)

        pdkdf2Bytes key iterations hashSize password salt |> Convert.ToBase64String
