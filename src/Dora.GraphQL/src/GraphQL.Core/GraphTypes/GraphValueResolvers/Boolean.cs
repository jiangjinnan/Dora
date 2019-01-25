using System;
using System.Collections.Generic;
using System.Text;

namespace Dora.GraphQL.GraphTypes
{
    internal static partial class GraphValueResolver
    {
        public static Func<object, object> Boolean = ResolveBoolean;
        private static object ResolveBoolean(object rawValue)
        {
            if (rawValue == null)
            {
                return null;
            }
            if (rawValue is bool)
            {
                return rawValue;
            }

            if (rawValue is int)
            {
                var intValue = (int)rawValue;
                return intValue == 1
                    ? true
                    : intValue == 0
                    ? false
                    : throw new GraphException($"Cannot resolve '{rawValue}' as a Boolean value.");
            }
            var strValue = rawValue.ToString();
            return string.Compare(strValue, "true", true) == 0
                ? true
                : string.Compare(strValue, "false", true) == 0
                ? false
                : throw new GraphException($"Cannot resolve '{rawValue}' as a Boolean value.");
        }
    }
}