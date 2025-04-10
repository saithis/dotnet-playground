#region License

// <copyright file="MessageBrokerEventsV3.cs" company="zvoove">
//     Copyright (c) 2025 zvoove Group GmbH
// </copyright>

#endregion

using Neuroglia.AsyncApi.v3;
using Wolverine;

namespace AsyncApiTestApi;

// https://github.com/asyncapi/net-sdk?tab=readme-ov-file#generate-code-first-asyncapi-documents
// https://www.asyncapi.com/blog/understanding-asyncapis#adding-channels-operations-and-messages


[AsyncApi("Demo API", "1.0.0", Description = "This is a demo api.", LicenseName = "Apache 2.0",
    LicenseUrl = "https://www.apache.org/licenses/LICENSE-2.0")]
[Channel(InternalExchangeName)]
[Channel(PublicExchangeName)]
[Channel(AuthzExchangeName)]
[Channel(InboxQueue)]
public class MessageBrokerEvents(IMessageBus bus) : IWolverineHandler
{
    public const string InternalExchangeName = "users.internal";
    public const string PublicExchangeName = "users.events";
    public const string AuthzExchangeName = "authz.events";
    public const string InboxQueue = "users.inbox";
    public const string SubscribeQueue = "users.subscriptions";

    [Operation(InternalUserEvent.MessageType, V3OperationAction.Send, $"#/channels/{InternalExchangeName}",
        Description = "Send message", MessagePayloadType = typeof(InternalUserEvent), Summary = "summary", Title = "title")]
    public async Task PublishInternalUserEventAsync(InternalUserEvent message, CancellationToken cancellationToken = default)
    {
        await bus.PublishAsync(message);
    }

    [Operation(UserCreated.MessageType, V3OperationAction.Send, $"#/channels/{PublicExchangeName}",
        Description = "Send message", MessagePayloadType = typeof(UserCreated), Summary = "summary", Title = "title")]
    public async Task PublishUserCreatedAsync(UserCreated message, CancellationToken cancellationToken = default)
    {
        await bus.PublishAsync(message);
    }

    [Operation(UserUpdated.MessageType, V3OperationAction.Send, $"#/channels/{PublicExchangeName}",
        Description = "Send message", MessagePayloadType = typeof(UserUpdated), Summary = "summary", Title = "title")]
    public async Task PublishUserUpdatedAsync(UserUpdated message, CancellationToken cancellationToken = default)
    {
        await bus.PublishAsync(message);
    }
    
    [Operation(UserDeleted.MessageType, V3OperationAction.Send, $"#/channels/{PublicExchangeName}",
        Description = "Send message", MessagePayloadType = typeof(UserDeleted), Summary = "summary", Title = "title")]
    public async Task PublishUserDeletedAsync(UserDeleted message, CancellationToken cancellationToken = default)
    {
        await bus.PublishAsync(message);
    }

    [Operation(DisableUser.MessageType, V3OperationAction.Receive, $"#/channels/{InboxQueue}",
        Description = "Receive message", MessagePayloadType = typeof(DisableUser), Summary = "summary", Title = "title")]
    public async Task Handle(DisableUser message)
    {
        Console.WriteLine($"Received message {message.UserId}");
    }

    [Operation(InternalUserEvent.MessageType, V3OperationAction.Receive, $"#/channels/{InternalExchangeName}",
        Description = "Receive message", MessagePayloadType = typeof(InternalUserEvent), Summary = "summary", Title = "title")]
    public async Task Handle(InternalUserEvent message)
    {
        Console.WriteLine($"Received message {message.UserId}");
    }
    
    [Operation(AuthzChanged.MessageType, V3OperationAction.Receive, $"#/channels/{AuthzExchangeName}",
        Description = "Receive message", MessagePayloadType = typeof(AuthzChanged), Summary = "summary", Title = "title")]
    public async Task Handle(AuthzChanged message)
    {
        Console.WriteLine($"Received message {message.UserId}");
    }
}
