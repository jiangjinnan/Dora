using Dora.Caching.Memory;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.DependencyInjection;
using System;
using Xunit;

namespace Dora.Caching.Test
{
  public class MemoryServiceCollectionExtensionsFixture
  {
    [Fact]
    public void AddMemoryCacheFactory_Argument_Not_Allow_Null()
    {
      IServiceCollection services = null;
      Assert.Throws<ArgumentNullException>(() => services.AddMemoryCacheFactory());
    }

    [Fact]
    public void AddMemoryCacheFactory()
    {
      IServiceProvider provider = new ServiceCollection()
       .AddMemoryCacheFactory()
       .BuildServiceProvider();

      Assert.NotNull(provider.GetRequiredService<IMemoryCache>());
      Assert.IsType<MemoryCacheFactory>(provider.GetRequiredService<ICacheFactory>());
    }
  }
}
