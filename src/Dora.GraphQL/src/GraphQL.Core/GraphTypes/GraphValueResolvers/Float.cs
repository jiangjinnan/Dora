using System;
using System.Globalization;

namespace Dora.GraphQL.GraphTypes
{
    public partial class GraphValueResolver
    {
        public static GraphValueResolver Float = new GraphValueResolver("Float", typeof(float), true,  ResolveFloat);
        private static object ResolveFloat(object rawValue)
        {
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
