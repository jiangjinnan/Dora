using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace Dora.Caching.Test
{
  public class KeyGeneratorFixture
  {
    [Fact]
    public void GenerateCacheKey()
    {
      Assert.Equal("FOOBAR.key", new KeyGenerator(new FoobarCache("Foobar")).GenerateCacheKey("key"));
    }
    [Fact]
    public void GenerateKeyOfKeys()
    {
      Assert.Equal("FOOBAR.KEYS", new KeyGenerator(new FoobarCache("Foobar")).GenerateKeyOfKeys());

    }
    [Fact]
    public void GenerateKeyOfType()
    {
      Assert.Equal("FOOBAR.key.TYPE", new KeyGenerator(new FoobarCache("Foobar")).GenerateKeyOfType("key"));
    }

    private class FoobarCache : Cache
    {
      public FoobarCache(string name) : base(name)
      {
      }

      protected internal override Task<CacheValue> GetCoreAsync(string key, Type valueType)
      {
        throw new NotImplementedException();
      }

      protected internal override Task RemoveCoreAsync(string key)
      {
        throw new NotImplementedException();
      }

      protected internal override Task SetCoreAsync(string key, object value, CacheEntryOptions options = null)
      {
        throw new NotImplementedException();
      }
    }
  }
}
