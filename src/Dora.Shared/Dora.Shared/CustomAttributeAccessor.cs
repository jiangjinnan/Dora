using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

namespace Dora
{
  internal static class CustomAttributeAccessor
  {
    private static ConcurrentDictionary<object, Attribute[]> _attributes = new ConcurrentDictionary<object, Attribute[]>();
    private static ConcurrentDictionary<object, Attribute[]> _ownAttributes = new ConcurrentDictionary<object, Attribute[]>();
    public static IEnumerable<Attribute> GetCustomAttributes(MemberInfo memberInfo, bool inherit)
    {
      Attribute[] attributes;
      if (inherit)
      {
        return _attributes.TryGetValue(memberInfo, out  attributes)
         ? attributes
         : _attributes[memberInfo] = memberInfo.GetCustomAttributes(true).ToArray();
        }

      return _ownAttributes.TryGetValue(memberInfo, out attributes)
        ? attributes
        : _attributes[memberInfo] = memberInfo.GetCustomAttributes(false).ToArray();
    }
    public static IEnumerable<Attribute> GetCustomAttributes(Type type, bool inherit)
    {
      TypeInfo typeInfo = type.GetTypeInfo();
      Attribute[] attributes;
      if (inherit)
      {
        return _attributes.TryGetValue(typeInfo, out attributes)
         ? attributes
         : _attributes[type] = typeInfo.GetCustomAttributes(true).ToArray();
      }

      return _ownAttributes.TryGetValue(typeInfo, out attributes)
        ? attributes
        : _attributes[type] = typeInfo.GetCustomAttributes(false).ToArray();
    }
  }
}
