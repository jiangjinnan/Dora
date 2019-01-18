using Dora.GraphQL.GraphTypes;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Dora.GraphQL.Resolvers
{
    public class PropertyResolver : IGraphResolver
    {
        private readonly Func<object, object> _resolver;
        private readonly PropertyInfo _propertyInfo;

        public PropertyResolver(PropertyInfo propertyInfo)
        {
            _propertyInfo = propertyInfo ?? throw new ArgumentNullException(nameof(propertyInfo));
            var source = Expression.Parameter(typeof(object), "source");
            var convert = Expression.Convert(source, propertyInfo.DeclaringType);
            Expression accessor = Expression.Call(convert, propertyInfo.GetMethod);
            if (propertyInfo.PropertyType.IsValueType)
            {
                accessor = Expression.Convert(accessor, typeof(object));
            }
            _resolver = Expression.Lambda<Func<object, object>>(accessor, source).Compile();
        }

        public ValueTask<object> ResolveAsync(ResolverContext context)
        {
            var result = _resolver(context.Container);
            return new ValueTask<object>(result);
        }
    }
}
