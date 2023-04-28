namespace FsToolbox.Core

open System
open System.Text

type Failure =
    { Message: string
      Exception: Exception option }

[<AutoOpen>]
module Helpers =
    let attempt<'R> (fn: unit -> 'R) =
        try
            fn () |> Ok
        with
        | exn ->
            { Message = exn.Message
              Exception = Some exn }
            |> Error

[<RequireQualifiedAccess>]
module Conversions =

    let fromUtf8 (str: string) = str |> Encoding.UTF8.GetBytes

    let toUtf8 (bytes: byte array) = bytes |> Encoding.UTF8.GetString

    let bytesToHex (bytes: byte array) =
        bytes
        |> Array.fold (fun (sb: StringBuilder) b -> sb.AppendFormat("{0:x2}", b)) (StringBuilder(bytes.Length * 2))
        |> fun sb -> sb.ToString()
        
    let toBase64 (data: byte array) = Convert.ToBase64String data
    
    let fromBase64 (str: string) = Convert.FromBase64String str