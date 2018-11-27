using System;
using System.Collections.Concurrent;

namespace Dora.DynamicProxy
{
    internal static class TypeExtensions
    {       
        public static Type GetNonByRefType(this Type type)
        {
            return type.IsByRef ? type.GetElementType() : type; 
        }
    }
} 