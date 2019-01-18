using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Dora.GraphQL
{
    internal static class AttributeAccessorExtensions
    {
        public static bool TryGetAttribute<TAttribute>(this IAttributeAccessor accessor, MemberInfo member, bool inherit, out TAttribute attribute)
            where TAttribute : Attribute
        {
            attribute = accessor.GetCustomAttributes(member, typeof(TAttribute), inherit).OfType<TAttribute>().FirstOrDefault();
            return attribute != null;
        }

        public static IEnumerable<TAttribute> GetAttributes<TAttribute>(this IAttributeAccessor accessor, MemberInfo member, bool inherit)
             where TAttribute : Attribute
            => accessor.GetCustomAttributes(member, typeof(TAttribute), inherit).OfType<TAttribute>();

        public static TAttribute GetAttribute<TAttribute>(this IAttributeAccessor accessor, MemberInfo member, bool inherit)
             where TAttribute : Attribute
            => accessor.GetAttributes<TAttribute>(member, inherit).FirstOrDefault();

        public static bool TryGetAttribute<TAttribute>(this IAttributeAccessor accessor, ParameterInfo  parameter, bool inherit, out TAttribute attribute)
            where TAttribute : Attribute
        {
            attribute = accessor.GetCustomAttributes(parameter, typeof(TAttribute)).OfType<TAttribute>().FirstOrDefault();
            return attribute != null;
        }

        public static IEnumerable<TAttribute> GetAttributes<TAttribute>(this IAttributeAccessor accessor, ParameterInfo parameter)
            where TAttribute : Attribute
           => accessor.GetCustomAttributes(parameter, typeof(TAttribute)).OfType<TAttribute>();

        public static TAttribute GetAttribute<TAttribute>(this IAttributeAccessor accessor, ParameterInfo parameter)
           where TAttribute : Attribute
          => accessor.GetAttributes<TAttribute>(parameter).FirstOrDefault();
    }
}
