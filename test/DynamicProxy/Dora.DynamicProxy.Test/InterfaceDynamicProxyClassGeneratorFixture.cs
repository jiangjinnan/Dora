using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Reflection;
using System.Reflection.Emit;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace Dora.DynamicProxy.Test
{
    public class InterfaceDynamicProxyClassGeneratorFixture
    {
        private T CreateProxy<T>(T target, InterceptorDelegate interceptor, Expression<Action<T>> methodCall)
        {
            var method = ((MethodCallExpression)methodCall.Body).Method;
            var methodBasedInterceptor = new MethodBasedInterceptorDecoration(method, interceptor);
            var decoration = new InterceptorDecoration(new MethodBasedInterceptorDecoration[] { methodBasedInterceptor }, null);
            var generator =  DynamicProxyClassGenerator.CreateInterfaceGenerator(typeof(T), decoration);
            var proxyType = generator.GenerateProxyType();
            return (T)Activator.CreateInstance(proxyType, target, decoration);
        }

        private T1 CreateProxy<T1, T2>(T1 target, InterceptorDelegate interceptor, Expression<Func<T1, T2>> propertyAccessor)
        { 
            var property = (PropertyInfo)((MemberExpression)propertyAccessor.Body).Member;
            var propertyBasedInterceptor = new PropertyBasedInterceptorDecoration(property, interceptor, interceptor);
            var decoration = new InterceptorDecoration(null, new PropertyBasedInterceptorDecoration[] { propertyBasedInterceptor });
            var generator = DynamicProxyClassGenerator.CreateInterfaceGenerator(typeof(T1), decoration);
            var proxyType = generator.GenerateProxyType();
            return (T1)Activator.CreateInstance(proxyType, target, decoration);
        }

        private T CreateProxy<T>(T target, InterceptorDelegate interceptor, MethodInfo method)
        {
            var methodBasedInterceptor = new MethodBasedInterceptorDecoration(method, interceptor);
            var decoration = new InterceptorDecoration(new MethodBasedInterceptorDecoration[] { methodBasedInterceptor }, null);
            var generator = DynamicProxyClassGenerator.CreateInterfaceGenerator(typeof(T), decoration);
            var proxyType = generator.GenerateProxyType();
            return (T)Activator.CreateInstance(proxyType, target, decoration);
        }

        [Fact]
        public async void TestReturnType()
        {
            //Void
            string flag1 = "";
            string flag2 = "";
            InterceptorDelegate interceptor = next => (context => { flag1 = "Foobar"; return next(context); });
            var proxy = this.CreateProxy<IFoobar>(new Foobar(() => flag2 = "Foobar"), interceptor, _ => _.Invoke1());
            proxy.Invoke1();
            Assert.Equal("Foobar", flag1);
            Assert.Equal("Foobar", flag2);

            //String
            flag1 = "";
            flag2 = "";
            proxy = this.CreateProxy<IFoobar>(new Foobar(() => flag2 = "Foobar"), interceptor, _ => _.Invoke2());
            Assert.Equal("Foobar", proxy.Invoke2());
            Assert.Equal("Foobar", flag1);
            Assert.Equal("Foobar", flag2);

            //Task
            flag1 = "";
            flag2 = "";
            proxy = this.CreateProxy<IFoobar>(new Foobar(() => flag2 = "Foobar"), interceptor, _ => _.Invoke3());
            await proxy.Invoke3();
            Assert.Equal("Foobar", flag1);
            Assert.Equal("Foobar", flag2);

            //Task<T>
            flag1 = "";
            flag2 = "";
            proxy = this.CreateProxy<IFoobar>(new Foobar(() => flag2 = "Foobar"), interceptor, _ => _.Invoke4());
            Assert.Equal("Foobar", await proxy.Invoke4());
            Assert.Equal("Foobar", flag1);
            Assert.Equal("Foobar", flag2);
        }

        [Fact]
        public void TestParameterType()
        {
            //General
            string flag = "";
            InterceptorDelegate interceptor = next => (context => { flag = "Foobar"; return next(context); });
            var proxy = this.CreateProxy<ICalculator>(new Calculator(), interceptor, _ => _.Add(0, 0));
            Assert.Equal(3, proxy.Add(1, 2));
            Assert.Equal("Foobar", flag);

            //ref, out
            double x = 1;
            double y = 2;
            double result;
            flag = "";
            proxy = this.CreateProxy<ICalculator>(new Calculator(), interceptor, _ => _.Substract(ref x, ref y, out result));
            proxy.Substract(ref x, ref y, out result);
            Assert.Equal(-1, result);
        }

        [Fact]
        public void TestGenericType()
        {
            //Foobar<T>()
            string flag = "";
            InterceptorDelegate interceptor = next => (context => { flag = "Foobar"; return next(context); });
            var proxy = this.CreateProxy<ICalculator<int>>(new IntCalculator(), interceptor, _ => _.Add(0, 0));
            Assert.Equal(3, proxy.Add(1, 2));
            Assert.Equal("Foobar", flag);
        }

        [Fact]
        public void TestGenericMethod()
        {
            var method = typeof(ICalculator).GetMethod("Multiply");
            string flag = "";
            InterceptorDelegate interceptor = next => (context => { flag = "Foobar"; return next(context); });
            var decoration = new InterceptorDecoration(new MethodBasedInterceptorDecoration[] { new MethodBasedInterceptorDecoration(method, interceptor) }, null);
            var proxy = this.CreateProxy<ICalculator>(new Calculator(), interceptor, typeof(ICalculator).GetMethod("Multiply"));

            //var proxy = new CalcultorProxy(new Calculator(), decoration);
            Assert.Equal(2, proxy.Multiply(1, 2));
            Assert.Equal("Foobar", flag);
        }

        [Fact]
        public void TestProperty()
        {
            string flag = "";
            InterceptorDelegate interceptor = next => (context => { flag = "Foobar"; return next(context); });
            var proxy = this.CreateProxy<IDataAccessor, string>(new DataAccessor { Data = "Foobar" }, interceptor, _ => _.Data);
            Assert.Equal("Foobar", proxy.Data);

            flag = "";
            proxy.Data = "123";
            Assert.Equal("Foobar", flag);           
        }

        [Fact]
        public void TestIndex()
        {
            string flag = "";
            InterceptorDelegate interceptor = next => (context => { flag = "Foobar"; return next(context); });
            var proxy = this.CreateProxy<IDataAccessor>(new DataAccessor(), interceptor, typeof(IDataAccessor).GetProperty("Item", typeof(string)).GetMethod);
          
            Assert.Equal("Foobar", proxy[0]);
            Assert.Equal("Foobar", flag);

            flag = "";
            proxy = this.CreateProxy<IDataAccessor>(new DataAccessor(), interceptor, typeof(IDataAccessor).GetProperty("Item", typeof(string)).SetMethod);
            proxy[0] = "abc";
            Assert.Equal("Foobar", flag);
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
                if(typeof(int) == typeof(T))
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

        public interface IDataAccessor
        {
            string Data { get; set; }
            string this[int index] { get;set; }
                
        }

        public class DataAccessor : IDataAccessor
        {
            public string this[int index]
            {
                get { return "Foobar"; }
                set { }
            }

            public string Data { get; set; }
        }

        public interface IFire
        {
            event EventHandler Trigger;
        }

        public class Fire : IFire
        {
            public event EventHandler Trigger;
        }
    }
}
