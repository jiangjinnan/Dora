using Dora.GraphQL.Schemas;
using System;

namespace Dora.GraphQL
{
    /// <summary>
    /// Attribute used to define GraphQL operation.
    /// </summary>
    /// <seealso cref="System.Attribute" />
    [AttributeUsage(AttributeTargets.Method, AllowMultiple =false, Inherited = false)]
    public class GraphOperationAttribute: Attribute
    {
        /// <summary>
        /// Gets or sets the GraphQL operation name.
        /// </summary>
        /// <value>
        /// The GraphQL operation name.
        /// </value>
        public string Name { get; set; }

        /// <summary>
        /// Gets the type of the operation.
        /// </summary>
        /// <value>
        /// The type of the operation.
        /// </value>
        public OperationType OperationType { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="GraphOperationAttribute"/> class.
        /// </summary>
        /// <param name="operationType">Type of the operation.</param>
        public GraphOperationAttribute(OperationType operationType)
        {
            OperationType = operationType;
        }
    }
}