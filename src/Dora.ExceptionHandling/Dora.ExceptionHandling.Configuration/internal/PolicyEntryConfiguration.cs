using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Text;

namespace Dora.ExceptionHandling.Configuration
{
    internal class PolicyEntryConfiguration
    {
        public Type ExceptionType { get;}
        public List<HandlerConfiguration> Handlers { get; }
        public PostHandlingAction PostHandlingAction { get;  }
        public PolicyEntryConfiguration(Type exceptionType, PostHandlingAction postHandlingAction)
        {
            this.ExceptionType = exceptionType;
            this.Handlers = new List<HandlerConfiguration>();
            this.PostHandlingAction = postHandlingAction;
        }
    }
}
