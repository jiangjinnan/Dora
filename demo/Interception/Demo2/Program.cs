using Dora.Interception;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Demo2
{
    public class Program
    {
        public static IWebHost Host {get;set;}
        public static void Main(string[] args)
        {
            Host = new WebHostBuilder()
               .UseKestrel()
               .ConfigureLogging(loggerFactory => loggerFactory.AddConsole((category, level) => category == "Demo2"))
               .UseStartup<Startup>()
               .Build();
            Host.Run();
        }
    }

    public class Startup
    {
        public void ConfigureServices(IServiceCollection services)
        {
            services
                .AddSingleton<IFoobarService, FoobarService>()
                .AddInterception()
                .AddMvc();
            services.ToInterceptable();
        }

        public void Configure(IApplicationBuilder app)
        {
            app
                .UseMiddleware<CheckServiceProvider>()
                .UseDeveloperExceptionPage()
                .UseMvc();
        }
    }

    public class HomeController : Controller
    {
        private IFoobarService _service;

        public HomeController(IFoobarService service)
        {
            _service = service;
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

    public class FoobarService : IFoobarService, IInterceptable
    {
        [HandleException("Demo2")]
        public Task InvokeAsync()
        {
            return Task.FromException(new Exception("Manually thrown exception"));
        }
    }

    public class CheckServiceProvider
    {
        private RequestDelegate _next;
        public CheckServiceProvider(RequestDelegate next)
        {
            _next = next;
        }
        public Task Invoke(HttpContext context)
        {
            Console.WriteLine(context.RequestServices);
            return _next(context);
        }
    }
}
