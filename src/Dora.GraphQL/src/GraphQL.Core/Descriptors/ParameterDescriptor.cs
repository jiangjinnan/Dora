using Dora.GraphQL.Schemas;
using System;
using System.Linq;
using System.Reflection;

namespace Dora.GraphQL.Descriptors
{
    /// <summary>
    /// GraphQL operation method parameter specific descriptor.
    /// </summary>
    public class ParameterDescriptor
    {
        /// <summary>
        /// Gets the parameter name.
        /// </summary>
        /// <value>
        /// The parameter name.
        /// </value>
        public string Name { get; }

        /// <summary>
        /// Gets the <see cref="ParameterInfo"/>
        /// </summary>
        /// <value>
        /// The <see cref="ParameterInfo"/>
        /// </value>
        public ParameterInfo ParameterInfo { get; }

        /// <summary>
        /// Gets a value indicating whether this instance is graph argument.
        /// </summary>
        /// <value>
        ///   <c>true</c> if this instance is graph argument; otherwise, <c>false</c>.
        /// </value>
        public bool IsGraphArgument { get; }

        /// <summary>
        /// Gets the name of the argument.
        /// </summary>
        /// <value>
        /// The name of the argument.
        /// </value>
        public string ArgumentName { get; }

        /// <summary>
        /// Gets a value indicating whether this instance is required argument.
        /// </summary>
        /// <value>
        ///   <c>true</c> if this instance is required argument; otherwise, <c>false</c>.
        /// </value>
        public bool IsRequired{ get; }

        /// <summary>
        /// Gets the attributes.
        /// </summary>
        /// <value>
        /// The attributes.
        /// </value>
        public Attribute[] Attributes { get; }

        /// <summary>
        /// Gets the operation.
        /// </summary>
        /// <value>
        /// The operation.
        /// </value>
        public GraphOperationDescriptor Operation { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="ParameterDescriptor"/> class.
        /// </summary>
        /// <param name="parameterInfo">The parameter information.</param>
        /// <param name="operation">The operation.</param>
        public ParameterDescriptor(ParameterInfo parameterInfo, GraphOperationDescriptor operation)
        {
            ParameterInfo = Guard.ArgumentNotNull( parameterInfo, nameof(parameterInfo));
            Name = parameterInfo.Name;
            Operation = Guard.ArgumentNotNull(operation, nameof(operation));
            Attributes = parameterInfo.GetCustomAttributes().OfType<Attribute>().ToArray();
            var argumentArrtibute = Attributes.OfType<ArgumentAttribute>().SingleOrDefault();
            IsGraphArgument = argumentArrtibute != null;
            if (IsGraphArgument)
            {
                ArgumentName = string.IsNullOrEmpty(argumentArrtibute.Name)
                    ? ParameterInfo.Name
                    : argumentArrtibute.Name;
                IsRequired = argumentArrtibute.IsRequired;
            }
        }
    }
}
