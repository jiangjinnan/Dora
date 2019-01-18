using System;

namespace Dora.GraphQL.GraphTypes
{
    public partial class GraphValueResolver
    {
        public static GraphValueResolver TimeSpan = new GraphValueResolver("TimeSpan", typeof(TimeSpan), true, ResolveTimeSpan);
        private static object ResolveTimeSpan(object rawValue)
        {
            if (rawValue is long)
            {
                return rawValue;
            }
            var strValue = rawValue.ToString();
            return System.TimeSpan.TryParse(strValue, out var result)
                ? result
                : throw new GraphException($"Cannot resolve '{rawValue}' as a TimeSpan value.");
        }
    }
}