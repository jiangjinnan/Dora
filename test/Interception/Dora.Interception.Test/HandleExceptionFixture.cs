using Dora.DynamicProxy;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace Dora.Interception.Test
{
    public class HandleExceptionFixture
    {
        [Fact]
        public async void InvokeAsync()
        {
            var proxy = new ServiceCollection()
                .AddScoped<IFoobar, Foobar>()
                .BuildInterceptableServiceProvider()
                .GetRequiredService<IFoobar>();

            try
            {
               
               var result = await proxy.Invoke1Async();
                throw new Exception();
            }
            catch (Exception ex)
            {
                Assert.IsType<InvalidOperationException>(ex);
            }

            try
            {
                await proxy.Invoke2Async();
                throw new Exception();
            }
            catch (Exception ex)
            {
                Assert.IsType<InvalidOperationException>(ex);
            }

            try
            {
                await proxy.Invoke3Async();
                throw new Exception();
            }
            catch (Exception ex)
            {
                Assert.IsType<TaskCanceledException>(ex);
            }
        }

        public interface IFoobar
        {
            Task<int> Invoke1Async();
            Task Invoke2Async();
            Task Invoke3Async();
        }

        [FooInterceptor]
        public class Foobar : IFoobar
        {
            public Task<int> Invoke1Async()=>throw new InvalidOperationException();
            public Task Invoke2Async() => throw new InvalidOperationException();
            public Task Invoke3Async() => new HttpClient().GetAsync("http://www.baidu.com", new CancellationTokenSource(1).Token);
        }

        public class FoobarInterceptorAttibute : InterceptorAttribute
        {
            public async Task InvokeAsync(InvocationContext context)
            {
                try
                {
                    await context.ProceedAsync();
                }
                catch
                { }
            }
            public override void Use(IInterceptorChainBuilder builder)
            {
                builder.Use(this, Order);
            }
        }
    }
}
