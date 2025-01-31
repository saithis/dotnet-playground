using System.Text.Json;

namespace PlaygroundApi.OutboxPattern;

public class NoopOutboxMessageSender(ILogger<NoopOutboxMessageSender> logger) : IOutboxMessageSender
{
    public Task SendAsync(OutboxMessage message, CancellationToken cancellationToken)
    {
        logger.LogInformation("Fake-Sending outbox message: {Message}", JsonSerializer.Serialize(message));
        return Task.CompletedTask;
    }
}