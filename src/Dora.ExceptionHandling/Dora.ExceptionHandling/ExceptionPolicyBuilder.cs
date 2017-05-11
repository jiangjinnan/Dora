using Dora.ExceptionHandling.Properties;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dora.ExceptionHandling
{
    public class ExceptionPolicyBuilder : IExceptionPolicyBuilder
    {
        private List<ExceptionPolicyEntry> _policyEntries;
        private ExceptionHandlerBuilder _preHanlderBuilder;
        private ExceptionHandlerBuilder _postHanlderBuilder;
        public IServiceProvider ServiceProvider { get; }

        public ExceptionPolicyBuilder(IServiceProvider serviceProvider)
        {
            this.ServiceProvider = Guard.ArgumentNotNull(serviceProvider, nameof(serviceProvider));
            _policyEntries = new List<ExceptionPolicyEntry>();
            _preHanlderBuilder = new ExceptionHandlerBuilder(serviceProvider);
            _postHanlderBuilder = new ExceptionHandlerBuilder(serviceProvider);
        }

        public void AddHandlers(Type exceptionType, PostHandlingAction postHandlingAction, Action<IExceptionHandlerBuilder> configure)
        {
            Guard.ArgumentNotNull(exceptionType, nameof(exceptionType));
            Guard.ArgumentNotNull(configure, nameof(configure));
            if (_policyEntries.Any(it => it.ExceptionType == exceptionType))
            {
                throw new ArgumentException(Resources.ExceptionDuplicateExceptionType.Fill(exceptionType.FullName), nameof(exceptionType));
            }
            ExceptionHandlerBuilder builder = new ExceptionHandlerBuilder(this.ServiceProvider);
            configure(builder);
            _policyEntries.Add(new ExceptionPolicyEntry(exceptionType, postHandlingAction, builder.Build()));
        }

        public void AddPostHandlers(Func<Exception, bool> predicate, Action<IExceptionHandlerBuilder> configure)
        {
            Guard.ArgumentNotNull(predicate, nameof(predicate));
            Guard.ArgumentNotNull(configure, nameof(configure));
            ExceptionHandlerBuilder builder = new ExceptionHandlerBuilder(this.ServiceProvider);
            configure(builder);
            _preHanlderBuilder.AddHandler(async context =>
            {
                if (predicate(context.Exception))
                {
                   await builder.Build()(context);
                }
            });
        }

        public void AddPreHandlers(Func<Exception, bool> predicate, Action<IExceptionHandlerBuilder> configure)
        {
            Guard.ArgumentNotNull(predicate, nameof(predicate));
            Guard.ArgumentNotNull(configure, nameof(configure));
            ExceptionHandlerBuilder builder = new ExceptionHandlerBuilder(this.ServiceProvider);
            configure(builder);
            _postHanlderBuilder.AddHandler(async context =>
            {
                if (predicate(context.Exception))
                {
                    await builder.Build()(context);
                }
            });
        }

        public IExceptionPolicy Build()
        {
            return new ExceptionPolicy(_policyEntries, _preHanlderBuilder.Build(), _postHanlderBuilder.Build());
        }
    }
}
