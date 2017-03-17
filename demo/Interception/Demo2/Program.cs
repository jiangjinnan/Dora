using Dora.Interception;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;

namespace Demo2
{
  public class Program
  {
    public static void Main(string[] args)
    {
new WebHostBuilder()
    .UseKestrel()
    .ConfigureServices(svcs => svcs
        .AddSingleton<ISystomClock, SystomClock>()
        .Configure<MemoryCacheEntryOptions>(options => options.AbsoluteExpirationRelativeToNow = TimeSpan.FromSeconds(2))
        .AddInterception(builder => builder.SetDynamicProxyFactory())
        .AddMvc())
    .Configure(app => app.UseMvc())
    .UseInterception()
    .Build()
    .Run();
    }
  }
}
