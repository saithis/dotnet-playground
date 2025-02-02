
using PlaygroundApi.OutboxPattern;

namespace PlaygroundApi.Events;

public class NoteAddedEvent : IMessage
{
    public required int Id { get; init; }
    public required string Text { get; init; }
}