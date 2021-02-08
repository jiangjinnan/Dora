using System;

namespace App2
{
    class Program
    {
        static void Main(string[] args)
        {
            var methods = typeof(Bar).GetMethods(System.Reflection.BindingFlags.NonPublic| System.Reflection.BindingFlags.Public| System.Reflection.BindingFlags.Instance);
        }
    }

    public interface IFoobar
    {
        void M3();
    }

    public class Foo
    {
        public virtual void M1() { }
    }

    [Foobar("1","2")]
    public class Bar:Foo, IFoobar
    {
        public virtual void M2() { }

        public override void M1()
        {
            base.M1();
        }

        //public void M3()
        //{
        //    throw new NotImplementedException();
        //}

        void IFoobar.M3()
        { }
    }

    [AttributeUsage( AttributeTargets.Class)]
    public class FoobarAttribute : Attribute
    {
        public FoobarAttribute(params object[] arguments)
        { }
    }
}
