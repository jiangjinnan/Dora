using System;
using System.Collections.Generic;
using System.Text;

namespace Dora.Caching.Distributed
{
  public interface ICacheSerializer
  {
    byte[] Serialize(object value);
    object Deserialized(byte[] bytes, Type type);
  }
}
