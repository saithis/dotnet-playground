using Wolverine.Attributes;

namespace WolverineApi.Messages;

[MessageIdentity("simple-event")]
public class SimpleEvent
{
    public Guid Id { get; set; }
}