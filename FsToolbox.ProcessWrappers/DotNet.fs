namespace FsToolbox.ProcessWrappers

open Microsoft.FSharp.Core

[<RequireQualifiedAccess>]
module DotNet =

    open System
    open FsToolbox.Core
    open FsToolbox.Core.Processes

    [<RequireQualifiedAccess>]
    type BuildType =
        | Project
        | Solution

        member bt.Serialize() =
            match bt with
            | Project -> "PROJECT"
            | Solution -> "SOLUTION"

    /// <summary>
    /// A type representing runtime identifiers.
    /// From https://learn.microsoft.com/en-us/dotnet/core/rid-catalog
    /// </summary>
    [<RequireQualifiedAccess>]
    type RuntimeIdentifier =
        | ``win-x64``
        | ``win-x86``
        | ``win-arm64``
        | ``linux-x64``
        | ``linux-musl-x64``
        | ``linux-musl-arm64``
        | ``linux-arm``
        | ``linux-arm64``
        | ``linux-bionic-arm64``
        | ``osx-x64``
        | ``osx-arm64``
        | Other of string

        member rti.Serialize() =
            match rti with
            | RuntimeIdentifier.``win-x64`` -> "win-x64"
            | RuntimeIdentifier.``win-x86`` -> "win-x86"
            | RuntimeIdentifier.``win-arm64`` -> "win-arm64"
            | RuntimeIdentifier.``linux-x64`` -> "linux-x64"
            | RuntimeIdentifier.``linux-musl-x64`` -> "linux-musl-x64"
            | RuntimeIdentifier.``linux-musl-arm64`` -> "linux-musl-arm64"
            | RuntimeIdentifier.``linux-arm`` -> "linux-arm"
            | RuntimeIdentifier.``linux-arm64`` -> "linux-arm64"
            | RuntimeIdentifier.``linux-bionic-arm64`` -> "linux-bionic-arm64"
            | RuntimeIdentifier.``osx-x64`` -> "osx-x64"
            | RuntimeIdentifier.``osx-arm64`` -> "osx-arm64"
            | Other value -> wrapString value

    [<RequireQualifiedAccess>]
    type ConfigurationType =
        | Debug
        | Release
        | Other of string

        member ct.Serialize() =
            match ct with
            | Debug -> "Debug"
            | Release -> "Release"
            | Other s -> wrapString s

    [<RequireQualifiedAccess>]
    type TerminalLogger =
        | Auto
        | On
        | Off

        member tl.Serialize() =
            match tl with
            | Auto -> "auto"
            | On -> "on"
            | Off -> "off"

    [<RequireQualifiedAccess>]
    type Verbosity =
        | Quiet
        | Minimal
        | Normal
        | Detailed
        | Diagnostic

        member v.Serialize() =
            match v with
            | Quiet -> "quiet"
            | Minimal -> "minimal"
            | Normal -> "normal"
            | Detailed -> "detailed"
            | Diagnostic -> "diagnostic"

    type BuildSettings =
        { BuildType: BuildType option
          Architecture: string option
          ArtifactsPath: string option
          Configuration: ConfigurationType option
          DisableBuildServers: bool option
          Framework: string option
          Force: bool option
          Interactive: bool option
          NoDependencies: bool option
          NoIncremental: bool option
          NoRestore: bool option
          NoLogo: bool option
          NoSelfContained: bool option
          Output: string option
          OS: string option
          Properties: Map<string, string> option
          RunTime: RuntimeIdentifier option
          SelfContained: bool option
          Source: string option
          TerminalLogger: TerminalLogger option
          Verbosity: Verbosity option
          UserCurrentRunTime: bool option
          VersionSuffix: string option }

        static member Default =
            { BuildType = None
              Architecture = None
              ArtifactsPath = None
              Configuration = failwith "todo"
              DisableBuildServers = failwith "todo"
              Framework = failwith "todo"
              Force = failwith "todo"
              Interactive = failwith "todo"
              NoDependencies = failwith "todo"
              NoIncremental = failwith "todo"
              NoRestore = failwith "todo"
              NoLogo = failwith "todo"
              NoSelfContained = failwith "todo"
              Output = failwith "todo"
              OS = failwith "todo"
              Properties = failwith "todo"
              RunTime = failwith "todo"
              SelfContained = failwith "todo"
              Source = failwith "todo"
              TerminalLogger = failwith "todo"
              Verbosity = failwith "todo"
              UserCurrentRunTime = failwith "todo"
              VersionSuffix = failwith "todo" }

        member settings.CreateArgs() =
            [ settings.BuildType |> Option.map (fun bt -> bt.Serialize())
              settings.Architecture |> Option.map (fun a -> $"--arch {wrapString a}")
              settings.ArtifactsPath
              |> Option.map (fun ap -> $"--artifacts-path {wrapString ap}")
              settings.Configuration
              |> Option.map (fun c -> $"--configuration {c.Serialize()}")
              settings.DisableBuildServers |> Option.ifTrue "--disable-build-servers"
              settings.Framework |> Option.map (fun f -> $"--framework {wrapString f}")
              settings.Force |> Option.ifTrue "--force"
              settings.Interactive |> Option.ifTrue "--interactive"
              settings.NoDependencies |> Option.ifTrue "--no-dependencies"
              settings.NoIncremental |> Option.ifTrue "--no-incremental"
              settings.NoRestore |> Option.ifTrue "--no-restore"
              settings.NoLogo |> Option.ifTrue "--nologo"
              settings.NoSelfContained |> Option.ifTrue "--no-self-contained"
              settings.Output |> Option.map (fun o -> $"--output {wrapString o}")
              settings.OS |> Option.map (fun os -> $"--os {wrapString os}")
              settings.RunTime |> Option.map (fun rt -> $"--runtime {rt}")

              yield!
                  settings.Properties
                  |> Option.map (fun properties ->
                      properties
                      |> Map.toList
                      |> List.map (fun (k, v) -> Some $"--property:{wrapString k}={wrapString v}"))
                  |> Option.defaultValue []

              ]
            |> List.choose id
            |> concatStrings " "

    type PublishSettings =
        { SourcePath: string option

        }

(*
    let publish (dotnetPath: string) name =

        let args =
            [ "publish"
              createSourcePath name context
              "--configuration Release"
              $"--output {Path.Combine(getPublishPath context, name)}"
              // TODO - Add type (such as linux-x64) and version etc(?)
              "-p:UseAppHost=false"
              $"/p:VersionPrefix={getVersion context}"
              match getVersionSuffix context with
              | Some v -> $"/p:VersionSuffix={v}"
              | None -> ""
              $"/p:InformationalVersion={getBuildName context}" ]
            |> (fun a -> String.Join(' ', a))
        //let args =

        //    "publish --configuration Release --output {output} /p:VersionPrefix={}.{}.{}"

        Process.execute
        let output, errors = Process.execute dotnetPath args None //(getSrcPath context)

        match errors.Length = 0 with
        | true -> Ok output.Head
        | false -> Error(String.Join(Environment.NewLine, errors))

    /// Run dotnet test and return the past to the results file.
    let test dotnetPath testName =
        context.Log("dot-net-test", "Running tests.")
        // dotnet test --logger "trx;logfilename=mytests.xml" -r C:\TestResults\FDOM\
        let args =
            [ "test"
              createSourcePath testName context
              $"--logger \"trx;logfilename={testName}.xml\""
              $"-r {getTestsPath context}" ]
            |> (fun a -> String.Join(' ', a))
        //let args =
        //    "publish --configuration Release --output {output} /p:VersionPrefix={}.{}.{}"

        let output, errors = Process.execute dotnetPath args None //path

        match errors.Length = 0 with
        | true ->
            output
            |> List.map (fun o -> context.Log("dot-net-test", o))
            |> ignore

            context.Log("dot-net-test", "Tests complete.")
            Ok(Path.Combine(getTestsPath context, $"{testName}.xml"))
        | false ->
            errors
            |> List.map (fun e -> context.LogError("dot-net-test", e))
            |> ignore

            let errorMessage = String.Join(Environment.NewLine, errors)
            context.LogError("dot-net-test", $"Tests failed. Error: {errorMessage}")
            Error(String.Join(Environment.NewLine, errors))

    /// Run dotnet test and return the past to the results file.

    let pack (dotnetPath: string) name =

        let args =
            [ "pack"
              createSourcePath name context
              "--configuration Release"
              $"--output {getPackagePath context}"
              $"/p:VersionPrefix={getVersion context}"
              match getVersionSuffix context with
              | Some v -> $"/p:VersionSuffix={v}"
              | None -> ""
              $"/p:InformationalVersion={getBuildName context}" ]
            |> (fun a -> String.Join(' ', a))

        let output, errors = Process.execute dotnetPath args None //(getSrcPath context)

        match errors.Length = 0 with
        | true -> Ok output.Head
        | false -> Error(String.Join(Environment.NewLine, errors))

    let push (dotnetPath: string) name source =

        let args =
            [ "nuget"
              "push"
              createPackagePath name context
              $"--source \"{source}\"" ]
            |> (fun a -> String.Join(' ', a))


        printfn $"******** Running command: {dotnetPath} {args}"

        let output, errors = Process.execute dotnetPath args None //(getSrcPath context)

        match errors.Length = 0 with
        | true -> Ok output.Head
        | false -> Error(String.Join(Environment.NewLine, errors))
*)
