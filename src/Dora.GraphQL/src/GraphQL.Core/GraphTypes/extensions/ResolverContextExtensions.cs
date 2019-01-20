using Dora.GraphQL.Selections;

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
            if (!context.Field.Arguments.TryGetValue(name, out var fieldArgument))
            {
                throw new GraphException($"The argument '{name}' is not defined on the filed '{context.Field.Name}' of {context.Container.GetType()}");
            }

            var graphType = fieldArgument.GraphType;            
            if (((IFieldSelection)context.Selection).Arguments.TryGetValue(name, out var argumentToken))
            {
                object rawValue = argumentToken.ValueToken;
                if (rawValue is string && context.GraphContext.Variables.TryGetValue((string)rawValue, out var value1))
                {
                    rawValue = value1;
                    return graphType.Resolve(value1);
                }
                if (!argumentToken.IsReference)
                {
                    return graphType.Resolve(rawValue);
                }
            }
            if (context.GraphContext.Arguments.TryGetValue(name, out var value2) && value2.DefaultValue != null)
            {
                return graphType.Resolve(value2.DefaultValue);
            }
            return graphType.Resolve(fieldArgument.DefaultValue);
        }
    }
}