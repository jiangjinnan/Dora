using Dora;
using Dora.Interception;
using System;
using Microsoft.Extensions.DependencyInjection.Extensions;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace Microsoft.Extensions.DependencyInjection
{
  /// <summary>
  /// Define some extension methods to register interception based services.
  /// </summary>
  public static class ServiceCollectionExtensions
  {
    /// <summary>
    /// Register interception based services.
    /// </summary>
    /// <param name="services">The service collection in which the service registrations are added.</param>
    /// <param name="configure"></param>
    /// <returns>The service collection with the interception based service registrations.</returns>
    public static IServiceCollection AddInterception(this IServiceCollection services, Action<InterceptionBuilder> configure = null)
    {
      Guard.ArgumentNotNull(services, nameof(services));
      configure?.Invoke(new InterceptionBuilder(services));
      services
         .AddScoped(typeof(IInterceptable<>), typeof(Interceptable<>))
         .TryAddScoped<IInterceptorChainBuilder, InterceptorChainBuilder>();
      return services;
    }
  }
}
