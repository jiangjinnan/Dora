using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace Dora.Interception
{
    /// <summary>
    /// The base class of all concrete proxy factory classes.
    /// </summary>
    public abstract class ProxyFactory : IProxyFactory
    {
        private static Dictionary<Type, Dictionary<MethodInfo, InterceptorDelegate>> _typedInterceptors = new Dictionary<Type, Dictionary<MethodInfo, InterceptorDelegate>>();
        private static object _syncHelper = new object();

        /// <summary>
        /// Gets the service provider to get dependent services.
        /// </summary>
        public IServiceProvider ServiceProvider { get; }

        /// <summary>
        /// Gets the interceptor chain builder.
        /// </summary>
        public IInterceptorChainBuilder Builder { get; }

        /// <summary>
        /// Create a new <see cref="ProxyFactory"/>.
        /// </summary>
        /// <param name="serviceProvider">The service provider to get dependent services.</param>
        /// <param name="builder">The interceptor chain builder.</param>
        /// <exception cref="ArgumentNullException">The argument <paramref name="serviceProvider"/> is null.</exception>
        /// <exception cref="ArgumentNullException">The argument <paramref name="builder"/> is null.</exception>
        public ProxyFactory(IServiceProvider serviceProvider, IInterceptorChainBuilder builder)
        {
            Guard.ArgumentNotNull(serviceProvider, nameof(serviceProvider));
            Guard.ArgumentNotNull(builder, nameof(builder));

            this.ServiceProvider = serviceProvider;
            this.Builder = builder;
        }

        /// <summary>
        /// Create a proxy wrapping specified target instance. 
        /// </summary>
        /// <param name="typeToProxy">The declaration type of proxy to create.</param>
        /// <param name="target">The target instance wrapped by the created proxy.</param>
        /// <returns>The proxy wrapping the specified target instance.</returns>
        /// <exception cref="ArgumentNullException">The argument <paramref name="typeToProxy"/> is null.</exception>
        /// <remarks>If the <paramref name="target"/> is null, this method will directly return null.</remarks>
        public virtual object CreateProxy(Type typeToProxy, object target)
        {
            Guard.ArgumentNotNull(typeToProxy, nameof(typeToProxy));
            if (target == null)
            {
                return target;
            }
            Dictionary<MethodInfo, InterceptorDelegate> interceptors;
            if (!_typedInterceptors.TryGetValue(target.GetType(), out interceptors))
            {
                lock (_syncHelper)
                {
                    _typedInterceptors[target.GetType()] = interceptors = this.CreateInterceptors(typeToProxy, target);
                }
            }
            if (!interceptors.Any())
            {
                return target;
            }
            return this.CreateProxyCore(typeToProxy, target, interceptors);
        }

        /// <summary>
        /// Create a proxy wrapping specified target instance and interceptors. 
        /// </summary>
        /// <param name="typeToProxy">The declaration type of proxy to create.</param>
        /// <param name="target">The target instance wrapped by the created proxy.</param>
        /// <param name="initerceptors">The interceptors specific to each methods.</param>
        /// <returns>The proxy wrapping the specified target instance.</returns>
        protected abstract object CreateProxyCore(Type typeToProxy, object target, IDictionary<MethodInfo, InterceptorDelegate> initerceptors);

        internal Dictionary<MethodInfo, InterceptorDelegate> CreateInterceptors(Type typeToProxy, object target)
        {
            Dictionary<MethodInfo, InterceptorDelegate> interceptors = new Dictionary<MethodInfo, InterceptorDelegate>();
            var providersOfClass = this.GetInterceptorProvidersForClass(target, out NonInterceptableAttribute nonInterceptable4Class);
            if (null != nonInterceptable4Class && !nonInterceptable4Class.InterceptorProviderTypes.Any())
            {
                return interceptors;
            }
            foreach (var method in typeToProxy.GetTypeInfo().GetMethods())
            {
                if (method.DeclaringType != typeToProxy)
                {
                    continue;
                }
                var providersOfMethod = this.GetInterceptorProvidersForMethod(method, target, out NonInterceptableAttribute nonInterceptable4Method).ToList();
                if (null != nonInterceptable4Method && !nonInterceptable4Method.InterceptorProviderTypes.Any())
                {
                    continue;
                }
                foreach (var provider in providersOfClass)
                {
                    if (!provider.AllowMultiple && providersOfMethod.Any(it => it.GetType() == provider.GetType()))
                    {
                        continue;
                    }
                    if (null != nonInterceptable4Method && nonInterceptable4Method.InterceptorProviderTypes.Contains(provider.GetType()))
                    {
                        continue;
                    }

                    if (null != nonInterceptable4Class && nonInterceptable4Class.InterceptorProviderTypes.Contains(provider.GetType()))
                    {
                        continue;
                    }
                    providersOfMethod.Add(provider);
                }

                if (providersOfMethod.Any())
                {
                    var newBuilder = this.Builder.New();
                    foreach (var provider in providersOfMethod)
                    {
                        provider.Use(newBuilder);
                    }
                    interceptors[method] = newBuilder.Build();
                }
            }
            return interceptors;
        }

        private IEnumerable<IInterceptorProvider> GetInterceptorProvidersForClass(object target, out NonInterceptableAttribute nonInterceptableAttribute)
        {
            var attribute = CustomAttributeAccessor.GetCustomAttribute<NonInterceptableAttribute>(target.GetType());
            nonInterceptableAttribute = attribute;
            var providers = target.GetType().GetTypeInfo().GetCustomAttributes().OfType<IInterceptorProvider>();
            if (null != attribute)
            {
                if (!nonInterceptableAttribute.InterceptorProviderTypes.Any())
                {
                    return new IInterceptorProvider[0];
                }
                providers = providers.Where(it => !attribute.InterceptorProviderTypes.Contains(it.GetType()));
            }
            foreach (var provider in providers)
            {
                (provider as IAttributeCollection)?.AddRange(CustomAttributeAccessor.GetCustomAttributes(target.GetType(), true));
            }
            return providers;
        }

        private IEnumerable<IInterceptorProvider> GetInterceptorProvidersForMethod(MethodInfo proxyMethod, object target, out NonInterceptableAttribute nonInterceptableAttribute)
        {
            MethodInfo targetMethod = this.GetTargetMethod(proxyMethod, target);
            if (null == targetMethod)
            {
                nonInterceptableAttribute = null;
                return new IInterceptorProvider[0];
            }

            var attribute = CustomAttributeAccessor.GetCustomAttribute<NonInterceptableAttribute>(targetMethod);
            nonInterceptableAttribute = attribute;
            var providers = CustomAttributeAccessor.GetCustomAttributes<IInterceptorProvider>(targetMethod);

            if (null != attribute)
            {
                if (!nonInterceptableAttribute.InterceptorProviderTypes.Any())
                {
                    return new IInterceptorProvider[0];
                }
                providers = providers.Where(it => !attribute.InterceptorProviderTypes.Contains(it.GetType()));
            }

            foreach (var provider in providers)
            {
                (provider as IAttributeCollection)?.AddRange(CustomAttributeAccessor.GetCustomAttributes(proxyMethod, true));
                (provider as IAttributeCollection)?.AddRange(CustomAttributeAccessor.GetCustomAttributes(target.GetType(), true));
            }
            return providers;
        }

        private MethodInfo GetTargetMethod(MethodInfo proxyMethod, object target)
        {
            if (proxyMethod.DeclaringType.GetTypeInfo().IsInterface)
            {
                var mapping = target.GetType().GetTypeInfo().GetRuntimeInterfaceMap(proxyMethod.DeclaringType);
                var index = Array.IndexOf(mapping.InterfaceMethods, proxyMethod);
                return mapping.TargetMethods[index];
            }

            return target.GetType().GetTypeInfo().GetMethods().FirstOrDefault(it => it.GetBaseDefinition() == proxyMethod.GetBaseDefinition());
        }
    }
}
