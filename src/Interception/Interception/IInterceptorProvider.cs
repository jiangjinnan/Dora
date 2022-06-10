using System.Reflection;

namespace Dora.Interception
{
    /// <summary>
    /// Provider to get the interceptors applied specified method.
    /// </summary>
    public interface IInterceptorProvider
    {
        /// <summary>
        /// Gets the interceptors applied specified method.
        /// </summary>
        /// <param name="method">The target method.</param>
        /// <returns>The <see cref="Sortable{InvokeDelegate}"/> represents the applied interceptors.</returns>
        IEnumerable<Sortable<InvokeDelegate>> GetInterceptors(MethodInfo method);

        /// <summary>
        /// Determines whether to suppress interception against specified method.
        /// </summary>
        /// <param name="method">The target method.</param>
        /// <returns>The <see cref="Boolean"/>value indicating whether to suppress interception against specified method.</returns>
        bool IsInterceptionSuppressed(MethodInfo method);

        /// <summary>
        /// Validates and ensure interceptors are applied to approriate members of specified type.
        /// </summary>
        /// <param name="methodValidator">A delegate used to ensure the method to which the interceptors are applied is interceptable.</param>
        /// <param name="propertyValidator">A delegate used to ensure the property to which the interceptors are applied is interceptable.</param>
        /// <param name="type">The type whose methods may be intercepted.</param>
        void Validate(Type type, Action<MethodInfo> methodValidator, Action<PropertyInfo> propertyValidator) {}
    }
}
