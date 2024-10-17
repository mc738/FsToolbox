namespace FsToolbox.ProcessWrappers

[<RequireQualifiedAccess>]
module Git =

    open System
    open FsToolbox.Core.Processes
    open FsToolbox.ProcessWrappers.Common
    open FsToolbox.Core

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
          ShallowSince: DateTime option
          ShallowExclude: string option
          SingleBranch: bool option
          NoTags: bool option
          RecurseSubmodules: string option
          ShallowModules: bool option
          RemoteSubmodules: bool option
          SeparateGitDirectory: bool option
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

        member gc.CreateArgs(gitPath: string) =
            [
                Some gitPath
                gc.Template |> Option.map (fun td -> $"--template={wrapString td}")
                gc.Local |> Option.ifTrue "--local"
                gc.NoHardLinks |> Option.ifTrue "--no-hardlinks"
                gc.Shared |> Option.ifTrue "--shared"
                gc.Reference |> Option.map (fun r -> $"--reference {wrapString r}")
                gc.Dissociate |> Option.ifTrue "--dissociate"
                gc.Quiet |> Option.ifTrue "--quiet"
                
                
                
                
            ]
            |> List.choose id
            |> concatStrings " "
    
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
        let output, errors = Process.execute gitPath $"clone {sourceUrl}" (path |> Some)

        match errors.Length = 0 with
        | true -> Ok output
        | false ->
            match errors.[0].StartsWith("Cloning into") with
            | true -> Ok [ "Cloned" ]
            | false -> Error errors

    let addTag (gitPath: string) (path: string) (tag: string) =
        let output, errors = Process.execute gitPath $"tag {tag}" (path |> Some)

        match errors.Length = 0 with
        | true -> Ok output
        | false -> Error "Tag not added"

    let pushTag (gitPath: string) (path: string) (tag: string) =
        let output, errors = Process.execute gitPath $"push origin {tag}" (path |> Some)
        // For whatever reason the results are returned in STDERR...

        match errors.Length > 1 with
        | true ->
            match errors.[1].StartsWith(" * [new tag]") with
            | true -> errors |> String.concat Environment.NewLine |> Ok
            | false -> Error "Tag not added"
        | false -> Error "Tag not added"

    let getAllCommits (gitPath: string) (path) =
        let output, errors = Process.execute gitPath $"log --oneline" (path |> Some)

        match errors.IsEmpty with
        | true -> Ok output
        | false -> Error errors

    let getChangedAllFiles (gitPath: string) (commitHash: string) path =
        let output, errors =
            Process.execute gitPath $"diff --name-only -r {commitHash}" (path |> Some)

        match errors.IsEmpty with
        | true -> Ok output
        | false -> Error errors

    let getChangedFiles (gitPath: string) (commitHash: string) path =
        let output, errors =
            Process.execute gitPath $"diff --name-only -r {commitHash} {commitHash}~1" (path |> Some)

        match errors.IsEmpty with
        | true -> Ok output
        | false -> Error errors
