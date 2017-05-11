using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Dora.ExceptionHandling
{
    public interface IExceptionPolicy
    {        
        Task<Exception> HandleException(Exception ex, Guid handlingId);
    }
}
