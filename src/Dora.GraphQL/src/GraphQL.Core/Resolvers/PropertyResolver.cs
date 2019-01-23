using Dora.GraphQL.GraphTypes;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Dora.GraphQL.Resolvers
{
    /// <summary>
    /// Property accessing based <see cref="IGraphResolver"/>.
    /// </summary>
    public class PropertyResolver : IGraphResolver
    {
        private readonly Func<object, object> _resolver;
        private readonly PropertyInfo _propertyInfo;

        /// <summary>
        /// Initializes a new instance of the <see cref="PropertyResolver"/> class.
        /// </summary>
        /// <param name="propertyInfo">The <see cref="PropertyInfo"/>.</param>
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

        /// <summary>
        /// Resolves the value the current selection node.
        /// </summary>
        /// <param name="context">The <see cref="T:Dora.GraphQL.GraphTypes.ResolverContext" /> in which the field value is resoved.</param>
        /// <returns>
        /// The resolved field's value.
        /// </returns>
        public ValueTask<object> ResolveAsync(ResolverContext context)
        {
            var result = _resolver(context.Container);
            return new ValueTask<object>(result);
        }
    }
}
