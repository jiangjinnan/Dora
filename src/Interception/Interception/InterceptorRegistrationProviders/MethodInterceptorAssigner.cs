using System.Collections.Generic;
using System.Reflection;

namespace Dora.Interception
{
    public class MethodInterceptorAssigner<TTarget> : IMethodInterceptorAssigner<TTarget>
    {
        private readonly Dictionary<MethodInfo, int> _assignedMethods = new Dictionary<MethodInfo, int>();
        public IDictionary<MethodInfo, int> GetAssignedMethods() => _assignedMethods;
        public IMethodInterceptorAssigner<TTarget> To(MethodInfo methodInfo, int order)
        {
            _assignedMethods[methodInfo] = order;
            return this;
        }
    }
}
