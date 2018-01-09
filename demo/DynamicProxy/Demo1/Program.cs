using Dora.DynamicProxy;
using System;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Threading.Tasks;

namespace Demo1
{
    public class Program
    {
        static void Main()
        {
            InterceptorDelegate interceptor = next => (async context =>
            {
                context.Arguments[0] = (int)context.Arguments[0] + 1;
                await next(context);
            });
            int x = 1;
            int y = 2;
            var method = ReflectionUtility.GetMethod<ICalculator>(_ => _.Substract(ref x, ref y));
            var methodBasedInterceptor = new MethodBasedInterceptorDecoration(method, interceptor);
            var decoration = new InterceptorDecoration(new MethodBasedInterceptorDecoration[] { methodBasedInterceptor }, null);
            //var generator = new InterfaceInterceptingProxyClassGenerator();
            //var proxyType = generator.GenerateProxyClass(typeof(ICalculator), decoration);
            //var proxy = (ICalculator)Activator.CreateInstance(proxyType, new Calculator(), decoration);
            //proxy.Substract(ref x, ref y);

            var proxy = new CalculatorProxy(new Calculator(), decoration);
            proxy.Substract(ref x, ref y);
        }

        //public static void Main(string[] args)
        //{
        //    var assemblyName = new AssemblyName("Dora.DynamicProxy.DynamicAssembly");
        //    var assemblyBuilder = AssemblyBuilder.DefineDynamicAssembly(assemblyName, AssemblyBuilderAccess.Run);
        //    var moduleBuilder = assemblyBuilder.DefineDynamicModule("Dora.DynamicProxy.DynamicAssembly.dll");
        //    var typeBuilder = moduleBuilder.DefineType("Test", TypeAttributes.Public);
        //    var methodBuilder = typeBuilder.DefineMethod("Test", MethodAttributes.Public, typeof(string), new Type[] { typeof(InvocationContext), typeof(Task) });
        //    var il = methodBuilder.GetILGenerator();

        //    var construcotorOfReturnValueAccessor = typeof(ReturnValueAccessor<string>).GetConstructor(new Type[] { typeof(InvocationContext) });
        //    il.DeclareLocal(typeof(ReturnValueAccessor<string>));
        //    il.DeclareLocal(typeof(Func<Task, string>));

        //    il.Emit(OpCodes.Ldarg_1);
        //    il.Emit(OpCodes.Newobj, construcotorOfReturnValueAccessor);
        //    il.Emit(OpCodes.Stloc_0);

        //    var getReturnValueMethod = typeof(ReturnValueAccessor<string>).GetMethod("GetReturnValue", new Type[] { typeof(Task) });
        //    var constructorOfFunc = typeof(Func<Task, string>).GetConstructors().Single();

        //    il.Emit(OpCodes.Ldloc_0);
        //    il.Emit(OpCodes.Ldftn, getReturnValueMethod);
        //    il.Emit(OpCodes.Newobj, constructorOfFunc);
        //    il.Emit(OpCodes.Stloc_1);

        //    var methodOfInvoke = typeof(Func<Task, string>).GetMethod("Invoke");
        //    il.Emit(OpCodes.Ldloc_1);
        //    il.Emit(OpCodes.Ldarg_2);
        //    il.Emit(OpCodes.Callvirt, methodOfInvoke);
        //    il.Emit(OpCodes.Ret);
        //    var type = typeBuilder.CreateType();

        //    //assemblyBuilder.Save(@"Dora.DynamicProxy.DynamicAssembly.dll");

        //    var invocationContext = new DefaultInvocationContext(typeof(Program).GetMethod("Main"), new object(), new object(), new object[] { 1, 2 });
        //    invocationContext.ReturnValue = Task.FromResult("abc");
        //    var instance = Activator.CreateInstance(type);

        //    var method = type.GetMethod("Test");
        //    var result = method.Invoke(instance, new object[] { invocationContext, Task.CompletedTask });
        //}

        //public static void Main(string[] args)
        //{
        //    InterceptorDelegate interceptor = next => (async context =>
        //    {
        //        context.Arguments[0] = 1000;
        //        context.ReturnValue = Task.FromResult("12345");
        //        await Task.CompletedTask;
        //        await next(context);
        //    });

        //    var decoration = new InterceptorDecoration(new MethodBasedInterceptorDecoration[] { new MethodBasedInterceptorDecoration(typeof(IFoobar).GetMethod("Invoke1"), interceptor) }, null);

        //    //var assemblyName = new AssemblyName("Dora.DynamicProxy.DynamicAssembly");
        //    //var ab = AssemblyBuilder.DefineDynamicAssembly(assemblyName, AssemblyBuilderAccess.RunAndCollect);
        //    //var mb = ab.DefineDynamicModule("Dora.DynamicProxy.DynamicAssembly.dll");
        //    var generator = new InterfaceInterceptingProxyClassGenerator();
        //    var type = generator.GenerateProxyClass(typeof(IFoobar), decoration);
        //    //var type = typeof(ReturnValueAccessor<>);
        //    //var invocationContext = new DefaultInvocationContext(typeof(IFoobar).GetMethod("Invoke1"), new object(), new Foobar(), new object[] { 1, 2 });
        //    //invocationContext.ReturnValue = Task.FromResult("abc");
        //    //var instance = (ReturnValueAccessor<string>)Activator.CreateInstance(type.MakeGenericType(typeof(string)), invocationContext);
        //    //Console.WriteLine(instance.GetReturnValue(Task.CompletedTask));
        //    var instance = (IFoobar)Activator.CreateInstance(type, new Foobar(), decoration);

        //    //var method = type.GetMethod("Invoke1_InvokeTarget", BindingFlags.NonPublic| BindingFlags.Instance);
        //    //method.Invoke(instance, new object[] { new DefaultInvocationContext(typeof(IFoobar).GetMethod("Invoke1"), instance, new Foobar(), new object[] { 1, 2 }) });

        //    var x = 1;
        //    var y = 2;
        //    Console.WriteLine(instance.Invoke1(ref x, ref y).Result);

        //    // ab.Save(@"Dora.DynamicProxy.DynamicAssembly.dll");

        //    //var invocationContext = new DefaultInvocationContext(typeof(Program).GetMethod("Main"), new object(), new object(), new object[0]);
        //    //invocationContext.ReturnValue = Task.FromResult("abc");
        //    //var v = (IReturnValueAccessor<string>)Activator.CreateInstance(type.MakeGenericType(typeof(string)), invocationContext);
        //    //var method = type.MakeGenericType(typeof(string)).GetMethod("GetReturnValue");
        //    //Console.WriteLine(v.GetReturnValue(Task.CompletedTask));
        //    //Console.Read();


        //    //var target = new Foobar(); 
        //    //InterceptorDelegate interceptor = next => (async context =>
        //    //{
        //    //    context.Arguments[0] = 1000;
        //    //    context.ReturnValue = Task.FromResult("12345");
        //    //    await Task.CompletedTask;
        //    //    //await next(context);
        //    //});
        //    //var proxy = new FoobarProxy(target, interceptor);
        //    //Console.WriteLine( proxy.Invoke1(1, 2).Result);
        //    //Console.WriteLine("Continue...");
        //    //proxy.Invoke2(1, 2);
        //    //proxy.Invoke4(1, 2);
        //    Console.Read();
        //}
    }

    public interface IFoobar
    {
        Task<string> Invoke1(ref int x, ref int y);
        //Task Invoke2(int x, int y);
        //string Invoke3(int x, int y);
        //void Invoke4(int x, int y);
        //void Invoke5(ref int x, ref int y);
    }

    public class Foobar : IFoobar
    {
        public Task<string> Invoke1(ref int  x, ref int y)
        {
            Console.WriteLine("Foobar.Invoke1...");
            return Task.FromResult($"{x} + {y}");
        }

        //public Task Invoke2(int x, int y)
        //{
        //    Console.WriteLine("Foobar.Invoke2...");
        //    return Task.CompletedTask;
        //}

        //public string Invoke3(int x, int y)
        //{
        //    Console.WriteLine("Foobar.Invoke3...");
        //    return $"{x} + {y}";
        //}

        //public void Invoke4(int x, int y)
        //{
        //    Console.WriteLine("Foobar.Invoke4...");
        //}

        //public void Invoke5(ref int x, ref int y)
        //{
        //    Console.WriteLine("Foobar.Invoke5...");
        //}

    }

    public interface ICalculator
    {
        int Add(int x, int y);
        int Substract(ref int x, ref int y);
    }

    public class CalculatorProxy : ICalculator
    {
        private ICalculator _target;
        private InterceptorDecoration _interceptors;
        public CalculatorProxy(ICalculator target, InterceptorDecoration interceptors)
        {
            _target = target;
            _interceptors = interceptors;
        }

        public int Add(int x, int y)
        {
            throw new NotImplementedException();
        }

        public int Substract(ref int x, ref int y)
        {
            var arguments = new object[2];
            arguments[0] = x;
            arguments[1] = y;
            var method = typeof(ICalculator).GetMethod("Substract");
            var context = new DefaultInvocationContext(method, this, _target, arguments);
            var interceptor = _interceptors.GetInterceptor(method);
            interceptor(this.InvokeTarget4Substract)(context).Wait();

            x = (int)arguments[0];
            y = (int)arguments[1];

            return (int)context.ReturnValue;
        }

        public Task InvokeTarget4Substract(InvocationContext context)
        {
            var arguments = context.Arguments;
            var x = (int)arguments[0];
            var y = (int)arguments[1];
            context.ReturnValue = _target.Substract(ref x, ref y);  
            return Task.CompletedTask;
        }
    }


    public class Calculator : ICalculator
    {
        public int Add(int x, int y)
        {
            return x + y;
        }

        public int Substract(ref int x, ref int y)
        {
            return x - y;
        }
    }

    //public class FoobarProxy : IFoobar
    //{
    //    private IFoobar _target;
    //    private InterceptorDelegate[] _interceptors;
    //    public FoobarProxy(IFoobar target, InterceptorDelegate[] interceptors)
    //    {
    //        _target = target;
    //        _interceptors = interceptors;
    //    }

    //    public Task<string> Invoke1(ref int x, ref int y)
    //    {
    //        var arguments = new object[] { x, y };
    //        var method = typeof(IFoobar).GetMethod("Invoke1");
    //        var context = new DefaultInvocationContext(method, this, _target, arguments);

    //        var interceptor = _interceptors[0];  
    //        var task = interceptor(Invoke1_Invoke_Target)(context);
    //        var returnValueAccessor = new ReturnValueAccessor<string>(context);
    //        Func<Task, string> func = new Func<Task, string>(returnValueAccessor.GetReturnValue);
    //        return task.ContinueWith<string>(func);
    //    }

    //    private Task Invoke1_Invoke_Target(InvocationContext context)
    //    {
    //        var arg1 = (int)context.Arguments[0];
    //        var arg2 = (int)context.Arguments[1];
    //        context.ReturnValue = _target.Invoke1(ref arg1, ref arg2);
    //        return Task.CompletedTask;
    //    }   
    //}
}
