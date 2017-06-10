using Microsoft.Extensions.DependencyInjection;
using System;
using System.Threading.Tasks;
using Xunit;

namespace Dora.ExceptionHandling.Test
{
    public class ExceptionHandlerBuilderExtensionsFixture
    {
        [Theory]
        [InlineData(null, "1")]
        [InlineData("1", null)]
        public void Use_Arguments_Not_Allow_Null(string builderIndicator, string typeIndicator)
        {
            IExceptionHandlerBuilder builder = builderIndicator == null
                ? null
                : new ExceptionHandlerBuilder(new ServiceCollection().BuildServiceProvider());
            Type type = typeIndicator == null
                ? null
                : typeof(string);
            Assert.Throws<ArgumentNullException>(() => builder.Use(type));
        }

        [Fact]
        public void Use_Invalid_Handler_Type()
        {
            var builder = new ExceptionHandlerBuilder(new ServiceCollection().BuildServiceProvider());
            Assert.Throws<ArgumentException>(() => builder.Use(typeof(InvalidHandler)));
            Assert.Throws<ArgumentException>(() => builder.Use<InvalidHandler>());
        }

        [Fact]
        public async void Use()
        {
            var serviceProvider = new ServiceCollection()
                .AddScoped<IFoo, Foo>()
                .AddScoped<IBaz, Baz>()
                .BuildServiceProvider();

            var handler = new ExceptionHandlerBuilder(serviceProvider)
                .Use(typeof(ValidHandler), new Bar())
                .Build();
            await handler(new ExceptionContext(new Exception()));
            Assert.NotNull(_foo);
            Assert.NotNull(_baz);

            _foo = null;
            _bar = null;
            _baz = null;

            handler = new ExceptionHandlerBuilder(serviceProvider)
                .Use<ValidHandler>(new Bar())
                .Build();
            await handler(new ExceptionContext(new Exception()));
            Assert.NotNull(_foo);
            Assert.NotNull(_bar);
            Assert.NotNull(_baz);

            _foo = null;
            _bar = null;
            _baz = null;

            handler = new ExceptionHandlerBuilder(serviceProvider)
                .Use<ValidHandler>(_=>false, new Bar())
                .Build();
            await handler(new ExceptionContext(new Exception()));
            Assert.Null(_foo);
            Assert.Null(_bar);
            Assert.Null(_baz);
        }

        public class InvalidHandler
        {
            public void HandleExceptionAsync(ExceptionContext context) { }
        }

        private interface IFoo { }
        private interface IBar { }
        private interface IBaz { }

        private class Foo : IFoo { }
        private class Bar : IBar { }
        private class Baz : IBaz { }

        private static IFoo _foo;
        private static IBar _bar;
        private static IBaz _baz;


        private class ValidHandler
        {
            public ValidHandler(IFoo foo, IBar bar)
            {
                _foo = foo;
                _bar = bar;
            }

            public  Task HandleExceptionAsync(ExceptionContext context, IBaz baz)
            {
                _baz = baz;
                return Task.CompletedTask;
            }
        }
    }
}
