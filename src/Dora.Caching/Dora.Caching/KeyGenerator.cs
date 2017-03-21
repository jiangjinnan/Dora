using System;
using System.Collections.Generic;
using System.Text;

namespace Dora.Caching
{
  internal class KeyGenerator
  {
    private readonly Cache _cache;
    public KeyGenerator(Cache cache)=> _cache = cache;
    public string GenerateCacheKey(string key) => $"{_cache.Name.ToUpperInvariant()}.{key}";
    public string GenerateKeyOfKeys()=> $"{_cache.Name.ToUpperInvariant()}.KEYS";
    public string GenerateKeyOfType(string key)=> $"{_cache.Name.ToUpperInvariant()}.{key}.TYPE";
  }
}
