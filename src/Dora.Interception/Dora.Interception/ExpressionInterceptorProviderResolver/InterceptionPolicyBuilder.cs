using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Text;

namespace Dora.Interception
{
    internal class InterceptionPolicyBuilder : IInterceptionPolicyBuilder
    {
        private readonly List<InterceptorProviderRegistration> _registrations;   
        public InterceptionPolicyBuilder(IServiceProvider serviceProvider)
        {
            _registrations = new List<InterceptorProviderRegistration>();
            ServiceProvider = serviceProvider?? throw new ArgumentNullException(nameof(serviceProvider));
        }

        public IServiceProvider ServiceProvider { get; }        
        public IEnumerable<InterceptorProviderRegistration> Build() => _registrations;            
        public IInterceptionPolicyBuilder For<TInterceptorProvider>(int order, Action<IInterceptorProviderRegistrationBuilder> configureTargets,  params object[] arguments) where TInterceptorProvider : IInterceptorProvider
        {
            var targetBuilder = new InterceptorProviderRegistrationBuilder();
            configureTargets?.Invoke(targetBuilder);
            Func<IInterceptorProvider> providerAccessor = () =>
            {
                var provider = ActivatorUtilities.CreateInstance<TInterceptorProvider>(ServiceProvider, arguments);
                var orderedElement = provider as IOrderedSequenceItem;
                if (null != orderedElement)
                {
                    orderedElement.Order = order;
                }
                return provider;
            };
            _registrations.Add(new InterceptorProviderRegistration(typeof(TInterceptorProvider), providerAccessor, targetBuilder.Build()));
            return this;
        }
    }
}