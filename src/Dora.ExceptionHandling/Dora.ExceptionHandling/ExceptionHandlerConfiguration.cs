using System;
using System.Collections.Generic;
using System.Text;

namespace Dora.ExceptionHandling
{
    /// <summary>
    /// 
    /// </summary>
    public abstract class ExceptionHandlerConfiguration
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="builder"></param>
        /// <param name="configuration"></param>
        public abstract void Use(IExceptionHandlerBuilder builder, IDictionary<string, string> configuration);
    }
}
