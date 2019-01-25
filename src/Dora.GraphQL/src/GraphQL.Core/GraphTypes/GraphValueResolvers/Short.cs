using System;
using System.Globalization;

namespace Dora.GraphQL.GraphTypes
{
    internal static partial class GraphValueResolver
    {
        public static Func<object, object> Short = ResolveShort;
        private static object ResolveShort(object rawValue)
        {
            if (rawValue == null)
            {
                return null;
            }

            if (rawValue is short)
            {
                return rawValue;
            }           
            var strValue = rawValue.ToString();
            return short.TryParse(strValue, out var result)
                ? result
                : throw new GraphException($"Cannot resolve '{rawValue}' as a Short value.");
        }
    }
}
