using System.Globalization;
using System.Reflection;
using System.Text;
using RabbitMQ.Client;
using Wolverine;
using Wolverine.RabbitMQ.Internal;

namespace WolverineApi;

public class MyMessageMapper(MyMessageMapper.Options? options = null) : IRabbitMqEnvelopeMapper
{
    public class Options
    {
        public Action<Envelope, IBasicProperties>? PostMapEnvelopeToOutgoing { get; set; }
        public Action<Envelope, IReadOnlyBasicProperties>? PostMapIncomingToEnvelope { get; set; }
    }

    public void MapEnvelopeToOutgoing(Envelope envelope, IBasicProperties outgoing)
    {
        outgoing.AppId = envelope.Source;
        outgoing.Timestamp = new AmqpTimestamp(envelope.SentAt.ToUnixTimeSeconds());
        outgoing.Type = envelope.MessageType;
        outgoing.MessageId = envelope.Id.ToString();
        outgoing.ContentType = envelope.ContentType;
        outgoing.CorrelationId = envelope.CorrelationId;
        if (envelope.DeliverBy.HasValue)
        {
            var ttl = Convert.ToInt32(envelope.DeliverBy.Value.Subtract(DateTimeOffset.Now).TotalMilliseconds);
            outgoing.Expiration = ttl.ToString(CultureInfo.InvariantCulture);
        }

        outgoing.Headers ??= new Dictionary<string, object?>();
        outgoing.Headers["traceparent"] = envelope.ParentId; // RabbitMQ OTEL default
        outgoing.Headers["cloudEvents_traceparent"] = envelope.ParentId; // CloudEvents default

        options?.PostMapEnvelopeToOutgoing?.Invoke(envelope, outgoing);
    }

    public void MapIncomingToEnvelope(Envelope envelope, IReadOnlyBasicProperties incoming)
    {
        SetInternalProperty(envelope, nameof(envelope.Source), incoming.AppId);
        SetInternalProperty(
            envelope,
            nameof(envelope.SentAt),
            DateTimeOffset.FromUnixTimeSeconds(incoming.Timestamp.UnixTime));
        envelope.MessageType = incoming.Type;
        envelope.Id = Guid.TryParse(incoming.MessageId, out var id) ? id : Guid.CreateVersion7();
        envelope.ContentType = incoming.ContentType;
        envelope.CorrelationId = incoming.CorrelationId;
        envelope.DeliverBy = incoming.Expiration != null && int.TryParse(incoming.Expiration, out var ttl)
            ? DateTimeOffset.Now.AddMilliseconds(ttl)
            : null;

        var incomingHeaders = incoming.Headers ?? new Dictionary<string, object?>();
        if (incomingHeaders.TryGetValue("traceparent", out var traceparent) // RabbitMQ OTEL default
            || incomingHeaders.TryGetValue("cloudEvents_traceparent", out traceparent) // CloudEvents default
            || incomingHeaders.TryGetValue("parent_id", out traceparent)) // Wolverine default
        {
            envelope.ParentId = ConvertToString(traceparent);
        }

        options?.PostMapIncomingToEnvelope?.Invoke(envelope, incoming);
    }

    public IEnumerable<string> AllHeaders()
    {
        yield break;
    }

    /// <remarks>Can be removed if this is fixed: https://github.com/JasperFx/wolverine/issues/1587</remarks>
    private static void SetInternalProperty<T>(Envelope envelope, string propertyName, T value)
    {
        typeof(Envelope).InvokeMember(
            propertyName,
            BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.SetProperty | BindingFlags.Instance,
            null,
            envelope,
            [value],
            CultureInfo.InvariantCulture);
    }

    private static string? ConvertToString(object? value)
    {
        return value switch
        {
            null => null,
            string str => str,
            byte[] bytes => Encoding.UTF8.GetString(bytes),
            _ => value.ToString() ?? string.Empty,
        };
    }

}