using Dora.GraphQL.GraphTypes;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Internal;
using System;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace Dora.GraphQL.Resolvers
{
    /// <summary>
    /// The method calling based <see cref="IGraphResolver"/>.
    /// </summary>
    public class MethodResolver : IGraphResolver
    {
        #region Fields
        private readonly ObjectMethodExecutor _executor;
        private readonly Type[] _prameterTypes;
        #endregion

        #region Constructors
        /// <summary>
        /// Initializes a new instance of the <see cref="MethodResolver"/> class.
        /// </summary>
        /// <param name="methodInfo">The method information.</param>
        public MethodResolver(MethodInfo methodInfo)
        {
            Guard.ArgumentNotNull(methodInfo, nameof(methodInfo));
            _executor = ObjectMethodExecutor.Create(methodInfo, methodInfo.DeclaringType.GetTypeInfo());
            _prameterTypes = methodInfo.GetParameters().Select(it => it.ParameterType).ToArray();
            if (_prameterTypes.Length == 0 || _prameterTypes[0] != typeof(ResolverContext) || (methodInfo.ReturnType != typeof(ValueTask<object>) && (methodInfo.ReturnType != typeof(Task<object>))))
            {
                throw new GraphException($"The {methodInfo.DeclaringType.Name}.{methodInfo.Name}  is not a valid resolver method.");
            }                
        }
        #endregion

        #region Public methods
        /// <summary>
        /// Resolves the value the current selection node.
        /// </summary>
        /// <param name="context">The <see cref="ResolverContext" /> in which the field value is resoved.</param>
        /// <returns>
        /// The resolved field's value.
        /// </returns>
        public async ValueTask<object> ResolveAsync(ResolverContext context)
        {
            var arguments = _prameterTypes.Select(it => it==typeof(ResolverContext) 
                ? context
                : ActivatorUtilities.CreateInstance(context.GraphContext.RequestServices, it));
            return await _executor.ExecuteAsync(context.Container, arguments.ToArray());
        }
        #endregion
    }
}