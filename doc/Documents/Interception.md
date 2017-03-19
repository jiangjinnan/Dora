Interception based services should be registered like this:
```csharp
new WebHostBuilder()
    .UseKestrel()
    .ConfigureServices(svcs => svcs
        .AddSingleton<ISystomClock, SystomClock>()
        .Configure<MemoryCacheEntryOptions>(options => options.AbsoluteExpirationRelativeToNow = TimeSpan.FromSeconds(2))
        .AddInterception(builder => builder.SetDynamicProxyFactory())
        .AddMvc())
    .Configure(app => app.UseMvc())
    .Build()
    .Run();
```
#### 4.2 Register a middleware to create an InterceptableServiceProvider 
If we do not want to inject _IInterceptable&lt;T&gt;_ service, we need to make the _InterceptableServiceProvider_ to provide the dependent services. This can be achieved by registering a middleware, which can be done by calling the _UseInterception_ method (it is the extension method of _IWebHostBuilder_ interface).
```charp
new WebHostBuilder()
    .UseKestrel()
    .ConfigureServices(svcs => svcs
        .AddSingleton<ISystomClock, SystomClock>()
        .Configure<MemoryCacheEntryOptions>(options => options.AbsoluteExpirationRelativeToNow = TimeSpan.FromSeconds(2))
        .AddInterception(builder => builder.SetDynamicProxyFactory())
        .AddMvc())
    .Configure(app => app.UseMvc())
    .UseInterception()
    .Build()
    .Run();
```
Now we can use the general programming to inject the service.
```csharp
public class HomeController: Controller
{
  private readonly ISystomClock _clock;
  public HomeController(ISystomClock clock)
  {
    _clock = clock;
  }

  [HttpGet("/")]
  public async Task Index()
  {
    this.Response.ContentType = "text/html";
    await this.Response.WriteAsync("<html><body><ul>");
    for (int i = 0; i < 5; i++)
    {
      await this.Response.WriteAsync($"<li>{_clock.GetCurrentTime()}({DateTime.UtcNow})</li>");
      await Task.Delay(1000);
    }
    await this.Response.WriteAsync("</ul><body></html>");
  }
}
```