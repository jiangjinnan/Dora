using Dora;
using Dora.Caching.Distributed;
using Dora.Caching.Distributed.Json;
using Microsoft.Extensions.DependencyInjection.Extensions;
using System;
using System.Collections.Generic;
using System.Text;

namespace Microsoft.Extensions.DependencyInjection
{
  public static class DistributedCacheFactoryBuilderExtensions
  {
    public static DistributedCacheFactoryBuilder SetJsonSerializer(this DistributedCacheFactoryBuilder builder)
    {
      Guard.ArgumentNotNull(builder, nameof(builder));
      builder.Services.TryAddSingleton<ICacheSerializer, JsonSerializer>();
      return builder;
    }
  }
}
