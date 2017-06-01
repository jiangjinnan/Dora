using System;
using System.Collections.Generic;

namespace Dora.ExceptionHandling.Configuration
{
    /// <summary>
    /// The base class of all concrete exception handler specific configuration classes.
    /// </summary>
    public abstract class ExceptionHandlerConfiguration
    {
        /// <summary>
        /// Register the specific exception handler based on configuration.
        /// </summary>
        /// <param name="predicate">A predicate to indicate whether to invoke the registered handler.</param>
        /// <param name="builder">The <see cref="IExceptionHandlerBuilder"/> used to register the specific exception handler.</param>
        /// <param name="configuration">A <see cref="IDictionary{String, String}"/> storing configuration for the exception handler to register.</param>
        public abstract void Use(IExceptionHandlerBuilder builder, Func<ExceptionContext,bool> predicate, IDictionary<string, string> configuration);
    }
}
