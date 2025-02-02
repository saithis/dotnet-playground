using System.ComponentModel.DataAnnotations;
using System.Text.Json;

namespace PlaygroundApi.OutboxPattern;

public class OutboxMessageEntity
{
    public Guid Id { get; private set; }
    [MaxLength(256)]
    public required string RouteKey { get; init; }
    [MaxLength(256)]
    public required string Topic { get; init; }
    [MaxLength(256)]
    public required string Name { get; init; }
    public required string Content { get; init; }
    public required DateTimeOffset CreatedAt { get; init; }
    public DateTimeOffset? ProcessedAt { get; private set; }
    public short? ErrorCount { get; private set; }
    [MaxLength(2000)]
    public string? Error { get; private set; }

    private OutboxMessageEntity(){}
    public static OutboxMessageEntity Create(IMessage message, TimeProvider timeProvider)
    {
        var messageType = message.GetType();
        
        // TODO: this should be a compile failure if possible
        if(Attribute.GetCustomAttribute(messageType, typeof (MessageAttribute)) is not MessageAttribute messageAttribute)
            throw new NullReferenceException($"MessageAttribute missing on type {messageType}");

        return new OutboxMessageEntity
        {
            Topic = messageAttribute.Topic,
            RouteKey = messageAttribute.RouteKey,
            Name = messageAttribute.Name,
            // TODO: make serializer overwriteable
            Content = JsonSerializer.Serialize<object>(message),
            CreatedAt = timeProvider.GetUtcNow(),
        };
    }

    public void MarkAsProcessed(TimeProvider timeProvider)
    {
        ProcessedAt = timeProvider.GetUtcNow();
    }
}