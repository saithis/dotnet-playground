using System.Text.Json;
using ZLogger;

var builder = WebApplication.CreateBuilder(args);

builder.Logging
    .ClearProviders()
    .AddZLoggerConsole(c =>
    {
        c.IncludeScopes = true;
        c.UseJsonFormatter(formatter =>
        {
            formatter.UseUtcTimestamp = true;
        });
    })
    .AddZLoggerConsole(c =>
    {
        c.IncludeScopes = true;
        c.UseJsonFormatter(formatter =>
        {
            formatter.UseUtcTimestamp = true;
            formatter.PropertyKeyValuesObjectName = JsonEncodedText.Encode("Properties");
        });
    });


var app = builder.Build();

app.Use(async (ctx, next) =>
{
    var logger = ctx.RequestServices.GetRequiredService<ILogger<Program>>();
    using var scope = logger.BeginScope("My scope message {MessageScopeVar}", "param of the message");
    using var scope2 = logger.BeginScope(new Dictionary<string, object>
    {
        { "FirstScopedProperty", "first value" },
        { "SecondScopedProperty", 2 },
    });    
    using var scope3 = logger.BeginScope(new Dictionary<string, object>
    {
        { "SingleScopedProperty", "only value" },
    });
    await next();
});

app.MapGet("/", (ILogger<Program> logger) =>
{
    logger.LogError("This is an error for {RequestPath}", "/");
    logger.ZLogError($"This is an error {1}");
    try
    {
        throw new InvalidOperationException("This is an error");
    }
    catch (Exception e)
    {
        logger.LogError(e, "This is an error with value {MyCustomValue}", 1);
    }
    return "Hello World!";
});

app.Run();