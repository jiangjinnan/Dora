using Dora.OpenTelemetry.Tracing;
using System.Diagnostics;

var builder = WebApplication.CreateBuilder(args);
builder.Services
    .AddOpenTelemetryTracing(tracing => tracing
        .AddConsoleExporter()
        .ExportToOtlpCollector()
        //.ExportToZipkin()
        .InstrumentAspNetCore()); ;

var app = builder.Build();
var random = new Random();
app.MapGet("/weatherforecast", async (ActivitySource source) =>
{
    using (source.StartActivity("baz")
        ?.AddEvent(new ActivityEvent(
        "log",
        DateTime.UtcNow,
        new ActivityTagsCollection(
            new Dictionary<string, object?>
            {
                { "log.severity", "error" },
                { "log.message", "User not found" },
                { "enduser.id", 123 },
            }
        )
    )))
    {
        await Task.Delay(100);
        using (new SuppressScope())
        {
            using (source.StartActivity("qux"))
            {
                if (random.Next(10) > 5)
                {
                    throw new InvalidOperationException("manually thrown exception");
                }

                await Task.Delay(100);
            }
        }
    }
});

app.Run();