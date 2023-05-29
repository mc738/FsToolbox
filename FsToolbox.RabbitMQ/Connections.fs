namespace FsToolbox.RabbitMQ

open FsToolbox.Core.Results
open RabbitMQ.Client


module Connections =

    let execute (fn: IConnection -> Result<unit, FailureResult>) (factory: ConnectionFactory) =
        use connection = factory.CreateConnection()

        try
            fn connection
        with ex ->
            { Message = $"Unhandled exception: {ex.Message}"
              DisplayMessage = "Connection failure"
              Exception = Some ex }
            |> Error
