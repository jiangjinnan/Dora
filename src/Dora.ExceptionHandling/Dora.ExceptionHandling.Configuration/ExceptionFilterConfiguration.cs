using System;
using System.Collections.Generic;
using System.Text;

namespace Dora.ExceptionHandling.Configuration
{
    public abstract class ExceptionFilterConfiguration
    {
        /// <summary>
        /// Register the specific exception filter based on configuration.
        /// </summary>
        /// <param name="builder">The <see cref="IExceptionHandlerBuilder"/> used to register the specific exception filter.</param>
        /// <param name="configuration">A <see cref="IDictionary{string, string}"/> storing configuration for the exception filter to register.</param>
        public abstract void Use(IExceptionManagerBuilder builder, IDictionary<string, string> configuration);
    }
}
