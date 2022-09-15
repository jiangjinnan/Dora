using App1;
using Dora.OpenTelemetry.Tracing;
using Microsoft.Extensions.DependencyInjection;

var serviceProvider = new ServiceCollection()
    .AddHttpClient()
    .AddOpenTelemetryTracing(tracing => tracing
        .SetServiceInstance("app1")
        .InstrumentHttpClient()
        .AddConsoleExporter()
        .ExportToOtlpCollector()
        //.ExportToZipkin(zipkin=>zipkin.SetSendTimeout(TimeSpan.FromSeconds(1)))
        )
    .AddSingleton<Invoker>()
    .BuildServiceProvider();

 var invoker = serviceProvider.GetRequiredService<Invoker>();
var httpClient = new HttpClient();
while (true)
{
    await invoker.InvokeAsync();
}


