#region License
// <copyright file="MessageBrokerEvents.cs" company="zvoove">
//     Copyright (c) 2025 zvoove Group GmbH
// </copyright>
#endregion

using Saunter.Attributes;
using Wolverine;

namespace AsyncApiTestApi;

// https://github.com/asyncapi/net-sdk?tab=readme-ov-file#generate-code-first-asyncapi-documents
// https://www.asyncapi.com/blog/understanding-asyncapis#adding-channels-operations-and-messages
[AsyncApi]
public class MessageBrokerEvents(IMessageBus bus) : IWolverineHandler
{
    public const string PublicExchangeName = "asyncapi.events";
    public const string ConsumeQueue = "asyncapi.consume-queue";


    [SubscribeOperation(typeof(Notification), Notification.RoutingKey, Summary = "Some summary for the event.", Description = "Some description for the event.")]
    public async Task PublishNotificationAsync(Notification message, CancellationToken cancellationToken = default)
    {
        await bus.PublishAsync(message);
    }

    [SubscribeOperation(typeof(ReconsumedNotification), ReconsumedNotification.RoutingKey, Summary = "Some summary for the event.", Description = "Some description for the event.")]
    public async Task PublishReconsumedNotificationAsync(ReconsumedNotification message, CancellationToken cancellationToken = default)
    {
        await bus.PublishAsync(message);
    }

    [Channel(PublicExchangeName, Servers = ["rabbitmq"])]
    [SubscribeOperation(typeof(CommandToOtherService), CommandToOtherService.RoutingKey, Summary = "Some summary for the event.", Description = "Some description for the event.")]
    public async Task PublishCommandToOtherServiceAsync(CommandToOtherService message, CancellationToken cancellationToken = default)
    {
        await bus.PublishAsync(message);
    }
    
    [PublishOperation(typeof(InboxMessage), InboxMessage.RoutingKey, Summary = "Some summary for the event.", Description = "Some description for the event.")]
    public async Task Handle(InboxMessage message)
    {
        Console.WriteLine($"Received message {message.Id}");
    }

    [Channel(ConsumeQueue, Servers = ["rabbitmq"])]
    [PublishOperation(typeof(ReconsumedNotification), ReconsumedNotification.RoutingKey, Summary = "Some summary for the event.", Description = "Some description for the event.")]
    public async Task Handle(ReconsumedNotification message)
    {
        Console.WriteLine($"Received message {message.Id}");
    }
}