using Microsoft.Extensions.DependencyInjection;
using System;
using System.Threading.Tasks;
using Xunit;

namespace Dora.Interception.Test
{
    public class OpenGenericTypeFixture
    {
        [Fact]
        public async void Intercept4Interface_Normal()
        {
            var foobar = new ServiceCollection()
                .AddInterception()
                .AddSingletonInterceptable(typeof(IFoobar<,>), typeof(Foo<,>))
                .BuildServiceProvider()
                .GetRequiredService<IFoobar<string, string>>();

            FakeInterceptorAttribute.Reset();
            await foobar.Invoke<int>("1", "2", 3);
            Assert.Equal("1", FakeInterceptorAttribute.Result);

            foobar = new ServiceCollection()
                .AddInterception()
                .AddSingleton(typeof(IFoobar<,>), typeof(Foo<,>))
                .BuildInterceptableServiceProvider()
                .GetRequiredService<IFoobar<string, string>>();

            FakeInterceptorAttribute.Reset();
            await foobar.Invoke<int>("1", "2", 3);
            Assert.Equal("1", FakeInterceptorAttribute.Result);

            //foobar = new ServiceCollection()
            //    .AddInterception()
            //    .AddSingletonInterceptable(typeof(IFoobar<,>), typeof(Bar<,>))
            //    .BuildServiceProvider()
            //    .GetRequiredService<IFoobar<string, string>>();

            //FakeInterceptorAttribute.Reset();
            //await foobar.Invoke<int>("1", "2", 3);
            //Assert.Equal("1", FakeInterceptorAttribute.Result);

            //foobar = new ServiceCollection()
            //    .AddInterception()
            //    .AddSingleton(typeof(IFoobar<,>), typeof(Bar<,>))
            //    .BuildInterceptableServiceProvider()
            //    .GetRequiredService<IFoobar<string, string>>();

            //FakeInterceptorAttribute.Reset();
            //await foobar.Invoke<int>("1", "2", 3);
            //Assert.Equal("1", FakeInterceptorAttribute.Result);
        }

        [Fact]
        public async void Intercept4Interface_ExplicitlyImplemented()
        {
            var foobar = new ServiceCollection()
                .AddInterception()
                .AddSingletonInterceptable(typeof(IFoobar<,>), typeof(Bar<,>))
                .BuildServiceProvider()
                .GetRequiredService<IFoobar<string, string>>();

            FakeInterceptorAttribute.Reset();
            await foobar.Invoke<int>("1", "2", 3);
            Assert.Equal("1", FakeInterceptorAttribute.Result);

            foobar = new ServiceCollection()
                .AddInterception()
                .AddSingleton(typeof(IFoobar<,>), typeof(Bar<,>))
                .BuildInterceptableServiceProvider()
                .GetRequiredService<IFoobar<string, string>>();

            FakeInterceptorAttribute.Reset();
            await foobar.Invoke<int>("1", "2", 3);
            Assert.Equal("1", FakeInterceptorAttribute.Result);
        }

        [Fact]
        public async void Intercept4Class()
        {
            //var foobar = new ServiceCollection()
            //    .AddInterception()
            //    .AddSingletonInterceptable(typeof(Foo<,>), typeof(Foo<,>))
            //    .BuildServiceProvider()
            //    .GetRequiredService<Foo<string, string>>();

            //FakeInterceptorAttribute.Reset();
            //await foobar.Invoke<int>("1", "2", 3);
            //Assert.Equal("1", FakeInterceptorAttribute.Result);

            var foobar = new ServiceCollection()
                .AddInterception()
                .AddSingleton(typeof(Foo<,>), typeof(Foo<,>))
                .BuildInterceptableServiceProvider()
                .GetRequiredService<Foo<string, string>>();

            FakeInterceptorAttribute.Reset();
            await foobar.Invoke<int>("1", "2", 3);
            Assert.Equal("1", FakeInterceptorAttribute.Result);

            //foobar = new ServiceCollection()
            //    .AddInterception()
            //    .AddSingletonInterceptable(typeof(IFoobar<,>), typeof(Bar<,>))
            //    .BuildServiceProvider()
            //    .GetRequiredService<IFoobar<string, string>>();

            //FakeInterceptorAttribute.Reset();
            //await foobar.Invoke<int>("1", "2", 3);
            //Assert.Equal("1", FakeInterceptorAttribute.Result);

            //foobar = new ServiceCollection()
            //    .AddInterception()
            //    .AddSingleton(typeof(IFoobar<,>), typeof(Bar<,>))
            //    .BuildInterceptableServiceProvider()
            //    .GetRequiredService<IFoobar<string, string>>();

            //FakeInterceptorAttribute.Reset();
            //await foobar.Invoke<int>("1", "2", 3);
            //Assert.Equal("1", FakeInterceptorAttribute.Result);
        }

        public interface IFoobar<T1, T2>
        {
            Task Invoke<T>(T1 arg1, T2 arg2, T arg3);
        }

        public class Foo<T3, T4> : IFoobar<T3, T4>
        {
            [FakeInterceptor]
            public virtual Task Invoke<T>(T3 arg1, T4 arg2, T arg3) => Task.CompletedTask;
        }


        public class Bar<T1, T2> : IFoobar<T1, T2>
        {
            [FakeInterceptor]

            Task IFoobar<T1, T2>.Invoke<T>(T1 arg1, T2 arg2, T arg3) => Task.CompletedTask;
        }
    }
}

