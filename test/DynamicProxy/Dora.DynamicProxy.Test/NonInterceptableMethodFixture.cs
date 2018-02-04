using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace Dora.DynamicProxy.Test
{
    public class NonInterceptableMethodFixture
    {

        [Fact]
        public async void TestReturnType()
        {
            string flag = "";
            var classGenerator = DynamicProxyClassGenerator.CreateInterfaceGenerator(typeof(IFoobar), InterceptorDecoration.Empty);
            var proxyType = classGenerator.GenerateProxyType();
            var proxy = (IFoobar)Activator.CreateInstance(proxyType, new Foobar(() => flag = "Foobar"), InterceptorDecoration.Empty);

            //Void  
            proxy.Invoke1();
            Assert.Equal("Foobar", flag);

            //String
            flag = "";
            Assert.Equal("Foobar", proxy.Invoke2());
            Assert.Equal("Foobar", flag);

            //Task
            flag = "";
            await proxy.Invoke3();
            Assert.Equal("Foobar", flag);

            //Task<T>
            flag = "";
            Assert.Equal("Foobar", await proxy.Invoke4());
            Assert.Equal("Foobar", flag);
        }

        [Fact]
        public void TestParameterType()
        {
            //General         
            var classGenerator = DynamicProxyClassGenerator.CreateInterfaceGenerator(typeof(ICalculator), InterceptorDecoration.Empty);
            var proxyType = classGenerator.GenerateProxyType();
            var proxy = (ICalculator)Activator.CreateInstance(proxyType, new Calculator(), InterceptorDecoration.Empty);

            Assert.Equal(3, proxy.Add(1, 2)); 

            //ref, out
            double x = 1;
            double y = 2;
            double result;  
            proxy.Substract(ref x, ref y, out result);
            Assert.Equal(-1, result);
        }

        [Fact]
        public void TestGenericType()
        {
            //Foobar<T>()     
            var classGenerator = DynamicProxyClassGenerator.CreateInterfaceGenerator(typeof(ICalculator<int>), InterceptorDecoration.Empty);
            var proxyType = classGenerator.GenerateProxyType();
            var proxy = (ICalculator<int>)Activator.CreateInstance(proxyType, new IntCalculator(), InterceptorDecoration.Empty);
            Assert.Equal(3, proxy.Add(1, 2));   
        }

        [Fact]
        public void TestGenericMethod()
        {                      
            var classGenerator = DynamicProxyClassGenerator.CreateInterfaceGenerator(typeof(ICalculator), InterceptorDecoration.Empty);
            var proxyType = classGenerator.GenerateProxyType();
            var proxy = (ICalculator)Activator.CreateInstance(proxyType, new Calculator(), InterceptorDecoration.Empty);      
            Assert.Equal(2, proxy.Multiply(1, 2));             
        }

        [Fact]
        public void TestProperty()
        {                         
            var classGenerator = DynamicProxyClassGenerator.CreateInterfaceGenerator(typeof(IDataAccessor), InterceptorDecoration.Empty);
            var proxyType = classGenerator.GenerateProxyType();
            var proxy = (IDataAccessor)Activator.CreateInstance(proxyType, new DataAccessor(), InterceptorDecoration.Empty);   
            proxy.Data = "123";
            Assert.Equal("123", proxy.Data); 
        }

        [Fact]
        public void TestIndex()
        {                       
            var classGenerator = DynamicProxyClassGenerator.CreateInterfaceGenerator(typeof(IDataAccessor), InterceptorDecoration.Empty);
            var proxyType = classGenerator.GenerateProxyType();
            var proxy = (IDataAccessor)Activator.CreateInstance(proxyType, new DataAccessor(), InterceptorDecoration.Empty); 
            proxy[0] = "abc";
            Assert.Equal("abc", proxy[0]);
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

        public interface IDataAccessor
        {
            string Data { get; set; }
            string this[int index] { get; set; }

        }

        public class DataAccessor : IDataAccessor
        {
            private string _value;
            public string this[int index]
            {
                get { return _value; }
                set { _value = value; }
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
