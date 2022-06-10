using App1;
using Microsoft.Extensions.DependencyInjection;

var timeProvider = new ServiceCollection()
    .AddMemoryCache()
    .AddSingleton<ISystemTimeProvider, SystemTimeProvider>()
    .AddSingleton<SystemTimeProvider>()
    .BuildInterceptableServiceProvider()
    .GetRequiredService<SystemTimeProvider>();

Console.WriteLine("Utc time:");
for (int index = 0; index < 5; index++)
{
    Console.WriteLine($"{timeProvider.GetCurrentTime(DateTimeKind.Utc)}[{DateTime.UtcNow}]");
    await Task.Delay(1000);
}


Console.WriteLine("Utc time:");
for (int index = 0; index < 5; index++)
{
    Console.WriteLine($"{timeProvider.GetCurrentTime(DateTimeKind.Local)}[{DateTime.Now}]");
    await Task.Delay(1000);
}