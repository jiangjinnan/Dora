using Dora.Caching.Properties;
using Microsoft.Extensions.Primitives;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Text;

namespace Dora.Caching
{
  public abstract class CacheFactory : ICacheFactory
  {
    private readonly ConcurrentDictionary<string, CacheEntryOptions> _optionsMap;
    private readonly ConcurrentDictionary<string, ICache> _cacheMap;

    public CacheFactory()
    {
      _optionsMap = new ConcurrentDictionary<string, CacheEntryOptions>(StringComparer.OrdinalIgnoreCase);
      _cacheMap = new ConcurrentDictionary<string, ICache>(StringComparer.OrdinalIgnoreCase);
    }

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
        foreach (var token in options.ExpirationTokensAccessor())
        {
          ChangeToken.OnChange(() => token, () => cache.ClearAsync());
        }
        return _cacheMap[name] = cache;
      }
      throw new ArgumentException(Resources.ExceptionCacheIsNotRegistered.Fill(name));
    }

    public ICacheFactory Map(string name, CacheEntryOptions options)
    {
      Guard.ArgumentNotNullOrWhiteSpace(name, nameof(name));
      Guard.ArgumentNotNull(options, nameof(options));

      _optionsMap[name] = options;
      return this;
    }

    protected abstract ICache Create(string name, CacheEntryOptions options); 
  }
}
