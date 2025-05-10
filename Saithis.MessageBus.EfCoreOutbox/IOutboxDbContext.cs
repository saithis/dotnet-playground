namespace Saithis.MessageBus.EfCoreOutbox;

public interface IOutboxDbContext
{
    // TODO: use dotnet 10 extension properties
    public OutboxStagingCollection OutboxMessages { get; }
}