using Dora.ExceptionHandling.Configuration.Properties;
using System;
using System.Collections.Generic;

namespace Dora.ExceptionHandling.Configuration
{
    /// <summary>
    /// Defines extension method to get configuration value.
    /// </summary>
    public static class HandlerConfigurationExtensions
    {
        /// <summary>
        /// Get the configuration property value.
        /// </summary>
        /// <param name="configuration">A <see cref="IDictionary{String, String} "/> representing the configuration.</param>
        /// <param name="name">The name of configuration property.</param>
        /// <returns>The configuration proeprty value.</returns>
        /// <exception cref="ArgumentNullException">The <paramref name="configuration"/> is null.</exception>
        /// <exception cref="ArgumentNullException">The <paramref name="name"/> is null.</exception>
        /// <exception cref="ArgumentException">The <paramref name="name"/> is a white space string or an invalid property name.</exception>
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
