using Dora.GraphQL.GraphTypes;
using Dora.GraphQL.Schemas;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Dora.GraphQL.Descriptors
{
    /// <summary>
    /// The GraphQL serivce specific descriptor.
    /// </summary>
    public class GraphServiceDescriptor
    {
        /// <summary>
        /// Gets the type of the service.
        /// </summary>
        /// <value>
        /// The type of the service.
        /// </value>
        public Type ServiceType { get; }

        /// <summary>
        /// Gets the operations.
        /// </summary>
        /// <value>
        /// The operations.
        /// </value>
        public IDictionary<string, GraphOperationDescriptor> Operations { get; }

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
        /// Initializes a new instance of the <see cref="GraphServiceDescriptor"/> class.
        /// </summary>
        /// <param name="serviceType">Type of the service.</param>
        /// <exception cref="ArgumentNullException">serviceType</exception>
        public GraphServiceDescriptor(Type serviceType)
        {
            ServiceType = serviceType ?? throw new ArgumentNullException(nameof(serviceType));
            Operations = serviceType
                .GetMethods(BindingFlags.Instance | BindingFlags.Public)
                .Where(it => it.GetCustomAttribute<GraphOperationAttribute>() != null)
                .Select(it => new GraphOperationDescriptor(it, this))
                .ToDictionary(it => it.Name, it => it);
            Attributes = serviceType.GetCustomAttributes().ToArray();
            KnownTypes = Attributes
                .OfType<KnownTypesAttribute>()
                .SelectMany(it => it.Types)
                .ToArray();
        }
    }
}