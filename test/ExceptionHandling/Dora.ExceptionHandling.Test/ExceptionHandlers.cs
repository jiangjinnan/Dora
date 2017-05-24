using Dora.ExceptionHandling.Configuration;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Dora.ExceptionHandling.Test
{
    public abstract class HandlerBase
    {
        public static List<Type> HandlerChain { get; } = new List<Type>();

        public string Argument1 { get; }
        public string Argument2 { get; }

        public HandlerBase(string arg1, string arg2)
        {
            this.Argument1 = arg1;
            this.Argument2 = arg2;
        }

        public Task HandleExceptionAsync(ExceptionContext context)
        {
            HandlerChain.Add(this.GetType());
            return Task.CompletedTask;
        }
    }

    [HandlerConfiguration(typeof(PreHandler1Configuration))]
    public class PreHandler1 : HandlerBase
    {
        public PreHandler1(string arg1, string arg2) : base(arg1, arg2)
        {
        }
    }

    [HandlerConfiguration(typeof(PreHandler2Configuration))]
    public class PreHandler2 : HandlerBase
    {
        public PreHandler2(string arg1, string arg2) : base(arg1, arg2)
        {
        }
    }
    [HandlerConfiguration(typeof(PostHandler1Configuration))]

    public class PostHandler1 : HandlerBase
    {
        public PostHandler1(string arg1, string arg2) : base(arg1, arg2)
        {
        }
    }


    [HandlerConfiguration(typeof(PostHandler2Configuration))]
    public class PostHandler2 : HandlerBase
    {
        public PostHandler2(string arg1, string arg2) : base(arg1, arg2)
        {
        }
    }


    [HandlerConfiguration(typeof(Handler1Configuration))]
    public class Handler1 : HandlerBase
    {
        public Handler1(string arg1, string arg2) : base(arg1, arg2)
        {
        }
    }

    [HandlerConfiguration(typeof(Handler2Configuration))]
    public class Handler2 : HandlerBase
    {
        public Handler2(string arg1, string arg2) : base(arg1, arg2)
        {
        }
    }

    [HandlerConfiguration(typeof(Handler3Configuration))]
    public class Handler3 : HandlerBase
    {
        public Handler3(string arg1, string arg2) : base(arg1, arg2)
        {
        }
    }

    [HandlerConfiguration(typeof(Handler4Configuration))]
    public class Handler4 : HandlerBase
    {
        public Handler4(string arg1, string arg2) : base(arg1, arg2)
        {
        }
    }

    public abstract class HandlerConfigurationBase : ExceptionHandlerConfiguration
    {
        protected void Use<THandler>(IExceptionHandlerBuilder builder, Func<ExceptionContext, bool> predicate, IDictionary<string, string> configuration)
            where THandler : HandlerBase
        {
            builder.Use<THandler>(configuration.GetValue("arg1"), configuration.GetValue("arg2"));
        }
    }
    public class PreHandler1Configuration : HandlerConfigurationBase
    {
        public override void Use(IExceptionHandlerBuilder builder, Func<ExceptionContext, bool> predicate, IDictionary<string, string> configuration)
        {
            this.Use<PreHandler1>(builder, predicate, configuration);
        }
    }

    public class PreHandler2Configuration : HandlerConfigurationBase
    {
        public override void Use(IExceptionHandlerBuilder builder, Func<ExceptionContext, bool> predicate, IDictionary<string, string> configuration)
        {
            this.Use<PreHandler2>(builder, predicate, configuration);
        }
    }

    public class PostHandler1Configuration : HandlerConfigurationBase
    {
        public override void Use(IExceptionHandlerBuilder builder, Func<ExceptionContext, bool> predicate, IDictionary<string, string> configuration)
        {
            this.Use<PostHandler1>(builder, predicate, configuration);
        }
    }

    public class PostHandler2Configuration : HandlerConfigurationBase
    {
        public override void Use(IExceptionHandlerBuilder builder, Func<ExceptionContext, bool> predicate, IDictionary<string, string> configuration)
        {
            this.Use<PostHandler2>(builder, predicate, configuration);
        }
    }

    public class Handler1Configuration : HandlerConfigurationBase
    {
        public override void Use(IExceptionHandlerBuilder builder, Func<ExceptionContext, bool> predicate, IDictionary<string, string> configuration)
        {
            this.Use<Handler1>(builder, predicate, configuration);
        }
    }

    public class Handler2Configuration : HandlerConfigurationBase
    {
        public override void Use(IExceptionHandlerBuilder builder, Func<ExceptionContext, bool> predicate, IDictionary<string, string> configuration)
        {
            this.Use<Handler2>(builder, predicate, configuration);
        }
    }

    public class Handler3Configuration : HandlerConfigurationBase
    {
        public override void Use(IExceptionHandlerBuilder builder, Func<ExceptionContext, bool> predicate, IDictionary<string, string> configuration)
        {
            this.Use<Handler3>(builder, predicate, configuration);
        }
    }

    public class Handler4Configuration : HandlerConfigurationBase
    {
        public override void Use(IExceptionHandlerBuilder builder, Func<ExceptionContext, bool> predicate, IDictionary<string, string> configuration)
        {
            this.Use<Handler4>(builder, predicate, configuration);
        }
    }


}
