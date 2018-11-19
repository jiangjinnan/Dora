
using Dora.DynamicProxy;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace Dora.Interception.Test
{
    public class LifetimeFixture
    {
        private static int _counter;

        [Fact]
        public void Singleton()
        {
            var provider = new ServiceCollection()
                .AddSingleton<Foobar, Foobar>()
                .BuildInterceptableServiceProvider();

            _counter = 0;
            var foobar = provider.GetRequiredService<Foobar>();
            provider.GetRequiredService<Foobar>();
            provider.GetRequiredService<Foobar>();
            Assert.NotSame(typeof(Foobar), foobar.GetType());
            Assert.Equal(1, _counter);

            var foobar1 = provider.GetRequiredService<IInterceptable<Foobar>>().Proxy;
            var foobar2 = provider.GetRequiredService<IInterceptable<Foobar>>().Proxy;
            Assert.NotSame(typeof(Foobar), foobar.GetType());
            Assert.Same(foobar1, foobar2);

            provider = new ServiceCollection()
                .AddSingleton<Foobar, Foobar>()
                .AddInterception()
                .BuildServiceProvider();

            _counter = 0;
            foobar = provider.GetRequiredService<IInterceptable<Foobar>>().Proxy;
            foobar = provider.GetRequiredService<IInterceptable<Foobar>>().Proxy;
            foobar = provider.GetRequiredService<IInterceptable<Foobar>>().Proxy;    
            Assert.NotSame(typeof(Foobar), foobar.GetType());
            Assert.Equal(1, _counter);
        }

        [Fact]
        public void Trsient()
        {
            var provider = new ServiceCollection()
                 .AddTransient<Foobar, Foobar>()
                 .BuildInterceptableServiceProvider();

            _counter = 0;
            provider.GetRequiredService<Foobar>();
            provider.GetRequiredService<Foobar>();
            provider.GetRequiredService<Foobar>();
            Assert.Equal(3, _counter);

            provider = new ServiceCollection()
                .AddTransient<Foobar, Foobar>()
                .AddInterception()
                .BuildServiceProvider();

            _counter = 0;
            var foobar = provider.GetRequiredService<IInterceptable<Foobar>>().Proxy;
            foobar = provider.GetRequiredService<IInterceptable<Foobar>>().Proxy;
            foobar = provider.GetRequiredService<IInterceptable<Foobar>>().Proxy;
            Assert.Equal(3, _counter);
        }

        [Fact]
        public void Scoped()
        {
            IServiceProvider root = new ServiceCollection()
                .AddScoped<Foobar, Foobar>()
                .AddInterception()
                .BuildServiceProvider();
            var provider1 = root
                .CreateScope()
                .ServiceProvider;
            var provider2 = root
                .CreateScope()
                .ServiceProvider;

            _counter = 0;
            var foobar = provider1.GetRequiredService<IInterceptable<Foobar>>().Proxy;
            foobar = provider1.GetRequiredService<IInterceptable<Foobar>>().Proxy;
            foobar = provider1.GetRequiredService<IInterceptable<Foobar>>().Proxy;

            foobar = provider2.GetRequiredService<IInterceptable<Foobar>>().Proxy;
            foobar = provider2.GetRequiredService<IInterceptable<Foobar>>().Proxy;
            foobar = provider2.GetRequiredService<IInterceptable<Foobar>>().Proxy;

            Assert.Equal(2, _counter);

             root = new ServiceCollection()
                .AddScoped<Foobar, Foobar>()
                .BuildInterceptableServiceProvider();

             provider1 = root
               .CreateScope()
               .ServiceProvider;
             provider2 = root
                .CreateScope()
                .ServiceProvider;

            _counter = 0;
            provider1.GetRequiredService<Foobar>();
            provider1.GetRequiredService<Foobar>();
            provider1.GetRequiredService<Foobar>();

            provider2.GetRequiredService<Foobar>();
            provider2.GetRequiredService<Foobar>();
            provider2.GetRequiredService<Foobar>();
            Assert.Equal(2, _counter);

            var foobar1 = provider1.GetRequiredService<IInterceptable<Foobar>>().Proxy;
            var foobar2 = provider1.GetRequiredService<IInterceptable<Foobar>>().Proxy;
            var foobar3 = provider2.GetRequiredService<IInterceptable<Foobar>>().Proxy;
            Assert.Same(foobar1, foobar2);
            Assert.NotSame(foobar1, foobar3);
        }


        private class FakeAttribute : InterceptorAttribute
        {
            public Task InvokeAsync(InvocationContext context) => context.ProceedAsync();
            public override void Use(IInterceptorChainBuilder builder) => builder.Use(this, Order);
        }

        public class Foobar
        {
            public Foobar() => _counter++;

            [Fake]
            public virtual void Invoke() { }
        }
    }
}
