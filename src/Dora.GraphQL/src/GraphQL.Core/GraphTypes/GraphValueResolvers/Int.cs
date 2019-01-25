using System;
using System.Globalization;

namespace Dora.GraphQL.GraphTypes
{
    internal static partial class GraphValueResolver
    {
        public static Func<object, object> Int = ResolveInt;
        private static object ResolveInt(object rawValue)
        {
            if (rawValue == null)
            {
                return null;
            }

            if (rawValue is int)
            {
                return rawValue;
            }           
            var strValue = rawValue.ToString();
            return int.TryParse(strValue, out var result)
                ? result
                : throw new GraphException($"Cannot resolve '{rawValue}' as an Int value.");
        }
    }
}
