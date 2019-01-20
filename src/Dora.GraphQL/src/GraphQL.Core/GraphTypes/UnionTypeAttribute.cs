using System;
using System.Linq;

namespace Dora.GraphQL.GraphTypes
{
    [AttributeUsage(AttributeTargets.Property)]
    public class UnionTypeAttribute: Attribute
    {
        public Type[] Types { get; }
        public Type Type { get; }
        public Type[] OtherTypes { get; }
        public UnionTypeAttribute(params Type[] types)
        {
            if (types.Length <= 1)
            {
                throw new ArgumentException($"Union GraphType should be created on at least 2 sclar types.",nameof(types));
            }

            if (types.Any(it => it.IsEnumerableType(out _)))
            {
                throw new ArgumentException($"Union GraphType can not be created on enumerable types.");
            }
            Type = types[0];
            var otherTypes = new Type[types.Length - 1];
            for (int index = 1; index < types.Length; index++)
            {
                otherTypes[index - 1] = types[index];
            }
            OtherTypes = otherTypes;
            Types = types;
        }
    }
}
