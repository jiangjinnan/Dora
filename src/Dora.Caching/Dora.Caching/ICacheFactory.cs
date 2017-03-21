using System;
using System.Collections.Generic;
using System.Text;

namespace Dora.Caching
{
  public interface ICacheFactory
  {
    ICacheFactory Map(string name, CacheEntryOptions options);
    ICache Get(string name);
  }
}
