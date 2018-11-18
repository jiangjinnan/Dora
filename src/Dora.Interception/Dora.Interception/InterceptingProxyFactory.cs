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
            this.InstanceDynamicProxyGenerator = Guard.ArgumentNotNull(instanceDynamicProxyGenerator, nameof(instanceDynamicProxyGenerator));
            this.TypeDynamicProxyGenerator = Guard.ArgumentNotNull(typeDynamicProxyGenerator, nameof(typeDynamicProxyGenerator));
            this.InterceptorResolver = Guard.ArgumentNotNull(interceptorResolver, nameof(interceptorResolver));
            this.ServiceProvider = Guard.ArgumentNotNull(serviceProvider, nameof(serviceProvider));
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

            if (!this.InstanceDynamicProxyGenerator.CanIntercept(typeToIntercept))
            {
                return target;
            }

            var interceptors = this.InterceptorResolver.GetInterceptors(typeToIntercept, target.GetType());
            if (interceptors.IsEmpty)
            {
                return target;
            }

            return this.InstanceDynamicProxyGenerator.Wrap(typeToIntercept, target, interceptors);
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
            return serviceProvider.GetService(typeToIntercept);

            Guard.ArgumentNotNull(typeToIntercept, nameof(typeToIntercept));
            Guard.ArgumentNotNull(serviceProvider, nameof(serviceProvider));
            var interceptors = this.InterceptorResolver.GetInterceptors(typeToIntercept);
            if (interceptors.IsEmpty && targetAccessor != null)
            {
                return targetAccessor();
            }
            return this.TypeDynamicProxyGenerator.Create(typeToIntercept, interceptors, serviceProvider);
        }
    }
}
