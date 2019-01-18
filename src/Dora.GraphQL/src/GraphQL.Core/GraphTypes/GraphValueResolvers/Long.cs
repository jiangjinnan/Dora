using System;
using System.Globalization;

namespace Dora.GraphQL.GraphTypes
{
    public partial class GraphValueResolver
    {
        public static GraphValueResolver Long = new GraphValueResolver("Long", typeof(long), true,  ResolveLong);
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
