using System;
using System.Collections.Generic;
using System.Text;

namespace Dora.ExceptionHandling
{
    /// <summary>
    /// 
    /// </summary>
    public class WrapHandlerConfiguration : ExceptionHandlerConfiguration
    {
        private const string ConfigurationNameOfWrapException = "wrapException";
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
            Type wrapExcecptionType = Type.GetType(configuration.GetValue(ConfigurationNameOfWrapException));
            string message = configuration.GetValue(ConfigurationNameOfMessage);
            builder.Use<WrapHandler>(wrapExcecptionType, message);
        }
    }
}
