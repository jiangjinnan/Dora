using System;
using System.Collections.Generic;
using System.Text;

namespace Dora.ExceptionHandling
{
    public interface IExceptionManagerBuilder
    {
        IServiceProvider ServiceProvider { get; }
        void AddPolicy(string policyName, Action<IExceptionPolicyBuilder> configure);
        ExceptionManager Build();
    }
}
