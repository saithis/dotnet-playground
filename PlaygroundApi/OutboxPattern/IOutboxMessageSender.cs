namespace PlaygroundApi.OutboxPattern;

public interface IOutboxMessageSender
{
    Task SendAsync(OutboxMessage message, CancellationToken cancellationToken);
}