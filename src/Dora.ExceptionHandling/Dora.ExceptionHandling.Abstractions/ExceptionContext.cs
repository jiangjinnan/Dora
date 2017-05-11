using System;
using System.Collections.Generic;
using System.Text;

namespace Dora.ExceptionHandling
{
    public class ExceptionContext
    {
        public Exception OriginalException { get; }
        public Exception Exception { get; set; }
        public Guid HandlingId { get; }
        public IDictionary<string, object> Properties { get; } 
        public ExceptionContext(Exception exception, Guid handlingId)
        {
            this.Exception = this.OriginalException = Guard.ArgumentNotNull(exception, nameof(exception));
            this.HandlingId = Guard.ArgumentNotNullOrEmpty(handlingId, nameof(handlingId));
            this.Properties = new Dictionary<string, object>();
        }
    }
}
