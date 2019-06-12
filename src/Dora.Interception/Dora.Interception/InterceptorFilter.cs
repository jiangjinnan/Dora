using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Dora.Interception
{
    /// <summary>
    /// Represents a custom <see cref="IInterceptorProviderResolver"/> which apply the specified <see cref="IInterceptorProvider"/> to filtered target methods.
    /// </summary>
    /// <seealso cref="Dora.Interception.IInterceptorProviderResolver" />
    public sealed class InterceptorFilter : IInterceptorProviderResolver
    {
        private readonly IInterceptorProvider[] _empty = new IInterceptorProvider[0];
        private readonly Dictionary<IInterceptorProvider, Func<MethodInfo, bool>> _filters = new Dictionary<IInterceptorProvider, Func<MethodInfo, bool>>();

        /// <summary>
        /// Gets the interceptor providers applied to the specified method.
        /// </summary>
        /// <param name="targetType">Type of the target.</param>
        /// <param name="targetMethod">The method to which the interceptor providers are applied to.</param>
        /// <returns>
        /// The interceptor providers applied to the specified method.
        /// </returns>
        public IInterceptorProvider[] GetInterceptorProvidersForMethod(Type targetType, MethodInfo targetMethod)
        => _filters.Where(it => it.Value(targetMethod)).Select(it => it.Key).ToArray();

        /// <summary>
        /// Gets the interceptor providers applied to the specified method.
        /// </summary>
        /// <param name="targetType">Type of the target.</param>
        /// <param name="targetProperty">The property to which the interceptor providers are applied to.</param>
        /// <param name="getOrSet">The property's GET or SET method.</param>
        /// <returns>
        /// The interceptor providers applied to the specified method.
        /// </returns>
        public IInterceptorProvider[] GetInterceptorProvidersForProperty(Type targetType, PropertyInfo targetProperty, PropertyMethod getOrSet)
        {
            switch (getOrSet)
            {
                case PropertyMethod.Get:
                    return GetInterceptorProvidersForMethod(targetType, targetProperty.GetMethod);
                case PropertyMethod.Set:
                    return GetInterceptorProvidersForMethod(targetType, targetProperty.SetMethod);
                default:
                    return GetInterceptorProvidersForMethod(targetType, targetProperty.GetMethod)
                        .Union(GetInterceptorProvidersForMethod(targetType, targetProperty.SetMethod))
                        .ToArray();
            }
        }

        /// <summary>
        /// Gets the interceptor providers applied to the specified type.
        /// </summary>
        /// <param name="targetType">The type to which the interceptor providers are applied to.</param>
        /// <returns>
        /// The interceptor providers applied to the specified type.
        /// </returns>
        public IInterceptorProvider[] GetInterceptorProvidersForType(Type targetType) => _empty;

        /// <summary>
        /// Determine whether the specified type should be intercepted.
        /// </summary>
        /// <param name="targetType">The type to be checked for interception.</param>
        /// <returns>
        /// A <see cref="T:System.Nullable`1" /> indicating whether the specified type should be intercepted.
        /// </returns>
        public bool? WillIntercept(Type targetType)
        {
            if (targetType.GetCustomAttributes<NonInterceptableAttribute>().Any())
            {
                return false;
            }
            return null;
        }

        /// <summary>
        /// Adds the specified interceptor provider.
        /// </summary>
        /// <param name="interceptorProvider">The interceptor provider.</param>
        /// <param name="filter">The <see cref="Func{MethodInfo, Boolean}"/> determining whether specified <see cref="IInterceptorProvider"/> should applied to specified <see cref="MethodInfo"/>.</param>
        /// <returns>Add a new <see cref="IInterceptorProvider"/> with specified filter.</returns>
        public InterceptorFilter Add(IInterceptorProvider interceptorProvider, Func<MethodInfo, bool> filter)
        {
            Guard.ArgumentNotNull(interceptorProvider, nameof(interceptorProvider));
            Guard.ArgumentNotNull(filter, nameof(filter));
            _filters.Add(interceptorProvider, filter);
            return this;
        }
    }
}
