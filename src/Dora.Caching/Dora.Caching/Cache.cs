using Microsoft.Extensions.Primitives;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Dora.Caching
{
  /// <summary>
  /// Represents the base class of all concrete cache classes.
  /// </summary>
  public abstract class Cache : ICache
  {
    private readonly EventWaitHandle _waitHandle;
    private readonly CacheEntryOptions _options4KeysAndTypes;
    private readonly KeyGenerator _keyGenerator;

    /// <summary>
    /// Gets the name of the cache.
    /// </summary>
    public string Name { get; }

    /// <summary>
    /// Creates a new <see cref="Cache"/>.
    /// </summary>
    /// <param name="name">The name of the cache to create.</param>  
    /// <exception cref="ArgumentNullException">The argument <paramref name="name"/> is null.</exception>
    /// <exception cref="ArgumentException">The argument <paramref name="name"/> is a white space string.</exception>
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
      _keyGenerator = new KeyGenerator(this);
    }

    #region ICache
    /// <summary>
    /// Clears all cache items.
    /// </summary>
    /// <returns>The task to clear all cache items.</returns>
    public async Task ClearAsync()
    {
      _waitHandle.WaitOne();
      try
      {
        var value = await this.GetCoreAsync(_keyGenerator.GenerateKeyOfKeys(), typeof(List<string>));
        foreach (var key in (value.Value as List<string>) ?? new List<string>())
        {
          await this.RemoveCoreAsync(_keyGenerator.GenerateCacheKey(key));
          await this.RemoveCoreAsync(_keyGenerator.GenerateKeyOfType(key));
        }
        await this.RemoveCoreAsync(_keyGenerator.GenerateKeyOfKeys());
      }
      finally
      {
        _waitHandle.Set();
      }
    }

    /// <summary>
    /// Inserts a new or overrides an existing cache item with a cache key to reference its location, 
    /// </summary>
    /// <param name="key">The cache key used to reference the item.</param>
    /// <param name="value">The object to be set into the cache.</param>
    /// <returns>A task to set the cache item.</returns>
    /// <exception cref="ArgumentNullException">The argument <paramref name="key"/> is null.</exception>
    /// <exception cref="ArgumentException">The argument <paramref name="key"/> is a white space string.</exception>
    public async Task SetAsync(string key, object value)
    {
      Guard.ArgumentNotNullOrWhiteSpace(key, nameof(key));
      _waitHandle.WaitOne();
      try
      {
        value = value ?? NullValue.Instance;
        await this.SetCoreAsync(_keyGenerator.GenerateCacheKey(key), value);
        await this.AddKeyAndTypeAsync(key, value.GetType());
      }
      finally
      {
        _waitHandle.Set();
      }
    }
    /// <summary>
    /// Retrieves the specified item from the cache object.
    /// </summary>
    /// <param name="key">The identifier for the cache item to retrieve. </param>
    /// <returns>A task to ge the cache value.</returns>
    /// <exception cref="ArgumentNullException">The argument <paramref name="name"/> is null.</exception>
    /// <exception cref="ArgumentException">The argument <paramref name="name"/> is a white space string.</exception>
    public async Task<CacheValue> GetAsync(string key)
    {
      Guard.ArgumentNotNullOrWhiteSpace(key, nameof(key));
      var typeCacheValue = await this.GetCoreAsync(_keyGenerator.GenerateKeyOfType(key), typeof(string));
      if (!typeCacheValue.Exists)
      {
        return CacheValue.NonExistent;
      }
      var cacheValue = await this.GetCoreAsync(_keyGenerator.GenerateCacheKey(key), Type.GetType(typeCacheValue.Value.ToString()));
      if (!cacheValue.Exists)
      {
        await this.RemoveKeyAndTypeAsync(key);
      }

      if (cacheValue.Value is NullValue)
      {
        cacheValue.Value = null;
      }
      return cacheValue;
    }

    /// <summary>
    /// Removes the specified item from the cache object.
    /// </summary>
    /// <param name="key">he cache key used to reference the item to remove.</param>
    /// <returns>The task to remove the cache item.</returns>
    /// <exception cref="ArgumentNullException">The argument <paramref name="name"/> is null.</exception>
    /// <exception cref="ArgumentException">The argument <paramref name="name"/> is a white space string.</exception>
    public async Task RemoveAsync(string key)
    {
      Guard.ArgumentNotNullOrWhiteSpace(key, nameof(key));
      _waitHandle.WaitOne();
      try
      {
        await this.RemoveCoreAsync(_keyGenerator.GenerateCacheKey(key));
        await this.RemoveKeyAndTypeAsync(key);
      }
      finally
      {
        _waitHandle.Set();
      }
    }
    #endregion
 
    #region Abstract Methods
    /// <summary>
    /// 
    /// </summary>
    /// <param name="key"></param>
    /// <param name="valueType"></param>
    /// <returns></returns>
    internal protected abstract Task<CacheValue> GetCoreAsync(string key, Type valueType);

    /// <summary>
    /// 
    /// </summary>
    /// <param name="key"></param>
    /// <param name="value"></param>
    /// <param name="options"></param>
    /// <returns></returns>
    internal protected abstract Task SetCoreAsync(string key, object value, CacheEntryOptions options = null);

    /// <summary>
    /// 
    /// </summary>
    /// <param name="key"></param>
    /// <returns></returns>
    internal protected abstract Task RemoveCoreAsync(string key);
    #endregion    

    private async Task AddKeyAndTypeAsync(string key, Type type)
    {
      string keyOfKeys = _keyGenerator.GenerateKeyOfKeys();
      string keyOfType = _keyGenerator.GenerateKeyOfType(key);

      var value = await this.GetCoreAsync(keyOfKeys, typeof(List<string>));
      var keys = (value.Value as List<string>) ?? new List<string>();
      if (!keys.Contains(key))
      {
        keys.Add(key);
      }
      await this.SetCoreAsync(keyOfKeys, keys, _options4KeysAndTypes);
      await this.SetCoreAsync(keyOfType, type.AssemblyQualifiedName, _options4KeysAndTypes);
    }

    private async Task RemoveKeyAndTypeAsync(string key)
    {
      string keyOfKeys = _keyGenerator.GenerateKeyOfKeys();
      string keyOfType = _keyGenerator.GenerateKeyOfType(key);

      var value = await this.GetCoreAsync(keyOfKeys, typeof(List<string>));
      var keys = (value.Value as List<string>) ?? new List<string>();
      if (keys.Contains(key))
      {
        keys.Remove(key);
      }
      await this.SetCoreAsync(keyOfKeys, keys, _options4KeysAndTypes);
      await this.RemoveCoreAsync(keyOfType);
    }

    internal class NullValue
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
