using Dora.ExceptionHandling.Abstractions.Properties;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Dora.ExceptionHandling
{
    /// <summary>
    /// The consumer uses this class to handle exception.
    /// </summary>
    public class ExceptionManager
    {
        private Dictionary<string, IExceptionPolicy> _policies;

        /// <summary>
        /// The default exception policy.
        /// </summary>
        public IExceptionPolicy DefaultPolicy { get; private set; }

        /// <summary>
        /// Create a new <see cref="ExceptionManager"/>.
        /// </summary>
        /// <param name="policies">A named exception policy list.</param>
        /// <exception cref="ArgumentNullException">The specified <paramref name="policies"/> is null.</exception>
        /// <exception cref="ArgumentException">The specified <paramref name="policies"/> is empty.</exception>
        /// <remarks>The name of exception policy name is not case sensitive.</remarks>
        public ExceptionManager(IDictionary<string, IExceptionPolicy> policies)
        {
            Guard.ArgumentNotNull(policies, nameof(policies));
            Guard.ArgumentNotNullOrEmpty(policies, nameof(policies));
            _policies = new Dictionary<string, IExceptionPolicy>(policies, StringComparer.OrdinalIgnoreCase);
        }

        /// <summary>
        /// Handle the specified exception based on given policy.
        /// </summary>
        /// <param name="exception">The <see cref="Exception"/> to handle.</param>
        /// <param name="policyName">The name of the exception policy used to handle the specified exception.</param>
        /// <param name="contextInitializer">An <see cref="Action{ExceptionContext}"/> is configure the <see cref="ExceptionContext"/> before it is dispatched to exception handler chain.</param>
        /// <returns>The task to handle the specified exception.</returns>
        /// <exception cref="ArgumentNullException">The specified <paramref name="exception"/> is null.</exception>
        /// <exception cref="ArgumentNullException">The specified <paramref name="policyName"/> is null.</exception>
        /// <exception cref="ArgumentException">The specified <paramref name="policyName"/> is an white space string or does not exist.</exception>
        public async Task HandleExceptionAsync(Exception exception, string policyName, Action<ExceptionContext> contextInitializer = null)
        {
            Guard.ArgumentNotNull(exception, nameof(exception));
            Guard.ArgumentNotNullOrWhiteSpace(policyName, nameof(policyName));

            if (_policies.TryGetValue(policyName, out IExceptionPolicy policy))
            {
                await this.HandleExceptionAsync(exception, policy, contextInitializer);
            }
            else
            {
                throw new ArgumentException(Resources.ExceptionPolicyNotFound.Fill(policyName), nameof(policyName));
            }
        }
        /// <summary>
        /// Handle the specified exception based on the default policy.
        /// </summary>
        /// <param name="exception">The <see cref="Exception"/> to handle.</param>
        /// <param name="contextInitializer">An <see cref="Action{ExceptionContext}"/> is configure the <see cref="ExceptionContext"/> before it is dispatched to exception handler chain.</param>
        /// <returns>The task to handle the specified exception.</returns>
        /// <exception cref="ArgumentNullException">The specified <paramref name="exception"/> is null.</exception>
        /// <exception cref="InvalidOperationException">The default exception policy is not set.</exception>
        public async Task HandleExceptionAsync(Exception exception, Action<ExceptionContext> contextInitializer = null)
        {
            Guard.ArgumentNotNull(exception, nameof(exception));
            if (null == this.DefaultPolicy)
            {
                throw new InvalidOperationException(Resources.ExceptionDefaultPolicyNotExists);
            }
            await this.HandleExceptionAsync(exception, this.DefaultPolicy, contextInitializer);
        }

        /// <summary>
        /// Set the default exception policy.
        /// </summary>
        /// <param name="policyName">The name of default policy to set.</param>
        /// <returns>The current <see cref="ExceptionManager"/>.</returns>
        public ExceptionManager SetDefaultPolicy(string policyName)
        {
            Guard.ArgumentNotNullOrWhiteSpace(policyName, nameof(policyName));
            if (_policies.TryGetValue(policyName, out IExceptionPolicy defaultPolicy))
            {
                this.DefaultPolicy = defaultPolicy;
                return this;
            }
            else
            {
                throw new ArgumentException(Resources.ExceptionPolicyNotFound.Fill(policyName), nameof(policyName));
            }
        }

        /// <summary>
        /// Handle the exception using specified exception policy.
        /// </summary>
        /// <param name="exception">The <see cref="Exception"/> to handle.</param>
        /// <param name="policy">The exception policy to handle the specified exception.</param>
        /// <param name="contextInitializer">A <see cref="Action{ExceptionContext}"/> to initialize <see cref="ExceptionContext"/> before performing exception handling.</param>
        /// <returns>The task to handle the specified exception.</returns>
        /// <exception cref="ArgumentNullException">The specified <paramref name="exception"/> is null.</exception>
        /// <exception cref="ArgumentNullException">The specified <paramref name="policy"/> is null.</exception>
        protected virtual async Task HandleExceptionAsync(Exception exception, IExceptionPolicy policy, Action<ExceptionContext> contextInitializer = null)
        {
            Guard.ArgumentNotNull(exception, nameof(exception));
            Guard.ArgumentNotNull(policy, nameof(policy));

            Func<ExceptionContext, Task> handler = policy.CreateHandler(exception, out PostHandlingAction postHandlingAction);
            ExceptionContext context = new ExceptionContext(exception);
            contextInitializer?.Invoke(context);
            await handler(context);
            if (postHandlingAction == PostHandlingAction.ThrowNew)
            {
                throw context.Exception ?? context.OriginalException;
            }
            if (postHandlingAction == PostHandlingAction.ThrowOriginal)
            {
                throw context.OriginalException;
            }
        }
    }
}
