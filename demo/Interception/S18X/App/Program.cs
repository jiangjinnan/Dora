using App;
using Microsoft.Extensions.DependencyInjection;

GetService<Foo>();
GetService<Bar>();
GetService<Baz>();

static void GetService<T>() where T:class
{
    try
    {
        Console.WriteLine($"{typeof(T).Name}:");
        _ = new ServiceCollection()
           .AddSingleton<T>()
           .BuildInterceptableServiceProvider()
           .GetRequiredService<T>();
        Console.WriteLine("OK");
    }
    catch (Exception ex)
    {
        Console.WriteLine(ex.Message);
    }
}
   