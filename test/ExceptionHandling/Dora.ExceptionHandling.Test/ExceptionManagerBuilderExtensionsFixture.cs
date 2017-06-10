using Dora.ExceptionHandling.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;
using Xunit;

namespace Dora.ExceptionHandling.Test
{
    public class ExceptionManagerBuilderExtensionsFixture
    {
        public void GetFilters_Argument_Not_Allow_Null()
        {
            IExceptionManagerBuilder builder = null;
            Assert.Throws<ArgumentNullException>(() => builder.GetFilters());
        }

        [Theory]
        [InlineData(null, "1", typeof(MatchAllFilter))]
        [InlineData("1", null, typeof(MatchAllFilter))]
        [InlineData("1", "", typeof(MatchAllFilter))]
        [InlineData("1", " ", typeof(MatchAllFilter))]
        [InlineData("1", "1", null)]
        [InlineData("1", "1", typeof(string))]

        public void UseFilter_Arguments_Not_Allow_Null(string builderIndicator, string name, Type type)
        {
            IExceptionManagerBuilder builder = builderIndicator == null
                ? null
                : new ExceptionManagerBuilder(new ServiceCollection().BuildServiceProvider());
            Assert.ThrowsAny<ArgumentException>(() => builder.UseFilter(name, type));
        }

        [Fact]
        public void Use_And_Get_Filter()
        {
            var builder = new ExceptionManagerBuilder(new ServiceCollection().AddScoped<IFoo, Foo>().BuildServiceProvider());
            builder.UseFilter("foobar", typeof(FoobarFiler), new Bar());
            FoobarFiler fitler = (FoobarFiler)builder.GetFilters()["foobar"];
            Assert.NotNull(fitler.Foo);
            Assert.NotNull(fitler.Bar);
        }

        private interface IFoo { }
        private interface IBar { }
        private class Foo : IFoo { }
        private class Bar : IBar { }
        private class FoobarFiler : IExceptionFilter
        {
            public IFoo Foo { get; }
            public IBar Bar { get; }
            public FoobarFiler(IFoo foo, IBar bar)
            {
                this.Foo = foo;
                this.Bar = bar;
            }
            public bool Match(ExceptionContext context)
            {
                throw new NotImplementedException();
            }
        }

        [Fact]
        public async void LoadSettings()
        {
            var manager = new ServiceCollection()
                .AddExceptionHandling(builder => builder.LoadSettings(@"exception.json"))
                .BuildServiceProvider()
                .GetRequiredService<ExceptionManager>();
            HandlerBase.HandlerChain.Clear();
            await manager.HandleExceptionAsync(new FooException(), "policy1");
            Assert.Equal(6, HandlerBase.HandlerChain.Count);

            HandlerBase.HandlerChain.Clear();
            await manager.HandleExceptionAsync(new BarException(), "policy1");
            Assert.Equal(6, HandlerBase.HandlerChain.Count);

            HandlerBase.HandlerChain.Clear();
            try
            {
                await manager.HandleExceptionAsync(new Exception(), "policy1");
            }
            catch
            { }
            Assert.Equal(4, HandlerBase.HandlerChain.Count);
        }
    }
}
