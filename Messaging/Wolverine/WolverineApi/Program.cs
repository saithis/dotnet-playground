using Microsoft.EntityFrameworkCore;
using Wolverine;
using Wolverine.EntityFrameworkCore;
using Wolverine.Http;
using Wolverine.RabbitMQ;
using Wolverine.RabbitMQ.Internal;
using Wolverine.SqlServer;
using WolverineApi;
using WolverineApi.Database;
using WolverineApi.Messages;

var builder = WebApplication.CreateBuilder(args);

var connectionString = "Server=localhost;Database=my_db;User Id=sa;Password=soSecurePw1!;Trust Server Certificate=True";

// Register a DbContext or multiple DbContext types as normal
builder.Services.AddDbContext<SampleDbContext>(
    x => x.UseSqlServer(connectionString), 
    
    // This is actually a significant performance gain
    // for Wolverine's sake
    optionsLifetime:ServiceLifetime.Singleton);

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
        .ToRabbitRoutingKey("wolverine-api.events", "simple-event")
        .UseDurableOutbox();

    opts.PublishMessage<ErrorEvent>()
        .ToRabbitRoutingKey("wolverine-api.events", "error-event")
        .UseDurableOutbox();

    opts.ListenToRabbitQueue("wolverine-api.subscriptions")
        .UseDurableInbox()
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
    
    
    // You'll need to independently tell Wolverine where and how to 
    // store messages as part of the transactional inbox/outbox
    opts.PersistMessagesWithSqlServer(connectionString);
    
    // Adding EF Core transactional middleware, saga support,
    // and EF Core support for Wolverine storage operations
    opts.UseEntityFrameworkCoreTransactions();
});
var app = builder.Build();

var scope = app.Services.CreateScope();
await scope.ServiceProvider.GetRequiredService<SampleDbContext>().Database.MigrateAsync();

app.MapGet("/", () => "Hello World!");

app.MapGet("/event", async (IMessageBus bus) =>
{   
    await bus.PublishAsync(new SimpleEvent()
    {
        Id = Guid.NewGuid(),
    });
});

app.MapGet("/event/error", async (IDbContextOutbox<SampleDbContext> outbox) =>
{
    var item = new Item
    {
        Name = Guid.NewGuid().ToString(),
    };
    outbox.DbContext.Items.Add(item);
    await outbox.PublishAsync(new ErrorEvent()
    {
        Id = item.Id,
    });
    await outbox.SaveChangesAndFlushMessagesAsync();
});

app.MapDeadLettersEndpoints();

app.Run();