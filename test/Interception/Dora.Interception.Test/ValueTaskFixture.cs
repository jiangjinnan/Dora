using Microsoft.Extensions.DependencyInjection;
using System.Threading.Tasks;
using Xunit;

namespace Dora.Interception.Test
{
    public class ValueTaskFixture
    {
        [Fact]
        public async void Intercept()
        {
            var provider = new ServiceCollection()
               .AddSingleton<IFoobar, Foobar>()
               .AddSingleton<Foobar, Foobar>()
               .BuildInterceptableServiceProvider();

            var proxy1 = provider.GetRequiredService<IFoobar>();
            await proxy1.Invoke1();
            Assert.Equal(1, await proxy1.Invoke2());

            var proxy2 = provider.GetRequiredService<Foobar>();
            await proxy2.Invoke1();
            Assert.Equal(1, await proxy2.Invoke2());
        }

        public interface IFoobar
        {
            ValueTask Invoke1();
            ValueTask<int> Invoke2();
        }

        [MyInterceptorAttribute]
        public class Foobar : IFoobar
        {
            public async virtual ValueTask Invoke1()
            {
                await Task.Delay(1);
            }

            public async virtual ValueTask<int> Invoke2()
            {
                await Task.Delay(1);
                return 1;
            }
        }

        public class MyInterceptorAttribute : InterceptorAttribute
        {
            public Task InvokeAsync(InvocationContext context)
            {
                return context.ProceedAsync();
            }
            public override void Use(IInterceptorChainBuilder builder)
            {
                builder.Use<MyInterceptorAttribute>(Order);
            }
        }
    }
}
