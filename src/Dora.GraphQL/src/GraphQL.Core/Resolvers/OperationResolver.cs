using Dora.GraphQL.GraphTypes;
using Dora.GraphQL.Schemas;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Internal;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace Dora.GraphQL.Resolvers
{
    public class OperationResolver : IGraphResolver
    {
        private readonly MethodInfo _methodInfo;
        private readonly ObjectMethodExecutor _executor;
        private readonly Dictionary<string, Type> _parameters;
        private Dictionary<string, string> _argumentNames;
        public OperationResolver(MethodInfo methodInfo)
        {
            _methodInfo = methodInfo ?? throw new ArgumentNullException(nameof(methodInfo));
            _executor = ObjectMethodExecutor.Create(_methodInfo, _methodInfo.DeclaringType.GetTypeInfo());
            _parameters = methodInfo.GetParameters().ToDictionary(it => it.Name, it => it.ParameterType);
        }

        public async ValueTask<object> ResolveAsync(ResolverContext context)
        {
            var serviceProvider = context.GraphContext.RequestServices;
            var accessor = serviceProvider.GetRequiredService<IAttributeAccessor>();
            _argumentNames = _argumentNames ?? _methodInfo.GetParameters().ToDictionary(it => it.Name, it => accessor.GetAttribute<ArgumentAttribute>(it)?.Name ?? it.Name);

            var arrguments = _parameters.Select(it => {
                var argumentName = _argumentNames[it.Key];              
                return context.Field.Arguments.ContainsKey(argumentName)
                     ? GetArgument(context, argumentName)
                     : ActivatorUtilities.CreateInstance(serviceProvider, _parameters[it.Key]);
            }).ToArray();

            var service = ActivatorUtilities.CreateInstance(serviceProvider, _methodInfo.DeclaringType);

            var returnType = _executor.MethodInfo.ReturnType;
            if (typeof(Task).IsAssignableFrom(returnType) || (returnType.IsGenericType && returnType.GetGenericTypeDefinition() == typeof(ValueTask<>)))
            {
                return await _executor.ExecuteAsync(service, arrguments);
            }
            return _executor.Execute(service, arrguments);
        }

        private object GetArgument(ResolverContext context, string name)
        {
            if (!context.GraphContext.TryGetArguments(out var arguments) || !arguments.ContainsKey(name))
            {
                return context.GetArgument(name);
            }

            var value = arguments[name];
            var graphType = context.Field.Arguments[name].GraphType;
            if (value.StartsWith("$"))
            {
               return graphType.Resolve( context.GraphContext.Variables[name.TrimStart('$')]);
            }

            return graphType.Resolve(value);
        }
    }
}
