module FsToolbox.Core

module Processes =

    [<RequireQualifiedAccess>]
    module Process =

        open System
        open System.Diagnostics

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

            let outputs =
                System.Collections.Generic.List<string>()

            let errors =
                System.Collections.Generic.List<string>()

            use p = new Process()
            let outputHandler f (_sender: obj) (args: DataReceivedEventArgs) = f args.Data

            p.StartInfo <- procInfo
            p.OutputDataReceived.AddHandler(DataReceivedEventHandler(outputHandler outputs.Add))
            p.ErrorDataReceived.AddHandler(DataReceivedEventHandler(outputHandler errors.Add))

            let started =
                try
                    p.Start()
                with
                | ex ->
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
                l
                |> Seq.filter (fun o -> String.IsNullOrWhiteSpace o |> not)
                |> List.ofSeq

            cleanOut outputs, cleanOut errors

        /// A wrapper around `Processes.execute` to be used in pipelines.
        let run (parameters: ProcessParameters) =
            let output, errors =
                execute parameters.Name parameters.Args parameters.StartDirectory

            match errors.Length = 0 with
            | true -> Ok output
            | false -> Error(String.concat Environment.NewLine errors)
