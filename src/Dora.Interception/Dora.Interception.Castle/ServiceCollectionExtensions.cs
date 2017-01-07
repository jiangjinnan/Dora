using Dora.Interception.Castle;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Microsoft.Extensions.DependencyInjection
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddInterception(this IServiceCollection services)
        {
            return services.AddInterception<DynamicProxyFactory>();
        }
    }
}
