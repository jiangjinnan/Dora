using System;
using System.Collections;
using System.Collections.Generic;

namespace Dora.ExceptionHandling.Configuration
{
    internal class ExceptionHandlerElement
    {
        public Type HandlerType { get;  }
        public IList<ArgumentElement> Arguments { get; }
        public ExceptionHandlerElement(Type handlerType)
        {
            this.HandlerType = handlerType;
            this.Arguments = new List<ArgumentElement>();
        }
    }
}
