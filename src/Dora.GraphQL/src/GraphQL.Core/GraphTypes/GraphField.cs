using System;
using System.Collections.Generic;

namespace Dora.GraphQL.GraphTypes
{
    /// <summary>
    /// Represents a field of <see cref="IGraphType"/>.
    /// </summary>
    public class GraphField
    {
        #region Properties
        /// <summary>
        /// Gets the field name.
        /// </summary>
        /// <value>
        /// The field name.
        /// </value>
        public string Name { get; }

        /// <summary>
        /// Gets field specific <see cref="IGraphType"/>.
        /// </summary>
        /// <value>
        /// The field specific <see cref="IGraphType"/>.
        /// </value>
        public IGraphType GraphType { get; }

        /// <summary>
        /// Gets the container type in which the field specific property is defined.
        /// </summary>
        /// <value>
        /// The container type in which the field specific property is defined.
        /// </value>
        public Type ContainerType { get; }

        /// <summary>
        /// Gets the <see cref="IGraphResolver"/> to get the field's value.
        /// </summary>
        /// <value>
        /// The <see cref="IGraphResolver"/> to get the field's value.
        /// </value>
        public IGraphResolver Resolver { get; }

        /// <summary>
        /// Gets the argument definitions.
        /// </summary>
        /// <value>
        /// The argument definitions.
        /// </value>
        public IDictionary<string, NamedGraphType> Arguments { get; }

        /// <summary>
        /// Gets the property dictionary.
        /// </summary>
        /// <value>
        /// The property dictionary.
        /// </value>
        public IDictionary<string, object> Properties { get; }
        #endregion

        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="GraphField"/> class.
        /// </summary>
        /// <param name="name">The field name.</param>
        /// <param name="graphType">The container type in which the field specific property is defined.</param>
        /// <param name="containerType">The container type in which the field specific property is defined.</param>
        /// <param name="resolver">The <see cref="IGraphResolver"/> to get the field's value.</param>
        public GraphField(string name, IGraphType graphType, Type containerType, IGraphResolver resolver)
        {
            Name = Guard.ArgumentNotNullOrWhiteSpace(name, nameof(graphType));
            GraphType = Guard.ArgumentNotNull( graphType, nameof(graphType));
            ContainerType = Guard.ArgumentNotNull(containerType, nameof(containerType));
            Resolver = Guard.ArgumentNotNull( resolver, nameof(resolver));
            Arguments = new Dictionary<string, NamedGraphType>();
            Properties = new Dictionary<string, object>();
        }
        #endregion
    }
}
