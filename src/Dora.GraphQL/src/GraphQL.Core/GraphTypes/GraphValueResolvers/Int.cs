using System;
using System.Globalization;

namespace Dora.GraphQL.GraphTypes
{
    public partial class GraphValueResolver
    {
        public static GraphValueResolver Int = new GraphValueResolver("Int", typeof(int), true, ResolveInt);
        private static object ResolveInt(object rawValue)
        {
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
