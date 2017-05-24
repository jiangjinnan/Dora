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
    public static class ExceptionManagerBuilderExtensions
    {
        private const string KeyOfFilters = "__Filters";
        private static ConcurrentDictionary<Type, ExceptionFilterConfiguration> _filterConfigurations = new ConcurrentDictionary<Type, ExceptionFilterConfiguration>();
        private static ConcurrentDictionary<Type, ExceptionHandlerConfiguration> _handlerConfigurations = new ConcurrentDictionary<Type, ExceptionHandlerConfiguration>();

        public static IDictionary<string, IExceptionFilter> GetFilters(this IExceptionManagerBuilder builder)
        {
            var filters = Guard.ArgumentNotNull(builder, nameof(builder)).Properties.TryGetValue(KeyOfFilters, out object value)
                ? value
                : builder.Properties[KeyOfFilters] = new Dictionary<string, IExceptionFilter>(StringComparer.OrdinalIgnoreCase);
            return (Dictionary<string, IExceptionFilter>)filters;
        }

        public static IExceptionManagerBuilder UseFilter(this IExceptionManagerBuilder builder, string filterName, Type filterType, params object[] argumnets)
        {
            Guard.ArgumentNotNull(builder, nameof(builder));
            Guard.ArgumentNotNullOrWhiteSpace(filterName, nameof(filterName));
            Guard.ArgumentAssignableTo<IExceptionFilter>(filterType, nameof(filterType));

            builder.GetFilters()[filterName] = (IExceptionFilter)ActivatorUtilities.CreateInstance(builder.ServiceProvider, filterType, argumnets);
            return builder;
        }

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
