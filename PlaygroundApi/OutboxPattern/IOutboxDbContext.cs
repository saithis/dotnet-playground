using Microsoft.EntityFrameworkCore;

namespace PlaygroundApi.OutboxPattern;

public interface IOutboxDbContext
{
    DbSet<OutboxMessage> OutboxMessages { get; set; }
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}