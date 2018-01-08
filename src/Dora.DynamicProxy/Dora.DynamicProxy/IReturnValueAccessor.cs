using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Dora.DynamicProxy
{
    public class ReturnValueAccessor<TResult>
    {
        private readonly InvocationContext _context;
        public ReturnValueAccessor(InvocationContext context)
        {
            _context = Guard.ArgumentNotNull(context, nameof(context));
        }
        public TResult GetReturnValue(Task task)
        {
            return ((Task<TResult>)_context.ReturnValue).Result;
        }
    }  
}
