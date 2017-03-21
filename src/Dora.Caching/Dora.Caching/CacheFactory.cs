using Dora.Caching.Properties;
using Microsoft.Extensions.Primitives;
using System;
using System.Collections.Concurrent;

namespace Dora.Caching
{
  /// <summary>
  /// Represents the base class of all concrete cache factory classes.
  /// </summary>
  public abstract class CacheFactory : ICacheFactory
  {
    private readonly ConcurrentDictionary<string, CacheEntryOptions> _optionsMap;
    private readonly ConcurrentDictionary<string, ICache> _cacheMap;

    /// <summary>
    /// Create a new  <see cref="CacheFactory"/>.
    /// </summary>
    public CacheFactory()
    {
      _optionsMap = new ConcurrentDictionary<string, CacheEntryOptions>(StringComparer.OrdinalIgnoreCase);
      _cacheMap = new ConcurrentDictionary<string, ICache>(StringComparer.OrdinalIgnoreCase);
    }

    /// <summary>
    /// Gets a registered cache.
    /// </summary>
    /// <param name="name">The name of registered cache to get.</param>
    /// <returns>The cache.</returns>
    /// <exception cref="ArgumentNullException">The argument <paramref name="name"/> is null.</exception>
    /// <exception cref="ArgumentException">The argument <paramref name="name"/> is a white space string, or is not registered</exception>
    public ICache Get(string name)
    {
      Guard.ArgumentNotNullOrWhiteSpace(name, nameof(name));

      if (_cacheMap.TryGetValue(name, out ICache cache))
      {
        return cache;
      }
      if (_optionsMap.TryGetValue(name, out CacheEntryOptions options))
      {
        cache = this.Create(name,options);
        if(null != options.ExpirationTokenAccessor)
        {
          ChangeToken.OnChange(() => options.ExpirationTokenAccessor(), () => cache.ClearAsync());
        }
        return _cacheMap[name] = cache;
      }
      throw new ArgumentException(Resources.ExceptionCacheIsNotRegistered.Fill(name));
    }

    /// <summary>
    /// Registers a new cache. 
    /// </summary>
    /// <param name="name">The cache's name.</param>
    /// <param name="options">The cache entry options.</param>
    /// <returns>the cache factory with specified cache registration.</returns>
    /// <exception cref="ArgumentNullException">The argument <paramref name="name"/> is null.</exception>
    /// <exception cref="ArgumentException">The argument <paramref name="name"/> is a white space string..</exception>
    /// <exception cref="ArgumentNullException">The argument <paramref name="options"/> is null.</exception>
    /// <remarks>If a duplicate cache is registered, it will be overridden.</remarks>
    public ICacheFactory Register(string name, CacheEntryOptions options)
    {
      Guard.ArgumentNotNullOrWhiteSpace(name, nameof(name));
      Guard.ArgumentNotNull(options, nameof(options));

      _optionsMap[name] = options;
      return this;
    }

    /// <summary>
    /// Create a new cache.
    /// </summary>
    /// <param name="name">The cache's name.</param>
    /// <param name="options">The cache entry options.</param>
    /// <returns>The cache to create.</returns>
    internal protected abstract ICache Create(string name, CacheEntryOptions options); 
  }
}
