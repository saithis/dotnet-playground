#region License
// <copyright file="Messages.cs" company="zvoove">
//     Copyright (c) 2025 zvoove Group GmbH
// </copyright>
#endregion

using Wolverine;
using Wolverine.Attributes;

namespace AsyncApiTestApi;


[MessageIdentity(Notification.RoutingKey)]
[Neuroglia.AsyncApi.v2.Message(Title = "Notification", Description = "This is a notification about something")]
[Neuroglia.AsyncApi.v3.Message(Title = "Notification title", Description = "This is a notification about something", Name = RoutingKey, Summary = "summary",
    ContentType = "application/json")]
public record Notification : IMessage
{
    public const string RoutingKey = "notification";

    public Guid Id { get; init; }
}

[MessageIdentity(RoutingKey)]
[Neuroglia.AsyncApi.v2.Message(Title = "Reconsumed Notification", Description = "This is a notification that this service itself will consume again")]
public record ReconsumedNotification : IMessage
{
    public const string RoutingKey = "reconsumed-notification";
    public Guid Id { get; init; }
}

[MessageIdentity(InboxMessage.RoutingKey)]
[Neuroglia.AsyncApi.v2.Message(Title = "Inbox message", Description = "This is a message, that others send us for cunsumption")]
public record InboxMessage : IMessage
{
    public const string RoutingKey = "inbox-message";
    public Guid Id { get; init; }
}

[MessageIdentity(RoutingKey)]
[Neuroglia.AsyncApi.v2.Message(Title = "Command to other service", Description = "Some description for the command")]
public record CommandToOtherService : IMessage
{
    public const string RoutingKey = "command-to-other-service";
    public Guid Id { get; init; }
}
