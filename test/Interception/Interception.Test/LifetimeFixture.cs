using Microsoft.Extensions.DependencyInjection;
using System;
using System.Threading.Tasks;
using Xunit;

namespace Dora.Interception.Test
{
    public class LifetimeFixture
    {
        [Fact]
        public void CheckLifetime()
        {
            SingleService.Instances = 0;
            ScopedService.Instances = 0;
            TransientService.Instances = 0;
            SingleService.Disposed = 0;
            ScopedService.Disposed = 0;
            TransientService.Disposed = 0;

            var foobar = new ServiceCollection()
                .AddSingleton<Foobar>()
                .AddSingleton<SingleService>()
                .AddScoped<ScopedService>()
                .AddTransient<TransientService>()
                .BuildInterceptableServiceProvider()
                .GetRequiredService<Foobar>();

            foobar.M();
            foobar.M();
            Assert.Equal(1, SingleService.Instances);
            Assert.Equal(2, ScopedService.Instances);
            Assert.Equal(2, TransientService.Instances);
            Assert.Equal(0, SingleService.Disposed);
            Assert.Equal(2, ScopedService.Disposed);
            Assert.Equal(2, TransientService.Disposed);
        }

        [FakeInterceptor]
        public class Foobar
        {
            public virtual void M() { }
        }

        public class FakeInterceptorAttribute : InterceptorAttribute
        {
            public ValueTask InvokeAsync(InvocationContext invocationContext, SingleService singleService, ScopedService scopedService, TransientService transientService)
                => invocationContext.ProceedAsync();
        }

        public class SingleService
        {
            public static int Instances = 0;
            public static int Disposed = 0;
            public SingleService() => Instances++;
        }

        public class ScopedService: IDisposable
        {
            public static int Instances = 0;
            public static int Disposed = 0;
            public ScopedService() => Instances++;
            public void Dispose() => Disposed++;
        }

        public class TransientService : IDisposable
        {
            public static int Instances = 0;
            public static int Disposed = 0;
            public TransientService() => Instances++;
            public void Dispose() => Disposed++;
        }
    }
}
