namespace FsToolbox.Core

open System
open System.Text.RegularExpressions

module TypeHelpers =

    let getName<'T> = typeof<'T>.FullName

    let typeName (t: Type) = t.FullName

    let boolName = getName<bool>

    let uByteName = getName<uint8>

    let uShortName = getName<uint16>

    let uIntName = getName<uint32>

    let uLongName = getName<uint64>

    let byteName = getName<byte>

    let shortName = getName<int16>

    let intName = getName<int>

    let longName = getName<int64>

    let floatName = getName<float>

    let doubleName = getName<double>

    let decimalName = getName<decimal>

    let charName = getName<char>

    let timestampName = getName<DateTime>

    let uuidName = getName<Guid>

    let stringName = getName<string>


    let isOption (value: string) =
        Regex
            .Match(value, "(?<=Microsoft.FSharp.Core.FSharpOption`1\[\[).+?(?=\,)")
            .Success

    let getOptionType value =
        // Maybe a bit wasteful doing this twice.
        Regex
            .Match(value, "(?<=Microsoft.FSharp.Core.FSharpOption`1\[\[).+?(?=\,)")
            .Value

    /// An internal DU for representing supported types.
    [<RequireQualifiedAccess>]
    type SupportedType =
        | Boolean
        | Byte
        | Char
        | Decimal
        | Double
        | Float
        | Int
        | Short
        | Long
        | String
        | DateTime
        | Guid
        | Option of SupportedType
        //| Json of Type

        static member TryFromName(name: String) =
            match name with
            | t when t = boolName -> Ok SupportedType.Boolean
            | t when t = charName -> Ok SupportedType.Char
            | t when t = byteName -> Ok SupportedType.Byte
            | t when t = decimalName -> Ok SupportedType.Decimal
            | t when t = doubleName -> Ok SupportedType.Double
            | t when t = floatName -> Ok SupportedType.Float
            | t when t = intName -> Ok SupportedType.Int
            | t when t = shortName -> Ok SupportedType.Short
            | t when t = longName -> Ok SupportedType.Long
            | t when t = stringName -> Ok SupportedType.String
            | t when t = timestampName -> Ok SupportedType.DateTime
            | t when t = uuidName -> Ok SupportedType.Guid
            | t when isOption t = true ->
                let ot = getOptionType t

                match SupportedType.TryFromName ot with
                | Ok st -> Ok(SupportedType.Option st)
                | Error e -> Error e
            | _ -> Error $"Type `{name}` not supported."

        static member TryFromType(typeInfo: Type) =
            SupportedType.TryFromName(typeInfo.FullName)

        static member FromName(name: string) =
            match SupportedType.TryFromName name with
            | Ok st -> st
            | Error _ -> SupportedType.String

        static member FromType(typeInfo: Type) =
            SupportedType.FromName(typeInfo.FullName)

    //member

    let createObj (t: Type) (value: string) =
        match SupportedType.TryFromType t with
        | Ok st ->
            match st with
            | SupportedType.Boolean ->
                match Boolean.TryParse value with
                | true, v -> box v
                | false, _ -> failwith $"Failed to parse `{value}` as bool."
            | SupportedType.Byte ->
                match Byte.TryParse value with
                | true, v -> box v
                | false, _ -> failwith $"Failed to parse `{value}` as byte."
            | SupportedType.Char ->
                match value.Length > 0 with
                | true -> box value.[0]
                | false -> box '\n' //match Boolean.TryParse value with | true, v -> box v | false, _ -> failwith $"Failed to parse `{value}` as bool."
            | SupportedType.Decimal ->
                match Decimal.TryParse value with
                | true, v -> box v
                | false, _ -> failwith $"Failed to parse `{value}` as decimal."
            | SupportedType.Double ->
                match Double.TryParse value with
                | true, v -> box v
                | false, _ -> failwith $"Failed to parse `{value}` as double."
            | SupportedType.Float ->
                match Double.TryParse value with
                | true, v -> box v
                | false, _ -> failwith $"Failed to parse `{value}` as double."
            | SupportedType.Int ->
                match Int32.TryParse value with
                | true, v -> box v
                | false, _ -> failwith $"Failed to parse `{value}` as int32."
            | SupportedType.Short ->
                match Int16.TryParse value with
                | true, v -> box v
                | false, _ -> failwith $"Failed to parse `{value}` as int16."
            | SupportedType.Long ->
                match Int64.TryParse value with
                | true, v -> box v
                | false, _ -> failwith $"Failed to parse `{value}` as int64."
            | SupportedType.String -> box value
            | SupportedType.DateTime ->
                match DateTime.TryParse value with
                | true, v -> box v
                | false, _ -> failwith $"Failed to parse `{value}` as datetime."
            | SupportedType.Guid ->
                match Guid.TryParse value with
                | true, v -> box v
                | false, _ -> failwith $"Failed to parse `{value}` as guid."
            | SupportedType.Option supportedType ->
                match supportedType with
                | SupportedType.Boolean ->
                    match Boolean.TryParse value with
                    | true, v -> box (Some v)
                    | false, _ -> failwith $"Failed to parse `{value}` as bool."
                | SupportedType.Byte ->
                    match Byte.TryParse value with
                    | true, v -> box (Some v)
                    | false, _ -> failwith $"Failed to parse `{value}` as byte."
                | SupportedType.Char ->
                    match value.Length > 0 with
                    | true -> box (Some value.[0])
                    | false -> box '\n' //match Boolean.TryParse value with | true, v -> box v | false, _ -> failwith $"Failed to parse `{value}` as bool."
                | SupportedType.Decimal ->
                    match Decimal.TryParse value with
                    | true, v -> box (Some v)
                    | false, _ -> failwith $"Failed to parse `{value}` as decimal."
                | SupportedType.Double ->
                    match Double.TryParse value with
                    | true, v -> box (Some v)
                    | false, _ -> failwith $"Failed to parse `{value}` as double."
                | SupportedType.Float ->
                    match Double.TryParse value with
                    | true, v -> box (Some v)
                    | false, _ -> failwith $"Failed to parse `{value}` as double."
                | SupportedType.Int ->
                    match Int32.TryParse value with
                    | true, v -> box (Some v)
                    | false, _ -> failwith $"Failed to parse `{value}` as int32."
                | SupportedType.Short ->
                    match Int16.TryParse value with
                    | true, v -> box (Some v)
                    | false, _ -> failwith $"Failed to parse `{value}` as int16."
                | SupportedType.Long ->
                    match Int64.TryParse value with
                    | true, v -> box (Some v)
                    | false, _ -> failwith $"Failed to parse `{value}` as int64."
                | SupportedType.String -> box (Some value)
                | SupportedType.DateTime ->
                    match DateTime.TryParse value with
                    | true, v -> box (Some v)
                    | false, _ -> failwith $"Failed to parse `{value}` as datetime."
                | SupportedType.Guid ->
                    match Guid.TryParse value with
                    | true, v -> box (Some v)
                    | false, _ -> failwith $"Failed to parse `{value}` as guid."
                | SupportedType.Option _ -> failwith "Nest option types not supported."
        | Error e -> failwith $"Error: {e}"

    let createDefault (t: Type) =
        match SupportedType.TryFromType t with
        | Ok st ->
            match st with
            | SupportedType.Boolean -> box false
            | SupportedType.Byte -> box 0uy
            | SupportedType.Char -> box '\n'
            | SupportedType.Decimal -> box 0m
            | SupportedType.Double -> box 0f
            | SupportedType.Float -> box 0f
            | SupportedType.Int -> box 0
            | SupportedType.Short -> box 0s
            | SupportedType.Long -> box 0L
            | SupportedType.String -> box String.Empty
            | SupportedType.DateTime -> box (DateTime())
            | SupportedType.Guid -> box (Guid.NewGuid())
            | SupportedType.Option _ -> box None
        | Error e -> failwith $"Error: {e}"
