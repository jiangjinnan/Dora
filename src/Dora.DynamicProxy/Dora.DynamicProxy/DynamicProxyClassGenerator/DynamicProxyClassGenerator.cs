using Dora.DynamicProxy.Properties;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Threading.Tasks;

namespace Dora.DynamicProxy
{
    /// <summary>
    /// Class generator to generate interceptable dynamic proxy class.
    /// </summary>
    public class DynamicProxyClassGenerator
    {
        #region Properties
        /// <summary>
        /// Gets the type to intercept.
        /// </summary>
        /// <value>
        /// The type to intercept.
        /// </value>
        public Type TypeToIntercept { get; }

        /// <summary>
        /// Gets the <see cref="InterceptorRegistry"/> representing the type members decorated with interceptors.
        /// </summary>
        /// <value>
        /// The <see cref="InterceptorRegistry"/> representing the type members decorated with interceptors.
        /// </value>
        public InterceptorRegistry Interceptors { get; }


        /// <summary>
        /// Gets the <see cref="ModuleBuilder"/>  in which the dynamic proxy classes are defined.
        /// </summary>
        /// <value>
        /// The <see cref="ModuleBuilder"/>  in which the dynamic proxy classes are defined.
        /// </value>
        public ModuleBuilder ModuleBuilder { get; }


        /// <summary>
        /// Gets the <see cref="TypeBuilder"/> representing the dynamic proxy class.
        /// </summary>
        /// <value>
        /// The <see cref="TypeBuilder"/> representing the dynamic proxy class.
        /// </value>
        public TypeBuilder TypeBuilder { get; }


        /// <summary>
        /// Gets the <see cref="FieldBuilder"/> representing the "_target" field.
        /// </summary>
        /// <value>
        /// The <see cref="FieldBuilder"/> representing the "_target" field.
        /// </value>
        public FieldBuilder TargetField { get; }

        /// <summary>
        /// Gets the <see cref="FieldBuilder"/> representing the "_interceptors" field.
        /// </summary>
        /// <value>
        /// The <see cref="FieldBuilder"/> representing the "_interceptors" field.
        /// </value>
        public FieldBuilder InterceptorsField { get; }
        #endregion

        #region Constructors
        /// <summary>
        /// Initializes a new instance of the <see cref="DynamicProxyClassGenerator"/> class.
        /// </summary>
        /// <param name="typeToIntercept">The type to intercept.</param>
        /// <param name="interceptors">The <see cref="InterceptorRegistry"/> representing the type members decorated with interceptors.</param>
        /// <exception cref="ArgumentNullException">Specified <paramref name="typeToIntercept"/> is null.</exception>    
        /// <exception cref="ArgumentNullException">Specified <paramref name="interceptors"/> is null.</exception>
        private DynamicProxyClassGenerator(Type typeToIntercept, InterceptorRegistry interceptors)
        {
            TypeToIntercept = Guard.ArgumentNotNull(typeToIntercept, nameof(typeToIntercept));
            Interceptors = Guard.ArgumentNotNull(interceptors, nameof(interceptors));

            var assemblyName = new AssemblyName($"AssemblyFor{typeToIntercept.Name}{GenerateSurfix()}");
            var assemblyBuilder = AssemblyBuilder.DefineDynamicAssembly(assemblyName, AssemblyBuilderAccess.Run);
            ModuleBuilder = assemblyBuilder.DefineDynamicModule($"{assemblyName}.dll");

            if (TypeToIntercept.IsInterface)
            {
                TypeBuilder = ModuleBuilder.DefineType($"{typeToIntercept.Name}{GenerateSurfix()}", TypeAttributes.Public, typeof(object), new Type[] { typeToIntercept });
                TargetField = TypeBuilder.DefineField("_target", typeToIntercept, FieldAttributes.Private);
            }
            else
            {
                TypeBuilder = ModuleBuilder.DefineType($"{typeToIntercept.Name}{GenerateSurfix()}", TypeAttributes.Public, typeToIntercept, new Type[] { typeof(IInterceptorsInitializer) });
            }
            InterceptorsField = TypeBuilder.DefineField("_interceptors", typeof(InterceptorRegistry), FieldAttributes.Private);
        }

        #endregion

        #region Public Methods
        /// <summary>
        /// Creates a <see cref="DynamicProxyClassGenerator"/> to generate dynamic proxy class based on interface based interception.
        /// </summary>
        /// <param name="interface">The interface type to intercept.</param>
        /// <param name="interceptors">The <see cref="InterceptorRegistry"/> representing the type members decorated with interceptors.</param>
        /// <returns>The created <see cref="DynamicProxyClassGenerator"/>.</returns>      
        /// <exception cref="ArgumentNullException">Specified <paramref name="interface"/> is null.</exception>   
        /// <exception cref="ArgumentException">Specified <paramref name="interface"/> is not an interface type.</exception>     
        /// <exception cref="ArgumentNullException">Specified <paramref name="interceptors"/> is null.</exception>
        public static DynamicProxyClassGenerator CreateInterfaceGenerator(Type @interface, InterceptorRegistry interceptors)
        {
            Guard.ArgumentNotNull(@interface, nameof(@interface));
            if (!@interface.IsInterface)
            {
                throw new ArgumentException(Resources.ExceptionArgumentNotInterface, nameof(@interface));
            }
            return new DynamicProxyClassGenerator(@interface, interceptors);
        }

        /// <summary>
        /// Creates a <see cref="DynamicProxyClassGenerator"/> to generate dynamic proxy class based on interface based interception.
        /// </summary>
        /// <param name="type">The interface type to intercept.</param>
        /// <param name="interceptors">The <see cref="InterceptorRegistry"/> representing the type members decorated with interceptors.</param>
        /// <returns>The created <see cref="DynamicProxyClassGenerator"/>.</returns>      
        /// <exception cref="ArgumentNullException">Specified <paramref name="type"/> is null.</exception>   
        /// <exception cref="ArgumentException">Specified <paramref name="type"/> is  a sealed type.</exception>     
        /// <exception cref="ArgumentNullException">Specified <paramref name="interceptors"/> is null.</exception>
        public static DynamicProxyClassGenerator CreateVirtualMethodGenerator(Type type, InterceptorRegistry interceptors)
        {
            Guard.ArgumentNotNull(type, nameof(type));
            if (type.IsSealed)
            {
                throw new ArgumentException(Resources.ExceptionSealedTypeNotAllowed, nameof(type));
            }
            return new DynamicProxyClassGenerator(type, interceptors);
        }

        /// <summary>
        /// Generates the type of the proxy.
        /// </summary>
        /// <returns>The type representing the generated dynamic proxy class.</returns>
        public Type GenerateProxyType()
        {
            if (TypeToIntercept.IsInterface)
            {
                GenerateForInterface();
            }
            else
            {
                GenerateForVirtualMethods();
            }

            return TypeBuilder.CreateTypeInfo();
        }
        #endregion

        #region Protected Methods

        /// <summary>
        /// Defines the constructor for class implementing the specified interface.
        /// </summary>
        /// <returns>The <see cref="ConstructorBuilder"/> representing the generated constructor.</returns>
        /// <example>
        /// public class FoobarProxy
        /// {
        ///     private Foobar _target;
        ///     private InterceptorRegistry _interceptors;
        ///     public (FoobarProxy target, InterceptorRegistry interceptors)
        ///     {
        ///         _target = target;
        ///         _interceptors = interceptors;
        ///     }
        /// }
        /// </example>
        protected virtual ConstructorBuilder DefineConstructorForImplementationClass()
        {
            var parameterTypes = new Type[] { TypeToIntercept, typeof(InterceptorRegistry) };
            var constructor = TypeBuilder.DefineConstructor(MethodAttributes.Public, CallingConventions.HasThis, parameterTypes);
            var il = constructor.GetILGenerator();

            //Call object's constructor.
            il.Emit(OpCodes.Ldarg_0);
            il.Emit(OpCodes.Call, ReflectionUtility.ConstructorOfObject);

            //Set _target filed
            il.Emit(OpCodes.Ldarg_0);
            il.Emit(OpCodes.Ldarg_1);
            il.Emit(OpCodes.Stfld, TargetField);

            //Set _interceptors field
            il.Emit(OpCodes.Ldarg_0);
            il.Emit(OpCodes.Ldarg_2);
            il.Emit(OpCodes.Stfld, InterceptorsField);

            //Return
            il.Emit(OpCodes.Ret);
            return constructor;
        }

        /// <summary>
        /// Defines the constructors for class inheriting the specified type.
        /// </summary>          
        /// <returns>The <see cref="ConstructorBuilder"/> array representing the generated constructors.</returns>
        protected virtual ConstructorBuilder[] DefineConstructorsForSubClass()
        {
            var constructors = TypeToIntercept.GetConstructors(BindingFlags.Instance| BindingFlags.Public);
            var constructorBuilders = new ConstructorBuilder[constructors.Length]; 
            for (int index1 = 0; index1 < constructors.Length; index1++)
            {
                var constructor = constructors[index1];
                var parameterTypes = constructor.GetParameters().Select(it => it.ParameterType).ToArray();
                var constructorBuilder = constructorBuilders[index1] = TypeBuilder.DefineConstructor(constructor.Attributes, CallingConventions.HasThis, parameterTypes);
                var il = constructorBuilder.GetILGenerator();   

                il.Emit(OpCodes.Ldarg_0);
                for (int index2 = 0; index2 < parameterTypes.Length; index2++)
                {
                    il.EmitLoadArgument(index2);
                }
                il.Emit(OpCodes.Call, constructor);
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
        protected virtual MethodBuilder DefineInterceptableMethod(MethodInfo methodInfo, MethodAttributes attributes)
        {
            Guard.ArgumentNotNull(methodInfo, nameof(methodInfo));

            var parameters = methodInfo.GetParameters();                         
            var parameterTypes = parameters.Select(it => it.ParameterType).ToArray();                                                                              
            var methodBuilder = TypeBuilder.DefineMethod(methodInfo.Name, attributes, methodInfo.ReturnType, parameterTypes);
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
            if (TargetField != null)
            {
                il.Emit(OpCodes.Ldfld, TargetField);
            }
            il.Emit(OpCodes.Ldloc_0);
            il.Emit(OpCodes.Newobj, ReflectionUtility.ConstructorOfDefaultInvocationContext);
            il.Emit(OpCodes.Stloc_1);    

            //Get and store current method specific interceptor
            il.Emit(OpCodes.Ldarg_0);
            il.Emit(OpCodes.Ldfld, InterceptorsField);
            il.Emit(OpCodes.Ldloc_2);
            il.Emit(OpCodes.Call, InterceptorRegistry.MethodOfGetInterceptor);

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
        protected virtual MethodBuilder DefineNonInterceptableMethod(MethodInfo methodInfo, MethodAttributes attributes)
        {
            Guard.ArgumentNotNull(methodInfo, nameof(methodInfo));
            methodInfo = Interceptors.GetTargetMethod(methodInfo);
            var parameters = methodInfo.GetParameters();                         
            var parameterTypes = parameters.Select(it => it.ParameterType).ToArray();
            var methodBuilder = TypeBuilder.DefineMethod(methodInfo.Name, attributes, methodInfo.ReturnType, parameterTypes);
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
            il.Emit(OpCodes.Ldfld, TargetField);
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
        protected virtual MethodAttributes? GetMethodAttributes(MethodInfo methodInfo)
        {
            Guard.ArgumentNotNull(methodInfo, nameof(methodInfo));
            if (TypeToIntercept.IsInterface)
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

        /// <summary>
        /// Defines the "SetInterceptors" method to set the interceptors of type <see cref="InterceptorRegistry"/>.
        /// </summary>
        /// <param name="attributes">The attributes applied to the generated method.</param>
        /// <returns>The <see cref="MethodBuilder"/> representing the generated method.</returns>
        protected virtual MethodBuilder DefineSetInterceptorsMethod(MethodAttributes attributes)
        {
            var methodBuilder = TypeBuilder.DefineMethod("SetInterceptors", attributes, typeof(void), new Type[] { typeof(InterceptorRegistry) });
            var il = methodBuilder.GetILGenerator();
            il.Emit(OpCodes.Ldarg_0);                        
            il.Emit(OpCodes.Ldarg_1);
            il.Emit(OpCodes.Stfld, InterceptorsField);
            il.Emit(OpCodes.Ret);
            return methodBuilder;
        }
        #endregion

        #region Private Methods

        private void GenerateForInterface()
        {
            var attributes = MethodAttributes.Public | MethodAttributes.HideBySig | MethodAttributes.NewSlot | MethodAttributes.Virtual | MethodAttributes.Final;
            DefineConstructorForImplementationClass();

            //Methods
            foreach (var methodInfo in TypeToIntercept.GetMethods().Where(it => !it.IsSpecialName))
            {
                if (Interceptors.Contains(methodInfo))
                {
                    DefineInterceptableMethod(methodInfo, attributes);
                }
                else
                {
                    DefineNonInterceptableMethod(methodInfo, attributes);
                }
            }

            //Properties
            foreach (var property in TypeToIntercept.GetProperties())
            {
                var parameterTypes = property.GetIndexParameters().Select(it => it.ParameterType).ToArray();
                var propertyBuilder = TypeBuilder.DefineProperty(property.Name, property.Attributes, property.PropertyType, parameterTypes);
                var getMethod = property.GetMethod;
                if (null != getMethod)
                {
                    var getMethodBuilder = Interceptors.IsInterceptable(getMethod)
                        ? DefineInterceptableMethod(getMethod, attributes)
                        : DefineNonInterceptableMethod(getMethod, attributes);
                    propertyBuilder.SetGetMethod(getMethodBuilder);
                }
                var setMethod = property.SetMethod;
                if (null != setMethod)
                {
                    var setMethodBuilder = Interceptors.IsInterceptable(setMethod)
                        ? DefineInterceptableMethod(setMethod, attributes)
                        : DefineNonInterceptableMethod(setMethod, attributes);
                    propertyBuilder.SetGetMethod(setMethodBuilder);
                }
            }

            //Events
            foreach (var eventInfo in TypeToIntercept.GetEvents())
            {
                var eventBuilder = TypeBuilder.DefineEvent(eventInfo.Name, eventInfo.Attributes, eventInfo.EventHandlerType);
                eventBuilder.SetAddOnMethod(DefineNonInterceptableMethod(eventInfo.AddMethod, attributes));
                eventBuilder.SetRemoveOnMethod(DefineNonInterceptableMethod(eventInfo.RemoveMethod, attributes));
            }
        }

        private void GenerateForVirtualMethods()
        {
            //Constructor
            DefineConstructorsForSubClass();

            //SetInterceptors method
            DefineSetInterceptorsMethod( MethodAttributes.Public| MethodAttributes.HideBySig| MethodAttributes.Virtual| MethodAttributes.Final);

            //Methods
            foreach (var methodInfo in TypeToIntercept.GetMethods(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic))
            { 
                if (!methodInfo.IsSpecialName && methodInfo.IsOverridable() && Interceptors.Contains(methodInfo) )
                {
                    var attributes = GetMethodAttributes(methodInfo);
                    if (null != attributes)
                    {
                        DefineInterceptableMethod(methodInfo, attributes.Value);
                    }
                }
            }

            //Properties
            foreach (var property in TypeToIntercept.GetProperties(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic))
            {
                PropertyBuilder propertyBuilder = null;

                //Getter
                var getMethod = property.GetMethod;
                if (getMethod !=  null && getMethod.IsOverridable() && Interceptors.Contains(getMethod) )
                {
                    var attributes = GetMethodAttributes(getMethod);
                    if (null != attributes)
                    {
                        var parameterTypes = property.GetIndexParameters().Select(it => it.ParameterType).ToArray();
                        propertyBuilder = TypeBuilder.DefineProperty(property.Name, property.Attributes, property.PropertyType, parameterTypes);
                        var methodBuilder = DefineInterceptableMethod(getMethod, attributes.Value);
                        propertyBuilder.SetGetMethod(methodBuilder);
                    }
                }

                //Setter
                var setMethod = property.SetMethod;
                if (setMethod != null && getMethod.IsOverridable() && Interceptors.Contains(setMethod))
                {
                    var attributes = GetMethodAttributes(setMethod);
                    if (null != attributes)
                    {
                        var parameterTypes = property.GetIndexParameters().Select(it => it.ParameterType).ToArray();
                        propertyBuilder = propertyBuilder?? TypeBuilder.DefineProperty(property.Name, property.Attributes, property.PropertyType, parameterTypes);
                        var methodBuilder = DefineInterceptableMethod(setMethod, attributes.Value);
                        propertyBuilder.SetSetMethod(methodBuilder);
                    }
                }
            }  
        }

        private Dictionary<Type, Type> DefineMethodGenericParameters(MethodBuilder methodBuilder, MethodInfo methodInfo)
        {
           var genericParameters = methodInfo.GetGenericArguments();
           var genericParameterNames = Enumerable.Range(1, genericParameters.Length).Select(it => $"T{it}").ToArray();
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

        private MethodBuilder DefineTargetInvokerMethod(MethodInfo methodInfo)
        {
            methodInfo = Interceptors.GetTargetMethod(methodInfo);
            var methodBuilder = TypeBuilder.DefineMethod($"Invoke{GenerateSurfix()}", MethodAttributes.Private| MethodAttributes.HideBySig, typeof(Task), new Type[] { typeof(InvocationContext) });    
            var genericParameterTypeMap = methodInfo.IsGenericMethod
              ? DefineMethodGenericParameters(methodBuilder, methodInfo)
              : new Dictionary<Type, Type>(); var parameters = methodInfo.GetParameters();
            var parameterTypes = parameters.Select(it => it.ParameterType.GetNonByRefType()).ToArray();

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
            if (TargetField != null)
            {
                il.Emit(OpCodes.Ldfld, TargetField);
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
                il.Emit(OpCodes.Call, genericMethod);
            }
            else
            {
                il.Emit(OpCodes.Call, methodInfo);
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
                default:  { il.Emit(OpCodes.Ldloc_S, index + 1); break; }
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
