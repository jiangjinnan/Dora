using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using System.Collections.Concurrent;
using Microsoft.AspNetCore.Builder;

namespace Demo3
{
    public class Program
    {
        public static void Main(string[] args)
        {
            new WebHostBuilder()
               .UseKestrel()
               .UseStartup<Startup>()
               .Build()
               .Run();
        }
    }

    public class Startup
    {
        public IServiceProvider ConfigureServices(IServiceCollection services)
        {
            services.AddMvc();
            return Cat.Instance
                .Register(services)
                .Register(typeof(IFoobarService), _ => new FoobarService());
        }

        public void Configure(IApplicationBuilder app)
        {
            app
                .UseDeveloperExceptionPage()
                .UseMvc();
        }
    }

    public class HomeController
    {
        private IFoobarService _service;

        public HomeController(IFoobarService service)
        {
            _service = service;
        }

        [HttpGet("/")]
        public  string  Index()
        {
            return "Hello world";
        }
    }

    public interface IFoobarService { }

    public class FoobarService : IFoobarService { }

    public class Cat : IServiceProvider
    {
        private static readonly Cat _instance = new Cat();
        private ConcurrentDictionary<Type, Func<Cat, object>> _registrations = new ConcurrentDictionary<Type, Func<Cat, object>>();
        private IServiceProvider _backup;

        private Cat()
        {
            _backup = new ServiceCollection().BuildServiceProvider();
        }

        public static Cat Instance
        {
            get { return _instance; }
        }

        public Cat Register(IServiceCollection svcs)
        {
            _backup = svcs.BuildServiceProvider();
            return this;
        }

        public Cat Register(Type serviceType, Func<Cat, object> instanceAccessor)
        {
            _registrations[serviceType] = instanceAccessor;
            return this;
        }

        public object GetService(Type serviceType)
        {
            Func<Cat, object> instanceAccessor;
            return _registrations.TryGetValue(serviceType, out instanceAccessor)
                ? instanceAccessor(this)
                : _backup.GetService(serviceType);
        }
    }

}
