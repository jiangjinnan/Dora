using Dora.ExceptionHandling.Properties;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Dora.ExceptionHandling
{
    public class ExceptionPolicy : IExceptionPolicy
    {
        public IEnumerable<ExceptionPolicyEntry> PolicyEntries { get; }
        public Func<ExceptionContext, Task> PreHandler { get; }
        public Func<ExceptionContext, Task> PostHandler { get; }

        public ExceptionPolicy(IEnumerable<ExceptionPolicyEntry> policyEntries, Func<ExceptionContext, Task> preHandler, Func<ExceptionContext, Task> postHandler)
        {
            var list = new List<ExceptionPolicyEntry>(Guard.ArgumentNotNullOrEmpty(policyEntries, nameof(policyEntries)));
            var group = list.GroupBy(it => it.ExceptionType).FirstOrDefault(it => it.Count() > 1);
            if(null != group)
            {
                throw new InvalidOperationException(Resources.ExceptionDuplicateExceptionType.Fill(group.First().ExceptionType.FullName)));
            }
            if (this.PolicyEntries.Any(it => it.ExceptionType != typeof(Exception)))
            {
                list.Add(new ExceptionPolicyEntry(typeof(Exception), PostHandlingAction.ThrowNew, _ => Task.CompletedTask));
            }
            this.PolicyEntries = list;
            this.PreHandler = Guard.ArgumentNotNull(preHandler, nameof(preHandler));
            this.PostHandler = Guard.ArgumentNotNull(postHandler, nameof(postHandler));
        }

        public async Task<Exception> HandleException(Exception exception, Guid handlingId)
        {
            ExceptionContext context = new ExceptionContext(Guard.ArgumentNotNull(exception, nameof(exception)), Guard.ArgumentNotNullOrEmpty(handlingId, nameof(handlingId)));
            ExceptionPolicyEntry policyEntry = this.GetPolicyEntry(exception.GetType());
            try
            {
                await this.PreHandler(context);
                await policyEntry.ExceptionHandler(context);
                await this.PostHandler(context);
                switch (policyEntry.PostHandlingAction)
                {
                    case PostHandlingAction.ThrowNew: return context.Exception;
                    case PostHandlingAction.ThrowOriginal: return context.OriginalException;
                    default: return null;
                }
            }
            catch (Exception ex)
            {
                return new ExceptionHandlingException(Resources.ExceptionHandlingError, ex);
            }
        }

        protected ExceptionPolicyEntry GetPolicyEntry(Type exceptionType)
        {
            Guard.ArgumentNotAssignableTo<Exception>(exceptionType, nameof(exceptionType));
            var entry = this.PolicyEntries.FirstOrDefault(it => it.ExceptionType == exceptionType);
            return entry ?? GetPolicyEntry(exceptionType.GetTypeInfo().BaseType);
        }
    }
}
