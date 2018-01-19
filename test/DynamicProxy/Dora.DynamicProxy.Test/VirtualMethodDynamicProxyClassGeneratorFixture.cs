using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace Dora.DynamicProxy.Test
{
    public class VirutalMethodDynamicProxyClassGeneratorFixture
    {
        private T CreateProxy<T>(InterceptorDelegate interceptor, Expression<Action<T>> methodCall)
        {
            var method = ((MethodCallExpression)methodCall.Body).Method;
            var methodBasedInterceptor = new MethodBasedInterceptorDecoration(method, interceptor);
            var decoration = new InterceptorDecoration(new MethodBasedInterceptorDecoration[] { methodBasedInterceptor }, null);
            var generator = DynamicProxyClassGenerator.CreateVirtualMethodGenerator(typeof(T), decoration);
            var proxyType = generator.GenerateProxyType(); 
            var proxy = (T)Activator.CreateInstance(proxyType);
            ((IInterceptorsInitializer)proxy).SetInterceptors(decoration);
            return proxy;
        }

        private T1 CreateProxy<T1, T2>(InterceptorDelegate interceptor, Expression<Func<T1, T2>> propertyAccessor)
        {
            var property = (PropertyInfo)((MemberExpression)propertyAccessor.Body).Member;
            var propertyBasedInterceptor = new PropertyBasedInterceptorDecoration(property, interceptor, interceptor);
            var decoration = new InterceptorDecoration(null, new PropertyBasedInterceptorDecoration[] { propertyBasedInterceptor });
            var generator = DynamicProxyClassGenerator.CreateVirtualMethodGenerator(typeof(T1), decoration);
            var proxyType = generator.GenerateProxyType();
            var proxy =  (T1)Activator.CreateInstance(proxyType);
            ((IInterceptorsInitializer)proxy).SetInterceptors(decoration);
            return proxy;
        }

        private T CreateProxy<T>( InterceptorDelegate interceptor, MethodInfo method)
        {
            var methodBasedInterceptor = new MethodBasedInterceptorDecoration(method, interceptor);
            var decoration = new InterceptorDecoration(new MethodBasedInterceptorDecoration[] { methodBasedInterceptor }, null);
            var generator = DynamicProxyClassGenerator.CreateVirtualMethodGenerator(typeof(T), decoration);
            var proxyType = generator.GenerateProxyType();
            var proxy = (T)Activator.CreateInstance(proxyType);
            ((IInterceptorsInitializer)proxy).SetInterceptors(decoration);
            return proxy;
        }

        [Fact]
        public async void TestReturnType()
        {
            //Void
            string flag1 = "";
            string flag2 = "";
            InterceptorDelegate interceptor = next => (context => { flag1 = "Foobar"; return next(context); });
            var proxy = this.CreateProxy<Foobar>(interceptor, _ => _.Invoke1());
            proxy.Action = () => flag2 = "Foobar";
            proxy.Invoke1();
            Assert.Equal("Foobar", flag1);
            Assert.Equal("Foobar", flag2);

            //String
            flag1 = "";
            flag2 = "";
            proxy = this.CreateProxy<Foobar>(interceptor, _ => _.Invoke2());
            proxy.Action = () => flag2 = "Foobar";
            Assert.Equal("Foobar", proxy.Invoke2());
            Assert.Equal("Foobar", flag1);
            Assert.Equal("Foobar", flag2);

            //Task
            flag1 = "";
            flag2 = "";
            proxy = this.CreateProxy<Foobar>(interceptor, _ => _.Invoke3());
            proxy.Action = () => flag2 = "Foobar";
            await proxy.Invoke3();
            Assert.Equal("Foobar", flag1);
            Assert.Equal("Foobar", flag2);

            //Task<T>
            flag1 = "";
            flag2 = "";
            proxy = this.CreateProxy<Foobar>(interceptor, _ => _.Invoke4());
            proxy.Action = () => flag2 = "Foobar";
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
            var proxy = this.CreateProxy<Calculator>( interceptor, _ => _.Add(0, 0));
            Assert.Equal(3, proxy.Add(1, 2));
            Assert.Equal("Foobar", flag);

            //ref, out
            double x = 1;
            double y = 2;
            double result;
            flag = "";
            proxy = this.CreateProxy<Calculator>( interceptor, _ => _.Substract(ref x, ref y, out result));
            proxy.Substract(ref x, ref y, out result);
            Assert.Equal(-1, result);
        }

        [Fact]
        public void TestGenericType()
        {
            //Foobar<T>()
            string flag = "";
            InterceptorDelegate interceptor = next => (context => { flag = "Foobar"; return next(context); });
            var proxy = this.CreateProxy<Calculator<int>>( interceptor, _ => _.Add(0, 0));
            Assert.Equal(3, proxy.Add(1, 2));
            Assert.Equal("Foobar", flag);
        }

        [Fact]
        public void TestGenericMethod()
        {
            var method = typeof(Calculator).GetMethod("Multiply");
            string flag = "";
            InterceptorDelegate interceptor = next => (context => { flag = "Foobar"; return next(context); });
            var decoration = new InterceptorDecoration(new MethodBasedInterceptorDecoration[] { new MethodBasedInterceptorDecoration(method, interceptor) }, null);
            var proxy = this.CreateProxy<Calculator>(interceptor, typeof(Calculator).GetMethod("Multiply"));
            Assert.Equal(2, proxy.Multiply(1, 2));
            Assert.Equal("Foobar", flag);
        }

        [Fact]
        public void TestProperty()
        {
            string flag = "";
            InterceptorDelegate interceptor = next => (context => { flag = "Foobar"; return next(context); });
            var proxy = this.CreateProxy<DataAccessor, string>(interceptor, _ => _.Data);
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
            var proxy = this.CreateProxy<DataAccessor>( interceptor, typeof(DataAccessor).GetProperty("Item", typeof(string)).GetMethod);

            Assert.Equal("Foobar", proxy[0]);
            Assert.Equal("Foobar", flag);

            flag = "";
            proxy = this.CreateProxy<DataAccessor>( interceptor, typeof(DataAccessor).GetProperty("Item", typeof(string)).SetMethod);
            proxy[0] = "abc";
            Assert.Equal("Foobar", flag);
        }  

        public class Foobar
        {
            public Action Action;

            public Foobar()
            {
                
            }
            public virtual void Invoke1()
            {
                Action();
            }

            public virtual string Invoke2()
            {
                Action();
                return "Foobar";
            }


            public virtual Task Invoke3()
            {
                Action();
                return Task.CompletedTask;
            }

            public virtual Task<string> Invoke4()
            {
                Action();
                return Task.FromResult("Foobar");
            }
        } 

        public class Calculator 
        {
            public virtual int Add(int x, int y)
            {
                return x + y;
            }

            public virtual T Multiply<T>(T x, T y)
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

            public virtual void Substract(ref double x, ref double y, out double result)
            {
                result = x - y;
            }
        }    

        public class Calculator<T>
        {
            public virtual  int Add(T x, T y)
            {
                return (int)(object)x + (int)(object)y;
            }
        }    

        public class DataAccessor
        {
            public virtual string this[int index]
            {
                get { return "Foobar"; }
                set { }
            }

            public virtual string Data
            {
                get { return "Foobar"; }
                set { }
            }
        }  
    }
}
