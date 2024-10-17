namespace FsToolbox.RabbitMQ

open FsToolbox.Core.Results
open RabbitMQ.Client
open RabbitMQ.Client.Exceptions


module Connections =

    let execute (fn: IConnection -> Result<unit, FailureResult>) (factory: ConnectionFactory) =
        use connection = factory.CreateConnection()

        try
            fn connection |> Result.mapError RabbitMQFailure.OperationFailure
        with
        | ex -> RabbitMQFailure.FromException ex |> Error
