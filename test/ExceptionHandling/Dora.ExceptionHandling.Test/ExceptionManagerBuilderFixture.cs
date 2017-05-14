using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Text;
using Xunit;

namespace Dora.ExceptionHandling.Test
{
    public class ExceptionManagerBuilderFixture
    {
        [Fact]
        public void New_Arguments_Not_Allow_Null()
        {
            Assert.Throws<ArgumentNullException>(() => new ExceptionManagerBuilder(null));
        }

        [Fact]
        public void New_Normal()
        {
            var serviceProvider = new ServiceCollection().BuildServiceProvider();
            Assert.Same(serviceProvider, new ExceptionManagerBuilder(serviceProvider).ServiceProvider);
        }

        [Theory]
        [InlineData(null, "1")]
        [InlineData("1", null)]
        public void AddPolicy_Arguments_Not_Allow_Null(string policyName, string configIndicator)
        {
            Action<IExceptionPolicyBuilder> config = configIndicator == null
                ? null as Action<IExceptionPolicyBuilder>
                : _ => { };
            Assert.Throws<ArgumentNullException>(() => new ExceptionManagerBuilder(new ServiceCollection().BuildServiceProvider()).AddPolicy(policyName, config));
        }

        [Theory]
        [InlineData("")]
        [InlineData(" ")]
        public void AddPolicy_PolicyName_Not_Empty(string policyName)
        {
            Assert.Throws<ArgumentException>(() => new ExceptionManagerBuilder(new ServiceCollection().BuildServiceProvider())
                .AddPolicy(policyName, _=> { }));
        }
    }
}
