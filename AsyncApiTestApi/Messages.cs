#region License

// <copyright file="Messages.cs" company="zvoove">
//     Copyright (c) 2025 zvoove Group GmbH
// </copyright>

#endregion

using Neuroglia.AsyncApi.v3;
using Wolverine;
using Wolverine.Attributes;

namespace AsyncApiTestApi;

[MessageIdentity(RoutingKey)]
[Message(Title = "Notification title", Description = "This is a notification about something", Name = RoutingKey, Summary = "summary",
    ContentType = "application/json")]
public record Notification : IMessage
{
    public const string RoutingKey = "notification";

    public Guid Id { get; init; }
}

[MessageIdentity(RoutingKey)]
[Message(Title = "ReconsumedNotification title", Description = "This is a notification about something", Name = RoutingKey,
    Summary = "summary",
    ContentType = "application/json")]
public record ReconsumedNotification : IMessage
{
    public const string RoutingKey = "reconsumed-notification";
    public Guid Id { get; init; }
}

[MessageIdentity(RoutingKey)]
[Message(Title = "InboxMessage title", Description = "This is a notification about something", Name = RoutingKey, Summary = "summary",
    ContentType = "application/json")]
public record InboxMessage : IMessage
{
    public const string RoutingKey = "inbox-message";
    public Guid Id { get; init; }
}

[MessageIdentity(RoutingKey)]
[Message(Title = "CommandToOtherService title", Description = "This is a notification about something", Name = RoutingKey,
    Summary = "summary",
    ContentType = "application/json")]
public record CommandToOtherService : IMessage
{
    public const string RoutingKey = "command-to-other-service";
    public Guid Id { get; init; }
}
