using System;
using System.Collections.Generic;
using System.Text;

namespace Dora.ExceptionHandling
{    
    public class ExceptionHandlingException : Exception
    {
        public ExceptionHandlingException() { }
        public ExceptionHandlingException(string message) : base(message) { }
        public ExceptionHandlingException(string message, Exception inner) : base(message, inner) { }
    }
}
