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
    public class DynamicProxyFactory : IInterceptingProxyFactory
    {
        private readonly ProxyGenerator _proxyGenerator;
        private readonly IInterceptorResolver   _interceptorResolver;

        /// <summary>
        /// Create a new <see cref="DynamicProxyFactory"/>.
        /// </summary>   
        public DynamicProxyFactory(IInterceptorResolver  interceptorResolver, IServiceProvider serviceProvider)
        {
            _interceptorResolver = Guard.ArgumentNotNull(interceptorResolver, nameof(interceptorResolver));
            this.ServiceProvider = Guard.ArgumentNotNull(serviceProvider, nameof(serviceProvider));
            _proxyGenerator = new ProxyGenerator();
        }

        /// <summary>
        /// Gets the service provider.
        /// </summary>
        /// <value>
        /// The service provider.
        /// </value>
        public IServiceProvider ServiceProvider { get; }

        /// <summary>
        /// Creates the specified type to intercept.
        /// </summary>
        /// <param name="typeToIntercept">The type to intercept.</param>
        /// <param name="serviceProvider">The service provider.</param>
        /// <param name="targetAccessor">The target accessor.</param>   
        /// <returns>The proxy wrapping the specified target instance.</returns>
        public object Create(Type typeToIntercept, IServiceProvider serviceProvider, Func<object> targetAccessor = null)
        {
            var interceptorDecoration = _interceptorResolver.GetInterceptors(typeToIntercept);
            if (interceptorDecoration.IsEmpty)
            {
                return targetAccessor == null
                    ? serviceProvider.GetService(typeToIntercept)
                    : targetAccessor();
            }

            var interceptors = interceptorDecoration.Interceptors
                .ToDictionary(it => it.Key, it => new DynamicProxyInterceptor(it.Value).ToInterceptor());
            var selector = new DynamicProxyInterceptorSelector(interceptors.ToDictionary(it => it.Key, it => it.Value));
            var options = new ProxyGenerationOptions { Selector = selector};
            return _proxyGenerator.CreateClassProxy(typeToIntercept, options, interceptors.Values.ToArray());
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
            Guard.ArgumentNotNull(target, nameof(target));
            var interceptorDecoration = _interceptorResolver.GetInterceptors(typeToIntercept, target.GetType());
            var interceptors = interceptorDecoration.Interceptors
               .ToDictionary(it => it.Key, it => new DynamicProxyInterceptor(it.Value).ToInterceptor());
            var selector = new DynamicProxyInterceptorSelector(interceptors.ToDictionary(it => it.Key, it => it.Value));
            var options = new ProxyGenerationOptions { Selector = selector };

            return _proxyGenerator.CreateInterfaceProxyWithTarget(typeToIntercept,target, interceptors.Values.ToArray());
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
