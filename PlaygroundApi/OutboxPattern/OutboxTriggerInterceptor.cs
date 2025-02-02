using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;

namespace PlaygroundApi.OutboxPattern;

public class OutboxTriggerInterceptor<TDbContext>(OutboxProcessor<TDbContext> outboxProcessor, TimeProvider timeProvider) 
    : SaveChangesInterceptor where TDbContext : DbContext, IOutboxDbContext
{
    public override ValueTask<InterceptionResult<int>> SavingChangesAsync(
        DbContextEventData eventData,
        InterceptionResult<int> result,
        CancellationToken cancellationToken = default
    )
    {
        DbContext? context = eventData.Context;
        if (context == null)
        {
            return ValueTask.FromResult(result);
        }

        if (context is not IOutboxDbContext outboxDbContext)
            throw new InvalidOperationException("Expected IOutboxDbContext");

        foreach (var message in outboxDbContext.OutboxMessages)
        {
            var outboxMessage = OutboxMessageEntity.Create(message, timeProvider);
            context.Set<OutboxMessageEntity>().Add(outboxMessage);
        }
        outboxDbContext.OutboxMessages.Clear();

        return ValueTask.FromResult(result);
    }

    public override async ValueTask<int> SavedChangesAsync(
        SaveChangesCompletedEventData eventData,
        int result,
        CancellationToken cancellationToken = default
    )
    {
        if (eventData.EntitiesSavedCount == 0)
            return result;

        var outboxMessages = eventData.Context?.ChangeTracker.Entries<OutboxMessageEntity>() ?? [];
        if (outboxMessages.Any(e => e.Entity.ProcessedAt == null))
        {
            await outboxProcessor.ScheduleNowAsync();
        }

        return result;
    }
}