using Dora.OpenTelemetry.Tracing;
using System.Diagnostics;

var builder = WebApplication.CreateBuilder(args);
builder.Services
    .AddOpenTelemetryTracing(tracing => tracing.SetServiceInstance("app2")
        .AddConsoleExporter()
        .AddZipkinExporter()
        .InstrumentAspNetCore()
        );

var app = builder.Build();

// Configure the HTTP request pipeline.

var summaries = new[]
{
    "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
};

app.MapGet("/weatherforecast", (ActivitySource source) =>
{
    using (source.StartActivity("qux"))
    {
        var forecast = Enumerable.Range(1, 5).Select(index =>
       new WeatherForecast
       (
           DateTime.Now.AddDays(index),
           Random.Shared.Next(-20, 55),
           summaries[Random.Shared.Next(summaries.Length)]
       ))
        .ToArray();
        return forecast;
    }
});

app.Run();

internal record WeatherForecast(DateTime Date, int TemperatureC, string? Summary)
{
    public int TemperatureF => 32 + (int)(TemperatureC / 0.5556);
}