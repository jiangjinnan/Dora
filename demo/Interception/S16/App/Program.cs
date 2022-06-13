using App;
using Microsoft.Extensions.DependencyInjection;

var invoker = new ServiceCollection()
    .AddSingleton<Invoker>()
    .AddSingleton<IFoobar, Foobar>()
    .BuildInterceptableServiceProvider()
    .GetRequiredService<Invoker>();

invoker.Invoke1();
invoker.Invoke2();