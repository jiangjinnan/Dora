using Dora.GraphQL.GraphTypes;
using Dora.GraphQL.Selections;
using System.Linq;
using System;

namespace Dora.GraphQL
{
    /// <summary>
    /// Defines <see cref="ResolverContext"/> specific extension methods.
    /// </summary>
    public static class ResolverContextExtensions
    {
        /// <summary>
        /// Gets the argument value.
        /// </summary>
        /// <typeparam name="T">The argument type.</typeparam>
        /// <param name="context">The <see cref="ResolverContext"/>.</param>
        /// <param name="argumentName">Name of the argument.</param>
        /// <returns>The argument value.</returns>
        public static T GetArgument<T>(this ResolverContext context, string argumentName)
        {
            Guard.ArgumentNotNullOrWhiteSpace(argumentName, nameof(argumentName));
            var value = context.GetArgument(argumentName);
            return value == null
                ? default
                : (T)value;
        }

        /// <summary>
        /// Gets the argument value.
        /// </summary>
        /// <param name="context">The <see cref="ResolverContext"/>.</param>
        /// <param name="argumentName">Name of the argument.</param>
        /// <returns></returns>
        /// <exception cref="GraphException"></exception>
        public static object GetArgument(this ResolverContext context, string argumentName)
        {
            if (!context.Field.Arguments.TryGetValue(argumentName, out var fieldArgument))
            {
                throw new GraphException($"The argument '{argumentName}' is not defined on the filed '{context.Field.Name}' of {context.Container.GetType()}");
            }

            var graphType = fieldArgument.GraphType;            
            if ((context.Selection).Arguments.TryGetValue(argumentName, out var argumentToken))
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
            if (context.GraphContext.Arguments.TryGetValue(argumentName, out var value2) && value2.DefaultValue != null)
            {
                return graphType.Resolve(value2.DefaultValue);
            }

            return fieldArgument.DefaultValue == null
                ? null
                : graphType.Resolve(fieldArgument.DefaultValue);
        }

        /// <summary>
        /// Check whether to skip the current selection node.
        /// </summary>
        /// <param name="context">The <see cref="ResolverContext"/>.</param>
        /// <returns>A <see cref="Boolean"/> value indicating whether to the current selection node.</returns>
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
                return (bool)GraphValueResolver.Boolean(argument.ValueToken);
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