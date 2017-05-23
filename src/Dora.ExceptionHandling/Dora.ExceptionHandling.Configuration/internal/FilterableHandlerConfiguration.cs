using System;
using System.Collections.Generic;
using System.Text;

namespace Dora.ExceptionHandling.Configuration
{
    internal class FilterableHandlerConfiguration : HandlerConfiguration
    {
        public string Filter { get; }
        public FilterableHandlerConfiguration(Type handlerType, string filter) : base(handlerType)
        {
            this.Filter = filter;
        }
    }
}
