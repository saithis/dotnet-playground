using System.Text.Json;
using ZLogger;
using ZLogger.Formatters;

var builder = WebApplication.CreateBuilder(args);

builder.Logging.AddZLoggerConsole(c =>
{
    c.IncludeScopes = true;
    c.CaptureThreadInfo = true;
    c.UseJsonFormatter(formatter =>
    {
        formatter.UseUtcTimestamp = true;
        
        // Category and ScopeValues is manually write in AdditionalFormatter at labels so remove from include properties.
        formatter.IncludeProperties = IncludeProperties.All;

        formatter.JsonPropertyNames = JsonPropertyNames.Default with
        {
            LogLevel =  JsonEncodedText.Encode("severity"),
            LogLevelNone = JsonEncodedText.Encode("default"),
            LogLevelTrace = JsonEncodedText.Encode("trace"),
            LogLevelDebug = JsonEncodedText.Encode("debug"),
            LogLevelInformation = JsonEncodedText.Encode("info"),
            LogLevelWarning = JsonEncodedText.Encode("warn"),
            LogLevelError = JsonEncodedText.Encode("error"),
            LogLevelCritical = JsonEncodedText.Encode("crit"),

            Message = JsonEncodedText.Encode("message"),
            Timestamp = JsonEncodedText.Encode("timestamp"),
            
        };

        formatter.PropertyKeyValuesObjectName = JsonEncodedText.Encode("jsonPayload");

        // cache JsonEncodedText outside of AdditionalFormatter
        var labels = JsonEncodedText.Encode("logging.googleapis.com/labels");
        var category = JsonEncodedText.Encode("category");
        var eventId = JsonEncodedText.Encode("eventId");
        var userId = JsonEncodedText.Encode("userId");

        formatter.AdditionalFormatter = (Utf8JsonWriter writer, in LogInfo logInfo) =>
        {
            writer.WriteStartObject(labels);
            writer.WriteString(category, logInfo.Category.JsonEncoded);
            writer.WriteString(eventId, logInfo.EventId.Name);

            if (logInfo.ScopeState != null && !logInfo.ScopeState.IsEmpty)
            {
                foreach (var item in logInfo.ScopeState.Properties)
                {
                    if (item.Key == "userId")
                    {
                        writer.WriteString(userId, item.Value!.ToString());
                        break;
                    }
                }
            }
            writer.WriteEndObject();
        };
    });
});


var app = builder.Build();

app.Use(async (ctx, next) =>
{
    var logger = ctx.RequestServices.GetRequiredService<ILogger<Program>>();
    using var scope = logger.BeginScope("My scope {Var}", 1);
    using var scope2 = logger.BeginScope("My scope 2 {Var}", 2);
    await next();
});

app.MapGet("/", (ILogger<Program> logger) =>
{
    logger.LogError("This is an error");
    logger.ZLogError($"This is an error {1}");
    return "Hello World!";
});

app.Run();