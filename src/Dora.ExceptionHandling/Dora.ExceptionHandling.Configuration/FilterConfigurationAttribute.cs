using System;

namespace Dora.ExceptionHandling.Configuration
{
    /// <summary>
    /// An attribute annotated to exception handler type to specify the specific configuraiton type.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
    public class FilterConfigurationAttribute: Attribute
    {
        /// <summary>
        /// The exception handler type specific configuraton type.
        /// </summary>
        public Type FilterConfigurationType { get; }

        /// <summary>
        /// Create a new <see cref="HandlerConfigurationAttribute"/>.
        /// </summary>
        /// <param name="filterConfigurationType">The exception handler type specific configuraton type.</param>
        /// <exception cref="ArgumentNullException">The <paramref name="filterConfigurationType"/> is null.</exception>
        /// <exception cref="ArgumentException">The <paramref name="filterConfigurationType"/> is not a valid exception type.</exception>
        public FilterConfigurationAttribute(Type filterConfigurationType)
        {
            this.FilterConfigurationType = Guard.ArgumentAssignableTo<ExceptionFilterConfiguration>(filterConfigurationType, nameof(filterConfigurationType));
        }
    }
}
