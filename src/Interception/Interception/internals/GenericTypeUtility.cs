using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

namespace Dora.Interception
{
    internal static class GenericTypeUtility
    {
        public static MethodInfo GetMethodInfo(Type closeType, MethodInfo method)
        {
            return closeType
                .GetMethods(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance)
                .Single(it=>Match(it, method));
            
        }

        private static bool Match(MethodInfo method1, MethodInfo method2)
        {
            if (method1.Name != method2.Name)
            {
                return false;
            }

            var parameters1 = method1.GetParameters();
            var parameters2 = method2.GetParameters();

            if (parameters1.Length != parameters2.Length)
            {
                return false;
            }

            for (int index = 0; index < parameters1.Length; index++)
            {
                if (parameters1[index].ParameterType != parameters2[index].ParameterType)
                {
                    return false;
                }
            }

            return true;
        }
    }
}
