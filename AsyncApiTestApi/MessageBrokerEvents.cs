#region License

// <copyright file="MessageBrokerEventsV2.cs" company="zvoove">
//     Copyright (c) 2025 zvoove Group GmbH
// </copyright>

#endregion

using Neuroglia.AsyncApi.v2;
using Wolverine;

namespace AsyncApiTestApi;

// https://github.com/asyncapi/net-sdk?tab=readme-ov-file#generate-code-first-asyncapi-documents
// https://www.asyncapi.com/blog/understanding-asyncapis#adding-channels-operations-and-messages
[AsyncApi("Test API", "1.0.0", Description = "Test API for async api generation.", LicenseName = "Apache 2.0",
    LicenseUrl = "https://www.apache.org/licenses/LICENSE-2.0")]
public class MessageBrokerEvents(IMessageBus bus) : IWolverineHandler
{
    public const string PublicExchangeName = "asyncapi.events";
    public const string SubscribeQueue = "asyncapi.consume-queue";
    public const string InboxQueue = "asyncapi.inbox-queue";

    [Channel(PublicExchangeName)]
    [PublishOperation(OperationId = Notification.RoutingKey, Summary = "Some summary for the event")]
    public async Task PublishNotificationAsync(Notification message, CancellationToken cancellationToken = default)
    {
        await bus.PublishAsync(message);
    }

    [Channel(PublicExchangeName)]
    [PublishOperation(OperationId = ReconsumedNotification.RoutingKey, Summary = "Some summary for the event")]
    public async Task PublishReconsumedNotificationAsync(ReconsumedNotification message, CancellationToken cancellationToken = default)
    {
        await bus.PublishAsync(message);
    }

    [Channel(PublicExchangeName)]
    [PublishOperation(OperationId = CommandToOtherService.RoutingKey, Summary = "Some summary for the event")]
    public async Task PublishCommandToOtherServiceAsync(CommandToOtherService message, CancellationToken cancellationToken = default)
    {
        await bus.PublishAsync(message);
    }

    [Channel(SubscribeQueue)]
    [SubscribeOperation(OperationId = InboxMessage.RoutingKey, Summary = "Some summary for the event")]
    public async Task Handle(InboxMessage message)
    {
        Console.WriteLine($"Received message {message.Id}");
    }

    [Channel(PublicExchangeName)]
    [SubscribeOperation(OperationId = ReconsumedNotification.RoutingKey, Summary = "Some summary for the event")]
    public async Task Handle(ReconsumedNotification message)
    {
        Console.WriteLine($"Received message {message.Id}");
    }
}
