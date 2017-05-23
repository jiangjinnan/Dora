using System;

namespace Dora.ExceptionHandling.Configuration
{
    /// <summary>
    /// An attribute annotated to exception handler type to specify the specific configuraiton type.
    /// </summary>
    [AttributeUsage( AttributeTargets.Class, AllowMultiple =false, Inherited = false)]
    public class HandlerConfigurationAttribute: Attribute
    {
        /// <summary>
        /// The exception handler type specific configuraton type.
        /// </summary>
        public Type HandlerConfigurationType { get; }

        /// <summary>
        /// Create a new <see cref="HandlerConfigurationAttribute"/>.
        /// </summary>
        /// <param name="handlerConfigurationType">The exception handler type specific configuraton type.</param>
        /// <exception cref="ArgumentNullException">The <paramref name="handlerConfigurationType"/> is null.</exception>
        /// <exception cref="ArgumentException">The <paramref name="handlerConfigurationType"/> is not a valid exception type.</exception>
        public HandlerConfigurationAttribute(Type handlerConfigurationType)
        {
            this.HandlerConfigurationType = Guard.ArgumentAssignableTo<ExceptionHandlerConfiguration>(handlerConfigurationType, nameof(handlerConfigurationType));
        }
    }
}
