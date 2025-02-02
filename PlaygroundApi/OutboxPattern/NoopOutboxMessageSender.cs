using System.Text.Json;

namespace PlaygroundApi.OutboxPattern;

public class NoopOutboxMessageSender(ILogger<NoopOutboxMessageSender> logger) : IOutboxMessageSender
{
    public Task SendAsync(OutboxMessageEntity messageEntity, CancellationToken cancellationToken)
    {
        logger.LogInformation("Fake-Sending outbox message: {Message}", JsonSerializer.Serialize(messageEntity));
        return Task.CompletedTask;
    }
}