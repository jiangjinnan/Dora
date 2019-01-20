using Dora.GraphQL.GraphTypes;
using System;
using System.Collections.Generic;
using System.Text;

namespace Dora.GraphQL
{
    internal static  class DictionaryExtensions
    {
        public static bool TryGetGetField(this IDictionary<NamedType, GraphField> fields, Type type, string name, out GraphField field)
        {
            Guard.ArgumentNotNull(fields, nameof(fields));
            Guard.ArgumentNotNull(type, nameof(type));
            Guard.ArgumentNotNullOrWhiteSpace(name, nameof(name));
            if (fields.TryGetValue(new NamedType(name, type), out field))
            {
                return true;
            }

            var baseType = type.BaseType;
            if (baseType != null && TryGetGetField(fields, baseType, name, out field))
            {
                return true;
            }

            foreach (var @interface in type.GetInterfaces())
            {
                if (TryGetGetField(fields, @interface, name, out field))
                {
                    return true;
                }
            }

            return (field = null) != null;
        }

        public static bool TryGetGetField(this IDictionary<NamedType, GraphField> fields, object container, string name, out GraphField field)
        {
            Guard.ArgumentNotNull(container, nameof(container));
            return fields.TryGetGetField(container.GetType(), name, out field);
        }
    }
}
