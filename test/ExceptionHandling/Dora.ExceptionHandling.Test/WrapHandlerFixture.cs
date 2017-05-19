using System;
using System.Collections.Generic;
using System.Text;
using Xunit;

namespace Dora.ExceptionHandling.Test
{
    public class WrapHandlerFixture
    {
        [Theory]
        [InlineData(null, "1")]
        [InlineData("1", null)]
        public void New_Arguments_Not_Allow_Null(string typeIndicator, string message)
        {
            Type exceptionType = typeIndicator == null
                ? null
                : typeof(Exception);
            Assert.Throws<ArgumentNullException>(() => new WrapHandler(exceptionType, message));
        }

        [Theory]
        [InlineData(typeof(int))]
        [InlineData(typeof(FooException))]
        public void New_Invalid_Exception_Type(Type exceptionType)
        {
            Assert.Throws<ArgumentException>(() => new WrapHandler(exceptionType, "fobar"));
        }

        [Theory]
        [InlineData("")]
        [InlineData(" ")]
        public void New_Message_Not_Empty(string message)
        {
            Assert.Throws<ArgumentException>(() => new WrapHandler(typeof(Exception), message));
        }

        [Fact]
        public async void HandleExceptionAsync_Arguments_Not_Allow_Null()
        {
            await Assert.ThrowsAsync<ArgumentNullException>(() => new WrapHandler(typeof(Exception), "foobar").HandleExceptionAsync(null));
        }

        [Fact]
        public async void HandleExceptionAsync_Normal()
        {
            var context = new ExceptionContext(new InvalidOperationException());
            await new WrapHandler(typeof(BarException), "foobar").HandleExceptionAsync(context);
            Assert.IsType<BarException>(context.Exception);
            Assert.Equal("foobar", context.Exception.Message);
            Assert.IsType<InvalidOperationException>(context.Exception.InnerException);
        }

        public class FooException : Exception { }
        public class BarException : Exception
        {
            public BarException(string message, Exception innerException) : base(message, innerException)
            { }
        }
    }
}
