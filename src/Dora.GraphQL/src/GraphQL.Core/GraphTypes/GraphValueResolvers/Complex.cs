using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Linq;

namespace Dora.GraphQL.GraphTypes
{
    public static partial class GraphValueResolver
    {
        public static Func<object, object> Complex(Type type)
        {
            if (type.IsEnumerableType(out _))
            {
                throw new GraphException($"GraphValueResolver cannot be created based on an enumerable type '{type}'");
            }

            var name = type.IsGenericType
                ? $"{type.Name.Substring(0, type.Name.IndexOf('`'))}Of{string.Join("", type.GetGenericArguments().Select(it => it.Name))}"
                : type.Name;
            return rawValue => ResolveComplex(type,rawValue, name);
        }

        private static object ResolveComplex(Type type, object rawValue, string name)
        {
            if (type.IsAssignableFrom(rawValue.GetType()))
            {
                return rawValue;
            }
            var jToken = rawValue as JToken;
            if (null != jToken)
            {
                return jToken.ToObject(type);
            }
            throw new GraphException($"Cannot resolve '{rawValue}' as a/an {name} value.");
        }
    }
}