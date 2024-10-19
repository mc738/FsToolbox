namespace FsToolbox.Core

open System
open System.Diagnostics
open FsToolbox.Core.Results

module Processes =

    [<RequireQualifiedAccess>]
    module Process =

        type ProcessParameters =
            { Name: string
              Args: string
              StartDirectory: string option }

        /// Execute a process
        let execute filename args startDir =
            // TODO add proper error handling.
            let timer = Stopwatch.StartNew()
            let procInfo = ProcessStartInfo()
            procInfo.RedirectStandardOutput <- true
            procInfo.RedirectStandardError <- true
            procInfo.UseShellExecute <- false
            procInfo.FileName <- filename
            procInfo.Arguments <- args


            match startDir with
            | Some d -> procInfo.WorkingDirectory <- d
            | _ -> ()

            let outputs = System.Collections.Generic.List<string>()

            let errors = System.Collections.Generic.List<string>()

            use p = new Process()
            let outputHandler f (_sender: obj) (args: DataReceivedEventArgs) = f args.Data

            p.StartInfo <- procInfo
            p.OutputDataReceived.AddHandler(DataReceivedEventHandler(outputHandler outputs.Add))
            p.ErrorDataReceived.AddHandler(DataReceivedEventHandler(outputHandler errors.Add))

            let started =
                try
                    p.Start()
                with ex ->
                    ex.Data.Add("filename", filename)
                    reraise ()

            if not started then
                failwithf $"Failed to start process {filename}"

            printfn $"Started {p.ProcessName} (pid: {p.Id})"
            printfn $"\tArgs: {args}"
            printfn $"\tId: {p.Id}"
            p.BeginOutputReadLine()
            p.BeginErrorReadLine()
            p.WaitForExit()
            timer.Stop()
            printfn $"Finished {filename} after {timer.ElapsedMilliseconds} milliseconds"

            let cleanOut l =
                l |> Seq.filter (fun o -> String.IsNullOrWhiteSpace o |> not) |> List.ofSeq

            cleanOut outputs, cleanOut errors

        /// A wrapper around `Processes.execute` to be used in pipelines.
        let run (parameters: ProcessParameters) =
            let output, errors =
                execute parameters.Name parameters.Args parameters.StartDirectory

            match errors.Length = 0 with
            | true -> Ok output
            | false -> Error(String.concat Environment.NewLine errors)

    type ProcessSettings =
        { Name: string
          Args: string
          OverrideName: bool
          OverrideArgs: bool
          StartHandler: ProcessStartHandler
          DiagnosticHandler: ProcessDiagnosticHandler
          ResultHandler: ProcessResult -> ActionResult<ProcessResult> }

        static member Default =
            { Name = ""
              Args = ""
              OverrideName = false
              OverrideArgs = false
              StartHandler = ProcessStartHandler.Default 
              DiagnosticHandler = ProcessDiagnosticHandler.Default
              ResultHandler = id >> ActionResult.Success }

    and ProcessDiagnosticHandler =
        { Logger: (string -> unit) option
          StandardOutputHandler: (string -> unit) option
          StandardErrorHandler: (string -> unit) option }

        static member Default =
            { Logger = None
              StandardOutputHandler = None
              StandardErrorHandler = None }

    and ProcessStartHandler =
        { Timeout: int option
          StartDirectory: string option
          StartInfoBuilder: ProcessStartInfo -> ProcessStartInfo }

        static member Default =
            { Timeout = None
              StartDirectory = None
              StartInfoBuilder = id }

    and ProcessFailure =
        { Results: ProcessResult
          IsTransient: bool }

    and ProcessResult =
        { ExitCode: int
          TimedOut: bool
          Args: string
          Pid: int
          StdOut: string list
          StdError: string list
          StartTime: DateTime
          ExecutionDuration: TimeSpan
          ExitTime: DateTime }

    let execute (settings: ProcessSettings) =

        let outputs = ResizeArray<string>()

        let errors = ResizeArray<string>()

        try
            let timer = Stopwatch.StartNew()
            let procInfo = ProcessStartInfo() |> settings.StartHandler.StartInfoBuilder

            if
                String.IsNullOrWhiteSpace procInfo.FileName
                || (settings.OverrideName && String.IsNullOrWhiteSpace settings.Name |> not)
            then
                procInfo.FileName <- settings.Name

            if
                String.IsNullOrWhiteSpace procInfo.Arguments
                || (settings.OverrideArgs && String.IsNullOrWhiteSpace settings.Args |> not)
            then
                procInfo.Arguments <- settings.Args

            match settings.StartHandler.StartDirectory with
            | Some d -> procInfo.WorkingDirectory <- d
            | _ -> ()

            if
                procInfo.RedirectStandardOutput |> not
                && settings.DiagnosticHandler.StandardOutputHandler.IsSome
            then
                procInfo.RedirectStandardOutput <- true

            if
                procInfo.RedirectStandardError |> not
                && settings.DiagnosticHandler.StandardOutputHandler.IsSome
            then
                procInfo.RedirectStandardError <- true

            let outputHandler (_sender: obj) (args: DataReceivedEventArgs) =
                settings.DiagnosticHandler.StandardOutputHandler
                |> Option.iter (fun fn -> fn args.Data)

                outputs.Add args.Data

            let errorHandler (_sender: obj) (args: DataReceivedEventArgs) =
                settings.DiagnosticHandler.StandardErrorHandler
                |> Option.iter (fun fn -> fn args.Data)

                errors.Add args.Data

            use proc = new Process()
            proc.OutputDataReceived.AddHandler(DataReceivedEventHandler(outputHandler))
            proc.ErrorDataReceived.AddHandler(DataReceivedEventHandler(errorHandler))

            match proc.Start() with
            | true ->
                let pid = proc.Id

                settings.DiagnosticHandler.Logger
                |> Option.iter (fun l -> l $"Started {proc.ProcessName} (pid: {proc.Id})")

                settings.DiagnosticHandler.Logger
                |> Option.iter (fun l -> l $"\tArgs: {procInfo.Arguments}")

                proc.BeginOutputReadLine()
                proc.BeginErrorReadLine()

                let exitedInTime =
                    match settings.StartHandler.Timeout with
                    | Some timeout -> proc.WaitForExit(timeout)
                    | None ->
                        proc.WaitForExit()
                        true

                timer.Stop()

                settings.DiagnosticHandler.Logger
                |> Option.iter (fun l -> l $"Process {proc.ProcessName} (pid: {proc.Id}) finished")

                settings.DiagnosticHandler.Logger
                |> Option.iter (fun l -> l $"\tCompleted in: {timer.ElapsedMilliseconds}ms")

                settings.DiagnosticHandler.Logger
                |> Option.iter (fun l -> l $"\tTimed out: {exitedInTime |> not}")

                let cleanOut l =
                    l
                    |> Seq.filter (fun o -> System.String.IsNullOrWhiteSpace o |> not)
                    |> List.ofSeq

                { ExitCode = proc.ExitCode
                  TimedOut = exitedInTime |> not
                  Args = procInfo.Arguments
                  Pid = pid
                  StdOut = outputs |> List.ofSeq
                  StdError = errors |> List.ofSeq
                  StartTime = proc.StartTime
                  ExecutionDuration = timer.Elapsed
                  ExitTime = proc.ExitTime }
                |> settings.ResultHandler
            | false -> FailureResult.Create() |> ActionResult.Failure
        with ex ->
            FailureResult.Create(
                $"Unhandled exception while executing process. Error: {ex.Message}",
                "Unhandled exception while executing process",
                ex = ex,
                metadata =
                    Map.ofList
                        [ "output", outputs |> String.concat Environment.NewLine
                          "errors", errors |> String.concat Environment.NewLine ]
            )
            |> ActionResult.Failure

    let executeWithDiagnostics
        filename
        args
        startDir
        (outputReceivedFn: (string -> unit) option)
        (errorReceivedFn: (string -> unit) option)
        =

        try
            let timer = Stopwatch.StartNew()
            let procInfo = ProcessStartInfo()
            procInfo.RedirectStandardOutput <- true
            procInfo.RedirectStandardError <- true
            procInfo.UseShellExecute <- false
            procInfo.FileName <- filename
            procInfo.Arguments <- args

            match startDir with
            | Some d -> procInfo.WorkingDirectory <- d
            | _ -> ()

            let outputs = System.Collections.Generic.List<string>()

            let errors = System.Collections.Generic.List<string>()

            use p = new Process()

            let outputHandler (_sender: obj) (args: DataReceivedEventArgs) =
                outputReceivedFn |> Option.iter (fun fn -> fn args.Data)
                outputs.Add args.Data

            let errorHandler (_sender: obj) (args: DataReceivedEventArgs) =
                errorReceivedFn |> Option.iter (fun fn -> fn args.Data)
                errors.Add args.Data

            p.StartInfo <- procInfo
            p.OutputDataReceived.AddHandler(DataReceivedEventHandler(outputHandler))
            p.ErrorDataReceived.AddHandler(DataReceivedEventHandler(errorHandler))

            match p.Start() with
            | true ->

                let pid = p.Id
                printfn $"Started {p.ProcessName} (pid: {p.Id})"
                printfn $"\tArgs: {args}"
                printfn $"\tId: {p.Id}"
                p.BeginOutputReadLine()
                p.BeginErrorReadLine()
                p.WaitForExit()
                timer.Stop()

                let cleanOut l =
                    l
                    |> Seq.filter (fun o -> System.String.IsNullOrWhiteSpace o |> not)
                    |> List.ofSeq

                { ExitCode = p.ExitCode
                  TimedOut = false
                  Args = failwith "todo"
                  Pid = pid
                  StdOut = outputs |> List.ofSeq
                  StdError = errors |> List.ofSeq
                  StartTime = p.StartTime
                  ExecutionDuration = timer.Elapsed
                  ExitTime = p.ExitTime }
                |> ActionResult.Success
            | false -> FailureResult.Create("Process failed to start") |> ActionResult.Failure
        with ex ->
            FailureResult.Create(
                $"Unhandled exception while executing process. Error: {ex.Message}",
                "Unhandled exception while executing process"
            )
            |> ActionResult.Failure
