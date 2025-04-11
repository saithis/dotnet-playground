#region License
// <copyright file="UserChangedPublisher.cs" company="zvoove">
//     Copyright (c) 2025 zvoove Group GmbH
// </copyright>
#endregion

namespace AsyncApiTestApi;

// Keep the same UserSignedUpEvent, UserEventPublisher, UserEventSubscriber
// classes with their attributes as shown in the previous example.
// e.g.:
[AsyncApiInfo("User Service API", "1.0.1", Description = "API for user events.")]
[AsyncApiServer("production-rabbitmq", "amqp://user:pass@rabbitmq.example.com:5672/", Description = "Production RabbitMQ Broker")]

[AsyncApiMessage("userSignedUpEvent", Summary = "Event published when a user signs up.")]
[RabbitMqMessageBinding(MessageType = "UserSignedUp.v1")]
public class UserSignedUpEvent { /* ... */ }

public class UserEventPublisher
{
    [AsyncApiChannel("user.events", Description = "Channel for all user related events.")]
    [RabbitMqChannelBinding(Is = "routingKey", ExchangeName = "user-exchange", ExchangeType = "topic")]
    public void DefineChannel() { }


    [AsyncApiOperation("user.events", AsyncApiAction.Send, typeof(UserSignedUpEvent), OperationId = "sendUserSignedUpEvent")]
    [RabbitMqOperationBinding(RoutingKey = "user.signedup.emea")]
    public Task PublishUserSignedUpAsync(UserSignedUpEvent eventData) { /* ... */ return Task.CompletedTask;}
}
// ... etc.