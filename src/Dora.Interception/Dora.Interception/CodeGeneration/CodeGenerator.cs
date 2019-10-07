using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Threading.Tasks;

namespace Dora.Interception
{
    /// <summary>
    /// Default implementation of <see cref="ICodeGenerator"/>
    /// </summary>
    public class CodeGenerator : ICodeGenerator
    {

        #region Fields
        private Type _targetType;
        private Type _interfaceOrBaseType;
        private IInterceptorRegistry _interceptors;
        private ModuleBuilder _moduleBuilder;
        private TypeBuilder _typeBuilder;
        private FieldBuilder _targetField;
        private FieldBuilder _interceptorsField;
        private readonly MethodInfo _methodOfGetInterceptor = ReflectionUtility.GetMethod<IInterceptorRegistry>(_ => _.GetInterceptor(null));
        private readonly MethodInfo _getInterceptorsMethod4Interface = ReflectionUtility.GetMethod<IInterceptorResolver>(_ => _.GetInterceptors(null, null));
        private readonly MethodInfo _getInterceptorsMethod4Class = ReflectionUtility.GetMethod<IInterceptorResolver>(_ => _.GetInterceptors(null));
        #endregion

        #region Public methods
        /// <summary>
        /// Generates interceptable proxy class.
        /// </summary>
        /// <param name="context">The <see cref="CodeGenerationContext"/> representing code generation based execution context.</param>
        /// <returns>The generated interceptable proxy class</returns>
        public Type GenerateInterceptableProxyClass(CodeGenerationContext context)
        {
            _interfaceOrBaseType = context.InterfaceOrBaseType;
            _targetType = context.TargetType;
            _interceptors = context.Interceptors;

            var assemblyName = new AssemblyName($"AssemblyFor{_interfaceOrBaseType.Name}{GenerateSurfix()}");
            var assemblyBuilder = AssemblyBuilder.DefineDynamicAssembly(assemblyName, AssemblyBuilderAccess.Run);
            _moduleBuilder = assemblyBuilder.DefineDynamicModule($"{assemblyName}.dll");

            if (_interfaceOrBaseType.IsInterface)
            {
                _typeBuilder = _moduleBuilder.DefineType($"{_interfaceOrBaseType.Name}{GenerateSurfix()}", TypeAttributes.Public, typeof(object), new Type[] { _interfaceOrBaseType });
                _targetField = _typeBuilder.DefineField("_target",  _interfaceOrBaseType, FieldAttributes.Private);
            }
            else
            {
                _typeBuilder = _moduleBuilder.DefineType($"{_interfaceOrBaseType.Name}{GenerateSurfix()}", TypeAttributes.Public, _interfaceOrBaseType);
            }

            if (_interfaceOrBaseType.IsGenericType)
            {
                var interfaceParameters = ((TypeInfo)_interfaceOrBaseType).GenericTypeParameters;
                var parameterNames = interfaceParameters.Select(it => it.Name).ToArray();
                var proxyParameters = _typeBuilder.DefineGenericParameters(parameterNames);
                for (int index = 0; index < interfaceParameters.Length; index++)
                {
                    var parameter = proxyParameters[index];
                    parameter.SetGenericParameterAttributes(interfaceParameters[index].GenericParameterAttributes);

                    var constraints = new List<Type>();
                    foreach (Type constraint in interfaceParameters[index].GetGenericParameterConstraints())
                    {
                        if (constraint.IsClass)
                        {
                            parameter.SetBaseTypeConstraint(constraint);
                        }
                        else
                        {
                            constraints.Add(constraint);
                        }
                    }
                    if (constraints.Count > 0)
                    {
                        parameter.SetInterfaceConstraints(constraints.ToArray());
                    }
                }
            }

            _interceptorsField = _typeBuilder.DefineField("_interceptors", typeof(IInterceptorRegistry), FieldAttributes.Private);

            if (_interfaceOrBaseType.IsInterface)
            {
                GenerateForInterface();
            }
            else
            {
                GenerateForVirtualMethods();
            }

            return _typeBuilder.CreateTypeInfo();
        }
        #endregion

        #region Private Methods

        /// <summary>
        /// Defines the constructor for class implementing the specified interface.
        /// </summary>
        /// <returns>The <see cref="ConstructorBuilder"/> representing the generated constructor.</returns>
        /// <example>
        /// public class FoobarProxy: IFoobar
        /// {
        ///     private Foobar _target;
        ///     private InterceptorRegistry _interceptors;
        ///     public (FoobarProxy target, IInterceptorResolver resolver)
        ///     {
        ///         _target = target;
        ///         _interceptors = resolver.GetInterceptors(typeof(IFoobar), typeof(Foobar));
        ///     }
        /// }
        /// </example>
        private ConstructorBuilder DefineConstructorForImplementationClass()
        {
            var targetParameterType = _interfaceOrBaseType.IsGenericTypeDefinition ? _targetType : _interfaceOrBaseType;
            var parameterTypes = new Type[] { targetParameterType, typeof(IInterceptorResolver) };
            var constructor = _typeBuilder.DefineConstructor(MethodAttributes.Public, CallingConventions.HasThis, parameterTypes);
            var il = constructor.GetILGenerator();

            //Call object's constructor.
            il.Emit(OpCodes.Ldarg_0);
            il.Emit(OpCodes.Call, ReflectionUtility.ConstructorOfObject);

            //Set _target filed
            il.Emit(OpCodes.Ldarg_0);
            il.Emit(OpCodes.Ldarg_1);
            il.Emit(OpCodes.Stfld, _targetField);

            //Set _interceptors field
            il.Emit(OpCodes.Ldarg_0);
            il.Emit(OpCodes.Ldarg_2);
            il.Emit(OpCodes.Ldtoken, _interfaceOrBaseType);
            il.Emit(OpCodes.Ldtoken, _targetType);
            il.Emit(OpCodes.Callvirt, _getInterceptorsMethod4Interface);
            il.Emit(OpCodes.Stfld, _interceptorsField);

            //Return
            il.Emit(OpCodes.Ret);
            return constructor;
        }

        /// <summary>
        /// Defines the constructors for class inheriting the specified type.
        /// </summary>          
        /// <returns>The <see cref="ConstructorBuilder"/> array representing the generated constructors.</returns>
        /// <example>
        /// public class FoobarProxy: Foobar
        /// {
        ///     private readonly IInterceptionRegistry _interceptors;
        ///     private InterceptorRegistry _interceptors;
        ///     public (Foo foo, Bar bar, IInterceptorResolver resolver):base(foo, bar)
        ///     {
        ///         _interceptors = resolver.GetInterceptors(typeof(Foobar));
        ///     }
        /// }
        /// </example>
        private ConstructorBuilder[] DefineConstructorsForSubClass()
        {
            var constructors = _interfaceOrBaseType.GetConstructors(BindingFlags.Instance | BindingFlags.Public);
            var constructorBuilders = new ConstructorBuilder[constructors.Length];
            for (int index1 = 0; index1 < constructors.Length; index1++)
            {
                var constructor = constructors[index1];
                var parameterTypes = constructor.GetParameters().Select(it => it.ParameterType).ToList();
                parameterTypes.Add(typeof(IInterceptorResolver));
                var constructorBuilder = constructorBuilders[index1] = _typeBuilder.DefineConstructor(constructor.Attributes, CallingConventions.HasThis, parameterTypes.ToArray());
                var il = constructorBuilder.GetILGenerator();

                il.Emit(OpCodes.Ldarg_0);
                for (int index2 = 0; index2 < parameterTypes.Count -1; index2++)
                {
                    il.EmitLoadArgument(index2);
                }                
                il.Emit(OpCodes.Call, constructor);

                il.Emit(OpCodes.Ldarg_0);
                il.EmitLoadArgument(parameterTypes.Count - 1);
                il.Emit(OpCodes.Ldtoken, _interfaceOrBaseType);
                il.Emit(OpCodes.Callvirt, _getInterceptorsMethod4Class);
                il.Emit(OpCodes.Stfld, _interceptorsField);

                il.Emit(OpCodes.Ret);
            }
            return constructorBuilders;
        }

        /// <summary>
        /// Defines the interceptable method.
        /// </summary>
        /// <param name="methodInfo">The <see cref="MethodInfo"/> of the type to intercept.</param>
        /// <param name="attributes">The attributes applied to the generated method.</param>
        /// <returns>The <see cref="MethodBuilder"/> representing the generated method.</returns>   
        /// <exception cref="ArgumentNullException">Specified <paramref name="methodInfo"/> is null.</exception>
        private MethodBuilder DefineInterceptableMethod(MethodInfo methodInfo, MethodAttributes attributes)
        {
            Guard.ArgumentNotNull(methodInfo, nameof(methodInfo));

            var parameters = methodInfo.GetParameters();
            var parameterTypes = parameters.Select(it => it.ParameterType).ToArray();
            var methodBuilder = _typeBuilder.DefineMethod(methodInfo.Name, attributes, methodInfo.ReturnType, parameterTypes);
            var genericParameterTypeMap = methodInfo.IsGenericMethod
                ? DefineMethodGenericParameters(methodBuilder, methodInfo)
                : new Dictionary<Type, Type>();
            for (int index = 0; index < parameters.Length; index++)
            {
                var parameter = parameters[index];
                methodBuilder.DefineParameter(index + 1, parameter.Attributes, parameter.Name);
            }

            var il = methodBuilder.GetILGenerator();

            il.DeclareLocal(typeof(object[]));                  //Loc_0: arguments   
            il.DeclareLocal(typeof(InvocationContext));         //Loc_1: invocationContext
            il.DeclareLocal(typeof(MethodBase));                //Loc_2: methodBase
            il.DeclareLocal(typeof(Task));                      //Loc_3: task

            var returnType = methodInfo.ReturnTaskOfResult()
                ? methodInfo.ReturnType.GetGenericArguments()[0]
                : methodInfo.ReturnType;
            returnType = genericParameterTypeMap.TryGetValue(returnType, out var type)
                ? type
                : returnType;

            //New object[] for InvocationContext.Arguments
            il.EmitLoadConstantInt32(parameters.Length);
            il.Emit(OpCodes.Newarr, typeof(object));

            //Load arguments and store them to object[]
            for (int index = 0; index < parameters.Length; index++)
            {
                var parameter = parameters[index];
                il.Emit(OpCodes.Dup);
                il.EmitLoadConstantInt32(index);
                il.EmitLoadArgument(index);
                if (parameter.ParameterType.IsByRef)
                {
                    il.EmitLdInd(parameter.ParameterType);
                }
                il.EmitBox(parameter.ParameterType);
                il.Emit(OpCodes.Stelem_Ref);
            }
            il.Emit(OpCodes.Stloc_0);

            //Load and store current method
            il.Emit(OpCodes.Ldtoken, methodInfo);
            if (methodInfo.DeclaringType.IsGenericType)
            {
                il.Emit(OpCodes.Ldtoken, methodInfo.DeclaringType);
                il.Emit(OpCodes.Call, ReflectionUtility.GetMethodFromHandleMethodOfMethodBase2);
            }
            else
            {
                il.Emit(OpCodes.Call, ReflectionUtility.GetMethodFromHandleMethodOfMethodBase1);
            }
            il.Emit(OpCodes.Stloc_2);

            //Create and store DefaultInvocationContext
            il.Emit(OpCodes.Ldloc_2);
            il.Emit(OpCodes.Ldarg_0);
            il.Emit(OpCodes.Ldarg_0);
            if (_targetField != null)
            {
                il.Emit(OpCodes.Ldfld, _targetField);
            }
            il.Emit(OpCodes.Ldloc_0);
            il.Emit(OpCodes.Newobj, ReflectionUtility.ConstructorOfDefaultInvocationContext);
            il.Emit(OpCodes.Stloc_1);

            //Get and store current method specific interceptor
            il.Emit(OpCodes.Ldarg_0);
            il.Emit(OpCodes.Ldfld, _interceptorsField);
            il.Emit(OpCodes.Ldloc_2);
            il.Emit(OpCodes.Callvirt, _methodOfGetInterceptor);

            //Create and store handler to invoke target method
            il.Emit(OpCodes.Ldarg_0);
            MethodInfo invokeTargetMethod = DefineTargetInvokerMethod(methodInfo);
            if (methodInfo.IsGenericMethod)
            {
                invokeTargetMethod = invokeTargetMethod.MakeGenericMethod(methodInfo.GetGenericArguments());
            }
            il.Emit(OpCodes.Ldftn, invokeTargetMethod);
            il.Emit(OpCodes.Newobj, ReflectionUtility.ConstructorOfInterceptDelegate);

            //Invoke final handler
            il.Emit(OpCodes.Ldloc_1);
            il.Emit(OpCodes.Call, ReflectionUtility.InvokeHandlerMethod);
            il.Emit(OpCodes.Stloc_3);

            //When return Task<TResult>
            if (methodInfo.ReturnTaskOfResult())
            {
                il.Emit(OpCodes.Ldloc_3);
                il.Emit(OpCodes.Ldloc_1);
                il.Emit(OpCodes.Call, ReturnValueAccessor.GetTaskOfResultMethodDefinition.MakeGenericMethod(returnType));
                il.Emit(OpCodes.Ret);
                return methodBuilder;
            }

            //When return Task
            if (methodInfo.ReturnTask())
            {
                il.Emit(OpCodes.Ldloc_3);
                il.Emit(OpCodes.Ret);
                return methodBuilder;
            }

            void StoreRefArguments()
            {
                if (parameters.Any(it => it.ParameterType.IsByRef))
                {
                    for (int index = 0; index < parameters.Length; index++)
                    {
                        var parameter = parameters[index];
                        if (parameter.ParameterType.IsByRef)
                        {
                            il.EmitLoadArgument(index);
                            il.Emit(OpCodes.Ldloc_0);
                            il.EmitLoadConstantInt32(index);
                            il.Emit(OpCodes.Ldelem_Ref);
                            il.EmitUnboxOrCast(parameter.ParameterType);
                            il.EmitStInd(parameter.ParameterType);
                        }
                    }
                }
            }

            //Return a general value
            if (!methodInfo.ReturnVoid())
            {
                il.DeclareLocal(returnType);
                il.Emit(OpCodes.Ldloc_1);
                il.Emit(OpCodes.Call, ReturnValueAccessor.GetResultMethodDefinition.MakeGenericMethod(returnType));
                il.Emit(OpCodes.Stloc_S, 4);
                StoreRefArguments();
                il.Emit(OpCodes.Ldloc_S, 4);
                il.Emit(OpCodes.Ret);
                return methodBuilder;
            }

            //Return void
            il.Emit(OpCodes.Ldloc_3);
            il.Emit(OpCodes.Callvirt, ReflectionUtility.WaitMethodOfTask);
            StoreRefArguments();
            il.Emit(OpCodes.Ret);
            return methodBuilder;
        }

        /// <summary>
        /// Defines the non interceptable method.
        /// </summary>
        /// <param name="methodInfo">The <see cref="MethodInfo"/> of the type to intercept.</param>
        /// <param name="attributes">The attributes applied to the generated method.</param>
        /// <returns>The <see cref="MethodBuilder"/> representing the generated method.</returns> 
        /// <exception cref="ArgumentNullException">Specified <paramref name="methodInfo"/> is null.</exception>  
        private MethodBuilder DefineNonInterceptableMethod(MethodInfo methodInfo, MethodAttributes attributes)
        {
            Guard.ArgumentNotNull(methodInfo, nameof(methodInfo));
            methodInfo = _interceptors.GetTargetMethod(methodInfo);
            var parameters = methodInfo.GetParameters();
            var parameterTypes = parameters.Select(it => it.ParameterType).ToArray();
            var methodBuilder = _typeBuilder.DefineMethod(methodInfo.Name, attributes, methodInfo.ReturnType, parameterTypes);
            if (methodInfo.IsGenericMethod)
            {
                DefineMethodGenericParameters(methodBuilder, methodInfo);
            }
            for (int index = 0; index < parameters.Length; index++)
            {
                var parameter = parameters[index];
                methodBuilder.DefineParameter(index, parameter.Attributes, parameter.Name);
            }
            var il = methodBuilder.GetILGenerator();

            il.Emit(OpCodes.Ldarg_0);
            il.Emit(OpCodes.Ldfld, _targetField);
            for (int index = 0; index < parameters.Length; index++)
            {
                var parameter = parameters[index];
                il.EmitLoadArgument(index);
            }
            il.Emit(OpCodes.Call, methodInfo);
            il.Emit(OpCodes.Ret);
            return methodBuilder;
        }

        /// <summary>
        /// Gets the method attributes applied to generated method.
        /// </summary>                                                 
        /// <param name="methodInfo">The <see cref="MethodInfo"/> of the type to intercept.</param>
        /// <returns></returns>
        private MethodAttributes? GetMethodAttributes(MethodInfo methodInfo)
        {
            Guard.ArgumentNotNull(methodInfo, nameof(methodInfo));
            if (_interfaceOrBaseType.IsInterface)
            {
                return MethodAttributes.Public | MethodAttributes.HideBySig | MethodAttributes.NewSlot | MethodAttributes.Virtual | MethodAttributes.Final;
            }
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

            return null;
        }

        ///// <summary>
        ///// Defines the "SetInterceptors" method to set the interceptors of type <see cref="InterceptorRegistry"/>.
        ///// </summary>
        ///// <param name="attributes">The attributes applied to the generated method.</param>
        ///// <returns>The <see cref="MethodBuilder"/> representing the generated method.</returns>
        //private  MethodBuilder DefineSetInterceptorsMethod(MethodAttributes attributes)
        //{
        //    var methodBuilder = _typeBuilder.DefineMethod("SetInterceptors", attributes, typeof(void), new Type[] { typeof(IInterceptorRegistry) });
        //    var il = methodBuilder.GetILGenerator();
        //    il.Emit(OpCodes.Ldarg_0);
        //    il.Emit(OpCodes.Ldarg_1);
        //    il.Emit(OpCodes.Stfld, _interceptorsField);
        //    il.Emit(OpCodes.Ret);
        //    return methodBuilder;
        //}

        private void GenerateForInterface()
        {
            var attributes = MethodAttributes.Public | MethodAttributes.HideBySig | MethodAttributes.NewSlot | MethodAttributes.Virtual | MethodAttributes.Final;
            DefineConstructorForImplementationClass();

            //Methods
            foreach (var methodInfo in _interfaceOrBaseType.GetMethods().Where(it => !it.IsSpecialName))
            {
                if (_interceptors.IsInterceptable(methodInfo))
                {
                    DefineInterceptableMethod(methodInfo, attributes);
                }
                else
                {
                    DefineNonInterceptableMethod(methodInfo, attributes);
                }
            }

            //Properties
            foreach (var property in _interfaceOrBaseType.GetProperties())
            {
                var parameterTypes = property.GetIndexParameters().Select(it => it.ParameterType).ToArray();
                var propertyBuilder = _typeBuilder.DefineProperty(property.Name, property.Attributes, property.PropertyType, parameterTypes);
                var getMethod = property.GetMethod;
                if (null != getMethod)
                {
                    var getMethodBuilder = _interceptors.IsInterceptable(getMethod)
                        ? DefineInterceptableMethod(getMethod, attributes)
                        : DefineNonInterceptableMethod(getMethod, attributes);
                    propertyBuilder.SetGetMethod(getMethodBuilder);
                }
                var setMethod = property.SetMethod;
                if (null != setMethod)
                {
                    var setMethodBuilder = _interceptors.IsInterceptable(setMethod)
                        ? DefineInterceptableMethod(setMethod, attributes)
                        : DefineNonInterceptableMethod(setMethod, attributes);
                    propertyBuilder.SetGetMethod(setMethodBuilder);
                }
            }

            //Events
            foreach (var eventInfo in _interfaceOrBaseType.GetEvents())
            {
                var eventBuilder = _typeBuilder.DefineEvent(eventInfo.Name, eventInfo.Attributes, eventInfo.EventHandlerType);
                eventBuilder.SetAddOnMethod(DefineNonInterceptableMethod(eventInfo.AddMethod, attributes));
                eventBuilder.SetRemoveOnMethod(DefineNonInterceptableMethod(eventInfo.RemoveMethod, attributes));
            }
        }

        private void GenerateForVirtualMethods()
        {
            //Constructor
            DefineConstructorsForSubClass();

            //SetInterceptors method
            //DefineSetInterceptorsMethod(MethodAttributes.Public | MethodAttributes.HideBySig | MethodAttributes.Virtual | MethodAttributes.Final);

            //Methods
            foreach (var methodInfo in _interfaceOrBaseType.GetMethods(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic))
            {
                if (!methodInfo.IsSpecialName && methodInfo.IsOverridable() && _interceptors.IsInterceptable(methodInfo))
                {
                    var attributes = GetMethodAttributes(methodInfo);
                    if (null != attributes)
                    {
                        DefineInterceptableMethod(methodInfo, attributes.Value);
                    }
                }
            }

            //Properties
            foreach (var property in _interfaceOrBaseType.GetProperties(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic))
            {
                PropertyBuilder propertyBuilder = null;

                //Getter
                var getMethod = property.GetMethod;
                if (getMethod != null && getMethod.IsOverridable() && _interceptors.IsInterceptable(getMethod))
                {
                    var attributes = GetMethodAttributes(getMethod);
                    if (null != attributes)
                    {
                        var parameterTypes = property.GetIndexParameters().Select(it => it.ParameterType).ToArray();
                        propertyBuilder = _typeBuilder.DefineProperty(property.Name, property.Attributes, property.PropertyType, parameterTypes);
                        var methodBuilder = DefineInterceptableMethod(getMethod, attributes.Value);
                        propertyBuilder.SetGetMethod(methodBuilder);
                    }
                }

                //Setter
                var setMethod = property.SetMethod;
                if (setMethod != null && getMethod.IsOverridable() && _interceptors.IsInterceptable(setMethod))
                {
                    var attributes = GetMethodAttributes(setMethod);
                    if (null != attributes)
                    {
                        var parameterTypes = property.GetIndexParameters().Select(it => it.ParameterType).ToArray();
                        propertyBuilder = propertyBuilder ?? _typeBuilder.DefineProperty(property.Name, property.Attributes, property.PropertyType, parameterTypes);
                        var methodBuilder = DefineInterceptableMethod(setMethod, attributes.Value);
                        propertyBuilder.SetSetMethod(methodBuilder);
                    }
                }
            }
        }

        private Dictionary<Type, Type> DefineMethodGenericParameters(MethodBuilder methodBuilder, MethodInfo methodInfo)
        {
            var genericParameters = methodInfo.GetGenericArguments();
            //var genericParameterNames = Enumerable.Range(1, genericParameters.Length).Select(it => $"T{it}").ToArray();
            var genericParameterNames = genericParameters.Select(it => it.Name).ToArray();
            var builders = methodBuilder.DefineGenericParameters(genericParameterNames);
            for (int index = 0; index < genericParameters.Length; index++)
            {
                var builder = builders[index];
                var genericParameter = genericParameters[index];
                if (!genericParameter.IsGenericParameter)
                {
                    continue;
                }
                builder.SetGenericParameterAttributes(genericParameter.GenericParameterAttributes);

                var interfaceConstraints = new List<Type>();
                foreach (Type constraint in genericParameter.GetGenericParameterConstraints())
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

            var map = new Dictionary<Type, Type>();
            var genericParameters2 = methodBuilder.GetGenericArguments();
            for (int index = 0; index < genericParameters.Length; index++)
            {
                map.Add(genericParameters[index], genericParameters2[index]);
            }

            return map;
        }

        //Task Invoke{Surfix}(InvocationContext invocationContext)
        private MethodBuilder DefineTargetInvokerMethod(MethodInfo methodInfo)
        {
            var @interface = methodInfo.DeclaringType;
            var prefix = @interface.IsGenericType
                ? @interface.FullName.Substring(0, @interface.FullName.IndexOf('`'))
                : @interface.FullName;
            prefix = prefix.Replace("+", ".");

            var isExplicitlyEmplemented = _interceptors.GetTargetMethod(methodInfo).Name.StartsWith(prefix);
            if (!isExplicitlyEmplemented)
            {
                methodInfo = _interceptors.GetTargetMethod(methodInfo);
            }
            var methodBuilder = _typeBuilder.DefineMethod($"Invoke{GenerateSurfix()}", MethodAttributes.Private | MethodAttributes.HideBySig, typeof(Task), new Type[] { typeof(InvocationContext) });
            var genericParameterTypeMap = methodInfo.IsGenericMethod
              ? DefineMethodGenericParameters(methodBuilder, methodInfo)
              : new Dictionary<Type, Type>(); 
            var parameters = methodInfo.GetParameters();
            var parameterTypes = parameters.Select(it => it.ParameterType.GetNonByRefType()).ToArray();

            for (int index = 0; index < parameterTypes.Length; index++)
            {
                if (genericParameterTypeMap.TryGetValue(parameterTypes[index], out var type))
                {
                    parameterTypes[index] = type;
                }
            }

            var il = methodBuilder.GetILGenerator();

            //InvocationContext.Arguments
            il.DeclareLocal(typeof(object[]));
            Array.ForEach(parameterTypes, it => il.DeclareLocal(it));
            var returnType = methodInfo.ReturnType;
            if (methodInfo.ReturnType != typeof(void))
            {
                returnType = genericParameterTypeMap.TryGetValue(methodInfo.ReturnType, out var type)
                    ? type
                    : methodInfo.ReturnType;
                il.DeclareLocal(returnType);
            }

            //Load and store InvocationContext.Arguments. 
            il.Emit(OpCodes.Ldarg_1);
            il.Emit(OpCodes.Callvirt, ReflectionUtility.GetMethodOfArgumentsPropertyOfInvocationContext);
            il.EmitStLocal4Arguments();

            //Load and store all arguments
            for (int index = 0; index < parameters.Length; index++)
            {
                var parameter = parameters[index];
                il.Emit(OpCodes.Ldloc_0);
                il.EmitLoadConstantInt32(index);
                il.Emit(OpCodes.Ldelem_Ref);
                il.EmitUnboxOrCast(parameterTypes[index]);
                il.EmitStLocal4Argument(index);
            }

            //Invoke target method.
            il.Emit(OpCodes.Ldarg_0);
            if (_targetField != null)
            {
                il.Emit(OpCodes.Ldfld, _targetField);
            }
            for (int index = 0; index < parameters.Length; index++)
            {
                var parameter = parameters[index];
                if (parameter.ParameterType.IsByRef)
                {
                    il.EmitLdLocala4Argument(index);
                }
                else
                {
                    il.EmitLdLocal4Argument(index);
                }
            }

            if (methodInfo.IsGenericMethod)
            {
                var genericMethod = methodInfo.MakeGenericMethod(genericParameterTypeMap.Values.ToArray());
                if (isExplicitlyEmplemented)
                {
                    il.Emit(OpCodes.Callvirt, genericMethod);
                }
                else
                {
                    il.Emit(OpCodes.Call, genericMethod);
                }
            }
            else
            {
                if (isExplicitlyEmplemented)
                {
                    il.Emit(OpCodes.Callvirt, methodInfo);
                }
                else
                {
                    il.Emit(OpCodes.Call, methodInfo);
                }
            }

            //Save return value to InvocationContext.ReturnValue
            if (methodInfo.ReturnType != typeof(void))
            {
                il.EmitStLocal4ReturnValue(parameters.Length);
                il.Emit(OpCodes.Ldarg_1);
                il.EmitLdLocal4ReturnValue(parameters.Length);
                il.EmitBox(returnType);
                il.Emit(OpCodes.Callvirt, ReflectionUtility.SetMethodOfReturnValueOfInvocationContext);
            }

            //Set ref arguments InvocationContext.Arguments
            if (parameters.Any(it => it.ParameterType.IsByRef))
            {
                for (int index = 0; index < parameters.Length; index++)
                {
                    var parameter = parameters[index];
                    if (parameter.ParameterType.IsByRef)
                    {
                        il.EmitLdLocal4Arguments();
                        il.EmitLoadConstantInt32(index);
                        il.EmitLdLocal4Argument(index);
                        il.EmitBox(parameterTypes[index]);
                        il.Emit(OpCodes.Stelem_Ref);
                    }
                }
            }

            //return Task.CompletedTask
            il.Emit(OpCodes.Call, ReflectionUtility.GetMethodOfCompletedTaskOfTask);
            il.Emit(OpCodes.Ret);

            return methodBuilder;
        }

        private Dictionary<Type, Type> DefineTypeGenericParameters(TypeBuilder typeBuilder, MethodInfo method)
        {
            var map = new Dictionary<Type, Type>();
            var genericParameters = method.GetGenericArguments();
            var genericParameterNames = Enumerable.Range(1, genericParameters.Length).Select(it => $"T{it}").ToArray();
            var builders = typeBuilder.DefineGenericParameters(genericParameterNames);
            for (int index = 0; index < genericParameters.Length; index++)
            {
                var builder = builders[index];
                var genericParameter = genericParameters[index];
                if (!genericParameter.IsGenericParameter)
                {
                    continue;
                }
                builder.SetGenericParameterAttributes(genericParameter.GenericParameterAttributes);

                var interfaceConstraints = new List<Type>();
                foreach (Type constraint in genericParameter.GetGenericParameterConstraints())
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
            var genericParameters2 = typeBuilder.GetGenericArguments();
            for (int index = 0; index < genericParameters.Length; index++)
            {
                map.Add(genericParameters[index], genericParameters2[index]);
            }

            return map;
        }

        private static string GenerateSurfix()
        {
            return Guid.NewGuid().ToString().Replace("-", "");
        }

        #endregion
    }

    internal static class TargetInvokerExtensions
    {
        public static void EmitLdLocal4Arguments(this ILGenerator il)
        {
            il.Emit(OpCodes.Ldloc_0);
        }

        public static void EmitStLocal4Arguments(this ILGenerator il)
        {
            il.Emit(OpCodes.Stloc_0);
        }

        public static void EmitLdLocal4Argument(this ILGenerator il, int index)
        {
            switch (index)
            {
                case 0: { il.Emit(OpCodes.Ldloc_1); break; }
                case 1: { il.Emit(OpCodes.Ldloc_2); break; }
                case 2: { il.Emit(OpCodes.Ldloc_3); break; }
                default: { il.Emit(OpCodes.Ldloc_S, index + 1); break; }
            }
        }

        public static void EmitLdLocala4Argument(this ILGenerator il, int index)
        {
            il.Emit(OpCodes.Ldloca_S, index + 1);
        }

        public static void EmitStLocal4Argument(this ILGenerator il, int index)
        {
            switch (index)
            {
                case 0: { il.Emit(OpCodes.Stloc_1); break; }
                case 1: { il.Emit(OpCodes.Stloc_2); break; }
                case 2: { il.Emit(OpCodes.Stloc_3); break; }
                default: { il.Emit(OpCodes.Stloc_S, index + 1); break; }
            }
        }

        public static void EmitLdLocal4ReturnValue(this ILGenerator il, int argumentCount)
        {
            switch (argumentCount)
            {
                case 0: { il.Emit(OpCodes.Ldloc_1); break; }
                case 1: { il.Emit(OpCodes.Ldloc_2); break; }
                case 2: { il.Emit(OpCodes.Ldloc_3); break; }
                default: { il.Emit(OpCodes.Ldloc_S, argumentCount + 1); break; }
            }
        }

        public static void EmitStLocal4ReturnValue(this ILGenerator il, int argumentCount)
        {
            switch (argumentCount)
            {
                case 0: { il.Emit(OpCodes.Stloc_1); break; }
                case 1: { il.Emit(OpCodes.Stloc_2); break; }
                case 2: { il.Emit(OpCodes.Stloc_3); break; }
                default: { il.Emit(OpCodes.Stloc_S, argumentCount + 1); break; }
            }
        }
    }
}