using Microsoft.Extensions.DependencyInjection;
using System;
using System.Threading.Tasks;
using Xunit;
using static Dora.Interception.Test.Interceptor;

namespace Dora.Interception.Test
{
    [InterceptorReset]
    public class CodeGeneratorFixture_Interface_GenericDefinition
    {
        [Fact]
        public void M1() => Test(foobar => foobar.M1(1, 2));


        [Fact]
        public void M2() => Test(foobar => foobar.M2(1, 2));

        [Fact]
        public async void M3() => await TestAsync(async foobar => await foobar.M3(1, 2));


        [Fact]
        public async void M4() => await TestAsync(async foobar => await foobar.M4(1, 2));

        [Fact]
        public async void M5() => await TestAsync(async foobar => await foobar.M5(1, 2));

        [Fact]
        public async void M6() => await TestAsync(async foobar => await foobar.M6(1, 2));


        [Fact]
        public void P1_Get() => Test(foobar => _ = foobar.P1);


        [Fact]
        public void P1_Set() => Test(foobar => foobar.P1 = null!);

        private IFoobar<IntValue, IntValue> GetFoobar() => new ServiceCollection().AddSingleton(typeof(IFoobar<,>), typeof(Foobar<,>)).BuildInterceptableServiceProvider().GetRequiredService<IFoobar<IntValue, IntValue>>();

        private void Test(Action<IFoobar<IntValue, IntValue>> call)
        {
            var foobar = GetFoobar();
            call(foobar);
            Assert.True(EnsureInterceptorInvoked());
        }

        private async Task TestAsync(Func<IFoobar<IntValue, IntValue>, Task> call)
        {
            var foobar = GetFoobar();
            await call(foobar);
            Assert.True(EnsureInterceptorInvoked());
        }

        public interface IFoobar<T1, T2>
            where T1 : IDisposable, IConvertable<int>
            where T2 : IConvertable<int>
        {
            void M1(T1 x, T2 y);
            int M2(T1 x, T2 y);
            Task M3(T1 x, T2 y);
            Task<int> M4(T1 x, T2 y);
            ValueTask M5(T1 x, T2 y);
            ValueTask<int> M6(T1 x, T2 y);

            T2 P1 { get; set; }
        }

        [Foo(Order = 1)]
        [Bar(Order = 2)]
        public class Foobar<T1, T2> : IFoobar<T1, T2>
            where T1 : IDisposable, IConvertable<int>
            where T2 : IConvertable<int>
        {
            public T2 P1 { get; set; }

            public void M1(T1 x, T2 y) { }

            public int M2(T1 x, T2 y)=>x.Convert() + y.Convert();

            public Task M3(T1 x, T2 y) => Task.Delay(1);

            public async Task<int> M4(T1 x, T2 y)
            {
                await Task.Delay(1);
                return x.Convert() + y.Convert();
            }

            public async ValueTask M5(T1 x, T2 y)
            {
                await Task.Delay(1);
            }

            public async ValueTask<int> M6(T1 x, T2 y)
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
    }
}
