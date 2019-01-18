using Dora.GraphQL.Selections;
using System;
using System.Collections.Generic;
using System.Text;

namespace Dora.GraphQL
{
    public static  class DirectiveExtensions
    {
        public static IDirective AddArgument(this IDirective directive, NamedValueToken argument)
        {
            Guard.ArgumentNotNull(directive, nameof(directive));
            directive.Arguments.Add(argument.Name, argument);
            return directive;
        }
    }
}
