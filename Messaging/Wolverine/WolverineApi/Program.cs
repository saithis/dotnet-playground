using Wolverine;
using Wolverine.Http;
using Wolverine.RabbitMQ;
using Wolverine.RabbitMQ.Internal;
using WolverineApi;
using WolverineApi.Messages;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddWolverineHttp();
builder.Services.AddWolverine(opts =>
{
    opts.ServiceName = "WolverineApi";
    
    opts.UseRabbitMq(r =>
    {
        r.HostName = "localhost";
        r.UserName = "guest";
        r.Password = "guest";
    })
    .AutoProvision()
    .CustomizeDeadLetterQueueing(new DeadLetterQueue("wolverine-api.wolverine-dead-letter-queue"))
    .ConfigureListeners(l =>
    {
        l.DeadLetterQueueing(new DeadLetterQueue($"{l.QueueName}.dlq"));
    })
    .DisableSystemRequestReplyQueueDeclaration()
    .DeclareExchange("wolverine-api.events", e =>
    {
        e.ExchangeType = ExchangeType.Topic;
        e.IsDurable = true;
        e.BindQueue("wolverine-api.subscriptions", "#");
    })
    .DeclareQueue("wolverine-api.subscriptions", q =>
    {
        q.IsDurable = true;
        q.QueueType = QueueType.quorum;
    })
    .ConfigureSenders(c => c.UseInterop(new MyMessageMapper()))
    .ConfigureListeners(c => c.UseInterop(new MyMessageMapper()));

    opts.PublishMessage<SimpleEvent>()
        .ToRabbitRoutingKey("wolverine-api.events", "simple-event");

    opts.ListenToRabbitQueue("wolverine-api.subscriptions")
        .PreFetchCount(100)
        .ListenerCount(5)
        .CircuitBreaker(cb =>
        {
            // 10% failures will cause the listener to pause
            cb.FailurePercentageThreshold = 10;
            cb.PauseTime = TimeSpan.FromMinutes(1);
        });
    
    // This will disable the conventional local queue routing that would take precedence over other conventional routing
    opts.Policies.DisableConventionalLocalRouting();
});
var app = builder.Build();

app.MapGet("/", () => "Hello World!");

app.MapGet("/event", async (IMessageBus bus) =>
{   
    await bus.PublishAsync(new SimpleEvent()
    {
        Id = Guid.NewGuid(),
    });
});
app.MapDeadLettersEndpoints();

app.Run();