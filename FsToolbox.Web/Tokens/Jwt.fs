namespace FsToolbox.Web.Tokens

module Jwt =

    open System
    open System.IdentityModel.Tokens.Jwt
    open System.Security.Claims
    open System.Security.Cryptography
    open System.Text
    open System.Text.Json.Serialization
    open Microsoft.IdentityModel.Tokens

    type JwtTokenValidationError =
        | EncryptionKeyNotFound
        | TokenDecryptionFailed
        | TokenExpired
        | TokenInvalidAudience
        | TokenInvalidLifetime
        | TokenInvalidSignature
        | TokenException of string
        | Unhandled of string

    type CreateTokenParameters =
        { Issuer: string
          Audience: string
          Lifetime: TokenLifetime
          Claims: (string * string) list }

        member tp.CreateClaims() = createClaims tp.Issuer tp.Claims

    type ValidateTokenParameters =
        { Issuer: string option
          Audience: string option
          ValidateLifetime: bool }

    let createSymmetricToken (parameters: CreateTokenParameters) (secretKey: byte array) =

        let signedKey = SymmetricSecurityKey(secretKey)

        let jwt =
            JwtSecurityToken(
                parameters.Issuer,
                parameters.Audience,
                parameters.CreateClaims(),
                parameters.Lifetime.Start,
                parameters.Lifetime.Expiry,
                SigningCredentials(signedKey, SecurityAlgorithms.HmacSha256)
            )

        let jwtSecurityHandler = JwtSecurityTokenHandler()
        jwtSecurityHandler.WriteToken(jwt)

    let createRsaToken (parameters: CreateTokenParameters) (rsa: RSACryptoServiceProvider) =

        let jwt =
            JwtSecurityToken(
                parameters.Issuer,
                parameters.Audience,
                parameters.CreateClaims(),
                parameters.Lifetime.Start,
                parameters.Lifetime.Expiry,
                SigningCredentials(RsaSecurityKey(rsa.ExportParameters(true)), SecurityAlgorithms.RsaSha512Signature)
            )

        let jwtSecurityHandler = JwtSecurityTokenHandler()
        jwtSecurityHandler.WriteToken(jwt)

    let validateSymmetricToken (parameters: ValidateTokenParameters) (secretKey: byte array) (token: string) =
        let tokenHandler = JwtSecurityTokenHandler()

        try
            let p = TokenValidationParameters()
            p.ValidateIssuerSigningKey <- true
            match parameters.Issuer with
            | Some issuer ->
                p.ValidateIssuer <- true
                p.ValidIssuer <- issuer
            | None -> ()
            
            match parameters.Audience with
            | Some audience ->
                p.ValidateAudience <- true
                p.ValidAudience <- audience
            | None -> ()
            
            p.IssuerSigningKey <- SymmetricSecurityKey(secretKey)
            p.ValidateLifetime <- parameters.ValidateLifetime
            
            match tokenHandler.ValidateToken(token, p) with
            | _ -> Ok()
        with
        | :? ArgumentNullException -> Error(JwtTokenValidationError.Unhandled "Null value")
        | :? ArgumentException -> Error(JwtTokenValidationError.Unhandled "Argument exception")
        | :? SecurityTokenEncryptionKeyNotFoundException -> Error EncryptionKeyNotFound
        | :? SecurityTokenDecryptionFailedException -> Error TokenDecryptionFailed
        | :? SecurityTokenExpiredException -> Error TokenExpired
        | :? SecurityTokenInvalidAudienceException -> Error TokenInvalidAudience
        | :? SecurityTokenInvalidLifetimeException -> Error TokenInvalidLifetime
        | :? SecurityTokenInvalidSignatureException -> Error TokenInvalidSignature
        | :? SecurityTokenException -> Error(TokenException "Error")

    let validateRsaToken (parameters: ValidateTokenParameters) (token: string) (rsa: RSACryptoServiceProvider) =
        let tokenHandler = JwtSecurityTokenHandler()

        try

            let p = TokenValidationParameters()
            p.ValidateIssuerSigningKey <- true
            match parameters.Issuer with
            | Some issuer ->
                p.ValidateIssuer <- true
                p.ValidIssuer <- issuer
            | None -> ()
            
            match parameters.Audience with
            | Some audience ->
                p.ValidateAudience <- true
                p.ValidAudience <- audience
            | None -> ()
            
            p.IssuerSigningKey <- RsaSecurityKey(rsa)
            p.ValidateLifetime <- parameters.ValidateLifetime
            
            match tokenHandler.ValidateToken(token, p) with
            | _ -> Ok()
        with
        | :? ArgumentNullException -> Error(JwtTokenValidationError.Unhandled "Null value")
        | :? ArgumentException -> Error(JwtTokenValidationError.Unhandled "Argument exception")
        | :? SecurityTokenEncryptionKeyNotFoundException -> Error EncryptionKeyNotFound
        | :? SecurityTokenDecryptionFailedException -> Error TokenDecryptionFailed
        | :? SecurityTokenExpiredException -> Error TokenExpired
        | :? SecurityTokenInvalidAudienceException -> Error TokenInvalidAudience
        | :? SecurityTokenInvalidLifetimeException -> Error TokenInvalidLifetime
        | :? SecurityTokenInvalidSignatureException -> Error TokenInvalidSignature
    //| :? SecurityTokenException -> Error(TokenException "Error")

    let parse token =
        let tokenHandler = JwtSecurityTokenHandler()
        tokenHandler.ReadJwtToken token

    let getClaimsMap (jwt: JwtSecurityToken) =
        jwt.Claims
        |> List.ofSeq
        |> List.map (fun claim -> claim.Type, claim)
        |> Map.ofList
