using Dora.ExceptionHandling.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Text;

namespace Dora.ExceptionHandling
{
    public static class ExceptionManagerBuilderExtensions
    {
        private const string KeyOfFilters = "__Filters";
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
    }
}
