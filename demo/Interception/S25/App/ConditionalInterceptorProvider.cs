using Dora.Interception;
using Microsoft.Extensions.Options;
using System.Reflection;

namespace App
{
    public class ConditionalInterceptorProvider : InterceptorProviderBase
    {
        private readonly ConditionalInterceptorProviderOptions _options;

        public ConditionalInterceptorProvider(IConventionalInterceptorFactory interceptorFactory, IOptions<ConditionalInterceptorProviderOptions> optionsAccessor) : base(interceptorFactory)
        => _options = optionsAccessor.Value;

        public override bool CanIntercept(Type targetType, MethodInfo method, out bool suppressed)
        {
            suppressed = false;
            return _options.Registrations.Any(it => it.Condition(targetType, method));
        }

        public override IEnumerable<Sortable<InvokeDelegate>> GetInterceptors(Type targetType, MethodInfo method)
        => _options.Registrations.Where(it => it.Condition(targetType, method)).Select(it => it.Factory(InterceptorFactory)).ToList();
    }

    public class ConditionalInterceptorProviderOptions
    {
        public IList<ConditionalInterceptorRegistration> Registrations { get; } = new List<ConditionalInterceptorRegistration>();
        public Registry<TInterceptor> For<TInterceptor>(params object[] arguments)=> new(factory => factory.CreateInterceptor(typeof(TInterceptor), arguments), this);
    }

    public class Registry<TInterceptor>
    {
        private readonly Func<IConventionalInterceptorFactory, InvokeDelegate> _factory;
        private readonly ConditionalInterceptorProviderOptions _options;

        public Registry(Func<IConventionalInterceptorFactory, InvokeDelegate> factory, ConditionalInterceptorProviderOptions options)
        {
            _factory = factory;
            _options = options;
        }

        public Registry<TInterceptor> To(int order, Func<Type, MethodInfo, bool> condition)
        {
            var entry = new ConditionalInterceptorRegistration(condition, factory=>new Sortable<InvokeDelegate>(order, _factory(factory)));
            _options.Registrations.Add(entry);
            return this;
        }
    }

    public class ConditionalInterceptorRegistration
    {
        public Func<Type, MethodInfo, bool> Condition { get; }
        public Func<IConventionalInterceptorFactory, Sortable<InvokeDelegate>> Factory { get; }
        public ConditionalInterceptorRegistration(Func<Type, MethodInfo, bool> condition, Func<IConventionalInterceptorFactory, Sortable<InvokeDelegate>> factory)
        {
            Condition = condition;
            Factory = factory;
        }
    }
}
