using Dora.Interception;

namespace App
{
    public class FoobarInterceptor
    {
        public async ValueTask InvokeAsync(InvocationContext invocationContext)
        {
            var method = invocationContext.MethodInfo;
            var parameters = method.GetParameters();
            Console.WriteLine($"Target: {invocationContext.Target}");
            Console.WriteLine($"Method: {method.Name}({string.Join(", ", parameters.Select(it => it.ParameterType.Name))})");            

            if (parameters.Length > 0)
            {
                Console.WriteLine("Arguments (by index)");
                for (int index = 0; index < parameters.Length; index++)
                {
                    Console.WriteLine($"    {index}:{invocationContext.GetArgument<object>(index)}");
                }

                Console.WriteLine("Arguments (by name)");
                foreach (var parameter in parameters)
                {
                    var parameterName = parameter.Name!;
                    Console.WriteLine($"    {parameterName}:{invocationContext.GetArgument<object>(parameterName)}");
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
