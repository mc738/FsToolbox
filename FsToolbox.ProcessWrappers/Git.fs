namespace FsToolbox.ProcessWrappers

[<RequireQualifiedAccess>]
module Git =

    open System
    open FsToolbox.Core.Processes
    open FsToolbox.ProcessWrappers.Common
    open FsToolbox.Core
    open FsToolbox.Core.Results
    open FsToolbox.Extensions.Strings

    type CloneSettings =
        { Repository: string
          Directory: string option
          Local: bool option
          NoHardLinks: bool option
          Shared: bool option
          Reference: string option
          Dissociate: bool option
          Quiet: bool option
          Verbose: bool option
          Progress: bool option
          ServerOption: string option
          NoCheckout: bool option
          RejectShallow: bool option
          Bare: bool option
          Sparse: bool option
          Filter: string option
          AlsoFilterSubModules: bool option
          Mirror: bool option
          Origin: string option
          Branch: string option
          UnloadPack: string option
          Template: string option
          ConfigurationVariables: Map<string, string> option
          Depth: int option
          ShallowSince: string option
          ShallowExclude: string option
          SingleBranch: bool option
          NoTags: bool option
          RecurseSubmodules: string list option
          ShallowModules: bool option
          RemoteSubmodules: bool option
          SeparateGitDirectory: string option
          RefFormat: string option
          Jobs: int option
          BundleUri: string option }

        static member Default =
            { Repository = ""
              Directory = None
              Local = None
              NoHardLinks = None
              Shared = None
              Reference = None
              Dissociate = None
              Quiet = None
              Verbose = None
              Progress = None
              ServerOption = None
              NoCheckout = None
              RejectShallow = None
              Bare = None
              Sparse = None
              Filter = None
              AlsoFilterSubModules = None
              Mirror = None
              Origin = None
              Branch = None
              UnloadPack = None
              Template = None
              ConfigurationVariables = None
              Depth = None
              ShallowSince = None
              ShallowExclude = None
              SingleBranch = None
              NoTags = None
              RecurseSubmodules = None
              ShallowModules = None
              RemoteSubmodules = None
              SeparateGitDirectory = None
              RefFormat = None
              Jobs = None
              BundleUri = None }

        member gc.CreateArgs() =
            [ Some "clone"
              gc.Template |> Option.map (fun td -> $"--template={wrapString td}")
              gc.Local |> Option.ifTrue "--local"
              gc.NoHardLinks |> Option.ifTrue "--no-hardlinks"
              gc.Shared |> Option.ifTrue "--shared"
              gc.Reference |> Option.map (fun r -> $"--reference {wrapString r}")
              gc.Dissociate |> Option.ifTrue "--dissociate"
              gc.Quiet |> Option.ifTrue "--quiet"
              gc.Verbose |> Option.ifTrue "--verbose"
              gc.Progress |> Option.ifTrue "--progress"
              gc.ServerOption |> Option.map (fun so -> $"--server-option={wrapString so}")
              gc.NoCheckout |> Option.ifTrue "--no-checkout"
              gc.RejectShallow
              |> Option.map (fun v ->
                  match v with
                  | true -> "--reject-shallow"
                  | false -> "--no-reject-shallow")
              gc.Bare |> Option.ifTrue "--bare"
              gc.Sparse |> Option.ifTrue "--sparse"
              gc.Filter |> Option.map (fun fs -> $"--filter={wrapString fs}")
              gc.AlsoFilterSubModules |> Option.ifTrue "--also-filter-submodules"
              gc.Mirror |> Option.ifTrue "--mirror"
              gc.Origin |> Option.map (fun o -> $"--origin {wrapString o}")
              gc.Branch |> Option.map (fun b -> $"--branch {wrapString b}")
              gc.UnloadPack |> Option.map (fun up -> $"--upload-pack {wrapString up}")
              gc.Template |> Option.map (fun t -> $"--template={wrapString t}")
              yield!
                  gc.ConfigurationVariables
                  |> Option.map (fun m ->
                      m
                      |> Map.toList
                      |> List.map (fun (k, v) -> Some $"--config {wrapString k}={wrapString v}"))
                  |> Option.defaultValue []
              gc.Depth |> Option.map (fun d -> $"--depth {d}")
              gc.ShallowSince
              |> Option.map (fun ss -> $"--shallow-since={forceWrapString ss}")
              gc.ShallowExclude |> Option.map (fun se -> $"--shallow-exclude={wrapString se}")
              gc.SingleBranch
              |> Option.map (fun sb -> if sb then "--single-branch" else "--no-single-branch")
              gc.NoTags |> Option.ifTrue "--no-tags"
              yield!
                  gc.RecurseSubmodules
                  |> Option.map (fun rs ->
                      rs
                      |> List.map (fun r ->
                          match r.IsNullOrWhiteSpace() with
                          | true -> Some "--recurse-submodules"
                          | false -> Some $"--recurse-submodules={wrapString r}"))
                  |> Option.defaultValue []
              gc.ShallowModules
              |> Option.map (fun sm ->
                  match sm with
                  | true -> "--shallow-submodules"
                  | false -> "--no-shallow-submodules")
              gc.RemoteSubmodules
              |> Option.map (fun rs ->
                  match rs with
                  | true -> "--remote-submodules"
                  | false -> "--no-remote-submodules")
              gc.SeparateGitDirectory
              |> Option.map (fun sgd -> $"--separate-git-dir={wrapString sgd}")
              gc.RefFormat |> Option.map (fun rf -> $"--separate-git-dir={wrapString rf}")
              gc.Jobs |> Option.map (fun j -> $"--jobs {j}")
              Some gc.Repository
              gc.Directory
              gc.BundleUri |> Option.map (fun bu -> $"--bundle-uri={wrapString bu}") ]
            |> List.choose id
            |> concatStrings " "

    // A collection of simplify standard git operations.
    module StandardOperations =

        let getLastCommitHash (gitPath: string) (path: string) =
            match
                Process.run
                    { Name = gitPath
                      Args = "rev-parse HEAD"
                      StartDirectory = (Some path) }
            with
            | Ok r when r.Length > 0 -> Ok r.Head
            | Ok r -> Ok "Not commit hash found."
            | Error e -> Error e

        let clone (gitPath: string) (sourceUrl: string) (path: string) =
            let output, errors = Process.executeWithDefaultSettings gitPath $"clone {sourceUrl}" (path |> Some)

            match errors.Length = 0 with
            | true -> Ok output
            | false ->
                match errors.[0].StartsWith("Cloning into") with
                | true -> Ok [ "Cloned" ]
                | false -> Error errors

        let addTag (gitPath: string) (path: string) (tag: string) =
            let output, errors = Process.executeWithDefaultSettings gitPath $"tag {tag}" (path |> Some)

            match errors.Length = 0 with
            | true -> Ok output
            | false -> Error "Tag not added"

        let pushTag (gitPath: string) (path: string) (tag: string) =
            let output, errors = Process.executeWithDefaultSettings gitPath $"push origin {tag}" (path |> Some)
            // For whatever reason the results are returned in STDERR...

            match errors.Length > 1 with
            | true ->
                match errors.[1].StartsWith(" * [new tag]") with
                | true -> errors |> String.concat Environment.NewLine |> Ok
                | false -> Error "Tag not added"
            | false -> Error "Tag not added"

        let getAllCommits (gitPath: string) (path) =
            let output, errors = Process.executeWithDefaultSettings gitPath $"log --oneline" (path |> Some)

            match errors.IsEmpty with
            | true -> Ok output
            | false -> Error errors

        let getChangedAllFiles (gitPath: string) (commitHash: string) path =
            let output, errors =
                Process.executeWithDefaultSettings gitPath $"diff --name-only -r {commitHash}" (path |> Some)

            match errors.IsEmpty with
            | true -> Ok output
            | false -> Error errors

        let getChangedFiles (gitPath: string) (commitHash: string) path =
            let output, errors =
                Process.executeWithDefaultSettings gitPath $"diff --name-only -r {commitHash} {commitHash}~1" (path |> Some)

            match errors.IsEmpty with
            | true -> Ok output
            | false -> Error errors

    let clone
        (startHandler: ProcessStartHandler)
        (diagnosticHandler: ProcessDiagnosticHandler)
        (dotNetPath: string)
        (cloneSettings: CloneSettings)
        =
        let settings =
            ({ Name = dotNetPath
               Args = cloneSettings.CreateArgs()
               OverrideName = true
               OverrideArgs = true
               StartHandler = startHandler
               DiagnosticHandler = diagnosticHandler
               ResultHandler =
                 fun pr ->
                     match pr.StdError.[0].StartsWith("Cloning into") && pr.ExitCode = 0 with
                     | true -> ActionResult.Success pr
                     | false ->
                         FailureResult.Create(
                             "Failed to execute `git clone`.",
                             metadata = Map.ofList [ "errors", pr.StdError |> String.concat Environment.NewLine ]
                         )
                         |> ActionResult.Failure }
            : ProcessSettings)

        Process.execute settings
