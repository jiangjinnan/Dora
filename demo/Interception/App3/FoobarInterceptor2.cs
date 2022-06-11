using Dora.Interception;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace App3
{
    public class FoobarInterceptor2
    {
        public ValueTask InvokeAsync(InvocationContext invocationContext)
        {
            var arg1 = invocationContext.GetArgument<int>("x");
            var arg2 = invocationContext.GetArgument<int>("y");

            invocationContext.SetArgument("x", arg1 + 1);
            invocationContext.SetArgument("y", arg2 + 1);
            return invocationContext.ProceedAsync();
        }
    }
}
