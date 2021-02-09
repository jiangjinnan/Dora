using System;
using System.Collections.Generic;
using System.Reflection;

namespace Dora.Interception
{
    /// <summary>
    /// Assigner to assign interceptor to target methods.
    /// </summary>
    public interface IInterceptorAssigner
    {
        /// <summary>
        /// Gets the assigned methods.
        /// </summary>
        /// <returns>A typed method-order pairs.</returns>
        IDictionary<Type, IDictionary<MethodInfo, int>> GetAssignedMethods();

        /// <summary>
        /// Assigns interceptor to target type and method.
        /// </summary>
        /// <param name="targetType">The target type..</param>
        /// <param name="method">The target method.</param>
        /// <param name="order">The order.</param>
        /// <returns>The current assigner.</returns>
        IInterceptorAssigner AssignTo(Type targetType, MethodInfo method, int order);
    }
}
