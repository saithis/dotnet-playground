#region License

// <copyright file="Program.cs" company="zvoove">
//     Copyright (c) 2025 zvoove Group GmbH
// </copyright>

#endregion

using AsyncApiTestApi;
using Saunter;
using Saunter.AsyncApiSchema.v2;
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

// Add Saunter to the application services. 
builder.Services.AddAsyncApiSchemaGeneration(options =>
{
    // Specify example type(s) from assemblies to scan.
    options.AssemblyMarkerTypes = [typeof(MessageBrokerEvents)];
    
    // Build as much (or as little) of the AsyncApi document as you like.
    // Saunter will generate Channels, Operations, Messages, etc, but you
    // may want to specify Info here.
    options.Middleware.UiTitle = "AsyncApi Test Service";
    options.AsyncApi = new AsyncApiDocument
    {
        Info = new Info("AsyncApi Test Service API", "1.0.0")
        {
            Description = "The Smartylighting Streetlights API allows you to remotely manage the city lights.",
            License = new License("MIT")
        },
        Servers =
        {
            ["rabbitmq"] = new Server("localhost:5672", "amqp")
        },
    };
});
builder.Services.AddHttpClient();

WebApplication app = builder.Build();
app.UseRouting();

app.MapAsyncApiDocuments();
app.MapAsyncApiUi();

app.MapGet("/", () => "Hello World!");

app.MapGet("/send", async (MessageBrokerEvents bus) =>
{
    await bus.PublishNotificationAsync(new Notification { Id = Guid.NewGuid() });
    await bus.PublishReconsumedNotificationAsync(new ReconsumedNotification { Id = Guid.NewGuid() });
    await bus.PublishCommandToOtherServiceAsync(new CommandToOtherService { Id = Guid.NewGuid() });
    return TypedResults.Ok("Messages sent");
});

app.Run();



