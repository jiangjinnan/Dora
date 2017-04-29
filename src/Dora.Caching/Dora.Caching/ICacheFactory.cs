namespace Dora.Caching
{
  /// <summary>
  /// Represents a factory to create cache.
  /// </summary>
  public interface ICacheFactory
  {
    /// <summary>
    /// Registers a new cache. 
    /// </summary>
    /// <param name="name">The cache's name.</param>
    /// <param name="options">The cache entry options.</param>
    /// <returns>the cache factory with specified cache registration.</returns>
    ICacheFactory Register(string name, CacheEntryOptions options);

    /// <summary>
    /// Gets a registered cache.
    /// </summary>
    /// <param name="name">The name of registered cache to get.</param>
    /// <returns>The cache.</returns>
    ICache Get(string name);
  }
}
