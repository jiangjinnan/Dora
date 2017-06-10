using Microsoft.Extensions.DependencyInjection;
using System;
using System.Threading.Tasks;
using Xunit;

namespace Dora.ExceptionHandling.Test
{
    public class ExceptionHandlerBuilderFixture
    {
        private static string _flag = "";


        [Fact]
        public void New_Arguments_Not_Allow_Null()
        {
            Assert.Throws<ArgumentNullException>(() => new ExceptionHandlerBuilder(null));
        }

        [Fact]
        public void New_Normal()
        {
            var provider = new ServiceCollection().BuildServiceProvider();
            Assert.Same(provider, new ExceptionHandlerBuilder(provider).ServiceProvider);
        }

        [Fact]
        public void Use_Arguments_Not_Allow_Null()
        {
            var provider = new ServiceCollection().BuildServiceProvider();
            Assert.Throws<ArgumentNullException>(() => new ExceptionHandlerBuilder(provider).Use(null));
        }

        [Fact]
        public async void Use_And_Build()
        {
            var builder = new ExceptionHandlerBuilder(new ServiceCollection().BuildServiceProvider());
            builder.Use( _ => { _flag = ""; return Task.CompletedTask; });
            builder.Use(_ => { _flag +="1"; return Task.CompletedTask; });
            builder.Use(_ => { _flag +="2"; return Task.CompletedTask; });
            builder.Use(_ => { _.Properties.Add("flag", _flag); return Task.CompletedTask; });
            var handler = builder.Build();
            var context = new ExceptionContext(new Exception());
            await handler(context);
            Assert.Equal("12", context.Properties["flag"]);
        }
    }
}
