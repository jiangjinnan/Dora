using Dora.Interception;

namespace App5
{
    public class ScopedService : IDisposable
    {
        public ScopedService() => Console.WriteLine($"{GetType().Name}:Construct...");
        public void Dispose() => Console.WriteLine($"{GetType().Name}:Dispose...");
    }
}
