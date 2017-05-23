using System;
using System.Collections.Generic;
using System.Text;

namespace Dora.ExceptionHandling.Configuration
{
    public interface IExceptionFilter
    {
        bool Match(Exception exception);
    }
}
