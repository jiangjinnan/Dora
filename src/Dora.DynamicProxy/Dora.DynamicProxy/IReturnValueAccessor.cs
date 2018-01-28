//using System;
//using System.Threading.Tasks;

//namespace Dora.DynamicProxy
//{
//    /// <summary>
//    /// Used to create the delegate Func{Task, T}, which get the return value from <see cref="InvocationContext"/>.
//    /// </summary>
//    /// <typeparam name="TResult">The type of the result.</typeparam>
//    public class ReturnValueAccessor<TResult>
//    {
//        private readonly InvocationContext _context;

//        /// <summary>
//        /// Initializes a new instance of the <see cref="ReturnValueAccessor{TResult}" /> class.
//        /// </summary>
//        /// <param name="context">The <see cref="InvocationContext" />.</param>
//        /// <exception cref="ArgumentNullException">Specified <paramref name="context"/> is null.</exception>
//        public ReturnValueAccessor(InvocationContext context)
//        {
//            _context = Guard.ArgumentNotNull(context, nameof(context));
//        }

//        /// <summary>
//        /// Gets the return value.
//        /// </summary>
//        /// <param name="task">The task.</param>
//        /// <returns>The return value.</returns>
//        public TResult GetReturnValue(Task task)
//        {
//            return ((Task<TResult>)_context.ReturnValue).Result;
//        }
//    }  
//}  