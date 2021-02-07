using System;
using System.Collections.Generic;
using System.Text;

namespace Dora.Interception
{
    public interface IServiceProviderAccessor
    {
        IServiceProvider ServiceProvider { get; }
    }
}
