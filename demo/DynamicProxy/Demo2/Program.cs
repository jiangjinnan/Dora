using Dora.DynamicProxy;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Text;
using System.Threading.Tasks;

namespace Demo2
{
    public class Program
    {
        public static void Main(string[] args)
        {
            //var assemblyName = new AssemblyName("Dora.DynamicProxy.DynamicAssembly");
            //var assemblyBuilder = AssemblyBuilder.DefineDynamicAssembly(assemblyName, AssemblyBuilderAccess.RunAndSave);
            //var moduleBuilder = assemblyBuilder.DefineDynamicModule("Dora.DynamicProxy.DynamicAssembly.dll");
            //var typeBuilder = moduleBuilder.DefineType("Test", TypeAttributes.Public);
            //var methodBuilder = typeBuilder.DefineMethod("Test", MethodAttributes.Public, typeof(Func<Task, string>), new Type[] { typeof(InvocationContext), typeof(Task) });
            //var il = methodBuilder.GetILGenerator();

            //var construcotorOfReturnValueAccessor = typeof(ReturnValueAccessor<string>).GetConstructor(new Type[] { typeof(InvocationContext) });
            //il.DeclareLocal(typeof(ReturnValueAccessor<string>));
            //il.DeclareLocal(typeof(Func<Task, string>));
            //il.Emit(OpCodes.Ldarg_1);
            //il.Emit(OpCodes.Newobj, construcotorOfReturnValueAccessor);
            //il.Emit(OpCodes.Stloc_0);

            //var getReturnValueMethod = typeof(ReturnValueAccessor<string>).GetMethod("GetReturnValue", new Type[] { typeof(Task) });
            //var constructorOfFunc = typeof(Func<Task, string>).GetConstructors().Single();
            //il.Emit(OpCodes.Ldarg_0);
            //il.Emit(OpCodes.Ldftn, getReturnValueMethod);
            //il.Emit(OpCodes.Newobj, constructorOfFunc);
            //il.Emit(OpCodes.Stloc_1);

            //var methodOfInvoke = typeof(Func<Task, string>).GetMethod("Invoke");
            //il.Emit(OpCodes.Ldloc_1);
            //il.Emit(OpCodes.Ldarg_2);
            //il.Emit(OpCodes.Callvirt, methodOfInvoke);
            //il.Emit(OpCodes.Ret);
            //var type = typeBuilder.CreateType();

            //assemblyBuilder.Save(@"Dora.DynamicProxy.DynamicAssembly.dll");

            //var invocationContext = new DefaultInvocationContext(typeof(Program).GetMethod("Main"), new object(), new object(), new object[] { 1, 2 });
            //invocationContext.ReturnValue = Task.FromResult("abc");
            //var instance = Activator.CreateInstance(type,)
        }
    }
}
