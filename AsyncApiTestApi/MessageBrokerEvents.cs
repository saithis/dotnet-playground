#region License
// <copyright file="MessageBrokerEvents.cs" company="zvoove">
//     Copyright (c) 2025 zvoove Group GmbH
// </copyright>
#endregion

namespace AsyncApiTestApi;

public abstract class MessageBrokerEvents
{
    public const string PublicExchangeName = "asyncapi.events";
    public const string ConsumeQueue = "asyncapi.consume-queue";

    public abstract Task PublishNotificationAsync(Notification message, CancellationToken cancellationToken = default);

    public abstract Task PublishReconsumedNotificationAsync(
        ReconsumedNotification message,
        CancellationToken cancellationToken = default
    );

    public abstract Task PublishCommandToOtherServiceAsync(
        CommandToOtherService message,
        CancellationToken cancellationToken = default
    );

    public abstract Task Handle(InboxMessage message);

    public abstract Task Handle(ReconsumedNotification message);
}
