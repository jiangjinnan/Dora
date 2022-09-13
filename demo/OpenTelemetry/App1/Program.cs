using Microsoft.Extensions.DependencyInjection;
using Dora.OpenTelemetry.Tracing;
using App1;
using Microsoft.Extensions.Logging;
using Dora.OpenTelemetry;
using System.Diagnostics;

var serviceProvider = new ServiceCollection()
    .AddHttpClient()
    //.AddLogging(logging => logging.AddConsole())
    .AddOpenTelemetryTracing(tracing => tracing.SetServiceInstance("app1")
        .AddConsoleExporter()
        .AddZipkinExporter()
        )
    .AddSingleton<Invoker>()
    .BuildServiceProvider();

 var invoker = serviceProvider   .GetRequiredService<Invoker>();
var httpClient = new HttpClient();
while (true)
{
    await invoker.InvokeAsync();

}


