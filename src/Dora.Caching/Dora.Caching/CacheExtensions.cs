using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Dora.Caching
{
  public static class CacheExtensions
  {
    public static async Task<T> GetValueAsync<T>(this ICache cache, string key, T defaultValue = default(T))
    {
      Guard.ArgumentNotNull(cache, nameof(cache));
      Guard.ArgumentNotNullOrWhiteSpace(key, nameof(key));

      var cacheValue = await cache.GetAsync(key);
      return cacheValue.Exists ? (T)cacheValue.Value : defaultValue;
    }
  }
}
