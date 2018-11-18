using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Dora.Interception;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace App
{
public class Startup
{
    public IServiceProvider ConfigureServices(IServiceCollection services)
    {
        //var registry = new InterceptorRegistry()
        //    .Add(new CacheReturnValueAttribute(), method => method.Name == "GetCurrentTime" && method.DeclaringType == typeof(SystemClock));


        services
            .AddScoped<SystemClock, SystemClock>()
            .AddSingleton<IHostedService, FoobarService>()
            .AddMvc();
        return services.BuildInterceptableServiceProvider(
            //builder=>builder.InterceptorProviderResolvers.Add("policy",registry)
            );
    }

        public void Configure(IApplicationBuilder app)
        {
            app.UseMvc();
        }
    }

    public class FoobarService : IHostedService
    {
        public FoobarService(SystemClock clock) { }
        public Task StartAsync(CancellationToken cancellationToken) => Task.CompletedTask;   
        public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;
    }
}


//public class FoobarInterceptorAttribute : InterceptorAttribute
//{
//    public string Baz { get; }
//    public FoobarInterceptorAttribute(string baz) => Baz = baz;

//    public async Task InvokeAsync(InvocationContext context, IFoo foo, IBar bar)
//    {
//        await foo.DoSomethingAsync();
//        await bar.DoSomethingAsync();
//        await context.ProceedAsync();
//    }
//    public override void Use(IInterceptorChainBuilder builder) => builder.Use(this, Order);
//}

//[AttributeUsage(AttributeTargets.Class| AttributeTargets.Method)]
//public class FoobarInterceptorAttribute : InterceptorAttribute
//{     
//    public string Baz { get; }
//    public FoobarInterceptorAttribute(string baz) => Baz = baz;
//    public override void Use(IInterceptorChainBuilder builder) => builder.Use<FoobarInterceptor>(Order, Baz);
//}

public class InterceptorRegistry : IInterceptorProviderResolver
{
    private readonly IInterceptorProvider[] _empty = new IInterceptorProvider[0];
    private readonly Dictionary<IInterceptorProvider, Func<MethodInfo, bool>> _policies = new Dictionary<IInterceptorProvider, Func<MethodInfo, bool>>();

    public IInterceptorProvider[] GetInterceptorProvidersForMethod(Type targetType, MethodInfo targetMethod)
    => _policies.Where(it => it.Value(targetMethod)).Select(it => it.Key).ToArray();

    public IInterceptorProvider[] GetInterceptorProvidersForProperty(Type targetType, PropertyInfo targetProperty, PropertyMethod getOrSet)
    {
        switch (getOrSet)
        {
            case PropertyMethod.Get:
                return GetInterceptorProvidersForMethod(targetType, targetProperty.GetMethod);
            case PropertyMethod.Set:
                return GetInterceptorProvidersForMethod(targetType, targetProperty.SetMethod);
            default:
                return GetInterceptorProvidersForMethod(targetType, targetProperty.GetMethod)
                    .Union(GetInterceptorProvidersForMethod(targetType, targetProperty.SetMethod))
                    .ToArray();
        }
    }

    public IInterceptorProvider[] GetInterceptorProvidersForType(Type targetType) => _empty;

    public bool? WillIntercept(Type targetType)
    {
        if (targetType.GetCustomAttributes<NonInterceptableAttribute>().Any())
        {
            return false;
        }
        return null;
    }

    public InterceptorRegistry Add(IInterceptorProvider interceptorProvider, Func<MethodInfo, bool> filter)
    {
        _policies.Add(interceptorProvider, filter);
        return this;
    }
}

