using Dora.Caching.Properties;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Dora.Caching
{
  public abstract class Cache : ICache
  {
    private readonly EventWaitHandle _waitHandle;
    private readonly CacheEntryOptions _options4KeysAndTypes;

    public string Name { get; }

    public Cache(string name)
    {
      Guard.ArgumentNotNullOrWhiteSpace(name, nameof(name));
      this.Name = name;
      _options4KeysAndTypes = new CacheEntryOptions
      {
        Priority = CacheItemPriority.NeverRemove,
         SlidingExpiration = TimeSpan.MaxValue
      };
      _waitHandle = new EventWaitHandle(true, EventResetMode.AutoReset);
    }

    #region ICache
    public async Task ClearAsync()
    {
      _waitHandle.WaitOne();
      try
      {
        foreach (var key in await this.GetKeysAsync())
        {
          await this.RemoveCoreAsync(key);
        }
        await this.SetKeysAsync(new List<string>());
      }
      finally
      {
        _waitHandle.Reset();
      }
    }

    public async Task SetAsync(string key, object value)
      {
      Guard.ArgumentNotNullOrWhiteSpace(key, nameof(key));
      _waitHandle.WaitOne();
      try
      {
        var realKey = this.GenerateCacheKey(key);
        value = value ?? NullValue.Instance;
        await this.SetCoreAsync(realKey, value);
        await this.SetValueTypeAsync(key, value.GetType());
        var keys = await this.GetKeysAsync();
        if (!keys.Contains(realKey))
        {
          keys.Add(realKey);
          await this.SetKeysAsync(keys);
        }
      }
      finally
      {
        _waitHandle.Set();
      }
    }

    public async Task<CacheValue> GetAsync(string key)
    {
      Guard.ArgumentNotNullOrWhiteSpace(key, nameof(key));
      Type type = await this.GetValueTypeAsync(key);
      if (null == type)
      {
        return CacheValue.NonExistent;
      }
      var realKey = this.GenerateCacheKey(key);
      var cacheValue = await this.GetCoreAsync(realKey, type);

      if (!cacheValue.Exists)
      {
        var keys = await this.GetKeysAsync();
        if (keys.Contains(realKey))
        {
          keys.Remove(realKey);
          await this.SetKeysAsync(keys);
        }

        string keyOfType = $"{this.GenerateCacheKey(key)}.TYPE";
        await this.RemoveCoreAsync(keyOfType);
      }

      if (cacheValue.Value is NullValue)
      {
        cacheValue.Value = null;
      }
      return cacheValue;
    }

    public async Task RemoveAsync(string key)
    {
      Guard.ArgumentNotNullOrWhiteSpace(key, nameof(key));
      _waitHandle.WaitOne();
      try
      {
        var realKey = this.GenerateCacheKey(key);
        await this.RemoveCoreAsync(realKey);
        var keys = await this.GetKeysAsync();
        if (keys.Contains(realKey))
        {
          keys.Remove(realKey);
          await this.SetKeysAsync(keys);
        }
        string keyOfType = $"{this.GenerateCacheKey(key)}.TYPE";
        await this.RemoveCoreAsync(keyOfType);
      }
      finally
      {
        _waitHandle.Set();
      }
    }
    #endregion
 
    #region Abstract Methods
    protected abstract Task<CacheValue> GetCoreAsync(string key, Type valueType);
    protected abstract Task SetCoreAsync(string key, object value, CacheEntryOptions options = null);
    protected abstract Task RemoveCoreAsync(string key);
    #endregion

    protected virtual string GenerateCacheKey(string key)
    {
      Guard.ArgumentNotNullOrWhiteSpace(key, nameof(key));
      return $"{this.Name.ToUpperInvariant()}.{key}";
    }

    private async Task<List<string>> GetKeysAsync()
    {
      string keyOfKeys = $"{this.Name.ToUpperInvariant()}.KEYS";
      var cacheValue = await this.GetCoreAsync(keyOfKeys, typeof(List<string>));
      return (cacheValue.Value as List<string>) ?? new List<string>();
    }

    private async Task SetKeysAsync(List<string> keys)
    {
      string keyOfKeys = $"{this.Name.ToUpperInvariant()}.KEYS";
      await this.SetCoreAsync(keyOfKeys, keys, _options4KeysAndTypes);
    }

    private async Task<Type> GetValueTypeAsync(string key)
    {
      string keyOfType = $"{this.GenerateCacheKey(key)}.TYPE";
      var cacheValue = await this.GetCoreAsync(keyOfType, typeof(string));
      return cacheValue.Exists
        ? Type.GetType(cacheValue.Value.ToString())
        : null;
    }

    private async Task SetValueTypeAsync(string key, Type type)
    {
      string keyOfType = $"{this.GenerateCacheKey(key)}.TYPE";
      await this.SetCoreAsync(keyOfType, type.AssemblyQualifiedName, _options4KeysAndTypes);
    }

    private class NullValue
    {
      private NullValue() { }
      public static NullValue Instance = new NullValue();
      public override bool Equals(object obj)
      {
        return obj is NullValue;
      }
      public override int GetHashCode()
      {
        return this.GetType().GetHashCode();
      }
    }
  }
}
