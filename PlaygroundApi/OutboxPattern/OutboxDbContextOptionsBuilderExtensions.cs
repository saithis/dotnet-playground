using Microsoft.EntityFrameworkCore;

namespace PlaygroundApi.OutboxPattern;

public static class OutboxDbContextOptionsBuilderExtensions
{
    public static DbContextOptionsBuilder RegisterOutbox<TDbContext>(this DbContextOptionsBuilder builder,
        IServiceProvider serviceProvider)
        where TDbContext : IOutboxDbContext
    {
        var outboxProcessor = serviceProvider.GetRequiredService<OutboxProcessor<TDbContext>>();
        var timeProvider = serviceProvider.GetRequiredService<TimeProvider>();
        var interceptor = new OutboxTriggerInterceptor<TDbContext>(outboxProcessor, timeProvider);
        return builder.AddInterceptors(interceptor);
    }
}