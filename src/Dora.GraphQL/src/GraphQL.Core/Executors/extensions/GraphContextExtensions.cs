using Dora.GraphQL.GraphTypes;
using Dora.GraphQL.Selections;
using System;
using System.Collections.Generic;
using System.Text;

namespace Dora.GraphQL.Executors
{
    public static class GraphContextExtensions
    {
        public static GraphContext AddArgument(this GraphContext  context, NamedGraphType argument)
        {
            Guard.ArgumentNotNull(context, nameof(context));
            context.Arguments[argument.Name] = argument;
            return context;
        }

        public static GraphContext AddFragment(this GraphContext context, IFragment  fragment)
        {
            Guard.ArgumentNotNull(context, nameof(context));
            Guard.ArgumentNotNull(fragment, nameof(fragment));
            context.Fragments[fragment.Name] = fragment;
            return context;
        }

        public static GraphContext AddSelection(this GraphContext context, ISelectionNode selection)
        {
            Guard.ArgumentNotNull(context, nameof(context));
            Guard.ArgumentNotNull(selection, nameof(selection));
            context.SelectionSet[selection.Name] = selection;
            return context;
        }
    }
}
