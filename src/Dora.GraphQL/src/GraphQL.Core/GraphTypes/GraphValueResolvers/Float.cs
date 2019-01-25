using System;
using System.Globalization;

namespace Dora.GraphQL.GraphTypes
{
    internal static partial class GraphValueResolver
    {
        public static Func<object, object> Float = ResolveFloat;
        private static object ResolveFloat(object rawValue)
        {
            if (rawValue == null)
            {
                return null;
            }

            if (rawValue is float)
            {
                return rawValue;
            }           
            var strValue = rawValue.ToString();
            return float.TryParse(strValue, out var result)
                ? result
                : throw new GraphException($"Cannot resolve '{rawValue}' as a Float value.");
        }
    }
}
