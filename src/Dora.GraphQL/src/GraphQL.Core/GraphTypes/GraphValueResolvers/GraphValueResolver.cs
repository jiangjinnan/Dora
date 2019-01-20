using System;
using System.Collections.Generic;
using System.Linq;

namespace Dora.GraphQL.GraphTypes
{
    public static partial class GraphValueResolver
    {
        private static Dictionary<Type, Func<object, object>> _sclarResolvers;
        private static Dictionary<Type, string> _scalarTypeNames;
        private static Type[] _scalarTypes;
        static GraphValueResolver()
        {
            _sclarResolvers = new Dictionary<Type, Func<object, object>>
            {
                [typeof(bool)] = Boolean,
                [typeof(DateTime)] = DateTime,
                [typeof(DateTimeOffset)] = DateTimeOffset,
                [typeof(decimal)] = Decimal,
                [typeof(float)] = Float,
                [typeof(double)] = Double,
                [typeof(short)] = Short,
                [typeof(int)] = Int,
                [typeof(long)] = Long,
                [typeof(TimeSpan)] = TimeSpan,
                [typeof(Guid)] = Guid,
                [typeof(string)] = String,
            };
            _scalarTypeNames = new Dictionary<Type, string>
            {
                [typeof(bool)] = "Boolean",
                [typeof(DateTime)] = "DateTime",
                [typeof(DateTimeOffset)] = "DateTimeOffset",
                [typeof(decimal)] = "Decimal",
                [typeof(float)] = "Float",
                [typeof(double)] = "Double",
                [typeof(short)] = "Short",
                [typeof(int)] = "Int",
                [typeof(long)] = "Long",
                [typeof(TimeSpan)] = "TimeSpan",
                [typeof(Guid)] = "Guid",
                [typeof(string)] = "String",
            };
            _scalarTypes = _scalarTypeNames.Keys.ToArray();
        }
        public static Func<object, object> GetResolver(Type type)
        {
            Guard.ArgumentNotNull(type, nameof(type));
            return _sclarResolvers.TryGetValue(type, out var value)
                ? value
                : Complex(type);
        }

        public static bool IsScalar(Type type) => _scalarTypeNames.ContainsKey(type);

        public static string GetGraphTypeName(Type type, params Type[] otherTypes)
        {
            Guard.ArgumentNotNull(type, nameof(type));
            return otherTypes.Any() 
                ? $"UnionOf{type.Name}{string.Join("",otherTypes.Select(it=>it.Name))}"
                : _scalarTypeNames.TryGetValue(type, out var value) ? value : GetComplexGraphTypeName(type);
        }

        public static Type[] ScalarTypes => _scalarTypes;

        private static string GetComplexGraphTypeName(Type type)
        {
            if (!type.IsGenericType)
            {
                return type.Name;
            }

            var argumentsPart = string.Join("", type.GetGenericArguments().Select(it => GetGraphTypeName(type)));
            return $"{type.Name.Substring(0, type.Name.Length - 2)}Of{argumentsPart}";
        }
    }
}
