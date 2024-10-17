namespace FsToolbox.RabbitMQ

open System
open FsToolbox.Core.Results
open RabbitMQ.Client.Exceptions

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
        { Name: string
          Durable: bool
          Exclusive: bool
          AutoDelete: bool }

    [<RequireQualifiedAccess>]
    type RabbitMQFailure =
        | AlreadyCloses of AlreadyClosedException
        | AuthenticationFailure of AuthenticationFailureException
        | BrokerUnreachable of BrokerUnreachableException
        | ChannelAllocation of ChannelAllocationException
        | ChannelError of ChannelErrorException
        | ConnectFailure of ConnectFailureException
        | HardProtocol of HardProtocolException
        | MalformedFrame of MalformedFrameException
        | OperationInterrupted of OperationInterruptedException
        | PacketNotRecognized of PacketNotRecognizedException
        | PossibleAuthenticationFailure of PossibleAuthenticationFailureException
        | Protocol of ProtocolException
        | ProtocolVersionMismatch of ProtocolVersionMismatchException
        | ProtocolViolation of ProtocolViolationException
        | RabbitMQClient of RabbitMQClientException
        | SoftProtocol of SoftProtocolException
        | SyntaxError of SyntaxErrorException
        | TopologyRecovery of TopologyRecoveryException
        | UnexpectedFrame of UnexpectedFrameException
        | UnexpectedMethod of UnexpectedMethodException
        | UnknownClassOrMethod of UnknownClassOrMethodException
        | UnsupportedMethod of UnsupportedMethodException
        | UnsupportedMethodField of UnsupportedMethodFieldException
        | WireFormatting of WireFormattingException
        | OperationFailure of FailureResult
        | UnhandledException of Exception

        static member FromException(ex: Exception) =
            match ex with
            | :? AlreadyClosedException as ex -> RabbitMQFailure.AlreadyCloses ex
            | :? AuthenticationFailureException as ex -> RabbitMQFailure.AuthenticationFailure ex
            | :? BrokerUnreachableException as ex -> RabbitMQFailure.BrokerUnreachable ex
            | :? ChannelAllocationException as ex -> RabbitMQFailure.ChannelAllocation ex
            | :? ChannelErrorException as ex -> RabbitMQFailure.ChannelError ex
            | :? ConnectFailureException as ex -> RabbitMQFailure.ConnectFailure ex
            | :? HardProtocolException as ex -> RabbitMQFailure.HardProtocol ex
            | :? MalformedFrameException as ex -> RabbitMQFailure.MalformedFrame ex
            | :? OperationInterruptedException as ex -> RabbitMQFailure.OperationInterrupted ex
            | :? PacketNotRecognizedException as ex -> RabbitMQFailure.PacketNotRecognized ex
            | :? PossibleAuthenticationFailureException as ex -> RabbitMQFailure.PossibleAuthenticationFailure ex
            // Order is imported - this is a sub class of ProtocolException
            | :? SoftProtocolException as ex -> RabbitMQFailure.SoftProtocol ex
            | :? ProtocolException as ex -> RabbitMQFailure.Protocol ex
            | :? ProtocolVersionMismatchException as ex -> RabbitMQFailure.ProtocolVersionMismatch ex
            | :? ProtocolViolationException as ex -> RabbitMQFailure.ProtocolViolation ex
            | :? SyntaxErrorException as ex -> RabbitMQFailure.SyntaxError ex
            | :? TopologyRecoveryException as ex -> RabbitMQFailure.TopologyRecovery ex
            | :? UnexpectedFrameException as ex -> RabbitMQFailure.UnexpectedFrame ex
            | :? UnexpectedMethodException as ex -> RabbitMQFailure.UnexpectedMethod ex
            | :? UnknownClassOrMethodException as ex -> RabbitMQFailure.UnknownClassOrMethod ex
            | :? WireFormattingException as ex -> RabbitMQFailure.WireFormatting ex
            | :? RabbitMQClientException as ex -> RabbitMQFailure.RabbitMQClient ex
            | :? UnsupportedMethodException as ex -> RabbitMQFailure.UnsupportedMethod ex
            | :? UnsupportedMethodFieldException as ex -> RabbitMQFailure.UnsupportedMethodField ex
            | ex -> RabbitMQFailure.UnhandledException ex


        member f.ToFailureResult() =
            match f with
            | AlreadyCloses alreadyClosedException ->
                FailureResult.Create(
                    "RabbitMQ connection already Closed",
                    ex = alreadyClosedException,
                    metadata =
                        Map.ofList
                            [ "link",
                              "https://rabbitmq.github.io/rabbitmq-dotnet-client/api/RabbitMQ.Client.Exceptions.AlreadyClosedException.html" ]
                )
            | AuthenticationFailure authenticationFailureException ->
                FailureResult.Create(
                    "RabbitMQ authentication failure",
                    ex = authenticationFailureException,
                    metadata =
                        Map.ofList
                            [ "link",
                              "https://rabbitmq.github.io/rabbitmq-dotnet-client/api/RabbitMQ.Client.Exceptions.AuthenticationFailureException.html" ]
                )
            | BrokerUnreachable brokerUnreachableException ->
                FailureResult.Create(
                    "RabbitMQ broker unreachable",
                    ex = brokerUnreachableException,
                    metadata =
                        Map.ofList
                            [ "link",
                              "https://rabbitmq.github.io/rabbitmq-dotnet-client/api/RabbitMQ.Client.Exceptions.BrokerUnreachableException.html" ]
                )
            | ChannelAllocation channelAllocationException ->
                FailureResult.Create(
                    "RabbitMQ channel allocation",
                    ex = channelAllocationException,
                    metadata =
                        Map.ofList
                            [ "link",
                              "https://rabbitmq.github.io/rabbitmq-dotnet-client/api/RabbitMQ.Client.Exceptions.ChannelAllocationException.html" ]
                )
            | ChannelError channelErrorException ->
                FailureResult.Create(
                    "RabbitMQ channel error",
                    ex = channelErrorException,
                    metadata =
                        Map.ofList
                            [ "link",
                              "https://rabbitmq.github.io/rabbitmq-dotnet-client/api/RabbitMQ.Client.Exceptions.ChannelErrorException.html" ]
                )
            | ConnectFailure connectFailureException ->
                FailureResult.Create(
                    "RabbitMQ connect failure",
                    ex = connectFailureException,
                    metadata =
                        Map.ofList
                            [ "link",
                              "https://rabbitmq.github.io/rabbitmq-dotnet-client/api/RabbitMQ.Client.Exceptions.ConnectFailureException.html" ]
                )
            | HardProtocol hardProtocolException ->
                FailureResult.Create(
                    "RabbitMQ hard protocol",
                    ex = hardProtocolException,
                    metadata =
                        Map.ofList
                            [ "link",
                              "https://rabbitmq.github.io/rabbitmq-dotnet-client/api/RabbitMQ.Client.Exceptions.HardProtocolException.html" ]
                )
            | MalformedFrame malformedFrameException ->
                FailureResult.Create(
                    "RabbitMQ malformed frame",
                    ex = malformedFrameException,
                    metadata =
                        Map.ofList
                            [ "link",
                              "https://rabbitmq.github.io/rabbitmq-dotnet-client/api/RabbitMQ.Client.Exceptions.MalformedFrameException.html" ]
                )
            | OperationInterrupted operationInterruptedException ->
                FailureResult.Create(
                    "RabbitMQ operation interrupted",
                    ex = operationInterruptedException,
                    metadata =
                        Map.ofList
                            [ "link",
                              "https://rabbitmq.github.io/rabbitmq-dotnet-client/api/RabbitMQ.Client.Exceptions.OperationInterruptedException.html" ]
                )
            | PacketNotRecognized packetNotRecognizedException ->
                FailureResult.Create(
                    "RabbitMQ operation interrupted",
                    ex = packetNotRecognizedException,
                    metadata =
                        Map.ofList
                            [ "link",
                              "https://rabbitmq.github.io/rabbitmq-dotnet-client/api/RabbitMQ.Client.Exceptions.PacketNotRecognizedException.html" ]
                )
            | PossibleAuthenticationFailure possibleAuthenticationFailureException ->
                FailureResult.Create(
                    "RabbitMQ possible authentication failure",
                    ex = possibleAuthenticationFailureException,
                    metadata =
                        Map.ofList
                            [ "link",
                              "https://rabbitmq.github.io/rabbitmq-dotnet-client/api/RabbitMQ.Client.Exceptions.PossibleAuthenticationFailureException.html" ]
                )
            | Protocol protocolException ->
                FailureResult.Create(
                    "RabbitMQ protocol error",
                    ex = protocolException,
                    metadata =
                        Map.ofList
                            [ "link",
                              "https://rabbitmq.github.io/rabbitmq-dotnet-client/api/RabbitMQ.Client.Exceptions.ProtocolException.html" ]
                )
            | ProtocolVersionMismatch protocolVersionMismatchException ->
                FailureResult.Create(
                    "RabbitMQ protocol version mismatch",
                    ex = protocolVersionMismatchException,
                    metadata =
                        Map.ofList
                            [ "link",
                              "https://rabbitmq.github.io/rabbitmq-dotnet-client/api/RabbitMQ.Client.Exceptions.ProtocolVersionMismatchException.html" ]
                )
            | ProtocolViolation protocolViolationException ->
                FailureResult.Create(
                    "RabbitMQ protocol violation",
                    ex = protocolViolationException,
                    metadata =
                        Map.ofList
                            [ "link",
                              "https://rabbitmq.github.io/rabbitmq-dotnet-client/api/RabbitMQ.Client.Exceptions.ProtocolViolationException.html" ]
                )
            | RabbitMQClient rabbitMqClientException ->
                FailureResult.Create(
                    "RabbitMQ client error",
                    ex = rabbitMqClientException,
                    metadata =
                        Map.ofList
                            [ "link",
                              "https://rabbitmq.github.io/rabbitmq-dotnet-client/api/RabbitMQ.Client.Exceptions.RabbitMQClientException.html" ]
                )
            | SoftProtocol softProtocolException ->
                FailureResult.Create(
                    "RabbitMQ client error",
                    ex = softProtocolException,
                    metadata =
                        Map.ofList
                            [ "link",
                              "https://rabbitmq.github.io/rabbitmq-dotnet-client/api/RabbitMQ.Client.Exceptions.SoftProtocolException.html" ]
                )
            | SyntaxError syntaxErrorException ->
                FailureResult.Create(
                    "RabbitMQ syntax error",
                    ex = syntaxErrorException,
                    metadata =
                        Map.ofList
                            [ "link",
                              "https://rabbitmq.github.io/rabbitmq-dotnet-client/api/RabbitMQ.Client.Exceptions.SyntaxErrorException.html" ]
                )
            | TopologyRecovery topologyRecoveryException ->
                FailureResult.Create(
                    "RabbitMQ topology recovery error",
                    ex = topologyRecoveryException,
                    metadata =
                        Map.ofList
                            [ "link",
                              "https://rabbitmq.github.io/rabbitmq-dotnet-client/api/RabbitMQ.Client.Exceptions.TopologyRecoveryException.html" ]
                )
            | UnexpectedFrame unexpectedFrameException ->
                FailureResult.Create(
                    "RabbitMQ unexpected frame",
                    ex = unexpectedFrameException,
                    metadata =
                        Map.ofList
                            [ "link",
                              "https://rabbitmq.github.io/rabbitmq-dotnet-client/api/RabbitMQ.Client.Exceptions.UnexpectedFrameException.html" ]
                )
            | UnexpectedMethod unexpectedMethodException ->
                FailureResult.Create(
                    "RabbitMQ unexpected method",
                    ex = unexpectedMethodException,
                    metadata =
                        Map.ofList
                            [ "link",
                              "https://rabbitmq.github.io/rabbitmq-dotnet-client/api/RabbitMQ.Client.Exceptions.UnexpectedMethodException.html" ]
                )
            | UnknownClassOrMethod unknownClassOrMethodException ->
                FailureResult.Create(
                    "RabbitMQ unexpected method",
                    ex = unknownClassOrMethodException,
                    metadata =
                        Map.ofList
                            [ "link",
                              "https://rabbitmq.github.io/rabbitmq-dotnet-client/api/RabbitMQ.Client.Exceptions.UnknownClassOrMethodException.html" ]
                )
            | UnsupportedMethod unsupportedMethodException ->
                FailureResult.Create(
                    "RabbitMQ unsupported method",
                    ex = unsupportedMethodException,
                    metadata =
                        Map.ofList
                            [ "link",
                              "https://rabbitmq.github.io/rabbitmq-dotnet-client/api/RabbitMQ.Client.Exceptions.UnsupportedMethodException.html" ]
                )
            | UnsupportedMethodField unsupportedMethodFieldException ->
                FailureResult.Create(
                    "RabbitMQ unsupported method field",
                    ex = unsupportedMethodFieldException,
                    metadata =
                        Map.ofList
                            [ "link",
                              "https://rabbitmq.github.io/rabbitmq-dotnet-client/api/RabbitMQ.Client.Exceptions.UnsupportedMethodFieldException.html" ]
                )
            | WireFormatting wireFormattingException ->
                FailureResult.Create(
                    "RabbitMQ wire formatting error",
                    ex = wireFormattingException,
                    metadata =
                        Map.ofList
                            [ "link",
                              "https://rabbitmq.github.io/rabbitmq-dotnet-client/api/RabbitMQ.Client.Exceptions.WireFormattingException.html" ]
                )
            | OperationFailure failure -> failure
            | UnhandledException ``exception`` -> FailureResult.Create("Unhandled exception", ex = ``exception``)
