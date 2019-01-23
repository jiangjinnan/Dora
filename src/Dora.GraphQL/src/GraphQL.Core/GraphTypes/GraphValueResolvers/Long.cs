using System;
using System.Globalization;

namespace Dora.GraphQL.GraphTypes
{
    internal static partial class GraphValueResolver
    {
        public static Func<object, object> Long = ResolveLong;
        private static object ResolveLong(object rawValue)
        {
            if (rawValue is long)
            {
                return rawValue;
            }           
            var strValue = rawValue.ToString();
            return long.TryParse(strValue, out var result)
                ? result
                : throw new GraphException($"Cannot resolve '{rawValue}' as a Long value.");
        }
    }
}
