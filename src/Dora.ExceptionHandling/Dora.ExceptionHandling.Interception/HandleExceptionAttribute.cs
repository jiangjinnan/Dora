using Dora.Interception;
using System.Linq;

namespace Dora.ExceptionHandling.Interception
{
    public class HandleExceptionAttribute : InterceptorAttribute
    {
        public string ExceptionPolicyName { get; }

        public HandleExceptionAttribute(string exceptionPolicyName = null)
        {
            this.ExceptionPolicyName = exceptionPolicyName;
        }

        public override void Use(IInterceptorChainBuilder builder)
        {
            builder.Use<ExceptionHandlingInterceptor>(this.Order, this.GetExceptionPolicyName());
        }

        protected virtual string GetExceptionPolicyName()
        {
            if (!string.IsNullOrWhiteSpace(this.ExceptionPolicyName))
            {
                return this.ExceptionPolicyName;
            }
            return this.Attributes.OfType<ExceptionPolicyAttribute>().FirstOrDefault()?.PolicyName;
        }
    }
}
