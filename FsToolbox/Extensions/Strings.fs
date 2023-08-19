namespace FsToolbox.Extensions

open System.Globalization
open System.Security.Cryptography

[<AutoOpen>]
module Strings =

    open System
    open System.Text
    open FsToolbox.Core

    type String with

        static member FromUtfBytes(bytes: byte array) = Conversions.toUtf8 bytes

        member str.ToSnakeCase() =
            str
            |> List.ofSeq
            |> List.fold
                (fun (acc, i) c ->
                    let newAcc =
                        match Char.IsUpper c, i = 0 with
                        | false, _ -> acc @ [ c ]
                        | true, true -> acc @ [ Char.ToLower(c) ]
                        | true, false -> acc @ [ '_'; Char.ToLower(c) ]

                    (newAcc, i + 1))
                ([], 0)
            |> (fun (chars, _) -> String(chars |> Array.ofList))

        member str.ToPascalCase() =
            let isMatch c = [ '_'; '-'; ' ' ] |> List.contains c

            str
            |> List.ofSeq
            |> List.fold
                (fun (acc, i) c ->
                    let newAcc =
                        //match c =
                        match i - 1 >= 0 && isMatch str.[i - 1], i = 0, isMatch c with
                        | true, _, false -> acc @ [ Char.ToUpper c ]
                        | true, _, true -> acc
                        | false, false, false -> acc @ [ c ]
                        | false, true, _ -> acc @ [ Char.ToUpper c ]
                        | false, false, true -> acc

                    //match Char.IsUpper c, i = 0 with
                    //| false, _ -> acc @ [ c ]
                    //| true, true -> acc @ [ Char.ToLower(c) ]
                    //| true, false -> acc @ [ '_'; Char.ToLower(c) ]
                    (newAcc, i + 1))
                ([], 0)
            |> (fun (chars, _) -> String(chars |> Array.ofList))

        member str.ToCamelCase() =
            let isMatch c = [ '_'; '-'; ' ' ] |> List.contains c

            str
            |> List.ofSeq
            |> List.fold
                (fun (acc, i) c ->
                    let newAcc =
                        //match c =
                        match i - 1 >= 0 && isMatch str.[i - 1], i = 0, isMatch c with
                        | true, _, false -> acc @ [ Char.ToUpper c ]
                        | true, _, true -> acc
                        | false, false, false -> acc @ [ c ]
                        | false, true, _ -> acc @ [ Char.ToUpper c ]
                        | false, false, true -> acc

                    //match Char.IsUpper c, i = 0 with
                    //| false, _ -> acc @ [ c ]
                    //| true, true -> acc @ [ Char.ToLower(c) ]
                    //| true, false -> acc @ [ '_'; Char.ToLower(c) ]
                    (newAcc, i + 1))
                ([], 0)
            |> (fun (chars, _) ->
                match chars.Length > 0 with
                | true -> String([ Char.ToLower chars.Head ] @ chars.Tail |> Array.ofList)
                | false -> "")

        member str.ToUtf8Bytes() = Conversions.fromUtf8 str

        member str.GetSHA256Hash() =
            str.ToUtf8Bytes() |> Hashing.generateHash (SHA256.Create())

        member str.IsNullOrEmpty() = String.IsNullOrEmpty str

        member str.IsNotNullOrEmpty() = str.IsNullOrEmpty() |> not

        member str.IsNullOrWhiteSpace() = String.IsNullOrWhiteSpace str

        member str.IsNotNullOrWhiteSpace() = str.IsNullOrWhiteSpace() |> not

        member str.ToOption(?emptyToNone: bool, ?whitespaceToNone: bool) =
            match emptyToNone |> Option.defaultValue true, whitespaceToNone |> Option.defaultValue true with
            | true, true
            | false, true ->
                match str.IsNotNullOrWhiteSpace() with
                | true -> Some str
                | false -> None
            | true, false ->
                match str.IsNotNullOrEmpty() with
                | true -> Some str
                | false -> None
            | false, false ->
                match str <> null with
                | true -> Some str
                | false -> None

        member str.TryToByte() = Strings.tryToByte

        member str.TryToBool(?additionalTrueValues: string list, ?additionFalseValues: string list) =
            Strings.tryToBool
                (additionalTrueValues |> Option.defaultValue [])
                (additionFalseValues |> Option.defaultValue [])
                str

        member str.TryToDouble() = Strings.tryToDouble str

        member str.TryToSingle() = Strings.tryToSingle str

        member str.TryToInt16() = Strings.tryToInt16 str

        member str.TryToInt32() = Strings.tryToInt32 str

        member str.TryToInt64() = Strings.tryToInt64 str

        member str.TryToUInt16() = Strings.tryToUInt16 str

        member str.TryToUInt32() = Strings.tryToUInt32 str

        member str.TryToUInt64() = Strings.tryToUInt64 str

        member str.TryToDateTime(format: string, ?provider: IFormatProvider, ?styles: DateTimeStyles) =
            Strings.tryToFormattedDateTime format provider styles str

        member str.TryToDateTime() = Strings.tryToDateTime str

        member str.RemoveChars(chars: char list, ?options: StringSplitOptions) = Strings.removeChars chars options str

        member str.ReplaceMultiple(replacements: (string * string) list) =
                replacements |> List.fold (fun (s:string) -> s.Replace) str