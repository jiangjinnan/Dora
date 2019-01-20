using System;
using System.Linq;

namespace Dora.GraphQL.GraphTypes
{
    [AttributeUsage( AttributeTargets.Class| AttributeTargets.Interface)]
    public class KnownTypesAttribute: Attribute
    {
        public Type[] Types { get; }
        public KnownTypesAttribute(params Type[] types)
        {
            Guard.ArgumentNotNullOrEmpty(types, nameof(types));
            if (types.Any(it => it.IsEnumerableType(out _)))
            {
                throw new ArgumentException($"Known type should not be created on enumerable types.");
            }
            Types = types;
        }
    }
}
