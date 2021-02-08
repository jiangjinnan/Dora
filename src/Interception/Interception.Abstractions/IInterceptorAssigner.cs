using System;
using System.Collections.Generic;
using System.Reflection;

namespace Dora.Interception
{
    public interface IInterceptorAssigner
    {
        IDictionary<Type, IDictionary<MethodInfo, int>> GetAssignedMethods();
        IInterceptorAssigner AssignTo(Type targetType, MethodInfo method, int order);
    }
}
