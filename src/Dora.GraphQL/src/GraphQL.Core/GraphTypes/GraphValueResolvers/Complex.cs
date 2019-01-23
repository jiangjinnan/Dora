using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Linq;

namespace Dora.GraphQL.GraphTypes
{
    internal static partial class GraphValueResolver
    {
        public static Func<object, object> Complex(Type type, IServiceProvider serviceProvider)
        {
            if (type.IsEnumerable(out _))
            {
                throw new GraphException($"GraphValueResolver cannot be created based on an enumerable type '{type}'");
            }

            var name = type.IsGenericType
                ? $"{type.Name.Substring(0, type.Name.IndexOf('`'))}Of{string.Join("", type.GetGenericArguments().Select(it => it.Name))}"
                : type.Name;
            return rawValue => ResolveComplex(type,rawValue, name, serviceProvider?.GetRequiredService<IJsonSerializerProvider>().JsonSerializer);
        }

        private static object ResolveComplex(Type type, object rawValue, string name, JsonSerializer serializer)
        {
            if (type.IsEnum)
            {
                return Enum.Parse(type, (string)rawValue);
            }

            if (type.IsAssignableFrom(rawValue.GetType()))
            {
                return rawValue;
            }
            var jToken = rawValue as JToken;
            if (null != jToken)
            {
                return jToken.ToObject(type, serializer);
            }
            throw new GraphException($"Cannot resolve '{rawValue}' as a/an {name} value.");
        }
    }
}