using Dora.Interception;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;

namespace Demo1
{
  public class Program
  {
    public static void Main(string[] args)
    {
var clock1 = new ServiceCollection()
  .AddMemoryCache(options => options.AbsoluteExpirationRelativeToNow = TimeSpan.FromSeconds(5))
  .AddSingleton<ISystomClock, SystomClock>()
  .AddInterception(builder => builder.SetDynamicProxyFactory())
  .BuildServiceProvider()
  .GetRequiredService<IInterceptable<ISystomClock>>()
  .Proxy;
for (int i = 0; i < int.MaxValue; i++)
{
  Console.WriteLine($"Current time: {clock1.GetCurrentTime()}");
  Task.Delay(1000).Wait();
}


var clock2 = new ServiceCollection()
  .AddMemoryCache(options => options.AbsoluteExpirationRelativeToNow = TimeSpan.FromSeconds(5))
  .AddSingleton<ISystomClock, SystomClock>()
  .AddInterception(builder => builder.SetDynamicProxyFactory())
  .BuildServiceProvider()
  .ToInterceptable()
  .GetRequiredService<ISystomClock>();

for (int i = 0; i < int.MaxValue; i++)
{
  Console.WriteLine($"Current time: {clock2.GetCurrentTime()}");
  Task.Delay(1000).Wait();
}
    }
  }
  public interface ISystomClock
  {
    DateTime GetCurrentTime();
  }

  public class SystomClock : ISystomClock
  {
    [CacheReturnValue]
    public DateTime GetCurrentTime()
    {
      return DateTime.UtcNow;
    }
  }
}
