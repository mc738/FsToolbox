namespace FsToolbox.Scripting

module ScriptHost =

    open System.IO
    open System.Text
    open FSharp.Compiler.Interactive.Shell

    let fsiSession _ =
        // Initialize output and input streams
        let sbOut = new StringBuilder()
        let sbErr = new StringBuilder()
        let inStream = new StringReader("")
        let outStream = new StringWriter(sbOut)
        let errStream = new StringWriter(sbErr)

        // Build command line arguments & start FSI session
        let argv = [| "C:\\fsi.exe" |]

        let allArgs = Array.append argv [| "--noninteractive" |]

        let fsiConfig = FsiEvaluationSession.GetDefaultConfiguration()

        FsiEvaluationSession.Create(fsiConfig, allArgs, inStream, outStream, errStream)

    let eval<'T> (path: string) (name: string) (fsi: FsiEvaluationSession) =
        try
            match fsi.EvalScriptNonThrowing path with
            | Choice1Of2 _, diagnostics ->

                match fsi.EvalExpressionNonThrowing name with
                | Choice1Of2 r, diagnostics ->
                    //printfn $"Evaluate script diagnostics: {diagnostics}"
                    //printfn $"Evaluate expression diagnostics: {diagnostics}"
                    r
                    |> Option.bind (fun v -> v.ReflectionValue |> unbox<'T> |> Ok |> Some)
                    |> Option.defaultValue (Result.Error "No result")
                | Choice2Of2 exn, diagnostics ->
                    printfn $"Error evaluating expression: {exn.Message}"
                    printfn $"Evaluate expression diagnostics: {diagnostics}"
                    Result.Error exn.Message
            | Choice2Of2 exn, diagnostics ->
                printfn $"Error evaluating script: {exn.Message}"
                printfn $"Evaluate script diagnostics: {diagnostics}"
                Result.Error exn.Message
        with exn ->
            Error $"Unhandled error: {exn.Message}"

    type HostContext =
        { FsiSession: FsiEvaluationSession }

        static member Create() = { FsiSession = fsiSession () }

        member hc.Eval<'T>(path, name) = eval<'T> path name hc.FsiSession
