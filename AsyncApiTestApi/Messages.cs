#region License
// <copyright file="Messages.cs" company="zvoove">
//     Copyright (c) 2025 zvoove Group GmbH
// </copyright>
#endregion


namespace AsyncApiTestApi;


public record Notification
{
    public const string RoutingKey = "notification";

    public Guid Id { get; init; }
}

public record ReconsumedNotification
{
    public const string RoutingKey = "reconsumed-notification";
    public Guid Id { get; init; }
}

public record InboxMessage
{
    public const string RoutingKey = "inbox-message";
    public Guid Id { get; init; }
}

public record CommandToOtherService
{
    public const string RoutingKey = "command-to-other-service";
    public Guid Id { get; init; }
}
