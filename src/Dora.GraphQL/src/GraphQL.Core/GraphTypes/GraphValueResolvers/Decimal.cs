using System;
using System.Globalization;

namespace Dora.GraphQL.GraphTypes
{
    public static partial class GraphValueResolver
    {
        public static Func<object, object> Decimal = ResolveDecimal;
        private static object ResolveDecimal(object rawValue)
        {
            if (rawValue is decimal)
            {
                return rawValue;
            }
           
            var strValue = rawValue.ToString();
            return decimal.TryParse(strValue, out var result)
                ? result
                : throw new GraphException($"Cannot resolve '{rawValue}' as a Decimal value.");
        }
    }
}
