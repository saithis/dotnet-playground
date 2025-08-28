using FluentValidation;
using FluentValidation.Internal;
using FluentValidation.Validators;
using Microsoft.OpenApi.Models;
using Microsoft.AspNetCore.OpenApi;
using System.Reflection;

namespace OpenApiFluentValidationApi;

/// <summary>
/// A custom schema transformer that enriches the OpenAPI schema with validation rules from FluentValidation.
/// This works by finding the corresponding FluentValidation validator for a given schema and applying the rules.
/// </summary>
internal sealed class FluentValidationSchemaTransformer(IServiceProvider serviceProvider) : IOpenApiSchemaTransformer
{
    /// <summary>
    /// Transforms the OpenAPI schema.
    /// </summary>
    /// <param name="schema">The schema to transform.</param>
    /// <param name="context">The transformation context.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    public Task TransformAsync(OpenApiSchema schema, OpenApiSchemaTransformerContext context, CancellationToken cancellationToken)
    {
        // Skip if we don't have a valid type
        if (context.JsonTypeInfo?.Type == null)
            return Task.CompletedTask;

        // Resolve the validator for the type associated with the schema.
        var validatorType = typeof(IValidator<>).MakeGenericType(context.JsonTypeInfo.Type);
        var validator = serviceProvider.GetService(validatorType) as IValidator;
        
        if (validator == null)
        {
            return Task.CompletedTask;
        }

        try
        {
            // Create a descriptor to inspect the validation rules.
            var descriptor = validator.CreateDescriptor();
            
            // Extract rules using a more robust approach
            ExtractValidationRules(schema, context.JsonTypeInfo.Type, descriptor);
        }
        catch (Exception)
        {
            // If anything goes wrong, fall back to the simple approach
            FallbackValidation(schema, context.JsonTypeInfo.Type, validator);
        }

        return Task.CompletedTask;
    }

    /// <summary>
    /// Extracts validation rules from the FluentValidation descriptor.
    /// </summary>
    private void ExtractValidationRules(OpenApiSchema schema, Type modelType, IValidatorDescriptor descriptor)
    {
        // Get all properties of the model
        var properties = modelType.GetProperties(BindingFlags.Public | BindingFlags.Instance);
        
        foreach (var property in properties)
        {
            var propertyName = property.Name;
            var camelCasePropertyName = ToCamelCase(propertyName);

            if (!schema.Properties.TryGetValue(camelCasePropertyName, out var propertySchema))
            {
                continue;
            }

            // Try to get validation rules for this property using different approaches
            try
            {
                // Approach 1: Try to get rules directly (this might work in some versions)
                var rules = GetRulesForProperty(descriptor, propertyName);
                ApplyRulesToProperty(schema, propertySchema, camelCasePropertyName, rules);
            }
            catch
            {
                // Approach 2: Try reflection to access internal members
                try
                {
                    var rulesFromReflection = GetRulesViaReflection(descriptor, propertyName);
                    ApplyRulesToProperty(schema, propertySchema, camelCasePropertyName, rulesFromReflection);
                }
                catch
                {
                    // Continue to next property if both approaches fail
                }
            }
        }
    }

    /// <summary>
    /// Attempts to get rules for a property using the public API.
    /// </summary>
    private IEnumerable<IValidationRule> GetRulesForProperty(IValidatorDescriptor descriptor, string propertyName)
    {
        // Try different method names that might exist in different FluentValidation versions
        var descriptorType = descriptor.GetType();
        
        // Try GetRulesForMember (older versions)
        var getRulesForMemberMethod = descriptorType.GetMethod("GetRulesForMember", new[] { typeof(string) });
        if (getRulesForMemberMethod != null)
        {
            var result = getRulesForMemberMethod.Invoke(descriptor, new object[] { propertyName });
            if (result is IEnumerable<IValidationRule> rules)
            {
                return rules;
            }
        }

        // Try GetRulesFor (might exist in some versions)
        var getRulesForMethod = descriptorType.GetMethod("GetRulesFor", new[] { typeof(string) });
        if (getRulesForMethod != null)
        {
            var result = getRulesForMethod.Invoke(descriptor, new object[] { propertyName });
            if (result is IEnumerable<IValidationRule> rules)
            {
                return rules;
            }
        }

        return Enumerable.Empty<IValidationRule>();
    }

    /// <summary>
    /// Attempts to get rules using reflection on internal members.
    /// </summary>
    private IEnumerable<IValidationRule> GetRulesViaReflection(IValidatorDescriptor descriptor, string propertyName)
    {
        var descriptorType = descriptor.GetType();
        
        // Try to access internal Rules property
        var rulesProperty = descriptorType.GetProperty("Rules", BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance);
        if (rulesProperty != null)
        {
            var allRules = rulesProperty.GetValue(descriptor) as IEnumerable<IValidationRule>;
            if (allRules != null)
            {
                // Filter rules for the specific property
                return allRules.Where(rule => GetPropertyNameFromRule(rule) == propertyName);
            }
        }

        return Enumerable.Empty<IValidationRule>();
    }

    /// <summary>
    /// Gets the property name from a validation rule using reflection.
    /// </summary>
    private string? GetPropertyNameFromRule(IValidationRule rule)
    {
        try
        {
            var ruleType = rule.GetType();
            var propertyNameProperty = ruleType.GetProperty("PropertyName");
            return propertyNameProperty?.GetValue(rule) as string;
        }
        catch
        {
            return null;
        }
    }

    /// <summary>
    /// Applies validation rules to an OpenAPI property schema.
    /// </summary>
    private void ApplyRulesToProperty(OpenApiSchema parentSchema, OpenApiSchema propertySchema, string propertyName, IEnumerable<IValidationRule> rules)
    {
        foreach (var rule in rules)
        {
            if (rule.Components != null)
            {
                foreach (var component in rule.Components)
                {
                    ApplyValidationRule(parentSchema, propertySchema, propertyName, component.Validator);
                }
            }
        }
    }

    /// <summary>
    /// Applies a specific validation rule to the OpenAPI schema property.
    /// </summary>
    private void ApplyValidationRule(OpenApiSchema parentSchema, OpenApiSchema propertySchema, string propertyName, IPropertyValidator validator)
    {
        switch (validator)
        {
            case INotNullValidator:
            case INotEmptyValidator:
                MakePropertyRequired(parentSchema, propertyName);
                propertySchema.Nullable = false;
                break;

            case ILengthValidator lengthValidator:
                if (lengthValidator.Min > 0)
                    propertySchema.MinLength = lengthValidator.Min;
                if (lengthValidator.Max > 0 && lengthValidator.Max < int.MaxValue)
                    propertySchema.MaxLength = lengthValidator.Max;
                break;

            case IRegularExpressionValidator regexValidator:
                propertySchema.Pattern = regexValidator.Expression;
                break;

            case IBetweenValidator betweenValidator:
                if (IsNumeric(propertySchema.Type))
                {
                    try
                    {
                        propertySchema.Minimum = Convert.ToDecimal(betweenValidator.From);
                        propertySchema.Maximum = Convert.ToDecimal(betweenValidator.To);
                        
                        // Check if it's an exclusive between validator
                        var validatorTypeName = betweenValidator.GetType().Name;
                        var isExclusive = validatorTypeName.Contains("Exclusive", StringComparison.OrdinalIgnoreCase);
                        propertySchema.ExclusiveMinimum = isExclusive;
                        propertySchema.ExclusiveMaximum = isExclusive;
                    }
                    catch
                    {
                        // If conversion fails, skip this validation
                    }
                }
                break;

            case IComparisonValidator comparisonValidator:
                if (IsNumeric(propertySchema.Type) && comparisonValidator.ValueToCompare != null)
                {
                    try
                    {
                        var value = Convert.ToDecimal(comparisonValidator.ValueToCompare);
                        switch (comparisonValidator.Comparison)
                        {
                            case Comparison.GreaterThan:
                                propertySchema.Minimum = value;
                                propertySchema.ExclusiveMinimum = true;
                                break;
                            case Comparison.GreaterThanOrEqual:
                                propertySchema.Minimum = value;
                                propertySchema.ExclusiveMinimum = false;
                                break;
                            case Comparison.LessThan:
                                propertySchema.Maximum = value;
                                propertySchema.ExclusiveMaximum = true;
                                break;
                            case Comparison.LessThanOrEqual:
                                propertySchema.Maximum = value;
                                propertySchema.ExclusiveMaximum = false;
                                break;
                        }
                    }
                    catch
                    {
                        // If conversion fails, skip this validation
                    }
                }
                break;

            case IEmailValidator:
                propertySchema.Format = "email";
                break;

            case ICreditCardValidator:
                propertySchema.Format = "credit-card";
                break;

            default:
                // Try to handle other validators using reflection
                HandleOtherValidators(propertySchema, validator);
                break;
        }
    }

    /// <summary>
    /// Handles other validators that might not be directly accessible.
    /// </summary>
    private void HandleOtherValidators(OpenApiSchema propertySchema, IPropertyValidator validator)
    {
        var validatorType = validator.GetType();
        var validatorName = validatorType.Name;

        // Handle MinimumLength and MaximumLength validators
        if (validatorName.Contains("MinimumLength"))
        {
            try
            {
                var lengthProperty = validatorType.GetProperty("Min") ?? validatorType.GetProperty("Length");
                if (lengthProperty != null)
                {
                    var length = lengthProperty.GetValue(validator);
                    if (length is int intLength)
                    {
                        propertySchema.MinLength = intLength;
                    }
                }
            }
            catch { }
        }
        else if (validatorName.Contains("MaximumLength"))
        {
            try
            {
                var lengthProperty = validatorType.GetProperty("Max") ?? validatorType.GetProperty("Length");
                if (lengthProperty != null)
                {
                    var length = lengthProperty.GetValue(validator);
                    if (length is int intLength)
                    {
                        propertySchema.MaxLength = intLength;
                    }
                }
            }
            catch { }
        }
    }

    /// <summary>
    /// Fallback validation when the main extraction fails.
    /// </summary>
    private void FallbackValidation(OpenApiSchema schema, Type modelType, IValidator validator)
    {
        try
        {
            // Create an instance of the type with default values to test validation
            var instance = Activator.CreateInstance(modelType);
            if (instance != null)
            {
                var validationContext = new ValidationContext<object>(instance);
                var result = validator.Validate(validationContext);
                
                // Mark fields as required based on validation failures
                foreach (var error in result.Errors)
                {
                    if (!string.IsNullOrEmpty(error.PropertyName))
                    {
                        var camelCasePropertyName = ToCamelCase(error.PropertyName);
                        if (schema.Properties.ContainsKey(camelCasePropertyName))
                        {
                            // Mark as required if the error suggests it's required
                            if (error.ErrorMessage.Contains("required", StringComparison.OrdinalIgnoreCase) ||
                                error.ErrorMessage.Contains("empty", StringComparison.OrdinalIgnoreCase))
                            {
                                MakePropertyRequired(schema, camelCasePropertyName);
                                schema.Properties[camelCasePropertyName].Nullable = false;
                            }
                        }
                    }
                }
            }
        }
        catch
        {
            // If even fallback fails, just continue - base OpenAPI generation will still work
        }
    }

    /// <summary>
    /// Marks a property as required in the parent schema's list of required properties.
    /// </summary>
    private void MakePropertyRequired(OpenApiSchema schema, string propertyName)
    {
        schema.Required ??= new HashSet<string>();
        if (!schema.Required.Contains(propertyName))
        {
            schema.Required.Add(propertyName);
        }
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

    /// <summary>
    /// Checks if the OpenAPI schema type is numeric.
    /// </summary>
    private bool IsNumeric(string? schemaType)
    {
        return schemaType == "integer" || schemaType == "number";
    }
}