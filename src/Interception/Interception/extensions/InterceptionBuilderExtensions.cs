using Dora.Interception;
using Dora.Interception.Expressions;


namespace Microsoft.Extensions.DependencyInjection
{
    /// <summary>
    /// Defines InterceptionBuilder based extension methods.
    /// </summary>
    public static class InterceptionBuilderExtensions
    {
        /// <summary>
        /// Registers the interceptors.
        /// </summary>
        /// <param name="builder">The <see cref="InterceptionBuilder"/>.</param>
        /// <param name="register">The <see cref="Action{IInterceptorRegistry} "/> used to register interceptors.</param>
        /// <returns>The current <see cref="InterceptionBuilder"/></returns>
        public static InterceptionBuilder RegisterInterceptors(this InterceptionBuilder builder, Action<IInterceptorRegistry> register)
        {
            if (builder == null) throw new ArgumentNullException(nameof(builder));
            if (register == null) throw new ArgumentNullException(nameof(register));
            builder.Services.Configure<InterceptionOptions>(it => it.InterceptorRegistrations = register);
            return builder;
        }
    }
}
