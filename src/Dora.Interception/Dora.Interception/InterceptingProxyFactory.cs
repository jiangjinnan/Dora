using Dora.DynamicProxy;
using System;

namespace Dora.Interception
{
    /// <summary>
    /// The default implementation of <see cref="IInterceptingProxyFactory"/>.
    /// </summary>
    /// <seealso cref="Dora.Interception.IInterceptingProxyFactory" />
    public class InterceptingProxyFactory : InterceptingProxyFactoryBase
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
            IServiceProvider serviceProvider) :base(interceptorResolver, serviceProvider)
        {
            InstanceDynamicProxyGenerator = Guard.ArgumentNotNull(instanceDynamicProxyGenerator, nameof(instanceDynamicProxyGenerator));
            TypeDynamicProxyGenerator = Guard.ArgumentNotNull(typeDynamicProxyGenerator, nameof(typeDynamicProxyGenerator));
        }          

        /// <summary>
        /// Create a proxy wrapping specified target instance.
        /// </summary>
        /// <param name="typeToIntercept">The type to intercept.</param>
        /// <param name="target">The target.</param>
        /// <param name="interceptors">The interceptors.</param>
        /// <returns></returns>
        protected override object Wrap(Type typeToIntercept, object target, InterceptorDecoration interceptors)
        {
            return InstanceDynamicProxyGenerator.Wrap(typeToIntercept, target, interceptors);
        }

        /// <summary>
        /// Create an interceptable proxy instance.
        /// </summary>
        /// <param name="typeToIntercept">The type to intercept.</param>
        /// <param name="serviceProvider">The <see cref="IServiceProvider" /> used to provide dependent service instances.</param>
        /// <param name="interceptors">The <see cref="InterceptorDecoration" /> representing which interceptors are applied to which members of a type to intercept.</param>
        /// <returns>
        /// The interceptable proxy instance.
        /// </returns>
        protected override object Create(Type typeToIntercept, IServiceProvider serviceProvider, InterceptorDecoration interceptors)
        {
            return TypeDynamicProxyGenerator.Create(typeToIntercept, interceptors, serviceProvider);
        }
    }
}
