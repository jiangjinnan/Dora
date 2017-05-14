using Dora.Interception;
using System;
using System.Collections.Generic;
using System.Text;

namespace Dora.ExceptionHandling
{
    public static class ExceptionContextExtensions
    {
        private const string KeyOfInvocationContext = "Dora.Interception.InvocationContext";
        public static ExceptionContext SetInvocationContext(this ExceptionContext exceptionContext, InvocationContext invocationContext)
        {
            Guard.ArgumentNotNull(exceptionContext, nameof(exceptionContext)).Properties[KeyOfInvocationContext] = Guard.ArgumentNotNull(invocationContext, nameof(invocationContext));
            return exceptionContext;
        }

        public static bool  TryGetInvocationContext(this ExceptionContext exceptionContext, out InvocationContext invocationContext)
        {
            invocationContext = null;
            if (!Guard.ArgumentNotNull(exceptionContext, nameof(exceptionContext)).Properties.TryGetValue(KeyOfInvocationContext, out object context))
            {
                return false;
            }
            return (invocationContext = (context as InvocationContext)) != null;
        }
    }
}