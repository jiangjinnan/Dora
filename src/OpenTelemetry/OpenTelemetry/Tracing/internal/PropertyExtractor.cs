using System.Collections.Concurrent;
using System.Linq.Expressions;
using System.Reflection;

namespace Dora.OpenTelemetry.Tracing
{
    internal static class PropertyExtractor
    {
        private static readonly ConcurrentDictionary<Tuple<Type, string>, Func<object?,object?>> _accessors =  new ();
        public static T GetValue<T>(object payload, string propertyName)
        {
            var accessor = _accessors.GetOrAdd(new Tuple<Type, string>(payload.GetType(), propertyName), CreateAccessor);
            var value = accessor(payload);
            return (value is T result ? result : default)!;
        }

        private static Func<object?, object?> CreateAccessor(Tuple<Type, string> key)
        { 
            var property = key.Item1.GetProperty(key.Item2, BindingFlags.Instance| BindingFlags.Public| BindingFlags.NonPublic );
            if (property?.GetMethod is null)
            {
                return _ => null;
            }

            var payload = Expression.Parameter(typeof(object));
            var converted = Expression.Convert(payload, key.Item1);
            var result = Expression.Call(converted, property.GetMethod);
            var convertResult = Expression.Convert(result, typeof(object));
            return Expression.Lambda<Func<object?, object?>>(convertResult, payload).Compile();
               
        }
    }
}
