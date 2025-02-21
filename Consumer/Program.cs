#region License

// <copyright file="Program.cs" company="zvoove">
//     Copyright (c) 2025 zvoove Group GmbH
// </copyright>

#endregion

using Wolverine;
using Wolverine.Attributes;
using Wolverine.RabbitMQ;
using WolverineConventions;

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

builder.UseWolverine(opts =>
{
    var rabbit = opts.UseRabbitMq(r =>
    {
        r.HostName = "localhost";
        r.UserName = "guest";
        r.Password = "guest";
    });
    
    rabbit.DeclareQueue("test.listen-all", c =>
    {
        c.BindExchange("my-producer", "#");
    }).AutoProvision();
    
    opts.ListenToRabbitQueue("test.listen-all")
        .ProcessInline();

    opts.Discovery.CustomizeMessageDiscovery(c => { c.Includes.WithAttribute<PublishMessageAttribute>(); });

    // This will disable the conventional local queue routing that would take precedence over other conventional routing
    opts.Policies.DisableConventionalLocalRouting();
});
WebApplication app = builder.Build();

app.MapGet("/", () => "Hello World!");

app.Run();

public class Handler : IWolverineHandler
{
    public void Handle(MyMessage message)
    {
        Console.WriteLine($"Received message {message.Id}");
    }

    public void Handle(MyMessage2 message)
    {
        Console.WriteLine($"Received message {message.Id}");
    }

    public void Handle(MyOtherMessage message)
    {
        Console.WriteLine($"Received message {message.Id}");
    }
}

[PublishMessage("my-producer", "my-message")]
[MessageIdentity("my-messagexxx")]
public record MyMessage : IMessage
{
    public Guid Id { get; init; }
}

[PublishMessage("my-producer", "my-message2")]
[MessageIdentity("my-message2")]
public record MyMessage2 : IMessage
{
    public Guid Id { get; init; }
}

[PublishMessage("my-producer2", "my-other-message")]
[MessageIdentity("my-other-message")]
public record MyOtherMessage : IMessage
{
    public Guid Id { get; init; }
}
