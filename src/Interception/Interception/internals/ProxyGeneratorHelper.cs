using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Dora.Interception
{
    public static class ProxyGeneratorHelper
    {
        public static Task ExecuteInterceptor(IInterceptor interceptor, InvokerDelegate next, InvocationContext invocationContext)
        {
            return interceptor.Delegate(next)(invocationContext);
        }

        public static T GetResult<T>(Task task, InvocationContext invocationContext)
        {
            task.Wait();
            return invocationContext.GetReturnValue<T>();
        }

        public static async Task GetTask(Task task, InvocationContext invocationContext)
        {
            await task;
            await invocationContext.GetReturnValue<Task>();
        }

        public static async Task<T> GetTaskOfResult<T>(Task task, InvocationContext invocationContext)
        {
            await task;
            return ((Task<T>)(invocationContext.ReturnValue)).Result;
        }

        public static async ValueTask GetValueTask(Task task, InvocationContext invocationContext)
        {
            await task;
            await invocationContext.GetReturnValue<ValueTask>();
        }

        public static ValueTask<T> GetValueTaskOfResult<T>(Task task, InvocationContext invocationContext)
        {
            var result = task.ContinueWith(t => ((ValueTask<T>)invocationContext.ReturnValue).Result);
            return new ValueTask<T>(result);
        }

        public static Task AsTaskByValueTask(ValueTask valueTask, InvocationContext invocationContext)
        {
            var task = valueTask.AsTask();
            invocationContext.SetReturnValue(new ValueTask(task));
            return task;
        }

        public static Task AsTaskByValueTaskOfResult<T>(ValueTask<T> valueTask, InvocationContext invocationContext)
        {
            var task = valueTask.AsTask();
            invocationContext.SetReturnValue(new ValueTask<T>(task));
            return task;
        }
    }
}
