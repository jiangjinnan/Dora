using System;
using System.Collections.Generic;
using System.Text;

namespace Dora.ExceptionHandling
{
    public class ExceptionManagerBuilder : IExceptionManagerBuilder
    {
        private Dictionary<string, IExceptionPolicy> _policies;
        public IServiceProvider ServiceProvider { get; }

        public ExceptionManagerBuilder(IServiceProvider serviceProvider)
        {
            this.ServiceProvider = Guard.ArgumentNotNull(serviceProvider, nameof(serviceProvider));
            _policies = new Dictionary<string, IExceptionPolicy>(StringComparer.OrdinalIgnoreCase);
        }

        public void AddPolicy(string policyName, Action<IExceptionPolicyBuilder> configure)
        {
            Guard.ArgumentNotNullOrEmpty(policyName, nameof(policyName));
            Guard.ArgumentNotNull(configure, nameof(configure));

            ExceptionPolicyBuilder builder = new ExceptionPolicyBuilder(this.ServiceProvider);
            configure(builder);
            _policies.Add(policyName, builder.Build());
        }

        public ExceptionManager Build()
        {
            return new ExceptionManager(_policies);
        }
    }
}
