using System;
using System.Collections.Generic;

namespace Dora.Interception
{
    public interface IInterceptionPolicyBuilder
    {
        IServiceProvider ServiceProvider { get; }
        IEnumerable<InterceptorProviderRegistration> Build();
        IInterceptionPolicyBuilder For<TInterceptorProvider>(int order, Action<IInterceptorProviderRegistrationBuilder> configureTargets, params object[] arguments)
            where TInterceptorProvider: IInterceptorProvider;
    }
}
