namespace FsToolbox.ProcessWrappers

open Microsoft.FSharp.Core

[<RequireQualifiedAccess>]
module DotNet =

    open System
    open FsToolbox.Core
    open FsToolbox.Core.Processes

    //[<RequireQualifiedAccess>]
    //type BuildType =
    //    | Project
    //    | Solution
    //
    //    member bt.Serialize() =
    //        match bt with
    //        | Project -> "PROJECT"
    //        | Solution -> "SOLUTION"
    //
    //[<RequireQualifiedAccess>]
    //type TestType =
    //    | Project
    //    | Solution
    //    | Directory
    //    | Dll
    //    | Exe of string
    //
    //    member bt.Serialize() =
    //        match bt with
    //        | Project -> "PROJECT"
    //        | Solution -> "SOLUTION"



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
        { Path: string option
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
            { Path = None
              Architecture = None
              ArtifactsPath = None
              Configuration = None
              DisableBuildServers = None
              Framework = None
              Force = None
              Interactive = None
              NoDependencies = None
              NoIncremental = None
              NoRestore = None
              NoLogo = None
              NoSelfContained = None
              Output = None
              OS = None
              Properties = None
              RunTime = None
              SelfContained = None
              Source = None
              TerminalLogger = None
              Verbosity = None
              UserCurrentRunTime = None
              VersionSuffix = None }

        member settings.CreateArgs() =
            [ Some "build"
              settings.Path |> Option.map wrapString
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
              settings.RunTime |> Option.map (fun rt -> $"--runtime {rt.Serialize()}")
              settings.SelfContained
              |> Option.map (fun sc ->
                  match sc with
                  | true -> "--self-contained true"
                  | false -> "--self-contained false")

              settings.Source |> Option.map (fun s -> $"--source {wrapString s}")
              settings.TerminalLogger |> Option.map (fun tl -> $"--tl:{tl.Serialize()}")
              settings.Verbosity |> Option.map (fun v -> $"--verbosity {v.Serialize()}")
              settings.UserCurrentRunTime
              |> Option.map (fun ucr ->
                  match ucr with
                  | true -> "--use-current-runtime true"
                  | false -> "--use-current-runtime false")

              settings.VersionSuffix
              |> Option.map (fun vs -> $"--version-suffix {wrapString vs}")

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

    type PackSettings =
        { Path: string option
          ArtifactsPath: string option
          Configuration: ConfigurationType option
          Force: bool option
          IncludeSource: bool option
          IncludeSymbols: bool option
          Interactive: bool option
          NoBuild: bool option
          NoDependencies: bool option
          NoRestore: bool option
          NoLogo: bool option
          Output: string option
          Properties: Map<string, string> option
          RunTime: RuntimeIdentifier option
          Serviceable: bool option
          TerminalLogger: TerminalLogger option
          Verbosity: Verbosity option
          VersionSuffix: string option }

        static member Default =
            { Path = None
              ArtifactsPath = None
              Configuration = None
              Force = None
              IncludeSource = None
              IncludeSymbols = None
              Interactive = None
              NoBuild = None
              NoDependencies = None
              NoRestore = None
              NoLogo = None
              Output = None
              Properties = None
              RunTime = None
              Serviceable = None
              TerminalLogger = None
              Verbosity = None
              VersionSuffix = None }

        member settings.CreateArgs() =
            [ Some "pack"
              settings.Path |> Option.map wrapString
              settings.ArtifactsPath
              |> Option.map (fun ap -> $"--artifacts-path {wrapString ap}")
              settings.Configuration
              |> Option.map (fun c -> $"--configuration {c.Serialize()}")
              settings.Force |> Option.ifTrue "--force"
              settings.Interactive |> Option.ifTrue "--interactive"
              settings.NoBuild |> Option.ifTrue "--no-build"
              settings.NoDependencies |> Option.ifTrue "--no-dependencies"
              settings.NoRestore |> Option.ifTrue "--no-restore"
              settings.NoLogo |> Option.ifTrue "--nologo"
              settings.Output |> Option.map (fun o -> $"--output {wrapString o}")
              settings.RunTime |> Option.map (fun rt -> $"--runtime {rt.Serialize()}")
              settings.Serviceable |> Option.ifTrue "--serviceable"
              settings.TerminalLogger |> Option.map (fun tl -> $"--tl:{tl.Serialize()}")
              settings.Verbosity |> Option.map (fun v -> $"--verbosity {v.Serialize()}")
              settings.VersionSuffix
              |> Option.map (fun vs -> $"--version-suffix {wrapString vs}")

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
        { Path: string option
          Architecture: string option
          ArtifactsPath: string option
          Configuration: ConfigurationType option
          DisableBuildServers: bool option
          Framework: string option
          Force: bool option
          Interactive: bool option
          Manifest: string option
          NoBuild: bool option
          NoDependencies: bool option
          NoRestore: bool option
          NoSelfContained: bool option
          NoLogo: bool option
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
            { Path = None
              Architecture = None
              ArtifactsPath = None
              Configuration = None
              DisableBuildServers = None
              Framework = None
              Force = None
              Interactive = None
              Manifest = None
              NoBuild = None
              NoDependencies = None
              NoRestore = None
              NoLogo = None
              NoSelfContained = None
              Output = None
              OS = None
              Properties = None
              RunTime = None
              SelfContained = None
              Source = None
              TerminalLogger = None
              Verbosity = None
              UserCurrentRunTime = None
              VersionSuffix = None }

        member settings.CreateArgs() =
            [ Some "publish"
              settings.Path |> Option.map wrapString
              settings.Architecture |> Option.map (fun a -> $"--arch {wrapString a}")
              settings.ArtifactsPath
              |> Option.map (fun ap -> $"--artifacts-path {wrapString ap}")
              settings.Configuration
              |> Option.map (fun c -> $"--configuration {c.Serialize()}")
              settings.DisableBuildServers |> Option.ifTrue "--disable-build-servers"
              settings.Framework |> Option.map (fun f -> $"--framework {wrapString f}")
              settings.Force |> Option.ifTrue "--force"
              settings.Interactive |> Option.ifTrue "--interactive"
              settings.NoBuild |> Option.ifTrue "--no-build"
              settings.NoDependencies |> Option.ifTrue "--no-dependencies"
              settings.NoRestore |> Option.ifTrue "--no-restore"
              settings.NoLogo |> Option.ifTrue "--nologo"
              settings.NoSelfContained |> Option.ifTrue "--no-self-contained"
              settings.Output |> Option.map (fun o -> $"--output {wrapString o}")
              settings.OS |> Option.map (fun os -> $"--os {wrapString os}")
              settings.RunTime |> Option.map (fun rt -> $"--runtime {rt.Serialize()}")
              settings.SelfContained
              |> Option.map (fun sc ->
                  match sc with
                  | true -> "--self-contained true"
                  | false -> "--self-contained false")

              settings.Source |> Option.map (fun s -> $"--source {wrapString s}")
              settings.TerminalLogger |> Option.map (fun tl -> $"--tl:{tl.Serialize()}")
              settings.Verbosity |> Option.map (fun v -> $"--verbosity {v.Serialize()}")
              settings.UserCurrentRunTime
              |> Option.map (fun ucr ->
                  match ucr with
                  | true -> "--use-current-runtime true"
                  | false -> "--use-current-runtime false")

              settings.VersionSuffix
              |> Option.map (fun vs -> $"--version-suffix {wrapString vs}")

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

    type TestSettings =
        { Path: string option
          TestAdapterPath: string option
          Architecture: string option
          ArtifactsPath: string option
          Blame: bool option
          BlameCrash: bool option
          BlameCrashDumpType: string option
          BlameCrashCollectAlways: bool option
          BlameHang: bool option
          BlameHangDumpType: string option
          BlameHangTimeout: string option
          Configuration: ConfigurationType option
          Collect: string option
          Diagnostic: string option
          Framework: string option
          EnvironmentalVariables: Map<string, string> option
          Filter: string option
          Interactive: bool option
          Logger: string option
          NoBuild: bool option
          NoLogo: bool option
          NoRestore: bool option
          Output: string option
          OS: string option
          ResultsDirectory: string option
          RunTime: RuntimeIdentifier option
          Settings: string option
          ListTests: bool option
          Verbosity: Verbosity option
          Args: string list option
          RunSettings: string option }

        static member Default =
            { Path = None
              TestAdapterPath = None
              Architecture = None
              ArtifactsPath = None
              Blame = None
              BlameCrash = None
              BlameCrashDumpType = None
              BlameCrashCollectAlways = None
              BlameHang = None
              BlameHangDumpType = None
              BlameHangTimeout = None
              Configuration = None
              Collect = None
              Diagnostic = None
              Framework = None
              EnvironmentalVariables = None
              Filter = None
              Interactive = None
              Logger = None
              NoBuild = None
              NoLogo = None
              NoRestore = None
              Output = None
              OS = None
              ResultsDirectory = None
              RunTime = None
              Settings = None
              ListTests = None
              Verbosity = None
              Args = None
              RunSettings = None }

        member settings.CreateArgs() =
            [ Some "test"
              settings.Path |> Option.map wrapString
              settings.Architecture |> Option.map (fun a -> $"--arch {wrapString a}")
              settings.ArtifactsPath
              |> Option.map (fun ap -> $"--artifacts-path {wrapString ap}")
              settings.Blame |> Option.ifTrue "--blame"
              settings.BlameCrash |> Option.ifTrue "--blame-crash"
              settings.BlameCrashDumpType
              |> Option.map (fun bcd -> $"--blame-crash-dump-type {wrapString bcd}")
              settings.BlameCrashCollectAlways |> Option.ifTrue "--blame-crash-collect-always"
              settings.BlameHang |> Option.ifTrue "--blame-hang"
              settings.BlameHangDumpType
              |> Option.map (fun bhd -> $"--blame-hang-dump-type {bhd}")
              settings.BlameHangTimeout
              |> Option.map (fun bht -> $"--blame-hang-timeout {bht}")
              settings.Configuration
              |> Option.map (fun c -> $"--configuration {c.Serialize()}")
              settings.Collect |> Option.map (fun c -> $"--collect {wrapString c}")
              settings.Diagnostic |> Option.map (fun d -> $"--diag {wrapString d}")
              settings.Framework |> Option.map (fun f -> $"--framework {wrapString f}")

              yield!
                  settings.EnvironmentalVariables
                  |> Option.map (fun evs ->
                      evs
                      |> Map.toList
                      |> List.map (fun (k, v) -> Some $"--environment {wrapString k}={wrapString v}"))
                  |> Option.defaultValue []

              settings.Filter |> Option.map (fun f -> $"--filter {wrapString f}")
              settings.Interactive |> Option.ifTrue "--interactive"
              settings.Logger |> Option.map (fun l -> $"--logger {wrapString l}")
              settings.NoBuild |> Option.ifTrue "--no-build"
              settings.NoLogo |> Option.ifTrue "--nologo"
              settings.NoRestore |> Option.ifTrue "--no-restore"
              settings.Output |> Option.map (fun o -> $"--output {wrapString o}")
              settings.OS |> Option.map (fun os -> $"--os {wrapString os}")
              settings.ResultsDirectory
              |> Option.map (fun rd -> $"--results-directory {wrapString rd}")
              settings.RunTime |> Option.map (fun rt -> $"--runtime {rt.Serialize()}")
              settings.Settings |> Option.map (fun s -> $"--settings {wrapString s}")
              settings.ListTests |> Option.ifTrue "--list-tests"
              settings.Verbosity |> Option.map (fun v -> $"--verbosity {v.Serialize()}")
              yield!
                  settings.Args
                  |> Option.map (fun s -> s |> List.map (wrapString >> Some))
                  |> Option.defaultValue []

              match settings.RunSettings with
              | Some rs ->
                  Some "--"
                  Some rs
              | None -> () ]
            |> List.choose id
            |> concatStrings " "

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
