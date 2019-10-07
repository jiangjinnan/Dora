using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Dora.Interception;

namespace App
{
public class Program
{
    public static void Main(string[] args)
    {
        Host.CreateDefaultBuilder()
            .UseInterceptableServiceProvider(configure: Configure)
                .ConfigureWebHostDefaults(buider => buider.UseStartup<Startup>())
                .Build()
                .Run();

        static void Configure(InterceptionBuilder interceptionBuilder)
        {
            interceptionBuilder.AddPolicy(policyBuilder => policyBuilder
                .For<CacheReturnValueAttribute>(order: 1, cache => cache
                    .To<SystemClock>(target => target
                        .IncludeMethod(clock => clock.GetCurrentTime(default)))));
        }
    }
}
}
