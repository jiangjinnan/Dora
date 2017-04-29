using Castle.DynamicProxy;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Dora.Interception.Castle
{
    /// <summary>
    /// A custom proxy factory leveraging <see cref="ProxyGenerator"/> to create proxy.
    /// </summary>
    public class DynamicProxyFactory : ProxyFactory
    {
        private readonly ProxyGenerator _proxyGenerator;

        /// <summary>
        /// Create a new <see cref="DynamicProxyFactory"/>.
        /// </summary>
        /// <param name="serviceProvider">The service provider to get dependent services.</param>
        /// <param name="builder">The interceptor chain builder.</param>
        public DynamicProxyFactory(IServiceProvider serviceProvider, IInterceptorChainBuilder builder) : base(serviceProvider, builder)
        {
            _proxyGenerator = new ProxyGenerator();
        }

        /// <summary>
        /// Create a proxy wrapping specified target instance. 
        /// </summary>
        /// <param name="typeToProxy">The declaration type of proxy to create.</param>
        /// <param name="target">The target instance wrapped by the created proxy.</param>
        /// <returns>The proxy wrapping the specified target instance.</returns>
        public override object CreateProxy(Type typeToProxy, object target)
        {
            if (target == null || target.GetType().Namespace.StartsWith("Castle."))
            {
                return target;
            }
            return base.CreateProxy(typeToProxy, target);
        }

        /// <summary>
        /// Create a proxy wrapping specified target instance and interceptors. 
        /// </summary>
        /// <param name="typeToProxy">The declaration type of proxy to create.</param>
        /// <param name="target">The target instance wrapped by the created proxy.</param>
        /// <param name="initerceptors">The interceptors specific to each methods.</param>
        /// <returns>The proxy wrapping the specified target instance.</returns>
        /// <exception cref="ArgumentNullException">The argument <paramref name="typeToProxy"/> is null.</exception>
        /// <exception cref="ArgumentNullException">The argument <paramref name="target"/> is null.</exception>
        /// <exception cref="ArgumentNullException">The argument <paramref name="initerceptors"/> is null.</exception>
        protected override object CreateProxyCore(Type typeToProxy, object target, IDictionary<MethodInfo, InterceptorDelegate> initerceptors)
        {
            Guard.ArgumentNotNull(typeToProxy, nameof(typeToProxy));
            Guard.ArgumentNotNull(target, nameof(target));
            Guard.ArgumentNotNull(initerceptors, nameof(initerceptors));

            if (!initerceptors.Any())
            {
                return target;
            }
            IDictionary<MethodInfo, IInterceptor> dic = initerceptors.ToDictionary(it => it.Key, it => (IInterceptor)new DynamicProxyInterceptor(it.Value));
            var selector = new DynamicProxyInterceptorSelector(dic);
            var options = new ProxyGenerationOptions { Selector = selector };
            if (typeToProxy.GetTypeInfo().IsInterface)
            {
                return _proxyGenerator.CreateInterfaceProxyWithTarget(typeToProxy, target, options, dic.Values.ToArray());
            }
            else
            {
                return _proxyGenerator.CreateClassProxyWithTarget(typeToProxy, target, options, dic.Values.ToArray());
            }
        }
    }
}
