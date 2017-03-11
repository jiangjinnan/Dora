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
                return null;
            }

            Dictionary<MethodInfo, InterceptorDelegate> interceptors;
            if (!_typedInterceptors.TryGetValue(target.GetType(), out interceptors))
            {
                lock (_syncHelper)
                {
                    _typedInterceptors[target.GetType()] =  interceptors = this.CreateInterceptors(typeToProxy, target);
                }
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

        private  Dictionary<MethodInfo, InterceptorDelegate> CreateInterceptors(Type typeToProxy, object target)
        {
            Dictionary<MethodInfo, InterceptorDelegate> interceptors = new Dictionary<MethodInfo, InterceptorDelegate>();
            var providersOfClass = this.GetInterceptorProvidersForClass(target);
            foreach (var method in typeToProxy.GetTypeInfo().GetMethods())
            {
                if (method.DeclaringType != typeToProxy)
                {
                    continue;
                }
                var providersOfMethod = this.GetInterceptorProvidersForMethod(method, target).ToList();
                foreach (var provider in providersOfClass)
                {
                    if (!provider.AllowMultiple && providersOfMethod.Any(it => it.GetType() == provider.GetType()))
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

        private IEnumerable<IInterceptorProvider> GetInterceptorProvidersForClass(object target)
        {            
            return target.GetType().GetTypeInfo().GetCustomAttributes().OfType<IInterceptorProvider>();
        }

        private IEnumerable<IInterceptorProvider> GetInterceptorProvidersForMethod(MethodInfo proxyMethod, object target)
        {
            MethodInfo targetMethod = target.GetType().GetTypeInfo().GetMethods().FirstOrDefault(it => Match(proxyMethod, it));
            if (null == targetMethod ||
                targetMethod.GetCustomAttribute<NonInterceptableAttribute>() != null || 
                target.GetType().GetTypeInfo().GetCustomAttribute<NonInterceptableAttribute>() != null)
            {
                return new IInterceptorProvider[0];
            }
            return targetMethod.GetCustomAttributes().OfType<IInterceptorProvider>();
        }

        private static bool Match(MethodInfo method1, MethodInfo method2)
        {
            if (method1.Name != method2.Name)
            {
                return false;
            }

            var parameters1 = method1.GetParameters();
            var parameters2 = method2.GetParameters();
            if (parameters1.Length != parameters2.Length)
            {
                return false;
            }

            for (int i = 0; i < parameters1.Length; i++)
            {
                if (parameters1[i].ParameterType != parameters2[i].ParameterType)
                {
                    return false;
                }
            }

            return true;
        }
    }
}
