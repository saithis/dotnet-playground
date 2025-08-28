using FluentValidation;
using OpenApiFluentValidationApi;
using SharpGrip.FluentValidation.AutoValidation.Endpoints.Extensions;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddOpenApi(c =>
{
    c.AddFluentValidationRules();
    c.AddObsoluteAttributes();
});
builder.Services.AddValidatorsFromAssemblyContaining<RegistrationRequestValidator>();
builder.Services.AddEndpointsApiExplorer();
    
var app = builder.Build();

app.MapOpenApi();

app.MapGet("/", () => "Hello World!");

// Define the API endpoint that uses the validated DTO.
app.MapPost("/register", (RegistrationRequest request) =>
    {
        return Results.Ok($"Registration successful for user: {request.Username}");
    })
    .AddFluentValidationAutoValidation()
    .WithOpenApi();

// New endpoint demonstrating UserProfile with obsolete properties
app.MapPost("/users/profile", (UserProfile profile) =>
    {
        return Results.Ok($"Profile updated for: {profile.FirstName} {profile.LastName}");
    })
    .AddFluentValidationAutoValidation()
    .WithOpenApi(operation => 
    {
        operation.Summary = "Update user profile";
        operation.Description = "Updates a user profile. Note: Some properties are deprecated.";
        return operation;
    });

// Example endpoint using the obsolete model (this would show the entire schema as deprecated)
app.MapPost("/users/old", (OldUserModel oldUser) =>
    {
        return Results.Ok("This endpoint uses an obsolete model");
    })
    .WithOpenApi(operation => 
    {
        operation.Summary = "Legacy user endpoint (DEPRECATED)";
        operation.Description = "This endpoint is deprecated and should not be used.";
        return operation;
    });

app.Run();