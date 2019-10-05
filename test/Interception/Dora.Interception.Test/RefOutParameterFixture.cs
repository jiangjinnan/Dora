using Microsoft.Extensions.DependencyInjection;
using System.Threading.Tasks;
using Xunit;

namespace Dora.Interception.Test
{
    public class RefOutParameterFixture
    {
        [Fact]
        public void Intercept()
        {
            var provider = new ServiceCollection()
                .AddSingleton<IFoobar, Foobar>()
                .AddSingleton<Foobar, Foobar>()
                .BuildInterceptableServiceProvider();

            var proxy1 = provider.GetRequiredService<IFoobar>();
            int x = 0;
            proxy1.Invoke(ref x, out int y);

            Assert.Equal(123, x);
            Assert.Equal(456, y);

            x = 1;
            y = 0;
            proxy1.Invoke(ref x, out y);

            Assert.Equal(10, x);
            Assert.Equal(10, y);

            var proxy2 = provider.GetRequiredService<Foobar>();

            x = 0;
            y = 0;
            proxy1.Invoke(ref x, out y);

            Assert.Equal(123, x);
            Assert.Equal(456, y);

            x = 1;
            y = 0;
            proxy2.Invoke(ref x, out y);

            Assert.Equal(10, x);
            Assert.Equal(10, y);
        }

        public interface IFoobar
        {
            void Invoke(ref int x, out int y);
        }

        [MyInterceptorAttribute]
        public class Foobar: IFoobar
        {
            public virtual void Invoke(ref int x, out int y)
            {
               x = y = 10;
            }
        }

        public class MyInterceptorAttribute : InterceptorAttribute
        {
            public async Task InvokeAsync(InvocationContext context)
            {
                if (context.Arguments[0].Equals(0))
                {
                    context.Arguments[0] = 123;
                    context.Arguments[1] = 456;
                }
                else
                {
                    await context.ProceedAsync();
                }
            }
            public override void Use(IInterceptorChainBuilder builder)
            {
                builder.Use<MyInterceptorAttribute>(Order);
            }
        }
    }
}
