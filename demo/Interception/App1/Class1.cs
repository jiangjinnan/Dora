//using System;
//using System.Reflection;
//using System.Threading.Tasks;
//using Microsoft.Extensions.DependencyInjection;

//namespace Dora.Interception.CodeGeneration
//{
//    public class SystemTimeProviderProxy344055600 : App1.ISystemTimeProvider, IInterfaceProxy
//    {
//        private readonly App1.ISystemTimeProvider _target;
//        private readonly IInvocationServiceScopeFactory _scopeFactory;
//        private static readonly Lazy<MethodInfo> _methodOfGetCurrentTime2136417087 = new Lazy<MethodInfo>(() => ProxyHelper.GetMethodInfo<App1.SystemTimeProvider>(100663319));
//        private static readonly Lazy<InvokeDelegate> _invokerOfGetCurrentTime2026160119 = new Lazy<InvokeDelegate>(() => MethodInvokerBuilder.Instance.Build(typeof(App1.SystemTimeProvider), ProxyHelper.GetMethodInfo<App1.SystemTimeProvider>(100663319), GetCurrentTime1860815268));
//        private class GetCurrentTimeContext842568340 : InvocationContext
//        {
//            internal System.DateTimeKind _kind;
//            internal System.DateTime _returnValue;

//            public override MethodInfo MethodInfo { get; }
//            public override IServiceProvider InvocationServices { get; }

//            public GetCurrentTimeContext842568340(object target, System.DateTimeKind kind, MethodInfo method, IServiceProvider invocationServices) : base(target)
//            {
//                _kind = kind;
//                MethodInfo = method;
//                InvocationServices = invocationServices;
//            }

//            public override TArgument GetArgument<TArgument>(string name)
//            {
//                return name switch
//                {
//                    "kind" => ProxyHelper.GetArgumentOrReturnValue<System.DateTimeKind, TArgument>(_kind),
//                    _ => throw new ArgumentException($"Invalid argument name { name}.", nameof(name))
//                };
//            }

//            public override TArgument GetArgument<TArgument>(int index)
//            {
//                return index switch
//                {
//                    0 => ProxyHelper.GetArgumentOrReturnValue<System.DateTimeKind, TArgument>(_kind),
//                    _ => throw new ArgumentOutOfRangeException(nameof(index))
//                };
//            }

//            public override InvocationContext SetArgument<TArgument>(string name, TArgument value)
//            {
//                return name switch
//                {
//                    "kind" => ProxyHelper.SetArgumentOrReturnValue<GetCurrentTimeContext842568340, System.DateTimeKind, TArgument>(this, value, (ctx, val) => ctx._kind = val),
//                    _ => throw new ArgumentException($"Invalid argument name { name}.", nameof(name))
//                };
//            }

//            public override InvocationContext SetArgument<TArgument>(int index, TArgument value)
//            {
//                return index switch
//                {
//                    0 => ProxyHelper.SetArgumentOrReturnValue<GetCurrentTimeContext842568340, System.DateTimeKind, TArgument>(this, value, (ctx, val) => ctx._kind = val),
//                    _ => throw new ArgumentOutOfRangeException(nameof(index))
//                };
//            }

//            public override TReturnValue GetReturnValue<TReturnValue>() => ProxyHelper.GetArgumentOrReturnValue<System.DateTime, TReturnValue>(_returnValue);
//            public override InvocationContext SetReturnValue<TReturnValue>(TReturnValue value) => ProxyHelper.SetArgumentOrReturnValue<GetCurrentTimeContext842568340, System.DateTime, TReturnValue>(this, value, (ctx, val) => ctx._returnValue = val);
//        }
//        public SystemTimeProviderProxy344055600(IServiceProvider provider, IInvocationServiceScopeFactory scopeFactory)
//        {
//            _target = ActivatorUtilities.CreateInstance<App1.SystemTimeProvider>(provider);
//            _scopeFactory = scopeFactory;
//        }
//        public System.DateTime GetCurrentTime(System.DateTimeKind kind)
//        {
//            using var scope = _scopeFactory.CreateInvocationScope();
//            var method = _methodOfGetCurrentTime2136417087.Value;
//            var context = new GetCurrentTimeContext842568340(_target, kind, method, scope.ServiceProvider);
//            var valueTask = _invokerOfGetCurrentTime2026160119.Value.Invoke(context);
//            if (!valueTask.IsCompleted) valueTask.GetAwaiter().GetResult();
//            return context._returnValue;
//        }
//        public static ValueTask GetCurrentTime1860815268(InvocationContext invocationContext)
//        {
//            var context = (GetCurrentTimeContext842568340)invocationContext;
//            var target = (App1.ISystemTimeProvider)invocationContext.Target;
//            var returnValue = target.GetCurrentTime(context._kind);
//            context._returnValue = returnValue;
//            return ValueTask.CompletedTask;
//        }
//    }
//    public sealed class SystemTimeProviderProxy1144353156 : App1.SystemTimeProvider, IVirtualMethodProxy
//    {
//        private readonly IInvocationServiceScopeFactory _scopeFactory;
//        private static readonly Lazy<MethodInfo> _methodOfGetCurrentTime639821840 = new Lazy<MethodInfo>(() => ProxyHelper.GetMethodInfo<App1.SystemTimeProvider>(100663319));
//        private readonly Lazy<InvokeDelegate> _invokerOfGetCurrentTime1125578964;

//        private class GetCurrentTimeContext511371780 : InvocationContext
//        {
//            internal System.DateTimeKind _kind;
//            internal System.DateTime _returnValue;

//            public override MethodInfo MethodInfo { get; }
//            public override IServiceProvider InvocationServices { get; }

//            public GetCurrentTimeContext511371780(object target, System.DateTimeKind kind, MethodInfo method, IServiceProvider invocationServices) : base(target)
//            {
//                _kind = kind;
//                MethodInfo = method;
//                InvocationServices = invocationServices;
//            }

//            public override TArgument GetArgument<TArgument>(string name)
//            {
//                return name switch
//                {
//                    "kind" => ProxyHelper.GetArgumentOrReturnValue<System.DateTimeKind, TArgument>(_kind),
//                    _ => throw new ArgumentException($"Invalid argument name { name}.", nameof(name))
//                };
//            }

//            public override TArgument GetArgument<TArgument>(int index)
//            {
//                return index switch
//                {
//                    0 => ProxyHelper.GetArgumentOrReturnValue<System.DateTimeKind, TArgument>(_kind),
//                    _ => throw new ArgumentOutOfRangeException(nameof(index))
//                };
//            }

//            public override InvocationContext SetArgument<TArgument>(string name, TArgument value)
//            {
//                return name switch
//                {
//                    "kind" => ProxyHelper.SetArgumentOrReturnValue<GetCurrentTimeContext511371780, System.DateTimeKind, TArgument>(this, value, (ctx, val) => ctx._kind = val),
//                    _ => throw new ArgumentException($"Invalid argument name { name}.", nameof(name))
//                };
//            }

//            public override InvocationContext SetArgument<TArgument>(int index, TArgument value)
//            {
//                return index switch
//                {
//                    0 => ProxyHelper.SetArgumentOrReturnValue<GetCurrentTimeContext511371780, System.DateTimeKind, TArgument>(this, value, (ctx, val) => ctx._kind = val),
//                    _ => throw new ArgumentOutOfRangeException(nameof(index))
//                };
//            }

//            public override TReturnValue GetReturnValue<TReturnValue>() => ProxyHelper.GetArgumentOrReturnValue<System.DateTime, TReturnValue>(_returnValue);
//            public override InvocationContext SetReturnValue<TReturnValue>(TReturnValue value) => ProxyHelper.SetArgumentOrReturnValue<GetCurrentTimeContext511371780, System.DateTime, TReturnValue>(this, value, (ctx, val) => ctx._returnValue = val);
//        }

//        public SystemTimeProviderProxy1144353156(IInvocationServiceScopeFactory scopeFactory) : base()
//        {
//            _scopeFactory = scopeFactory;
//            _invokerOfGetCurrentTime1125578964 = new Lazy<InvokeDelegate>(() => MethodInvokerBuilder.Instance.Build(ProxyHelper.GetMethodInfo<App1.SystemTimeProvider>(100663319), GetCurrentTime1977085165));
//        }

//        public override System.DateTime GetCurrentTime(System.DateTimeKind kind)
//        {
//            using var scope = _scopeFactory.CreateInvocationScope();
//            var method = _methodOfGetCurrentTime639821840.Value;
//            var context = new GetCurrentTimeContext511371780(this, kind, method, scope.ServiceProvider);
//            var valueTask = _invokerOfGetCurrentTime1125578964.Value.Invoke(context);
//            if (!valueTask.IsCompleted) valueTask.GetAwaiter().GetResult();
//            return context._returnValue;
//        }

//        public ValueTask GetCurrentTime1977085165(InvocationContext invocationContext)
//        {
//            var context = (GetCurrentTimeContext511371780)invocationContext;
//            var returnValue = base.GetCurrentTime(context._kind);
//            context._returnValue = returnValue;
//            return ValueTask.CompletedTask;
//        }
//    }
//}

