using System.Reflection;

namespace Dora.Interception
{
    /// <summary>
    /// Builder used to build the invoker based on specified target method call.
    /// </summary>
    public interface IMethodInvokerBuilder
    {
        /// <summary>
        /// Builds the invoker based on specified target method call.
        /// </summary>
        /// <param name="method">The target method.</param>
        /// <param name="targetMethodInvoker">The target method call.</param>
        /// <returns>The created <see cref="InvokeDelegate"/> used to call the applied interceptor chain and target method. </returns>
        InvokeDelegate Build(MethodInfo method, InvokeDelegate targetMethodInvoker);

        /// <summary>
        /// Determines whether the specified type is interceptable.
        /// </summary>
        /// <param name="method">The target method.</param>
        /// <returns>
        ///   <c>true</c> if the specified type is interceptable; otherwise, <c>false</c>.
        /// </returns>
        bool CanIntercept(MethodInfo method);
    }
}
