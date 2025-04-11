
using System.Reflection;
using ByteBard.AsyncAPI.Models;
using ByteBard.AsyncAPI.Models.Interfaces;
using Newtonsoft.Json.Schema; // RabbitMQ bindings models

// Assuming custom attributes from previous example are defined in scope

public class AsyncApiGenerator
{
    public AsyncApiDocument Generate(params Assembly[] assemblies)
    {
        // Use AsyncAPI.NET's document model
        var doc = new AsyncApiDocument
        {
             Asyncapi = "3.0.0" // Explicitly set v3
             // Initialize collections
             // Servers = new Dictionary<string, Server>(), // Added below
             // Channels = new Dictionary<string, ChannelItem>(), // Added below
             // Operations = new Dictionary<string, Operation>(), // Added below
             // Components = new Components { ... } // Added below
        };

        var allTypes = assemblies.SelectMany(a => a.GetLoadableTypes()).ToList(); // Helper extension method recommended

        // 1. Process Global Info
        var infoAttr = assemblies.Select(a => a.GetCustomAttribute<AsyncApiInfoAttribute>()).FirstOrDefault(a => a != null);
        doc.Info = new AsyncApiInfo()
        {
            Title = infoAttr?.Title ?? "Default API Title",
            Version = infoAttr?.Version ?? "1.0.0",
            Description = infoAttr?.Description
        };

        // 2. Process Servers
        doc.Servers = new Dictionary<string, AsyncApiServer>();
        var serverAttrs = assemblies.SelectMany(a => a.GetCustomAttributes<AsyncApiServerAttribute>());
         foreach (var serverAttr in serverAttrs)
         {
             if (!doc.Servers.ContainsKey(serverAttr.Name))
             {
                 // Using AsyncAPI.NET's Server model
                 doc.Servers.Add(serverAttr.Name, new AsyncApiServer {
                      Url = serverAttr.Url, // Library expects full URL here usually
                      Protocol = serverAttr.Protocol,
                      Description = serverAttr.Description
                      // Add bindings if needed via serverAttr properties and map to ServerBindings
                 });
             }
         }


        // 3. Initialize Components
        doc.Components = new AsyncApiComponents
        {
            Messages = new Dictionary<string, AsyncApiMessage>(),
            Schemas = new Dictionary<string, AsyncApiJsonSchema>()
            // Initialize other component types if used (SecuritySchemes, etc.)
        };


        // 4. Discover Messages and Schemas
        var schemaGenerator = new JsonSchemaGenerator();

        var messageTypes = allTypes.Where(t => t.GetCustomAttribute<AsyncApiMessageAttribute>() != null);
        foreach (var type in messageTypes)
        {
            var msgAttr = type.GetCustomAttribute<AsyncApiMessageAttribute>();
            var msgRbMqAttr = type.GetCustomAttribute<RabbitMqMessageBindingAttribute>();
            string schemaId = type.Name; // Use type name as schema ID convention

            // Generate JSON Schema using NJsonSchema
            if (!doc.Components.Schemas.ContainsKey(schemaId))
            {
                var nJsonSchema = schemaGenerator.Generate(type);
                // Convert NJsonSchema to AsyncApi.Models.Schema
                // This might involve JSON serialization/deserialization or manual mapping
                // depending on AsyncAPI.NET's Schema object structure.
                // Simplified approach: serialize NJsonSchema to Dictionary
                var schemaJson = nJsonSchema.ToString();
                var schemaDict = System.Text.Json.JsonSerializer.Deserialize<Dictionary<string, object>>(schemaJson);
                doc.Components.Schemas.Add(schemaId, new AsyncApiJsonSchema(schemaDict)); // Use AsyncAPI.NET Schema
            }

            // Create Message using AsyncAPI.NET's Message model
            if (!doc.Components.Messages.ContainsKey(msgAttr.MessageId))
            {
                var message = new AsyncApiMessage
                {
                    // MessageId is the key in the dictionary, not usually a field in AsyncAPI v3 message object itself unless using traits
                    Name = msgAttr.Name ?? type.Name,
                    Title = msgAttr.Summary, // Map Summary to Title
                    Summary = msgAttr.Summary,
                    ContentType = msgAttr.ContentType,
                    Payload = new AsyncApiSchemaReference($"#/components/schemas/{schemaId}") // Use SchemaReference
                };

                // Add RabbitMQ Message Binding using AsyncAPI.NET binding model
                if (msgRbMqAttr != null)
                {
                    var rbmqBinding = new AsyncApiRabbitMQMessageBinding
                    {
                        ContentEncoding = msgRbMqAttr.ContentEncoding,
                        MessageType = msgRbMqAttr.MessageType,
                        BindingVersion = "0.2.0" // Specify binding version
                        // Map other relevant properties from attribute if they exist
                    };
                    message.Bindings = new AsyncApiMessageBindings { Reference = null, Bindings = new Dictionary<string, IMessageBinding>() };
                    message.Bindings.Bindings.Add("rabbitmq", rbmqBinding);
                }
                doc.Components.Messages.Add(msgAttr.MessageId, message);
            }
        }

        // 5. Discover Channels
        doc.Channels = new Dictionary<string, AsyncApiChannel>(); // Use ChannelItem for v3
        var channelDefs = allTypes
            .SelectMany(t => t.GetMethods(BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static | BindingFlags.DeclaredOnly))
            .Select(m => new { Method = m, Attr = m.GetCustomAttribute<AsyncApiChannelAttribute>(), BindAttr = m.GetCustomAttribute<RabbitMqChannelBindingAttribute>() })
            .Where(x => x.Attr != null)
             .Concat(allTypes // Also check class level
                 .Select(t => new { Type = t, Attr = t.GetCustomAttribute<AsyncApiChannelAttribute>(), BindAttr = t.GetCustomAttribute<RabbitMqChannelBindingAttribute>() })
                 .Where(x => x.Attr != null)
                 .Select(x=> new { Method = (MethodInfo)null, Attr = x.Attr, BindAttr = x.BindAttr }) // Dummy MethodInfo
                 );


        foreach (var chDef in channelDefs)
        {
            if (!doc.Channels.ContainsKey(chDef.Attr.ChannelId))
            {
                 // Using AsyncAPI.NET's ChannelItem model
                var channelItem = new AsyncApiChannel
                {
                    Address = chDef.Attr.ChannelId, // Set channel address (new in v3)
                    Description = chDef.Attr.Description
                    // 'Messages' will be populated by Operations referencing this channel
                    // 'Operations' are now top-level in v3, linked back to channels.
                };

                // Add RabbitMQ Channel Binding
                if (chDef.BindAttr != null)
                {
                    var rbmqBinding = MapRabbitMqChannelBinding(chDef.BindAttr);
                    channelItem.Bindings = new ChannelBindings { Reference = null, Bindings = new Dictionary<string, IChannelBinding>() };
                    channelItem.Bindings.Bindings.Add("rabbitmq", rbmqBinding);
                }
                doc.Channels.Add(chDef.Attr.ChannelId, channelItem);
            }
        }

        // 6. Discover Operations (Top Level for v3)
        doc.Operations = new Dictionary<string, Operation>();
         var operationDefs = allTypes
            .SelectMany(t => t.GetMethods(BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static | BindingFlags.DeclaredOnly))
            .Select(m => new { Method = m, Attr = m.GetCustomAttribute<AsyncApiOperationAttribute>(), BindAttr = m.GetCustomAttribute<RabbitMqOperationBindingAttribute>() })
            .Where(x => x.Attr != null)
             .Concat(allTypes // Also check class level
                 .Select(t => new { Type = t, Attr = t.GetCustomAttribute<AsyncApiOperationAttribute>(), BindAttr = t.GetCustomAttribute<RabbitMqOperationBindingAttribute>() })
                 .Where(x => x.Attr != null)
                 .Select(x=> new { Method = (MethodInfo)null, Attr = x.Attr, BindAttr = x.BindAttr }) // Dummy MethodInfo
                 );

         foreach(var opDef in operationDefs)
         {
            string operationId = opDef.Attr.OperationId ?? GenerateOperationId(opDef.Attr.Action, opDef.Attr.ChannelId); // Use helper

            if (!doc.Operations.ContainsKey(operationId))
            {
                var msgAttr = opDef.Attr.MessagePayloadType?.GetCustomAttribute<AsyncApiMessageAttribute>();
                if (msgAttr == null) {
                     Console.Error.WriteLine($"Warning: Message payload type {opDef.Attr.MessagePayloadType?.Name} for operation {operationId} is missing [AsyncApiMessage] attribute. Skipping message reference.");
                     continue;
                 }

                 // Check if channel exists
                 if (!doc.Channels.ContainsKey(opDef.Attr.ChannelId)) {
                     Console.Error.WriteLine($"Warning: Channel '{opDef.Attr.ChannelId}' referenced by operation '{operationId}' not found via [AsyncApiChannel] attribute. Skipping operation.");
                     continue;
                 }

                 // Using AsyncAPI.NET's Operation model
                 var operation = new AsyncApiOperation
                 {
                     Action = opDef.Attr.Action == AsyncApiAction.Send ? OperationAction.Send : OperationAction.Receive,
                     Channel = new AsyncApiChannelReference($"#/channels/{opDef.Attr.ChannelId}"), // Link to channel via reference
                     Summary = opDef.Attr.Summary ?? opDef.Method?.Name,
                     // Link messages processed by this operation
                     Messages = new List<IMessageReference> { new MessageReference($"#/components/messages/{msgAttr.MessageId}") }
                 };

                 // Add RabbitMQ Operation Binding
                 if (opDef.BindAttr != null)
                 {
                     var rbmqBinding = MapRabbitMqOperationBinding(opDef.BindAttr);
                     operation.Bindings = new OperationBindings { Reference = null, Bindings = new Dictionary<string, IOperationBinding>() };
                     operation.Bindings.Bindings.Add("rabbitmq", rbmqBinding);
                 }

                 doc.Operations.Add(operationId, operation);

                // Optional: Add Message Reference to ChannelItem if needed (though v3 focuses on Ops->Msg)
                // Ensure ChannelItem.Messages exists and add the reference if not present
                /*
                if (doc.Channels.TryGetValue(opDef.Attr.ChannelId, out var channelItem)) {
                    if (channelItem.Messages == null) {
                        channelItem.Messages = new Dictionary<string, IMessageReference>();
                    }
                    if (!channelItem.Messages.ContainsKey(msgAttr.MessageId)) {
                        channelItem.Messages.Add(msgAttr.MessageId, new MessageReference($"#/components/messages/{msgAttr.MessageId}"));
                    }
                }
                */
            }
         }

        // 7. Cleanup (Optional - remove empty collections if library doesn't handle it)
        // The library's serialization might handle omitting empty collections automatically. Check its behavior.


        return doc;
    }

    // --- Helper to generate Operation ID ---
    private string GenerateOperationId(AsyncApiAction action, string channelId)
    {
        // Simple ID generation, make more robust if needed
        var actionStr = action.ToString().ToLowerInvariant();
        var channelStr = channelId.Replace('.', '_').Replace('/', '_'); // Sanitize channel
        return $"{actionStr}_{channelStr}";
    }


    // --- Helper Methods for Mapping Attributes to AsyncAPI.NET Binding Models ---

    private RabbitMQChannelBinding MapRabbitMqChannelBinding(RabbitMqChannelBindingAttribute attr)
    {
        if (attr == null) return null;

        var binding = new RabbitMQChannelBinding
        {
            Is = attr.Is == "queue" ? RabbitMQChannelBindingIs.Queue : RabbitMQChannelBindingIs.RoutingKey,
            BindingVersion = "0.2.0"
        };

        if (!string.IsNullOrEmpty(attr.ExchangeName))
        {
            binding.Exchange = new RabbitMQChannelBindingExchange
            {
                Name = attr.ExchangeName,
                Type = attr.ExchangeType, // Assuming type names match (e.g., "topic", "direct")
                Durable = attr.ExchangeDurable,
                AutoDelete = attr.ExchangeAutoDelete,
                // VHost = attr.Vhost // Map if needed
            };
        }

        if (!string.IsNullOrEmpty(attr.QueueName))
        {
            binding.Queue = new RabbitMQChannelBindingQueue
            {
                Name = attr.QueueName,
                Durable = attr.QueueDurable,
                Exclusive = attr.QueueExclusive,
                AutoDelete = attr.QueueAutoDelete,
                 // VHost = attr.Vhost // Map if needed
            };
        }
        return binding;
    }

    private RabbitMQOperationBinding MapRabbitMqOperationBinding(RabbitMqOperationBindingAttribute attr)
    {
        if (attr == null) return null;
        return new RabbitMQOperationBinding
        {
            RoutingKeys = !string.IsNullOrEmpty(attr.RoutingKey) ? new List<string> { attr.RoutingKey } : null, // Library expects a list
            Ack = attr.Ack,
            Priority = attr.Priority,
            Mandatory = attr.Mandatory,
            BindingVersion = "0.2.0"
            // Map other properties like DeliveryMode, Bcc, Cc if present in attribute and library model
        };
    }
}

// Helper Extension method (place in a static class)
public static class AssemblyExtensions
{
    public static IEnumerable<Type> GetLoadableTypes(this Assembly assembly)
    {
        if (assembly == null) throw new ArgumentNullException(nameof(assembly));
        try
        {
            return assembly.GetTypes();
        }
        catch (ReflectionTypeLoadException e)
        {
            Console.Error.WriteLine($"Warning: Could not load all types from assembly {assembly.FullName}. Error: {e.Message}");
            return e.Types.Where(t => t != null); // Return the types that could be loaded
        }
    }
}