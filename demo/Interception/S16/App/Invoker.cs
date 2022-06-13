using Dora.Interception;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace App
{
    public class Invoker
    {
        [Interceptor(typeof(FoobarInterceptor), "Invoke1")]
        public virtual void Invoke1() => Console.WriteLine("Invoker.Invoke1()");

        [Interceptor(typeof(FoobarInterceptor), "Invoke2")]
        public virtual void Invoke2() => Console.WriteLine("Invoker.Invoke2()");
    }
}
