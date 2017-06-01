using Dora.ExceptionHandling.Configuration;
using System;
using System.Collections.Generic;
using System.Text;

namespace Dora.ExceptionHandling
{
    /// <summary>
    /// The <see cref="ExceptionHandlerConfiguration"/> specific to <see cref="ReplaceHandler"/>.
    /// </summary>
    public class ReplaceHandlerConfiguration : ExceptionHandlerConfiguration
    {
        private const string ConfigurationNameOfReplaceException = "replaceException";
        private const string ConfigurationNameOfMessage = "message";

        /// <summary>
        /// Register the specific exception handler based on configuration.
        /// </summary>
        /// <param name="predicate">A predicate to indicate whether to invoke the registered handler.</param>
        /// <param name="builder">The <see cref="IExceptionHandlerBuilder"/> used to register the specific exception handler.</param>
        /// <param name="configuration">A <see cref="IDictionary{String, String}"/> storing configuration for the exception handler to register.</param>
        /// <exception cref="ArgumentNullException">The specified <paramref name="builder"/> is null.</exception>
        /// <exception cref="ArgumentNullException">The specified <paramref name="predicate"/> is null.</exception>
        /// <exception cref="ArgumentNullException">The specified <paramref name="configuration"/> is null.</exception>
        public override void Use(IExceptionHandlerBuilder builder, Func<ExceptionContext, bool> predicate, IDictionary<string, string> configuration)
        {
            Guard.ArgumentNotNull(builder, nameof(builder));
            Guard.ArgumentNotNull(configuration, nameof(configuration));

            Type replaceExcecptionType = Type.GetType(configuration.GetValue(ConfigurationNameOfReplaceException));
            string message = configuration.GetValue(ConfigurationNameOfMessage);
            builder.Use<ReplaceHandler>(replaceExcecptionType, predicate, message);
        }
    }
}
