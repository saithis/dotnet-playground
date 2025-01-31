using System.ComponentModel.DataAnnotations.Schema;

namespace PlaygroundApi.OutboxPattern;

public class OutboxMessage
{
    public Guid Id { get; set; }
    public required string Content { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset? ProcessedAt { get; set; }
}