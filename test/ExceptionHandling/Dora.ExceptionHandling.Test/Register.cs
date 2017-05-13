using Microsoft.Extensions.DependencyInjection;
using System;
using System.Threading.Tasks;
using Xunit;

namespace Dora.ExceptionHandling.Test
{
    public class Register
    {
        private static string _foo;
        private static string _bar;
        private static string _baz;

        [Fact]
        public async void HandleException()
        {
            _foo = _bar = _baz = null;
            ExceptionManager manager = new ServiceCollection()
                .AddSingleton<IFoo, Foobarbaz>()
                .AddSingleton<IBar, Foobarbaz>()
                .AddSingleton<IBaz, Foobarbaz>()
                .AddExceptionHandling(managerBuilder => managerBuilder
                    .AddPolicy("default", policyBuilder => policyBuilder
                        .For<Exception>(PostHandlingAction.None, handlerBuilder =>handlerBuilder.Use<Foobar>().Use<Baz>())))
                .BuildServiceProvider()
                .GetRequiredService<ExceptionManager>();
            await manager.HandleExceptionAsync(new Exception(), "default");
            Assert.Equal(_foo, "foo");
            Assert.Equal(_bar, "bar");
            Assert.Equal(_baz, "baz");
        }

        public interface IFoo
        {
            void Set();
        }
        public interface IBar
        {
            void Set();
        }
        public interface IBaz
        {
            void Set();
        }
        public class Foobarbaz : IFoo, IBar, IBaz
        {
            void IFoo.Set()
            {
                _foo = "foo";
            }

            void IBar.Set()
            {
                _bar = "bar";
            }

            void IBaz.Set()
            {
                _baz = "baz";
            }
        }
        public class Foobar
        {
            private IFoo _foo;
            public Foobar(IFoo foo)
            {
                _foo = foo;
            }

            public Task HandleExceptionAsync(ExceptionContext context, IBar bar)
            {
                _foo.Set();
                bar.Set();
                return Task.CompletedTask;
            }        
        }

        public class Baz
        {
            public Task HandleExceptionAsync(ExceptionContext context, IBaz baz)
            {
                baz.Set();
                return Task.CompletedTask;
            }
        }
    }
}
