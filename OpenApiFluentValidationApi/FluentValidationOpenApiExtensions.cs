using Microsoft.AspNetCore.OpenApi;

namespace OpenApiFluentValidationApi;

/// <summary>
/// Provides extension methods for integrating FluentValidation handling with Microsoft.AspNetCore.OpenApi.
/// </summary>
public static class FluentValidationOpenApiExtensions
{
    
    /// <summary>
    /// Adds schema transformers that incorporate FluentValidation rules into the OpenAPI document.
    /// </summary>
    /// <param name="options">The <see cref="OpenApiOptions"/> to configure.</param>
    /// <returns>The configured <see cref="OpenApiOptions"/>.</returns>
    public static OpenApiOptions AddFluentValidationRules(this OpenApiOptions options)
    {
        // Register the FluentValidation schema transformer
        options.AddSchemaTransformer<FluentValidationSchemaTransformer>();
        
        return options;
    }
}