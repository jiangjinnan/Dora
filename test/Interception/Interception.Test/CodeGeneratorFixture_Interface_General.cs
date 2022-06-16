using Microsoft.Extensions.DependencyInjection;
using System;
using System.Threading.Tasks;
using Xunit;
using static Dora.Interception.Test.Interceptor;

namespace Dora.Interception.Test
{
    [InterceptorReset]
    public  class CodeGeneratorFixture_Interface_General
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

        public interface IFoobar
        {
            void M1(int x, int y);
            int M2(int x, int y);
            void M3(ref int x, out int y);
            Task M4(int x, int y);
            Task<int> M5(int x, int y);
            ValueTask M6(int x, int y);
            ValueTask<int> M7(int x, int y);
            string P1 { get; set; }
        }

        [Foo(Order = 1)]
        [Bar(Order = 2)]
        public class Foobar : IFoobar
        {
            public string P1 { get; set; } = default!;

            public void M1(int x, int y) { }

            public int M2(int x, int y) => 1;

            public void M3(ref int x, out int y) { y = 1; }

            public Task M4(int x, int y) => Task.Delay(1);

            public async Task<int> M5(int x, int y)
            {
               await Task.Delay(1);
                return 1;
            }

            public async ValueTask M6(int x, int y)
            {
                await Task.Delay(1);
            }

            public async ValueTask<int> M7(int x, int y)
            {
                await Task.Delay(1);
                return 1;
            }
        }

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
    }
}
