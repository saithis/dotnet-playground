using Rebus.Bus;
using Rebus.Config;
using Rebus.Routing.TypeBased;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddRebus(c => c
    .Options(o => o.SetBusName("Rebus Producer"))
    .Routing(r => r.TypeBased().Map<MyMessage>("my-exchange"))
    .Transport(t => t
        .UseRabbitMq("amqp://guest:guest@localhost/", "my-input-queue")
        .ExchangeNames("my-rebus-direct", "my-rebus-topic")
        ));

var app = builder.Build();

app.MapGet("/", async (IBus bus) =>
{
    await bus.Publish(new MyMessage());
    return "Hello World!";
});

app.Run();



// [PublishMessage("my-producer", "my-message")]
// [MessageIdentity("my-message")]
public record MyMessage
{
    public Guid Id { get; init; } = Guid.NewGuid();
}