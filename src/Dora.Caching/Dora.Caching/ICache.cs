using System.Threading.Tasks;

namespace Dora.Caching
{
  /// <summary>
  /// Represents a general cache.
  /// </summary>
  public interface ICache
  {
    /// <summary>
    /// Retrieves the specified item from the cache object.
    /// </summary>
    /// <param name="key">The identifier for the cache item to retrieve. </param>
    /// <returns>A task to ge the cache value.</returns>
    Task<CacheValue> GetAsync(string key);

    /// <summary>
    /// Inserts a new or overrides an existing cache item with a cache key to reference its location, 
    /// </summary>
    /// <param name="key">The cache key used to reference the item.</param>
    /// <param name="value">The object to be set into the cache.</param>
    /// <returns>A task to set the cache item.</returns>
    Task SetAsync(string key, object value);

    /// <summary>
    /// Removes the specified item from the cache object.
    /// </summary>
    /// <param name="key">he cache key used to reference the item to remove.</param>
    /// <returns>The task to remove the cache item.</returns>
    Task RemoveAsync(string key);

    /// <summary>
    /// Clears all cache items.
    /// </summary>
    /// <returns>The task to clear all cache items.</returns>
    Task ClearAsync();
  }
}
