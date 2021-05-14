using System;

namespace Dora.Interception
{
    internal static class TypeExtensions
    {
        public static Type SelfOrElementType(this Type type) => type.IsByRef ? type.GetElementType() : type;
    }
}
