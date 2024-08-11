namespace FsToolbox.AppEnvironment.Args

[<RequireQualifiedAccess>]
module ArgParser =

    open System
    open FsToolbox.AppEnvironment.Args.Mapping
    open FsToolbox.Core
    open Microsoft.FSharp.Reflection

    module Internal =

        type ParameterSignature =
            { MatchValues: string list
              RequiresValue: bool }

        type OptionsSignature =
            { Parameters: ParameterSignature list }

            member os.TryFind(key: string) =
                os.Parameters
                |> List.fold
                    (fun r ps ->
                        match r, ps.MatchValues |> List.contains key with
                        | Some v, _ -> Some v
                        | None, true -> Some ps
                        | None, false -> None)
                    None

        type Arg = { Key: string; Value: string }

        let parseParameters (signature: OptionsSignature) (args: string list) =
            let rec parse (args: string list, result: Arg list, currentKey: string option) =
                match args, currentKey with
                | head :: rest, Some ck -> parse (rest, result @ [ { Key = ck; Value = head } ], None)
                | head :: rest, None ->
                    match signature.TryFind head with
                    | Some ps when ps.RequiresValue -> parse (rest, result, Some head)
                    | Some ps -> parse (rest, result @ [ { Key = head; Value = "true" } ], None)
                    | None ->
                        printfn $"Parameter `{head}` unknown."
                        parse (rest, result, None)
                | [], Some ck -> result @ [ { Key = ck; Value = "true" } ]
                | [], None -> result

            parse (args, [], None)

        let createOptionSignature (mappedRecord: MappedRecord) =
            mappedRecord.Fields
            |> Array.map (fun f ->
                ({ MatchValues = f.MatchValues
                   RequiresValue =
                     String.Equals(f.Type.FullName, TypeHelpers.boolName, StringComparison.Ordinal)
                     |> not }
                : ParameterSignature))
            |> fun r -> ({ Parameters = r |> List.ofArray })

        let createParameter (mo: MappedOption) (args: string list) =
            // For now handle [app name] [command] [options]
            // i.e. Each option in the DU will have a corresponding record (and only one).
            // Command - is the DU
            // Options - is the option record.
            mo.Method.GetParameters()
            |> fun pi ->
                match pi.Length = 1 with
                | true ->
                    let p = pi.[0]

                    match mapRecord p.ParameterType with
                    | Ok mpi ->
                        let os = createOptionSignature mpi
                        let parsedArgs = parseParameters os args

                        let values =
                            mpi.Fields
                            |> Array.sortBy (fun mpi -> mpi.Ordinal)
                            |> Array.map (fun f ->
                                
                                let tryGetFromParsedArgs _ =
                                    parsedArgs
                                    |> List.tryFind (fun pa -> f.MatchValues |> List.contains pa.Key)
                                    |> Option.map (fun a -> a.Value)

                                let tryGetFromEnvironmentalVariable _ =
                                    f.EnvironmentalVariable
                                    |> Option.bind (Environment.GetEnvironmentVariable >> Strings.toOptional)

                                let result =
                                    match f.PreferEnvironmentalVariable with
                                    | true ->
                                        tryGetFromEnvironmentalVariable () |> Option.orElseWith tryGetFromParsedArgs
                                    | false ->
                                        tryGetFromParsedArgs () |> Option.orElseWith tryGetFromEnvironmentalVariable

                                match result with
                                | Some v -> TypeHelpers.createObj f.Type v
                                | None -> TypeHelpers.createDefault f.Type)

                        FSharpValue.MakeRecord(mpi.Type, values)
                    | Error e -> Error e
                | false -> Error "Unsupported."

    let tryGetOptions<'T> (args: string list) =
        args
        |> List.tail // discard the app name.
        |> fun argv ->
            let (cmd, args) = argv.Head, argv.Tail

            match Mapping.getUnionOption<'T> cmd with
            | Ok mo -> Internal.createParameter mo args |> Mapping.createOptions<'T> mo |> Ok
            | Error e -> Error e

    let run<'T> () = tryGetOptions<'T> (Environment.GetCommandLineArgs() |> List.ofArray)
        