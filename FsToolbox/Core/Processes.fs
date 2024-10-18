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
          StartInfoBuilder: ProcessStartInfo -> ProcessStartInfo
          StandardOutputHandler: (string -> unit) option
          StandardErrorHandler: (string -> unit) option
          ResultHandler: ProcessResult -> ActionResult<ProcessResult> }

    and ProcessFailure =
        { Results: ProcessResult
          IsTransient: bool }

    and ProcessResult =
        { ExitCode: int
          Args: string
          Pid: int
          StdOut: string list
          StdError: string list
          StartTime: DateTime
          ExecutionDuration: TimeSpan
          ExitTime: DateTime }

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
