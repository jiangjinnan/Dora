using Dora.DynamicProxy;
using Microsoft.Extensions.DependencyInjection;
using System.Threading.Tasks;
using Xunit;

namespace Dora.Interception.Test
{
    public class DegradationFixture
    {
        private static string _flag = "";

        [Fact]
        public async void Test()
        {
            _flag = "";
            var foobar = new ServiceCollection()
                .AddSingleton<Foobar, Foobar>()
                .BuildInterceptableServiceProvider()
                .GetRequiredService<Foobar>();
            var result = await foobar.InvokeAsync();
            Assert.Equal("Target", result);
            Assert.Equal("TargetFoobar", _flag);
        }

        public class Foobar
        {
            [FoobarInterceptor]
            public virtual async Task<string> InvokeAsync ()
            {
                await Task.Delay(1000);
                _flag += "Target";
                return _flag;
            }
        }

        public class FoobarInterceptorAttribute : InterceptorAttribute
        {
            public async Task InvokeAsync(InvocationContext context)
            {
                await context.ProceedAsync();
                await Task.Delay(1000);
                _flag += "Foobar";
            }
            public override void Use(IInterceptorChainBuilder builder) => builder.Use(this, Order);
        }
    }

    
}
