using Dora.GraphQL.Executors;
using Dora.GraphQL.GraphTypes;
using System.Collections.Generic;
using System.Linq;

namespace Dora.GraphQL
{
    public static class GraphContextExtensions
    {
        public static GraphContext AddArgument(this GraphContext  context, NamedGraphType argument)
        {
            Guard.ArgumentNotNull(context, nameof(context));
            context.Arguments[argument.Name] = argument;
            return context;
        }

        //(foo:123, bar:456)
        //Not(foo:$foo, bar:$bar)
        internal static void SetArguments(this GraphContext context, string query)
        {
            if (TryNormalizeQuery(query, out _, out var value))
            {
                context.Properties[GraphDefaults.PropertyNames.InlineArguments] = value;
            }
        }

        internal static bool TryGetArguments(this GraphContext context, out IDictionary<string, string> arguments)
        {
            if (context.Properties.TryGetValue(GraphDefaults.PropertyNames.InlineArguments, out var value))
            {
                arguments = (IDictionary<string, string>)value;
                return true;
            }
            return (arguments = null) != null;
        }

        /*
          query GetCustomer {
            Customer (name: foobar){
                Id
                Name
		        Type        
            }
        }"
        =>
          query GetCustomer {
            Customer {
                Id
                Name
		        Type        
            }
        }"
        */
        internal static bool TryNormalizeQuery(this string query, out string normalizedQuery, out IDictionary<string, string> arguments)
        {
            normalizedQuery = null;
            arguments = null;

            var indexOf1stBrace = query.IndexOf('{');
            var indexOf2ndBrace = query.IndexOf('{', indexOf1stBrace + 1);
            var indexOfLeftParenthesis = query.IndexOf('(', indexOf1stBrace);

            //Without arguments
            if (indexOfLeftParenthesis<0 || indexOfLeftParenthesis > indexOf2ndBrace)
            {
                return false;
            }

            var indexOfRightParenthesis = query.IndexOf(')', indexOf1stBrace);
            var expression = query.Substring(indexOfLeftParenthesis + 1, indexOfRightParenthesis - indexOfLeftParenthesis - 1);
            var array = expression.Split(',');
            var dictionay = new Dictionary<string, string>();
            foreach (var argument in array)
            {
                var split = argument.Split(':');
                dictionay.Add(split[0].Trim(), split[1].Trim());
            }

            if (dictionay.All(it => it.Value.StartsWith("$")))
            {
                return false;
            }

            normalizedQuery = query.Substring(0, indexOfLeftParenthesis) + query.Substring(indexOfRightParenthesis + 1);
            arguments = dictionay;
            return true;
        }
    }
}
