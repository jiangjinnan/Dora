using System.Collections.Generic;
using System.Reflection;

namespace Dora.Interception
{
    /// <summary>
    /// Assigner to assign interceptor to target methods.
    /// </summary>
    /// <typeparam name="TTarget">The target type.</typeparam>
    public interface IMethodInterceptorAssigner<TTarget>
    {
        /// <summary>
        /// Assigns interceptor to specified method.
        /// </summary>
        /// <param name="targetMethod">The target method.</param>
        /// <param name="order">The order.</param>
        /// <returns>The current assigner.</returns>
        IMethodInterceptorAssigner<TTarget> To(MethodInfo targetMethod, int order);

        /// <summary>
        /// Gets the assigned target methods.
        /// </summary>
        /// <returns>A dictionary whose entry is method-order pair.</returns>
        IDictionary<MethodInfo, int> GetAssignedMethods();
    }
}
