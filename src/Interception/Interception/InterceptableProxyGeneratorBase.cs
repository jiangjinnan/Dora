using System;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
using System.Text;

namespace Dora.Interception
{
    public abstract class InterceptableProxyGeneratorBase : IInterceptableProxyGenerator
    {
        public ModuleBuilder ModuleBuilder { get; }
        public IInterceptorRegistrationProvider RegistrationProvider { get; }
        public InterceptableProxyGeneratorBase(IInterceptorRegistrationProvider registrationProvider)
        {
            RegistrationProvider = registrationProvider ?? throw new ArgumentNullException(nameof(registrationProvider));
            var assemblyBuilder = AssemblyBuilder.DefineDynamicAssembly(new AssemblyName("Dora.Interception.InterceptableProxies"), AssemblyBuilderAccess.RunAndCollect);
            ModuleBuilder = assemblyBuilder.DefineDynamicModule("Dora.Interception.InterceptableProxies.dll");
        }

        public abstract Type Generate(Type serviceType, Type implementationType);
    }
}
