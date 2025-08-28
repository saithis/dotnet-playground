using FluentValidation;
using OpenApiFluentValidationApi;
using SharpGrip.FluentValidation.AutoValidation.Endpoints.Extensions;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddOpenApi(c =>
{
    c.AddFluentValidationRules();
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

app.Run();