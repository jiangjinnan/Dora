using System;
using Xunit;

namespace Dora.ExceptionHandling.Test
{
    public class ExceptionContextFixture
    {
        [Fact]
        public void New_Arguments_Not_Allow_Null()
        {
            Assert.Throws<ArgumentNullException>(() => new ExceptionContext(null));
        }

        [Fact]
        public void New_Normal()
        {
            var ex = new Exception();
            var context = new ExceptionContext(ex);
            Assert.Same(ex, context.OriginalException);
            Assert.Same(ex, context.Exception);
            Assert.NotEqual(Guid.Empty, context.HandlingId);
            Assert.NotNull(context.Properties);
        }
    }
}
