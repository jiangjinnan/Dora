using Dora;
using Dora.Caching;
using Dora.Caching.Memory;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace Microsoft.Extensions.DependencyInjection
{
  public static class ServiceCollectionExtensions
  {
    public static IServiceCollection AddMemoryCacheFactory(this IServiceCollection services)
    {
      Guard.ArgumentNotNull(services, "services");
      services
        .AddMemoryCache()
        .TryAddSingleton<ICacheFactory, MemoryCacheFactory>();
      return services;
    }
  }
}
