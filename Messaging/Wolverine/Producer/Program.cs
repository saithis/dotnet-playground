#region License

// <copyright file="Program.cs" company="zvoove">
//     Copyright (c) 2025 zvoove Group GmbH
// </copyright>

#endregion

using Wolverine;
using Wolverine.Attributes;
using Wolverine.EntityFrameworkCore;
using WolverineConventions;

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

builder.UseWolverine(opts =>
{
    opts.ApplyZvooveConventions(new ZvooveConventionOptions
    {
        AssemblytoScan = typeof(MyMessage).Assembly,
        ConfigureRabbitMq = r =>
        {
            r.HostName = "localhost";
            r.UserName = "guest";
            r.Password = "guest";
        },
    });

    // This will disable the conventional local queue routing that would take precedence over other conventional routing
    opts.Policies.DisableConventionalLocalRouting();
});
WebApplication app = builder.Build();

app.MapGet("/", () => "Hello World!");

app.MapGet("/send", async (IMessageBus bus, IDbContextOutbox outbox) =>
{
    var message1 = new MyMessage { Id = Guid.NewGuid() };
    var message2 = new MyMessage2 { Id = Guid.NewGuid() };
    var message3 = new MyOtherMessage { Id = Guid.NewGuid() };
    await bus.PublishAsync(message1);
    await bus.PublishAsync(message2);
    await bus.PublishAsync(message3);
    return new
    {
        message1, message2, message3,
    };
});

app.Run();

[RabbitExchange("my-producer", "my-message")]
[MessageIdentity("my-message")]
public record MyMessage : IMessage
{
    public Guid Id { get; init; }
}

[RabbitExchange("my-producer", "my-message2")]
[MessageIdentity("my-message2")]
public record MyMessage2 : IMessage
{
    public Guid Id { get; init; }
}

[RabbitExchange("my-producer2", "my-other-message")]
[MessageIdentity("my-other-message")]
public record MyOtherMessage : IMessage
{
    public Guid Id { get; init; }
}
