using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Text;
using System.Threading.Tasks;

namespace Dora.Interception
{
    public class InterfaceProxyGenerator : InterceptableProxyGeneratorBase
    {
        private readonly MethodAttributes _methodAttributes = MethodAttributes.Public | MethodAttributes.HideBySig | MethodAttributes.NewSlot | MethodAttributes.Virtual | MethodAttributes.Final;
        private TypeBuilder _proxyTypeBuilder;
        private FieldBuilder _targetFieldBuilder;
        private FieldBuilder _interceptorProviderFieldBuilder;
        private bool _isGenericType;
        private Type[] _genericArguments;
        private Type _interface;
        private Type _implementationType;
        private Type _targetType;


        public InterfaceProxyGenerator(IInterceptorRegistrationProvider registrationProvider) : base(registrationProvider)
        {             
        }

        public override Type Generate(Type serviceType, Type implementationType)
        {
            _interface = serviceType;
            _implementationType = implementationType;
            CreateProxyTypeBuilder();
            DefineConstructor();

            if (serviceType.IsGenericTypeDefinition)
            {
                //TODO
                foreach (var targetMethod in implementationType.GetMethods())
                {
                    if (RegistrationProvider.WillIntercept(targetMethod))
                    {
                        DefineInterceptableMethod(new MethodMetadata(targetMethod));
                    }
                    else
                    {
                        DefineNonInterceptableMethod(new MethodMetadata(targetMethod));
                    }
                }
            }
            else
            {
                var mapping = _implementationType.GetInterfaceMap(_interface);
                foreach (var targetMethod in mapping.TargetMethods)
                {
                    if (RegistrationProvider.WillIntercept(targetMethod))
                    {
                        DefineInterceptableMethod(new MethodMetadata(targetMethod));
                    }
                    else
                    {
                        DefineNonInterceptableMethod(new MethodMetadata(targetMethod));
                    }
                }
            }

            return _proxyTypeBuilder.CreateTypeInfo();
        }

        private void  CreateProxyTypeBuilder()
        {
            _isGenericType = _implementationType.IsGenericTypeDefinition;
            _proxyTypeBuilder = ModuleBuilder.DefineType($"{_implementationType}Proxy", TypeAttributes.Public, typeof(object), new Type[] { _interface });           

            if (_isGenericType)
            {
                var genericArguments = _implementationType.GetGenericArguments();
                var genericParameterNames = genericArguments.Select(it => it.Name).ToArray();
                var builders = _proxyTypeBuilder.DefineGenericParameters(genericParameterNames);
                CopyGenericParameterAttributes(genericArguments, builders);
                _genericArguments = _proxyTypeBuilder.GetGenericArguments();
            }
            else
            {
                _genericArguments = Array.Empty<Type>();
            }

            _targetType = _isGenericType ? _implementationType.MakeGenericType(_genericArguments) : _implementationType;
            _targetFieldBuilder = _proxyTypeBuilder.DefineField("_target", _targetType, FieldAttributes.Private);
            _interceptorProviderFieldBuilder = _proxyTypeBuilder.DefineField("_interceptorProvider", typeof(IInterceptorProvider), FieldAttributes.Private);
        }

        private void DefineConstructor()
        {
            var constructor = _proxyTypeBuilder.DefineConstructor(MethodAttributes.Public, CallingConventions.Standard, new Type[] { typeof(IServiceProvider), typeof(IInterceptorProvider) });
            var il = constructor.GetILGenerator();

            il.Emit(OpCodes.Ldarg_0);
            il.Emit(OpCodes.Call, Members.ConstructorOfObject);

            //_target = ActivatorUtilities.CreateInstance<T>();
            il.Emit(OpCodes.Ldarg_0); //proxy
            il.Emit(OpCodes.Ldarg_1);//serviceProvider
            il.Emit(OpCodes.Call, Members.CreateInstanceOfActivatorUtilities.MakeGenericMethod(_targetType));
            il.Emit(OpCodes.Stfld, _targetFieldBuilder);

            //_interceptorProvider = interceptorProvider;
            il.Emit(OpCodes.Ldarg_0); //proxy
            il.Emit(OpCodes.Ldarg_2); //interceptorProvider
            il.Emit(OpCodes.Stfld, _interceptorProviderFieldBuilder);

            il.Emit(OpCodes.Ret);
        }

        void DefineNonInterceptableMethod(MethodMetadata methodMetadata)
        {
            var targetMethod = methodMetadata.MethodInfo;
            var methodBuilder = CreateMethodBuilder(methodMetadata, out var parameterTypes, out var returnType);

            var il = methodBuilder.GetILGenerator();
            il.Emit(OpCodes.Ldarg_0);
            il.Emit(OpCodes.Ldfld, _targetFieldBuilder);
            for (int index = 0; index < parameterTypes.Length; index++)
            {
                il.EmitLdArgs(index + 1);
            }

            if (targetMethod.DeclaringType == _targetType)
            {
                il.Emit(OpCodes.Call, targetMethod);
            }
            else
            {
                il.Emit(OpCodes.Callvirt, targetMethod);
            }
            il.Emit(OpCodes.Ret);
        }

        private void DefineInterceptableMethod(MethodMetadata methodMetadata)
        {
            var closureTypeBuilder = DefineClosureType(methodMetadata, out var invokeMethod, out var closureConstructor, out var constructorParameterTypes);
            Type closureType = closureTypeBuilder.CreateTypeInfo();
            var methodBuilder = CreateMethodBuilder(methodMetadata, out var parameterTypes, out var returnType);
            if (closureTypeBuilder.IsGenericType)
            {
                var genericArguments = methodBuilder.IsGenericMethod
                    ? _genericArguments.Concat(methodBuilder.GetGenericArguments()).ToArray()
                    : _genericArguments;
                var genericType = closureTypeBuilder.MakeGenericType(genericArguments);
                closureConstructor = TypeBuilder.GetConstructor(genericType, closureConstructor);
                invokeMethod = TypeBuilder.GetMethod(genericType, invokeMethod);
                closureType = closureType.MakeGenericType(genericArguments);
            }

            var il = methodBuilder.GetILGenerator();

            //var method = MethodBase.GetMethodFromHandleOfMethodBase(..);
            var method = il.DeclareLocal(typeof(MethodInfo));
            if (_isGenericType)
            {
                var closeType = _implementationType.MakeGenericType(_genericArguments);
                il.Emit(OpCodes.Ldtoken, methodMetadata.MethodInfo);
                il.Emit(OpCodes.Ldtoken, closeType);
                il.Emit(OpCodes.Call, Members.GetMethodFromHandle2OfMethodBase);
            }
            else
            {
                il.Emit(OpCodes.Ldtoken, methodMetadata.MethodInfo);
                il.Emit(OpCodes.Call, Members.GetMethodFromHandleOfMethodBase);
            }
           
            il.Emit(OpCodes.Stloc, method);

            //var interceptor = _interceptorProvider.GetInterceptor(method);
            var interceptor = il.DeclareLocal(typeof(IInterceptor));
            il.Emit(OpCodes.Ldarg_0);
            il.Emit(OpCodes.Ldfld, _interceptorProviderFieldBuilder);
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
            il.Emit(OpCodes.Ldfld, _targetFieldBuilder);
            il.Emit(OpCodes.Ldloc, method);
            il.Emit(OpCodes.Ldnull);
            il.Emit(OpCodes.Newobj, Members.ConstructorOfInvocationContext);
            il.Emit(OpCodes.Stloc, invocationContext);

            il.Emit(OpCodes.Br_S, contextCreated);

            il.MarkLabel(captureArgument);

            //var arguments = new object[]{arg1, arg2,...}
            var arguments = il.DeclareLocal(typeof(object[]));
            il.Emit(OpCodes.Ldc_I4, parameterTypes.Length);
            il.Emit(OpCodes.Newarr, typeof(object));
            for (int index = 0; index < parameterTypes.Length; index++)
            {
                il.Emit(OpCodes.Dup);
                il.Emit(OpCodes.Ldc_I4, index);
                il.EmitLdArgs(index + 1);
                il.EmitTryBox(parameterTypes[index]);
                il.Emit(OpCodes.Stelem_Ref);
            }
            il.Emit(OpCodes.Stloc, arguments);

            // var invocationContext = new InvocationContext(target, method, arguments)
            il.Emit(OpCodes.Ldarg_0);
            il.Emit(OpCodes.Ldfld, _targetFieldBuilder);
            il.Emit(OpCodes.Ldloc, method);
            il.Emit(OpCodes.Ldloc, arguments);
            il.Emit(OpCodes.Newobj, Members.ConstructorOfInvocationContext);
            il.Emit(OpCodes.Stloc, invocationContext);

            il.MarkLabel(contextCreated);


            //var closure = new Closure(target, arg1, arg2, ..., arguments);
            var closure = il.DeclareLocal(closureType);
            il.Emit(OpCodes.Ldarg_0);
            il.Emit(OpCodes.Ldfld, _targetFieldBuilder);
            for (int index = 0; index < parameterTypes.Length; index++)
            {
                il.EmitLdArgs(index + 1);
            }
            il.Emit(OpCodes.Ldloc, arguments);
            il.Emit(OpCodes.Newobj, closureConstructor);
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

        private TypeBuilder DefineClosureType(MethodMetadata methodMetadata, out MethodInfo invokeMethod, out ConstructorInfo constructor, out Type[] constructorParameterTypes)
        {
            var closureType = CreateClosureType(methodMetadata, out var targetType, out var parameterTypes, out var returnType, out var genericMethodArguments);
            var targetMethod = methodMetadata.MethodInfo;
            if (targetMethod.DeclaringType.IsGenericTypeDefinition)
            {
                targetMethod = GenericTypeUtility.GetMethodInfo(targetType, targetMethod);
            }

            var fields = new FieldBuilder[parameterTypes.Length + 2];
            constructorParameterTypes = new Type[parameterTypes.Length + 2];
            fields[0] = closureType.DefineField("_target", targetType, FieldAttributes.Private | FieldAttributes.InitOnly);
            constructorParameterTypes[0] = targetType;
            var parameters = targetMethod.GetParameters();
            for (int index = 0; index < parameterTypes.Length; index++)
            {
                var parameter = parameters[index];
                fields[index + 1] = closureType.DefineField($"_{parameter.Name}", parameterTypes[index], FieldAttributes.Private);
                constructorParameterTypes[index + 1] = parameterTypes[index];
            }
            fields[fields.Length - 1] = closureType.DefineField("_arguments", typeof(object[]), FieldAttributes.Private);
            constructorParameterTypes[fields.Length - 1] = typeof(object[]);

            var constructorBuilder = closureType.DefineConstructor(MethodAttributes.Public, CallingConventions.Standard, constructorParameterTypes);
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

            var invokeMethodBuilder = closureType.DefineMethod("InvokeAsync", MethodAttributes.Public | MethodAttributes.HideBySig, typeof(Task), new Type[] { typeof(InvocationContext) });
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
            return closureType
                //.CreateTypeInfo()
                ;

            void SetReturnValue()
            {
                il.Emit(OpCodes.Ldarg_1);
                il.Emit(OpCodes.Ldloc, returnValue);
                il.Emit(OpCodes.Call, Members.SetReturnValueOfInvocationContext.MakeGenericMethod(returnType));
            }
        }

        private TypeBuilder CreateClosureType(MethodMetadata methodMetadata, out Type targetType, out Type[] parameterTypes, out Type returnType, out Type[] genericMethodArguments)
        {
            var targetMethod = methodMetadata.MethodInfo;
            var parameters = targetMethod.GetParameters();
            var closureType = ModuleBuilder.DefineType($"{targetMethod.DeclaringType.Name}_{targetMethod.Name}Closure", TypeAttributes.Public, typeof(object));
            if (!methodMetadata.IsGenericMethod && !_isGenericType)
            {
                parameterTypes = targetMethod.GetParameters().Select(it => it.ParameterType).ToArray();
                returnType = targetMethod.ReturnType;
                targetType = _implementationType;
                genericMethodArguments = Array.Empty<Type>();
            }
            else
            {
                var genericArguments = !methodMetadata.IsGenericMethod
                    ? _implementationType.GetGenericArguments()
                    : _implementationType.GetGenericArguments().Concat( targetMethod.GetGenericArguments()).ToArray();
                var genericArgumentNames = genericArguments.Select(it => it.Name).ToArray();
                var genericParameterBuilders = closureType.DefineGenericParameters(genericArgumentNames);
                CopyGenericParameterAttributes(genericArguments, genericParameterBuilders);
              
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
                    ? _implementationType
                    : _implementationType.MakeGenericType(_implementationType.GetGenericArguments().Select(it => genericArguments.Single(it2 => it2.Name == it.Name)).ToArray());

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

        private MethodBuilder CreateMethodBuilder(MethodMetadata methodMetadata, out Type[] parameterTypes, out Type returnType)
        {
            var targetMethod = methodMetadata.MethodInfo;
            var parameters = targetMethod.GetParameters();
            var originalReturnType = targetMethod.ReturnType;

            if (!methodMetadata.IsGenericMethod)
            {
                returnType = originalReturnType.IsGenericParameter ? _genericArguments.Single(it => it.Name == originalReturnType.Name) : originalReturnType;
                parameterTypes = parameters.Select(it => it.ParameterType).ToArray();
                return _proxyTypeBuilder.DefineMethod(targetMethod.Name, _methodAttributes, returnType, parameterTypes);
            }

            var methodBuilder = _proxyTypeBuilder.DefineMethod(targetMethod.Name, _methodAttributes);
            var genericArguments = targetMethod.GetGenericArguments();
            var genericArgumentNames = genericArguments.Select(it => it.Name).ToArray();
            var generaicParameterBuilders = methodBuilder.DefineGenericParameters(genericArgumentNames);
            CopyGenericParameterAttributes(genericArguments, generaicParameterBuilders);

            genericArguments = _genericArguments.Concat(methodBuilder.GetGenericArguments()).ToArray();

            parameterTypes = (from parameter in parameters
                              let type = parameter.ParameterType
                              select type.IsGenericParameter ? genericArguments.Single(it => it.Name == type.Name) : type)
                              .ToArray();
            methodBuilder.SetParameters(parameterTypes);
            returnType = originalReturnType.IsGenericParameter ? genericArguments.Single(it => it.Name == targetMethod.ReturnType.Name) : originalReturnType;
            methodBuilder.SetReturnType(returnType);
            return methodBuilder;
        }

        private static void CopyGenericParameterAttributes(Type[] originalGenericArguments, GenericTypeParameterBuilder[] generaicParameterBuilders)
        {
            for (int index = 0; index < originalGenericArguments.Length; index++)
            {
                var builder = generaicParameterBuilders[index];
                var genericArgument = originalGenericArguments[index];
                builder.SetGenericParameterAttributes(genericArgument.GenericParameterAttributes);

                var interfaceConstraints = new List<Type>();
                foreach (Type constraint in genericArgument.GetGenericParameterConstraints())
                {
                    if (constraint.IsClass)
                    {
                        builder.SetBaseTypeConstraint(constraint);
                    }
                    else
                    {
                        interfaceConstraints.Add(constraint);
                    }
                }
                if (interfaceConstraints.Count > 0)
                {
                    builder.SetInterfaceConstraints(interfaceConstraints.ToArray());
                }
            }
        }
    }
}
