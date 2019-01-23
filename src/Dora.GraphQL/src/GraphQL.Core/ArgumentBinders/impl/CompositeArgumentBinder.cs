using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Dora.GraphQL.ArgumentBinders
{
    internal class CompositeArgumentBinder : IArgumentBinder
    {
        private readonly IEnumerable<IArgumentBinder> _binders;
        public CompositeArgumentBinder(IEnumerable<IArgumentBinder> binders)
        {
            _binders = Guard.ArgumentNotNullOrEmpty(binders, nameof(binders));
        }

        public async ValueTask<ArgumentBindingResult> BindAsync(ArgumentBinderContext context)
        {
            foreach (var binder in _binders)
            {
                var result = await binder.BindAsync(context);
                if (result.IsArgumentBound)
                {
                    return result;
                }
            }
            return ArgumentBindingResult.Failed();
        }
    }
}
