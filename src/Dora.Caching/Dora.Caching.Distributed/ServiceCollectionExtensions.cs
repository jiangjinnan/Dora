using Dora;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Dora.Caching;
using Dora.Caching.Distributed;

namespace Microsoft.Extensions.DependencyInjection
{
  public static class ServiceCollectionExtensions
  {
    public static IServiceCollection AddDistributedCacheFactory(this IServiceCollection services, Action<DistributedCacheFactoryBuilder> configure = null)
    {
      Guard.ArgumentNotNull(services, "services");
      services.TryAddSingleton<ICacheFactory, DistributedCacheFactory>();
      configure?.Invoke(new DistributedCacheFactoryBuilder(services));
      return services;
    }
  }
}
