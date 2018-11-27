using System;
using Xunit;

namespace Dora.Interception.Test
{
    public class InterceptorAttributeFixture
    {
        [Fact]
        public void AllowMultiple()
        {
            Assert.True(new FooAttribute().AllowMultiple);
            Assert.False(new BarAttribute().AllowMultiple);
        }

        [AttributeUsage(AttributeTargets.Method, AllowMultiple = true)]
        private class FooAttribute : InterceptorAttribute
        {
            public override void Use(IInterceptorChainBuilder builder)
            {
                throw new NotImplementedException();
            }
        }

        [AttributeUsage(AttributeTargets.Method, AllowMultiple = false)]
        private class BarAttribute : InterceptorAttribute
        {
            public override void Use(IInterceptorChainBuilder builder)
            {
                throw new NotImplementedException();
            }
        }

        private class Attribute1 : Attribute { }
        private class Attribute2 : Attribute { }
        private class Attribute3 : Attribute { }

    }
}
