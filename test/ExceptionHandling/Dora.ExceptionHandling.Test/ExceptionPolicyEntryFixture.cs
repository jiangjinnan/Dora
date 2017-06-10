using System;
using System.Threading.Tasks;
using Xunit;

namespace Dora.ExceptionHandling.Test
{
    public class ExceptionPolicyEntryFixture
    {
        [Theory]
        [InlineData(null, "1")]
        [InlineData("1", null)]
        public void New_Arguments_Not_Allow_Null(string typeIndicator, string handlerIndicator)
        {
            Type type = typeIndicator == null
                ? null
                : typeof(Exception);
            Func<ExceptionContext, Task> handler = handlerIndicator == null
                ? null
                : new Func<ExceptionContext, Task>(_ => Task.CompletedTask);
            Assert.Throws<ArgumentNullException>(()=>new ExceptionPolicyEntry(type, PostHandlingAction.None, handler));
        }

        [Fact]
        public void New_Normal()
        {
            Func<ExceptionContext, Task> handler = _ => Task.CompletedTask;
            var entry = new ExceptionPolicyEntry(typeof(ArgumentNullException), PostHandlingAction.ThrowOriginal, handler);
            Assert.Same(typeof(ArgumentNullException), entry.ExceptionType);
            Assert.Same(handler, entry.Handler);
            Assert.Equal(PostHandlingAction.ThrowOriginal, entry.PostHandlingAction);

        }
    }
}
