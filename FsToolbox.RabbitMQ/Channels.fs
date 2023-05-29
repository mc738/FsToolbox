namespace FsToolbox.RabbitMQ

open FsToolbox.Core.Results
open RabbitMQ.Client

module Channels =


    let declareQueue (definition: QueueDefinition) (channel: IModel) =
        channel.QueueDeclare(
            queue = definition.Name,
            durable = definition.Durable,
            exclusive = definition.Exclusive,
            autoDelete = definition.AutoDelete
        )

    let run (fn: IModel -> Result<unit, FailureResult>) (connection: IConnection) =
        use channel = connection.CreateModel()

        try
            fn channel
        with ex ->
            { Message = $"Unhandled exception: {ex.Message}"
              DisplayMessage = "Channel failure"
              Exception = Some ex }
            |> Error
