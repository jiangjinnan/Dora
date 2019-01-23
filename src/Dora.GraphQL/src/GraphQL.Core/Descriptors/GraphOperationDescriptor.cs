using Dora.GraphQL.GraphTypes;
using Dora.GraphQL.Schemas;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Dora.GraphQL.Descriptors
{
    /// <summary>
    /// GraphQL operation method specific descriptor.
    /// </summary>
    public class GraphOperationDescriptor
    {
        /// <summary>
        /// Gets the <see cref="MethodInfo"/>
        /// </summary>
        /// <value>
        /// The <see cref="MethodInfo"/>.
        /// </value>
        public MethodInfo MethodInfo { get; }

        /// <summary>
        /// Gets the <see cref="GraphServiceDescriptor"/>.
        /// </summary>
        /// <value>
        /// The <see cref="GraphServiceDescriptor"/>.
        /// </value>
        public GraphServiceDescriptor Service { get; }

        /// <summary>
        /// Gets the operation name.
        /// </summary>
        /// <value>
        /// The operation name.
        /// </value>
        public string Name { get; }

        /// <summary>
        /// Gets the GraphQL operation type.
        /// </summary>
        /// <value>
        /// The GraphQL operation type.
        /// </value>
        public OperationType OperationType { get; }

        /// <summary>
        /// Gets the parameters.
        /// </summary>
        /// <value>
        /// The parameters.
        /// </value>
        public IDictionary<string, ParameterDescriptor> Parameters { get; }

        /// <summary>
        /// Gets the attributes.
        /// </summary>
        /// <value>
        /// The attributes.
        /// </value>
        public Attribute[] Attributes { get; }

        /// <summary>
        /// Gets the known types.
        /// </summary>
        /// <value>
        /// The known types.
        /// </value>
        public Type[] KnownTypes { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="GraphOperationDescriptor"/> class.
        /// </summary>
        /// <param name="methodInfo">The method information.</param>
        /// <param name="service">The service.</param>
        public GraphOperationDescriptor(MethodInfo methodInfo, GraphServiceDescriptor service)
        {
            Service = service ?? throw new ArgumentNullException(nameof(service));
            MethodInfo = methodInfo ?? throw new ArgumentNullException(nameof(methodInfo));
            Parameters = methodInfo.GetParameters()
                .Select(it => new ParameterDescriptor(it, this))
                .ToDictionary(it => it.Name, it => it);
            Attributes = methodInfo.GetCustomAttributes().ToArray();
            KnownTypes = Attributes.OfType<KnownTypesAttribute>()
                .SelectMany(it => it.Types)
                .ToArray();
            var operationAttribute = Attributes.OfType<GraphOperationAttribute>().Single();
            Name = string.IsNullOrEmpty(operationAttribute.Name)
                ? methodInfo.Name
                : operationAttribute.Name;
            OperationType = operationAttribute.OperationType;
        }
    }
}
