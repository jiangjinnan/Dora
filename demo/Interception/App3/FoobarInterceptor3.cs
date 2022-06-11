using Dora.Interception;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace App3
{
    public class FoobarInterceptor3
    {
        public async ValueTask InvokeAsync(InvocationContext invocationContext)
        {           
             await invocationContext.ProceedAsync();
            invocationContext.SetReturnValue(0);
        }
    }
}
