using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Xunit;
using System.Linq;

namespace Dora.Caching.Test
{
  public class CacheFixture
  {
    private static Dictionary<string, object> _store = new Dictionary<string, object>();

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData(" ")]
    public void New_Name_Not_Allow_Null_Or_White_Space(string name)
    {
      Assert.ThrowsAny<ArgumentException>(() => new FoobarCache(name));
    }

    [Fact]
    public void New()
    {
      Assert.Equal("Foobar", new FoobarCache("Foobar").Name);
    }

    [Fact]
    public async void ClearAsync()
    {
      _store.Clear();
      _store.Add("Abc", new object());

      var cache = new FoobarCache("Foobar");
      await cache.SetAsync("123", new object());
      await cache.SetAsync("456", new object());
      await cache.SetAsync("789", new object());

      Assert.True( _store.Count>1);
      await cache.ClearAsync();
      Assert.Equal("Abc", _store.Single().Key);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData(" ")]
    public async void SetAsync_Key_Not_Allow_Null_Or_White_Space(string key)
    {
      await Assert.ThrowsAnyAsync<ArgumentException>(() => new FoobarCache("Foobar").SetAsync(key, new object()));
    }

    [Fact]
    public async void SetAsync()
    {
      _store.Clear();
      var cache = new FoobarCache("Foobar");
      var valule = DateTime.Now;
      await cache.SetAsync("123", valule);
      Assert.Equal(valule, _store["FOOBAR.123"]);
      Assert.Equal("123", ((List<string>)_store["FOOBAR.KEYS"]).Single());
      Assert.Equal(typeof(DateTime).AssemblyQualifiedName, _store["FOOBAR.123.TYPE"]);

      await cache.ClearAsync();
      await cache.SetAsync("456", null);
      Assert.Equal(Cache.NullValue.Instance, _store["FOOBAR.456"]);
      Assert.Equal("456", ((List<string>)_store["FOOBAR.KEYS"]).Single());
      Assert.Equal(typeof(Cache.NullValue).AssemblyQualifiedName,_store["FOOBAR.456.TYPE"]);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData(" ")]
    public async void GetAsync_Key_Not_Allow_Null_Or_White_Space(string key)
    {
      await Assert.ThrowsAnyAsync<ArgumentException>(() => new FoobarCache("Foobar").GetAsync(key));
    }

    [Fact]
    public async void GetAsync()
    {
      _store.Clear();
      _store["FOOBAR.key1"] = 123;
      _store["FOOBAR.key1.TYPE"] = typeof(int).AssemblyQualifiedName;
      _store["FOOBAR.KEYS"] = new List<string> { "key1" };

      var cache = new FoobarCache("Foobar");
      Assert.True((await cache.GetAsync("key1")).Exists);
      Assert.Equal(123, (await cache.GetAsync("key1")).Value);
      await cache.ClearAsync();
      Assert.False(_store.Any());


      Assert.False((await cache.GetAsync("key2")).Exists);
      _store["FOOBAR.key2"] = Cache.NullValue.Instance;
      _store["FOOBAR.key2.TYPE"] = typeof(Cache.NullValue).AssemblyQualifiedName;
      _store["FOOBAR.KEYS"] = new List<string> { "key2" };
      Assert.True((await cache.GetAsync("key2")).Exists);
      Assert.Null((await cache.GetAsync("key2")).Value);
    }

    private class FoobarCache : Cache
    {
      public FoobarCache(string name) : base(name)
      {
      }

      protected internal override Task<CacheValue> GetCoreAsync(string key, Type valueType)
      {
        return _store.TryGetValue(key, out object value)
          ? Task.FromResult(new CacheValue { Exists = true, Value = value })
          : Task.FromResult(CacheValue.NonExistent);
      }

      protected internal override Task RemoveCoreAsync(string key)
      {
        _store.Remove(key);
        return Task.CompletedTask;
      }

      protected internal override Task SetCoreAsync(string key, object value, CacheEntryOptions options = null)
      {
        _store[key] = value;
        return Task.CompletedTask;
      }
    }
  }
}
