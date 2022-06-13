using Dora.Interception;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace App
{
    public class Calculator
    {
        [Interceptor(typeof(FoobarInterceptor))]
        public virtual int Add(int x, int y) => x + y;
    }
}
