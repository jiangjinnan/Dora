using Microsoft.Extensions.Primitives;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace Dora.Caching.Test
{
  public class CacheFactoryFixture
  {
    private static string _expirationIndicator;
    private static CancellationTokenSource _source;

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("  ")]
    public void Get_Name_Not_Allow_Null_Or_White_Space(string name)
    {
      Assert.ThrowsAny<ArgumentException>(() => new FoobarCacheFactory().Get(name));
    }
    [Fact]
    public void Get_Name_Must_Be_Registered()
    {
      Assert.ThrowsAny<ArgumentException>(() => new FoobarCacheFactory().Get("Foobar"));
    }

    [Fact]
    public void Get()
    {
      var factory = new FoobarCacheFactory().Register("foobar", new CacheEntryOptions());
      FoobarCache cache1 = (FoobarCache)factory.Get("Foobar");
      FoobarCache cache2 = (FoobarCache)factory.Get("foobar");
      Assert.Same(cache1, cache2);
    }
    [Fact]
    public async void Get_Clear_On_Change()
    {
      _source = new CancellationTokenSource();
      Func<IChangeToken> changeTokenAccessor = () => _source.IsCancellationRequested ?
        new CancellationChangeToken((_source = new CancellationTokenSource()).Token)
        : new CancellationChangeToken(_source.Token);
      var factory = new FoobarCacheFactory().Register("foobar", new CacheEntryOptions { ExpirationTokenAccessor = changeTokenAccessor });
      var cache = factory.Get("foobar");
      _source.Cancel();
      await Task.Delay(100);
      Assert.NotNull(_expirationIndicator);

      _expirationIndicator = null;
      _source.Cancel();
      await Task.Delay(100);
      Assert.NotNull(_expirationIndicator);
    }

    [Theory]
    [InlineData(null, "1")]
    [InlineData("", "1")]
    [InlineData("  ", "1")]
    [InlineData("Foobar", null)]
    public void Register_Arguments_Not_Allow_Null_Or_White_Space(string name, string optionsIndicator)
    {
      CacheEntryOptions options = optionsIndicator == null ? null : new CacheEntryOptions();
      Assert.ThrowsAny<ArgumentException>(() => new FoobarCacheFactory().Get(name));
    }

    private class FoobarCache : ICache
    {
      public Task ClearAsync()
      {
        _expirationIndicator = Guid.NewGuid().ToString();
        return Task.CompletedTask;
      }

      public Task<CacheValue> GetAsync(string key)
      {
        throw new NotImplementedException();
      }

      public Task RemoveAsync(string key)
      {
        throw new NotImplementedException();
      }

      public Task SetAsync(string key, object value)
      {
        throw new NotImplementedException();
      }
    }

    private class FoobarCacheFactory : CacheFactory
    {
      internal protected override ICache Create(string name, CacheEntryOptions options)
      {
        return new FoobarCache();
      }
    }
  }
}
