using Dora.ExceptionHandling.Abstractions.Properties;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace Dora.ExceptionHandling
{
    /// <summary>
    /// The default implementation of exception policy.
    /// </summary>
    public class ExceptionPolicy : IExceptionPolicy
    {
        /// <summary>
        /// Exception policy entries each of which is specific to an exception type.
        /// </summary>
        public IEnumerable<ExceptionPolicyEntry> Entries { get; }

        /// <summary>
        /// The common exception handler chain invoked before all type specific handler chain.
        /// </summary>
        public Func<ExceptionContext, Task> PreHandler { get; }

        /// <summary>
        /// The common exception handler chain invoked after all type specific handler chain.
        /// </summary>
        public Func<ExceptionContext, Task> PostHandler { get; }

        /// <summary>
        /// Create a new <see cref="ExceptionPolicy"/>.
        /// </summary>
        /// <param name="policyEntries">Exception policy entries each of which is specific to an exception type.</param>
        /// <param name="preHandler">The common exception handler chain invoked before all type specific handler chain.</param>
        /// <param name="postHandler">The common exception handler chain invoked after all type specific handler chain.</param>
        /// <exception cref="ArgumentNullException">The <paramref name="policyEntries"/> is null.</exception>
        /// <exception cref="ArgumentException">The <paramref name="policyEntries"/> is empty.</exception>
        /// <exception cref="InvalidOperationException">The <paramref name="policyEntries"/> has multiple entries for the same exception type..</exception>
        /// <exception cref="ArgumentNullException">The <paramref name="preHandler"/> is null.</exception>
        /// <exception cref="ArgumentNullException">The <paramref name="postHandler"/> is null.</exception>
        public ExceptionPolicy(IEnumerable<ExceptionPolicyEntry> policyEntries, Func<ExceptionContext, Task> preHandler, Func<ExceptionContext, Task> postHandler)
        {
            var list = new List<ExceptionPolicyEntry>(Guard.ArgumentNotNull(policyEntries, nameof(policyEntries)));
            var group = list.GroupBy(it => it.ExceptionType).FirstOrDefault(it => it.Count() > 1);
            if(null != group)
            {
                throw new ArgumentException(Resources.ExceptionDuplicateExceptionType.Fill(group.First().ExceptionType.FullName), nameof(policyEntries));
            }
            if (!list.Any(it => it.ExceptionType == typeof(Exception)))
            {
                list.Add(new ExceptionPolicyEntry(typeof(Exception), PostHandlingAction.ThrowNew, _ => Task.CompletedTask));
            }
            this.Entries = list;
            this.PreHandler = Guard.ArgumentNotNull(preHandler, nameof(preHandler));
            this.PostHandler = Guard.ArgumentNotNull(postHandler, nameof(postHandler));
        }        

        /// <summary>
        /// Create an exception handler based on specified exception to handle.
        /// </summary>
        /// <param name="exception">The exception to handle.</param>
        /// <param name="postHandlingAction">A <see cref="PostHandlingAction"/> determining what action should occur after an exception is handled by the configured exception handling chain. </param>
        /// <returns>A <see cref="Func{TExceptionContext, Task}"/> representing the exception handler.</returns>
        public Func<ExceptionContext, Task> CreateHandler(Exception exception, out PostHandlingAction postHandlingAction)
        {
            Guard.ArgumentNotNull(exception, nameof(exception));
            ExceptionPolicyEntry policyEntry = this.GetPolicyEntry(exception.GetType());
            postHandlingAction = policyEntry.PostHandlingAction;
            return async context =>
            {
                await this.PreHandler(context);
                await policyEntry.Handler(context);
                await this.PostHandler(context);
            };
        }

        /// <summary>
        /// Get <see cref="ExceptionPolicyEntry"/> specific to given exception type.
        /// </summary>
        /// <param name="exceptionType">The type of exception to which the retrieved <see cref="ExceptionPolicyEntry"/> is specific.</param>
        /// <returns>The <see cref="ExceptionPolicyEntry"/> specific to given exception type.</returns>
        /// <exception cref="ArgumentNullException">The <paramref name="exceptionType"/> is null.</exception>
        internal protected ExceptionPolicyEntry GetPolicyEntry(Type exceptionType)
        {
            Guard.ArgumentAssignableTo<Exception>(exceptionType, nameof(exceptionType));
            var entry = this.Entries.FirstOrDefault(it => it.ExceptionType == exceptionType);
            return entry ?? GetPolicyEntry(exceptionType.GetTypeInfo().BaseType);
        }
    }
}
