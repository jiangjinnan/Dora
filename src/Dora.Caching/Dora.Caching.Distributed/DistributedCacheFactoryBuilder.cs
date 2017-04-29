using Dora;
using Dora.Caching.Distributed;
using Microsoft.Extensions.DependencyInjection.Extensions;
using System;
using System.Collections.Generic;
using System.Text;

namespace Microsoft.Extensions.DependencyInjection
{
  public class DistributedCacheFactoryBuilder
  {
    public IServiceCollection Services { get; }

    public DistributedCacheFactoryBuilder(IServiceCollection services)
    {
      this.Services = Guard.ArgumentNotNull(services, nameof(services));
    }

    public DistributedCacheFactoryBuilder AddSerializer(Func<IServiceProvider, ICacheSerializer> serializerFactory)
    {
      this.Services.AddSingleton<ICacheSerializer>(serializerFactory);
      return this;
    }
  }
}
