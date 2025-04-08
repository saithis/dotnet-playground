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
[Channel(PublicExchangeName, Description = "This channel is used to exchange test messages.", Servers = ["#/servers/rabbitmq"],
    // Name = "Public service events", This is the name referenced from other attributes
    Summary = "summary of the channel", Title = "our title")]
public class MessageBrokerEvents(IMessageBus bus) : IWolverineHandler
{
    public const string PublicExchangeName = "asyncapi.events";
    public const string SubscribeQueue = "asyncapi.subscribe-queue";
    public const string InboxQueue = "asyncapi.inbox-queue";

    [Operation(Notification.RoutingKey, V3OperationAction.Send, $"#/channels/{PublicExchangeName}",
        Description = "Send message", MessagePayloadType = typeof(Notification), Summary = "summary", Title = "title")]
    [Tag(Reference = $"#/components/tags/{Notification.RoutingKey}")]
    public async Task PublishNotificationAsync(Notification message, CancellationToken cancellationToken = default)
    {
        await bus.PublishAsync(message);
    }

    [Operation(ReconsumedNotification.RoutingKey, V3OperationAction.Send, $"#/channels/{PublicExchangeName}",
        Description = "Send message", MessagePayloadType = typeof(ReconsumedNotification), Summary = "summary", Title = "title")]
    [Tag(Reference = $"#/components/tags/{ReconsumedNotification.RoutingKey}")]
    public async Task PublishReconsumedNotificationAsync(ReconsumedNotification message, CancellationToken cancellationToken = default)
    {
        await bus.PublishAsync(message);
    }

    [Operation(CommandToOtherService.RoutingKey, V3OperationAction.Send, $"#/channels/{PublicExchangeName}",
        Description = "Send message", MessagePayloadType = typeof(CommandToOtherService), Summary = "summary", Title = "title")]
    [Tag(Reference = $"#/components/tags/{CommandToOtherService.RoutingKey}")]
    public async Task PublishCommandToOtherServiceAsync(CommandToOtherService message, CancellationToken cancellationToken = default)
    {
        await bus.PublishAsync(message);
    }

    public async Task Handle(InboxMessage message)
    {
        Console.WriteLine($"Received message {message.Id}");
    }

    public async Task Handle(ReconsumedNotification message)
    {
        Console.WriteLine($"Received message {message.Id}");
    }
}
