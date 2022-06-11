using Dora.Interception;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace App3
{
    public class FoobarInterceptor1
    {
        public async ValueTask InvokeAsync(InvocationContext invocationContext)
        {
            var method = invocationContext.MethodInfo;
            var parameters = method.GetParameters();
            Console.WriteLine($"Target: {invocationContext.Target}");
            Console.WriteLine($"Method: {method.Name}({string.Join(", ", parameters.Select(it => it.ParameterType.Name))})") ;
            if (parameters.Length > 0)
            {
                Console.WriteLine("Arguments");
                for (int index = 0; index < parameters.Length; index++)
                {
                    Console.WriteLine($"\t{index}:{invocationContext.GetArgument<object>(index)}");
                }
            }
            await invocationContext.ProceedAsync();
            if (method.ReturnType != typeof(void))
            {
                Console.WriteLine($"Return: {invocationContext.GetReturnValue<object>()}");
            }
        }
    }
}
