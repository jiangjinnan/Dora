using System;

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
        /// Register a new exception policy.
        /// </summary>
        /// <param name="policyName">The name of exception policy to register.</param>
        /// <param name="configure">A <see cref="Action{IExceptionPolicyBuilder}"/> to build the registered exception policy.</param>
        void AddPolicy(string policyName, Action<IExceptionPolicyBuilder> configure);

        /// <summary>
        /// Build the <see cref="ExceptionManager"/>.
        /// </summary>
        /// <returns>The <see cref="ExceptionManager"/> to build.</returns>
        ExceptionManager Build();
    }
}
