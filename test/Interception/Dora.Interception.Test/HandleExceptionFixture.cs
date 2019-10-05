using Microsoft.Extensions.DependencyInjection;
using System;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace Dora.Interception.Test
{
    public class HandleExceptionFixture
    {
        [Fact]
        public async void InterceptInterface()
        {
            var foobar = new ServiceCollection()
                .AddInterception()
                .AddSingletonInterceptable<IFoobar, Foobar>()
                .BuildServiceProvider()
                .GetRequiredService<IFoobar>();

            try
            {
                await foobar.Invoke1Async();
                throw new Exception();
            }
            catch (Exception ex)
            {
                Assert.IsType<FakeException>(ex);
                Assert.IsType<InvalidOperationException>(ex.InnerException);
            }

            try
            {
                await foobar.Invoke2Async();
                throw new Exception();
            }
            catch (Exception ex)
            {
                Assert.IsType<FakeException>(ex);
                Assert.IsType<InvalidOperationException>(ex.InnerException);
            }

            try
            {
                await foobar.Invoke3Async();
                throw new Exception();
            }
            catch (Exception ex)
            {
                Assert.IsType<FakeException>(ex);
                Assert.IsType<TaskCanceledException>(ex.InnerException);
            }

            foobar = new ServiceCollection()
                .AddSingleton<IFoobar, Foobar>()
                .BuildInterceptableServiceProvider()
                .GetRequiredService<IFoobar>();
            try
            {
                await foobar.Invoke1Async();
                throw new Exception();
            }
            catch (Exception ex)
            {
                Assert.IsType<FakeException>(ex);
                Assert.IsType<InvalidOperationException>(ex.InnerException);
            }

            try
            {
                await foobar.Invoke2Async();
                throw new Exception();
            }
            catch (Exception ex)
            {
                Assert.IsType<FakeException>(ex);
                Assert.IsType<InvalidOperationException>(ex.InnerException);
            }

            try
            {
                await foobar.Invoke3Async();
                throw new Exception();
            }
            catch (Exception ex)
            {
                Assert.IsType<FakeException>(ex);
                Assert.IsType<TaskCanceledException>(ex.InnerException);
            }
        }

        [Fact]
        public async void InterceptClass()
        {
            var foobar = new ServiceCollection()
                .AddInterception()
                .AddSingletonInterceptable<Foobar, Foobar>()
                .BuildServiceProvider()
                .GetRequiredService<Foobar>();

            try
            {
                await foobar.Invoke1Async();
                throw new Exception();
            }
            catch (Exception ex)
            {
                Assert.IsType<FakeException>(ex);
                Assert.IsType<InvalidOperationException>(ex.InnerException);
            }

            try
            {
                await foobar.Invoke2Async();
                throw new Exception();
            }
            catch (Exception ex)
            {
                Assert.IsType<FakeException>(ex);
                Assert.IsType<InvalidOperationException>(ex.InnerException);
            }

            try
            {
                await foobar.Invoke3Async();
                throw new Exception();
            }
            catch (Exception ex)
            {
                Assert.IsType<FakeException>(ex);
                Assert.IsType<TaskCanceledException>(ex.InnerException);
            }

            foobar = new ServiceCollection()
                .AddSingleton<Foobar, Foobar>()
                .BuildInterceptableServiceProvider()
                .GetRequiredService<Foobar>();
            try
            {
                await foobar.Invoke1Async();
                throw new Exception();
            }
            catch (Exception ex)
            {
                Assert.IsType<FakeException>(ex);
                Assert.IsType<InvalidOperationException>(ex.InnerException);
            }

            try
            {
                await foobar.Invoke2Async();
                throw new Exception();
            }
            catch (Exception ex)
            {
                Assert.IsType<FakeException>(ex);
                Assert.IsType<InvalidOperationException>(ex.InnerException);
            }

            try
            {
                await foobar.Invoke3Async();
                throw new Exception();
            }
            catch (Exception ex)
            {
                Assert.IsType<FakeException>(ex);
                Assert.IsType<TaskCanceledException>(ex.InnerException);
            }
        }

        public interface IFoobar
        {
            Task<int> Invoke1Async();
            Task Invoke2Async();
            Task Invoke3Async();
        }

        [FakeInterceptor]
        public class Foobar : IFoobar
        {
            public virtual Task<int> Invoke1Async()=>throw new InvalidOperationException();
            public virtual Task Invoke2Async() => throw new InvalidOperationException();
            public virtual Task Invoke3Async() => new HttpClient().GetAsync("http://www.baidu.com", new CancellationTokenSource(1).Token);
        }
    }
}
