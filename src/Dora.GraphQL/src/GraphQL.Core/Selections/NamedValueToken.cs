namespace Dora.GraphQL.Selections
{
    public struct NamedValueToken
    {    
        public string Name { get; }
        public object ValueToken { get; }
        public bool IsVaribleReference { get; }
        public NamedValueToken(string name, object valueToken, bool isVariableReference) : this()
        {
            Name = Guard.ArgumentNotNullOrWhiteSpace( name, nameof(name));
            ValueToken = Guard.ArgumentNotNull(valueToken, nameof(valueToken));
            IsVaribleReference = isVariableReference;
        }
    }
}
