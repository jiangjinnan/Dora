using Dora.ExceptionHandling.Interception;
using Dora.Interception;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Xunit;
using System.Reflection;
using Microsoft.Extensions.DependencyInjection;

namespace Dora.ExceptionHandling.Test
{
    public class ExceptionHandlingInterceptorFixture
    {
        private static string _flag;

        [Theory]
        [InlineData(null, "1")]
        [InlineData("1", null)]
        public void New_Arguments_Not_Allow_Null(string nextIndicator, string managerIndicator)
        {
            InterceptDelegate next = nextIndicator == null
                ? null
                : (InterceptDelegate)(_ => Task.CompletedTask);
            ExceptionManager manager = managerIndicator == null
               ? null
               : new ExceptionManager(new Dictionary<string, IExceptionPolicy> { ["policy1"] = new ExceptionPolicy(new ExceptionPolicyEntry[0], _=>Task.CompletedTask, _=>Task.CompletedTask)});
            Assert.Throws<ArgumentNullException>(() => new ExceptionHandlingInterceptor(next, manager));
        }

        [Fact]
        public async void InvokeAsync()
        {
            var manager = new ExceptionManagerBuilder(new ServiceCollection().BuildServiceProvider())
                .AddPolicy("policy1", policies => policies.Configure(handlers => handlers.Use(_ => Task.Run(() => _flag = "policy1"))))
                .AddPolicy("policy2", policies => policies.Configure(handlers => handlers.Use(_ => Task.Run(() => _flag = "policy2"))))
                .SetDefaultPolicy("policy1")
                .Build();

            var interceptor = new ExceptionHandlingInterceptor(_ => Task.Run(() => throw new InvalidOperationException()), manager, "policy2");
            _flag = null;
            await Assert.ThrowsAsync<InvalidOperationException>(() => interceptor.InvokeAsync(new FakeInvocationContext()));
            Assert.Equal("policy2", _flag);

             interceptor = new ExceptionHandlingInterceptor(_ => Task.Run(() => throw new InvalidOperationException()), manager);
            _flag = null;
            await Assert.ThrowsAsync<InvalidOperationException>(() => interceptor.InvokeAsync(new FakeInvocationContext()));
            Assert.Equal("policy1", _flag);
        }

        private class FakeInvocationContext : InvocationContext
        {
            public override object[] Arguments => throw new NotImplementedException();

            public override Type[] GenericArguments => throw new NotImplementedException();

            public override object InvocationTarget => throw new NotImplementedException();

            public override MethodInfo Method => throw new NotImplementedException();

            public override MethodInfo MethodInvocationTarget => throw new NotImplementedException();

            public override object Proxy => throw new NotImplementedException();

            public override object ReturnValue { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

            public override Type TargetType => throw new NotImplementedException();

            public override object GetArgumentValue(int index)
            {
                throw new NotImplementedException();
            }

            public override void SetArgumentValue(int index, object value)
            {
                throw new NotImplementedException();
            }
        }
    }
}
