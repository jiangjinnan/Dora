using Dora.GraphQL.GraphTypes;
using Dora.GraphQL.Schemas;
using Dora.GraphQL.Selections;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace Dora.GraphQL.Executors
{
    public class GraphContext
    {
        public string OperationName { get; }
        public OperationType OperationType { get; }
        public IDictionary<string, NamedGraphType> Arguments { get; }
        public IDictionary<string, ISelectionNode> SelectionSet { get; }
        public IDictionary<string, IFragment> Fragments { get; }
        public IDictionary<string, object> Variables { get; }
        public GraphField Operation { get; }
        public IServiceProvider RequestServices { get; }
        public GraphContext(string operationName, OperationType operationType, GraphField operation, IServiceProvider requestServices)
        {
            OperationName = operationName;
            OperationType = operationType;
            Operation = Guard.ArgumentNotNull(operation, nameof(operation));
            RequestServices = Guard.ArgumentNotNull(requestServices, nameof(requestServices));
            Arguments = new Dictionary<string, NamedGraphType>();
            SelectionSet = new Dictionary<string, ISelectionNode>();
            Fragments = new Dictionary<string, IFragment>();
            Variables = new Dictionary<string, object>();
        }
    }
}
