using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;

namespace Dora.Interception
{
    public interface IMethodInterceptorAssigner<TTarget>
    {
        IMethodInterceptorAssigner<TTarget> To(MethodInfo methodInfo, int order);
        IDictionary<MethodInfo, int> GetAssignedMethods();
    }
}
