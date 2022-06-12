using System;
using System.Reflection;
using Xunit.Sdk;

namespace Dora.Interception.Test
{
    [AttributeUsage(AttributeTargets.Class)]
    public class InterceptorResetAttribute: BeforeAfterTestAttribute
    {
        public override void Before(MethodInfo methodUnderTest) => Interceptor.Reset();
    }
}
