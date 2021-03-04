//using Microsoft.Extensions.DependencyInjection;
//using System;
//using System.Collections.Generic;
//using System.Reflection;
//using System.Text;
//using System.Threading.Tasks;

//namespace Dora.Interception
//{
//    public interface IFoobar
//    {
//        void InvokeAsVoid(int x, int y);
//        int InvokeAsResult(int x, int y);
//        Task InvokeAsTask(int x, int y);
//        Task<int> InvokeAsTaskOfResult(int x, int y);
//        ValueTask InvokeAsValueTask(int x, int y);
//        ValueTask<int> InvokeAsValueTaskOfResult(int x, int y);

//        T GenericInvokeAsResult<T>(T x, T y);
//    }
//    public class Foobar : IFoobar
//    {
//        public int InvokeAsResult(int x, int y)
//        {
//            throw new NotImplementedException();
//        }

//        public void InvokeAsVoid(int x, int y)
//        {
//            throw new NotImplementedException();
//        }

//        public Task InvokeAsTask(int x, int y)
//        {
//            throw new NotImplementedException();
//        }

//        public Task<int> InvokeAsTaskOfResult(int x, int y)
//        {
//            throw new NotImplementedException();
//        }

//        public ValueTask InvokeAsValueTask(int x, int y)
//        {
//            throw new NotImplementedException();
//        }

//        public ValueTask<int> InvokeAsValueTaskOfResult(int x, int y)
//        {
//            throw new NotImplementedException();
//        }

//        public void InvokeAsRefOut(ref int x, ref int y, out int result)
//        {
//            throw new NotImplementedException();
//        }

//        public T GenericInvokeAsResult<T>(T x, T y)
//        {
//            throw new NotImplementedException();
//        }
//    }

//    public class FoobarProxy : IFoobar
//    {
//private readonly Foobar _target;
//private readonly IInterceptorProvider   _interceptorProvider;
//public FoobarProxy(IServiceProvider serviceProvider, IInterceptorProvider interceptorProvider)
//{
//    _target = ActivatorUtilities.CreateInstance<Foobar>(serviceProvider);
//    _interceptorProvider = interceptorProvider;
//}

//        public void InvokeAsVoid(int x, int y)
//        {
//            MethodInfo currentMethod = null;
//            var interceptor = _interceptorProvider.GetInterceptor(currentMethod);
//            var invocationContext = interceptor.CaptureArguments
//                ? new InvocationContext(_target, currentMethod, new object[] { x, y })
//                : new InvocationContext(_target, currentMethod);
//            InvokerDelegate next = new InvokeAsVoidClosure(_target, x, y, invocationContext.Arguments).Invoke;
//            var task = interceptor.Delegate(next)(invocationContext);
//            task.Wait();
//        }

//        public int InvokeAsResult(int x, int y)
//        {
//            MethodInfo currentMethod = null;
//            var interceptor = _interceptorProvider.GetInterceptor(currentMethod);
//            var invocationContext = interceptor.CaptureArguments
//                ? new InvocationContext(_target, currentMethod, new object[] { x, y })
//                : new InvocationContext(_target, currentMethod);
//            InvokerDelegate next = new InvokeAsVoidClosure(_target, x, y, invocationContext.Arguments).Invoke;
//            var task = interceptor.Delegate(next)(invocationContext);
//            return ProxyGeneratorHelper.GetResult<int>(task, invocationContext);
//        }

//        public Task InvokeAsTask(int x, int y)
//        {
//            MethodInfo currentMethod = null;
//            var interceptor = _interceptorProvider.GetInterceptor(currentMethod);
//            var invocationContext = interceptor.CaptureArguments
//                ? new InvocationContext(_target, currentMethod, new object[] { x, y })
//                : new InvocationContext(_target, currentMethod);
//            InvokerDelegate next = new InvokeAsTaskClosure(_target, x, y).Invoke;
//            return interceptor.Delegate(next)(invocationContext);
//        }

//        public Task<int> InvokeAsTaskOfResult(int x, int y)
//        {
//            MethodInfo currentMethod = null;
//            var interceptor = _interceptorProvider.GetInterceptor(currentMethod);
//            var invocationContext = interceptor.CaptureArguments
//                ? new InvocationContext(_target, currentMethod, new object[] { x, y })
//                : new InvocationContext(_target, currentMethod);
//            InvokerDelegate next = new InvokeAsTaskOfResultClosure(_target, x, y).Invoke;
//            var task = interceptor.Delegate(next)(invocationContext);

//            return ProxyGeneratorHelper.GetTaskOfResult<int>(task, invocationContext);
//        }

//        public ValueTask InvokeAsValueTask(int x, int y)
//        {
//            MethodInfo currentMethod = null;
//            var interceptor = _interceptorProvider.GetInterceptor(currentMethod);
//            var invocationContext = interceptor.CaptureArguments
//                ? new InvocationContext(_target, currentMethod, new object[] { x, y })
//                : new InvocationContext(_target, currentMethod);
//            InvokerDelegate next = new InvokeAsTaskOfResultClosure(_target, x, y).Invoke;

//            var task = interceptor.Delegate(next)(invocationContext);
//            return ProxyGeneratorHelper.GetValueTask(task, invocationContext);
//        }

//        public ValueTask<int> InvokeAsValueTaskOfResult(int x, int y)
//        {
//            MethodInfo currentMethod = null;
//            var interceptor = _interceptorProvider.GetInterceptor(currentMethod);
//            var invocationContext = interceptor.CaptureArguments
//                ? new InvocationContext(_target, currentMethod, new object[] { x, y })
//                : new InvocationContext(_target, currentMethod);
//            InvokerDelegate next = new InvokeAsTaskOfResultClosure(_target, x, y).Invoke;

//            var task = interceptor.Delegate(next)(invocationContext);
//            return ProxyGeneratorHelper.GetValueTaskOfResult<int>(task, invocationContext);
//        }

//        public T GenericInvokeAsResult<T>(T x, T y)
//        {
//            MethodInfo currentMethod = null;
//            var interceptor = _interceptorProvider.GetInterceptor(currentMethod);
//            var invocationContext = interceptor.CaptureArguments
//                ? new InvocationContext(_target, currentMethod, new object[] { x, y })
//                : new InvocationContext(_target, currentMethod);
//            InvokerDelegate next = new GenericInvokeAsResultClosure<T>(_target, x, y, invocationContext.Arguments).Invoke;
//            var task = interceptor.Delegate(next)(invocationContext);
//            return ProxyGeneratorHelper.GetResult<T>(task, invocationContext);
//        }

//        private class InvokeAsVoidClosure
//        {
//            private  Foobar _target;
//            private object[] _arguments;
//            private int _x;
//            private int _y;

//            public InvokeAsVoidClosure(Foobar target, int x, int y, object[] arguments)
//            {
//                _target = target;
//                if (arguments == null)
//                {
//                    _x = x;
//                    _y = y;
//                }
//                else
//                {
//                    _arguments = arguments;
//                }
//            }

//            public Task Invoke(InvocationContext invocationContext)
//            {
//                if (_arguments == null)
//                {
//                    _target.InvokeAsVoid(_x, _y);
//                }
//                else
//                {
//                    _target.InvokeAsVoid((int)_arguments[0], (int)_arguments[1]);
//                }
//                return Task.CompletedTask;
//            }               
//        }

//        private class InvokeAsVoidClosure2
//        {
//            private Foobar _target;
//            private object[] _arguments;

//            public InvokeAsVoidClosure2(Foobar target, object[] arguments)
//            {
//                _target = target;
//                _arguments = arguments;
//            }

//            public Task Invoke(InvocationContext invocationContext)
//            {
//                _target.InvokeAsVoid((int)_arguments[0], (int)_arguments[1]);
//                return Task.CompletedTask;
//            }
//        }

//        private class InvokeAsResultClosure
//        {
//            private Foobar _target;
//            private int _x;
//            private int _y;

//            public InvokeAsResultClosure(Foobar target, int x, int y)
//            {
//                _target = target;
//                _x = x;
//                _y = y;
//            }

//            public Task Invoke(InvocationContext invocationContext)
//            {
//                invocationContext.SetReturnValue(_target.InvokeAsResult(_x, _y));
//                return Task.CompletedTask;
//            }
//        }

//        private class InvokeAsTaskClosure
//        {
//            private Foobar _target;
//            private int _x;
//            private int _y;

//            public InvokeAsTaskClosure(Foobar target, int x, int y)
//            {
//                _target = target;
//                _x = x;
//                _y = y;
//            }

//            public Task Invoke(InvocationContext invocationContext)
//            {
//                var task = _target.InvokeAsTask(_x, _y);
//                invocationContext.SetReturnValue(task);
//                return task;
//            }
//        }

//        private class InvokeAsTaskOfResultClosure
//        {
//            private Foobar _target;
//            private int _x;
//            private int _y;

//            public InvokeAsTaskOfResultClosure(Foobar target, int x, int y)
//            {
//                _target = target;
//                _x = x;
//                _y = y;
//            }

//            public Task Invoke(InvocationContext invocationContext)
//            {
//                var task = _target.InvokeAsTaskOfResult(_x, _y);
//                invocationContext.SetReturnValue(task);
//                return task;
//            }
//        }

//        private class InvokeAsValueTaskClosure
//        {
//            private Foobar _target;
//            private int _x;
//            private int _y;
//            public InvokeAsValueTaskClosure(Foobar target, int x, int y)
//            {
//                _target = target;
//                _x = x;
//                _y = y;
//            }

//            public Task Invoke(InvocationContext invocationContext)
//            {
//                var valueTask = _target.InvokeAsValueTask(_x, _y);
//                return ProxyGeneratorHelper.AsTaskByValueTask(valueTask, invocationContext);
//            }
//        }

//        private class InvokeAsValueTaskOfResultClosure
//        {
//            private Foobar _target;
//            private int _x;
//            private int _y;

//            public InvokeAsValueTaskOfResultClosure(Foobar target, int x, int y)
//            {
//                _target = target;
//                _x = x;
//                _y = y;
//            }

//            public Task Invoke(InvocationContext invocationContext)
//            {
//                var valueTask = _target.InvokeAsValueTaskOfResult(_x, _y);
//                return ProxyGeneratorHelper.AsTaskByValueTaskOfResult(valueTask, invocationContext);
//            }
//        }

//        private class GenericInvokeAsResultClosure<T>
//        {
//            private Foobar _target;
//            private T _x;
//            private T _y;
//            private object[] _arugments;

//            public GenericInvokeAsResultClosure(Foobar target, T x, T y, object[] arguments)
//            {
//                _target = target;
//                if (arguments == null)
//                {
//                    _x = x;
//                    _y = y;
//                }
//                else
//                {
//                    _arugments = arguments;
//                }
//            }

//            public Task Invoke(InvocationContext invocationContext)
//            {
//                T result;
//                if (_arugments == null)
//                {
//                     result = _target.GenericInvokeAsResult(_x, _y);
//                }
//                else
//                {
//                     result = _target.GenericInvokeAsResult((T)_arugments[0], (T)_arugments[1]);
//                }
//                invocationContext.SetReturnValue(result);
//                return Task.CompletedTask;
//            }
//        }
//    }
//}
