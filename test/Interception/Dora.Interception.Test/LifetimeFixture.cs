using Microsoft.Extensions.DependencyInjection;
using System;
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

            var foobar1 = provider.GetRequiredService<Foobar>();
            var foobar2 = provider.GetRequiredService<Foobar>();
            Assert.NotSame(typeof(Foobar), foobar.GetType());
            Assert.Same(foobar1, foobar2);
        }

        [Fact]
        public void Trsient()
        {
            var provider = new ServiceCollection()
                 .AddTransient<Foobar, Foobar>()
                 .BuildInterceptableServiceProvider();

            _counter = 0;
            var foobar = provider.GetRequiredService<Foobar>();
            foobar = provider.GetRequiredService<Foobar>();
            foobar = provider.GetRequiredService<Foobar>();
            Assert.Equal(3, _counter);
        }

        [Fact]
        public void Scoped()
        {
             var root = new ServiceCollection()
                .AddScoped<Foobar, Foobar>()
                .BuildInterceptableServiceProvider();

             var provider1 = root
               .CreateScope()
               .ServiceProvider;
             var provider2 = root
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
