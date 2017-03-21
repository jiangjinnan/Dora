using System;
using System.Threading.Tasks;

namespace Dora.Caching
{
  public interface ICache
  {
    Task<CacheValue> GetAsync(string key);
    Task SetAsync(string key, object value);
    Task RemoveAsync(string key);
    Task ClearAsync();
  }
}
