using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Text;

namespace Dora.DynamicProxy
{
    internal static class TypeExtensions
    {
        private static readonly ConcurrentDictionary<Type, Type> _mapping = new ConcurrentDictionary<Type, Type>();

        public static Type GetNonByRefType(this Type type)
        {
            if (type.IsByRef)
            {
                return _mapping.TryGetValue(type, out var nonByRefType)
                    ? nonByRefType
                    : _mapping[type] = Type.GetType(type.AssemblyQualifiedName.Replace("&", ""));
            }
            return type;
        }
    }
} 