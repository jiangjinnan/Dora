﻿using System.Reflection;

namespace Dora.Interception
{
    /// <summary>
    /// Provider to get the interceptors applied specified method.
    /// </summary>
    public interface IInterceptorProvider
    {
        /// <summary>
        /// Determines specified method can be intercepted.
        /// </summary>
        /// <param name="targetType">Target type.</param>
        /// <param name="method">The method to check.</param>        
        /// <param name="suppressed">A <see cref="Boolean"/>value indicating whether to suppress interception.</param>
        /// <returns>A <see cref="Boolean"/>value indicating specified method is intercepted.</returns>
        bool CanIntercept(Type targetType, MethodInfo method, out bool suppressed);

        /// <summary>
        /// Gets the interceptors applied specified method.
        /// </summary>
        /// <param name="targetType">Target type.</param>
        /// <param name="method">The target method.</param>
        /// <returns>The <see cref="Sortable{InvokeDelegate}"/> represents the applied interceptors.</returns>
        IEnumerable<Sortable<InvokeDelegate>> GetInterceptors(Type targetType, MethodInfo method);

        /// <summary>
        /// Validates and ensure interceptors are applied to approriate members of specified type.
        /// </summary>
        /// <param name="methodValidator">A delegate used to ensure the method to which the interceptors are applied is interceptable.</param>
        /// <param name="propertyValidator">A delegate used to ensure the property to which the interceptors are applied is interceptable.</param>
        /// <param name="targetType">The type whose methods may be intercepted.</param>
        void Validate(Type targetType, Action<MethodInfo> methodValidator, Action<PropertyInfo> propertyValidator);
    }
}
