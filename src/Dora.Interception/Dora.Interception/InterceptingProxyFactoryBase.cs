using Dora.DynamicProxy;
using System;

namespace Dora.Interception
{
    /// <summary>
    /// The default implementation of <see cref="IInterceptingProxyFactory"/>.
    /// </summary>
    /// <seealso cref="Dora.Interception.IInterceptingProxyFactory" />
    public abstract class InterceptingProxyFactoryBase : IInterceptingProxyFactory
    {
        /// <summary>
        /// Gets the interceptor collector.
        /// </summary>
        /// <value>
        /// The interceptor collector.
        /// </value>
        public IInterceptorResolver InterceptorResolver { get; }

        /// <summary>
        /// Get a service provider to get dependency services.
        /// </summary>
        public IServiceProvider ServiceProvider { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="InterceptingProxyFactory"/> class.
        /// </summary>
        /// <param name="interceptorResolver">The interceptor collector.</param>
        /// <param name="serviceProvider">The service provider.</param>
        /// <exception cref="ArgumentNullException">Specified <paramref name="interceptorResolver"/> is null.</exception> 
        /// <exception cref="ArgumentNullException">Specified <paramref name="serviceProvider"/> is null.</exception> 
        public InterceptingProxyFactoryBase(
            IInterceptorResolver interceptorResolver,
            IServiceProvider serviceProvider)
        {
            InterceptorResolver = Guard.ArgumentNotNull(interceptorResolver, nameof(interceptorResolver));
            ServiceProvider = Guard.ArgumentNotNull(serviceProvider, nameof(serviceProvider));
        }

        /// <summary>
        /// Create a proxy wrapping specified target instance to wrap the specified target instance.
        /// </summary>
        /// <param name="typeToIntercept">The type to intercept.</param>
        /// <param name="target">The target instance wrapped by the created proxy.</param>
        /// <returns>The proxy wrapping the specified target instance.</returns>
        public object Wrap(Type typeToIntercept, object target)
        {
            Guard.ArgumentNotNull(typeToIntercept, nameof(typeToIntercept));
            Guard.ArgumentNotNull(target, nameof(target));
            Guard.ArgumentAssignableTo(typeToIntercept, target.GetType(), nameof(target));

            if (!CanIntercept(typeToIntercept))
            {
                return target;
            }  
            var interceptors = InterceptorResolver.GetInterceptors(typeToIntercept, target.GetType());
            if (interceptors.IsEmpty)
            {
                return target;
            }  
            return Wrap(typeToIntercept, target, interceptors);
        }     

        /// <summary>
        /// Creates the specified type to proxy.
        /// </summary>
        /// <param name="typeToIntercept">The type to proxy.</param>
        /// <param name="serviceProvider">The service provider.</param>
        /// <param name="targetAccessor">The target instance accessor.</param>
        /// <returns>
        /// The proxy wrapping the specified target instance.
        /// </returns>
        public object Create(Type typeToIntercept, IServiceProvider serviceProvider, Func<object> targetAccessor = null)
        {
            Guard.ArgumentNotNull(typeToIntercept, nameof(typeToIntercept));
            Guard.ArgumentNotNull(serviceProvider, nameof(serviceProvider));

            if (!CanIntercept(typeToIntercept))
            {
                return targetAccessor?.Invoke() ?? serviceProvider.GetService(typeToIntercept);
            }
            var interceptors = InterceptorResolver.GetInterceptors(typeToIntercept);
            if (interceptors.IsEmpty)
            {
                return targetAccessor?.Invoke() ?? serviceProvider.GetService(typeToIntercept);
            }
            return Create(typeToIntercept, serviceProvider, interceptors);
        }

        /// <summary>
        /// Create a proxy wrapping specified target instance.
        /// </summary>
        /// <param name="typeToIntercept">The type to intercept.</param>
        /// <param name="target">The target.</param>
        /// <param name="interceptors">The interceptors.</param>
        protected abstract object Wrap(Type typeToIntercept, object target, InterceptorDecoration interceptors);

        /// <summary>
        /// Create an interceptable proxy instance.
        /// </summary>
        /// <param name="typeToIntercept">The type to intercept.</param>
        /// <param name="serviceProvider">The <see cref="IServiceProvider"/> used to provide dependent service instances.</param>
        /// <param name="interceptors">The <see cref="InterceptorDecoration"/> representing which interceptors are applied to which members of a type to intercept.</param>
        /// <returns>The interceptable proxy instance.</returns>
        protected abstract object Create(Type typeToIntercept, IServiceProvider serviceProvider, InterceptorDecoration interceptors);

        /// <summary>
        /// Determines whether specified type can be intercepted.
        /// </summary>
        /// <param name="typeToIntercept">The type to intercept.</param>
        /// <returns>
        ///   <c>true</c> if this instance can intercept the specified type to intercept; otherwise, <c>false</c>.
        /// </returns>
        protected virtual bool CanIntercept(Type typeToIntercept)
        {
            Guard.ArgumentNotNull(typeToIntercept, nameof(typeToIntercept));
            return typeToIntercept.IsInterface || !typeToIntercept.IsSealed;
        }
    }
}
