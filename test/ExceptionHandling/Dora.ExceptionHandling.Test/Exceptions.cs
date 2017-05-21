using System;
using System.Collections.Generic;
using System.Text;

namespace Dora.ExceptionHandling.Test
{
    
    public class FooException : Exception
    {
        public FooException() { }
        public FooException(string message) : base(message) { }
        public FooException(string message, Exception inner) : base(message, inner) { }
    }

    public class BarException : Exception
    {
        public BarException() { }
        public BarException(string message) : base(message) { }
        public BarException(string message, Exception inner) : base(message, inner) { }
    }
}
