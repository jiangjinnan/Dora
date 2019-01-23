using System;
using Dora.GraphQL.Descriptors;
using Dora.GraphQL.GraphTypes;

namespace Dora.GraphQL.ArgumentBinders
{
    /// <summary>
    /// Represents the argument binding execution context.
    /// </summary>
    public struct ArgumentBinderContext
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ArgumentBinderContext"/> struct.
        /// </summary>
        /// <param name="parameter">The <see cref="ParameterDescriptor"/> describing the parameter.</param>
        /// <param name="resolverContext">The <see cref="ResolverContext"/>.</param>
        public ArgumentBinderContext(ParameterDescriptor parameter, ResolverContext resolverContext) : this()
        {
            Parameter = parameter ?? throw new ArgumentNullException(nameof(parameter));
            ResolverContext = resolverContext;
        }

        /// <summary>
        /// Gets the <see cref="ParameterDescriptor"/> describing the parameter.
        /// </summary>
        /// <value>The <see cref="ParameterDescriptor"/> describing the parameter.</value>
        public ParameterDescriptor Parameter { get; }

        /// <summary>
        /// Gets the <see cref="ResolverContext"/>.
        /// </summary>
        /// <value>
        /// The <see cref="ResolverContext"/>.
        /// </value>
        public ResolverContext ResolverContext { get; }
    }
}