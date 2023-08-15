namespace FsToolbox.Core

module Strings =

    open System
    open System.Text

    let bytesToHex (bytes: byte array) = Convert.ToHexString bytes

    (*
        bytes
        |> Array.fold (fun (sb: StringBuilder) b -> sb.AppendFormat("{0:x2}", b)) (StringBuilder(bytes.Length * 2))
        |> fun sb -> sb.ToString()
        *)

    let equalOrdinal a b =
        String.Equals(a, b, StringComparison.Ordinal)

    let equalOrdinalIgnoreCase a b =
        String.Equals(a, b, StringComparison.OrdinalIgnoreCase)

    let toOptional (str: string) =
        match String.IsNullOrWhiteSpace str with
        | true -> None
        | false -> Some str

    let fromOptional (str: string option) =
        match str with
        | Some v -> v
        | None -> String.Empty

    let tryToByte (str: string) =
        match Byte.TryParse str with
        | true, v -> Some v
        | false, _ -> None

    let tryToBool (additionTrueValues: string list) (additionalFalseValues: string list) (str: string) =
        let trueValues = [ "true"; "yes"; "ok"; "1"; yield! additionTrueValues ]

        let falseValues = [ "false"; "no"; "none"; "0"; yield! additionalFalseValues ]

        match str.ToLower() with
        | v when trueValues |> List.contains v -> Some true
        | v when falseValues |> List.contains v -> Some false
        | _ ->
            match Boolean.TryParse str with
            | true, v -> Some v
            | false, _ -> None
