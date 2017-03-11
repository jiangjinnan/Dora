using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using System.Text;

namespace Dora.Interception
{
    /// <summary>
    /// A service provider to get the proxy wrapping the target service.
    /// </summary>
    public class InterceptableServiceProvider : IServiceProvider
    {
        private readonly IServiceProvider _serviceProvider;

        /// <summary>
        /// Create a new <see cref="InterceptableServiceProvider"/>.
        /// </summary>
        /// <param name="serviceProvider">The service provider to provide target service.</param>
        /// <exception cref="ArgumentNullException">The argument <paramref name="serviceProvider"/> is null.</exception>
        public InterceptableServiceProvider(IServiceProvider serviceProvider)
        {
            Guard.ArgumentNotNull(serviceProvider, nameof(serviceProvider));
            _serviceProvider = serviceProvider;
        }


        /// <summary>
        /// Gets the service object of the specified type.
        /// </summary>
        /// <param name="serviceType"> An object that specifies the type of service object to get.</param>
        /// <returns> A service object of type serviceType.-or- null if there is no service object of type serviceType.</returns>
        /// <exception cref="ArgumentNullException">The argument <paramref name="serviceType"/> is null.</exception>
        public object GetService(Type serviceType)
        {
            Guard.ArgumentNotNull(serviceType, nameof(serviceType));
            var proxyFactory = _serviceProvider.GetRequiredService<IProxyFactory>();
            var service = _serviceProvider.GetService(serviceType);
            if (serviceType.GetTypeInfo().IsGenericType && serviceType.GetGenericTypeDefinition() == typeof(IEnumerable<>))
            {
                List<object> list = new List<object>();
                foreach (var item in (IEnumerable)service)
                {
                    list.Add(item);
                }
                Type elementType = serviceType.GetTypeInfo().GetGenericArguments()[0];
                Array array = Array.CreateInstance(elementType, list.Count);
                for (int index = 0; index < list.Count; index++)
                {
                    array.SetValue(proxyFactory.CreateProxy(elementType, list[index]), index);
                }
                return array;
            }
            return proxyFactory.CreateProxy(serviceType, service);
        }
    }
}
