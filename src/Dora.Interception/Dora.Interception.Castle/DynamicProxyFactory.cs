using Castle.DynamicProxy;
using Dora.DynamicProxy;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace Dora.Interception.Castle
{
    /// <summary>
    /// A custom proxy factory leveraging <see cref="ProxyGenerator"/> to create proxy.
    /// </summary>
    public class DynamicProxyFactory : InterceptingProxyFactoryBase
    {
        private readonly ProxyGenerator _proxyGenerator;

        /// <summary>
        /// Create a new <see cref="DynamicProxyFactory"/>.
        /// </summary>   
        public DynamicProxyFactory(IInterceptorResolver interceptorResolver, IServiceProvider serviceProvider)
            :base(interceptorResolver, serviceProvider)
        {
            _proxyGenerator = new ProxyGenerator();
        }             

        /// <summary>
        /// Create an interceptable proxy instance.
        /// </summary>
        /// <param name="typeToIntercept">The type to intercept.</param>
        /// <param name="serviceProvider">The <see cref="T:System.IServiceProvider" /> used to provide dependent service instances.</param>
        /// <param name="interceptors">The <see cref="T:Dora.DynamicProxy.InterceptorDecoration" /> representing which interceptors are applied to which members of a type to intercept.</param>
        /// <returns>
        /// The interceptable proxy instance.
        /// </returns>
        protected override object Create(Type typeToIntercept, IServiceProvider serviceProvider, InterceptorRegistry interceptors)
        {
            var dictionary = interceptors.Interceptors
                 .ToDictionary(it => it.Key, it => new DynamicProxyInterceptor(it.Value).ToInterceptor());
            var selector = new DynamicProxyInterceptorSelector(dictionary.ToDictionary(it => it.Key, it => it.Value));
            var options = new ProxyGenerationOptions { Selector = selector };
            return _proxyGenerator.CreateClassProxy(typeToIntercept, options, dictionary.Values.ToArray());
        }

        /// <summary>
        /// Create a proxy wrapping specified target instance.
        /// </summary>
        /// <param name="typeToIntercept">The type to intercept.</param>
        /// <param name="target">The target.</param>
        /// <param name="interceptors">The interceptors.</param>
        /// <returns></returns>
        protected override object Wrap(Type typeToIntercept, object target, InterceptorRegistry interceptors)
        {
            var dictionary = interceptors.Interceptors
                .ToDictionary(it => it.Key, it => new DynamicProxyInterceptor(it.Value).ToInterceptor());
            var selector = new DynamicProxyInterceptorSelector(dictionary.ToDictionary(it => it.Key, it => it.Value));
            var options = new ProxyGenerationOptions { Selector = selector };

            return _proxyGenerator.CreateInterfaceProxyWithTarget(typeToIntercept, target, dictionary.Values.ToArray());
        }

        internal class FoobarAsyncInterceptor : AsyncInterceptorBase
        {
            protected override Task InterceptAsync(IInvocation invocation, Func<IInvocation, Task> proceed)
            {
                return proceed(invocation);
            }

            protected override async Task<TResult> InterceptAsync<TResult>(IInvocation invocation, Func<IInvocation, Task<TResult>> proceed)
            {
                await proceed(invocation);
                return await (Task<TResult>)invocation.ReturnValue;
            }
        }
    }
}
