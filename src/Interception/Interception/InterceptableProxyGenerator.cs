﻿using System;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Threading.Tasks;

namespace Dora.Interception
{
    public class InterceptableProxyGenerator : IInterceptableProxyGenerator
    {
        private readonly ModuleBuilder _moduleBuilder;
        private readonly IInterceptorRegistrationProvider _registrationProvider;
        public InterceptableProxyGenerator(IInterceptorRegistrationProvider registrationProvider)
        {
            _registrationProvider = registrationProvider ?? throw new ArgumentNullException(nameof(registrationProvider));
            var assemblyBuilder = AssemblyBuilder.DefineDynamicAssembly(new AssemblyName("Dora.Interception.InterceptableProxies"), AssemblyBuilderAccess.RunAndCollect);
            _moduleBuilder = assemblyBuilder.DefineDynamicModule("Dora.Interception.InterceptableProxies.dll");
        }
        public Type Generate(Type serviceType, Type implementationType)
        {
            if (serviceType.IsInterface)
            {
                return GenerateForInterface(serviceType, implementationType);
            }
            throw new NotImplementedException();
        }

        private Type GenerateForInterface(Type @interface, Type implementationType)
        {
            var attributes = MethodAttributes.Public | MethodAttributes.HideBySig | MethodAttributes.NewSlot | MethodAttributes.Virtual | MethodAttributes.Final;
            var proxyType = _moduleBuilder.DefineType($"{implementationType}Proxy", TypeAttributes.Public, typeof(object), new Type[] { @interface });
            var target = proxyType.DefineField("_target", implementationType, FieldAttributes.Private);
            var interceptorProvider = proxyType.DefineField("_interceptorProvider", typeof(IInterceptorProvider), FieldAttributes.Private);
            DefineConstructor();
            var mapping = implementationType.GetInterfaceMap(@interface);
            foreach (var targetMethod in mapping.TargetMethods)
            {
                if (_registrationProvider.WillIntercept(targetMethod))
                {
                    DefineInterceptableMethod(new MethodMetadata(targetMethod));
                }
                else
                {
                    DefineNonInterceptableMethod(new MethodMetadata(targetMethod));
                }
            }

            void DefineConstructor()
            {
                var constructor = proxyType.DefineConstructor(MethodAttributes.Public, CallingConventions.Standard, new Type[] { typeof(IServiceProvider), typeof(IInterceptorProvider) });
                var il = constructor.GetILGenerator();

                il.Emit(OpCodes.Ldarg_0);
                il.Emit(OpCodes.Call, Members.ConstructorOfObject);

                //_target = ActivatorUtilities.CreateInstance<T>();
                il.Emit(OpCodes.Ldarg_0); //proxy
                il.Emit(OpCodes.Ldarg_1);//serviceProvider
                il.Emit(OpCodes.Call, Members.CreateInstanceOfActivatorUtilities.MakeGenericMethod(implementationType));
                il.Emit(OpCodes.Stfld, target);

                //_interceptorProvider = interceptorProvider;
                il.Emit(OpCodes.Ldarg_0); //proxy
                il.Emit(OpCodes.Ldarg_2); //interceptorProvider
                il.Emit(OpCodes.Stfld, interceptorProvider);

                il.Emit(OpCodes.Ret);
            }

            return proxyType.CreateTypeInfo();

            void DefineInterceptableMethod(MethodMetadata methodMetadata)
            {
                var targetMethod = methodMetadata.MethodInfo;
                var closureType = DefineClosureType(methodMetadata, out var invokeMethod, out var constructor);
                var parameters = targetMethod.GetParameters();
                var parameterTypes = parameters.Select(it => it.ParameterType).ToArray();
                var methodBuilder = proxyType.DefineMethod(targetMethod.Name, attributes, targetMethod.ReturnType, parameterTypes);

                var il = methodBuilder.GetILGenerator();

                //var method = MethodBase.GetMethodFromHandleOfMethodBase(..);
                var method = il.DeclareLocal(typeof(MethodInfo));
                il.Emit(OpCodes.Ldtoken, targetMethod);
                il.Emit(OpCodes.Call, Members.GetMethodFromHandleOfMethodBase);
                il.Emit(OpCodes.Stloc, method);

                //var interceptor = _interceptorProvider.GetInterceptor(method);
                var interceptor = il.DeclareLocal(typeof(IInterceptor));
                il.Emit(OpCodes.Ldarg_0);
                il.Emit(OpCodes.Ldfld, interceptorProvider);
                il.Emit(OpCodes.Ldloc, method);
                il.Emit(OpCodes.Callvirt, Members.GetInterceptorOfInterceptorProvider);
                il.Emit(OpCodes.Stloc, interceptor);

                //new InvocationContext(target, method, null)
                var invocationContext = il.DeclareLocal(typeof(InvocationContext));
                var captureArgument = il.DefineLabel();
                var contextCreated = il.DefineLabel();
                il.Emit(OpCodes.Ldloc, interceptor);
                il.Emit(OpCodes.Callvirt, Members.GetCaptureArgumentsOfInterceptor);
                il.Emit(OpCodes.Brtrue_S, captureArgument);
                il.Emit(OpCodes.Ldarg_0);
                il.Emit(OpCodes.Ldfld, target);
                il.Emit(OpCodes.Ldloc, method);
                il.Emit(OpCodes.Ldnull);
                il.Emit(OpCodes.Newobj, Members.ConstructorOfInvocationContext);
                il.Emit(OpCodes.Stloc, invocationContext);

                il.Emit(OpCodes.Br_S, contextCreated);

                il.MarkLabel(captureArgument);

                //var arguments = new object[]{arg1, arg2,...}
                var arguments = il.DeclareLocal(typeof(object[]));
                il.Emit(OpCodes.Ldc_I4, parameters.Length);
                il.Emit(OpCodes.Newarr, typeof(object));
                for (int index = 0; index < parameters.Length; index++)
                {
                    var parameter = parameters[index];
                    il.Emit(OpCodes.Dup);
                    il.Emit(OpCodes.Ldc_I4, index);
                    il.EmitLdArgs(index + 1);
                    if (parameter.ParameterType.IsValueType)
                    {
                        il.Emit(OpCodes.Box, parameter.ParameterType);
                    }
                    il.Emit(OpCodes.Stelem_Ref);
                }
                il.Emit(OpCodes.Stloc, arguments);

                // var invocationContext = new InvocationContext(target, method, arguments)
                il.Emit(OpCodes.Ldarg_0);
                il.Emit(OpCodes.Ldfld, target);
                il.Emit(OpCodes.Ldloc, method);
                il.Emit(OpCodes.Ldloc, arguments);
                il.Emit(OpCodes.Newobj, Members.ConstructorOfInvocationContext);
                il.Emit(OpCodes.Stloc, invocationContext);

                il.MarkLabel(contextCreated);

                //var closure = new Closure(target, arg1, arg2, ..., arguments);
                var closure = il.DeclareLocal(closureType);
                il.Emit(OpCodes.Ldarg_0);
                il.Emit(OpCodes.Ldfld, target);
                for (int index = 0; index < parameters.Length; index++)
                {
                    il.EmitLdArgs(index + 1);
                }
                il.Emit(OpCodes.Ldloc, arguments);
                il.Emit(OpCodes.Newobj, constructor);
                il.Emit(OpCodes.Stloc, closure);

                //var next = closure.InvokeAsync;
                var next = il.DeclareLocal(typeof(InvokerDelegate));
                il.Emit(OpCodes.Ldloc, closure);
                il.Emit(OpCodes.Ldftn, invokeMethod);
                il.Emit(OpCodes.Newobj, Members.ConstructorOfInvokerDelegate);
                il.Emit(OpCodes.Stloc, next);

                //var task = interceptor.Delegate(next)(invocationContext);
                var task = il.DeclareLocal(typeof(Task));
                il.Emit(OpCodes.Ldloc, interceptor);
                il.Emit(OpCodes.Ldloc, next);
                il.Emit(OpCodes.Ldloc, invocationContext);
                il.Emit(OpCodes.Call, Members.ExecuteInterceptorOfProxyGeneratorHelper);
                il.Emit(OpCodes.Stloc, task);

                var returnKind = methodMetadata.ReturnKind;
                var returnType = targetMethod.ReturnType;
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

            void DefineNonInterceptableMethod(MethodMetadata methodMetadata)
            {
                var targetMethod = methodMetadata.MethodInfo;
                var closureType = DefineClosureType(methodMetadata, out var invokeMethod, out var constructor);
                var parameters = targetMethod.GetParameters();
                var parameterTypes = parameters.Select(it => it.ParameterType).ToArray();
                var methodBuilder = proxyType.DefineMethod(targetMethod.Name, attributes, targetMethod.ReturnType, parameterTypes);

                var il = methodBuilder.GetILGenerator();
                il.Emit(OpCodes.Ldarg_0);
                il.Emit(OpCodes.Ldfld, target);
                for (int index = 0; index < parameters.Length; index++)
                {
                    il.EmitLdArgs(index + 1);
                }

                if (targetMethod.DeclaringType == implementationType)
                {
                    il.Emit(OpCodes.Call, targetMethod);
                }
                else
                {
                    il.Emit(OpCodes.Callvirt, targetMethod);
                }
                il.Emit(OpCodes.Ret);
            }

            Type DefineClosureType(MethodMetadata methodMetadata, out MethodInfo invokeMethod, out ConstructorInfo constructor)
            {
                var targetMethod = methodMetadata.MethodInfo;
                var parameters = targetMethod.GetParameters();
                var closureType = _moduleBuilder.DefineType($"{targetMethod.DeclaringType.Name}_{targetMethod.Name}Closure", TypeAttributes.Public, typeof(object));
                var fields = new FieldBuilder[parameters.Length + 2];
                var constructorParameterTypes = new Type[parameters.Length + 2];
                fields[0] = closureType.DefineField("_target", implementationType, FieldAttributes.Private | FieldAttributes.InitOnly);
                constructorParameterTypes[0] = implementationType;
                for (int index = 0; index < parameters.Length; index++)
                {
                    var parameter = parameters[index];
                    fields[index + 1] = closureType.DefineField($"_{parameter.Name}", parameter.ParameterType, FieldAttributes.Private);
                    constructorParameterTypes[index + 1] = parameter.ParameterType;
                }
                fields[fields.Length - 1] = closureType.DefineField("_arguments", typeof(object[]), FieldAttributes.Private);
                constructorParameterTypes[fields.Length - 1] = typeof(object[]);

                var constructorBuilder = closureType.DefineConstructor(MethodAttributes.Public, CallingConventions.Standard, constructorParameterTypes);
                var il = constructorBuilder.GetILGenerator();

                il.EmitWriteLine("ss");

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

                var invokeMethodBuilder = closureType.DefineMethod("InvokeAsync", MethodAttributes.Public| MethodAttributes.HideBySig, typeof(Task), new Type[] { typeof(InvocationContext) });
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
                    var parameterType = parameters[index].ParameterType;
                    if (parameterType.IsValueType)
                    {
                        il.Emit(OpCodes.Unbox, parameterType);
                    }
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
                if (implementationType == targetMethod.DeclaringType)
                {
                    il.Emit(OpCodes.Call, targetMethod);
                }
                else
                {
                    il.Emit(OpCodes.Callvirt, targetMethod);
                }
                LocalBuilder returnValue = null;
                var returnKind = methodMetadata.ReturnKind;
                var returnType = targetMethod.ReturnType;
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
                return closureType.CreateTypeInfo();

                void SetReturnValue()
                {
                    il.Emit(OpCodes.Ldarg_1);
                    il.Emit(OpCodes.Ldloc, returnValue);
                    il.Emit(OpCodes.Call, Members.SetReturnValueOfInvocationContext.MakeGenericMethod(returnType));
                }
            }
        }
    }
}