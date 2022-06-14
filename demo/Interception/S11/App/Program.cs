using App;
using Microsoft.Extensions.DependencyInjection;

var invoker = new ServiceCollection()
    .AddSingleton<Invoker>()
    .BuildInterceptableServiceProvider()
    .GetRequiredService<Invoker>();

invoker.Invoke();
