using System;
using System.Collections.Generic;
using System.Text;
using Xunit;

namespace Dora.DynamicProxy.Test
{
    public class DefaultInvocationContextFixture
    {
        [Fact]
        public void GetTargetMethod()
        {
            var conext = new DefaultInvocationContext(
                typeof(IFoobar).GetMethod("Invoke"), new Foo(), new Bar(), new object[] { "foobar" });
            Assert.Same(conext.TargetMethod, typeof(Bar).GetMethod("Invoke"));
        }

        private interface IFoobar
        {
            void Invoke();
        }

        private class Foo : IFoobar
        {
            public void Invoke()
            {
                throw new NotImplementedException();
            }
        }

        private class Bar : IFoobar
        {
            public void Invoke()
            {
                throw new NotImplementedException();
            }
        }
    }
}
