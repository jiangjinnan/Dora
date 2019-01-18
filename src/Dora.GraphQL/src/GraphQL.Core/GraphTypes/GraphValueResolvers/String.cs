using System;
using System.Globalization;

namespace Dora.GraphQL.GraphTypes
{
    public partial class GraphValueResolver
    {
        public static GraphValueResolver String = new GraphValueResolver("String", typeof(string), true, ResolveString);
        private static object ResolveString(object rawValue) => rawValue.ToString();
    }
}
