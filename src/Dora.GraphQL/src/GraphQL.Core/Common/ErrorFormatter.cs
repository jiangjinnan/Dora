using Microsoft.Extensions.ObjectPool;
using System;
using System.Text;

namespace Dora.GraphQL
{
    internal sealed class ErrorFormatter 
    {
        private ObjectPool<StringBuilder> _stringBuilderPool;
        public static readonly ErrorFormatter Instance = new ErrorFormatter();   
        private ErrorFormatter() => _stringBuilderPool = new DefaultObjectPoolProvider().CreateStringBuilderPool();
        public string Format(Exception exception)
        {
            Guard.ArgumentNotNull(exception, nameof(exception));
            AggregateException aggregateException = exception as AggregateException;
            if (null != aggregateException && aggregateException.InnerException != null)
            {
                return GenerateErrorMessage(aggregateException.InnerException);
            }
            return GenerateErrorMessage(exception);
        }
        private string GenerateErrorMessage(Exception ex)
        {
            var builder = _stringBuilderPool.Get();
            try
            {
                AddError(builder, ex, 0);
                var message = builder.ToString();
                return message;
            }
            finally
            {
                _stringBuilderPool.Return(builder);
            }
        }
        private void AddError(StringBuilder builder, Exception ex, int indent)
        {
            builder.AppendLine($"{new string('\t', indent)} Type: {ex.GetType().AssemblyQualifiedName}");
            builder.AppendLine($"{new string('\t', indent)} Message: {ex.Message}");
            builder.AppendLine($"{new string('\t', indent)} StackTrace: {ex.StackTrace}");
            builder.AppendLine($"{new string('\t', indent)} HelpLink: {ex.HelpLink}");

            AggregateException aggregateException = ex as AggregateException;
            if (aggregateException != null)
            {
                foreach (var innerException in aggregateException.InnerExceptions)
                {
                    AddError(builder, innerException, indent + 1);
                }
            }
            else
            {
                if (null != ex.InnerException)
                {
                    AddError(builder, ex.InnerException, indent + 1);
                }
            }
        }
    }
}
