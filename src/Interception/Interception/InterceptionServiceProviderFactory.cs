using Microsoft.Extensions.DependencyInjection;
using System;

namespace Dora.Interception
{
    public class InterceptionServiceProviderFactory : IServiceProviderFactory<InterceptionContainer>
    {
        public InterceptionContainer CreateBuilder(IServiceCollection services)
        {
            return new InterceptionContainer(services);
        }

        public IServiceProvider CreateServiceProvider(InterceptionContainer containerBuilder)
        {
            return containerBuilder.BuildServiceProvider();
        }
    }
}
