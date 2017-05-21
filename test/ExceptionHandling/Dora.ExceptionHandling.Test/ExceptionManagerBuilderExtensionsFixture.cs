using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Text;
using Xunit;

namespace Dora.ExceptionHandling.Test
{
    public class ExceptionManagerBuilderExtensionsFixture
    {
        [Fact]
        public async void HandleException()
        {
            var manager = new ServiceCollection()
                .AddExceptionHandling(builder => builder.LoadSettings("exception.json"))
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
