using Microsoft.AspNetCore.OpenApi;

namespace OpenApiFluentValidationApi;

/// <summary>
/// Provides extension methods for integrating FluentValidation with Microsoft.AspNetCore.OpenApi.
/// </summary>
public static class FluentValidationOpenApiExtensions
{
    
    /// <summary>
    /// Adds a schema transformer that incorporates FluentValidation rules into the OpenAPI document.
    /// </summary>
    /// <param name="options">The <see cref="OpenApiOptions"/> to configure.</param>
    /// <returns>The configured <see cref="OpenApiOptions"/>.</returns>
    public static OpenApiOptions AddFluentValidationRules(this OpenApiOptions options)
    {
        // This registers our custom schema transformer.
        // It's a singleton because it doesn't hold state besides the IServiceProvider.
        options.AddSchemaTransformer<FluentValidationSchemaTransformer>();
        return options;
    }
}