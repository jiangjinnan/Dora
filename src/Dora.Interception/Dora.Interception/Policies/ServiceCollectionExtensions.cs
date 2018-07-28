using Dora.Interception.Policies;
using Microsoft.Extensions.DependencyInjection;
using System;

namespace Dora.Interception
{
    /// <summary>
    /// Define extension methods agasint <see cref="InterceptionBuilder"/>.
    /// </summary>
    public static class ServiceCollectionExtensions
    {
        /// <summary>
        /// Adds the interception policy.
        /// </summary>
        /// <param name="builder">The <see cref="InterceptionBuilder"/> to perform interception service registration.</param>
        /// <param name="configure">The <see cref="Action{IInterceptionPolicyBuilder}"/> used to build interception policy.</param>
        /// <returns>The current <see cref="InterceptionBuilder"/>. </returns>
        public static InterceptionBuilder AddPolicy(this InterceptionBuilder builder, Action<IInterceptionPolicyBuilder> configure)
        {
            Guard.ArgumentNotNull(builder, nameof(builder));
            Guard.ArgumentNotNull(configure, nameof(configure));

            var serviceProvider = builder.Services.BuildServiceProvider();
            var registrationBuilder = new InterceptionPolicyBuilder(serviceProvider);
            configure.Invoke(registrationBuilder);              
            var resolver = new PolicyInterceptorProviderResolver(registrationBuilder.Build());
            builder.InterceptorProviderResolvers.Add(nameof(PolicyInterceptorProviderResolver), resolver);
            return builder;
        }

        //public static InterceptionBuilder AddPolicy(this InterceptionBuilder builder, string fileName, Assembly[] references, string[] imports)
        //{
        //    var serviceProvider = builder.Services.BuildServiceProvider();
        //    var policyBuilder = new InterceptionPolicyBuilder(serviceProvider);
        //    var script = CSharpScript
        //        .Create<IInterceptionPolicyBuilder>("var builder = Builder;", ScriptOptions.Default
        //            .WithReferences(typeof(IInterceptionPolicyBuilder).Assembly)
        //            .WithReferences(references)
        //            .WithImports(imports)
        //            .WithImports("Dora.Interception"), typeof(Host))
        //        .ContinueWith(File.ReadAllText(fileName), ScriptOptions.Default
        //            .WithReferences(typeof(IInterceptionPolicyBuilder).Assembly)
        //            .WithReferences(references)
        //            .WithImports(imports)
        //            .WithImports("Dora.Interception"))
        //        .ContinueWith("builder");
        //    script.RunAsync(new Host(policyBuilder)).Wait();
        //    var resolver = new PolicyInterceptorProviderResolver(policyBuilder.Build());
        //    builder.InterceptorProviderResolvers.Add(nameof(PolicyInterceptorProviderResolver), resolver);
        //    return builder;
        //}
    }

    //public class Host
    //{
    //    public Host(IInterceptionPolicyBuilder builder)
    //    {
    //        Builder = builder ?? throw new ArgumentNullException(nameof(builder));
    //    }
    //    public IInterceptionPolicyBuilder Builder { get; }
    //}
}
