#region License

// <copyright file="Program.cs" company="zvoove">
//     Copyright (c) 2025 zvoove Group GmbH
// </copyright>

#endregion

using System.Net.Mime;
using AsyncApiTestApi;
using Saunter;
using Saunter.AsyncApiSchema.v2;
using SlimMessageBus;
using SlimMessageBus.Host;
using SlimMessageBus.Host.AsyncApi;
using SlimMessageBus.Host.RabbitMQ;
using SlimMessageBus.Host.Serialization.Json;

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

builder.Services.AddScoped<MessageBrokerEvents>();

builder.Services.AddSlimMessageBus(mbb =>
{
    mbb.WithProviderRabbitMQ(cfg  =>
    {
        cfg.ConnectionString = "amqp://guest:guest@localhost/";

        // Fine tune the underlying RabbitMQ.Client:
        cfg.ConnectionFactory.ClientProvidedName = $"MyService_{Environment.MachineName}";
        
        // All exchanges declared on producers will be durable by default
        cfg.UseExchangeDefaults(durable: true);
        
        cfg.UseDeadLetterExchangeDefaults(durable: false, autoDelete: false, exchangeType: ExchangeType.Direct, routingKey: string.Empty);
        cfg.UseQueueDefaults(durable: true);
        
        // All messages will get the ContentType message property assigned
        cfg.UseMessagePropertiesModifier((m, p) =>
        {
            p.ContentType = MediaTypeNames.Application.Json;
        });

        cfg.UseTopologyInitializer((channel, applyDefaultTopology) =>
        {
            channel.ExchangeDelete(MessageBrokerEvents.PublicExchangeName, false);
            channel.QueueDelete(MessageBrokerEvents.ConsumeQueue, false, false);
            
            
            // apply default SMB inferred topology
            applyDefaultTopology();
        });
    });
    
    mbb.AddServicesFromAssemblyContaining<MessageBrokerEvents>();
    mbb.AddJsonSerializer();
    mbb.AddAspNet();
    mbb.AddAsyncApiDocumentGenerator();

    RegisterPublicEvent<Notification>(Notification.RoutingKey);
    RegisterPublicEvent<ReconsumedNotification>(ReconsumedNotification.RoutingKey);
    RegisterPublicEvent<InboxMessage>(InboxMessage.RoutingKey);
    RegisterPublicEvent<CommandToOtherService>(CommandToOtherService.RoutingKey);
    
    RegisterSubscribe<ReconsumedNotification, MessageBrokerEvents>(ReconsumedNotification.RoutingKey, MessageBrokerEvents.PublicExchangeName, ReconsumedNotification.RoutingKey);
    RegisterInbox<InboxMessage, MessageBrokerEvents>(InboxMessage.RoutingKey, MessageBrokerEvents.ConsumeQueue);
    
    void RegisterSubscribe<TMessage, TConsumer>(string messageId, string fromExchange, string? routingKey = null)
        where TMessage : class
        where TConsumer : class, IConsumer<TMessage>
    {
        mbb.Consume<TMessage>(x => x
            // Use the subscriber queue, do not auto delete
            .Queue(MessageBrokerEvents.ConsumeQueue, autoDelete: false)
            .ExchangeBinding(fromExchange, routingKey)
            // The queue declaration in RabbitMQ will have a reference to the dead letter exchange and the DL exchange will be created
            .DeadLetterExchange($"{MessageBrokerEvents.ConsumeQueue}-dlq", exchangeType: ExchangeType.Direct)
            .WithConsumer<TConsumer>());
    }
    
    void RegisterInbox<TMessage, TConsumer>(string messageId, string queueName)
        where TMessage : class
        where TConsumer : class, IConsumer<TMessage>
    {
        mbb.Consume<TMessage>(x => x
            // Use the subscriber queue, do not auto delete
            .Queue(queueName, autoDelete: false, durable: true)
            .Path(queueName)
            // The queue declaration in RabbitMQ will have a reference to the dead letter exchange and the DL exchange will be created
            .DeadLetterExchange($"{MessageBrokerEvents.ConsumeQueue}-dlq", exchangeType: ExchangeType.Direct)
            .WithConsumer<TConsumer>());
    }
    
    void RegisterPublicEvent<T>(string messageId)
        where T : class
    {
        mbb.Produce<T>(x => x
            // Will declare an orders exchange of type Fanout
            .Exchange(MessageBrokerEvents.PublicExchangeName, exchangeType: ExchangeType.Topic)
            // Will use a routing key provider that for a given message will take it's Id field
            .RoutingKeyProvider((m, p) => messageId)
            // Will use
            .MessagePropertiesModifier((m, p) =>
            {
                p.MessageId = messageId;
            }));
    }
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
builder.Services.AddHttpContextAccessor();
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



