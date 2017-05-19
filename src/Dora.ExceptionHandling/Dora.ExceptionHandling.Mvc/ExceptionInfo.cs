using System;
using System.Collections.Generic;
using System.Text;

namespace Dora.ExceptionHandling.Mvc
{
    public class ExceptionInfo
    {
        public string ExceptionType { get; }
        public  string HelpLink { get; }
        public int HResult { get;  }
        public  string Message { get; }
        public  string Source { get;  }
        public  string StackTrace { get; }
        public ExceptionInfo InnerException { get; }
        public ExceptionInfo(Exception exception, bool includeInnerException)
        {
            Guard.ArgumentNotNull(exception, nameof(exception));
            this.ExceptionType = exception.GetType().AssemblyQualifiedName;
            this.HelpLink = exception.HelpLink;
            this.HResult = exception.HResult;
            this.Message = exception.Message;
            this.Source = exception.Source;
            this.StackTrace = exception.StackTrace;
            if (includeInnerException && exception.InnerException != null)
            {
                this.InnerException = new ExceptionInfo(exception.InnerException, true);
            }
        }
    }
}
