using System.Diagnostics;

namespace Dora.OpenTelemetry.Tracing
{
    public interface IDiagnosticInstrumentation: IInstrumentation
    {
        string ActivitySourceName { get; }
        bool Match(DiagnosticListener diagnosticListener);
        void OnNext(KeyValuePair<string, object?> arguments);
    }
}
