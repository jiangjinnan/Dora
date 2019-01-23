using Dora.GraphQL.ArgumentBinders;
using Dora.GraphQL.Descriptors;
using Dora.GraphQL.GraphTypes;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Internal;
using System;
using System.Reflection;
using System.Threading.Tasks;

namespace Dora.GraphQL.Resolvers
{
    /// <summary>
    /// The GraphQL operation specific <see cref="IGraphResolver"/>.
    /// </summary>
    public class OperationResolver : IGraphResolver
    {
        #region Fields
        private readonly GraphOperationDescriptor _operation;
        private readonly ObjectMethodExecutor _executor;
        private readonly IArgumentBinder _binder;
        #endregion

        #region Constructors
        /// <summary>
        /// Initializes a new instance of the <see cref="OperationResolver"/> class.
        /// </summary>
        /// <param name="operation">The <see cref="GraphOperationDescriptor"/> describing the current GraphQL operation.</param>
        /// <param name="binderProvider">The <see cref="IArgumentBinderProvider"/> to provide <see cref="IArgumentBinder"/>.</param>
        public OperationResolver(GraphOperationDescriptor  operation, IArgumentBinderProvider binderProvider)
        {
            _operation = operation ?? throw new ArgumentNullException(nameof(operation));
            _executor = ObjectMethodExecutor.Create(_operation.MethodInfo, _operation.Service.ServiceType.GetTypeInfo());
            _binder = Guard.ArgumentNotNull(binderProvider, nameof(binderProvider)).GetArgumentBinder();
        }
        #endregion

        #region Public methods
        /// <summary>
        /// Resolves the value the current selection node.
        /// </summary>
        /// <param name="context">The <see cref="T:Dora.GraphQL.GraphTypes.ResolverContext" /> in which the field value is resoved.</param>
        /// <returns>
        /// The resolved field's value.
        /// </returns>
        public async ValueTask<object> ResolveAsync(ResolverContext context)
        {
            var arguments = new object[_operation.Parameters.Count];
            var index = 0;
            foreach (var parameter in _operation.Parameters.Values)
            {
                var bindingContext = new ArgumentBinderContext(parameter, context);
                var result = await _binder.BindAsync(bindingContext);
                var value = result.IsArgumentBound
                    ? result.Value
                    : parameter.ParameterInfo.DefaultValue;
                arguments[index++] = value;
            }
            var serviceProvider = context.GraphContext.RequestServices;
            var service = ActivatorUtilities.CreateInstance(serviceProvider, _operation.Service.ServiceType);

            var returnType = _executor.MethodInfo.ReturnType;
            if (typeof(Task).IsAssignableFrom(returnType) || (returnType.IsGenericType && returnType.GetGenericTypeDefinition() == typeof(ValueTask<>)))
            {
                return await _executor.ExecuteAsync(service, arguments);
            }
            return _executor.Execute(service, arguments);
        }
        #endregion
    }
}
