using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Diagnostics;

namespace PlaygroundApi.OutboxPattern;

public class OutboxTriggerInterceptor<TDbContext>(OutboxProcessor<TDbContext> outboxProcessor, TimeProvider timeProvider) 
    : SaveChangesInterceptor where TDbContext : IOutboxDbContext
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

        EntityEntry<OutboxMessage>[] newMessages = context
            .ChangeTracker
            .Entries<OutboxMessage>()
            .Where(e => e.State == EntityState.Added)
            .ToArray();

        foreach (EntityEntry<OutboxMessage> entry in newMessages)
        {
            entry.Entity.CreatedAt = timeProvider.GetUtcNow();
        }

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

        var outboxMessages = eventData.Context?.ChangeTracker.Entries<OutboxMessage>() ?? [];
        if (outboxMessages.Any(e => e.Entity.ProcessedAt == null))
        {
            await outboxProcessor.ScheduleNowAsync();
        }

        return result;
    }
}