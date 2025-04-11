#region License

// <copyright file="Messages.cs" company="zvoove">
//     Copyright (c) 2025 zvoove Group GmbH
// </copyright>

#endregion

using Wolverine.Attributes;

namespace AsyncApiTestApi;


[MessageIdentity(MessageType)]
[AsyncApiMessage(MessageType, Summary = "Internal event for keeping user data in sync on update.")]
[RabbitMqMessageBinding(MessageType = MessageType)]
public record InternalUserEvent
{
    public const string MessageType = "internal-user-event";
    
    public int UserId { get; init; }
}

[MessageIdentity(MessageType)]
[AsyncApiMessage(MessageType, Summary = "User was created.")]
[RabbitMqMessageBinding(MessageType = MessageType)]
public record UserCreated
{
    public const string MessageType = "user-created";
    
    public int UserId { get; init; }
}

[MessageIdentity(MessageType)]
[AsyncApiMessage(MessageType, Summary = "User was updated.")]
[RabbitMqMessageBinding(MessageType = MessageType)]
public record UserUpdated
{
    public const string MessageType = "user-updated";

    public int UserId { get; init; }
}

[MessageIdentity(MessageType)]
[AsyncApiMessage(MessageType, Summary = "User was deleted.")]
[RabbitMqMessageBinding(MessageType = MessageType)]
public record UserDeleted
{
    public const string MessageType = "user-deleted";

    public int UserId { get; init; }
}

[MessageIdentity(MessageType)]
[AsyncApiMessage(MessageType, Summary = "User is requested to be disabled.")]
[RabbitMqMessageBinding(MessageType = MessageType)]
public record DisableUser
{
    public const string MessageType = "disable-user";

    public int UserId { get; init; }
}

[MessageIdentity(MessageType)]
[AsyncApiMessage(MessageType, Summary = "User authorization rules changed.")]
[RabbitMqMessageBinding(MessageType = MessageType)]
public record AuthzChanged
{
    public const string MessageType = "authz-changed";

    public int UserId { get; init; }
}