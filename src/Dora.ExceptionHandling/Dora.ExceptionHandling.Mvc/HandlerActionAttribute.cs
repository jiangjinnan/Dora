using System;
using System.Collections.Generic;
using System.Text;

namespace Dora.ExceptionHandling.Mvc
{
    [AttributeUsage( AttributeTargets.Class| AttributeTargets.Method)]
    public class HandlerActionAttribute: Attribute
    {
        public string HandlerAction { get; }
        public HandlerActionAttribute(string handlerAction)
        {
            this.HandlerAction = Guard.ArgumentNotNullOrWhiteSpace(handlerAction, nameof(handlerAction));
        }
    }
}
