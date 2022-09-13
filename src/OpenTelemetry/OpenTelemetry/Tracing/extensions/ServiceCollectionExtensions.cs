using Dora.OpenTelemetry;
using Dora.OpenTelemetry.Tracing;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace Microsoft.Extensions.DependencyInjection
{
    public static  class ServiceCollectionExtensions
    {
        public static IServiceCollection AddOpenTelemetryTracing(this IServiceCollection services, Action<TracingBuilder>? setup = null)
        { 
            if (services is null) throw new ArgumentNullException(nameof(services));
            services.AddOptions();
            services.AddLogging();
            services.TryAddSingleton<IActivityListenerFactory, ActivityListenerFactory>();
            services.TryAddSingleton<IActivityProcessor, ActivityProcessor>();
            services.TryAddSingleton<IActivitySourceProvider, ActivitySourceProvider>();
            services.TryAddSingleton<IResourceProvider, ResourceProvider>();
            services.TryAddSingleton<ISampler, AlwaysOnSampler>();
            //services.TryAddSingleton(provider => provider.GetRequiredService<IActivitySourceProvider>().GetActivitySource());
            services.AddSingleton(ActivitySourceProvider.ActivitySourceFactory);
            services.AddOptions<TracingOptions>().Configure<IServiceProvider>((options, provider) =>
            {
                foreach (var instrumentation in provider.GetServices<IInstrumentation>().OfType<IDiagnosticInstrumentation>())
                {
                    options.ActivitySourceNames.Add(instrumentation.ActivitySourceName);
                }
            });
            if (setup is not null)
            {
                setup(new TracingBuilder(services));
            }
            return services;
        }
    }
}
