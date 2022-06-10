using Microsoft.Extensions.DependencyInjection;

namespace Dora.Interception.CodeGeneration
{
    internal class InterceptableServiceProviderFactory : IServiceProviderFactory<InterceptableContainerBuilder>
    {
        private readonly ServiceProviderOptions _options;
        private readonly Action<InterceptionBuilder>? _setup;
        public InterceptableServiceProviderFactory(ServiceProviderOptions options, Action<InterceptionBuilder>? setup)
        {
            _options = options ?? throw new ArgumentNullException(nameof(options));
            _setup = setup;
        }
        public InterceptableContainerBuilder CreateBuilder(IServiceCollection services) => new(services, _options, _setup);
        public IServiceProvider CreateServiceProvider(InterceptableContainerBuilder containerBuilder) => containerBuilder.CreateServiceProvider();
    }
}
