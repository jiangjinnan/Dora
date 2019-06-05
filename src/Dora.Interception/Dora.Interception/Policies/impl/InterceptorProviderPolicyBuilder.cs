using System;

namespace Dora.Interception.Policies
{
    internal class InterceptorProviderPolicyBuilder<TInterceptorProvider> : IInterceptorProviderPolicyBuilder
    {
        private readonly InterceptorProviderPolicy _interceptorProviderPolicy;

        public InterceptorProviderPolicyBuilder(Func<IInterceptorProvider> interceptorProviderFactory)
        {
            Guard.ArgumentNotNull(interceptorProviderFactory, nameof(interceptorProviderFactory));
            _interceptorProviderPolicy = new InterceptorProviderPolicy(typeof(TInterceptorProvider), interceptorProviderFactory);
        }

        public IInterceptorProviderPolicyBuilder To<TTarget>(Action<ITargetPolicyBuilder<TTarget>> configure)
        {
            Guard.ArgumentNotNull(configure, nameof(configure));
            var builder = new TargetPolicyBuilder<TTarget>();
            configure.Invoke(builder);
            _interceptorProviderPolicy.TargetPolicies.Add(builder.Build());
            return this;
        }

        public InterceptorProviderPolicy Build() => _interceptorProviderPolicy;
    }
}
