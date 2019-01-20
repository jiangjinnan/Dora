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

        public static GraphContext AddFragment(this GraphContext context, string name, IFragment  fragment)
        {
            Guard.ArgumentNotNull(context, nameof(context));
            Guard.ArgumentNotNull(fragment, nameof(fragment));
            context.Fragments[name] = fragment;
            return context;
        }
    }
}
