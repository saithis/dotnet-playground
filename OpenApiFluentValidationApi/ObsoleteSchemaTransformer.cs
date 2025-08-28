using Microsoft.AspNetCore.OpenApi;
using Microsoft.OpenApi.Models;
using System.Reflection;

namespace OpenApiFluentValidationApi;

/// <summary>
/// A schema transformer that marks OpenAPI schema elements as deprecated when they have the [Obsolete] attribute.
/// This transformer processes both type-level and property-level obsolete attributes.
/// </summary>
internal sealed class ObsoleteSchemaTransformer : IOpenApiSchemaTransformer
{
    /// <summary>
    /// Transforms the OpenAPI schema to mark obsolete elements as deprecated.
    /// </summary>
    /// <param name="schema">The schema to transform.</param>
    /// <param name="context">The transformation context.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    public Task TransformAsync(OpenApiSchema schema, OpenApiSchemaTransformerContext context, CancellationToken cancellationToken)
    {
        var type = context.JsonTypeInfo.Type;

        // Check if the entire type is marked as obsolete
        HandleTypeObsolete(schema, type);

        // Check individual properties for obsolete attributes
        HandlePropertyObsolete(schema, type);

        return Task.CompletedTask;
    }

    /// <summary>
    /// Handles type-level obsolete attributes.
    /// </summary>
    private void HandleTypeObsolete(OpenApiSchema schema, Type type)
    {
        var obsoleteAttribute = type.GetCustomAttribute<ObsoleteAttribute>();
        if (obsoleteAttribute == null) 
            return;

        // Mark the entire schema as deprecated
        schema.Deprecated = true;

        // Add deprecation information to the description
        var deprecationMessage = CreateDeprecationMessage(obsoleteAttribute);
        if (!string.IsNullOrEmpty(deprecationMessage))
        {
            schema.Description = string.IsNullOrEmpty(schema.Description) 
                ? deprecationMessage 
                : $"{schema.Description}\n\n{deprecationMessage}";
        }
    }

    /// <summary>
    /// Handles property-level obsolete attributes.
    /// </summary>
    private void HandlePropertyObsolete(OpenApiSchema schema, Type type)
    {
        // Get all public instance properties
        var properties = type.GetProperties(BindingFlags.Public | BindingFlags.Instance);

        foreach (var property in properties)
        {
            var obsoleteAttribute = property.GetCustomAttribute<ObsoleteAttribute>();
            if (obsoleteAttribute == null) 
                continue;

            // Convert property name to camelCase to match OpenAPI schema
            var schemaPropertyName = ToCamelCase(property.Name);
            
            if (!schema.Properties.TryGetValue(schemaPropertyName, out var propertySchema)) 
                continue;

            // Mark the property as deprecated
            propertySchema.Deprecated = true;

            // Add deprecation information to the property description
            var deprecationMessage = CreateDeprecationMessage(obsoleteAttribute);
            if (!string.IsNullOrEmpty(deprecationMessage))
            {
                propertySchema.Description = string.IsNullOrEmpty(propertySchema.Description) 
                    ? deprecationMessage 
                    : $"{propertySchema.Description}\n\n{deprecationMessage}";
            }
        }
    }

    /// <summary>
    /// Creates a deprecation message from the ObsoleteAttribute.
    /// </summary>
    private string CreateDeprecationMessage(ObsoleteAttribute obsoleteAttribute)
    {
        var message = "**DEPRECATED**";
        
        if (!string.IsNullOrEmpty(obsoleteAttribute.Message))
        {
            message += $": {obsoleteAttribute.Message}";
        }

        return message;
    }

    /// <summary>
    /// Converts a string to camelCase.
    /// </summary>
    private string ToCamelCase(string name)
    {
        if (string.IsNullOrEmpty(name) || !char.IsUpper(name[0]))
        {
            return name;
        }
        return char.ToLowerInvariant(name[0]) + name[1..];
    }
} 