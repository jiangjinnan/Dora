using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;
using static Dora.Interception.Test.Interceptor;

namespace Dora.Interception.Test
{
    [InterceptorReset]
    public class CodeGeneratorFixture_Interface_GenericMethod
    {
        [Fact]
        public void M1() => Test(foobar => foobar.M1((IntValue)1, (IntValue)2));


        [Fact]
        public void M2() => Test(foobar => foobar.M2((IntValue)1, (IntValue)2));

        [Fact]
        public async void M3() => await TestAsync(async foobar => await foobar.M3((IntValue)1, (IntValue)2));


        [Fact]
        public async void M4() => await TestAsync(async foobar => await foobar.M4((IntValue)1, (IntValue)2));

        [Fact]
        public async void M5() => await TestAsync(async foobar => await foobar.M5((IntValue)1, (IntValue)2));

        [Fact]
        public async void M6() => await TestAsync(async foobar => await foobar.M6((IntValue)1, (IntValue)2));

        private IFoobar GetFoobar() => new ServiceCollection().AddSingleton<IFoobar, Foobar>().BuildInterceptableServiceProvider().GetRequiredService<IFoobar>();

        private void Test(Action<IFoobar> call)
        {
            var foobar = GetFoobar();
            call(foobar);
            Assert.True(EnsureInterceptorInvoked());
        }

        private async Task TestAsync(Func<IFoobar, Task> call)
        {
            var foobar = GetFoobar();
            await call(foobar);
            Assert.True(EnsureInterceptorInvoked());
        }
        public interface IFoobar
        {
            void M1<T1, T2>(T1 x, T2 y) where T1 : IDisposable, IConvertable<int> where T2 : IConvertable<int>;
            int M2<T1, T2>(T1 x, T2 y) where T1 : IDisposable, IConvertable<int> where T2 : IConvertable<int>;
            Task M3<T1, T2>(T1 x, T2 y) where T1 : IDisposable, IConvertable<int> where T2 : IConvertable<int>;
            Task<int> M4<T1, T2>(T1 x, T2 y) where T1 : IDisposable, IConvertable<int> where T2 : IConvertable<int>;
            ValueTask M5<T1, T2>(T1 x, T2 y) where T1 : IDisposable, IConvertable<int> where T2 : IConvertable<int>;
            ValueTask<int> M6<T1, T2>(T1 x, T2 y) where T1 : IDisposable, IConvertable<int> where T2 : IConvertable<int>;
        }

        [Foo(Order = 1)]
        [Bar(Order = 2)]
        public class Foobar : IFoobar
        {
            public void M1<T1, T2>(T1 x, T2 y)
                where T1 : IDisposable, IConvertable<int>
                where T2 : IConvertable<int>
            { }

            public int M2<T1, T2>(T1 x, T2 y)
                where T1 : IDisposable, IConvertable<int>
                where T2 : IConvertable<int>
            {
                return x.Convert() + y.Convert();
            }

            public async Task M3<T1, T2>(T1 x, T2 y)
                where T1 : IDisposable, IConvertable<int>
                where T2 : IConvertable<int>
            {
                await Task.Delay(1);
            }

            public async Task<int> M4<T1, T2>(T1 x, T2 y)
                where T1 : IDisposable, IConvertable<int>
                where T2 : IConvertable<int>
            {
                await Task.Delay(1);
                return x.Convert() + y.Convert();
            }

            public async ValueTask M5<T1, T2>(T1 x, T2 y)
                where T1 : IDisposable, IConvertable<int>
                where T2 : IConvertable<int>
            {
                await Task.Delay(1);
            }

            public async ValueTask<int> M6<T1, T2>(T1 x, T2 y)
                where T1 : IDisposable, IConvertable<int>
                where T2 : IConvertable<int>
            {
                await Task.Delay(1);
                return x.Convert() + y.Convert();
            }
        }

        public interface IConvertable<T>
        {
            T Convert();
        }

        public class IntValue : IConvertable<int>, IDisposable
        {
            private readonly int _value;

            public IntValue(int value)
            {
                _value = value;
            }

            public int Convert() => _value;

            public void Dispose() { }

            public static implicit operator IntValue(int d) => new(d);
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
        }
    }
}
