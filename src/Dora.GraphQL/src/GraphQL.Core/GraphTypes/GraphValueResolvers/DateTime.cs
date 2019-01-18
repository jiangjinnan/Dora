using System;
using System.Globalization;

namespace Dora.GraphQL.GraphTypes
{
    public partial class GraphValueResolver
    {
        public static GraphValueResolver DateTime = new GraphValueResolver("DateTime", typeof(DateTimeOffset), true, ResolveDateTime);
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
