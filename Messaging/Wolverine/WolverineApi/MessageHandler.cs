using Wolverine;
using WolverineApi.Messages;

public class MessageHandler : IWolverineHandler
{
    public void Handle(SimpleEvent message)
    {
        Console.WriteLine($"Received message {message.Id}");
    }
    public void Handle(ErrorEvent message)
    {
        Console.WriteLine($"Received message {message.Id} but will throw now");
        throw new NotSupportedException("This message is not supported");
    }
}