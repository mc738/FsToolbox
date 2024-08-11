namespace FsToolbox.AppEnvironment.Args

module Mapping =


    open System
    open System.Reflection
    open Microsoft.FSharp.Reflection

    type CommandValue(name: string) =

        inherit Attribute()

        member att.Name = name
        
    type ArgValueAttribute(shortName: string, longName: string, ?environmentalVariableName: string, ?preferEnvironmentalVariable: bool) =

        inherit Attribute()

        member att.MatchValues = [ shortName; longName ]
      
        member att.EnvironmentVariableName = environmentalVariableName
        
        member att.PreferEnvironmentalVariable = preferEnvironmentalVariable |> Option.defaultValue false

    type MappedOption =
        {
          //Args: string list
          UnionCase: UnionCaseInfo
          Method: MethodInfo }

    type MappedRecord =
        { Name: string
          Type: Type
          Fields: Field array }

    and Field =
        { Name: string
          Ordinal: int
          Type: Type
          MatchValues: string list
          EnvironmentalVariable: string option
          PreferEnvironmentalVariable: bool }

    let getCommandName (uci: UnionCaseInfo) =
        uci.GetCustomAttributes(typeof<CommandValue>)
        |> Array.tryHead
        |> Option.map (fun att -> (att :?> CommandValue).Name)
        |> Option.defaultWith (fun _ -> uci.Name)

    let getUnionOption<'T> (cmd: string) =
        let t = typeof<'T>

        match FSharpType.IsUnion(t) with
        | true ->
            FSharpType.GetUnionCases(t)
            |> Array.tryFind (fun uc -> String.Equals(getCommandName uc, cmd, StringComparison.OrdinalIgnoreCase))
            |> Option.bind (fun uc ->
                // Get the New[Option] method.
                // From here the Record type for that option can be retrieved.
                try

                    { UnionCase = uc
                      Method = t.GetMethod($"New{uc.Name}") }
                    |> Ok
                with _ ->
                    Error "Method not found."
                |> Some)
            |> Option.defaultValue (Error "Type not found.")
        | false -> Error "Not union type."

    let createOptions<'T> (mappedOption: MappedOption) (parameter: obj) =
        //let uci =with
        FSharpValue.MakeUnion(mappedOption.UnionCase, [| parameter |]) :?> 'T

    let mapRecord (recordType: Type) =
        match FSharpType.IsRecord recordType with
        | true ->
            recordType.GetProperties()
            |> Array.mapi (fun i pi ->
                match Attribute.GetCustomAttribute(pi, typeof<ArgValueAttribute>) with
                | att when att <> null ->
                    let ava = att :?> ArgValueAttribute

                    ({ Name = pi.Name
                       Ordinal = i
                       Type = pi.PropertyType
                       MatchValues = ava.MatchValues
                       EnvironmentalVariable = ava.EnvironmentVariableName
                       PreferEnvironmentalVariable = ava.PreferEnvironmentalVariable }
                    : Field)
                | _ ->
                    ({ Name = pi.Name
                       Ordinal = i
                       Type = pi.PropertyType
                       MatchValues = [ $"--{pi.Name.ToLower()}" ]
                       EnvironmentalVariable = None
                       PreferEnvironmentalVariable = false }
                    : Field))
            |> fun f ->
                { Name = recordType.Name
                  Type = recordType
                  Fields = f }
                |> Ok
        | false -> Error "Not a record type."
