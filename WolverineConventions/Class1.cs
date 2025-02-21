using System.Reflection;
using RabbitMQ.Client;
using Wolverine;
using Wolverine.Configuration;
using Wolverine.RabbitMQ;
using Wolverine.RabbitMQ.Internal;
using ExchangeType = Wolverine.RabbitMQ.ExchangeType;

namespace WolverineConventions;

[AttributeUsage(AttributeTargets.Class)]
public class PublishMessageAttribute(string exchangeName, string? routingKey = null) : Attribute
{
    public string ExchangeName { get; set; } = exchangeName;
    public string? RoutingKey { get; set; } = routingKey;
}

public static class WolverineHelpers
{
    private static readonly MethodInfo publishMessageMethodInfo =
        typeof(WolverineOptions).GetMethod(nameof(WolverineOptions.PublishMessage))!;

    public static PublishingExpression PublishMessage(this WolverineOptions options, Type type)
    {
        MethodInfo generic = publishMessageMethodInfo.MakeGenericMethod(type);
        object? result = generic.Invoke(options, []);
        return (result as PublishingExpression)!;
    }

    public static IEnumerable<Type> GetTypesWithAttribute(Assembly assembly, Type attributeType)
    {
        foreach (Type type in assembly.GetTypes())
        {
            if (type.GetCustomAttributes(attributeType, true).Length > 0)
            {
                yield return type;
            }
        }
    }

    public static WolverineOptions ApplyZvooveConventions(this WolverineOptions opts, ZvooveConventionOptions zopts)
    {
        RabbitMqTransportExpression rabbit = opts.UseRabbitMq(zopts.ConfigureRabbitMq)
            .AutoProvision();

        HashSet<string> exchanges = new();
        IEnumerable<Type> messageTypes = GetTypesWithAttribute(zopts.AssemblytoScan, typeof(PublishMessageAttribute));
        foreach (Type messageType in messageTypes)
        {
            var publishInfo = messageType.GetCustomAttribute<PublishMessageAttribute>()!;
            exchanges.Add(publishInfo.ExchangeName);

            if (publishInfo.RoutingKey != null)
            {
                opts.PublishMessage(messageType).ToRabbitRoutingKey(publishInfo.ExchangeName, publishInfo.RoutingKey);
            }
            else
            {
                opts.PublishMessage(messageType).ToRabbitExchange(publishInfo.ExchangeName);
            }
        }

        foreach (string exchange in exchanges)
        {
            rabbit.DeclareExchange(exchange, x => { x.ExchangeType = ExchangeType.Topic; });
        }

        return opts;
    }
}

public class ZvooveConventionOptions
{
    public required Assembly AssemblytoScan { get; set; }
    public required Action<ConnectionFactory> ConfigureRabbitMq { get; set; }
}