using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace Dora.Interception
{
    public abstract class ProxyFactory : IProxyFactory
    {
        public IServiceProvider ServiceProvider { get; }
        public IInterceptorChainBuilder Builder { get; }

        public ProxyFactory(IServiceProvider serviceProvider, IInterceptorChainBuilder builder)
        {
            this.ServiceProvider = serviceProvider;
            this.Builder = builder;
        }

        public virtual object CreateProxy(Type typeToProxy, object target)
        {
            if (target == null)
            {
                return null;
            }
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
            return this.CreateProxyCore(typeToProxy, target, interceptors);
        }

        protected abstract object CreateProxyCore(Type typeToProxy, object target, IDictionary<MethodInfo, InterceptorDelegate> initerceptors);

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
