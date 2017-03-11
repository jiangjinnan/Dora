using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.AspNetCore.Builder;
using Dora.Interception;

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
                    .AddCastleInterception()
                    .AddMvc())
                .Configure(app => app
                    .UseDeveloperExceptionPage()
                    .UseMvc())
                //.UseInterception()
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

    public class FoobarService : IFoobarService
    {
        [HandleException("Demo1")]
        public Task InvokeAsync()
        {
            return Task.FromException(new Exception("Manually thrown exception"));
        }
    }
}
