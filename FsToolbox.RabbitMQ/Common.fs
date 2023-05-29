namespace FsToolbox.RabbitMQ

[<AutoOpen>]
module Common =
    
    type RabbitMQCredentials =
        | LocalGuest
        | LocalUser of RabbitMQLocalUserCredentials
        | RemoteUser of RabbitMQRemoteUserCredentials

    and RabbitMQLocalUserCredentials = { Username: string; Password: string }

    and RabbitMQRemoteUserCredentials =
        { Endpoint: string
          Username: string
          Password: string }
        
    
    type QueueDefinition =
        {
            Name: string
            Durable: bool
            Exclusive: bool
            AutoDelete: bool
        }

