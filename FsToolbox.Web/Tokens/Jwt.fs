namespace FsToolbox.Web.Tokens

module Jwt =

    open System
    open System.IdentityModel.Tokens.Jwt
    open System.Security.Cryptography
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
          ValidateLifetime: bool
          CacheSignatureProviders: bool }

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

    let createRsaToken (parameters: CreateTokenParameters) (rsa: RSA) =

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

    let validateRsaToken (parameters: ValidateTokenParameters) (token: string) (rsa: RSA) =
        let tokenHandler = JwtSecurityTokenHandler()

        try

            let p = TokenValidationParameters()

            let cp = CryptoProviderFactory()

            // This is used to fix this is issue:
            // https://github.com/AzureAD/azure-activedirectory-identitymodel-extensions-for-dotnet/issues/1433
            // It came up when 2 tests in a project used the same rsa csp.
            // Both tests would pass when run by themselves but one would fail when both were run at the same time.
            // However the validation would actually work if you tried it again after that.
            // See https://github.com/mc738/FsToolbox/issues/2
            cp.CacheSignatureProviders <- parameters.CacheSignatureProviders

            p.CryptoProviderFactory <- cp

            p.ValidateIssuerSigningKey <- true

            match parameters.Issuer with
            | Some issuer ->
                p.ValidateIssuer <- true
                p.ValidIssuer <- issuer
            | None -> p.ValidateIssuer <- false

            match parameters.Audience with
            | Some audience ->
                p.ValidateAudience <- true
                p.ValidAudience <- audience
            | None -> p.ValidateAudience <- false

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
