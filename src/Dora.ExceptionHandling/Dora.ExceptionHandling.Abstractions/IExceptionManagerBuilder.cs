using System;
using System.Collections.Generic;

namespace Dora.ExceptionHandling
{
    /// <summary>
    /// A builder to build an <see cref="ExceptionManager"/>.
    /// </summary>
    public interface IExceptionManagerBuilder
    {
        /// <summary>
        /// A <see cref="IServiceProvider"/> to provide neccessary dependent services.
        /// </summary>
        IServiceProvider ServiceProvider { get; }

        /// <summary>
        /// A property dictionary.
        /// </summary>
        IDictionary<string, object> Properties { get; }

        /// <summary>
        /// Register a new exception policy.
        /// </summary>
        /// <param name="policyName">The name of exception policy to register.</param>
        /// <param name="configure">A <see cref="Action{IExceptionPolicyBuilder}"/> to build the registered exception policy.</param>
        IExceptionManagerBuilder AddPolicy(string policyName, Action<IExceptionPolicyBuilder> configure);

        /// <summary>
        /// Set the default exception policy.
        /// </summary>
        /// <param name="policyName">The name of defualt exception policy.</param>
        /// <returns>The current <see cref="ExceptionManager"/>.</returns>
        IExceptionManagerBuilder SetDefaultPolicy(string policyName);

        /// <summary>
        /// Build the <see cref="ExceptionManager"/>.
        /// </summary>
        /// <returns>The <see cref="ExceptionManager"/> to build.</returns>
        ExceptionManager Build();
    }
}
