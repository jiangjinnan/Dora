using Dora.GraphQL.GraphTypes;

namespace Dora.GraphQL.Selections
{
    /// <summary>
    /// Represents fragement specific <see cref="ISelectionNode"/>.
    /// </summary>
    public interface IFragment: ISelectionNode
    {
        /// <summary>
        /// Gets fragement specific <see cref="IGraphType"/>.
        /// </summary>
        /// <value>
        /// The fragement specific <see cref="IGraphType"/>
        /// </value>
        IGraphType GraphType { get; }
    }
}
