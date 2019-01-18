using System;
using System.Collections.Generic;
using System.Reflection;

namespace Dora.GraphQL
{
    public interface IAttributeAccessor
    {
        object[] GetCustomAttributes(MemberInfo member,Type attributeType, bool inherit);
        IEnumerable<Attribute> GetCustomAttributes(ParameterInfo  parameter, Type attributeType);
    }

    public class AttributeAccessor : IAttributeAccessor
    {
        public object[] GetCustomAttributes(MemberInfo member, Type attributeType, bool inherit)
       => member.GetCustomAttributes(attributeType, inherit);

        public IEnumerable<Attribute> GetCustomAttributes(ParameterInfo parameter, Type attributeType)
         => parameter.GetCustomAttributes(attributeType);
    }
}
