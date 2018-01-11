using Dora.DynamicProxy;
using System;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Reflection.Emit;
using System.Threading.Tasks;

namespace Demo1
{
    public class Program
    {
        static void Main()
        {
            var method = typeof(ICalculator).GetMethod("Multiply");
            string flag = "";
            InterceptorDelegate interceptor = next => (context => { flag = "Foobar"; return next(context); });
            var decoration = new InterceptorDecoration(new MethodBasedInterceptorDecoration[] { new MethodBasedInterceptorDecoration(method, interceptor) }, null);
            var proxy = CreateProxy<ICalculator>(new Calculator(), interceptor, typeof(ICalculator).GetMethod("Multiply"));
            var result = proxy.Multiply(2, 3);

            //var proxy = new CalcultorProxy(new Calculator(), decoration);
            
        }
        private static T CreateProxy<T>(T target, InterceptorDelegate interceptor, MethodInfo method)
        {
            var methodBasedInterceptor = new MethodBasedInterceptorDecoration(method, interceptor);
            var decoration = new InterceptorDecoration(new MethodBasedInterceptorDecoration[] { methodBasedInterceptor }, null);
            var generator = new InterfaceInterceptingProxyClassGenerator();
            var proxyType = generator.GenerateProxyClass(typeof(T), decoration);
            return (T)Activator.CreateInstance(proxyType, target, decoration);
        }


        private static T CreateProxy<T>(T target, InterceptorDelegate interceptor, Expression<Action<T>> methodCall)
        {
            var method = ((MethodCallExpression)methodCall.Body).Method;
            var methodBasedInterceptor = new MethodBasedInterceptorDecoration(method, interceptor);
            var decoration = new InterceptorDecoration(new MethodBasedInterceptorDecoration[] { methodBasedInterceptor }, null);
            var generator = new InterfaceInterceptingProxyClassGenerator();
            var proxyType = generator.GenerateProxyClass(typeof(T), decoration);
            return (T)Activator.CreateInstance(proxyType, target, decoration);
        }

        public interface IFoobar
        {
            void Invoke1();
            string Invoke2();
            Task Invoke3();
            Task<string> Invoke4();
        }

        public class Foobar : IFoobar
        {
            private Action _action;

            public Foobar(Action action)
            {
                _action = action;
            }
            public void Invoke1()
            {
                _action();
            }

            public string Invoke2()
            {
                _action();
                return "Foobar";
            }


            public Task Invoke3()
            {
                _action();
                return Task.CompletedTask;
            }

            public Task<string> Invoke4()
            {
                _action();
                return Task.FromResult("Foobar");
            }
        }

        public interface ICalculator
        {
            int Add(int x, int y);
            void Substract(ref double x, ref double y, out double result);
            T Multiply<T>(T x, T y);
        }

        public class Calculator : ICalculator
        {
            public int Add(int x, int y)
            {
                return x + y;
            }

            public T Multiply<T>(T x, T y)
            {
                if (typeof(int) == typeof(T))
                {
                    return (T)(object)(Convert.ToInt32(x) * Convert.ToInt32(y));
                }

                if (typeof(double) == typeof(T))
                {
                    return (T)(object)(Convert.ToDouble(x) * Convert.ToDouble(y));
                }

                return default(T);
            }

            public void Substract(ref double x, ref double y, out double result)
            {
                result = x - y;
            }
        }

        public interface ICalculator<T>
        {
            T Add(T x, T y);
        }

        public class IntCalculator : ICalculator<int>
        {
            public int Add(int x, int y)
            {
                return x + y;
            }
        }

        public class CalcultorProxy : ICalculator
        {
            private ICalculator _target;
            private InterceptorDecoration _interceptors;

            public CalcultorProxy(ICalculator target, InterceptorDecoration interceptors)
            {
                _target = target;
                _interceptors = interceptors;
            }

            public int Add(int x, int y)
            {
                throw new NotImplementedException();
            }

            public T Multiply<T>(T x, T y)
            {
                var method = typeof(ICalculator).GetMethod("Multiply");
                var interceptor = _interceptors.GetInterceptor(method);
                var invoker = new TargetInvoker<T>(_target);
                InterceptDelegate next = new InterceptDelegate(invoker.Invoke);
                var arguments = new object[] { x, y };
                var context = new DefaultInvocationContext(method, this, _target, arguments);
                interceptor(next)(context).Wait();
                return (T)context.ReturnValue;
            }

            public void Substract(ref double x, ref double y, out double result)
            {
                throw new NotImplementedException();
            }

            private class TargetInvoker<T>
            {
                private ICalculator _target;

                public TargetInvoker(ICalculator target)
                {
                    _target = target;
                }

                public Task Invoke(InvocationContext context)
                {
                    var arguments = context.Arguments;
                    var x = (T)arguments[0];
                    var y = (T)arguments[1];
                    var result = _target.Multiply(x, y);
                    context.ReturnValue = result;
                    return Task.CompletedTask;
                }
            }
        }
    }
}
