using Dora.Caching.Memory;
using Microsoft.Extensions.Caching.Memory;
using System;
using Xunit;

namespace Dora.Caching.Test
{
  public class MemoryCacheFactoryFixture
  {
    [Fact]
    public void New_Argument_Not_Allow_Null()
    {
      Assert.Throws<ArgumentNullException>(() => new MemoryCacheFactory(null));
    }
    [Theory]
    [InlineData(null, "1")]
    [InlineData("", "1")]
    [InlineData(" ", "1")]
    [InlineData("1", null)]
    public void Create_Arguments_Not_Allow_Null_Or_White_Space(string name, string optionsIndicator)
    {
      CacheEntryOptions options = optionsIndicator == null ? null : new CacheEntryOptions();
      var factory = new MemoryCacheFactory(new FoobarCache());
      Assert.ThrowsAny<ArgumentException>(() => factory.Create(name, options));
    }

    [Fact]
    public void Create()
    {
      var factory = new MemoryCacheFactory(new FoobarCache());
      var cache = (Memory.MemoryCache)factory.Create("Foobar", new CacheEntryOptions());
      Assert.Equal("Foobar", cache.Name);
    }

    private class FoobarCache : IMemoryCache
    {
      public ICacheEntry CreateEntry(object key)
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
