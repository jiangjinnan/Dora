using System.Reflection;
using System.Threading.Tasks;

namespace Dora.DynamicProxy
{
    /// <summary>
    ///  Define some methods to get return value from <see cref="InvocationContext"/>.
    /// </summary>
    public static class ReturnValueAccessor
    {
        #region Properties
        /// <summary>
        /// The <seealso cref="GetTaskOfResult"/> specific <see cref="MethodInfo"/>. 
        /// </summary>
        /// <value>
        /// The <seealso cref="GetTaskOfResult"/> specific <see cref="MethodInfo"/>. 
        /// </value>
        public static MethodInfo GetTaskOfResultMethodDefinition { get; }

        /// <summary>
        /// The <seealso cref="GetResult"/> specific <see cref="MethodInfo"/>. 
        /// </summary>
        /// <value>
        /// The <seealso cref="GetResult"/> specific <see cref="MethodInfo"/>. 
        /// </value>
        public static MethodInfo GetResultMethodDefinition { get; }

        #endregion

        #region Constructors

        /// <summary>
        /// Initializes the <see cref="ReturnValueAccessor"/> class.
        /// </summary>
        static ReturnValueAccessor()
        {
            GetTaskOfResultMethodDefinition = typeof(ReturnValueAccessor).GetMethod("GetTaskOfResult");
            GetResultMethodDefinition = typeof(ReturnValueAccessor).GetMethod("GetResult");
        }
        #endregion

        #region Public Methods

        /// <summary>
        /// Get return value of type <see cref="Task{TResult}" />.
        /// </summary>
        /// <typeparam name="TResult">The type of the result.</typeparam>
        /// <param name="intercept">The intercept.</param>
        /// <param name="invocationContext">The <see cref="InvocationContext" /> representing the current method invocation context.</param>
        /// <returns>
        /// The return value of type <see cref="Task{TResult}" />.
        /// </returns>
        public static Task<TResult> GetTaskOfResult<TResult>(Task intercept, InvocationContext invocationContext)
        => intercept.ContinueWith(_ => invocationContext.ReturnValue != null
            ? ((Task<TResult>)invocationContext.ReturnValue).Result
            : default(TResult));

        /// <summary>
        /// Gets the result.
        /// </summary>
        /// <typeparam name="TResult">The type of the return value.</typeparam>   
        /// <param name="invocationContext">The <see cref="InvocationContext"/> representing the current method invocation context.</param>
        /// <returns>The return value.</returns>
        public static TResult GetResult<TResult>(InvocationContext invocationContext)
        =>(TResult)invocationContext.ReturnValue;
        #endregion
    }
}
