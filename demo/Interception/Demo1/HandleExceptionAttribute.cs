using Dora.Interception;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Demo1
{
    public class HandleExceptionAttribute : InterceptorAttribute
    {
        private string _logCategory;
        public HandleExceptionAttribute(string logCategory)
        {
            _logCategory = logCategory;
        }
        public override void Use(IInterceptorChainBuilder builder)
        {
            builder.Use<ExceptionHandler>(this.Order, _logCategory);
        }
    }
}
