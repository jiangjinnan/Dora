using System;
using System.Globalization;

namespace Dora.GraphQL.GraphTypes
{
    internal static partial class GraphValueResolver
    {
        public static Func<object, object> DateTime = ResolveDateTime;
        private static object ResolveDateTime(object rawValue)
        {
            if (rawValue is DateTime)
            {
                return rawValue;
            }
           
            var strValue = rawValue.ToString();
            return System.DateTime.TryParseExact(strValue, "yyyy-MM-dd HH:mm:ss", CultureInfo.CurrentCulture, DateTimeStyles.AllowWhiteSpaces, out var result)
                ? result
                : throw new GraphException($"Cannot resolve '{rawValue}' as a DateTime value.");
        }
    }
}
