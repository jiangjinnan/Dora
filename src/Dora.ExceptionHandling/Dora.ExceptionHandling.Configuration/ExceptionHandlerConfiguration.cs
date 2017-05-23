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
        /// <param name="builder">The <see cref="IExceptionHandlerBuilder"/> used to register the specific exception handler.</param>
        /// <param name="configuration">A <see cref="IDictionary{string, string}"/> storing configuration for the exception handler to register.</param>
        public abstract void Use(IExceptionHandlerBuilder builder, IDictionary<string, string> configuration);

        public void Use(IExceptionHandlerBuilder builder, IDictionary<string, string> configuration, Func<IExceptionFilter> filterAccessor)
        {

        }
    }
}
