namespace FsToolbox.Core

open System.Globalization
open System.Text.Encodings.Web
open System.Web
open Microsoft.FSharp.Core

module Strings =

    open System

    [<RequireQualifiedAccess>]
    type NormalizeCaseType =
        | UpperCase
        | LowerCase
        | None

    [<RequireQualifiedAccess>]
    type EncodingType =
        | Url
        | Html
        | None

    type SlugifySettings =

        { Replacement: string
          CharactersToRemove: char array
          FilterFunction: char -> bool
          NormalizeCase: NormalizeCaseType
          Encoding: EncodingType }

        static member Default =
            { Replacement = "_"
              CharactersToRemove =
                [| '_'
                   '-'
                   '?'
                   '/'
                   '\\'
                   '.'
                   ','
                   '!'
                   '"'
                   '''
                   '£'
                   '$'
                   '%'
                   '^'
                   '&'
                   '*'
                   '('
                   ')'
                   '['
                   ']'
                   '{'
                   '}'
                   ' '
                   '\t'
                   ':'
                   ':'
                   '@'
                   '<'
                   '>'
                   '`'
                   '¬'
                   '=' |]
              FilterFunction = fun c -> Char.IsLetterOrDigit c || Char.IsPunctuation c
              NormalizeCase = NormalizeCaseType.LowerCase
              Encoding = EncodingType.None }

    type RandomStringSettings =
        { IncludeLowerCaseLetters: bool
          IncludeUpperCaseLetters: bool
          IncludeDigits: bool
          IncludeSymbol: bool
          Encoding: EncodingType }

        member rss.GetCharacters() =
            [ if rss.IncludeLowerCaseLetters then
                  "abcdefghijklmnopqrstuvwxyz"
              if rss.IncludeUpperCaseLetters then
                  "ABCDEFGHIJKLMNOPQRSTUVWXYZ"
              if rss.IncludeDigits then
                  "0123456789"
              if rss.IncludeSymbol then
                  ":;@'<>,/?!\"£$%^&*()\\|¬`#~-_=+" ]
            |> String.concat ""

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

    let tryToDouble (str: string) =
        match Double.TryParse str with
        | true, v -> Some v
        | false, _ -> None

    let tryToDecimal (str: string) =
        match Decimal.TryParse str with
        | true, v -> Some v
        | false, _ -> None

    let tryToSingle (str: string) =
        match Single.TryParse str with
        | true, v -> Some v
        | false, _ -> None

    let tryToInt16 (str: string) =
        match Int16.TryParse str with
        | true, v -> Some v
        | false, _ -> None

    let tryToUInt16 (str: string) =
        match UInt16.TryParse str with
        | true, v -> Some v
        | false, _ -> None

    let tryToInt32 (str: string) =
        match Int32.TryParse str with
        | true, v -> Some v
        | false, _ -> None

    let tryToUInt32 (str: string) =
        match UInt32.TryParse str with
        | true, v -> Some v
        | false, _ -> None

    let tryToInt64 (str: string) =
        match Int64.TryParse str with
        | true, v -> Some v
        | false, _ -> None

    let tryToUInt64 (str: string) =
        match UInt64.TryParse str with
        | true, v -> Some v
        | false, _ -> None

    let tryToFormattedDateTime
        (format: string)
        (provider: IFormatProvider option)
        (style: DateTimeStyles option)
        (str: string)
        =
        match
            DateTime.TryParseExact(
                str,
                format,
                provider |> Option.defaultValue CultureInfo.InvariantCulture,
                style |> Option.defaultValue DateTimeStyles.None
            )
        with
        | true, v -> Some v
        | false, _ -> None

    let tryToDateTime (str: string) =
        match DateTime.TryParse str with
        | true, v -> Some v
        | false, _ -> None

    let tryToGuid (format: string option) (str: string) =
        match format with
        | Some f ->
            match Guid.TryParseExact(str, f) with
            | true, v -> Some v
            | false, _ -> None
        | None ->
            match Guid.TryParse str with
            | true, v -> Some v
            | false, _ -> None


    let removeChars (chars: char list) (options: StringSplitOptions option) (str: string) =
        str.Split(chars |> Array.ofList, options |> Option.defaultValue StringSplitOptions.RemoveEmptyEntries)
        |> String.concat ""

    let urlEncode (str: string) = UrlEncoder.Default.Encode str

    let urlDecode (str: string) = HttpUtility.UrlDecode str

    let htmlEncode (str: string) = HttpUtility.HtmlEncode str

    let htmlDecode (str: string) = HttpUtility.HtmlDecode str

    let encode (encodingType: EncodingType) (str: string) =
        match encodingType with
        | EncodingType.Url -> urlEncode str
        | EncodingType.Html -> htmlEncode str
        | EncodingType.None -> str

    let slugify (options: SlugifySettings) (str: string) =
        str
        |> String.filter options.FilterFunction
        |> _.Split(options.CharactersToRemove, StringSplitOptions.RemoveEmptyEntries)
        |> String.concat "_"
        |> fun r ->
            match options.Encoding with
            | EncodingType.Url -> urlEncode r
            | EncodingType.Html -> htmlEncode r
            | EncodingType.None -> r

    let createRandomString (settings: RandomStringSettings) (length: int) =
        let a = Array.zeroCreate length

        let chars = settings.GetCharacters() |> Seq.toArray

        for i in 0 .. (length - 1) do
            a[i] <- Array.randomItem chars

        String(a) |> encode settings.Encoding
