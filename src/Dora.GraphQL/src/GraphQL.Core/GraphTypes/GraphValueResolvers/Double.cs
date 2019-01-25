using System;
using System.Globalization;

namespace Dora.GraphQL.GraphTypes
{
    internal static partial class GraphValueResolver
    {
        public static Func<object, object> Double = ResolveDouble;
        private static object ResolveDouble(object rawValue)
        {
            if (rawValue == null)
            {
                return null;
            }

            if (rawValue is double)
            {
                return rawValue;
            }           
            var strValue = rawValue.ToString();
            return double.TryParse(strValue, out var result)
                ? result
                : throw new GraphException($"Cannot resolve '{rawValue}' as a Double value.");
        }
    }
}
