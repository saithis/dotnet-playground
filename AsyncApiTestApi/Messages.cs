#region License
// <copyright file="Messages.cs" company="zvoove">
//     Copyright (c) 2025 zvoove Group GmbH
// </copyright>
#endregion

using Saunter.Attributes;
using Wolverine;
using Wolverine.Attributes;

namespace AsyncApiTestApi;


[MessageIdentity(Notification.RoutingKey)]
public record Notification : IMessage
{
    public const string RoutingKey = "notification";

    public Guid Id { get; init; }
}

[MessageIdentity(RoutingKey)]
public record ReconsumedNotification : IMessage
{
    public const string RoutingKey = "reconsumed-notification";
    public Guid Id { get; init; }
}

[MessageIdentity(InboxMessage.RoutingKey)]
public record InboxMessage : IMessage
{
    public const string RoutingKey = "inbox-message";
    public Guid Id { get; init; }
}

[MessageIdentity(RoutingKey)]
public record CommandToOtherService : IMessage
{
    public const string RoutingKey = "command-to-other-service";
    public Guid Id { get; init; }
}
