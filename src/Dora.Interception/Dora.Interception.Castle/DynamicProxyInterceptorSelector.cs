using Castle.DynamicProxy;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Reflection;
using Dora.DynamicProxy;

namespace Dora.Interception.Castle
{
    internal class DynamicProxyInterceptorSelector : IInterceptorSelector
    {
        private readonly IDictionary<int, IInterceptor> _interceptors;

        public DynamicProxyInterceptorSelector(IDictionary<int, IInterceptor> interceptors)
        {
            _interceptors = Guard.ArgumentNotNull(interceptors, nameof(interceptors))
                .ToDictionary(it => it.Key, it => it.Value);
        }  

        public IInterceptor[] SelectInterceptors(Type type, MethodInfo method, IInterceptor[] interceptors)
        {
            return interceptors;
            //if (_interceptors.TryGetValue(method.MetadataToken, out var interceptor) && interceptors.Contains(interceptor))
            //{
            //    return new IInterceptor[] { interceptor };
            //}
            //return new IInterceptor[0];
        }
    }
}
