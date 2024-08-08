module FsToolbox.Scripting

open System.IO.Pipes

module InterprocessCommunication =

    let rec read (pipe: NamedPipeServerStream) =
        task {
            try
                match pipe.IsConnected with
                | true ->



                    return! read (pipe)
                | false -> return Ok()
            with ex ->
                return Error ex.Message
        }


    let startServer name =
        task {
            let pipeServer = new NamedPipeServerStream(name, PipeDirection.InOut, 1)

            //let threadId =

            do! pipeServer.WaitForConnectionAsync()

            try


                pipeServer.ReadAsync()


                let ss = new StreamString(pipeServer)

                return ()
            with ex ->

                return ()

        }


    ()
