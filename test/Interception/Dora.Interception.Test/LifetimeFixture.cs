using Microsoft.Extensions.DependencyInjection;
using System;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace Dora.Interception.Test
{
    public class LifetimeFixture
    {

        [Fact]
        public void Singleton4Interface()
        {
            IServiceProvider root = new ServiceCollection()
                .AddInterception()
                .AddSingleton<IFoobar, Foobar>()
                .BuildServiceProvider();

            Foobar.Counter = 0;
            var foobar = root.GetRequiredService<IFoobar>();
            var foobar1 = root.CreateScope().ServiceProvider.GetRequiredService<IFoobar>();
            var foobar2 = root.CreateScope().ServiceProvider.GetRequiredService<IFoobar>();

            Assert.Equal(1, Foobar.Counter);
            Assert.Same(foobar, foobar1);
            Assert.Same(foobar, foobar2);

            root = new ServiceCollection()
                .AddSingleton<IFoobar, Foobar>()
                .BuildInterceptableServiceProvider();

            Foobar.Counter = 0;
            foobar = root.GetRequiredService<IFoobar>();
            foobar1 = root.CreateScope().ServiceProvider.GetRequiredService<IFoobar>();
            foobar2 = root.CreateScope().ServiceProvider.GetRequiredService<IFoobar>();

            Assert.Equal(1, Foobar.Counter);
            Assert.Same(foobar, foobar1);
            Assert.Same(foobar, foobar2);
        }

        [Fact]
        public void Singleton4Class()
        {
            IServiceProvider root = new ServiceCollection()
                .AddInterception()
                .AddSingleton<Foobar, Foobar>()
                .BuildServiceProvider();

            Foobar.Counter = 0;
            var foobar = root.GetRequiredService<Foobar>();
            var foobar1 = root.CreateScope().ServiceProvider.GetRequiredService<Foobar>();
            var foobar2 = root.CreateScope().ServiceProvider.GetRequiredService<Foobar>();

            Assert.Equal(1, Foobar.Counter);
            Assert.Same(foobar, foobar1);
            Assert.Same(foobar, foobar2);

            root = new ServiceCollection()
                .AddSingleton<Foobar, Foobar>()
                .BuildInterceptableServiceProvider();

            Foobar.Counter = 0;
            foobar = root.GetRequiredService<Foobar>();
            foobar1 = root.CreateScope().ServiceProvider.GetRequiredService<Foobar>();
            foobar2 = root.CreateScope().ServiceProvider.GetRequiredService<Foobar>();

            Assert.Equal(1, Foobar.Counter);
            Assert.Same(foobar, foobar1);
            Assert.Same(foobar, foobar2);
        }

        [Fact]
        public void Scoped4Interface()
        {
            IServiceProvider root = new ServiceCollection()
                .AddInterception()
                .AddScopedInterceptable<IFoobar, Foobar>()
                .BuildServiceProvider();

            var scope1 = root.CreateScope();
            var scope2 = root.CreateScope();

            Foobar.Counter = 0;
            var foobar11 = scope1.ServiceProvider.GetRequiredService<IFoobar>();
            var foobar12 = scope1.ServiceProvider.GetRequiredService<IFoobar>();
            var foobar21 = scope2.ServiceProvider.GetRequiredService<IFoobar>();
            var foobar22 = scope2.ServiceProvider.GetRequiredService<IFoobar>();

            Assert.Equal(2, Foobar.Counter);
            Assert.Same(foobar11, foobar12);
            Assert.Same(foobar21, foobar22);

            root = new ServiceCollection()
                .AddScoped<IFoobar, Foobar>()
                .BuildInterceptableServiceProvider();

             scope1 = root.CreateScope();
             scope2 = root.CreateScope();

            Foobar.Counter = 0;
             foobar11 = scope1.ServiceProvider.GetRequiredService<IFoobar>();
             foobar12 = scope1.ServiceProvider.GetRequiredService<IFoobar>();
             foobar21 = scope2.ServiceProvider.GetRequiredService<IFoobar>();
             foobar22 = scope2.ServiceProvider.GetRequiredService<IFoobar>();

            Assert.Equal(2, Foobar.Counter);
            Assert.Same(foobar11, foobar12);
            Assert.Same(foobar21, foobar22);
        }

        [Fact]
        public void Scoped4Class()
        {
            IServiceProvider root = new ServiceCollection()
                .AddInterception()
                .AddScopedInterceptable<Foobar, Foobar>()
                .BuildServiceProvider();

            var scope1 = root.CreateScope();
            var scope2 = root.CreateScope();

            Foobar.Counter = 0;
            var foobar11 = scope1.ServiceProvider.GetRequiredService<Foobar>();
            var foobar12 = scope1.ServiceProvider.GetRequiredService<Foobar>();
            var foobar21 = scope2.ServiceProvider.GetRequiredService<Foobar>();
            var foobar22 = scope2.ServiceProvider.GetRequiredService<Foobar>();

            Assert.Equal(2, Foobar.Counter);
            Assert.Same(foobar11, foobar12);
            Assert.Same(foobar21, foobar22);

            root = new ServiceCollection()
                .AddScoped<Foobar, Foobar>()
                .BuildInterceptableServiceProvider();

            scope1 = root.CreateScope();
            scope2 = root.CreateScope();

            Foobar.Counter = 0;
            foobar11 = scope1.ServiceProvider.GetRequiredService<Foobar>();
            foobar12 = scope1.ServiceProvider.GetRequiredService<Foobar>();
            foobar21 = scope2.ServiceProvider.GetRequiredService<Foobar>();
            foobar22 = scope2.ServiceProvider.GetRequiredService<Foobar>();

            Assert.Equal(2, Foobar.Counter);
            Assert.Same(foobar11, foobar12);
            Assert.Same(foobar21, foobar22);
        }

        [Fact]
        public void Transient4Interface()
        {
            IServiceProvider root = new ServiceCollection()
                .AddInterception()
                .AddTransientInterceptable<IFoobar, Foobar>()
                .BuildServiceProvider();

            var scope1 = root.CreateScope();
            var scope2 = root.CreateScope();

            Foobar.Counter = 0;
            var foobar11 = scope1.ServiceProvider.GetRequiredService<IFoobar>();
            var foobar12 = scope1.ServiceProvider.GetRequiredService<IFoobar>();
            var foobar21 = scope2.ServiceProvider.GetRequiredService<IFoobar>();
            var foobar22 = scope2.ServiceProvider.GetRequiredService<IFoobar>();

            Assert.Equal(4, Foobar.Counter);
            Assert.NotSame(foobar11, foobar12);
            Assert.NotSame(foobar21, foobar22);

            root = new ServiceCollection()
                .AddTransient<IFoobar, Foobar>()
                .BuildInterceptableServiceProvider();

             scope1 = root.CreateScope();
             scope2 = root.CreateScope();

            Foobar.Counter = 0;
             foobar11 = scope1.ServiceProvider.GetRequiredService<IFoobar>();
             foobar12 = scope1.ServiceProvider.GetRequiredService<IFoobar>();
             foobar21 = scope2.ServiceProvider.GetRequiredService<IFoobar>();
             foobar22 = scope2.ServiceProvider.GetRequiredService<IFoobar>();

            Assert.Equal(4, Foobar.Counter);
            Assert.NotSame(foobar11, foobar12);
            Assert.NotSame(foobar21, foobar22);
        }

        [Fact]
        public void Transient4Class()
        {
            IServiceProvider root = new ServiceCollection()
                .AddInterception()
                .AddTransientInterceptable<Foobar, Foobar>()
                .BuildServiceProvider();

            var scope1 = root.CreateScope();
            var scope2 = root.CreateScope();

            Foobar.Counter = 0;
            var foobar11 = scope1.ServiceProvider.GetRequiredService<Foobar>();
            var foobar12 = scope1.ServiceProvider.GetRequiredService<Foobar>();
            var foobar21 = scope2.ServiceProvider.GetRequiredService<Foobar>();
            var foobar22 = scope2.ServiceProvider.GetRequiredService<Foobar>();

            Assert.Equal(4, Foobar.Counter);
            Assert.NotSame(foobar11, foobar12);
            Assert.NotSame(foobar21, foobar22);

            root = new ServiceCollection()
                .AddTransient<Foobar, Foobar>()
                .BuildInterceptableServiceProvider();

            scope1 = root.CreateScope();
            scope2 = root.CreateScope();

            Foobar.Counter = 0;
            foobar11 = scope1.ServiceProvider.GetRequiredService<Foobar>();
            foobar12 = scope1.ServiceProvider.GetRequiredService<Foobar>();
            foobar21 = scope2.ServiceProvider.GetRequiredService<Foobar>();
            foobar22 = scope2.ServiceProvider.GetRequiredService<Foobar>();

            Assert.Equal(4, Foobar.Counter);
            Assert.NotSame(foobar11, foobar12);
            Assert.NotSame(foobar21, foobar22);
        }

        public interface IFoobar
        {
            Task InvokeAsync();
        }

        public class Foobar : IFoobar
        {
            public static int Counter;

            public Foobar() => Interlocked.Increment(ref Counter);

            [FakeInterceptor]
            public virtual Task InvokeAsync() => Task.CompletedTask;
        }
    }
}
