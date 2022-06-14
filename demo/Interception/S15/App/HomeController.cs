using Microsoft.AspNetCore.Mvc;

namespace App
{
public class HomeController
{
    [HttpGet("/")]
    public string Index([FromServices] Invoker invoker)
    {
        invoker.Invoke();
        Console.WriteLine();
        invoker.Invoke();
        return "OK";
    }
}
}
