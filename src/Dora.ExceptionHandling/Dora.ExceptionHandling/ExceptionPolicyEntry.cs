using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Dora.ExceptionHandling
{
    public class ExceptionPolicyEntry
    {
        public Type ExceptionType { get; }
        public PostHandlingAction PostHandlingAction { get; }
        public Func<ExceptionContext, Task> ExceptionHandler { get; }
        public ExceptionPolicyEntry(Type exceptionType, PostHandlingAction postHandlingAction, Func<ExceptionContext, Task> exceptionHandler)
        {
            this.ExceptionType = Guard.ArgumentNotAssignableTo<Exception>(exceptionType, nameof(exceptionType));
            this.ExceptionHandler =Guard.ArgumentNotNull(exceptionHandler, nameof(exceptionHandler));
            this.PostHandlingAction = postHandlingAction;
        }
    }
}
