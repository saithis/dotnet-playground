#region License

// <copyright file="Messages.cs" company="zvoove">
//     Copyright (c) 2025 zvoove Group GmbH
// </copyright>

#endregion

using Neuroglia.AsyncApi.v2;
using Wolverine;
using Wolverine.Attributes;

namespace AsyncApiTestApi;

[MessageIdentity(RoutingKey)]
[Message(Title = "Notification", Description = "This is a notification about something", Name = RoutingKey)]
public record Notification : IMessage
{
    public const string RoutingKey = "notification";

    public Guid Id { get; init; }
}

[MessageIdentity(RoutingKey)]
[Message(Title = "Reconsumed Notification", Description = "This is a notification that this service itself will consume again", Name = RoutingKey)]
public record ReconsumedNotification : IMessage
{
    public const string RoutingKey = "reconsumed-notification";
    public Guid Id { get; init; }
}

[MessageIdentity(RoutingKey)]
[Message(Title = "Inbox message", Description = "This is a message, that others send us for cunsumption", Name = RoutingKey)]
public record InboxMessage : IMessage
{
    public const string RoutingKey = "inbox-message";
    public Guid Id { get; init; }
}

[MessageIdentity(RoutingKey)]
[Message(Title = "Command to other service", Description = "Some description for the command", Name = RoutingKey)]
public record CommandToOtherService : IMessage
{
    public const string RoutingKey = "command-to-other-service";
    public Guid Id { get; init; }
}
