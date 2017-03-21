using Microsoft.Extensions.DependencyInjection;
using System;
using System.Threading.Tasks;
using Xunit;

namespace Dora.Caching.Test
{
  public class MemoryCacheFactoryFixture
  {
    [Fact]
    public async void Set_And_Get_Value_Memory()
    {
      ICache cache = new ServiceCollection()
        .AddMemoryCacheFactory()
        .BuildServiceProvider()
        .GetRequiredService<ICacheFactory>()
        .Map("Cache1", new CacheEntryOptions { AbsoluteExpirationRelativeToNow = TimeSpan.FromSeconds(2) })
        .Get("Cache1");

      object value = new object();
      await cache.SetAsync("key1", value);
      var cacheValue = await cache.GetAsync("key1");
      Assert.Same((await cache.GetValueAsync<object>("key1")), value);
      await Task.Delay(2000);
      Assert.Null((await cache.GetValueAsync<object>("key1")));

      await cache.SetAsync("key2", null);
      cacheValue = await cache.GetAsync("key2");
      Assert.True(cacheValue.Exists);
      Assert.Null(cacheValue.Value);
    }

    [Fact]
    public async void Set_And_Get_Value_Distributed()
    {
      ICache cache = new ServiceCollection()
        .AddDistributedRedisCache(options=>options.Configuration = "localhost")
        .AddDistributedCacheFactory(builder=>builder.SetJsonSerializer())
        .BuildServiceProvider()
        .GetRequiredService<ICacheFactory>()
        .Map("Cache1", new CacheEntryOptions { AbsoluteExpirationRelativeToNow = TimeSpan.FromSeconds(2) })
        .Get("Cache1");

      object value = "abc";
      await cache.SetAsync("key1", value);
      var cacheValue = await cache.GetAsync("key1");
      Assert.Equal((await cache.GetValueAsync<object>("key1")), value);
      await Task.Delay(2000);
      Assert.Null((await cache.GetValueAsync<object>("key1")));

      await cache.SetAsync("key2", null);
      cacheValue = await cache.GetAsync("key2");
      Assert.True(cacheValue.Exists);
      Assert.Null(cacheValue.Value);
    }
  }
}
