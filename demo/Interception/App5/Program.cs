using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Dora.Interception;
using System.Reflection;

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
            interceptionBuilder.AddPolicy("Interception.dora", script => script
                .AddReferences(Assembly.GetExecutingAssembly())
                .AddImports("App"));
        }
    }
}
}
