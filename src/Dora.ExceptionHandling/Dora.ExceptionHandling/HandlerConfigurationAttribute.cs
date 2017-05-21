using System;
using System.Collections.Generic;
using System.Text;

namespace Dora.ExceptionHandling
{
    /// <summary>
    /// 
    /// </summary>
    public class HandlerConfigurationAttribute: Attribute
    {
        /// <summary>
        /// 
        /// </summary>
        public Type HandlerConfigurationType { get; }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="handlerConfigurationType"></param>
        public HandlerConfigurationAttribute(Type handlerConfigurationType)
        {
            this.HandlerConfigurationType = Guard.ArgumentAssignableTo<ExceptionHandlerConfiguration>(handlerConfigurationType, nameof(handlerConfigurationType));
        }
    }
}
