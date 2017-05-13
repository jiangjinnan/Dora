using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace Dora.ExceptionHandling.Test
{
    public class ExceptionPolicyBuilderFixture
    {
        [Fact]
        public void New_Arguments_Not_Allow_Null()
        {
            Assert.Throws<ArgumentNullException>(() => new ExceptionPolicyBuilder(null));
        }

        [Fact]
        public void New_Normal()
        {
            var serviceProvider = new ServiceCollection().BuildServiceProvider();
            Assert.Same(serviceProvider, new ExceptionPolicyBuilder(serviceProvider).ServiceProvider);
        }

        [Theory]
        [InlineData(null, "1")]
        [InlineData("1", null)]
        public void For_Arguments_Not_Allow_Null(string typeIndicator, string configureIndicator)
        {
            Type exceptionType = typeIndicator == null
                ? null
                : typeof(Exception);
            Action<IExceptionHandlerBuilder> configure = configureIndicator == null
                ? null as Action<IExceptionHandlerBuilder>
                : _ => { };
            var builder = new ExceptionPolicyBuilder(new ServiceCollection().BuildServiceProvider());
            Assert.Throws<ArgumentNullException>(() => builder.For(exceptionType, PostHandlingAction.None, configure));
        }

        [Fact]
        public void For_Must_Be_Exception_Type()
        {
            var builder = new ExceptionPolicyBuilder(new ServiceCollection().BuildServiceProvider());
            Assert.Throws<ArgumentException>(()=>builder.For(typeof(int), PostHandlingAction.None, _ => { }));
        }

        [Theory]
        [InlineData(null, "1")]
        [InlineData("1", null)]
        public void Pre_Arguments_Not_Allow_Null(string predicateIndicator, string configureIndicator)
        {
            Func<Exception, bool> predicate = predicateIndicator == null
                ? null as Func<Exception, bool>
                : _ => true;
            Action<IExceptionHandlerBuilder> configure = configureIndicator == null
                ? null as Action<IExceptionHandlerBuilder>
                : _ => { };
            var builder = new ExceptionPolicyBuilder(new ServiceCollection().BuildServiceProvider());
            Assert.Throws<ArgumentNullException>(() => builder.Pre(predicate,  configure));
        }

        [Theory]
        [InlineData(null, "1")]
        [InlineData("1", null)]
        public void Post_Arguments_Not_Allow_Null(string predicateIndicator, string configureIndicator)
        {
            Func<Exception, bool> predicate = predicateIndicator == null
                ? null as Func<Exception, bool>
                : _ => true;
            Action<IExceptionHandlerBuilder> configure = configureIndicator == null
                ? null as Action<IExceptionHandlerBuilder>
                : _ => { };
            var builder = new ExceptionPolicyBuilder(new ServiceCollection().BuildServiceProvider());
            Assert.Throws<ArgumentNullException>(() => builder.Post(predicate, configure));
        }

        [Fact]
        public async void Build()
        {
            var builder = new ExceptionPolicyBuilder(new ServiceCollection().BuildServiceProvider());
            builder.For<FoobarException>(PostHandlingAction.ThrowNew, _ => _.Use(context=> { _flag += "Foobar:"; return Task.CompletedTask; }));
            builder.For<FooException>(PostHandlingAction.ThrowNew, _ => _.Use(context => { _flag += "Foo:"; return Task.CompletedTask; }));
            builder.For<BarException>(PostHandlingAction.ThrowNew, _ => _.Use(context => { _flag += "Bar:"; return Task.CompletedTask; }));
            builder.For<BazException>(PostHandlingAction.ThrowNew, _ => _.Use(context => { _flag += "Baz:"; return Task.CompletedTask; }));

            builder.Pre(ex => ex is FoobarException, _ => _.Use(context => { _flag += "Foobar:"; return Task.CompletedTask; }));
            builder.Pre(ex => ex is BazException, _ => _.Use(context => { _flag += "Baz:"; return Task.CompletedTask; }));
            builder.Post(ex => ex is FoobarException, _ => _.Use(context => { _flag += "Foobar:"; return Task.CompletedTask; }));
            builder.Post(ex => ex is BazException, _ => _.Use(context => { _flag += "Baz:"; return Task.CompletedTask; }));


            var policy = builder.Build();
            PostHandlingAction action;
            _flag = "";
            await policy.CreateExceptionHandler(new FoobarException(), out action)(new ExceptionContext(new FoobarException()));
            Assert.Equal("Foobar:Foobar:Foobar:", _flag);


            _flag = "";
            await policy.CreateExceptionHandler(new FooException(), out action)(new ExceptionContext(new FooException()));
            Assert.Equal("Foobar:Foo:Foobar:", _flag);

            _flag = "";
            await policy.CreateExceptionHandler(new BarException(), out action)(new ExceptionContext(new BarException()));
            Assert.Equal("Foobar:Bar:Foobar:", _flag);

            _flag = "";
            await policy.CreateExceptionHandler(new BazException(), out action)(new ExceptionContext(new BazException()));
            Assert.Equal("Baz:Baz:Baz:", _flag);
        }

        private static string _flag;
        private class FoobarException : Exception { }
        private class FooException : FoobarException { }
        private class BarException : FoobarException { }
        private class BazException : Exception { }
    }
}
