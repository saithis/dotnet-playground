#region License

// <copyright file="MessageBrokerEventsV3.cs" company="zvoove">
//     Copyright (c) 2025 zvoove Group GmbH
// </copyright>

#endregion

using Wolverine;

namespace AsyncApiTestApi;

// https://github.com/asyncapi/net-sdk?tab=readme-ov-file#generate-code-first-asyncapi-documents
// https://www.asyncapi.com/blog/understanding-asyncapis#adding-channels-operations-and-messages

[AsyncApiInfo("User Service API", "1.0.1", Description = "API for user events.")]
[AsyncApiServer("production-rabbitmq", "amqp://user:pass@rabbitmq.example.com:5672/", Description = "Production RabbitMQ Broker")]


public class MessageBrokerEvents(IMessageBus bus) : IWolverineHandler
{
    public const string InternalExchangeName = "users.internal";
    public const string PublicExchangeName = "users.events";
    public const string AuthzExchangeName = "authz.events";
    public const string InboxQueue = "users.inbox";
    public const string SubscribeQueue = "users.subscriptions";

    [AsyncApiChannel(PublicExchangeName, Description = "Channel for all public user related events.")]
    [RabbitMqChannelBinding(Is = "routingKey", ExchangeName = PublicExchangeName, ExchangeType = "topic")]
    public void DefinePublicChannel() { }
    
    [AsyncApiChannel(InternalExchangeName, Description = "Channel for all internal user related events.")]
    [RabbitMqChannelBinding(Is = "routingKey", ExchangeName = InternalExchangeName, ExchangeType = "topic")]
    public void DefineInternalChannel() { }
    
    [AsyncApiChannel(AuthzExchangeName, Description = "Authz events channel.")]
    [RabbitMqChannelBinding(Is = "routingKey", ExchangeName = AuthzExchangeName, ExchangeType = "topic")]
    public void DefineAuthzChannel() { }
    
    [AsyncApiChannel(InboxQueue, Description = "Channel for all public user related commands.")]
    [RabbitMqChannelBinding(Is = "routingKey", ExchangeName = InboxQueue, ExchangeType = "direct")]
    public void DefineInboxChannel() { }
    
    
    [AsyncApiOperation(InternalExchangeName, AsyncApiAction.Send, typeof(InternalUserEvent), OperationId = InternalUserEvent.MessageType)]
    [RabbitMqOperationBinding(RoutingKey = InternalUserEvent.MessageType)]
    public async Task PublishInternalUserEventAsync(InternalUserEvent message, CancellationToken cancellationToken = default)
    {
        await bus.PublishAsync(message);
    }

    [AsyncApiOperation(PublicExchangeName, AsyncApiAction.Send, typeof(UserCreated), OperationId = UserCreated.MessageType)]
    [RabbitMqOperationBinding(RoutingKey = UserCreated.MessageType)]
    public async Task PublishUserCreatedAsync(UserCreated message, CancellationToken cancellationToken = default)
    {
        await bus.PublishAsync(message);
    }

    [AsyncApiOperation(PublicExchangeName, AsyncApiAction.Send, typeof(UserUpdated), OperationId = UserUpdated.MessageType)]
    [RabbitMqOperationBinding(RoutingKey = UserUpdated.MessageType)]
    public async Task PublishUserUpdatedAsync(UserUpdated message, CancellationToken cancellationToken = default)
    {
        await bus.PublishAsync(message);
    }
    
    [AsyncApiOperation(PublicExchangeName, AsyncApiAction.Send, typeof(UserDeleted), OperationId = UserDeleted.MessageType)]
    [RabbitMqOperationBinding(RoutingKey = UserDeleted.MessageType)]
    public async Task PublishUserDeletedAsync(UserDeleted message, CancellationToken cancellationToken = default)
    {
        await bus.PublishAsync(message);
    }

    [AsyncApiOperation(InboxQueue, AsyncApiAction.Receive, typeof(DisableUser), OperationId = DisableUser.MessageType)]
    [RabbitMqOperationBinding(RoutingKey = DisableUser.MessageType)]
    public async Task Handle(DisableUser message)
    {
        Console.WriteLine($"Received message {message.UserId}");
    }

    [AsyncApiOperation(InternalExchangeName, AsyncApiAction.Receive, typeof(InternalUserEvent), OperationId = InternalUserEvent.MessageType+"Receive")]
    [RabbitMqOperationBinding(RoutingKey = InternalUserEvent.MessageType)]
    public async Task Handle(InternalUserEvent message)
    {
        Console.WriteLine($"Received message {message.UserId}");
    }
    
    [AsyncApiOperation(AuthzExchangeName, AsyncApiAction.Send, typeof(AuthzChanged), OperationId = AuthzChanged.MessageType)]
    [RabbitMqOperationBinding(RoutingKey = AuthzChanged.MessageType)]
    public async Task Handle(AuthzChanged message)
    {
        Console.WriteLine($"Received message {message.UserId}");
    }
}
