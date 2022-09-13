namespace Dora.OpenTelemetry.Tracing
{
    public  class SuppressScope : IDisposable
    {
        private readonly SuppressScope? _original;
        private static readonly AsyncLocal<SuppressScope?> _current = new();
        public static SuppressScope? Current => _current.Value;
        public SuppressScope()
        {
            _original = _current.Value;
            _current.Value = new SuppressScope();
        }
        public static bool IsSuppressed=> _current.Value is not null;
        public void Dispose()=> _current.Value = _original;
    }
}
