using App;
using Microsoft.Extensions.DependencyInjection;

Invoke((ServiceLifetime)int.Parse(args.FirstOrDefault()??"0"));


static void Invoke(ServiceLifetime lifetime)
{
    Console.WriteLine(lifetime);
    try
    {
        var services = new ServiceCollection().AddSingleton<Invoker>();
        services.Add(ServiceDescriptor.Describe(typeof(FoobarService), typeof(FoobarService), lifetime));
        var invoker = services.BuildInterceptableServiceProvider().GetRequiredService<Invoker>();
        invoker.Invoke();
    }
    catch (Exception ex)
    {
        Console.WriteLine(ex.Message);
    }
}

