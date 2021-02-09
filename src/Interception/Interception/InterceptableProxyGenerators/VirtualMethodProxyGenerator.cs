using System;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Threading.Tasks;

namespace Dora.Interception
{
    public class VirtualMethodProxyGenerator : InterceptableProxyGeneratorBase
    {
        private Type _implementationType;
        private TypeBuilder _proxyTypeBuilder;
        private FieldBuilder _interceptorProviderFieldBuilder;
        private bool _isGenericType;
        private Type[] _genericArguments;

        public VirtualMethodProxyGenerator(IInterceptorRegistrationProvider registrationProvider) : base(registrationProvider)
        {
        }

        public override Type Generate(Type serviceType, Type implementationType)
        {
            if (serviceType.IsInterface || implementationType.IsSealed)
            {
                return null;
            }

            _implementationType = implementationType;
            CreateProxyTypeBuilder();
            foreach (var constructor in implementationType.GetConstructors())
            {
                DefineConstructor(constructor);
            }
            foreach (var method in implementationType.GetMethods(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance))
            {
                if (method.IsSpecialName || !method.IsVirtual || !RegistrationProvider.WillIntercept(method))
                {
                    continue;
                }
                DefineInterceptableMethod(new MethodMetadata(method));
            }

            foreach (var property in implementationType.GetProperties(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance))
            {
                DefineProperty(property);
            }
            return _proxyTypeBuilder.CreateTypeInfo();
        }

        private void DefineConstructor(ConstructorInfo orginalConstructor)
        {
            var parameterTypes = orginalConstructor.GetParameters().Select(it => it.ParameterType).ToArray();
            if (parameterTypes.Any(it => it.IsGenericParameter))
            {
                parameterTypes = parameterTypes.Select(it => it.IsGenericParameter ? _genericArguments.Single(it2 => it2.Name == it.Name) : it).ToArray();
            }
            var constructor = _proxyTypeBuilder.DefineConstructor(orginalConstructor.Attributes, CallingConventions.Standard, parameterTypes.Concat(new Type[] { typeof(IInterceptorProvider) }).ToArray());
            var il = constructor.GetILGenerator();

            il.Emit(OpCodes.Ldarg_0);
            for (int index = 1; index < parameterTypes.Length + 1; index++)
            {
                il.EmitLdArgs(index);
            }
            il.Emit(OpCodes.Call, orginalConstructor);

            il.Emit(OpCodes.Ldarg_0);
            il.EmitLdArgs(parameterTypes.Length + 1);
            il.Emit(OpCodes.Stfld, _interceptorProviderFieldBuilder);
            il.Emit(OpCodes.Ret);
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
            il.Emit(OpCodes.Ldarg_0); //this
            il.Emit(OpCodes.Ldloc, method); //method
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
            il.Emit(OpCodes.Ldloc, method);
            il.Emit(OpCodes.Ldloc, arguments);
            il.Emit(OpCodes.Newobj, Members.ConstructorOfInvocationContext);
            il.Emit(OpCodes.Stloc, invocationContext);

            il.MarkLabel(contextCreated);

            //var closure = new Closure(target, arg1, arg2, ..., arguments);
            var closure = il.DeclareLocal(closureType);
            il.Emit(OpCodes.Ldarg_0);
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

        private PropertyBuilder DefineProperty(PropertyInfo property)
        {
            PropertyBuilder propertyBuilder = null;
            var getMethod = DefinePropertyMethod(property.GetMethod);
            var setMethod = DefinePropertyMethod(property.SetMethod);
            if (getMethod != null || setMethod != null)            
            {
                var parameterTypes = property.GetIndexParameters().Select(it => it.ParameterType).ToArray();
                propertyBuilder = _proxyTypeBuilder.DefineProperty(property.Name, property.Attributes, property.PropertyType, parameterTypes);
                if (getMethod != null)
                {
                    propertyBuilder.SetGetMethod(getMethod);
                }
                if (setMethod != null)
                {
                    propertyBuilder.SetSetMethod(setMethod);
                }
            }
            return propertyBuilder;

            MethodBuilder DefinePropertyMethod(MethodInfo methodInfo)
            {
                if (methodInfo != null && methodInfo.IsVirtual && RegistrationProvider.WillIntercept(methodInfo))
                {
                    var attributes = GetMethodAttributes(methodInfo);
                    var parameterTypes = property.GetIndexParameters().Select(it => it.ParameterType).ToArray();
                    return DefineInterceptableMethod(new MethodMetadata(methodInfo));
                }
                return null;
            }
        }

        private void CreateProxyTypeBuilder()
        {
            _isGenericType = _implementationType.IsGenericTypeDefinition;
            _proxyTypeBuilder = ModuleBuilder.DefineType($"{_implementationType}Proxy_{Guid.NewGuid().ToString().Replace("-", "")}", TypeAttributes.Public, _implementationType);

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

            _interceptorProviderFieldBuilder = _proxyTypeBuilder.DefineField("_interceptorProvider", typeof(IInterceptorProvider), FieldAttributes.Private| FieldAttributes.InitOnly);
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
                return _proxyTypeBuilder.DefineMethod(targetMethod.Name, GetMethodAttributes(targetMethod), returnType, parameterTypes);
            }

            var methodBuilder = _proxyTypeBuilder.DefineMethod(targetMethod.Name, GetMethodAttributes(targetMethod));
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

        private MethodAttributes GetMethodAttributes(MethodInfo methodInfo)
        {
            var attributes = MethodAttributes.HideBySig | MethodAttributes.Virtual | MethodAttributes.Final;
            if (methodInfo.IsPublic)
            {
                return MethodAttributes.Public | attributes;
            }
            if (methodInfo.IsFamily)
            {
                return MethodAttributes.Family | attributes;
            }
            if (methodInfo.IsFamilyAndAssembly)
            {
                return MethodAttributes.FamANDAssem | attributes;
            }
            if (methodInfo.IsFamilyOrAssembly)
            {
                return MethodAttributes.FamORAssem | attributes;
            }

            throw new Exception(); 
        }
    }
}
