using System;
using System.Collections.Generic;

namespace Dora.ExceptionHandling.Configuration
{
    internal class HandlerConfiguration
    {
        public Type HandlerType { get;  }
        public IList<ArgumentConfiguration> Arguments { get; }
        public HandlerConfiguration(Type handlerType)
        {
            this.HandlerType = handlerType;
            this.Arguments = new List<ArgumentConfiguration>();
        }
    }
}
