using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Text;

namespace Dora.ExceptionHandling.Configuration
{
    internal class PolicyEntryElement
    {
        public Type ExceptionType { get;}
        public List<ExceptionHandlerElement> Handlers { get; }
        public PostHandlingAction PostHandlingAction { get;  }
        public PolicyEntryElement(Type exceptionType, PostHandlingAction postHandlingAction)
        {
            this.ExceptionType = exceptionType;
            this.Handlers = new List<ExceptionHandlerElement>();
            this.PostHandlingAction = postHandlingAction;
        }
    }
}
