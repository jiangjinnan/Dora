using System;
using System.Collections.Generic;
using System.Reflection;

namespace Dora.Interception
{
    public class InterceptorAssigner : IInterceptorAssigner
    {
        private readonly Dictionary<Type, IDictionary<MethodInfo, int>> _assignedMethods = new Dictionary<Type, IDictionary<MethodInfo, int>>();

        public IInterceptorAssigner AssignTo(Type targetType, MethodInfo method, int order)
        {
            var methods = _assignedMethods.TryGetValue(targetType, out var value)
                ? value
                : _assignedMethods[targetType] = new Dictionary<MethodInfo, int>();
            methods.Add(method, order);
            return this;
        }

        public IDictionary<Type, IDictionary<MethodInfo, int>> GetAssignedMethods() => _assignedMethods;
    }

}
