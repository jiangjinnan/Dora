using Dora.DynamicProxy;
using System;

namespace Dora.Interception
{
    /// <summary>
    /// The default implementation of <see cref="IInterceptingProxyFactory"/>.
    /// </summary>
    /// <seealso cref="Dora.Interception.IInterceptingProxyFactory" />
    public class InterceptingProxyFactory : IInterceptingProxyFactory
    {
        /// <summary>
        /// Gets the instance dynamic proxy generator.
        /// </summary>
        /// <value>
        /// The instance dynamic proxy generator.
        /// </value>
        public IInstanceDynamicProxyGenerator InstanceDynamicProxyGenerator { get; }

        /// <summary>
        /// Gets the type dynamic proxy generator.
        /// </summary>
        /// <value>
        /// The type dynamic proxy generator.
        /// </value>
        public ITypeDynamicProxyGenerator TypeDynamicProxyGenerator { get; }

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
        /// <param name="instanceDynamicProxyGenerator">The instance dynamic proxy generator.</param>
        /// <param name="typeDynamicProxyGenerator">The type dynamic proxy generator.</param>
        /// <param name="interceptorResolver">The interceptor collector.</param>
        /// <param name="serviceProvider">The service provider.</param>
        /// <exception cref="ArgumentNullException">Specified <paramref name="instanceDynamicProxyGenerator"/> is null.</exception> 
        /// <exception cref="ArgumentNullException">Specified <paramref name="typeDynamicProxyGenerator"/> is null.</exception> 
        /// <exception cref="ArgumentNullException">Specified <paramref name="interceptorResolver"/> is null.</exception> 
        /// <exception cref="ArgumentNullException">Specified <paramref name="serviceProvider"/> is null.</exception> 
        public InterceptingProxyFactory(
            IInstanceDynamicProxyGenerator instanceDynamicProxyGenerator,
            ITypeDynamicProxyGenerator typeDynamicProxyGenerator,
            IInterceptorResolver interceptorResolver,
            IServiceProvider serviceProvider)
        {
            InstanceDynamicProxyGenerator = Guard.ArgumentNotNull(instanceDynamicProxyGenerator, nameof(instanceDynamicProxyGenerator));
            TypeDynamicProxyGenerator = Guard.ArgumentNotNull(typeDynamicProxyGenerator, nameof(typeDynamicProxyGenerator));
            InterceptorResolver = Guard.ArgumentNotNull(interceptorResolver, nameof(interceptorResolver));
            ServiceProvider = Guard.ArgumentNotNull(serviceProvider, nameof(serviceProvider));
        } 

        /// <summary>
        /// Create a proxy wrapping specified target instance.
        /// </summary>
        /// <param name="typeToIntercept">The declaration type of proxy to create.</param>
        /// <param name="target">The target instance wrapped by the created proxy.</param>
        /// <returns>
        /// The proxy wrapping the specified target instance.
        /// </returns>
        public object Wrap(Type typeToIntercept, object target)
        {
            Guard.ArgumentNotNull(typeToIntercept, nameof(typeToIntercept));
            Guard.ArgumentNotNull(target,nameof(target));
            Guard.ArgumentAssignableTo(typeToIntercept, target.GetType(), nameof(target));

            if (!InstanceDynamicProxyGenerator.CanIntercept(typeToIntercept))
            {
                return target;
            }

            var interceptors = InterceptorResolver.GetInterceptors(typeToIntercept, target.GetType());
            if (interceptors.IsEmpty)
            {
                return target;
            }

            return InstanceDynamicProxyGenerator.Wrap(typeToIntercept, target, interceptors);
        }

        /// <summary>
        /// Creates the specified type to proxy.
        /// </summary>
        /// <param name="typeToIntercept">The type to proxy.</param>
        /// <returns>
        /// The proxy wrapping the specified target instance.
        /// </returns>
        public object Create(Type typeToIntercept)
        {    
            Guard.ArgumentNotNull(typeToIntercept, nameof(typeToIntercept));
            var interceptors = InterceptorResolver.GetInterceptors(typeToIntercept);             
            return TypeDynamicProxyGenerator.Create(typeToIntercept, interceptors, ServiceProvider);
        }
    }
}
