using System;
using System.Collections.Generic;
using System.Text;

namespace Dora.ExceptionHandling.Configuration
{
    /// <summary>
    /// The <see cref="IExceptionFilter"/> specific configuration.
    /// </summary>
    public abstract class ExceptionFilterConfiguration
    {
        /// <summary>
        /// Register the specific exception filter based on configuration.
        /// </summary>
        /// <param name="builder">The <see cref="IExceptionHandlerBuilder"/> used to register the specific exception filter.</param>
        /// <param name="filterName"></param>
        /// <param name="arguments">A <see cref="IDictionary{String, String}"/> storing configuration for the exception filter to register.</param>
        public abstract void Use(IExceptionManagerBuilder builder, string filterName, IDictionary<string, string> arguments);
    }
}
