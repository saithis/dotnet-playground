using System.Text.Json;

namespace PlaygroundApi.OutboxPattern;

public class OutboxMessageEntity
{
    public Guid Id { get; set; }
    public string Type { get; init; }
    public required string Content { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset? ProcessedAt { get; set; }
    public int? ErrorCount { get; init; }
    public string? Error { get; init; }


    public static OutboxMessageEntity Create(IMessage message)
    {
        return new OutboxMessageEntity
        {
            Content = JsonSerializer.Serialize(message),
        };
    }
}