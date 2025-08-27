using Wolverine;
using WolverineApi.Messages;

public class MessageHandler : IWolverineHandler
{
    public void Handle(SimpleEvent message)
    {
        Console.WriteLine($"Received message {message.Id}");
    }
}