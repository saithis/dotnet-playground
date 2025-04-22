#region License

// <copyright file="Program.cs" company="zvoove">
//     Copyright (c) 2025 zvoove Group GmbH
// </copyright>

#endregion

using System.Text.Json.Nodes;
using AsyncApiTestApi;
using ByteBard.AsyncAPI;
using ByteBard.AsyncAPI.Bindings.AMQP;
using ByteBard.AsyncAPI.Models;
using ByteBard.AsyncAPI.Models.Interfaces;
using Wolverine;
using Wolverine.Attributes;
using Wolverine.RabbitMQ;
using Wolverine.RabbitMQ.Internal;
using ExchangeType = Wolverine.RabbitMQ.ExchangeType;

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

    rabbit.DeclareQueue(MessageBrokerEvents.InboxQueue, c => { c.IsDurable = true; })
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

app.MapGet("/", () =>
{
    var doc = new AsyncApiDocument
    {
        Asyncapi = "3.0.0", // Explicitly set v3
        // Initialize collections
        Servers = new Dictionary<string, AsyncApiServer>
        {
            {
                "scram-connections", new AsyncApiServer
                {
                    Host = "test.rabbitmq.org:18092",
                    Protocol = "amqp",
                    Description = "RabbitMQ server with SCRAM authentication",
                    Tags = new List<AsyncApiTag>
                    {
                        new()
                        {
                            Name = "env:test-scram",
                            Description = "This environment is meant for running internal tests through\nscramSha256",
                        },
                    },
                }
            },
        },
        Components = new AsyncApiComponents
        {
            Messages = new Dictionary<string, AsyncApiMessage>
            {
                {
                    "internalUserEvent",
                    new AsyncApiMessage
                    {
                        Name = "internalUserEvent",
                        Title = "Internal event for updating the user",
                        Summary = "Internal event for keeping user data in sync on update.",
                        Description = "Internal event for keeping user data in sync on update.",
                        Bindings = new AsyncApiBindings<IMessageBinding>
                        {
                            {
                                "amqp", new AMQPMessageBinding { MessageType = "internal-user-event", ContentEncoding = "application/json" }
                            },
                        },
                        ContentType = "application/json",
                        Payload = new AsyncApiMultiFormatSchema
                        {
                            SchemaFormat = "application/vnd.asyncapi+json;version=2.0.0",
                            Schema = new AsyncApiJsonSchema
                            {
                                Properties = new Dictionary<string, AsyncApiJsonSchema>
                                {
                                    {
                                        "userId", new AsyncApiJsonSchema
                                        {
                                            Type = SchemaType.Integer,
                                            Format = "int32",
                                            Description = "The ID of the user.",
                                        }
                                    },
                                },
                            },
                        },
                        Extensions = new Dictionary<string, IAsyncApiExtension>
                        {
                            { "x-eventcatalog-message-version", new AsyncApiAny(JsonNode.Parse("\"1.0.0\"")) }, // How to do this without parsing?
                        },
                    }
                },
            },
        },
        Channels = new Dictionary<string, AsyncApiChannel>
        {
            {
                "internalExchange", new AsyncApiChannel
                {
                    Description = "Channel for internal communication.",
                    Messages = new Dictionary<string, AsyncApiMessage>
                    {
                        {
                            "internalUserEvent", new AsyncApiMessageReference("#/components/messages/internalUserEvent")
                        },
                    },
                    Extensions = new Dictionary<string, IAsyncApiExtension>
                    {
                        { "x-eventcatalog-channel-version", new AsyncApiAny(JsonNode.Parse("\"1.0.0\"")) }, // How to do this without parsing?
                    },
                }
            },
        },
        Operations = new Dictionary<string, AsyncApiOperation>
        {
            {
                "internal-user-sync-send", new AsyncApiOperation
                {
                    Action = ByteBard.AsyncAPI.Models.AsyncApiAction.Send,
                    Title = "Internal operation to sync user data (send).",
                    Channel = new AsyncApiChannelReference("#/channels/internalExchange"),
                    Messages = new List<AsyncApiMessageReference>
                    {
                        new("#/channels/internalExchange/messages/internalUserEvent"),
                    },
                    Bindings = new AsyncApiBindings<IOperationBinding>
                    {
                        {
                            "amqp", new AMQPOperationBinding
                            {
                                Expiration = 1,
                                Cc = ["internal-user-event"],
                            }
                        },
                    },
                }
            },
        },
    };

    return doc.Serialize(AsyncApiVersion.AsyncApi3_0, AsyncApiFormat.Yaml);
});

app.MapGet("/send", async (MessageBrokerEvents bus) =>
{
    await bus.PublishUserCreatedAsync(new UserCreated { UserId = 1 });
    await bus.PublishUserUpdatedAsync(new UserUpdated { UserId = 1 });
    await bus.PublishUserDeletedAsync(new UserDeleted { UserId = 1 });
    await bus.PublishInternalUserEventAsync(new InternalUserEvent { UserId = 2 });
    return TypedResults.Ok("Messages sent");
});

app.Run();
