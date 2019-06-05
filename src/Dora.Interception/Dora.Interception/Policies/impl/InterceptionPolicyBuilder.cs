using Microsoft.Extensions.DependencyInjection;
using System;

namespace Dora.Interception.Policies
{
    internal class InterceptionPolicyBuilder : IInterceptionPolicyBuilder
    {
        private readonly InterceptionPolicy _policy;   
        public InterceptionPolicyBuilder(IServiceProvider serviceProvider)
        {
            ServiceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
            _policy = new InterceptionPolicy();
        }

        public IServiceProvider ServiceProvider { get; }        
        public InterceptionPolicy Build() => _policy;            
        public IInterceptionPolicyBuilder For<TInterceptorProvider>(
            int order, 
            Action<IInterceptorProviderPolicyBuilder> configureTargets,  
            params object[] arguments) where TInterceptorProvider : IInterceptorProvider
        {
            IInterceptorProvider GetInteceptorProvider()
            {
                var provider = ActivatorUtilities.CreateInstance<TInterceptorProvider>(ServiceProvider, arguments);
                if (provider is IOrderedSequenceItem orderedElement)
                {
                    orderedElement.Order = order;
                }
                return provider;
            }
            var targetBuilder = new InterceptorProviderPolicyBuilder<TInterceptorProvider>(GetInteceptorProvider);
            configureTargets?.Invoke(targetBuilder);
            _policy.Add(targetBuilder.Build());
            return this;
        }
    }
}