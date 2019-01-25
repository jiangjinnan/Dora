using System;

namespace Dora.GraphQL.GraphTypes
{
    internal static partial class GraphValueResolver
    {
        public static Func<object, object> Guid = ResolveGuid;
        private static object ResolveGuid(object rawValue)
        {
            if (rawValue == null)
            {
                return null;
            }

            if (rawValue is Guid)
            {
                return rawValue;
            }
            var strValue = rawValue.ToString();
            return System.Guid.TryParse(strValue, out var result)
                ? result
                : throw new GraphException($"Cannot resolve '{rawValue}' as a Guid value.");
        }
    }
}