using Dora.ExceptionHandling.Properties;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Dora.ExceptionHandling
{
    public class ExceptionManager
    {
        private Dictionary<string, IExceptionPolicy> _policies;
        public ExceptionManager(IDictionary<string, IExceptionPolicy> policies)
        {
            Guard.ArgumentNotNull(policies, nameof(policies));
            Guard.ArgumentNotNullOrEmpty(policies, nameof(policies));
            _policies = new Dictionary<string, IExceptionPolicy>(policies, StringComparer.OrdinalIgnoreCase);
        }

        public async Task HandleExceptionAsync(Exception exception, string policyName)
        {
            Guard.ArgumentNotNull(exception, nameof(exception));
            Guard.ArgumentNotNullOrEmpty(policyName, nameof(policyName));
            if (_policies.TryGetValue(policyName, out IExceptionPolicy policy))
            {
               Exception newException = await policy.HandleException(exception, Guid.NewGuid());
                if (null != newException)
                {
                    throw newException;
                }
            }
            else
            {
                throw new ArgumentException(Resources.ExceptionPolicyNotFound.Fill(policyName), nameof(policyName));
            }
        }
    }
}
