using App;
using Microsoft.Extensions.DependencyInjection;

var provider = new ServiceCollection()
    .AddSingleton<SingletonService>()
    .AddScoped<ScopedService>()
    .AddTransient<TransientService>()
    .AddSingleton<Invoker>()
    .BuildInterceptableServiceProvider();
using (provider as IDisposable)
{ 
   var invoker = provider .GetRequiredService<Invoker>();
    invoker.Invoke();
    Console.WriteLine();
    invoker.Invoke();
}

