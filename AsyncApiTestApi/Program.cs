#region License

// <copyright file="Program.cs" company="zvoove">
//     Copyright (c) 2025 zvoove Group GmbH
// </copyright>

#endregion

using AsyncApiTestApi;
using Wolverine;
using Wolverine.Attributes;
using Wolverine.RabbitMQ;
using Wolverine.RabbitMQ.Internal;

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

builder.Services.AddScoped<MessageBrokerEvents>();

builder.UseWolverine(opts =>
{
    RabbitMqTransportExpression rabbit = opts.UseRabbitMq(r =>
    {
        r.HostName = "localhost";
        r.UserName = "guest";
        r.Password = "guest";
    });

    rabbit.DeclareExchange(MessageBrokerEvents.PublicExchangeName, x => { x.ExchangeType = ExchangeType.Topic; }).AutoProvision();
    rabbit.DeclareExchange(MessageBrokerEvents.InternalExchangeName, x => { x.ExchangeType = ExchangeType.Topic; }).AutoProvision();

    rabbit.DeclareQueue(MessageBrokerEvents.InboxQueue, c =>
        {
            c.IsDurable = true;
        })
        .AutoProvision();

    rabbit.DeclareQueue(MessageBrokerEvents.SubscribeQueue, c =>
        {
            c.BindExchange(MessageBrokerEvents.InternalExchangeName, "#"); // read all our messages again for testing stuff
            c.BindExchange(MessageBrokerEvents.AuthzExchangeName, AuthzChanged.MessageType);
        })
        .AutoProvision();

    opts.ListenToRabbitQueue(MessageBrokerEvents.InboxQueue)
        .ProcessInline();
    opts.ListenToRabbitQueue(MessageBrokerEvents.SubscribeQueue)
        .ProcessInline();

    opts.Discovery.CustomizeMessageDiscovery(c => { c.Includes.WithAttribute<MessageIdentityAttribute>(); });

    opts.PublishMessage<UserCreated>().ToRabbitRoutingKey(MessageBrokerEvents.PublicExchangeName, UserCreated.MessageType);
    opts.PublishMessage<UserUpdated>().ToRabbitRoutingKey(MessageBrokerEvents.PublicExchangeName, UserUpdated.MessageType);
    opts.PublishMessage<UserDeleted>().ToRabbitRoutingKey(MessageBrokerEvents.PublicExchangeName, UserDeleted.MessageType);
    
    // Technically this are not out events, but for testing we publish them anyway
    rabbit.DeclareExchange(MessageBrokerEvents.AuthzExchangeName, x => { x.ExchangeType = ExchangeType.Topic; }).AutoProvision();
    opts.PublishMessage<AuthzChanged>().ToRabbitRoutingKey(MessageBrokerEvents.AuthzExchangeName, AuthzChanged.MessageType);
    opts.PublishMessage<DisableUser>().ToRabbitQueue(MessageBrokerEvents.InboxQueue);

    // This will disable the conventional local queue routing that would take precedence over other conventional routing
    opts.Policies.DisableConventionalLocalRouting();
});

WebApplication app = builder.Build();

app.MapGet("/", () => "Hello World!");

app.MapGet("/send", async (MessageBrokerEvents bus) =>
{
    await bus.PublishUserCreatedAsync(new UserCreated() { UserId = 1 });
    await bus.PublishUserUpdatedAsync(new UserUpdated() { UserId = 1 });
    await bus.PublishUserDeletedAsync(new UserDeleted() { UserId = 1 });
    await bus.PublishInternalUserEventAsync(new InternalUserEvent() { UserId = 2 });
    return TypedResults.Ok("Messages sent");
});

app.Run();



