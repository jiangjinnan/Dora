using Microsoft.Extensions.Caching.Distributed;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Dora.Caching.Distributed
{
  public class DistributedCache : Cache
  {
    private readonly IDistributedCache _cache;
    private readonly ICacheSerializer _serializer;
    private readonly DistributedCacheEntryOptions _options;

    public DistributedCache(string name, IDistributedCache cache, ICacheSerializer serializer, DistributedCacheEntryOptions options) : base(name)
    {
      _cache = Guard.ArgumentNotNull(cache, nameof(cache));
      _serializer = Guard.ArgumentNotNull(serializer, nameof(serializer));
      _options = Guard.ArgumentNotNull(options, nameof(options));
    }

    internal protected override async Task<CacheValue> GetCoreAsync(string key, Type valueType)
    {
      Guard.ArgumentNotNullOrWhiteSpace(key, nameof(key));
      Guard.ArgumentNotNull(valueType, nameof(valueType));

      var bytes = await _cache.GetAsync(key);
      if (null == bytes)
      {
        return CacheValue.NonExistent;
      }
      return new CacheValue { Exists = true, Value = _serializer.Deserialized(bytes, valueType) };
    }

    internal protected override async Task RemoveCoreAsync(string key)
    {
       Guard.ArgumentNotNullOrWhiteSpace(key, nameof(key));
       await _cache.RemoveAsync(key);
    }

    internal protected override async Task SetCoreAsync(string key, object value, CacheEntryOptions options= null)
    {
      Guard.ArgumentNotNullOrWhiteSpace(key, nameof(key));
      var options2 = options != null ? new DistributedCacheEntryOptions
      {
        AbsoluteExpiration = options.AbsoluteExpiration,
        AbsoluteExpirationRelativeToNow = options.AbsoluteExpirationRelativeToNow,
        SlidingExpiration = options.SlidingExpiration
      } : _options;
      await _cache.SetAsync(key, _serializer.Serialize(value), options2);
    }
  }
}
