using Castle.DynamicProxy;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace Dora.Interception.Castle
{
    public class DynamicProxyFactory : ProxyFactory
    {
        private ProxyGenerator _proxyGenerator;
        public DynamicProxyFactory(IServiceProvider serviceProvider, IInterceptorChainBuilder builder) : base(serviceProvider, builder)
        {
            _proxyGenerator = new ProxyGenerator();
        }

        public override object CreateProxy(Type typeToProxy, object target)
        {
            if (target.GetType().Namespace.StartsWith("Castle."))
            {
                return target;
            }
            return base.CreateProxy(typeToProxy, target);
        }

        protected override object CreateProxyCore(Type typeToProxy, object target, IDictionary<MethodInfo, InterceptorDelegate> initerceptors)
        {
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
