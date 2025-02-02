using Microsoft.EntityFrameworkCore;

namespace PlaygroundApi.OutboxPattern;

public interface IOutboxDbContext
{
    ICollection<IMessage> OutboxMessages { get; } // TODO: use custom class, that wraps list and only exposes a public add method
}