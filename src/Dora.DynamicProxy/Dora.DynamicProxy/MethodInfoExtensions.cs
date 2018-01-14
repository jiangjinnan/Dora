using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Dora.DynamicProxy
{
    internal static class MethodInfoExtensions
    {
        public static bool ReturnVoid(this MethodInfo method)
        {
            return method.ReturnType == typeof(void);
        }

        public static bool ReturnTask(this MethodInfo method)
        {
            return method.ReturnType == typeof(Task);
        }

        public static bool ReturnTaskOfResult(this MethodInfo method)
        {
            return method.ReturnType.IsGenericType && method.ReturnType.GetGenericTypeDefinition() == typeof(Task<>);
        } 
    }
}
