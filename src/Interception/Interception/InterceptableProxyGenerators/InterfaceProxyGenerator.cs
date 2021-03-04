using System;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
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
            if (!serviceType.IsInterface)
            {
                return null;
            }
            _interface = serviceType;
            _implementationType = implementationType;
            CreateProxyTypeBuilder();
            DefineConstructor();

            var methodMap = InterfaceMapper.GetMethodMap(serviceType, implementationType);
            foreach (var method in methodMap.Values)
            {
                if (method.IsSpecialName)
                {
                    continue;
                }
                var metadata = new MethodMetadata(method);
                _ = RegistrationProvider.WillIntercept(method) ? DefineInterceptableMethod(metadata) : DefineNonInterceptableMethod(metadata);
            }

            var properties = _implementationType.GetProperties(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
            foreach (var property in properties)
            {
                var method = property.GetMethod;
                if (method != null && methodMap.Values.Contains(method))
                {
                    DefineProperty(property);
                    continue;
                }

                method = property.SetMethod;
                if (method != null && methodMap.Values.Contains(method))
                {
                    DefineProperty(property);
                }
            }

            var events = _implementationType.GetEvents(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
            foreach (var @event in events)
            {
                var method = @event.AddMethod;
                if (method != null && methodMap.Values.Contains(method))
                {
                    DefineEvent(@event);
                    continue;
                }

                method = @event.RemoveMethod;
                if (method != null && methodMap.Values.Contains(method))
                {
                    DefineEvent(@event);
                }
            }
            return _proxyTypeBuilder.CreateTypeInfo();
        }

        private void  CreateProxyTypeBuilder()
        {
            _isGenericType = _implementationType.IsGenericTypeDefinition;
            _proxyTypeBuilder = ModuleBuilder.DefineType($"{_implementationType}Proxy_{Guid.NewGuid().ToString().Replace("-", "")}", TypeAttributes.Public, typeof(object), new Type[] { _interface });           

            if (_isGenericType)
            {
                var genericArguments = _implementationType.GetGenericArguments();
                var genericParameterNames = genericArguments.Select(it => it.Name).ToArray();
                var builders = _proxyTypeBuilder.DefineGenericParameters(genericParameterNames);
                CopyGenericParameterAttributes(genericArguments, _proxyTypeBuilder.GetGenericArguments(), builders);
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

        MethodBuilder DefineNonInterceptableMethod(MethodMetadata methodMetadata)
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
            return methodBuilder;
        }

        private MethodBuilder DefineInterceptableMethod(MethodMetadata methodMetadata)
        {
            var closureTypeBuilder = DefineClosureType(methodMetadata, _implementationType, out var invokeMethod, out var closureConstructor, out var constructorParameterTypes);
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

            var targetMethod = methodMetadata.MethodInfo;
            if (targetMethod.IsGenericMethodDefinition)
            {
                targetMethod = targetMethod.MakeGenericMethod(methodBuilder.GetGenericArguments());
            }

            var il = methodBuilder.GetILGenerator();

            //var method = MethodBase.GetMethodFromHandleOfMethodBase(..);
            var method = il.DeclareLocal(typeof(MethodInfo));
           
            if (_isGenericType)
            {
                var closeType = _implementationType.MakeGenericType(_genericArguments);
                il.Emit(OpCodes.Ldtoken, targetMethod);
                il.Emit(OpCodes.Ldtoken, closeType);
                il.Emit(OpCodes.Call, Members.GetMethodFromHandle2OfMethodBase);
            }
            else
            {
                il.Emit(OpCodes.Ldtoken, targetMethod);
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
            EmitReturnValueExtractionCode(il, methodMetadata.ReturnKind, returnType, invocationContext, task);
            return methodBuilder;
        }       

        private void DefineProperty(PropertyInfo property)
        {
            var parameterTypes = property.GetIndexParameters().Select(it => it.ParameterType).ToArray();
            var propertyBuilder = _proxyTypeBuilder.DefineProperty(property.Name, property.Attributes, property.PropertyType, parameterTypes);
            var getMethod = property.GetMethod;
            if (null != getMethod)
            {
                var metadata = new MethodMetadata(getMethod);
                var getMethodBuilder = RegistrationProvider.WillIntercept(getMethod)
                    ? DefineInterceptableMethod(metadata)
                    : DefineNonInterceptableMethod(metadata);
                propertyBuilder.SetGetMethod(getMethodBuilder);
            }
            var setMethod = property.SetMethod;
            if (null != setMethod)
            {
                var metadata = new MethodMetadata(setMethod);
                var setMethodBuilder = RegistrationProvider.WillIntercept(setMethod)
                      ? DefineInterceptableMethod(metadata)
                      : DefineNonInterceptableMethod(metadata);
                propertyBuilder.SetGetMethod(setMethodBuilder);
            }
        }
        private void DefineEvent(EventInfo eventInfo)
        {
            var eventBuilder = _proxyTypeBuilder.DefineEvent(eventInfo.Name, eventInfo.Attributes, eventInfo.EventHandlerType);
            eventBuilder.SetAddOnMethod(DefineNonInterceptableMethod(new MethodMetadata(eventInfo.AddMethod)));
            eventBuilder.SetRemoveOnMethod(DefineNonInterceptableMethod(new MethodMetadata(eventInfo.RemoveMethod)));
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
            CopyGenericParameterAttributes(genericArguments, methodBuilder.GetGenericArguments(), generaicParameterBuilders);

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
    }
}
