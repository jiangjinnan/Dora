using System;
using System.Threading.Tasks;
using Xunit;

namespace Dora.DynamicProxy.Test
{
    public class ReflectionUtilityFixture
    {
        [Fact]
        public  void GetConstructor()
        {
            Assert.Same(typeof(Foobar).GetConstructor(Type.EmptyTypes), ReflectionUtility.GetConstructor<Foobar>(() => new Foobar()));
        }

        [Fact]
        public void GetMethod()
        {
            Assert.Same(typeof(Foobar).GetMethod("Invoke1", Type.EmptyTypes), ReflectionUtility.GetMethod<Foobar>(_ => _.Invoke1()));
            Assert.Same(typeof(Foobar).GetMethod("Invoke2", Type.EmptyTypes), ReflectionUtility.GetMethod<Foobar>(_ => Foobar.Invoke2()));
        }

        [Fact]
        public void GetProperty()
        {
            Assert.Same(typeof(Foobar).GetProperty("Value1", Type.EmptyTypes), ReflectionUtility.GetProperty<Foobar, object>(_ => _.Value1));
            Assert.Same(typeof(Foobar).GetProperty("Value2", Type.EmptyTypes), ReflectionUtility.GetProperty<Foobar, object>(_ => Foobar.Value2));
        }

        public class Foobar
        {
            public void Invoke1() { }
            public static void Invoke2() { }     
            public object Value1 { get; set; }
            public static  object Value2 { get; set; }
        }
    }  
}
