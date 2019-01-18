using System;
using System.Globalization;

namespace Dora.GraphQL.GraphTypes
{
    public partial class GraphValueResolver
    {
        public static GraphValueResolver DateTimeOffset = new GraphValueResolver("DateTimeOffset", typeof(DateTimeOffset), true, ResolveDateTimeOffset);
        private static object ResolveDateTimeOffset(object rawValue)
        {
            if (rawValue is DateTimeOffset)
            {
                return rawValue;
            }
           
            var strValue = rawValue.ToString();
            return System.DateTimeOffset.TryParseExact(strValue, "yyyy-MM-dd HH:mm:ss +/-HH:MM", CultureInfo.CurrentCulture, DateTimeStyles.AllowWhiteSpaces, out var result)
                ? result
                : throw new GraphException($"Cannot resolve '{rawValue}' as a DateTimeOffset value.");
        }
    }
}
