using Dora.ExceptionHandling.Configuration.Properties;
using Microsoft.Extensions.FileProviders;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text;
using System.Linq;
using Dora.ExceptionHandling.Configuration;

namespace Dora.ExceptionHandling
{
    public static class ExceptionManagerBuilderExtensions
    {
        private static ConcurrentDictionary<Type, Type> _configuartionTypeMapping = new ConcurrentDictionary<Type, Type>();
        public static IExceptionManagerBuilder LoadSettings(this IExceptionManagerBuilder builder, string filePath, IFileProvider fileProvider = null)
        {
            Guard.ArgumentNotNull(builder, nameof(builder));
            Guard.ArgumentNotNullOrWhiteSpace(filePath, nameof(filePath));

            fileProvider = fileProvider ?? new PhysicalFileProvider(Directory.GetCurrentDirectory());
            var configBuilder = new ConfigurationBuilder(fileProvider, filePath);
            var settings = configBuilder.Build();
            foreach (var it in settings)
            {
                builder.AddPolicy(it.Key, policyBuilder => BuildExceptionPolicy(policyBuilder, it.Value));
            }
            return builder;
        }

        private static void BuildExceptionPolicy(IExceptionPolicyBuilder builder, ExceptionPolicyElement policy)
        {
            foreach (var preHandler in policy.PreHandlers)
            {
                var config = GetHandlerConfiguration(preHandler.HandlerType);
                builder.Pre(handlerBuilder => config.Use(handlerBuilder, preHandler.Arguments.ToDictionary(it => it.Name, it => it.Value)));
            }

            foreach (var preHandler in policy.PostHandlers)
            {
                var config = GetHandlerConfiguration(preHandler.HandlerType);
                builder.Post(handlerBuilder => config.Use(handlerBuilder, preHandler.Arguments.ToDictionary(it => it.Name, it => it.Value)));
            }

            foreach (var entry in policy.PolicyEntries)
            {
                builder.For(entry.ExceptionType, entry.PostHandlingAction, handlerBuilder =>
                {
                    foreach (var handler in entry.Handlers)
                    {
                        GetHandlerConfiguration(handler.HandlerType)
                        .Use(handlerBuilder, handler.Arguments.ToDictionary(it => it.Name, it => it.Value));
                    }
                });
            }
        }

        private static ExceptionHandlerConfiguration GetHandlerConfiguration(Type handlerType)
        {
            if (_configuartionTypeMapping.TryGetValue(handlerType, out Type confiogurtionType))
            {
                return (ExceptionHandlerConfiguration)Activator.CreateInstance(confiogurtionType);
            }

            HandlerConfigurationAttribute attribute = handlerType.GetTypeInfo().GetCustomAttribute<HandlerConfigurationAttribute>();
            if (null == attribute)
            {
                throw new InvalidOperationException(Resources.ExceptionHandlerConfigurationTypeNotSet.Fill(handlerType.FullName));
            }
            return (ExceptionHandlerConfiguration)Activator.CreateInstance(_configuartionTypeMapping[handlerType] = attribute.HandlerConfigurationType);
        }
    }
}
