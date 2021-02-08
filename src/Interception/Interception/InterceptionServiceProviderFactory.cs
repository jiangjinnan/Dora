using Microsoft.Extensions.DependencyInjection;
using System;

namespace Dora.Interception
{
    public class InterceptionServiceProviderFactory : IServiceProviderFactory<InterceptionContainer>
    {
        public Action<InterceptionBuilder> _setup;
        public InterceptionServiceProviderFactory(Action<InterceptionBuilder> setup = null)
        {
            _setup = setup;
        }

        public InterceptionContainer CreateBuilder(IServiceCollection services)
        {
            return new InterceptionContainer(services.AddInterception(_setup));
        }

        public IServiceProvider CreateServiceProvider(InterceptionContainer containerBuilder)
        {
            return containerBuilder.BuildServiceProvider();
        }
    }
}
