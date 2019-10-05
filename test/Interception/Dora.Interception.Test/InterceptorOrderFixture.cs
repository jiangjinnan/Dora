using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace Dora.Interception.Test
{
    public class InterceptorOrderFixture
    {
        [Fact]
        public async void InterceptInterface()
        {
            var foobar = new ServiceCollection()
                .AddInterception()
                .AddSingletonInterceptable<IFoobar, Foobar>()
                .BuildServiceProvider()
                .GetRequiredService<IFoobar>();
            FakeInterceptorAttribute.Reset();
            await foobar.InvokeAsync();
            Assert.Equal("312", FakeInterceptorAttribute.Result);

            foobar = new ServiceCollection()
                .AddSingleton<IFoobar, Foobar>()
                .BuildInterceptableServiceProvider()
                .GetRequiredService<IFoobar>();
            FakeInterceptorAttribute.Reset();
            await foobar.InvokeAsync();
            Assert.Equal("312", FakeInterceptorAttribute.Result);
        }

        [Fact]
        public async void InterceptClass()
        {
            var foobar = new ServiceCollection()
                .AddInterception()
                .AddSingletonInterceptable<Foobar, Foobar>()
                .BuildServiceProvider()
                .GetRequiredService<Foobar>();
            FakeInterceptorAttribute.Reset();
            await foobar.InvokeAsync();
            Assert.Equal("312", FakeInterceptorAttribute.Result);

            foobar = new ServiceCollection()
                .AddSingleton<Foobar, Foobar>()
                .BuildInterceptableServiceProvider()
                .GetRequiredService<Foobar>();
            FakeInterceptorAttribute.Reset();
            await foobar.InvokeAsync();
            Assert.Equal("312", FakeInterceptorAttribute.Result);
        }

        public interface IFoobar
        {
            Task InvokeAsync();
        }

        public class Foobar : IFoobar
        {
            [FakeInterceptor(3, Order = 1)]
            [FakeInterceptor(1, Order = 2)]
            [FakeInterceptor(2, Order = 3)]
            public virtual Task InvokeAsync() => Task.CompletedTask;
        }
    }
}
