using Dora.GraphQL.GraphTypes;
using Dora.GraphQL.Selections;
using System.Linq;

namespace Dora.GraphQL
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
            if ((context.Selection).Arguments.TryGetValue(name, out var argumentToken))
            {
                object rawValue = argumentToken.ValueToken;
                if (rawValue is string && context.GraphContext.Variables.TryGetValue((string)rawValue, out var value1))
                {
                    rawValue = value1;
                    return graphType.Resolve(value1);
                }
                if (!argumentToken.IsVaribleReference)
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
        public static bool Skip(this ResolverContext context)
        {
            bool ResolveArgumentValue(NamedValueToken argument)
            {
                if (argument.IsVaribleReference)
                {
                    var variableName = (string)argument.ValueToken;
                    if (context.GraphContext.Variables.TryGetValue(variableName, out var value1))
                    {
                        return (bool)GraphValueResolver.Boolean(value1);
                    }
                    if (context.GraphContext.Arguments.TryGetValue(variableName, out var value2) && value2.DefaultValue != null)
                    {
                        return (bool)GraphValueResolver.Boolean(value2.DefaultValue);
                    }
                }
                return (bool)GraphValueResolver.Boolean(argument.Name);
            }

            var skipDirective = context.Selection.Directives.SingleOrDefault(it => it.Name == "skip");
            if (null != skipDirective)
            {
                if (!skipDirective.Arguments.TryGetValue("if", out var argument))
                {
                    throw new GraphException("The required argument 'if' is not specified to the @skip directive.");
                }
                return ResolveArgumentValue(argument);
            }
            var includeDirective = context.Selection.Directives.SingleOrDefault(it => it.Name == "include");

            if (null != includeDirective)
            {
                if (!includeDirective.Arguments.TryGetValue("if", out var argument))
                {
                    throw new GraphException("The required argument 'if' is not specified to the @include directive.");
                }
                return !ResolveArgumentValue(argument);
            }
            return false;
        }
    }
}