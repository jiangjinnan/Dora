using System.Collections.Generic;

namespace Dora.GraphQL.ArgumentBinders
{
    /// <summary>
    /// Default implementation of <see cref="IArgumentBinderProvider"/>.
    /// </summary>
    public class ArgumentBinderProvider : IArgumentBinderProvider
    {
        private readonly CompositeArgumentBinder _binder;

        /// <summary>
        /// Initializes a new instance of the <see cref="ArgumentBinderProvider"/> class.
        /// </summary>
        /// <param name="binders">The <see cref="IArgumentBinder"/>s as the inner binders.</param>
        public ArgumentBinderProvider(IEnumerable<IArgumentBinder> binders) => _binder = new CompositeArgumentBinder(binders);

        /// <summary>
        /// Gets the argument binder.
        /// </summary>
        /// <returns>
        /// The <see cref="T:Dora.GraphQL.ArgumentBinders.IArgumentBinder" />.
        /// </returns>
        public IArgumentBinder GetArgumentBinder() => _binder;
    }
}
