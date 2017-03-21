using Microsoft.Extensions.Caching.Memory;
using System;
using System.Collections.Generic;
using System.Text;

namespace Dora.Caching.Memory
{
  public class MemoryCacheFactory : CacheFactory
  {
    private readonly IMemoryCache _cache;
    public MemoryCacheFactory(IMemoryCache cache)
    {
      Guard.ArgumentNotNull(cache, nameof(cache));
      _cache = cache;
    }
    protected override ICache Create(string name, CacheEntryOptions options)
    {
      Guard.ArgumentNotNullOrWhiteSpace(name, nameof(name));
      Guard.ArgumentNotNull(options, nameof(options));
      MemoryCacheEntryOptions options2 = new MemoryCacheEntryOptions
      {
        AbsoluteExpiration = options.AbsoluteExpiration,
        AbsoluteExpirationRelativeToNow = options.AbsoluteExpirationRelativeToNow,
        SlidingExpiration = options.SlidingExpiration,
        Priority = (Microsoft.Extensions.Caching.Memory.CacheItemPriority)(int)options.Priority
      };
      return new MemoryCache(name, _cache, options2);
    }
  }
}
