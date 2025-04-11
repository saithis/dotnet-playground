using System;
using System.Collections.Generic;

// --- Core AsyncAPI Attributes ---

public enum AsyncApiAction { Send, Receive }

[AttributeUsage(AttributeTargets.Assembly | AttributeTargets.Class, AllowMultiple = false)]
public class AsyncApiInfoAttribute : Attribute
{
    public string Title { get; }
    public string Version { get; }
    public string Description { get; set; }

    public AsyncApiInfoAttribute(string title, string version)
    {
        Title = title;
        Version = version;
    }
}

[AttributeUsage(AttributeTargets.Class | AttributeTargets.Assembly, AllowMultiple = true)]
public class AsyncApiServerAttribute : Attribute
{
    public string Name { get; }
    public string Url { get; }
    public string Protocol { get; } = "amqp"; // Default for RabbitMQ
    public string Description { get; set; }

    public AsyncApiServerAttribute(string name, string url)
    {
        Name = name;
        Url = url;
    }
}

[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = true)]
public class AsyncApiChannelAttribute : Attribute
{
    public string ChannelId { get; } // e.g., "user.signedup.queue" or "user.events.exchange"
    public string Description { get; set; }

    public AsyncApiChannelAttribute(string channelId)
    {
        ChannelId = channelId;
    }
}

[AttributeUsage(AttributeTargets.Method | AttributeTargets.Class, AllowMultiple = true)]
public class AsyncApiOperationAttribute : Attribute
{
    public string ChannelId { get; }
    public AsyncApiAction Action { get; } // Send (Publish) or Receive (Subscribe)
    public string OperationId { get; set; }
    public string Summary { get; set; }
    public Type MessagePayloadType { get; } // Link to the message DTO

    public AsyncApiOperationAttribute(string channelId, AsyncApiAction action, Type messagePayloadType)
    {
        ChannelId = channelId;
        Action = action;
        MessagePayloadType = messagePayloadType;
    }
}

[AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
public class AsyncApiMessageAttribute : Attribute
{
    public string MessageId { get; } // Unique ID for referencing, e.g., "userSignedUpEvent"
    public string Name { get; set; } // Optional descriptive name
    public string ContentType { get; set; } = "application/json";
    public string Summary { get; set; }

    public AsyncApiMessageAttribute(string messageId)
    {
        MessageId = messageId;
    }
}

// --- RabbitMQ Binding Attributes ---

[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = false)] // Often applied alongside AsyncApiChannel
public class RabbitMqChannelBindingAttribute : Attribute
{
    /// <summary>Is this binding for a queue or exchange?</summary>
    public string Is { get; set; } = "routingKey"; // or 'queue'
    public string ExchangeName { get; set; }
    public string ExchangeType { get; set; } = "topic"; // e.g., direct, topic, fanout, headers
    public bool ExchangeDurable { get; set; } = true;
    public bool ExchangeAutoDelete { get; set; } = false;
    public string QueueName { get; set; }
    public bool QueueDurable { get; set; } = true;
    public bool QueueExclusive { get; set; } = false;
    public bool QueueAutoDelete { get; set; } = false;
    // Add Vhost if needed: public string Vhost { get; set; } = "/";
}

[AttributeUsage(AttributeTargets.Method | AttributeTargets.Class, AllowMultiple = false)] // Often applied alongside AsyncApiOperation
public class RabbitMqOperationBindingAttribute : Attribute
{
    ///<summary>The routing key for publishing or binding.</summary>
    public string RoutingKey { get; set; }
    public int? Priority { get; set; } // Message priority
    public bool Mandatory { get; set; } // For publishes
    public bool Ack { get; set; } = true; // For consumes (whether ack is expected)
    // Add other relevant RabbitMQ operation properties (e.g., deliveryMode, bcc, cc)
}

[AttributeUsage(AttributeTargets.Class, AllowMultiple = false)] // Often applied alongside AsyncApiMessage
public class RabbitMqMessageBindingAttribute : Attribute
{
    public string ContentEncoding { get; set; } // e.g., "gzip"
    public string MessageType { get; set; } // Application-specific message type header
    // Add other relevant message properties (e.g., correlationId, replyTo)
}