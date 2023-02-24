namespace FsToolbox.Core

module Jwt =

    open System.Security.Cryptography
    open System.Text.Json.Serialization
    
    [<CLIMutable>]
    type JwtSettings =
        { [<JsonPropertyName("name")>]
          Name: string
          [<JsonPropertyName("secretKey")>]
          SecretKey: string
          [<JsonPropertyName("audience")>]
          Audience: string
          [<JsonPropertyName("tokenExpiry")>]
          TokenExpiry: float
          [<JsonPropertyName("issuer")>]
          Issuer: string }

    type ServiceJwtSettings =
        { [<JsonPropertyName("name")>]
          Name: string
          [<JsonPropertyName("serverPublicKey")>]
          ServerPublicKey: string
          [<JsonPropertyName("audience")>]
          Audience: string
          [<JsonPropertyName("tokenExpiry")>]
          TokenExpiry: float
          [<JsonPropertyName("issuer")>]
          Issuer: string }

    type JwtTokenValidationError =
        | EncryptionKeyNotFound
        | TokenDecryptionFailed
        | TokenExpired
        | TokenInvalidAudience
        | TokenInvalidLifetime
        | TokenInvalidSignature
        | TokenException of string
        | Unhandled of string

    let createClaim issuer key value = Claim(key, value, issuer)

    let createClaims issuer (keyValues: (string * string) list) =
        keyValues
        |> List.map (fun (k, v) -> Claim(k, v, issuer))

    let createSymmetricToken (settings: JwtSettings) username claims =
        let claims =
            [ Claim("username", username, settings.Issuer) ]
            @ createClaims settings.Issuer claims

        let signedKey =
            SymmetricSecurityKey(Encoding.ASCII.GetBytes(settings.SecretKey))

        let (now: Nullable<DateTime>) = Nullable<DateTime>(DateTime.UtcNow)

        let expiryTime =
            Nullable<DateTime>(now.Value.AddMinutes(settings.TokenExpiry))

        let jwt =
            JwtSecurityToken(
                settings.Issuer,
                settings.Audience,
                claims,
                now,
                expiryTime,
                SigningCredentials(signedKey, SecurityAlgorithms.HmacSha256)
            )

        let jwtSecurityHandler = JwtSecurityTokenHandler()
        jwtSecurityHandler.WriteToken(jwt)

    let createRsaToken issuer audience createdOn expiresOn privateRsaXml username claims =
        let claims =
            [ Claim("username", username, issuer) ]
            @ createClaims issuer claims

        use rsa = new RSACryptoServiceProvider()
        rsa.FromXmlString(privateRsaXml)

        let jwt =
            JwtSecurityToken(
                issuer,
                audience,
                claims,
                Nullable<DateTime>(createdOn),
                Nullable<DateTime>(expiresOn),
                SigningCredentials(RsaSecurityKey(rsa.ExportParameters(true)), SecurityAlgorithms.RsaSha512Signature)
            )

        let jwtSecurityHandler = JwtSecurityTokenHandler()
        jwtSecurityHandler.WriteToken(jwt)

    let validateSymmetricToken (settings: JwtSettings) (token: string) =
        let tokenHandler = JwtSecurityTokenHandler()

        try
            let p = TokenValidationParameters()
            p.ValidateIssuerSigningKey <- true
            p.ValidateIssuer <- true
            p.ValidateAudience <- true
            p.ValidIssuer <- settings.Issuer
            p.ValidAudience <- settings.Audience
            p.IssuerSigningKey <- SymmetricSecurityKey(Encoding.ASCII.GetBytes(settings.SecretKey))

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

    let validateRsaToken issuer audience rsaPublicXml (token: string) =
        let tokenHandler = JwtSecurityTokenHandler()

        try
            use rsa = new RSACryptoServiceProvider()
            rsa.FromXmlString(rsaPublicXml)

            let p = TokenValidationParameters()
            p.ValidateIssuerSigningKey <- true
            p.ValidateIssuer <- true
            p.ValidateAudience <- true
            p.ValidIssuer <- issuer
            p.ValidAudience <- audience
            p.IssuerSigningKey <- RsaSecurityKey(rsa)

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

    type JwtClaim =
        { Issuer: string
          OriginalIssuer: string
          Type: string
          Value: string }

    type JwtClaimMap =
        { Claims: Map<string, JwtClaim> }


        static member Create(claims: JwtClaim list) =
            { Claims =
                  claims
                  |> List.map (fun c -> c.Type, c)
                  |> Map.ofList }

        member jcm.TryGet(claim) = jcm.Claims.TryFind claim

    let getClaims (jwt: JwtSecurityToken) =
        jwt.Claims
        |> List.ofSeq
        |> List.map
            (fun claim ->
                { Issuer = claim.Issuer
                  OriginalIssuer = claim.OriginalIssuer
                  Type = claim.Type
                  Value = claim.Value })
        |> JwtClaimMap.Create

