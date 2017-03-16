using Dora.Interception;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
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
            new WebHostBuilder()
                .UseKestrel()
                .ConfigureLogging(loggerFactory => loggerFactory.AddConsole((category, level) => category == "Demo1"))
                .ConfigureServices(svcs => svcs
                    .AddSingleton<IFoobarService, FoobarService>()
                    .AddInterception(builder=>builder.SetDynamicProxyFactory())
                    .AddMvc())
                .Configure(app => app
                    .UseDeveloperExceptionPage()
                    .UseMvc())
                .Build()
                .Run();
        }
    }

    public class HomeController : Controller
    {
        private IFoobarService _service;

        public HomeController(IInterceptable<IFoobarService> service)
        {
            _service = service.Proxy;
        }

        [HttpGet("/")]
        public async Task Index()
        {
            await _service.InvokeAsync();
        }
    }

    public interface IFoobarService
    {
        Task InvokeAsync();
    }
  
    [Foobar]
    public class FoobarService : IFoobarService
    {
        [HandleException("Demo1")]
        public Task InvokeAsync()
        {
            return Task.FromException(new Exception("Manually thrown exception"));
        }
    }

  public class FoobarAttribute : Attribute
  { }
}
