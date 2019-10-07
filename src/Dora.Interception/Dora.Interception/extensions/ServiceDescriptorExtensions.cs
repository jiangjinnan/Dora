using Microsoft.Extensions.DependencyInjection;
using System.Reflection;

namespace Dora.Interception
{
    internal static class ServiceDescriptorExtensions
    {
        private static readonly Assembly _assembly = typeof(ServiceDescriptorExtensions).Assembly;
        public static bool IsInterceptable(this ServiceDescriptor serviceDescriptor)
        {
            Guard.ArgumentNotNull(serviceDescriptor, nameof(serviceDescriptor));
            return serviceDescriptor.ImplementationFactory?.Method?.DeclaringType?.Assembly == _assembly;
        }
    }
}