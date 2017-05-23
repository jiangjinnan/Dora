using Dora.ExceptionHandling.Configuration;
using System;
using System.Collections.Generic;
using System.Text;

namespace Dora.ExceptionHandling
{
    /// <summary>
    /// 
    /// </summary>
    public class ReplaceHandlerConfiguration : ExceptionHandlerConfiguration
    {
        private const string ConfigurationNameOfReplaceException = "replaceException";
        private const string ConfigurationNameOfMessage = "message";

        /// <summary>
        /// 
        /// </summary>
        /// <param name="builder"></param>
        /// <param name="configuration"></param>
        public override void Use(IExceptionHandlerBuilder builder, IDictionary<string, string> configuration)
        {
            Guard.ArgumentNotNull(builder, nameof(builder));
            Guard.ArgumentNotNull(configuration, nameof(configuration));

            Type replaceExcecptionType = Type.GetType(configuration.GetValue(ConfigurationNameOfReplaceException));
            string message = configuration.GetValue(ConfigurationNameOfMessage);
            builder.Use<ReplaceHandler>(replaceExcecptionType, message);
        }
    }
}
