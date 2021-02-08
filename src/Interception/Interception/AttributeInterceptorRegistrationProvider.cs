using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Dora.Interception
{
    public class AttributeInterceptorRegistrationProvider : IInterceptorRegistrationProvider
    {
        private readonly ConcurrentDictionary<Type, IEnumerable<InterceptorRegistration>> _registrations = new ConcurrentDictionary<Type, IEnumerable<InterceptorRegistration>>();
        public IEnumerable<InterceptorRegistration> GetRegistrations(Type type)
        {
            return _registrations.GetOrAdd(type, CreateRegistrations);
        }

        public IEnumerable<InterceptorRegistration> CreateRegistrations(Type type)
        {
            if (type.GetCustomAttributes<DisallowInterceptionAttribute>().Any())
            {
                return Array.Empty<InterceptorRegistration>();
            }

            var globalAttributes = type.GetCustomAttributes<InterceptorAttribute>();
            var registrations = new List<InterceptorRegistration>();
            foreach (var kv in GetMethods(type))
            {

                var method = kv.Key;
                if (globalAttributes.Any())
                {
                    registrations.AddRange(globalAttributes.Select(it => new InterceptorRegistration(_ => it.CreateInterceptor(_), method, it.Order)));
                }
                var attributes = kv.Value;
                if (attributes.Any())
                {
                    registrations.AddRange(attributes.Select(it => new InterceptorRegistration(_ => it.CreateInterceptor(_), method, it.Order)));
                }
            }
            return registrations;
        }

        private Dictionary<MethodInfo, IEnumerable<InterceptorAttribute>> GetMethods(Type type)
        {
            var dictionary = new Dictionary<MethodInfo, IEnumerable<InterceptorAttribute>>();
            foreach (var method in type.GetMethods(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance))
            {
                if (method.DeclaringType == typeof(object) || method.GetCustomAttributes<DisallowInterceptionAttribute>().Any())
                {
                    continue;
                }
                dictionary[method] = method.GetCustomAttributes<InterceptorAttribute>();
            }
            foreach (var property in type.GetProperties(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance))
            {
                if (property.GetCustomAttributes<DisallowInterceptionAttribute>().Any())
                {
                    continue;
                }
                var method = property.GetMethod;
                if (method != null && !method.GetCustomAttributes<DisallowInterceptionAttribute>().Any())
                {
                    dictionary[method] = method.GetCustomAttributes<InterceptorAttribute>();
                }

                method = property.SetMethod;
                if (method != null && !method.GetCustomAttributes<DisallowInterceptionAttribute>().Any())
                {
                    dictionary[method] = method.GetCustomAttributes<InterceptorAttribute>();
                }
            }
            return dictionary;
        }
    }
}