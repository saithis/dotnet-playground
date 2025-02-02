namespace PlaygroundApi.OutboxPattern;

public interface IOutboxMessageSender
{
    Task SendAsync(OutboxMessageEntity messageEntity, CancellationToken cancellationToken);
}