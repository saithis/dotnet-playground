using Wolverine.Attributes;

namespace WolverineApi.Messages;

[MessageIdentity("error-event")]
public class ErrorEvent
{
    public Guid Id { get; set; }
}