namespace Dora.GraphQL.GraphTypes
{
    public  struct NamedGraphType
    {        
        public string Name { get; }
        public IGraphType GraphType { get; }
        public string DefaultValue { get; }
        public NamedGraphType(string name, IGraphType graphType, string defaultValue = null):this()
        {
            Name = Guard.ArgumentNotNullOrWhiteSpace(name, nameof(name));
            GraphType = Guard.ArgumentNotNull(graphType, nameof(graphType));
            DefaultValue = defaultValue;
        }
    }
}
