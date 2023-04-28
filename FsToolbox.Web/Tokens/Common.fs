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
          Length: TokenLength }

        static member Create(?start: DateTime, ?minutes: int, ?seconds: int) =
            { Start = start |> Option.defaultValue DateTime.UtcNow
              Length = TokenLength.FromStart(minutes |> Option.defaultValue 0, seconds |> Option.defaultValue 0) }

        static member Create(until: DateTime) =
            { Start = DateTime.UtcNow
              Length = TokenLength.Specific until }
            
        member tlt.GetStart() = tlt.Start

        member tlt.GetExpiry() =
            match tlt.Length with
            | FromStart(minutes, seconds) -> tlt.Start.AddSeconds(float ((60 * minutes) + seconds))
            | Specific dateTime -> dateTime

    and TokenLength =
        | FromStart of Minutes: int * Seconds: int
        | Specific of DateTime
