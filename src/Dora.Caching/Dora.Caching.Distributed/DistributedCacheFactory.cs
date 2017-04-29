using Microsoft.Extensions.Caching.Distributed;
using System;
using System.Collections.Generic;
using System.Text;

namespace Dora.Caching.Distributed
{
  public class DistributedCacheFactory : CacheFactory
  {
    private readonly IDistributedCache _cache;
    private ICacheSerializer _serializer;

    public DistributedCacheFactory(IDistributedCache cache, ICacheSerializer serializer)
    {
      _cache = Guard.ArgumentNotNull(cache, nameof(cache));
      _serializer = Guard.ArgumentNotNull(serializer, nameof(serializer));
    }

    internal protected override ICache Create(string name, CacheEntryOptions options)
    {
      Guard.ArgumentNotNullOrWhiteSpace(name, nameof(name));
      Guard.ArgumentNotNull(options, nameof(options));

      var options2 = new DistributedCacheEntryOptions
      {
        AbsoluteExpiration = options.AbsoluteExpiration,
        AbsoluteExpirationRelativeToNow = options.AbsoluteExpirationRelativeToNow,
        SlidingExpiration = options.SlidingExpiration
      };

      return new DistributedCache(name, _cache, _serializer, options2);
    }
  }
}
