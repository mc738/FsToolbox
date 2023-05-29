namespace FsToolbox.RabbitMQ

open System
open System.Text
open System.Text.Json
open FsToolbox.Core.Results
open RabbitMQ.Client
open RabbitMQ.Client.Events

module Channels =

    let declareQueue (definition: QueueDefinition) (channel: IModel) =
        channel,
        channel.QueueDeclare(
            queue = definition.Name,
            durable = definition.Durable,
            exclusive = definition.Exclusive,
            autoDelete = definition.AutoDelete
        )

    type PublishParameters =
        { Exchange: string option
          Channel: string option
          Properties: PublishProperties option }

    and PublishProperties = { Persistent: bool }

    let publish (parameters: PublishParameters) (body: byte array) (channel: IModel) =
        let props =
            match parameters.Properties with
            | Some p ->
                let mutable props = channel.CreateBasicProperties()

                props.Persistent <- p.Persistent

                props
            | None -> null

        channel.BasicPublish(
            exchange = (parameters.Exchange |> Option.defaultValue String.Empty),
            routingKey = (parameters.Exchange |> Option.defaultValue String.Empty),
            basicProperties = props,
            body = body
        )


        channel

    let publishJson<'T> (parameters: PublishParameters) (value: 'T) (channel: IModel) =
        let body = JsonSerializer.Serialize value |> Encoding.UTF8.GetBytes

        publish parameters body channel

    let consume (queue: string) (fn: byte array -> Result<unit, FailureResult>) (channel: IModel) =
        let consumer = EventingBasicConsumer(channel)
        
        consumer.Received.Add(fun args ->
            
            let message = args.Body.ToArray()
            
            match fn message with
            | Ok _ -> channel.BasicAck(deliveryTag = args.DeliveryTag, multiple = false)
            | Error f ->
                // TODO add dead lettering.
                // This will constantly requeue messages. Even if they are poison.
                // Need a specialist return type?
                // Success
                // TransientFailure
                // PermanentFailure
                //
                // https://medium.com/codait/handling-failure-successfully-in-rabbitmq-22ffa982b60f
                //
                // For TransientFailures:
                // 1. Ack message (to clear original)
                // 2. Publish a new message with retry count
                // 3. If retry count is too high (say over 10) - dead letter.
                channel.BasicReject(deliveryTag = args.DeliveryTag, requeue = false))
        
        channel.BasicConsume(queue, autoAck = false, consumer = consumer)
    
    let consumeJson<'T> (queue: string) (fn: 'T -> Result<unit, FailureResult>) (channel: IModel) =
        
        let handler = fun (d: byte array) ->
            d |> Encoding.UTF8.GetString |> JsonSerializer.Deserialize<'T> |> fn
        
        consume queue handler channel
        
    let run (fn: IModel -> Result<unit, FailureResult>) (connection: IConnection) =
        use channel = connection.CreateModel()

        try
            fn channel
        with ex ->
            { Message = $"Unhandled exception: {ex.Message}"
              DisplayMessage = "Channel failure"
              Exception = Some ex }
            |> Error
