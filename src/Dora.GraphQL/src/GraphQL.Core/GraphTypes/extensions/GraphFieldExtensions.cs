using Dora.GraphQL.GraphTypes;
using Dora.GraphQL.Resolvers;

namespace Dora.GraphQL
{
    internal static class GraphFieldExtensions
    {
        public static bool HasCustomResolver(this GraphField field)
        {
            Guard.ArgumentNotNull(field, nameof(field));
            return field.Properties.TryGetValue(GraphDefaults.PropertyNames.HasCustomResolvers, out var value)
                ? (bool)value
                : false;
        }

   

        public static bool SetHasCustomResolverFlags(this GraphField field)
        {
            Guard.ArgumentNotNull(field, nameof(field));
            bool? flag = null;
            if (field.Resolver is MethodResolver)
            {
                flag = true;
            }
            foreach (var subField in field.GraphType.Fields.Values)
            {
                if (SetHasCustomResolverFlags(subField))
                {
                    flag = true;
                }
            }
            if (flag == true)
            {
                field.Properties[GraphDefaults.PropertyNames.HasCustomResolvers] = true;
                return true;
            }
            return false;
        }
    }
}
