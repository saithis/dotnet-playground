
using PlaygroundApi.OutboxPattern;

namespace PlaygroundApi.Events;

[Message("note-added", "my-topic", "my-route-key")]
public class NoteAddedEvent : IMessage
{
    public required int Id { get; init; }
    public required string Text { get; init; }
}