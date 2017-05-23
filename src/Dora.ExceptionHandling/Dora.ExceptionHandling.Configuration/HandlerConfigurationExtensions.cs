using Dora.ExceptionHandling.Configuration.Properties;
using System;
using System.Collections.Generic;

namespace Dora.ExceptionHandling.Configuration
{
    /// <summary>
    /// 
    /// </summary>
    public static class HandlerConfigurationExtensions
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="configuration"></param>
        /// <param name="name"></param>
        /// <returns></returns>
        public static string GetValue(this IDictionary<string, string> configuration, string name)
        {
            Guard.ArgumentNotNullOrEmpty(configuration, nameof(configuration));
            Guard.ArgumentNotNullOrWhiteSpace(name, nameof(name));

            if (configuration.TryGetValue(name, out string value))
            {
                return value;
            }
            throw new ArgumentException(Resources.ExceptionConfigurationPropertyNotExists.Fill(name), nameof(configuration));
        }
    }
}
