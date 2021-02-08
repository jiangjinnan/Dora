using Dora.Interception;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace Interception.Test
{
    public class InterceptorFixture
    {
        [Fact]
        public void Construct()
        {
            Assert.Throws<ArgumentNullException>(() => new Interceptor(null, true));
            InterceptorDelegate @delegate = next => (context => Task.CompletedTask);
            var interceptor = new Interceptor(@delegate, true);
            Assert.Same(@delegate, interceptor.Delegate);
            Assert.True(interceptor.CaptureArguments);
        }
    }
}
