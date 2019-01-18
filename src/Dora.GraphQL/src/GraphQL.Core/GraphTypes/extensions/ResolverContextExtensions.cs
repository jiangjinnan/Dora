namespace Dora.GraphQL.GraphTypes
{
    public static class ResolverContextExtensions
    {
        public static T GetArgument<T>(this ResolverContext context, string name)
        {
           return (T)context.GetArgument(name);
        }
        public static object GetArgument(this ResolverContext context, string name)
        {
            if (!context.Field.Arguments.TryGetValue(name, out var argument))
            {
                throw new GraphException($"The argument '{name}' is not defined on the filed '{context.Field.Name}' of {context.Container.GetType()}");
            }

            var graphType = argument.GraphType;            
            if (!context.Selection.Arguments.TryGetValue(name, out var argumentToken))
            {
                return string.IsNullOrWhiteSpace(argument.DefaultValue)
                    ? throw new GraphException($"The argument '{name}' without default value is not provided.")
                    : graphType.Resolve(argument.DefaultValue);
            }

            object rawValue = argumentToken.ValueToken;
            if (rawValue is string && context.GraphContext.Variables.TryGetValue((string)rawValue, out var value))
            {
                rawValue = value;
            }
            return graphType.Resolve(rawValue);
        }
    }
}