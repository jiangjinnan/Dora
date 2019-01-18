using System;
using System.Globalization;

namespace Dora.GraphQL.GraphTypes
{
    public static partial class GraphValueResolver
    {
        public static Func<object, object> String =  ResolveString;
        private static object ResolveString(object rawValue) => rawValue.ToString();
    }
}
