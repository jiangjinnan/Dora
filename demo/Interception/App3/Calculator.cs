using Dora.Interception;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace App3
{
    public class Calculator
    {
        [Interceptor(typeof(FoobarInterceptor3))]
        public virtual int Add(int x, int y) => x + y;
    }
}
