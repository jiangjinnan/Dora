namespace App
{
public class ServiceBase : IDisposable
{
    public ServiceBase()=>Console.WriteLine($"{GetType().Name}.new()");
    public void Dispose() => Console.WriteLine($"{GetType().Name}.Dispose()");
}

public class SingletonService : ServiceBase { }
public class ScopedService : ServiceBase { }
public class TransientService : ServiceBase { }
}
