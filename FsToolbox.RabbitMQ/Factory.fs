namespace FsToolbox.RabbitMQ

module Factory =

    open RabbitMQ.Client

    let create (credentials: RabbitMQCredentials) =

        let (endpoint, username, password) =
            match credentials with
            | LocalGuest -> "localhost", None, None
            | LocalUser luc -> "localhost", Some luc.Username, Some luc.Password
            | RemoteUser ruc -> ruc.Endpoint, Some ruc.Username, Some ruc.Password

        let mutable factory = ConnectionFactory()

        factory.Endpoint <- AmqpTcpEndpoint endpoint

        match username with
        | Some un -> factory.UserName <- un
        | None -> ()

        match password with
        | Some pw -> factory.Password <- pw
        | None -> ()

        factory

    let f = ()

    let d = ()
