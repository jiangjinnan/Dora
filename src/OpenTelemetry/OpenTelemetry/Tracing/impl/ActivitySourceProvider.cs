using Microsoft.Extensions.DependencyInjection;
using System.Diagnostics;

namespace Dora.OpenTelemetry.Tracing
{
    internal class ActivitySourceProvider : IActivitySourceProvider
    {
        private readonly object _lock = new();
        private readonly Dictionary<string, ActivitySource> _sources = new();
        private readonly IResourceProvider _resourceProvider;
        internal static bool _initialized;
        private readonly IActivityListenerFactory _factory;
        private readonly IEnumerable<IInstrumentation> _instrumentations;

        public readonly static Func<IServiceProvider, ActivitySource> ActivitySourceFactory
            = provider => provider.GetRequiredService<IActivitySourceProvider>().GetActivitySource();

        public ActivitySourceProvider(IResourceProvider resourceProvider, IActivityListenerFactory factory, IEnumerable<IInstrumentation> instrumentations)
        {
            _resourceProvider = resourceProvider ?? throw new ArgumentNullException(nameof(resourceProvider));
            _factory = factory ?? throw new ArgumentNullException(nameof(factory));
            _instrumentations = instrumentations ?? throw new ArgumentNullException(nameof(instrumentations));
        }

        public ActivitySource GetActivitySource(string? name = null, string version= "")
        {
            EnsureInitialized();
            name = name?.ToLowerInvariant()?? _resourceProvider.GetAttributes()[OpenTelemetryDefaults.ResourceAttributes.ServiceName]!.ToString()!;
            if (_sources.TryGetValue(name, out var source))
            {
                return source;
            }
            source = new ActivitySource(name, version);
            return _sources[name] = source;
        }



        private void EnsureInitialized()
        {
            if (_initialized)
            {
                return;
            }

            lock (_lock)
            {
                if (_initialized)
                {
                    return;
                }

                ActivitySource.AddActivityListener(_factory.CreateActivityListener());
                DiagnosticListener.AllListeners.Subscribe(new DiagnosticListenerObserver(_instrumentations));

                _initialized = true;
            }            
        }

        private class DiagnosticListenerObserver : IObserver<DiagnosticListener>
        {
            private readonly IEnumerable<IDiagnosticInstrumentation> _instrumentations;
            public DiagnosticListenerObserver(IEnumerable<IInstrumentation> instrumentations) => _instrumentations = instrumentations.OfType<IDiagnosticInstrumentation>();

            public void OnCompleted() { }
            public void OnError(Exception error) { }

            public void OnNext(DiagnosticListener listener)
            {
                foreach (var instrumentation in _instrumentations.Where(it => it.Match(listener)))
                {
                    listener.Subscribe(new InstrumentationObserver(instrumentation));
                }
            }
        }

        private class InstrumentationObserver : IObserver<KeyValuePair<string, object?>>
        {
            private readonly IDiagnosticInstrumentation _instrumentation;
            public InstrumentationObserver(IDiagnosticInstrumentation instrumentation)=> _instrumentation = instrumentation;
            public void OnCompleted() { }
            public void OnError(Exception error) { }
            public void OnNext(KeyValuePair<string, object?> kv)=>_instrumentation.OnNext(kv);
        }
    }
}
