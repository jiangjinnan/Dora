using Dora.ExceptionHandling.Configuration;
using Dora.ExceptionHandling.Configuration.Properties;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.FileProviders;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;

namespace Dora.ExceptionHandling
{
    /// <summary>
    /// Defines some extension methods for <see cref="IExceptionManagerBuilder"/>.
    /// </summary>
    public static class ExceptionManagerBuilderExtensions
    {
        private const string KeyOfFilters = "__Filters";
        private static ConcurrentDictionary<Type, ExceptionFilterConfiguration> _filterConfigurations = new ConcurrentDictionary<Type, ExceptionFilterConfiguration>();
        private static ConcurrentDictionary<Type, ExceptionHandlerConfiguration> _handlerConfigurations = new ConcurrentDictionary<Type, ExceptionHandlerConfiguration>();

        /// <summary>
        /// Get registered exception filters stored in the specified <see cref="IExceptionManagerBuilder"/>'s property collection.
        /// </summary>
        /// <param name="builder">The <see cref="IExceptionManagerBuilder"/> whose property collection includes the filters to get.</param>
        /// <returns>A <see cref="IDictionary{String, IExceptionFilter}"/> representing the named exception filters.</returns>
        /// <exception cref="ArgumentNullException">The specified <paramref name="builder"/> is null.</exception>
        public static IDictionary<string, IExceptionFilter> GetFilters(this IExceptionManagerBuilder builder)
        {
            var filters = Guard.ArgumentNotNull(builder, nameof(builder)).Properties.TryGetValue(KeyOfFilters, out object value)
                ? value
                : builder.Properties[KeyOfFilters] = new Dictionary<string, IExceptionFilter>(StringComparer.OrdinalIgnoreCase);
            return (IDictionary<string, IExceptionFilter>)filters;
        }

        /// <summary>
        /// Register an exception filter to specified <see cref="IExceptionManagerBuilder"/>.
        /// </summary>
        /// <param name="builder">The <see cref="IExceptionManagerBuilder"/> to which the exception filter is registered.</param>
        /// <param name="filterName">The name of the registered exception filter.</param>
        /// <param name="filterType">The type of exception filter.</param>
        /// <param name="argumnets">The arguments passed to construct the target exception filter.</param>
        /// <returns>The <see cref="IExceptionManagerBuilder"/> with the exception filter registration.</returns>
        /// <exception cref="ArgumentNullException">The specified <paramref name="builder"/> is null.</exception>
        /// <exception cref="ArgumentNullException">The specified <paramref name="filterName"/> is null.</exception>
        /// <exception cref="ArgumentNullException">The specified <paramref name="filterType"/> is null.</exception>
        /// <exception cref="ArgumentException">The specified <paramref name="filterName"/> is a white space string.</exception>
        public static IExceptionManagerBuilder UseFilter(this IExceptionManagerBuilder builder, string filterName, Type filterType, params object[] argumnets)
        {
            Guard.ArgumentNotNull(builder, nameof(builder));
            Guard.ArgumentNotNullOrWhiteSpace(filterName, nameof(filterName));
            Guard.ArgumentAssignableTo<IExceptionFilter>(filterType, nameof(filterType));

            builder.GetFilters()[filterName] = (IExceptionFilter)ActivatorUtilities.CreateInstance(builder.ServiceProvider, filterType, argumnets);
            return builder;
        }

        /// <summary>
        /// Load exception policy settings to specified <see cref="IExceptionManagerBuilder"/>.
        /// </summary>
        /// <param name="builder">The <see cref="IExceptionManagerBuilder"/> to which the loaded settings is appled.</param>
        /// <param name="filePath">The path of the exception policy configuration file.</param>
        /// <param name="fileProvider">The <see cref="IFileProvider"/> used to load the configuration file.</param>
        /// <returns>The <see cref="IExceptionManagerBuilder"/> to which the loaded settings is appled.</returns>
        /// <remarks>If <paramref name="fileProvider"/> is not explicitly specified, the current directory specific <see cref="PhysicalFileProvider"/> will be used.</remarks>
        /// <exception cref="ArgumentNullException">The specified <paramref name="builder"/> is null.</exception>
        /// <exception cref="ArgumentNullException">The specified <paramref name="filePath"/> is null.</exception>
        /// <exception cref="ArgumentException">The specified <paramref name="filePath"/> is a white space string.</exception>
        public static IExceptionManagerBuilder LoadSettings(this IExceptionManagerBuilder builder, string filePath, IFileProvider fileProvider = null)
        {
            Guard.ArgumentNotNull(builder, nameof(builder));
            Guard.ArgumentNotNullOrWhiteSpace(filePath, nameof(filePath));
            fileProvider = fileProvider ?? new PhysicalFileProvider(Directory.GetCurrentDirectory());

            var configBuilder = new ConfigurationBuilder(fileProvider, filePath);
            var policies = configBuilder.Build(out IDictionary<string, FilterConfiguration> filters);
            foreach (var it in filters)
            {
                ExceptionFilterConfiguration config = GetFilterConfiguration(it.Value.FilterType);
                config.Use(builder, it.Key, it.Value.Arguments.ToDictionary(item=>item.Name, item=>item.Value));
            }

            foreach (var it in policies)
            {
                builder.AddPolicy(it.Key, policyBuilder => BuildPolicy(policyBuilder, it.Value, builder.GetFilters()));
            }
            return builder;
        }

        private static void BuildPolicy(IExceptionPolicyBuilder builder, PolicyConfiguration config, IDictionary<string, IExceptionFilter> filters)
        {
            foreach (var it in config.PreHandlers)
            {
                var handlerConfig = GetHandlerConfiguration(it.HandlerType);
                var filterableConfig = it as FilterableHandlerConfiguration;
                if (null == filterableConfig)
                {
                    builder.Pre(handlerBuilder => handlerConfig.Use(handlerBuilder, _ => true, it.Arguments.ToDictionary(item => item.Name, item => item.Value)));
                }
                else
                {
                    builder.Pre(handlerBuilder => handlerConfig.Use(handlerBuilder, context => filters[filterableConfig.Filter].Match(context), it.Arguments.ToDictionary(item => item.Name, item => item.Value)));
                }
            }

            foreach (var it in config.PostHandlers)
            {
                var handlerConfig = GetHandlerConfiguration(it.HandlerType);
                var filterableConfig = it as FilterableHandlerConfiguration;
                if (null == filterableConfig)
                {
                    builder.Post(handlerBuilder => handlerConfig.Use(handlerBuilder, _ => true, it.Arguments.ToDictionary(item => item.Name, item => item.Value)));
                }
                else
                {
                    builder.Post(handlerBuilder => handlerConfig.Use(handlerBuilder, context => filters[filterableConfig.Filter].Match(context), it.Arguments.ToDictionary(item => item.Name, item => item.Value)));
                }
            }

            foreach (var entry in config.PolicyEntries)
            {
                builder.For(entry.ExceptionType, entry.PostHandlingAction, handlerBuilder => {
                    foreach (var it in entry.Handlers)
                    {
                        var handlerConfig = GetHandlerConfiguration(it.HandlerType);
                        var filterableConfig = it as FilterableHandlerConfiguration;
                        if (null == filterableConfig)
                        {
                            handlerConfig.Use(handlerBuilder, _ => true, it.Arguments.ToDictionary(item => item.Name, item => item.Value));
                        }
                        else
                        {
                            handlerConfig.Use(handlerBuilder, context => filters[filterableConfig.Filter].Match(context), it.Arguments.ToDictionary(item => item.Name, item => item.Value));
                        }
                    }
                });
            }
        }

        private static ExceptionHandlerConfiguration GetHandlerConfiguration(Type handlerType)
        {
            if (_handlerConfigurations.TryGetValue(handlerType, out ExceptionHandlerConfiguration config))
            {
                return config;
            }
            HandlerConfigurationAttribute attribute = handlerType.GetTypeInfo().GetCustomAttribute<HandlerConfigurationAttribute>();
            if (null == attribute)
            {
                throw new InvalidOperationException(Resources.ExceptionHandlerConfigurationTypeNotSet.Fill(handlerType.FullName));
            }
            return _handlerConfigurations[handlerType] = (ExceptionHandlerConfiguration)Activator.CreateInstance(attribute.HandlerConfigurationType);
        }

        private static ExceptionFilterConfiguration GetFilterConfiguration(Type filterType)
        {
            if (_filterConfigurations.TryGetValue(filterType, out ExceptionFilterConfiguration config))
            {
                return config;
            }
            FilterConfigurationAttribute attribute = filterType.GetTypeInfo().GetCustomAttribute<FilterConfigurationAttribute>();
            if (null == attribute)
            {
                throw new InvalidOperationException(Resources.ExceptionFilterConfigurationTypeNotSet.Fill(filterType.FullName));
            }
            return _filterConfigurations[filterType] = (ExceptionFilterConfiguration)Activator.CreateInstance(attribute.FilterConfigurationType);
        }
    }
}
