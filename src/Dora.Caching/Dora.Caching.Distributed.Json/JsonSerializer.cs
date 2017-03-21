using Newtonsoft.Json;
using System;
using System.Text;

namespace Dora.Caching.Distributed.Json
{
  public class JsonSerializer : ICacheSerializer
  {
    public object Deserialized(byte[] bytes, Type type)
    {
      Guard.ArgumentNotNull(bytes, nameof(bytes));
      Guard.ArgumentNotNull(type, nameof(type));

     return  JsonConvert.DeserializeObject(Encoding.UTF8.GetString(bytes), type);
    }

    public byte[] Serialize(object value)
    {
      Guard.ArgumentNotNull(value, nameof(value));
      return Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(value));
    }
  }
}
