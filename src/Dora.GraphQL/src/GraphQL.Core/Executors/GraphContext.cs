using Dora.GraphQL.GraphTypes;
using Dora.GraphQL.Schemas;
using Dora.GraphQL.Selections;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace Dora.GraphQL.Executors
{
    /// <summary>
    /// Represents GrapQL request context.
    /// </summary>
    public class GraphContext
    {
        /// <summary>
        /// Gets the name of the GraphQL operation.
        /// </summary>
        /// <value>
        /// The name of the GraphQL operation.
        /// </value>
        public string OperationName { get; }

        /// <summary>
        /// Gets the type of the GraphQL operation.
        /// </summary>
        /// <value>
        /// The type of the GraphQL operation.
        /// </value>
        public OperationType OperationType { get; }

        /// <summary>
        /// Gets the argument definitions.
        /// </summary>
        /// <value>
        /// The argument definitions.
        /// </value>
        public IDictionary<string, NamedGraphType> Arguments { get; }

        /// <summary>
        /// Gets the selection set.
        /// </summary>
        /// <value>
        /// The selection set.
        /// </value>
        public ICollection<ISelectionNode> SelectionSet { get; internal set; }

        /// <summary>
        /// Gets the variables.
        /// </summary>
        /// <value>
        /// The variables.
        /// </value>
        public IDictionary<string, object> Variables { get; }

        /// <summary>
        /// Gets the operation specific <see cref="GraphField"/>.
        /// </summary>
        /// <value>
        /// The operation specific <see cref="GraphField"/>..
        /// </value>
        public GraphField Operation { get; }

        /// <summary>
        /// Gets the request specific <see cref="IServiceProvider"/>.
        /// </summary>
        /// <value>
        /// The request specific <see cref="IServiceProvider"/>.
        /// </value>
        public IServiceProvider RequestServices { get; }

        /// <summary>
        /// Gets the property dictionary.
        /// </summary>
        /// <value>
        /// The property dictionary.
        /// </value>
        public IDictionary<string, object> Properties { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="GraphContext"/> class.
        /// </summary>
        /// <param name="operationName">Name of the operation.</param>
        /// <param name="operationType">Type of the operation.</param>
        /// <param name="operation">The operation.</param>
        /// <param name="requestServices">The request services.</param>
        public GraphContext(string operationName, OperationType operationType, GraphField operation, IServiceProvider requestServices)
        {
            OperationName = operationName;
            OperationType = operationType;
            Operation = Guard.ArgumentNotNull(operation, nameof(operation));
            RequestServices = Guard.ArgumentNotNull(requestServices, nameof(requestServices));
            Arguments = new Dictionary<string, NamedGraphType>();
            SelectionSet = new Collection<ISelectionNode>();
            Variables = new Dictionary<string, object>();
            Properties = new Dictionary<string, object>();
        }
    }
}
