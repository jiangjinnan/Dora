using Dora.Caching.Properties;
using System;
using System.Collections.Generic;
using System.Text;

namespace Dora.Caching
{
  public class CacheValue
  {
    private readonly static CacheValue _nonExistent = new CacheValue();
    public bool Exists { get; set; }
    public object Value { get; set; }
    public virtual bool IsValid
    {
      get { return this.Exists || null == this.Value; }
    }

    public static CacheValue NonExistent
    {
      get { return _nonExistent; }
    }
  }
}
