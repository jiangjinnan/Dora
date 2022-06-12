using Dora.Interception;

namespace App5
{
    public class ServiceBase : IDisposable
    {
        public ServiceBase() => Console.WriteLine($"{GetType().Name}:Construct...");
        public void Dispose() => Console.WriteLine($"{GetType().Name}:Dispose...");
    }

    public class SingletonService : ServiceBase { }
    public class ScopedService : ServiceBase { }
    public class TransientService : ServiceBase { }
}
