using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Dora.DynamicProxy
{
    /// <summary>
    /// 
    /// </summary>
    public static class ReturnValueAccessor
    {
        /// <summary>
        /// Gets the get task of result method definition.
        /// </summary>
        /// <value>
        /// The get task of result method definition.
        /// </value>
        public static MethodInfo GetTaskOfResultMethodDefinition { get; }

        /// <summary>
        /// Gets the get result method definition.
        /// </summary>
        /// <value>
        /// The get result method definition.
        /// </value>
        public static MethodInfo GetResultMethodDefinition { get; }

        /// <summary>
        /// Initializes the <see cref="ReturnValueAccessor"/> class.
        /// </summary>
        static ReturnValueAccessor()
        {
            GetTaskOfResultMethodDefinition = typeof(ReturnValueAccessor).GetMethod("GetTaskOfResult");
            GetResultMethodDefinition = typeof(ReturnValueAccessor).GetMethod("GetResult");
        }


        /// <summary>
        /// Gets the task of result.
        /// </summary>
        /// <typeparam name="TResult">The type of the result.</typeparam>
        /// <param name="invocationContext">The invocation context.</param>
        /// <returns></returns>
        public static Task<TResult> GetTaskOfResult<TResult>(InvocationContext invocationContext)
        {
            return (Task<TResult>)invocationContext.ReturnValue;
        }

        /// <summary>
        /// Gets the result.
        /// </summary>
        /// <typeparam name="TResult">The type of the result.</typeparam>
        /// <param name="invocationContext">The invocation context.</param>
        /// <returns></returns>
        public static TResult GetResult<TResult>(InvocationContext invocationContext)
        {
            return (TResult)invocationContext.ReturnValue;
        }
    }
}
