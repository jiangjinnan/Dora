using Dora.ExceptionHandling.Abstractions.Properties;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Dora.ExceptionHandling
{
    /// <summary>
    /// The default implementation of exception policy builder.
    /// </summary>
    public class ExceptionPolicyBuilder : IExceptionPolicyBuilder
    {
        private List<ExceptionPolicyEntry> _policyEntries;
        private ExceptionHandlerBuilder _preHanlderBuilder;
        private ExceptionHandlerBuilder _postHanlderBuilder;

        /// <summary>
        /// A <see cref="IServiceProvider"/> to provide neccessary dependent services.
        /// </summary>
        public IServiceProvider ServiceProvider { get; }

        /// <summary>
        /// Create a new <see cref="ExceptionPolicyBuilder"/>.
        /// </summary>
        /// <param name="serviceProvider">A <see cref="IServiceProvider"/> to provide neccessary dependent services.</param>
        /// <exception cref="ArgumentNullException">The <paramref name="serviceProvider"/> is null.</exception>
        public ExceptionPolicyBuilder(IServiceProvider serviceProvider)
        {
            this.ServiceProvider = Guard.ArgumentNotNull(serviceProvider, nameof(serviceProvider));

            _policyEntries = new List<ExceptionPolicyEntry>();
            _preHanlderBuilder = new ExceptionHandlerBuilder(serviceProvider);
            _postHanlderBuilder = new ExceptionHandlerBuilder(serviceProvider);
        }

        /// <summary>
        /// Register exception handler chain for specified exception type.
        /// </summary>
        /// <param name="exceptionType">The type of exception handled by the registered exception handler chain.</param>
        /// <param name="postHandlingAction">Determining what action should occur after an exception is handled by the configured exception handling chain.</param>
        /// <param name="configure">An <see cref="Action{IExceptionHandlerBuilder}"/> to build the exception handler chain.</param>
        /// <returns>The current <see cref="IExceptionPolicyBuilder"/>.</returns>
        /// <exception cref="ArgumentNullException">The <paramref name="exceptionType"/> is null.</exception>
        /// <exception cref="ArgumentNullException">The <paramref name="postHandlingAction"/> is null.</exception>
        /// <exception cref="ArgumentNullException">The <paramref name="configure"/> is null.</exception>
        public IExceptionPolicyBuilder For(Type exceptionType, PostHandlingAction postHandlingAction, Action<IExceptionHandlerBuilder> configure)
        {
            Guard.ArgumentNotNull(exceptionType, nameof(exceptionType));
            Guard.ArgumentNotNull(configure, nameof(configure));

            if (_policyEntries.Any(it => it.ExceptionType == exceptionType))
            {
                throw new ArgumentException(Resources.ExceptionDuplicateExceptionType.Fill(exceptionType.FullName), nameof(exceptionType));
            }
            ExceptionHandlerBuilder builder = new ExceptionHandlerBuilder(this.ServiceProvider);
            configure(builder);
            _policyEntries.Add(new ExceptionPolicyEntry(exceptionType, postHandlingAction, builder.Build()));
            return this;
        }

        /// <summary>
        /// Register common exception handler chain which is invoked after the ones registered to exception type.
        /// </summary>
        /// <param name="configure">An <see cref="Action{IExceptionHandlerBuilder}"/> to build the exception handler chain.</param>
        /// <returns>The current <see cref="IExceptionPolicyBuilder"/>.</returns>
        /// <exception cref="ArgumentNullException">The <paramref name="configure"/> is null.</exception>
        public IExceptionPolicyBuilder Post(Action<IExceptionHandlerBuilder> configure)
        {
            Guard.ArgumentNotNull(configure, nameof(configure));

            ExceptionHandlerBuilder builder = new ExceptionHandlerBuilder(this.ServiceProvider);
            configure(builder);
            _postHanlderBuilder.Use(async context =>
            {
                await builder.Build()(context);
            });
            return this;
        }

        /// <summary>
        /// Register common exception handler chain which is invoked before the ones registered to exception type.
        /// </summary>
        /// <param name="configure">An <see cref="Action{IExceptionHandlerBuilder}"/> to build the exception handler chain.</param>
        /// <returns>The current <see cref="IExceptionPolicyBuilder"/>.</returns>
        /// <exception cref="ArgumentNullException">The <paramref name="configure"/> is null.</exception>
        public IExceptionPolicyBuilder Pre(Action<IExceptionHandlerBuilder> configure)
        {
            Guard.ArgumentNotNull(configure, nameof(configure));
            ExceptionHandlerBuilder builder = new ExceptionHandlerBuilder(this.ServiceProvider);
            configure(builder);
            _preHanlderBuilder.Use(async context =>
            {
                await builder.Build()(context);
            });
            return this;
        }

        /// <summary>
        /// Build the exception policy.
        /// </summary>
        /// <returns>The exception policy to build.</returns>
        public IExceptionPolicy Build()
        {
            return new ExceptionPolicy(_policyEntries, _preHanlderBuilder.Build(), _postHanlderBuilder.Build());
        }
    }
}
