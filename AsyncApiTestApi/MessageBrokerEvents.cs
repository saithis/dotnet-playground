#region License
// <copyright file="MessageBrokerEvents.cs" company="zvoove">
//     Copyright (c) 2025 zvoove Group GmbH
// </copyright>
#endregion

using Saunter.Attributes;
using SlimMessageBus;

namespace AsyncApiTestApi;

// https://github.com/asyncapi/net-sdk?tab=readme-ov-file#generate-code-first-asyncapi-documents
// https://www.asyncapi.com/blog/understanding-asyncapis#adding-channels-operations-and-messages
[AsyncApi]
public class MessageBrokerEvents(IMessageBus bus) : IConsumer<InboxMessage>, IConsumer<ReconsumedNotification>
{
    public const string PublicExchangeName = "asyncapi.events";
    public const string ConsumeQueue = "asyncapi.consume-queue";

    public async Task PublishNotificationAsync(Notification message, CancellationToken cancellationToken = default)
    {
        await bus.Publish(message, cancellationToken: cancellationToken);
    }

    public async Task PublishReconsumedNotificationAsync(ReconsumedNotification message, CancellationToken cancellationToken = default)
    {
        await bus.Publish(message, cancellationToken: cancellationToken);
    }

    public async Task PublishCommandToOtherServiceAsync(CommandToOtherService message, CancellationToken cancellationToken = default)
    {
        await bus.Publish(message, cancellationToken: cancellationToken);
    }
    
    public Task OnHandle(InboxMessage message, CancellationToken cancellationToken)
    {
        Console.WriteLine($"Received message {message.Id}");
        return Task.CompletedTask;
    }

    public Task OnHandle(ReconsumedNotification message, CancellationToken cancellationToken)
    {
        Console.WriteLine($"Received message {message.Id}");
        return Task.CompletedTask;
    }
}