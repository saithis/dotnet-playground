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
[Neuroglia.AsyncApi.v2.AsyncApi("Test API", "1.0.0", Description = "Test API for async api generation.", LicenseName = "Apache 2.0", LicenseUrl = "https://www.apache.org/licenses/LICENSE-2.0")]
public class MessageBrokerEventsV2(IMessageBus bus) : MessageBrokerEvents, IWolverineHandler
{
    [Neuroglia.AsyncApi.v2.Channel(PublicExchangeName), PublishOperation(OperationId = Notification.RoutingKey, Summary = "Some summary for the event")]
    public override async Task PublishNotificationAsync(Notification message, CancellationToken cancellationToken = default)
    {
        await bus.PublishAsync(message);
    }

    [Neuroglia.AsyncApi.v2.Channel(PublicExchangeName), PublishOperation(OperationId = ReconsumedNotification.RoutingKey, Summary = "Some summary for the event")]
    public override async Task PublishReconsumedNotificationAsync(ReconsumedNotification message, CancellationToken cancellationToken = default)
    {
        await bus.PublishAsync(message);
    }

    [Neuroglia.AsyncApi.v2.Channel(PublicExchangeName), PublishOperation(OperationId = CommandToOtherService.RoutingKey, Summary = "Some summary for the event")]
    public override async Task PublishCommandToOtherServiceAsync(CommandToOtherService message, CancellationToken cancellationToken = default)
    {
        await bus.PublishAsync(message);
    }
    
    [Neuroglia.AsyncApi.v2.Channel(ConsumeQueue), SubscribeOperation(OperationId = InboxMessage.RoutingKey, Summary = "Some summary for the event")]
    public override async Task Handle(InboxMessage message)
    {
        Console.WriteLine($"Received message {message.Id}");
    }

    [Neuroglia.AsyncApi.v2.Channel(PublicExchangeName), SubscribeOperation(OperationId = ReconsumedNotification.RoutingKey, Summary = "Some summary for the event")]
    public override async Task Handle(ReconsumedNotification message)
    {
        Console.WriteLine($"Received message {message.Id}");
    }
}
