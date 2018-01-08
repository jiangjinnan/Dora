using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Text;
using System.Threading.Tasks;

namespace Dora.DynamicProxy
{
    public class InterfaceInterceptingProxyClassGenerator1
    {
        private static ConstructorInfo ConstructorOfObject = typeof(object).GetConstructor(new Type[0]);
        private static MethodInfo GetMethodFromHandleMethod = typeof(MethodBase).GetMethod("GetMethodFromHandle", new Type[] { typeof(RuntimeMethodHandle) });
        private static ConstructorInfo ConstructorOfDefaultInvocationContext = typeof(DefaultInvocationContext).GetConstructors().Single();    
        private static MethodInfo GetArgumentsMethod = typeof(InvocationContext).GetProperty("Arguments", BindingFlags.Instance| BindingFlags.Public).GetMethod;
        private static MethodInfo GetReturnValue = typeof(InvocationContext).GetProperty("ReturnValue", BindingFlags.Instance | BindingFlags.Public).GetMethod;
        private static MethodInfo SetReturnValue = typeof(InvocationContext).GetProperty("ReturnValue", BindingFlags.Instance | BindingFlags.Public).SetMethod;
        private static MethodInfo GetCompletedTask = typeof(Task).GetProperty("CompletedTask").GetMethod;
        private static ConstructorInfo ConstructorOfInterceptDelegate = typeof(InterceptDelegate).GetConstructors().Single();
        private static MethodInfo GetMethodFromHandle = typeof(MethodBase).GetMethod("GetMethodFromHandle",new Type[] { typeof(RuntimeMethodHandle) });
        private static MethodInfo InvokeOfInterceptorDelegate = typeof(InterceptorDelegate).GetMethod("Invoke", new Type[] { typeof(InterceptDelegate) });
        private static MethodInfo InvokeOfInterceptDelegate = typeof(InterceptDelegate).GetMethod("Invoke", new Type[] { typeof(InvocationContext) });
        private static MethodInfo ContiueWith =  (from method in typeof(Task).GetMethods()
                     let returnType = method.ReturnType
                     let parameters = method.GetParameters()
                     where method.Name == "ContinueWith" &&
                      returnType.IsGenericType &&
                      parameters.Length == 1 &&
                      parameters[0].ParameterType.IsGenericType && parameters[0].ParameterType.GetGenericTypeDefinition() == typeof(Func<,>)
                     select method).Single();
        private static MethodInfo FromResult = typeof(Task).GetMethod("FromResult");                                                                

        public Type GenerateInterceptingProxyType(Type @interface, MethodInfo[] interceptableMethods)
        {
            var assemblyName = new AssemblyName("Dora.DynamicProxy.DynamicAssembly");
            var ab = AssemblyBuilder.DefineDynamicAssembly(assemblyName, AssemblyBuilderAccess.Run);
            var mb = ab.DefineDynamicModule("Dora.DynamicProxy.DynamicAssembly.dll");
            //var returnValueAccessorClass = this.DefineReturnValueAccessorClass(mb);
            var typeAttributes = TypeAttributes.Class | TypeAttributes.Public | TypeAttributes.Sealed;
            var tb = mb.DefineType($"{@interface.Name}___DynamicProxy", typeAttributes, null, new Type[] { @interface });
            this.DefineMembers(tb, @interface, interceptableMethods);
            return tb.CreateTypeInfo();
        } 
        private void DefineMembers(TypeBuilder tb, Type @interface, MethodInfo[] interceptableMethods)
        {
            var targetField = tb.DefineField("_target", @interface, FieldAttributes.Private);
            var interceptorsField= tb.DefineField("_interceptors", typeof(InterceptorDelegate[]), FieldAttributes.Private);
            this.DefineConstructor(tb, @interface, targetField, interceptorsField);
            int index = 0;
            foreach (var method in interceptableMethods)
            {
                ImplementInterceptableMethod(tb, method, index++, targetField, interceptorsField);
            }
        }

        private void DefineConstructor(TypeBuilder tb, Type @interface, FieldBuilder targetFiled, FieldBuilder interceptorsField)
        {
            var constructor = tb.DefineConstructor(MethodAttributes.Public, CallingConventions.HasThis, new Type[] { @interface, typeof(InterceptorDelegate[])});
            var il = constructor.GetILGenerator();

            //Invoke object's constructor
            il.Emit(OpCodes.Ldarg_0);
            il.Emit(OpCodes.Call, ConstructorOfObject);

            //Set _target field
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

        private void ImplementInterceptableMethod(
            TypeBuilder tb, 
            MethodInfo method, 
            int index, 
            FieldBuilder targetField, 
            FieldBuilder interceptorsField)
        {
            var parameters = method.GetParameters();
            var invokeTargetMethod = this.DefineInvokeTargetMethod(tb, method, targetField);
            var mb = tb.DefineMethod(method.Name, MethodAttributes.Public| MethodAttributes.Virtual, method.ReturnType, parameters.Select(it => it.ParameterType).ToArray());
            var il = mb.GetILGenerator();

            var handler = il.DeclareLocal(typeof(InterceptDelegate));
            var interceptor = il.DeclareLocal(typeof(InterceptorDelegate));
            var arguments = il.DeclareLocal(typeof(object[]));
            var methodBase = il.DeclareLocal(typeof(MethodBase));
            var invocationContext = il.DeclareLocal(typeof(InvocationContext));  
            var task = il.DeclareLocal(typeof(Task));
            var returnType = method.ReturnType.GetGenericArguments()[0];
            var returnValueAccessor = il.DeclareLocal(typeof(ReturnValueAccessor<>).MakeGenericType(returnType));
            var func = il.DeclareLocal(typeof(Func<,>).MakeGenericType(typeof(Task), returnType));

            EmitLoadConstantInt32(il, parameters.Length);
            il.Emit(OpCodes.Newarr, typeof(object)); 

            for (int index1 = 0; index1 < parameters.Length; index1++)
            {
                var parameter = parameters[index1];
                il.Emit(OpCodes.Dup);
                EmitLoadConstantInt32(il, index1);
                EmitLoadArgument(il, index1);
                EmitBox(il, parameter.ParameterType);
                il.Emit(OpCodes.Stelem_Ref);
            } 
            il.Emit(OpCodes.Stloc, arguments);


            il.Emit(OpCodes.Ldtoken, method);
            il.Emit(OpCodes.Call, GetMethodFromHandleMethod);
            il.Emit(OpCodes.Stloc, methodBase);

            il.Emit(OpCodes.Ldloc, methodBase);
            il.Emit(OpCodes.Ldarg_0);
            il.Emit(OpCodes.Ldarg_0);
            il.Emit(OpCodes.Ldfld, targetField);
            il.Emit(OpCodes.Ldloc, arguments);
            il.Emit(OpCodes.Newobj, ConstructorOfDefaultInvocationContext);
            il.Emit(OpCodes.Stloc, invocationContext);

            il.Emit(OpCodes.Ldarg_0);
            il.Emit(OpCodes.Ldfld, interceptorsField);
            EmitLoadConstantInt32(il, index);
            il.Emit(OpCodes.Ldelem_Ref);
            il.Emit(OpCodes.Stloc, interceptor);

            il.Emit(OpCodes.Ldarg_0);
            il.Emit(OpCodes.Ldftn, invokeTargetMethod);
            il.Emit(OpCodes.Newobj, ConstructorOfInterceptDelegate);
            il.Emit(OpCodes.Stloc, handler);

            il.Emit(OpCodes.Ldloc, interceptor);
            il.Emit(OpCodes.Ldloc, handler);
            il.Emit(OpCodes.Callvirt, InvokeOfInterceptorDelegate);
            il.Emit(OpCodes.Stloc, handler);

            il.Emit(OpCodes.Ldloc, handler);
            il.Emit(OpCodes.Ldloc, invocationContext);
            il.Emit(OpCodes.Callvirt, InvokeOfInterceptDelegate);
            il.Emit(OpCodes.Stloc, task);
                                                                            
            var constructor = typeof(ReturnValueAccessor<>).MakeGenericType(returnType).GetConstructor(new Type[] { typeof(InvocationContext) });
            il.Emit(OpCodes.Ldloc, invocationContext);
            il.Emit(OpCodes.Newobj, constructor);
            il.Emit(OpCodes.Stloc, returnValueAccessor);
                                                        
            il.Emit(OpCodes.Ldloc, returnValueAccessor);
            il.Emit(OpCodes.Ldftn, typeof(ReturnValueAccessor<>).MakeGenericType(returnType).GetMethod("GetReturnValue", new Type[] { typeof(Task) }));
            var constructorOfFunc = typeof(Func<,>).MakeGenericType(typeof(Task), returnType).GetConstructors().Single();
            il.Emit(OpCodes.Newobj, constructorOfFunc);
            il.Emit(OpCodes.Stloc, func);

            il.Emit(OpCodes.Ldloc, task);
            il.Emit(OpCodes.Ldloc, func); 
            il.Emit(OpCodes.Callvirt, ContiueWith.MakeGenericMethod(returnType));  

            il.Emit(OpCodes.Ret);
        }

        public MethodInfo DefineInvokeTargetMethod(
            TypeBuilder tb,
            MethodInfo methodInfo,
            FieldBuilder targetField)
        {
            var mb = tb.DefineMethod($"{methodInfo.Name}_InvokeTarget", MethodAttributes.Private, typeof(Task), new Type[] { typeof(InvocationContext)});
            var parameters = methodInfo.GetParameters();
            var il = mb.GetILGenerator();
            var argumentsOfContext = il.DeclareLocal(typeof(object[]));
            var arguments = parameters.Select(it => il.DeclareLocal(it.ParameterType)).ToArray();
            var x = il.DeclareLocal(typeof(int));
            LocalBuilder returnValue = null;
            if (methodInfo.ReturnType != typeof(void))
            {
                returnValue = il.DeclareLocal(methodInfo.ReturnType);
            }
            il.Emit(OpCodes.Ldarg_1);
            il.Emit(OpCodes.Callvirt, GetArgumentsMethod);
            il.Emit(OpCodes.Stloc, argumentsOfContext);

            for (int index = 0; index < parameters.Length; index++)
            {
                var parameter = parameters[index];
                il.Emit(OpCodes.Ldloc, argumentsOfContext);
                EmitLoadConstantInt32(il, index);
                il.Emit(OpCodes.Ldelem_Ref);
                EmitUnboxOrCast(il, parameter.ParameterType);  
                il.Emit(OpCodes.Stloc, arguments[index]);  
            }

            il.Emit(OpCodes.Ldarg_0);
            il.Emit(OpCodes.Ldfld, targetField);
            for (int index = 0; index < parameters.Length; index++)
            {
                il.Emit(OpCodes.Ldloc, arguments[index]);
            }
            il.Emit(OpCodes.Callvirt, methodInfo);
            if (methodInfo.ReturnType != typeof(void))
            {
                il.Emit(OpCodes.Stloc, returnValue);
                il.Emit(OpCodes.Ldarg_1);
                il.Emit(OpCodes.Ldloc, returnValue);
                EmitBox(il, methodInfo.ReturnType);
                il.Emit(OpCodes.Callvirt, SetReturnValue);
            }
            il.Emit(OpCodes.Call, GetCompletedTask);
            il.Emit(OpCodes.Ret);
            return mb;  
        }

        private MemberInfo DefineMethodToGetResultValue(TypeBuilder tb, MethodBase method, Type returnType, LocalBuilder invocationContext)
        {
            var type = returnType.GetGenericArguments()[0];
            var mb = tb.DefineMethod($"{method.Name}__Return", MethodAttributes.Private, type, new Type[] { typeof(Task) });
            var il = mb.GetILGenerator();

            il.Emit(OpCodes.Ldarg_0);
            //il.Emit(OpCodes.ldf);
            return null; 
        }

        private static void EmitLoadArgument(ILGenerator il, int index)
        {
            if (index < LoadArgsOpcodes.Length)
            {
                il.Emit(LoadArgsOpcodes[index]);
            }
            else
            {
                il.Emit(OpCodes.Ldarg, index + 1);
            }
        }
        private static void EmitLoadConstantInt32(ILGenerator il, int number)
        {
            if (number <= 8)
            {
                il.Emit(LoadConstantInt32Opcodes[number]);
            }
            else
            {
                il.Emit(OpCodes.Ldc_I4, number);
            }
        }

        private static readonly OpCode[] LoadArgsOpcodes =
       {
            OpCodes.Ldarg_1,
            OpCodes.Ldarg_2,
            OpCodes.Ldarg_3
        };

        private static readonly OpCode[] LoadConstantInt32Opcodes =
       {
            OpCodes.Ldc_I4_0,
            OpCodes.Ldc_I4_1,
            OpCodes.Ldc_I4_2,
            OpCodes.Ldc_I4_3,
            OpCodes.Ldc_I4_4,
            OpCodes.Ldc_I4_5,
            OpCodes.Ldc_I4_6,
            OpCodes.Ldc_I4_7,
            OpCodes.Ldc_I4_8,
        };

        private static void EmitBox(ILGenerator il, Type typeOnStack)
        {
            if (typeOnStack.IsValueType || typeOnStack.IsGenericParameter)
            {
                il.Emit(OpCodes.Box, typeOnStack);
            }
        }

        private static void EmitUnboxOrCast(ILGenerator il, Type targetType)
        {
            if (targetType.IsValueType || targetType.IsGenericParameter)
            {
                il.Emit(OpCodes.Unbox_Any, targetType);
            }
            else
            {
                il.Emit(OpCodes.Castclass, targetType);
            }
        }
    }
}
