using System;

namespace Dora.GraphQL.Schemas
{
    [AttributeUsage(AttributeTargets.Method, AllowMultiple =false, Inherited = false)]
    public sealed class GraphOperationAttribute: Attribute
    {
        public string Name { get; set; }
        public OperationType OperationType { get; }

        public GraphOperationAttribute(OperationType operationType)
        {
            OperationType = operationType;
        }
    }
}