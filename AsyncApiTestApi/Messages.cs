#region License

// <copyright file="Messages.cs" company="zvoove">
//     Copyright (c) 2025 zvoove Group GmbH
// </copyright>

#endregion

using Neuroglia.AsyncApi.v3;
using Wolverine;
using Wolverine.Attributes;

namespace AsyncApiTestApi;


[MessageIdentity(MessageType)]
[Message(
    Name = MessageType, 
    Title = "Internal event for updating the user", 
    Summary = "Internal event for keeping user data in sync on update.",
    ContentType = "application/json"
    )]
public record InternalUserEvent
{
    public const string MessageType = "internal-user-event";
    
    public int UserId { get; init; }
}

[MessageIdentity(MessageType)]
[Message(
    Name = MessageType, 
    Title = "User created event", 
    Summary = "User was created.",
    ContentType = "application/json"
)]
public record UserCreated
{
    public const string MessageType = "user-created";
    
    public int UserId { get; init; }
}

[MessageIdentity(MessageType)]
[Message(
    Name = MessageType, 
    Title = "User updated event", 
    Summary = "User was updated.",
    ContentType = "application/json"
)]
public record UserUpdated
{
    public const string MessageType = "user-updated";

    public int UserId { get; init; }
}

[MessageIdentity(MessageType)]
[Message(
    Name = MessageType, 
    Title = "User deleted event", 
    Summary = "User was deleted.",
    ContentType = "application/json"
)]
public record UserDeleted
{
    public const string MessageType = "user-deleted";

    public int UserId { get; init; }
}

[MessageIdentity(MessageType)]
[Message(
    Name = MessageType, 
    Title = "User disable command", 
    Summary = "User is requested to be disabled.",
    ContentType = "application/json"
)]
public record DisableUser
{
    public const string MessageType = "disable-user";

    public int UserId { get; init; }
}

[MessageIdentity(MessageType)]
[Message(
    Name = MessageType, 
    Title = "Authorization changed event", 
    Summary = "User authorization rules changed.",
    ContentType = "application/json"
)]
public record AuthzChanged
{
    public const string MessageType = "authz-changed";

    public int UserId { get; init; }
}