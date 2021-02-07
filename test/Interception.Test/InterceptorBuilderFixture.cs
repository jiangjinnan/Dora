using Dora.Interception;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Xunit;

namespace Interception.Test
{
    public class InterceptorBuilderFixture
    {
        private static List<Type> _interceptors = new List<Type>();

        [Fact]
        public async void Build()
        {
            _interceptors.Clear();
            var serviceProvider = new ServiceCollection()
                .AddSingleton<Foo>()
                .AddSingleton<Bar>()
                .AddSingleton<Interceptor1>()
                .AddSingleton<Interceptor2>()
                .AddSingleton<IServiceProviderAccessor>(_=>new FakeServiceProviderAccessor(_))
                .BuildServiceProvider();

            var method = typeof(InterceptorBuilderFixture).GetMethod("Build");

            var registrations = new InterceptorRegistration[]
                {
                    new InterceptorRegistration(_=>_.GetRequiredService<Interceptor1>(), method, 1),
                    new InterceptorRegistration(_=>_.GetRequiredService<Interceptor2>(), method, 2)
                };
            var builder = ActivatorUtilities.CreateInstance<InterceptorBuilder>(serviceProvider);
            var interceptor = builder.Build(registrations);
            InvokerDelegate next = _ => { _interceptors.Add(typeof(InterceptorBuilderFixture)); return Task.CompletedTask; } ;
            await interceptor.Delegate(next)(new InvocationContext(new object(), method));

            Assert.Equal(3, _interceptors.Count);
            Assert.Equal(typeof(Interceptor1), _interceptors[0]);
            Assert.Equal(typeof(Interceptor2), _interceptors[1]);
            Assert.Equal(typeof(InterceptorBuilderFixture), _interceptors[2]);
        }

        private class Foo { }
        private class Bar { }
        private class Interceptor1
        {
            public Interceptor1(Foo foo)
            {
                foo = foo ?? throw new ArgumentNullException(nameof (foo));
            }

            public Task InvokeAsync(Bar bar, InvocationContext invocationContext)
            {
                bar = bar ?? throw new ArgumentNullException(nameof(bar));
                _interceptors.Add(this.GetType());
                return invocationContext.InvokeAsync();
            }
        }
        private class Interceptor2
        {
            public Interceptor2(Foo foo)
            {
                foo = foo ?? throw new ArgumentNullException(nameof(foo));
            }

            public Task InvokeAsync(Bar bar, InvocationContext invocationContext)
            {
                bar = bar ?? throw new ArgumentNullException(nameof(bar));
                _interceptors.Add(this.GetType());
                return invocationContext.InvokeAsync();
            }
        }

        private class FakeServiceProviderAccessor : IServiceProviderAccessor
        {
            public IServiceProvider ServiceProvider { get; }
            public FakeServiceProviderAccessor(IServiceProvider serviceProvider)
            {
                ServiceProvider = serviceProvider;
            }
        }
    }
}
