#region License

// <copyright file="Program.cs" company="zvoove">
//     Copyright (c) 2025 zvoove Group GmbH
// </copyright>

#endregion

using AsyncApiTestApi;
using Neuroglia.AsyncApi;
using Neuroglia.AsyncApi.Bindings.Amqp;
using Neuroglia.Data.Schemas.Json;
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

    rabbit.DeclareQueue(MessageBrokerEvents.InboxQueue, c =>
        {
            c.IsDurable = true;
        })
        .AutoProvision();

    rabbit.DeclareQueue(MessageBrokerEvents.SubscribeQueue, c =>
        {
            c.BindExchange(MessageBrokerEvents.PublicExchangeName, "#"); // read all our messages again for testing stuff
        })
        .AutoProvision();

    opts.ListenToRabbitQueue(MessageBrokerEvents.InboxQueue)
        .ProcessInline();
    opts.ListenToRabbitQueue(MessageBrokerEvents.SubscribeQueue)
        .ProcessInline();

    opts.Discovery.CustomizeMessageDiscovery(c => { c.Includes.WithAttribute<MessageIdentityAttribute>(); });

    opts.PublishMessage<Notification>().ToRabbitRoutingKey(MessageBrokerEvents.PublicExchangeName, Notification.RoutingKey);
    opts.PublishMessage<ReconsumedNotification>().ToRabbitRoutingKey(MessageBrokerEvents.PublicExchangeName, Notification.RoutingKey);
    opts.PublishMessage<CommandToOtherService>().ToRabbitRoutingKey(MessageBrokerEvents.PublicExchangeName, Notification.RoutingKey);

    // This will disable the conventional local queue routing that would take precedence over other conventional routing
    opts.Policies.DisableConventionalLocalRouting();
});
builder.Services.AddAsyncApiGeneration(config => 
    config
        .WithMarkupType<MessageBrokerEvents>()        
        .UseDefaultV3DocumentConfiguration(asyncApi =>
        {
            asyncApi
                .WithServer("rabbitmq", server => server
                    .WithHost("localhost:5672")
                    .WithProtocol(AsyncApiProtocol.Amqp)
                    .WithDescription("RabbitMQ server")
                    .WithBinding(new AmqpServerBindingDefinition())
                )
                .WithChannel("test", c => c
                    .WithServer("#/servers/rabbitmq")
                    .WithTitle("Test Channel")
                    .WithDescription("This is a channel for testing the asyncapi document creation fluent api")
                    .WithAddress(MessageBrokerEvents.PublicExchangeName)
                    .WithBinding(new AmqpChannelBindingDefinition
                    {
                        Exchange = new AmqpExchangeDefinition()
                        {
                            Name = MessageBrokerEvents.PublicExchangeName,
                            Type = AmqpExchangeType.Topic,
                            Durable = true,
                            AutoDelete = false,
                        },
                        Type = AmqpChannelType.RoutingKey,
                    })
                )
                .WithOperation("test-operation", o => o
                    .WithAction(Neuroglia.AsyncApi.v3.V3OperationAction.Send)
                    .WithTitle("test operation")
                    .WithChannel("#/channels/test")
                    .WithBinding(new AmqpOperationBindingDefinition 
                    {
                        DeliveryMode = AmqpDeliveryMode.Persistent
                    })
                    // .WithMessage("#/components/messages/message1") // TODO: why doesn't this work?!?
                )
                .WithMessageComponent("message1", a => a
                    .WithName("message1")
                    .WithTitle("Test Message")
                    .WithDescription("This is a test message")
                    .WithContentType("application/json")
                    .WithPayloadSchema(s => s.WithSchema("{ PropertyA: 213}"))
                );
            
            
        }));
builder.Services.AddRazorPages();
builder.Services.AddAsyncApiUI();
builder.Services.AddSingleton<IJsonSchemaResolver, JsonSchemaResolver>();
builder.Services.AddHttpClient();

WebApplication app = builder.Build();
app.UseStaticFiles();
app.UseRouting();
app.MapAsyncApiDocuments();
app.MapRazorPages();

app.MapGet("/", () => "Hello World!");

app.MapGet("/send", async (MessageBrokerEvents bus) =>
{
    await bus.PublishNotificationAsync(new Notification { Id = Guid.NewGuid() });
    await bus.PublishReconsumedNotificationAsync(new ReconsumedNotification { Id = Guid.NewGuid() });
    await bus.PublishCommandToOtherServiceAsync(new CommandToOtherService { Id = Guid.NewGuid() });
    return TypedResults.Ok("Messages sent");
});

app.Run();



