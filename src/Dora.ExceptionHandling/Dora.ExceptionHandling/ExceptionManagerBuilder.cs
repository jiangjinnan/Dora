using System;
using System.Collections.Generic;
using System.Text;

namespace Dora.ExceptionHandling
{
    /// <summary>
    /// The default implementation of exception manger builder.
    /// </summary>
    public class ExceptionManagerBuilder : IExceptionManagerBuilder
    {
        private Dictionary<string, IExceptionPolicy> _policies;

        /// <summary>
        /// A <see cref="IServiceProvider"/> to provide neccessary dependent services.
        /// </summary>
        public IServiceProvider ServiceProvider { get; }

        /// <summary>
        /// Create a new <see cref="ExceptionManagerBuilder"/>.
        /// </summary>
        /// <param name="serviceProvider">A <see cref="IServiceProvider"/> to provide neccessary dependent services.</param>
        /// <exception cref="ArgumentNullException">The <paramref name="serviceProvider"/> is null.</exception>
        public ExceptionManagerBuilder(IServiceProvider serviceProvider)
        {
            this.ServiceProvider = Guard.ArgumentNotNull(serviceProvider, nameof(serviceProvider));
            _policies = new Dictionary<string, IExceptionPolicy>(StringComparer.OrdinalIgnoreCase);
        }

        /// <summary>
        /// Register a new exception policy.
        /// </summary>
        /// <param name="policyName">The name of exception policy to register.</param>
        /// <param name="configure">A <see cref="Action{IExceptionPolicyBuilder}"/> to build the registered exception policy.</param>
        /// <exception cref="ArgumentNullException">The <paramref name="policyName"/> is null.</exception>
        /// <exception cref="ArgumentException">The <paramref name="policyName"/> is a white space string.</exception>
        /// <exception cref="ArgumentNullException">The <paramref name="configure"/> is null.</exception>
        public void AddPolicy(string policyName, Action<IExceptionPolicyBuilder> configure)
        {
            Guard.ArgumentNotNullOrWhiteSpace(policyName, nameof(policyName));
            Guard.ArgumentNotNull(configure, nameof(configure));

            ExceptionPolicyBuilder builder = new ExceptionPolicyBuilder(this.ServiceProvider);
            configure(builder);
            _policies.Add(policyName, builder.Build());
        }

        /// <summary>
        /// Build the <see cref="ExceptionManager"/>.
        /// </summary>
        /// <returns>The <see cref="ExceptionManager"/> to build.</returns>
        public ExceptionManager Build()
        {
            return new ExceptionManager(_policies);
        }
    }
}
