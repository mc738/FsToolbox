namespace FsToolbox.Web.Tokens

open System

[<AutoOpen>]
module Common =

    open System.Security.Claims

    let createClaim issuer key value = Claim(key, value, issuer)

    let createClaims issuer (keyValues: (string * string) list) =
        keyValues |> List.map (fun (k, v) -> Claim(k, v, issuer))

    type TokenLifetime =
        { Start: DateTime
          Expiry: DateTime }

        static member Create(?start: DateTime, ?minutes: int, ?seconds: int) =
            let s = start |> Option.defaultValue DateTime.UtcNow
            let mins = minutes |> Option.defaultValue 0
            let secs = seconds |> Option.defaultValue 0

            { Start = s
              Expiry = s.AddSeconds(float ((60 * mins) + secs)) }

        static member Create(expiry: DateTime) =
            { Start = DateTime.UtcNow
              Expiry = expiry }

        static member Create(start: DateTime, expiry: DateTime) = { Start = start; Expiry = expiry }
