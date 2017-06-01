using System;
using System.Collections.Generic;
using System.Text;

namespace Dora.ExceptionHandling.Mvc
{
    /// <summary>
    /// 
    /// </summary>
    [AttributeUsage( AttributeTargets.Class| AttributeTargets.Method)]
    public class HandlerActionAttribute: Attribute
    {
        /// <summary>
        /// 
        /// </summary>
        public string HandlerAction { get; }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="handlerAction"></param>
        public HandlerActionAttribute(string handlerAction)
        {
            this.HandlerAction = Guard.ArgumentNotNullOrWhiteSpace(handlerAction, nameof(handlerAction));
        }
    }
}
