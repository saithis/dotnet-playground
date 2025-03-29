using System.Diagnostics;
using System.Text.Json;
using Paramore.Brighter;
using Paramore.Brighter.Extensions.DependencyInjection;
using Paramore.Brighter.MessagingGateway.RMQ;
using Paramore.Brighter.Transforms.Attributes;
using RabbitMQ.Client;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddSingleton<IAmAMessageTransformAsync, Transformer>();

builder.Services.AddBrighter(c =>
{
    
}).UseExternalBus(c =>
{
    c.ProducerRegistry = new RmqProducerRegistryFactory(
        new RmqMessagingGatewayConnection
        {
            Name = "MyCommandConnection",
            AmpqUri = new AmqpUriSpecification(
                new Uri("amqp://guest:guest@localhost:5672"),
                connectionRetryCount: 5,
                retryWaitInMilliseconds: 250,
                circuitBreakTimeInMilliseconds: 30000),
            Exchange = new Exchange("my-brighter-exchange", durable: true, supportDelay: false,
                type: ExchangeType.Topic),
            DeadLetterExchange = new Exchange("my-brighter-exchange.dlq", durable: true, supportDelay: false,
                type: ExchangeType.Topic),
            Heartbeat = 15,
            PersistMessages = true
        }, [
            new RmqPublication
            {
                Topic = new RoutingKey("my-default-route-key"),
                WaitForConfirmsTimeOutInMilliseconds = 1000,
                MakeChannels = OnMissingChannel.Create,
                RequestType = typeof(MyMessage),
            }
        ]).Create();
})
.AutoFromAssemblies();

var app = builder.Build();

app.MapGet("/", async (IAmACommandProcessor proc) =>
{
    await proc.PublishAsync(new MyMessage());
    await proc.PostAsync(new MyMessage());
    return "Hello World!";
});

app.Run();

public record MyMessage : IEvent
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public Activity Span { get; set; }
}

public class MyMessageMessageMapper : IAmAMessageMapperAsync<MyMessage>
{
    public Message MapToMessage(MyMessage request, Publication publication)
    {
        var header = new MessageHeader(
            messageId: request.Id, 
            topic: new RoutingKey("my-default-route-key"), 
            messageType: MessageType.MT_EVENT); 
        var payload = JsonSerializer.Serialize(request, new JsonSerializerOptions(JsonSerializerDefaults.General));
        var body = new MessageBody(payload, "application/json", CharacterEncoding.UTF8);
        var message = new Message(header, body);
        message.Header.Bag.Add("MyCustomHeader", request.Span?.Id);
        return message;
    }

    public MyMessage MapToRequest(Message message)
    {
        return JsonSerializer.Deserialize<MyMessage>(message.Body.Value, JsonSerialisationOptions.Options);
    }

    public async Task<Message> MapToMessageAsync(MyMessage request, Publication publication,
        CancellationToken cancellationToken = new CancellationToken())
    {
        return MapToMessage(request, publication);
    }

    public async Task<MyMessage> MapToRequestAsync(Message message, CancellationToken cancellationToken = new CancellationToken())
    {
        return MapToRequest(message);
    }

    public IRequestContext? Context { get; set; }
}

public class Transformer : IAmAMessageTransformAsync
{
    public void Dispose()
    {
        // TODO release managed resources here
    }

    public void InitializeWrapFromAttributeParams(params object[] initializerList)
    {
    }

    public void InitializeUnwrapFromAttributeParams(params object[] initializerList)
    {
    }

    public async Task<Message> WrapAsync(Message message, Publication publication, CancellationToken cancellationToken)
    {
        message.Header.Bag.Add("WrapHeader", DateTimeOffset.UtcNow.ToUnixTimeMilliseconds().ToString());
        return message;
    }

    public async Task<Message> UnwrapAsync(Message message, CancellationToken cancellationToken)
    {
        return message;
    }

    public IRequestContext? Context { get; set; }
}