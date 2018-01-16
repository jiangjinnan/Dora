using Dora.DynamicProxy;
using System;

namespace Dora.Interception
{
    /// <summary>
    /// 
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
        public IInterceptorCollector InterceptorCollector { get; }

        /// <summary>
        /// Get a service provider to get dependency services.
        /// </summary>
        public IServiceProvider ServiceProvider { get; }


        /// <summary>
        /// Initializes a new instance of the <see cref="InterceptingProxyFactory"/> class.
        /// </summary>
        /// <param name="instanceDynamicProxyGenerator">The instance dynamic proxy generator.</param>
        /// <param name="typeDynamicProxyGenerator">The type dynamic proxy generator.</param>
        /// <param name="interceptorCollector">The interceptor collector.</param>
        /// <param name="serviceProvider">The service provider.</param>
        public InterceptingProxyFactory(
            IInstanceDynamicProxyGenerator  instanceDynamicProxyGenerator,
             ITypeDynamicProxyGenerator typeDynamicProxyGenerator,
            IInterceptorCollector interceptorCollector,
            IServiceProvider serviceProvider)
        {
            this.InstanceDynamicProxyGenerator = Guard.ArgumentNotNull(instanceDynamicProxyGenerator, nameof(instanceDynamicProxyGenerator));
            this.TypeDynamicProxyGenerator = Guard.ArgumentNotNull(typeDynamicProxyGenerator, nameof(typeDynamicProxyGenerator));
            this.InterceptorCollector = Guard.ArgumentNotNull(interceptorCollector, nameof(interceptorCollector));
            this.ServiceProvider = Guard.ArgumentNotNull(serviceProvider, nameof(serviceProvider));
        }

        /// <summary>
        /// Wraps the specified type to proxy.
        /// </summary>
        /// <param name="typeToProxy">The type to proxy.</param>
        /// <param name="target">The target.</param>
        /// <returns></returns>
        public object Wrap(Type typeToProxy, object target)
        {
            Guard.ArgumentNotNull(typeToProxy, nameof(typeToProxy));
            Guard.ArgumentNotNull(target,nameof(target));
            Guard.ArgumentAssignableTo(typeToProxy, target.GetType(), nameof(target));

            if (!this.InstanceDynamicProxyGenerator.CanIntercept(typeToProxy))
            {
                return target;
            }

            var interceptors = this.InterceptorCollector.GetInterceptors(typeToProxy, target.GetType());
            if (interceptors.IsEmpty)
            {
                return target;
            }

            return this.InstanceDynamicProxyGenerator.Wrap(typeToProxy, target, interceptors);
        }

        /// <summary>
        /// Creates the specified type to proxy.
        /// </summary>
        /// <param name="typeToProxy">The type to proxy.</param>
        /// <param name="serviceProvider">The service provider.</param>
        /// <returns></returns>
        public object Create(Type typeToProxy, IServiceProvider serviceProvider)
        {
            Guard.ArgumentNotNull(typeToProxy, nameof(typeToProxy));
            Guard.ArgumentNotNull(serviceProvider, nameof(serviceProvider));
            var interceptors = this.InterceptorCollector.GetInterceptors(typeToProxy);
            return this.TypeDynamicProxyGenerator.Create(typeToProxy, interceptors, serviceProvider);
        }
    }
}
