using System;

namespace Dora.ExceptionHandling.Mvc
{
    /// <summary>
    /// This attribute is decorated with controller class or action method to specify handler action name.
    /// </summary>
    [AttributeUsage( AttributeTargets.Class| AttributeTargets.Method)]
    public class HandlerActionAttribute: Attribute
    {
        /// <summary>
        /// The name of handler action.
        /// </summary>
        public string HandlerAction { get; }

        /// <summary>
        /// Creates a new <see cref="HandlerActionAttribute"/>.
        /// </summary>
        /// <param name="handlerAction">The name of handler action.</param>
        /// <exception cref="ArgumentNullException">The specified <paramref name="handlerAction"/> is null.</exception>
        /// <exception cref="ArgumentException">The specified <paramref name="handlerAction"/> is a white space string.</exception>
        public HandlerActionAttribute(string handlerAction)
        {
            this.HandlerAction = Guard.ArgumentNotNullOrWhiteSpace(handlerAction, nameof(handlerAction));
        }
    }
}
