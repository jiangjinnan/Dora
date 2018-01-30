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
    /// Generator to generator interceptable dynamic proxy class.
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
        /// Gets the <see cref="InterceptorDecoration"/> representing the type members decorated with interceptors.
        /// </summary>
        /// <value>
        /// The <see cref="InterceptorDecoration"/> representing the type members decorated with interceptors.
        /// </value>
        public InterceptorDecoration Interceptors { get; }


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
        /// <param name="interceptors">The <see cref="InterceptorDecoration"/> representing the type members decorated with interceptors.</param>
        /// <exception cref="ArgumentNullException">Specified <paramref name="typeToIntercept"/> is null.</exception>    
        /// <exception cref="ArgumentNullException">Specified <paramref name="interceptors"/> is null.</exception>
        private DynamicProxyClassGenerator(Type typeToIntercept, InterceptorDecoration interceptors)
        {
            this.TypeToIntercept = Guard.ArgumentNotNull(typeToIntercept, nameof(typeToIntercept));
            this.Interceptors = Guard.ArgumentNotNull(interceptors, nameof(interceptors));

            var assemblyName = new AssemblyName($"AssemblyFor{typeToIntercept.Name}{GenerateSurfix()}");
            var assemblyBuilder = AssemblyBuilder.DefineDynamicAssembly(assemblyName, AssemblyBuilderAccess.Run);
            this.ModuleBuilder = assemblyBuilder.DefineDynamicModule($"{assemblyName}.dll");

            if (this.TypeToIntercept.IsInterface)
            {
                this.TypeBuilder = this.ModuleBuilder.DefineType($"{typeToIntercept.Name}{GenerateSurfix()}", TypeAttributes.Public, typeof(object), new Type[] { typeToIntercept });
                this.TargetField = this.TypeBuilder.DefineField("_target", typeToIntercept, FieldAttributes.Private);
            }
            else
            {
                this.TypeBuilder = this.ModuleBuilder.DefineType($"{typeToIntercept.Name}{GenerateSurfix()}", TypeAttributes.Public, typeToIntercept, new Type[] { typeof(IInterceptorsInitializer) });
            }
            this.InterceptorsField = this.TypeBuilder.DefineField("_interceptors", typeof(InterceptorDecoration), FieldAttributes.Private);
        }

        #endregion

        #region Public Methods
        /// <summary>
        /// Creates a <see cref="DynamicProxyClassGenerator"/> to generate dynamic proxy class based on interface based interception.
        /// </summary>
        /// <param name="interface">The interface type to intercept.</param>
        /// <param name="interceptors">The <see cref="InterceptorDecoration"/> representing the type members decorated with interceptors.</param>
        /// <returns>The created <see cref="DynamicProxyClassGenerator"/>.</returns>      
        /// <exception cref="ArgumentNullException">Specified <paramref name="interface"/> is null.</exception>   
        /// <exception cref="ArgumentException">Specified <paramref name="interface"/> is not an interface type.</exception>     
        /// <exception cref="ArgumentNullException">Specified <paramref name="interceptors"/> is null.</exception>
        public static DynamicProxyClassGenerator CreateInterfaceGenerator(Type @interface, InterceptorDecoration interceptors)
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
        /// <param name="interceptors">The <see cref="InterceptorDecoration"/> representing the type members decorated with interceptors.</param>
        /// <returns>The created <see cref="DynamicProxyClassGenerator"/>.</returns>      
        /// <exception cref="ArgumentNullException">Specified <paramref name="type"/> is null.</exception>   
        /// <exception cref="ArgumentException">Specified <paramref name="type"/> is  a sealed type.</exception>     
        /// <exception cref="ArgumentNullException">Specified <paramref name="interceptors"/> is null.</exception>
        public static DynamicProxyClassGenerator CreateVirtualMethodGenerator(Type type, InterceptorDecoration interceptors)
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
            if (this.TypeToIntercept.IsInterface)
            {
                this.GenerateForInterface();
            }
            else
            {
                this.GenerateForVirtualMethods();
            }

            return this.TypeBuilder.CreateTypeInfo();
        }
        #endregion

        #region Protected Methods

        /// <summary>
        /// Defines the constructor for class implementing the specified interface.
        /// </summary>
        /// <returns>The <see cref="ConstructorBuilder"/> representing the generated constructor.</returns>
        protected virtual ConstructorBuilder DefineConstructorForImplementationClass()
        {
            var parameterTypes = new Type[] { this.TypeToIntercept, typeof(InterceptorDecoration) };
            var constructor = this.TypeBuilder.DefineConstructor(MethodAttributes.Public, CallingConventions.HasThis, parameterTypes);
            var il = constructor.GetILGenerator();

            //Call object's constructor.
            il.Emit(OpCodes.Ldarg_0);
            il.Emit(OpCodes.Call, ReflectionUtility.ConstructorOfObject);

            //Set _target filed
            il.Emit(OpCodes.Ldarg_0);
            il.Emit(OpCodes.Ldarg_1);
            il.Emit(OpCodes.Stfld, this.TargetField);

            //Set _interceptors field
            il.Emit(OpCodes.Ldarg_0);
            il.Emit(OpCodes.Ldarg_2);
            il.Emit(OpCodes.Stfld, this.InterceptorsField);

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
            var constructors = this.TypeToIntercept.GetConstructors(BindingFlags.Instance| BindingFlags.Public);
            var constructorBuilders = new ConstructorBuilder[constructors.Length]; 
            for (int index1 = 0; index1 < constructors.Length; index1++)
            {
                var constructor = constructors[index1];
                var parameterTypes = constructor.GetParameters().Select(it => it.ParameterType).ToArray();
                var constructorBuilder = constructorBuilders[index1] = this.TypeBuilder.DefineConstructor(constructor.Attributes, CallingConventions.HasThis, parameterTypes);
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
            var methodBuilder = this.TypeBuilder.DefineMethod(methodInfo.Name, attributes, methodInfo.ReturnType, parameterTypes);
            var genericParameterTypeMap = methodInfo.IsGenericMethod
                ? this.DefineMethodGenericParameters(methodBuilder, methodInfo)
                : new Dictionary<Type, Type>(); 
            for (int index = 0; index < parameters.Length; index++)
            {
                var parameter = parameters[index];
                methodBuilder.DefineParameter(index + 1, parameter.Attributes, parameter.Name);
            } 

            var il = methodBuilder.GetILGenerator();      

            il.DeclareLocal(typeof(InterceptDelegate));         //Loc_0: handler 
            il.DeclareLocal(typeof(InterceptorDelegate));       //Loc_1: interceptor
            il.DeclareLocal(typeof(object[]));                  //Loc_2: arguments   
            il.DeclareLocal(typeof(InvocationContext));         //Loc_3: invocationContext
            il.DeclareLocal(typeof(MethodBase));                //Loc_4: methodBase
            il.DeclareLocal(typeof(Task));                      //Loc_5: task

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
            il.Emit(OpCodes.Stloc_2); 

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
            il.Emit(OpCodes.Stloc_S, 4); 

            //Create and store DefaultInvocationContext
            il.Emit(OpCodes.Ldloc_S, 4);
            il.Emit(OpCodes.Ldarg_0);
            il.Emit(OpCodes.Ldarg_0);
            if (this.TargetField != null)
            {
                il.Emit(OpCodes.Ldfld, this.TargetField);
            }
            il.Emit(OpCodes.Ldloc_2);
            il.Emit(OpCodes.Newobj, ReflectionUtility.ConstructorOfDefaultInvocationContext);
            il.Emit(OpCodes.Stloc_3);    

            //Get and store current method specific interceptor
            il.Emit(OpCodes.Ldarg_0);
            il.Emit(OpCodes.Ldfld, this.InterceptorsField);
            il.Emit(OpCodes.Ldloc_S, 4);
            il.Emit(OpCodes.Call, InterceptorDecoration.MethodOfGetInterceptor);
            il.Emit(OpCodes.Stloc_1);                  

            //Create and store handler to invoke target method
            il.Emit(OpCodes.Ldarg_0);   
            MethodInfo invokeTargetMethod = this.DefineTargetInvokerMethod(methodInfo);  
            if (methodInfo.IsGenericMethod)
            { 
                invokeTargetMethod = invokeTargetMethod.MakeGenericMethod(methodInfo.GetGenericArguments());   
            }  
            il.Emit(OpCodes.Ldftn, invokeTargetMethod);
            il.Emit(OpCodes.Newobj, ReflectionUtility.ConstructorOfInterceptDelegate);
            il.Emit(OpCodes.Stloc_0);

            il.Emit(OpCodes.Ldloc_1);
            il.Emit(OpCodes.Ldloc_0);
            il.Emit(OpCodes.Ldloc_3);
            il.Emit(OpCodes.Call, ReflectionUtility.InvokeHandlerMethod);        
            il.Emit(OpCodes.Stloc_S, 5); 
           
            //When return Task<TResult>
            if (methodInfo.ReturnTaskOfResult())
            {    
                il.Emit(OpCodes.Ldloc_3);
                il.Emit(OpCodes.Call, ReturnValueAccessor.GetTaskOfResultMethodDefinition.MakeGenericMethod(returnType));
                il.Emit(OpCodes.Ret);
                return methodBuilder;
            }

            //When return Task
            if (methodInfo.ReturnTask())
            {
                il.Emit(OpCodes.Ldloc_S, 5);
                il.Emit(OpCodes.Ret);
                return methodBuilder;
            }

            Action storeRefArguments = () =>
            {
                if (parameters.Any(it => it.ParameterType.IsByRef))
                {
                    for (int index = 0; index < parameters.Length; index++)
                    {
                        var parameter = parameters[index];
                        if (parameter.ParameterType.IsByRef)
                        {
                            il.EmitLoadArgument(index);
                            il.Emit(OpCodes.Ldloc_2);
                            il.EmitLoadConstantInt32(index);
                            il.Emit(OpCodes.Ldelem_Ref);
                            il.EmitUnboxOrCast(parameter.ParameterType);
                            il.EmitStInd(parameter.ParameterType);
                        }
                    }
                }
            };

            //Return a general value
            if (!methodInfo.ReturnVoid())
            {
                var returnValue = il.DeclareLocal(returnType);
                il.Emit(OpCodes.Ldloc_3);
                il.Emit(OpCodes.Call, ReturnValueAccessor.GetResultMethodDefinition.MakeGenericMethod(returnType));
                il.Emit(OpCodes.Stloc, returnValue);
                storeRefArguments();
                il.Emit(OpCodes.Ldloc, returnValue);
                il.Emit(OpCodes.Ret);
                return methodBuilder;
            }

            //Return void
            il.Emit(OpCodes.Ldloc_S, 5);
            il.Emit(OpCodes.Callvirt, ReflectionUtility.WaitMethodOfTask);
            storeRefArguments();
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
            methodInfo = this.Interceptors.GetTargetMethod(methodInfo);
            var parameters = methodInfo.GetParameters();                         
            var parameterTypes = parameters.Select(it => it.ParameterType).ToArray();
            var methodBuilder = this.TypeBuilder.DefineMethod(methodInfo.Name, attributes, methodInfo.ReturnType, parameterTypes);
            if (methodInfo.IsGenericMethod)
            {
                this.DefineMethodGenericParameters(methodBuilder, methodInfo);
            }
            for (int index = 0; index < parameters.Length; index++)
            {
                var parameter = parameters[index];
                methodBuilder.DefineParameter(index, parameter.Attributes, parameter.Name);
            } 
            var il = methodBuilder.GetILGenerator();

            il.Emit(OpCodes.Ldarg_0);
            il.Emit(OpCodes.Ldfld, this.TargetField);
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
            if (this.TypeToIntercept.IsInterface)
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
        /// Defines the "SetInterceptors" method to set the interceptors of type <see cref="InterceptorDecoration"/>.
        /// </summary>
        /// <param name="attributes">The attributes applied to the generated method.</param>
        /// <returns>The <see cref="MethodBuilder"/> representing the generated method.</returns>
        protected virtual MethodBuilder DefineSetInterceptorsMethod(MethodAttributes attributes)
        {
            var methodBuilder = this.TypeBuilder.DefineMethod("SetInterceptors", attributes, typeof(void), new Type[] { typeof(InterceptorDecoration) });
            var il = methodBuilder.GetILGenerator();
            il.Emit(OpCodes.Ldarg_0);                        
            il.Emit(OpCodes.Ldarg_1);
            il.Emit(OpCodes.Stfld, this.InterceptorsField);
            il.Emit(OpCodes.Ret);
            return methodBuilder;
        }
        #endregion

        #region Private Methods

        private void GenerateForInterface()
        {
            var attributes = MethodAttributes.Public | MethodAttributes.HideBySig | MethodAttributes.NewSlot | MethodAttributes.Virtual | MethodAttributes.Final;
            this.DefineConstructorForImplementationClass();
            foreach (var methodInfo in this.TypeToIntercept.GetMethods().Where(it => !it.IsSpecialName))
            {
                if (this.Interceptors.Contains(methodInfo))
                {
                    this.DefineInterceptableMethod(methodInfo, attributes);
                }
                else
                {
                    this.DefineNonInterceptableMethod(methodInfo, attributes);
                }
            }
            foreach (var property in this.TypeToIntercept.GetProperties())
            {
                var parameterTypes = property.GetIndexParameters().Select(it => it.ParameterType).ToArray();
                var propertyBuilder = this.TypeBuilder.DefineProperty(property.Name, property.Attributes, property.PropertyType, parameterTypes);
                var getMethod = property.GetMethod;
                if (null != getMethod)
                {
                    var getMethodBuilder = this.Interceptors.IsInterceptable(getMethod)
                        ? this.DefineInterceptableMethod(getMethod, attributes)
                        : this.DefineNonInterceptableMethod(getMethod, attributes);
                    propertyBuilder.SetGetMethod(getMethodBuilder);
                }
                var setMethod = property.SetMethod;
                if (null != setMethod)
                {
                    var setMethodBuilder = this.Interceptors.IsInterceptable(setMethod)
                        ? this.DefineInterceptableMethod(setMethod, attributes)
                        : this.DefineNonInterceptableMethod(setMethod, attributes);
                    propertyBuilder.SetGetMethod(setMethodBuilder);
                }
            }

            foreach (var eventInfo in this.TypeToIntercept.GetEvents())
            {
                var eventBuilder = this.TypeBuilder.DefineEvent(eventInfo.Name, eventInfo.Attributes, eventInfo.EventHandlerType);
                eventBuilder.SetAddOnMethod(this.DefineNonInterceptableMethod(eventInfo.AddMethod, attributes));
                eventBuilder.SetRemoveOnMethod(this.DefineNonInterceptableMethod(eventInfo.RemoveMethod, attributes));
            }
        }

        private void GenerateForVirtualMethods()
        {
            this.DefineConstructorsForSubClass();
            this.DefineSetInterceptorsMethod( MethodAttributes.Public| MethodAttributes.HideBySig| MethodAttributes.Virtual| MethodAttributes.Final);
            foreach (var methodInfo in this.TypeToIntercept.GetMethods(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic))
            { 
                if (!methodInfo.IsSpecialName && methodInfo.IsVirtual && this.Interceptors.Contains(methodInfo))
                {
                    var attributes = this.GetMethodAttributes(methodInfo);
                    if (null != attributes)
                    {
                        this.DefineInterceptableMethod(methodInfo, attributes.Value);
                    }
                }
            }

            foreach (var property in this.TypeToIntercept.GetProperties(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic))
            {
                PropertyBuilder propertyBuilder = null;
                var getMethod = property.GetMethod;
                if (getMethod !=  null && getMethod.IsVirtual && this.Interceptors.Contains(getMethod))
                {
                    var attributes = this.GetMethodAttributes(getMethod);
                    if (null != attributes)
                    {
                        var parameterTypes = property.GetIndexParameters().Select(it => it.ParameterType).ToArray();
                        propertyBuilder = this.TypeBuilder.DefineProperty(property.Name, property.Attributes, property.PropertyType, parameterTypes);
                        var methodBuilder = this.DefineInterceptableMethod(getMethod, attributes.Value);
                        propertyBuilder.SetGetMethod(methodBuilder);
                    }
                }

                var setMethod = property.SetMethod;
                if (setMethod != null && setMethod.IsVirtual && this.Interceptors.Contains(setMethod))
                {
                    var attributes = this.GetMethodAttributes(setMethod);
                    if (null != attributes)
                    {
                        var parameterTypes = property.GetIndexParameters().Select(it => it.ParameterType).ToArray();
                        propertyBuilder = propertyBuilder?? this.TypeBuilder.DefineProperty(property.Name, property.Attributes, property.PropertyType, parameterTypes);
                        var methodBuilder = this.DefineInterceptableMethod(setMethod, attributes.Value);
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
            methodInfo = this.Interceptors.GetTargetMethod(methodInfo);
            var methodBuilder = this.TypeBuilder.DefineMethod("Invoke", MethodAttributes.Private| MethodAttributes.HideBySig, typeof(Task), new Type[] { typeof(InvocationContext) });    
            var genericParameterTypeMap = methodInfo.IsGenericMethod
              ? this.DefineMethodGenericParameters(methodBuilder, methodInfo)
              : new Dictionary<Type, Type>(); var parameters = methodInfo.GetParameters();
            var parameterTypes = parameters.Select(it => it.ParameterType.GetNonByRefType()).ToArray();

            var il = methodBuilder.GetILGenerator();

            //InvocationContext.Arguments
            il.DeclareLocal(typeof(object[]));          //Loc_0: arguments
            var returnType = methodInfo.ReturnType;
            if (methodInfo.ReturnType != typeof(void))
            {
                returnType = genericParameterTypeMap.TryGetValue(methodInfo.ReturnType, out var type)
                    ? type
                    : methodInfo.ReturnType;
                il.DeclareLocal(returnType);          //Loc_1
            }

            var arguments = parameterTypes.Select(it => il.DeclareLocal(it)).ToArray();

            //Load and store InvocationContext.Arguments. 
            il.Emit(OpCodes.Ldarg_1);
            il.Emit(OpCodes.Callvirt, ReflectionUtility.GetMethodOfArgumentsPropertyOfInvocationContext);
            il.Emit(OpCodes.Stloc_0);

            //Load and store all arguments
            for (int index = 0; index < parameters.Length; index++)
            {
                var parameter = parameters[index];
                il.Emit(OpCodes.Ldloc_0);
                il.EmitLoadConstantInt32(index);
                il.Emit(OpCodes.Ldelem_Ref);
                il.EmitUnboxOrCast(parameterTypes[index]);
                il.Emit(OpCodes.Stloc, arguments[index]);
            }

            //Invoke target method.
            il.Emit(OpCodes.Ldarg_0);
            if (this.TargetField != null)
            {
                il.Emit(OpCodes.Ldfld, this.TargetField);
            }
            for (int index = 0; index < parameters.Length; index++)
            {
                var parameter = parameters[index];
                if (parameter.ParameterType.IsByRef)
                {
                    il.Emit(OpCodes.Ldloca, arguments[index]);
                }
                else
                {
                    il.Emit(OpCodes.Ldloc, arguments[index]);
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
            if (returnType != typeof(void))
            {
                il.Emit(OpCodes.Stloc_1);
                il.Emit(OpCodes.Ldarg_1);
                il.Emit(OpCodes.Ldloc_1);
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
                        il.Emit(OpCodes.Ldloc_0);
                        il.EmitLoadConstantInt32(index);
                        il.Emit(OpCodes.Ldloc, arguments[index]);
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

        private Type DefineTargetInvokerType(MethodInfo methodInfo)
        {
            var className = $"TargetInvoker_{methodInfo}{GenerateSurfix()}";
            var typeBuilder = this.ModuleBuilder.DefineType(className, TypeAttributes.Public| TypeAttributes.Sealed);
            var genericParameterTypeMap = methodInfo.IsGenericMethod
               ? this.DefineTypeGenericParameters(typeBuilder, methodInfo)
               : new Dictionary<Type, Type>();                                                       
            var targetField = typeBuilder.DefineField("_target", methodInfo.DeclaringType, FieldAttributes.Private);
            var constructorBuilder = typeBuilder.DefineConstructor(MethodAttributes.Public, CallingConventions.HasThis, new Type[] { methodInfo.DeclaringType });
            var il = constructorBuilder.GetILGenerator();

            il.Emit(OpCodes.Ldarg_0);
            il.Emit(OpCodes.Call, ReflectionUtility.ConstructorOfObject);

            il.Emit(OpCodes.Ldarg_0);   
            il.Emit(OpCodes.Ldarg_1);
            il.Emit(OpCodes.Stfld, targetField);
            il.Emit(OpCodes.Ret);

            var methodBuilder = typeBuilder.DefineMethod("Invoke", MethodAttributes.Public, typeof(Task), new Type[] { typeof(InvocationContext) });
            var parameters = methodInfo.GetParameters();
            var parameterTypes = parameters.Select(it => genericParameterTypeMap.TryGetValue(it.ParameterType, out var type) ? type.GetNonByRefType() : it.ParameterType.GetNonByRefType()).ToArray();

            il = methodBuilder.GetILGenerator();

            //InvocationContext.Arguments
            il.DeclareLocal(typeof(object[]));
            var returnType = methodInfo.ReturnType;
            if (methodInfo.ReturnType != typeof(void))
            {
                returnType = genericParameterTypeMap.TryGetValue(methodInfo.ReturnType, out var type)
                    ? type
                    : methodInfo.ReturnType;
                il.DeclareLocal(returnType);
            }

            var arguments = parameterTypes.Select(it => il.DeclareLocal(it)).ToArray();

            //Load and store InvocationContext.Arguments. 
            il.Emit(OpCodes.Ldarg_1);
            il.Emit(OpCodes.Callvirt, ReflectionUtility.GetMethodOfArgumentsPropertyOfInvocationContext);
            il.Emit(OpCodes.Stloc_0);

            //Load and store all arguments
            for (int index = 0; index < parameters.Length; index++)
            {
                var parameter = parameters[index];
                il.Emit(OpCodes.Ldloc_0);
                il.EmitLoadConstantInt32(index);
                il.Emit(OpCodes.Ldelem_Ref);
                il.EmitUnboxOrCast(parameterTypes[index]);
                il.Emit(OpCodes.Stloc, arguments[index]);
            }  

            //Invoke target method.
            il.Emit(OpCodes.Ldarg_0);
            il.Emit(OpCodes.Ldfld, targetField);
            for (int index = 0; index < parameters.Length; index++)
            {
                var parameter = parameters[index];
                if (parameter.ParameterType.IsByRef)
                {
                    il.Emit(OpCodes.Ldloca, arguments[index]);
                }
                else
                {
                    il.Emit(OpCodes.Ldloc, arguments[index]);
                }
            }
            if (methodInfo.IsGenericMethod)
            {
                var genericMethod = methodInfo.MakeGenericMethod(genericParameterTypeMap.Values.ToArray());
                if (this.TypeToIntercept.IsInterface)
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
                if (this.TypeToIntercept.IsInterface)
                {
                    il.Emit(OpCodes.Callvirt, methodInfo);
                }
                else
                {             
                    il.Emit(OpCodes.Call, methodInfo);
                }
            }

            //Save return value to InvocationContext.ReturnValue
            if (returnType != typeof(void))
            {
                il.Emit(OpCodes.Stloc_1);
                il.Emit(OpCodes.Ldarg_1);
                il.Emit(OpCodes.Ldloc_1);
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
                        il.Emit(OpCodes.Ldloc_0);
                        il.EmitLoadConstantInt32(index);
                        il.Emit(OpCodes.Ldloc, arguments[index]);
                        il.EmitBox(parameterTypes[index]);
                        il.Emit(OpCodes.Stelem_Ref);
                    }
                }
            }

            //return Task.CompletedTask
            il.Emit(OpCodes.Call, ReflectionUtility.GetMethodOfCompletedTaskOfTask);
            il.Emit(OpCodes.Ret);

            return typeBuilder.CreateTypeInfo();
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
}
