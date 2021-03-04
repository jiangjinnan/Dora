using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Text;
using System.Threading.Tasks;

namespace Dora.Interception
{
    public abstract class InterceptableProxyGeneratorBase : IInterceptableProxyGenerator
    {
        public ModuleBuilder ModuleBuilder { get; }
        public IInterceptorRegistrationProvider RegistrationProvider { get; }
        public InterceptableProxyGeneratorBase(IInterceptorRegistrationProvider registrationProvider)
        {
            RegistrationProvider = registrationProvider ?? throw new ArgumentNullException(nameof(registrationProvider));
            var assemblyBuilder = AssemblyBuilder.DefineDynamicAssembly(new AssemblyName("Dora.Interception.InterceptableProxies"), AssemblyBuilderAccess.RunAndCollect);
            ModuleBuilder = assemblyBuilder.DefineDynamicModule("Dora.Interception.InterceptableProxies.dll");
        }

        public abstract Type Generate(Type serviceType, Type implementationType);

        protected void EmitReturnValueExtractionCode(ILGenerator il, MethodReturnKind returnKind, Type returnType, LocalBuilder invocationContext, LocalBuilder task)
        {
            switch (returnKind)
            {
                case MethodReturnKind.Void:
                    {
                        il.Emit(OpCodes.Ldloc, task);
                        il.Emit(OpCodes.Call, Members.WaitOfTask);
                        il.Emit(OpCodes.Ret);
                        break;
                    }
                case MethodReturnKind.Result:
                    {
                        il.Emit(OpCodes.Ldloc, task);
                        il.Emit(OpCodes.Ldloc, invocationContext);
                        il.Emit(OpCodes.Call, Members.GetResultOfProxyGeneratorHelper.MakeGenericMethod(returnType));
                        il.Emit(OpCodes.Ret);
                        break;
                    }
                case MethodReturnKind.Task:
                    {
                        il.Emit(OpCodes.Ldloc, task);
                        il.Emit(OpCodes.Ret);
                        break;
                    }
                case MethodReturnKind.TaskOfResult:
                    {
                        il.Emit(OpCodes.Ldloc, task);
                        il.Emit(OpCodes.Ldloc, invocationContext);
                        il.Emit(OpCodes.Call, Members.GetTaskOfResultOfProxyGeneratorHelper.MakeGenericMethod(returnType.GetGenericArguments()[0]));
                        il.Emit(OpCodes.Ret);
                        break;
                    }
                case MethodReturnKind.ValueTask:
                    {
                        il.Emit(OpCodes.Ldloc, task);
                        il.Emit(OpCodes.Ldloc, invocationContext);
                        il.Emit(OpCodes.Call, Members.GetValueTasOfProxyGeneratorHelper);
                        il.Emit(OpCodes.Ret);
                        break;
                    }
                case MethodReturnKind.ValueTaskOfResult:
                    {
                        il.Emit(OpCodes.Ldloc, task);
                        il.Emit(OpCodes.Ldloc, invocationContext);
                        il.Emit(OpCodes.Call, Members.GetValueTaskOfResultOfProxyGeneratorHelper.MakeGenericMethod(returnType.GetGenericArguments()[0]));
                        il.Emit(OpCodes.Ret);
                        break;
                    }
            }
        }

        protected TypeBuilder DefineClosureType(MethodMetadata methodMetadata, Type originalTargetType, out MethodInfo invokeMethod, out ConstructorInfo constructor, out Type[] constructorParameterTypes)
        {
            var closureTypeBuilder = CreateClosureType(methodMetadata, originalTargetType, out var targetType, out var parameterTypes, out var returnType, out var genericMethodArguments);
            var targetMethod = methodMetadata.MethodInfo;
            if (targetMethod.DeclaringType.IsGenericTypeDefinition)
            {
                targetMethod = GenericTypeUtility.GetMethodInfo(targetType, targetMethod);
            }

            var fields = new FieldBuilder[parameterTypes.Length + 2];
            constructorParameterTypes = new Type[parameterTypes.Length + 2];
            fields[0] = closureTypeBuilder.DefineField("_target", targetType, FieldAttributes.Private | FieldAttributes.InitOnly);
            constructorParameterTypes[0] = targetType;
            var parameters = targetMethod.GetParameters();
            for (int index = 0; index < parameterTypes.Length; index++)
            {
                var parameter = parameters[index];
                fields[index + 1] = closureTypeBuilder.DefineField($"_{parameter.Name}", parameterTypes[index], FieldAttributes.Private);
                constructorParameterTypes[index + 1] = parameterTypes[index];
            }
            fields[fields.Length - 1] = closureTypeBuilder.DefineField("_arguments", typeof(object[]), FieldAttributes.Private);
            constructorParameterTypes[fields.Length - 1] = typeof(object[]);

            var constructorBuilder = closureTypeBuilder.DefineConstructor(MethodAttributes.Public, CallingConventions.Standard, constructorParameterTypes);
            var il = constructorBuilder.GetILGenerator();

            il.Emit(OpCodes.Ldarg_0);
            il.Emit(OpCodes.Call, Members.ConstructorOfObject);

            //_target = target;
            il.Emit(OpCodes.Ldarg_0);
            il.EmitLdArgs(1);
            il.Emit(OpCodes.Stfld, fields[0]);

            //if (arguments == null)
            var argumentIsNull = il.DefineLabel();
            il.EmitLdArgs(constructorParameterTypes.Length);
            il.Emit(OpCodes.Ldnull);
            il.Emit(OpCodes.Ceq);
            il.Emit(OpCodes.Brtrue, argumentIsNull);

            //_arguments = arguments
            il.Emit(OpCodes.Ldarg_0);
            il.EmitLdArgs(fields.Length);
            il.Emit(OpCodes.Stfld, fields.Last());
            il.Emit(OpCodes.Ret);

            il.MarkLabel(argumentIsNull);
            //_x = x;
            //_y - y;
            //...
            for (int index = 1; index < fields.Length - 1; index++)
            {
                il.Emit(OpCodes.Ldarg_0);
                il.EmitLdArgs(index + 1);
                il.Emit(OpCodes.Stfld, fields[index]);
            }
            il.Emit(OpCodes.Ret);

            constructor = constructorBuilder;

            var invokeMethodBuilder = closureTypeBuilder.DefineMethod("InvokeAsync", MethodAttributes.Public | MethodAttributes.HideBySig, typeof(Task), new Type[] { typeof(InvocationContext) });
            il = invokeMethodBuilder.GetILGenerator();

            //Load _target
            il.Emit(OpCodes.Ldarg_0);
            il.Emit(OpCodes.Ldfld, fields.First());

            argumentIsNull = il.DefineLabel();
            var argumentsLoaded = il.DefineLabel();

            //if(_arguments == null)
            il.Emit(OpCodes.Ldarg_0);
            il.Emit(OpCodes.Ldfld, fields.Last());
            il.Emit(OpCodes.Ldnull);
            il.Emit(OpCodes.Ceq);
            il.Emit(OpCodes.Brtrue, argumentIsNull);

            //Load arguments from object array.
            for (int index = 0; index < fields.Length - 2; index++)
            {
                il.Emit(OpCodes.Ldarg_0);
                il.Emit(OpCodes.Ldfld, fields.Last());
                il.Emit(OpCodes.Ldc_I4, index);
                il.Emit(OpCodes.Ldelem_Ref);
                il.EmitUnboxOrCast(parameterTypes[index]);
            }
            il.Emit(OpCodes.Br_S, argumentsLoaded);

            il.MarkLabel(argumentIsNull);
            //Load seperate arguments
            for (int index = 1; index < fields.Length - 1; index++)
            {
                il.Emit(OpCodes.Ldarg_0);
                il.Emit(OpCodes.Ldfld, fields[index]);
            }

            il.MarkLabel(argumentsLoaded);
            if (targetMethod.IsGenericMethodDefinition)
            {
                targetMethod = targetMethod.MakeGenericMethod(genericMethodArguments);
            }
            if (targetType == targetMethod.DeclaringType)
            {
                il.Emit(OpCodes.Call, targetMethod);
            }
            else
            {
                il.Emit(OpCodes.Callvirt, targetMethod);
            }

            LocalBuilder returnValue = null;
            var returnKind = methodMetadata.ReturnKind;
            switch (returnKind)
            {
                case MethodReturnKind.Void:
                    {
                        il.Emit(OpCodes.Call, Members.GetCompletedTaskOfTask);
                        il.Emit(OpCodes.Ret);
                        break;
                    }
                case MethodReturnKind.Result:
                    {
                        returnValue = il.DeclareLocal(returnType);
                        il.Emit(OpCodes.Stloc, returnValue);
                        SetReturnValue();
                        il.Emit(OpCodes.Call, Members.GetCompletedTaskOfTask);
                        il.Emit(OpCodes.Ret);
                        break;
                    }
                case MethodReturnKind.Task:
                case MethodReturnKind.TaskOfResult:
                    {
                        returnValue = il.DeclareLocal(returnType);
                        il.Emit(OpCodes.Stloc, returnValue);
                        SetReturnValue();
                        il.Emit(OpCodes.Ldloc, returnValue);
                        il.Emit(OpCodes.Ret);
                        break;
                    }
                case MethodReturnKind.ValueTask:
                    {
                        il.Emit(OpCodes.Ldarg_1);
                        il.Emit(OpCodes.Call, Members.AsTaskByValueTaskOfProxyGeneratorHelper);
                        il.Emit(OpCodes.Ret);
                        break;
                    }
                case MethodReturnKind.ValueTaskOfResult:
                    {
                        il.Emit(OpCodes.Ldarg_1);
                        il.Emit(OpCodes.Call, Members.AsTaskByValueTaskOfResultOfProxyGeneratorHelper.MakeGenericMethod(returnType.GetGenericArguments()[0]));
                        il.Emit(OpCodes.Ret);
                        break;
                    }
            }

            invokeMethod = invokeMethodBuilder;
            return closureTypeBuilder;

            void SetReturnValue()
            {
                il.Emit(OpCodes.Ldarg_1);
                il.Emit(OpCodes.Ldloc, returnValue);
                il.Emit(OpCodes.Call, Members.SetReturnValueOfInvocationContext.MakeGenericMethod(returnType));
            }
        }

        protected  void CopyGenericParameterAttributes(Type[] originalGenericArguments, Type[] newGenericArguments, GenericTypeParameterBuilder[] generaicParameterBuilders)
        {
            for (int index = 0; index < originalGenericArguments.Length; index++)
            {
                var builder = generaicParameterBuilders[index];
                var originalGenericArgument = originalGenericArguments[index];
                builder.SetGenericParameterAttributes(originalGenericArgument.GenericParameterAttributes);

                var interfaceConstraints = new List<Type>();
                foreach (Type constraint in originalGenericArgument.GetGenericParameterConstraints())
                {
                    if (constraint.IsClass)
                    {
                        if (constraint.IsGenericType)
                        {
                            builder.SetBaseTypeConstraint(MakeGenericType(originalGenericArguments, newGenericArguments, constraint));
                        }
                        else
                        {
                            builder.SetBaseTypeConstraint(constraint);
                        }                       
                    }
                    else
                    {
                        if (constraint.IsGenericType)
                        {
                            interfaceConstraints.Add(MakeGenericType(originalGenericArguments, newGenericArguments, constraint));
                        }
                        else
                        {
                            interfaceConstraints.Add(constraint);
                        }
                    }
                }
                if (interfaceConstraints.Count > 0)
                {
                    builder.SetInterfaceConstraints(interfaceConstraints.ToArray());
                }
            }            
        }
        
        private TypeBuilder CreateClosureType(MethodMetadata methodMetadata, Type originalTargetType, out Type targetType, out Type[] parameterTypes, out Type returnType, out Type[] genericMethodArguments)
        {
            var targetMethod = methodMetadata.MethodInfo;
            var parameters = targetMethod.GetParameters();
            var closureType = ModuleBuilder.DefineType($"{targetMethod.DeclaringType.Name}_{targetMethod.Name}Closure", TypeAttributes.Public, typeof(object));
            if (!methodMetadata.IsGenericMethod && !originalTargetType.IsGenericType)
            {
                parameterTypes = targetMethod.GetParameters().Select(it => it.ParameterType).ToArray();
                returnType = targetMethod.ReturnType;
                targetType = originalTargetType;
                genericMethodArguments = Array.Empty<Type>();
            }
            else
            {
                var genericArguments = !methodMetadata.IsGenericMethod
                    ? originalTargetType.GetGenericArguments()
                    : originalTargetType.GetGenericArguments().Concat(targetMethod.GetGenericArguments()).ToArray();
                var genericArgumentNames = genericArguments.Select(it => it.Name).ToArray();
                var genericParameterBuilders = closureType.DefineGenericParameters(genericArgumentNames);
                CopyGenericParameterAttributes(genericArguments, closureType.GetGenericArguments(), genericParameterBuilders);

                genericArguments = closureType.GetGenericArguments();
                parameterTypes = (from parameter in parameters
                                  let type = parameter.ParameterType
                                  select type.IsGenericParameter ? genericArguments.Single(it => it.Name == type.Name) : type)
                                  .ToArray();

                returnType = targetMethod.ReturnType;
                if (returnType.IsGenericParameter)
                {
                    returnType = genericArguments.Single(it => it.Name == targetMethod.ReturnType.Name);
                }

                targetType = methodMetadata.IsGenericMethod
                    ? originalTargetType
                    : originalTargetType.MakeGenericType(originalTargetType.GetGenericArguments().Select(it => genericArguments.Single(it2 => it2.Name == it.Name)).ToArray());

                if (targetMethod.IsGenericMethod)
                {
                    genericMethodArguments = targetMethod.GetGenericArguments().Select(it => genericArguments.Single(it2 => it2.Name == it.Name)).ToArray();
                }
                else
                {
                    genericMethodArguments = Array.Empty<Type>();
                }
            }

            return closureType;
        }

        internal static Type MakeGenericType(Type[] originalGenericArguments, Type[] newGenericArguments, Type genericType)
        {
            return genericType.GetGenericTypeDefinition().MakeGenericType(GetGenericArguments(originalGenericArguments, newGenericArguments, genericType));           
        }

        internal static Type[] GetGenericArguments(Type[] originalGenericArguments, Type[] newGenericArguments, Type genericType)
        {
            var originalArguments = genericType.GetGenericArguments();
            var arguments = new Type[originalArguments.Length];
            for (int index = 0; index < originalArguments.Length; index++)
            {
                var originalArgument = originalArguments[index];
                if (originalArgument.IsGenericType)
                {
                    arguments[index] = MakeGenericType(originalGenericArguments, newGenericArguments, originalArgument);
                    continue;
                }

                if (originalArgument.IsGenericParameter)
                {
                    var position = Array.IndexOf(originalGenericArguments, originalArgument);
                    arguments[index] = newGenericArguments[position];
                    continue;
                }

                arguments[index] = originalArgument;
            }

            return arguments;
        }
    }
}
