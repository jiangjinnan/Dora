using System;
using System.Collections.Generic;
using System.Reflection;

namespace Dora.GraphQL
{
    /// <summary>
    /// Represents customer attribute accessor.
    /// </summary>
    public interface IAttributeAccessor
    {
        /// <summary>
        /// Gets <see cref="MemberInfo"/> specific attributes.
        /// </summary>
        /// <param name="member">The member.</param>
        /// <param name="attributeType">Type of the attribute.</param>
        /// <param name="inherit">if set to <c>true</c> [inherit].</param>
        /// <returns>The attributes.</returns>
        object[] GetCustomAttributes(MemberInfo member,Type attributeType, bool inherit);

        /// <summary>
        /// Gets the <see cref="ParameterInfo"/> specific attributes.
        /// </summary>
        /// <param name="parameter">The parameter.</param>
        /// <param name="attributeType">Type of the attribute.</param>
        /// <returns>The attributes.</returns>
        IEnumerable<Attribute> GetCustomAttributes(ParameterInfo  parameter, Type attributeType);
    }   
}
