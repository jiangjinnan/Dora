using System;
using System.Collections.Generic;
using System.Reflection;

namespace Dora.GraphQL
{
    /// <summary>
    /// Default implementation of <see cref="IAttributeAccessor"/>.
    /// </summary>
    /// <seealso cref="Dora.GraphQL.IAttributeAccessor" />
    public class AttributeAccessor : IAttributeAccessor
    {
        /// <summary>
        /// Gets <see cref="T:System.Reflection.MemberInfo" /> specific attributes.
        /// </summary>
        /// <param name="member">The member.</param>
        /// <param name="attributeType">Type of the attribute.</param>
        /// <param name="inherit">if set to <c>true</c> [inherit].</param>
        /// <returns>
        /// The attributes.
        /// </returns>
        public object[] GetCustomAttributes(MemberInfo member, Type attributeType, bool inherit)
        => member.GetCustomAttributes(attributeType, inherit);

        /// <summary>
        /// Gets the <see cref="T:System.Reflection.ParameterInfo" /> specific attributes.
        /// </summary>
        /// <param name="parameter">The parameter.</param>
        /// <param name="attributeType">Type of the attribute.</param>
        /// <returns>
        /// The attributes.
        /// </returns>
        public IEnumerable<Attribute> GetCustomAttributes(ParameterInfo parameter, Type attributeType)
         => parameter.GetCustomAttributes(attributeType);
    }
}
