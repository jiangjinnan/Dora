using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Text;
using System.Threading.Tasks;

namespace Dora.DynamicProxy
{
    public class InterfaceInterceptingProxyClassGenerator
    {
        public Type GenerateProxyClass(Type @interface, InterceptorDecoration interceptorDecoration)
        {
            Guard.ArgumentNotNull(@interface, nameof(@interface));
            Guard.ArgumentNotNull(interceptorDecoration, nameof(interceptorDecoration));

            var assemblyName = new AssemblyName($"AssemblyFor{@interface.Name}{GenerateSurfix()}");
            var assemblyBuilder = AssemblyBuilder.DefineDynamicAssembly(assemblyName, AssemblyBuilderAccess.Run);
            var moduleBuilder = assemblyBuilder.DefineDynamicModule($"{assemblyName}.dll");
            var typeBuilder = moduleBuilder.DefineType($"{@interface.Name}{GenerateSurfix()}", TypeAttributes.Public, typeof(object), new Type[] { @interface });
            if (@interface.IsGenericType)
            {
                DefineGenericParameters(@interface, typeBuilder);
            }
            var targetField = typeBuilder.DefineField("_target", @interface, FieldAttributes.Private);
            var interceptorsField = typeBuilder.DefineField("_interceptors", typeof(InterceptorDecoration), FieldAttributes.Private);
            this.DefineConstructor(typeBuilder, @interface, targetField, interceptorsField);
            foreach (var item in interceptorDecoration.MethodBasedInterceptors)
            {
                this.DefineInterceptableMethod(typeBuilder, item.Key, targetField, interceptorsField);
            }

            foreach (var method in @interface.GetMethods())
            {
                if (!interceptorDecoration.Contains(method))
                {
                    this.DefineNonInterceptableMethod(typeBuilder, method, targetField);
                }
            }

            return typeBuilder.CreateTypeInfo();
        }

        private static void DefineGenericParameters(Type @interface, TypeBuilder typeBuilder)
        {
            var genericArguments = @interface.GetGenericArguments();
            var argumentNames = genericArguments.Select(it => it.Name).ToArray();
            var genericParameterBuilders = typeBuilder.DefineGenericParameters(argumentNames);
            for (int index = 0; index < genericArguments.Length; index++)
            {
                var genericArgument = genericArguments[index];
                var genericParameterBuilder = genericParameterBuilders[index];
                genericParameterBuilder.SetGenericParameterAttributes(genericArgument.GenericParameterAttributes);

                var interfaceConstraints = new List<Type>();
                foreach (Type constraint in genericArgument.GetGenericParameterConstraints())
                {
                    if (constraint.IsClass)
                    {
                        genericParameterBuilder.SetBaseTypeConstraint(constraint);
                    }
                    else
                    {
                        interfaceConstraints.Add(constraint);
                    }
                }
                if (interfaceConstraints.Count > 0)
                {
                    genericParameterBuilder.SetInterfaceConstraints(interfaceConstraints.ToArray());
                }
            }
        }

        private void DefineConstructor(TypeBuilder typeBuilder, Type @interface, FieldBuilder targetFiled, FieldBuilder interceptorsField)
        {
            var constructor = typeBuilder.DefineConstructor(MethodAttributes.Public, CallingConventions.HasThis, new Type[] { @interface, typeof(InterceptorDecoration) });
            var il = constructor.GetILGenerator();

            //Call object's constructor.
            il.Emit(OpCodes.Ldarg_0);
            il.Emit(OpCodes.Call, ReflectionUtility.ConstructorOfObject);

            //Set _target filed
            il.Emit(OpCodes.Ldarg_0);
            il.Emit(OpCodes.Ldarg_1);
            il.Emit(OpCodes.Stfld, targetFiled);

            //Set _interceptors field
            il.Emit(OpCodes.Ldarg_0);
            il.Emit(OpCodes.Ldarg_2);
            il.Emit(OpCodes.Stfld, interceptorsField);

            //Return
            il.Emit(OpCodes.Ret);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="typeBuilder"></param>
        /// <param name="methodInfo"></param>
        /// <param name="targetField"></param>
        /// <returns></returns>
        /// <example>
        /// <code >
        ///  private Task Add_Invoke_Target(InvocationContext context)
        ///  {
        ///    var arg1 = (int)context.Arguments[0];
        ///    var arg2 = (int)context.Arguments[1];
        ///    context.ReturnValue = _target.Add(arg1, arg2);
        ///    return Task.CompletedTask;
        /// }
        /// </code>
        /// </example>
        private MethodInfo DefineInvokeTargetMethod(
            TypeBuilder typeBuilder,
            MethodInfo methodInfo,
            FieldBuilder targetField)
        {
            var mb = typeBuilder.DefineMethod($"{methodInfo.Name}_Invoke_Target", MethodAttributes.Private, typeof(Task), new Type[] { typeof(InvocationContext) });
            var parameters = methodInfo.GetParameters();
            var il = mb.GetILGenerator();

            //InvocationContext.Arguments
            il.DeclareLocal(typeof(object[]));
            if (methodInfo.ReturnType != typeof(void))
            {
                il.DeclareLocal(methodInfo.ReturnType);
            }
            var arguments = parameters.Select(it => il.DeclareLocal(
                it.ParameterType.IsByRef 
                ? it.ParameterType.GetNonByRefType()
                : it.ParameterType)).ToArray(); 

            //Load and store InvocationContext.Arguments. 
            il.Emit(OpCodes.Ldarg_1);
            il.Emit(OpCodes.Callvirt, ReflectionUtility.ArgumentsPropertyOfInvocationContext.GetMethod);
            il.Emit(OpCodes.Stloc_0);

            //Load and store all arguments
            for (int index = 0; index < parameters.Length; index++)
            {
                var parameter = parameters[index];
                il.Emit(OpCodes.Ldloc_0);
                il.EmitLoadConstantInt32(index);
                il.Emit(OpCodes.Ldelem_Ref);
                il.EmitUnboxOrCast(parameter.ParameterType);
                il.Emit(OpCodes.Stloc, arguments[index]);
            }

            //Invoke target method.
            il.Emit(OpCodes.Ldarg_0);
            il.Emit(OpCodes.Ldfld, targetField);
            for (int index = 0; index < parameters.Length; index++)
            {
                //TODO: Consider ref/out parameter
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
            il.Emit(OpCodes.Callvirt, methodInfo);

            //Save return value to InvocationContext.ReturnValue
            if (methodInfo.ReturnType != typeof(void))
            {
                il.Emit(OpCodes.Stloc_1);
                il.Emit(OpCodes.Ldarg_1);
                il.Emit(OpCodes.Ldloc_1);
                il.EmitBox(methodInfo.ReturnType);
                il.Emit(OpCodes.Callvirt, ReflectionUtility.ReturnValueOfInvocationContext.SetMethod);
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
                        il.EmitBox(parameter.ParameterType);
                        il.Emit(OpCodes.Stelem_Ref);
                    }
                }
            }

            //return Task.CompletedTask
            il.Emit(OpCodes.Call, ReflectionUtility.CompletedTaskOfTask.GetMethod);   
          
            il.Emit(OpCodes.Ret);
            return mb;
        }

        private void DefineInterceptableMethod(
         TypeBuilder typeBuilder,
         MethodInfo method,
         FieldBuilder targetField,
         FieldBuilder interceptorsField)
        {
            var parameters = method.GetParameters();
            var invokeTargetMethod = this.DefineInvokeTargetMethod(typeBuilder, method, targetField);
            var parameterTypes = parameters.Select(it => it.ParameterType).ToArray();
            var methodBuilder = typeBuilder.DefineMethod(method.Name, MethodAttributes.Public | MethodAttributes.Virtual, method.ReturnType, parameterTypes);
            methodBuilder.SetParameters(parameters.Select(it => it.ParameterType).ToArray());
            for (int index = 0; index < parameters.Length; index++)
            {
                var parameter = parameters[index];
                methodBuilder.DefineParameter(index + 1, parameter.Attributes, parameter.Name);
            } 

            var il = methodBuilder.GetILGenerator();

            var handler = il.DeclareLocal(typeof(InterceptDelegate));
            var interceptor = il.DeclareLocal(typeof(InterceptorDelegate));
            var arguments = il.DeclareLocal(typeof(object[]));
            var methodBase = il.DeclareLocal(typeof(MethodBase));
            var invocationContext = il.DeclareLocal(typeof(InvocationContext));
            var task = il.DeclareLocal(typeof(Task));
            var returnType = method.ReturnTaskOfResult()? method.ReturnType.GetGenericArguments()[0]: method.ReturnType;

            LocalBuilder returnValueAccessor = null;
            LocalBuilder func = null;
            if (method.ReturnType != typeof(void))
            {
                returnValueAccessor = il.DeclareLocal(typeof(ReturnValueAccessor<>).MakeGenericType(returnType));
                func = il.DeclareLocal(typeof(Func<,>).MakeGenericType(typeof(Task), returnType));
            } 

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
            il.Emit(OpCodes.Stloc, arguments);

            //Load and store current method
            il.Emit(OpCodes.Ldtoken, method);
            il.Emit(OpCodes.Call, ReflectionUtility.GetMethodFromHandleMethodOfMethodBase);
            il.Emit(OpCodes.Stloc, methodBase);

            //Create and store DefaultInvocationContext
            il.Emit(OpCodes.Ldloc, methodBase);
            il.Emit(OpCodes.Ldarg_0);
            il.Emit(OpCodes.Ldarg_0);
            il.Emit(OpCodes.Ldfld, targetField);
            il.Emit(OpCodes.Ldloc, arguments);
            il.Emit(OpCodes.Newobj, ReflectionUtility.ConstructorOfDefaultInvocationContext);
            il.Emit(OpCodes.Stloc, invocationContext);

            //Get and store current method specific interceptor
            il.Emit(OpCodes.Ldarg_0);
            il.Emit(OpCodes.Ldfld, interceptorsField);
            il.Emit(OpCodes.Ldloc, methodBase);
            il.Emit(OpCodes.Callvirt, InterceptorDecoration.MethodOfgetInterceptor);
            il.Emit(OpCodes.Stloc, interceptor);

            //Create and store handler to invoke target method
            il.Emit(OpCodes.Ldarg_0);
            il.Emit(OpCodes.Ldftn, invokeTargetMethod);
            il.Emit(OpCodes.Newobj, ReflectionUtility.ConstructorOfInterceptDelegate);
            il.Emit(OpCodes.Stloc, handler);

            //Invoke the interceptor and store the result (an InterceptDelegate object) as handler. 
            il.Emit(OpCodes.Ldloc, interceptor);
            il.Emit(OpCodes.Ldloc, handler);
            il.Emit(OpCodes.Callvirt, ReflectionUtility.InvokeMethodOfInterceptorDelegate);
            il.Emit(OpCodes.Stloc, handler);

            //Invoke the the final handler and store the returned Task
            il.Emit(OpCodes.Ldloc, handler);
            il.Emit(OpCodes.Ldloc, invocationContext);
            il.Emit(OpCodes.Callvirt, ReflectionUtility.InvokeMethodOfInterceptDelegate);
            il.Emit(OpCodes.Stloc, task); 

            //When return Task<TResult>
            if (method.ReturnTaskOfResult())
            {
                //Create and store ReturnValueAccessor<Return>
                il.Emit(OpCodes.Ldloc, invocationContext);
                il.Emit(OpCodes.Newobj, ReflectionUtility.GetConstructorOfRetureValueAccessor(returnType));
                il.Emit(OpCodes.Stloc, returnValueAccessor);

                //Create a Func<Task, TReturn> to represent the ReturnValueAccessor<Return>.GetReturnValue
                il.Emit(OpCodes.Ldloc, returnValueAccessor);
                il.Emit(OpCodes.Ldftn, ReflectionUtility.GetMethodsOfGetReturnValue(returnType));
                il.Emit(OpCodes.Newobj, ReflectionUtility.GetConstructorOfFuncOfTaskAndReturnValue(returnType));
                il.Emit(OpCodes.Stloc, func);

                //Invoke handler's ContinueWith 
                il.Emit(OpCodes.Ldloc, task);
                il.Emit(OpCodes.Ldloc, func);
                il.Emit(OpCodes.Callvirt, ReflectionUtility.GetMethodOfContiueWithMethodOfTask(returnType));
                il.Emit(OpCodes.Ret);
                return;
            }

            //When return Task
            if (method.ReturnTask())
            {
                il.Emit(OpCodes.Ldloc, task);
                il.Emit(OpCodes.Ret);
                return;
            }  
            
            il.Emit(OpCodes.Ldloc, task);
            il.Emit(OpCodes.Callvirt, ReflectionUtility.WaitMethodOfTask);

            if (parameters.Any(it => it.ParameterType.IsByRef))
            {
                for (int index = 0; index < parameters.Length; index++)
                {
                    var parameter = parameters[index];
                    if (parameter.ParameterType.IsByRef)
                    {
                        il.EmitLoadArgument(index);
                        il.Emit(OpCodes.Ldloc, arguments);
                        il.EmitLoadConstantInt32(index);
                        il.Emit(OpCodes.Ldelem_Ref);
                        il.EmitUnboxOrCast(parameter.ParameterType);
                        il.EmitStInd(parameter.ParameterType);   
                    }
                }
            }

            // When return void
            if (method.ReturnVoid())
            {
                il.Emit(OpCodes.Ret);
            }  
            
            il.Emit(OpCodes.Ldloc, invocationContext);
            il.Emit(OpCodes.Callvirt, ReflectionUtility.ReturnValueOfInvocationContext.GetMethod);
            il.EmitUnboxOrCast(method.ReturnType);
            il.Emit(OpCodes.Ret);
        }

        private void DefineNonInterceptableMethod(
              TypeBuilder typeBuilder,
              MethodInfo method,
              FieldBuilder targetField
            )
        {
            var parameters = method.GetParameters();
            var invokeTargetMethod = this.DefineInvokeTargetMethod(typeBuilder, method, targetField);
            var parameterTypes = parameters.Select(it => it.ParameterType).ToArray();
            var methodBuilder = typeBuilder.DefineMethod(method.Name, MethodAttributes.Public | MethodAttributes.Virtual, method.ReturnType, parameterTypes);
            for (int index = 0; index < parameters.Length; index++)
            {
                var parameter = parameters[index];
                methodBuilder.DefineParameter(index, parameter.Attributes, parameter.Name);
            } 
            var il = methodBuilder.GetILGenerator();

            il.Emit(OpCodes.Ldarg_0);
            il.Emit(OpCodes.Ldfld, targetField);
            for (int index = 0; index < parameters.Length; index++)
            {
                var parameter = parameters[index];
                il.EmitLoadArgument(index); 
            }
            il.Emit(OpCodes.Callvirt, method);
            il.Emit(OpCodes.Ret);
        }


        private static string GenerateSurfix()
        {
            return Guid.NewGuid().ToString().Replace("-", "");
        }
    }
}
