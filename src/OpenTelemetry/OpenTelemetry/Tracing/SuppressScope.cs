namespace Dora.OpenTelemetry.Tracing
{
    public  class SuppressScope : IDisposable
    {
        private readonly bool _originallySuppressed;
        private static readonly AsyncLocal<bool> _suppressed = new();
        public SuppressScope()
        {
            _originallySuppressed = _suppressed.Value;
            _suppressed.Value = true;
        }
        public static bool IsSuppressed => _suppressed.Value;
        public void Dispose()=> _suppressed.Value = _originallySuppressed;
    }
}
