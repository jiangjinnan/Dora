using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Dora.Interception
{
    internal static class InterfaceMapper
    {
        public static IDictionary<MethodInfo, MethodInfo> GetMethodMap(Type @interface, Type implementationType)
        {
            var dictionary = new Dictionary<MethodInfo, MethodInfo>();
            if (implementationType.IsGenericTypeDefinition)
            {
                var interfaceMethods = @interface.GetMethods();
                var implementationMethods = implementationType.GetMethods(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
                foreach (var interfaceMethod in interfaceMethods)
                {
                    var targetMethods = implementationMethods.Where(it => Match(it, interfaceMethod)).ToArray();
                    var explicitlyImplemented = targetMethods.SingleOrDefault(it => it.Name.EndsWith($"{@interface.FullName}.{interfaceMethod.Name}"));
                    if (explicitlyImplemented != null)
                    {
                        dictionary.Add(interfaceMethod, explicitlyImplemented);
                        continue;
                    }
                    if (targetMethods.Length == 1)
                    {
                        dictionary.Add(interfaceMethod, targetMethods[0]);
                        continue;
                    }
                    throw new InvalidOperationException($"Cannot locate the target method of the interface method {@interface.Name}.{interfaceMethod.Name}");
                }
            }
            else
            {
                var mapping = implementationType.GetInterfaceMap(@interface);
                for (int index = 0; index < mapping.InterfaceMethods.Length; index++)
                {
                    dictionary.Add(mapping.InterfaceMethods[index], mapping.TargetMethods[index]);
                }
            }
            return dictionary;
        }

        private static bool Match(MethodInfo targetMethod, MethodInfo interfaceMethod)
        {
            if (targetMethod.Name != interfaceMethod.Name && targetMethod.Name != $"{interfaceMethod.DeclaringType.FullName}.{interfaceMethod.Name}")
            {
                return false;
            }

            var parameters1 = targetMethod.GetParameters();
            var parameters2 = interfaceMethod.GetParameters();

            if (parameters1.Length != parameters2.Length)
            {
                return false;
            }

            for (int index = 0; index < parameters1.Length; index++)
            {
                var parameterType1 = parameters1[index].ParameterType;
                var parameterType2 = parameters2[index].ParameterType;

                if (parameterType1.IsGenericParameter != parameterType2.IsGenericParameter)
                {
                    return false;
                }

                if (!parameterType1.IsGenericParameter && parameterType1 != parameterType2)
                {
                    return false;
                }
            }

            return true;
        }
    }
}
