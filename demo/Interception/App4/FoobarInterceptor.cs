using Dora.Interception;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace App4
{
    public class FooInterceptor
    {
        public async ValueTask InvokeAsync(InvocationContext invocationContext)
        {
            Console.WriteLine("FooInterceptor-Before");
            await invocationContext.ProceedAsync();
            Console.WriteLine("FooInterceptor-After");
        }
    }

    public class BarInterceptor
    {
        public async ValueTask InvokeAsync(InvocationContext invocationContext)
        {
            Console.WriteLine("BarInterceptor-Before");
            await invocationContext.ProceedAsync();
            Console.WriteLine("BarInterceptor-After");
        }
    }
}
