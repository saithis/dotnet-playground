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

builder.Services.AddScoped<MessageBrokerEvents, MessageBrokerEventsV2>();

builder.UseWolverine(opts =>
{
    RabbitMqTransportExpression rabbit = opts.UseRabbitMq(r =>
    {
        r.HostName = "localhost";
        r.UserName = "guest";
        r.Password = "guest";
    });

    rabbit.DeclareExchange(MessageBrokerEvents.PublicExchangeName, x => { x.ExchangeType = ExchangeType.Topic; }).AutoProvision();

    rabbit.DeclareQueue(MessageBrokerEvents.ConsumeQueue, c =>
        {
            c.BindExchange(MessageBrokerEvents.PublicExchangeName, "#"); // read all our messages again for testing stuff
        })
        .AutoProvision();

    opts.ListenToRabbitQueue(MessageBrokerEvents.ConsumeQueue)
        .ProcessInline();

    opts.Discovery.CustomizeMessageDiscovery(c => { c.Includes.WithAttribute<MessageIdentityAttribute>(); });

    opts.PublishMessage<Notification>().ToRabbitRoutingKey(MessageBrokerEvents.PublicExchangeName, Notification.RoutingKey);
    opts.PublishMessage<ReconsumedNotification>().ToRabbitRoutingKey(MessageBrokerEvents.PublicExchangeName, Notification.RoutingKey);
    opts.PublishMessage<InboxMessage>().ToRabbitRoutingKey(MessageBrokerEvents.PublicExchangeName, Notification.RoutingKey);
    opts.PublishMessage<CommandToOtherService>().ToRabbitRoutingKey(MessageBrokerEvents.PublicExchangeName, Notification.RoutingKey);

    // This will disable the conventional local queue routing that would take precedence over other conventional routing
    opts.Policies.DisableConventionalLocalRouting();
});
builder.Services.AddAsyncApiGeneration(config => 
    // config
    //     .WithMarkupType<MessageBrokerEventsV3>()        
    //     .UseDefaultV3DocumentConfiguration(asyncApi =>
    //     {
    //         asyncApi.WithServer("rabbitmq", server =>
    //         {
    //             server
    //                 .WithHost("localhost:5672")
    //                 .WithProtocol(AsyncApiProtocol.Amqp)
    //                 .WithDescription("RabbitMQ server");
    //         });
    //     }));
    config
        .WithMarkupType<MessageBrokerEventsV2>() 
        .UseDefaultV2DocumentConfiguration(asyncApi =>
        {
            // https://github.com/asyncapi/net-sdk?tab=readme-ov-file#asyncapi-v2
            asyncApi.WithServer("rabbitmq", server =>
            {
                server
                    .WithUrl(new Uri("amqp://localhost:5672"))
                    .WithProtocol(AsyncApiProtocol.Amqp)
                    .WithBinding(new AmqpServerBindingDefinition())
                    .WithDescription("RabbitMQ server");
            });
            // asyncApi.WithChannel(MessageBrokerEvents.PublicExchangeName, channel =>
            // {
            //     channel
            //         .WithDescription("Public exchange for outgoing events")
            //         .WithBinding(new AmqpChannelBindingDefinition()
            //         {
            //             Exchange = new AmqpExchangeDefinition()
            //             {
            //                 Durable = true,
            //                 Type = AmqpExchangeType.Topic,
            //                 Name = MessageBrokerEvents.PublicExchangeName,
            //             },
            //             Type = AmqpChannelType.RoutingKey,
            //         });
            // });
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



