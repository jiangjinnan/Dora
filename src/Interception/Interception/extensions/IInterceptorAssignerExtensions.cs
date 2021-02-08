using System;
using System.Reflection;

namespace Dora.Interception
{
    public static class IInterceptorAssignerExtensions
    {
        public static IInterceptorAssigner For<TTarget>(this IInterceptorAssigner assigner, MethodInfo methodInfo, int order)
        {
            return assigner.AssignTo(typeof(TTarget), methodInfo, order);
        }
        public static IInterceptorAssigner For<TTarget>(this IInterceptorAssigner assigner, Action<IMethodInterceptorAssigner<TTarget>> assignment)
        {
            var methodAssigner = new MethodInterceptorAssigner<TTarget>();
            assignment(methodAssigner);
            foreach (var kv in methodAssigner.GetAssignedMethods())
            {
                assigner.AssignTo(typeof(TTarget), kv.Key, kv.Value);
            }
            return assigner;
        }
    }
}
