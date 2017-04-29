using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.DependencyInjection;
using System;
using Xunit;

namespace Dora.Caching.Test
{
  public class MemoryCacheFixture
  {
    [Theory]
    [InlineData(null, "1")]
    [InlineData("1", null)]
    public void New_Arguments_Not_Allow_Null_Or_White_Space(string cacheIndicator, string optionsIndicator)
    {
      Microsoft.Extensions.Caching.Memory.IMemoryCache cache = cacheIndicator == null ? null : new FoobarCache();
      var options = optionsIndicator == null ? null : new Microsoft.Extensions.Caching.Memory.MemoryCacheEntryOptions();
      Assert.Throws<ArgumentNullException>(() => new Memory.MemoryCache("foo", cache, options));
    }

    [Fact]
    public async void SetCoreAsync_And_GetCoreAsync()
    {
      var cache = new ServiceCollection().AddMemoryCache().BuildServiceProvider().GetRequiredService<Microsoft.Extensions.Caching.Memory.IMemoryCache>();
      var options = new Microsoft.Extensions.Caching.Memory.MemoryCacheEntryOptions
      {
        AbsoluteExpirationRelativeToNow = TimeSpan.FromSeconds(2)
      };
      var memoryCache = new Memory.MemoryCache("foobar", cache, options);
      await memoryCache.SetCoreAsync("key1", 123);
      Assert.Equal(123, cache.TryGetValue("key1", out object value) ? value : null);
      Assert.Equal(123, (await memoryCache.GetCoreAsync("key1", null)).Value);
    }
    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData(" ")]
    public async void RemoveCoreAsync_Arguments_Not_Allow_Null_Or_White_Space(string key)
    {
      var memoryCache = new Memory.MemoryCache("foobar", new FoobarCache(), new Microsoft.Extensions.Caching.Memory.MemoryCacheEntryOptions());
      await Assert.ThrowsAnyAsync<ArgumentException>(() => memoryCache.RemoveCoreAsync(key));
    }

    [Fact]
    public async void RemoveCoreAsync()
    {
      var cache = new ServiceCollection().AddMemoryCache().BuildServiceProvider().GetRequiredService<Microsoft.Extensions.Caching.Memory.IMemoryCache>();
      var options = new MemoryCacheEntryOptions
      {
        AbsoluteExpirationRelativeToNow = TimeSpan.FromSeconds(2)
      };
      var memoryCache = new Memory.MemoryCache("foobar", cache, options);
      cache.Set("key1", 123);
      await memoryCache.RemoveCoreAsync("key1");
      Assert.False(cache.TryGetValue("key1", out object value));
    }

    [Fact]
    public async void RemoveCoreAsync_Duplicate()
    {
      var cache = new ServiceCollection().AddMemoryCache().BuildServiceProvider().GetRequiredService<Microsoft.Extensions.Caching.Memory.IMemoryCache>();
      var options = new MemoryCacheEntryOptions
      {
        AbsoluteExpirationRelativeToNow = TimeSpan.FromSeconds(2)
      };
      var memoryCache = new Memory.MemoryCache("foobar", cache, options);
      cache.Set("key1", 123);
      await memoryCache.RemoveCoreAsync("key1");
      await memoryCache.RemoveCoreAsync("key1");
    }

    private class FoobarCache : Microsoft.Extensions.Caching.Memory.IMemoryCache
    {
      public Microsoft.Extensions.Caching.Memory.ICacheEntry CreateEntry(object key)
      {
        throw new NotImplementedException();
      }

      public void Dispose()
      {
        throw new NotImplementedException();
      }

      public void Remove(object key)
      {
        throw new NotImplementedException();
      }

      public bool TryGetValue(object key, out object value)
      {
        throw new NotImplementedException();
      }
    }
  }
}
