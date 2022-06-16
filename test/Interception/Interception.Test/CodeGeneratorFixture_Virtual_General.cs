using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Threading.Tasks;
using Xunit;
using Xunit.Sdk;
using static Dora.Interception.Test.Interceptor;

namespace Dora.Interception.Test
{
    [InterceptorReset]
    public  class CodeGeneratorFixture_Virtual_General
    {
        [Fact]
        public void M1() => Test(foobar => foobar.M1(1, 2));


        [Fact]
        public void M2() => Test(foobar => foobar.M2(1, 2));


        [Fact]
        public void M3()
        {
            var x = 1;
            int y;
            Test(foobar => foobar.M3(ref x, out y));
        }

        [Fact]
        public async void M4() => await TestAsync(async foobar => await foobar.M4(1, 2));


        [Fact]
        public async void M5() => await TestAsync(async foobar => await foobar.M5(1, 2));

        [Fact]
        public async void M6() => await TestAsync(async foobar => await foobar.M6(1, 2));

        [Fact]
        public async void M7() => await TestAsync(async foobar => await foobar.M7(1, 2));


        [Fact]
        public void P1_Get() => Test(foobar => _ = foobar.P1);


        [Fact]
        public void P1_Set() => Test(foobar => foobar.P1 = null!);

        [Foo(Order = 1)]
        [Bar(Order = 2)]
        public class Foobar
        {
            public virtual string? P1 { get; set; }

            public virtual void M1(int x, int y) { }

            public virtual int M2(int x, int y) => 1;

            public virtual void M3(ref int x, out int y) { y = 1; }

            public virtual Task M4(int x, int y) => Task.Delay(1);

            public virtual async Task<int> M5(int x, int y)
            {
               await Task.Delay(1);
                return 1;
            }

            public virtual async ValueTask M6(int x, int y)
            {
                await Task.Delay(1);
            }

            public virtual async ValueTask<int> M7(int x, int y)
            {
                await Task.Delay(1);
                return 1;
            }
        }

        private Foobar GetFoobar() => new ServiceCollection().AddSingleton<Foobar>().BuildInterceptableServiceProvider().GetRequiredService<Foobar>();

        private void Test(Action<Foobar> call)
        { 
            var foobar = GetFoobar();
            call(foobar);
            Assert.True(EnsureInterceptorInvoked());
        }

        private async Task TestAsync(Func<Foobar, Task> call)
        {
            var foobar = GetFoobar();
            await call(foobar);
            Assert.True(EnsureInterceptorInvoked());
        }

        public static class Interceptor
        {
            public static readonly List<object> Interceptors = new();
            public static void Reset() => Interceptors.Clear();

            public static bool EnsureInterceptorInvoked() => Interceptors[0] is FooAttribute && Interceptors[1] is BarAttribute;

            public class FooAttribute : InterceptorAttribute
            {
                public ValueTask InvokeAsync(InvocationContext invocationContext)
                {
                    Interceptors.Add(this);
                    return invocationContext.ProceedAsync();
                }
            }

            public class BarAttribute : InterceptorAttribute
            {
                public ValueTask InvokeAsync(InvocationContext invocationContext)
                {
                    Interceptors.Add(this);
                    return invocationContext.ProceedAsync();
                }
            }

            [AttributeUsage(AttributeTargets.Class)]
            public class InterceptorResetAttribute : BeforeAfterTestAttribute
            {
                public override void Before(MethodInfo methodUnderTest) => Interceptor.Reset();
            }
        }
    }
}
