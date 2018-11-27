using System;
using System.Collections.Generic;

namespace Dora.Interception.Policies
{
    /// <summary>
    /// Target type based interception policy.
    /// </summary>
    public class TargetTypePolicy
    {
        /// <summary>
        /// Gets the target type the <see cref="IInterceptorProvider"/> is applied to.
        /// </summary>
        /// <value>
        /// The target type the <see cref="IInterceptorProvider"/> is applied to.
        /// </value>
        public Type TargetType { get; }

        /// <summary>
        /// Gets or sets a flag indicating whether to apply specified <see cref="IInterceptorProvider"/> to all interceptable members.
        /// </summary>
        /// <value>
        /// The flag indicating whether to apply specified <see cref="IInterceptorProvider"/> to all interceptable members.
        /// </value>
        public bool? IncludeAllMembers { get; set; }

        /// <summary>
        /// Gets the explicitly marked methods to which the specified <see cref="IInterceptorProvider"/> is applied.
        /// </summary>
        /// <value>
        /// The explicitly marked methods to which the specified <see cref="IInterceptorProvider"/> is applied.
        /// </value>
        public ISet<int> IncludedMethods { get; }

        /// <summary>
        /// Gets the explicitly excluded methods for the specified <see cref="IInterceptorProvider"/>.
        /// </summary>
        /// <value>
        /// The explicitly excluded methods for the specified <see cref="IInterceptorProvider"/>.
        /// </value>
        public ISet<int> ExludedMethods { get; }

        /// <summary>
        /// Gets the explicitly marked properties to which the specified <see cref="IInterceptorProvider"/> is applied.
        /// </summary>
        /// <value>
        /// The explicitly marked properties to which the specified <see cref="IInterceptorProvider"/> is applied.
        /// </value>
        public IDictionary<int, PropertyMethod> IncludedProperties { get; }

        /// <summary>
        /// Gets the explicitly excluded properties for the specified <see cref="IInterceptorProvider"/>.
        /// </summary>
        /// <value>
        /// The explicitly excluded properties for the specified <see cref="IInterceptorProvider"/>.
        /// </value>
        public IDictionary<int, PropertyMethod> ExcludedProperties { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="TargetTypePolicy"/> class.
        /// </summary>
        /// <param name="targetType">Type of the target.</param>
        /// <exception cref="ArgumentNullException" >Specified <paramref name="targetType"/> is null.</exception>
        public TargetTypePolicy(Type targetType)
        {
            TargetType = Guard.ArgumentNotNull(targetType, nameof(targetType));
            IncludedMethods = new HashSet<int>();
            ExludedMethods = new HashSet<int>();
            IncludedProperties = new Dictionary<int, PropertyMethod>();
            ExcludedProperties = new Dictionary<int, PropertyMethod>();
        }
    }
}
