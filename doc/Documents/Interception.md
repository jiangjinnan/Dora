## Dora.Interception
_Dora.Interception_ provides an abstract interception model for AOP programming. Leveraging such an interception model, we can define an _Interceptor_ to conduct non-business cross-cutting function in a very graceful programming style. _Dora.Interception_ is designed for _.NET Core_, and the _Dependency Injection (Microsoft.Extensions.DependencyInjection)_ is directly integrated.
There is only interception implementation based on _Castle_. You can provide your custom interception implementation if you want.
### 1. How to define an interceptor?
Considering consuming dependent services in a _DI_ style, we do not use an interface or abstract class to represent an interceptor. An interceptor class can be defined following the programming convention below:
```csharp
public class CacheInterceptor
{
    private readonly InterceptDelegate _next;
    private readonly IMemoryCache _cache;
    private readonly MemoryCacheEntryOptions _options;
    public CacheInterceptor(InterceptDelegate next, IMemoryCache cache, IOptions<MemoryCacheEntryOptions> optionsAccessor)
    {
        _next = next;
        _cache = cache;
        _options = optionsAccessor.Value;
    }

    public async Task InvokeAsync(InvocationContext context)
    {
        if (!context.Method.GetParameters().All(it => it.IsIn))
        {
            await _next(context);
        }

        var key = new Cachekey(context.Method, context.Arguments);
        if (_cache.TryGetValue(key, out object value))
        {
            context.ReturnValue = value;
        }
        else
        {
            await _next(context);
            _cache.Set(key, context.ReturnValue, _options);
        }
    }

    private class Cachekey
    {
        public MethodInfo Method { get; }
        public object[] InputArguments { get; }

        public Cachekey(MethodInfo method, object[] arguments)
        {
            this.Method = method;
            this.InputArguments = arguments;
        }

        public override bool Equals(object obj)
        {
            Cachekey another = obj as Cachekey;
            if (null == another)
            {
                return false;
            }
            if (!this.Method.Equals(another.Method))
            {
                return false;
            }
            for (int index = 0; index < this.InputArguments.Length; index++)
            {
                var argument1 = this.InputArguments[index];
                var argument2 = another.InputArguments[index];
                if (argument1 == null && argument2 == null)
                {
                    continue;
                }

                if (argument1 == null || argument2 == null)
                {
                    return false;
                }

                if (!argument2.Equals(argument2))
                {
                    return false;
                }
            }
            return true;
        }

        public override int GetHashCode()
        {
            int hashCode = this.Method.GetHashCode();
            foreach (var argument in this.InputArguments)
            {
                hashCode = hashCode ^ argument.GetHashCode();
            }
            return hashCode;
        }
    }
}
```
The two classes _InterceptDelegate_ and _InvocationContext_ are defined in the NuGet package "_Dora.Interception_". To install _Dora.Interception_, run the following command in the _Package Manager Console_:
```
PM>Install-Package Dora.Interception
```
The above _CacheInterceptor_ is a simple interceptor class which helps us to cache a method’s return value, and the key is generated based on all input arguments (If target method has ref or output parameters, caching will be disabled). For the subsequent invocations against the same method, the cached value will be directly returned and target method will not be invoked. What is used for caching by this interceptor is an _IMemoryCache_ object.
The _CacheInterceptor_ illustrates the typical programming convention to define an interceptor class:
* The interceptor class must be an instance class, and static class is illegal.
* The interceptor class must have such a public instance constructor:  
  * Its first parameter’s type must be _InterceptDelegate_, which is used to invoke the next interceptor or target method.  
  * It is allowed to have any number of parameters. (e.g. _CacheInterceptor_’s constructor has a parameter _cache_ of _IMemoryCache_ type).
* The interceptor must have such an _InvokeAsync_ method to conduct specific cross-cutting functionalities:  
  * This method must be an instance method , and static method is illegal.
  * This method must be asynchronous method whose return type is _Task_.  
  * This method’s first parameter must be an _InvocationContext_ object, which carries the contextual information about the method invocation, including the _MethodInfo_, arguments, etc. This context object can be also used to set return value or output parameters. 
  * It is allowed to have any number of parameters, which is bound in a _DI_ manner, so the related service registrations must be added in advanced.  
  * If we need to proceed to next interceptor or method of target instance, we must invoke the _InterceptDelegate_ delegate initialized in constructor.
The following code snippet illustrates another definition of _CacheInterceptor_, in which the _IMemoryCache_ service and _IOptions&lt;MemoryCacheEntryOptions&gt;_ are both directly injected into the _InvokeAsync_ method.
```csharp
public class CacheInterceptor
{
  private readonly InterceptDelegate _next;
  public CacheInterceptor(InterceptDelegate next)
  {
    _next = next;
  }

  public async Task InvokeAsync(InvocationContext context, IMemoryCache cache, IOptions<MemoryCacheEntryOptions> optionsAccessor)
  {
    if (!context.Method.GetParameters().All(it => it.IsIn))
    {
      await _next(context);
    }

    var key = new Cachekey(context.Method, context.Arguments);
    if (cache.TryGetValue(key, out object value))
    {
      context.ReturnValue = value;
    }
    else
    {
      await _next(context);
      cache.Set(key, context.ReturnValue, optionsAccessor.Value);
    }
  }
}
```
### 2. How to apply the interceptor?
The interceptor is applied to target method by declaring specific attribute to the method or class, and we call such an attribute as “_interceptor provider attribute_”. Each interceptor class has its specific provider attribute, which is usually derived from the _InterceptorAttribute_ class. For the above _CacheInterceptor_ class, we can define its provider attribute like this:
```csharp
[AttributeUsage( AttributeTargets.Method)]
public class CacheReturnValueAttribute : InterceptorAttribute
{
  public override void Use(IInterceptorChainBuilder builder)
  {
    builder.Use<CacheInterceptor>(this.Order);
  }
} 
```
Concrete interceptor provider attribute class must override the abstract _Use_ method, which has a parameter of _IInterceptorChainBuilder_ type. In most situations, je just need to call this _IInterceptorChainBuilder_ object’s _Use&lt;TInterceptor&gt_; method to register the specific interceptor. Except for specifying the interceptor type as the generate argument, we must specify the order which determine the position for the interceptor in the built interceptor chain. When interceptor is instantiated, the arguments of constructor can be provided in a _DI_ manner. For the arguments which cannot be injected, we must explicitly specify them. This is very similar to register _ASP.NET Core_ middleware.
```csharp
public static IInterceptorChainBuilder Use<TInterceptor>(this IInterceptorChainBuilder builder, int order, params object[] arguments)
```
The interceptor provider attribute can be declared to class or method level. As illustrated in the following code snippet, we declare the _CacheRetuenValueAttribute_ to the _GetCurrentTime_ method of _SystemClock_ class.
```csharp
public interface ISystomClock
{
  DateTime GetCurrentTime();
}

public class SystomClock : ISystomClock
{
  [CacheReturnValue]
  public DateTime GetCurrentTime()
  {
    return DateTime.UtcNow;
  }
}
```
#### 2.1 Suppress the interceptor providers
If the interceptor provider attribute is declared to a particular class, it means the specific interceptor is applied to all of its methods. If this class has a method which cannot be intercepted, we can declare a _NonInterceptableAttribute_ to this method. When declaring the _NonInterceptableAttribute_, we can specify the types of interceptor provider attribute to suppress. If no interceptor provider type is specified, it means to suppress all kinds of interceptors.
```csharp
[Interceptor1]
[Interceptor2]
public class Service: IService
{
    public void M1();
   [NonInterceptable(typeof(Interceptor1Attribute))]
    public void M2();
}
```
By default, the interceptor provider attributes declared in the base class will be inherited by its sub classes. The _NonInterceptableAttribute_ can be also used to suppress the interceptor provider attributes declared in base class.
```csharp
[Interceptor1]
[Interceptor2]
public class Service1: IService
{
    …
}
[NonInterceptable(typeof(Interceptor1Attribute))]
public class Service2: Service1
{
    …
}
```
#### 2.2 Control the execution order
If multiple interceptors are applied to the same method, we can determine their execution order by specifying specific interceptor provider attribute's _Order_ property. 
```csharp
public class Foobar: IFoobar
{
    [Interceptor(Order = 1)]
    [Interceptor(Order = 2)]
    public void Invoke();
}
```
### 3. Use ServiceProvider to create proxy
Proxy based interception mechanism adopted by _Dora.Interception_. An internal service proxy is provided to create a proxy to wrap the target instance, and the applied interceptors are applied to the proxy. Only the method is called against the proxy instead of target instance can be injected. _Dora.Interception_ integrates .NET Core _Dependency Injection_ frameowork, such a proxy can be provided by _ServiceProvdier_.
#### 3.1 Use IInterceptable&lt;T&gt; to get the proxy
Instead of providing the target service instance, we can use _ServiceProvider_ to provide a specific _IInterceptable&lt;T&gt;_ instance. Its _Proxy_ proeprty returns the proxy which can be injected.
```charp
public interface IInterceptable<T> where T : class
{
    T Proxy { get; }
}
```
In the following program, we follow dependency injection programming style to get the _SystemClock_ specific proxy.
```csharp
var clock = new ServiceCollection()
  .AddMemoryCache()
  .AddSingleton<ISystomClock, SystomClock>()
  .AddInterception(builder => builder.SetDynamicProxyFactory())
  .BuildServiceProvider()
  .GetRequiredService<IInterceptable<ISystomClock>>()
  .Proxy;
for (int i = 0; i < int.MaxValue; i++)
{
  Console.WriteLine($"Current time: {clock.GetCurrentTime()}");
  Task.Delay(1000).Wait();
}
```
The _IInterceptorChainBuilder_ interface's extension method _SetDynamicProxyFactory_ is used to registere a _DynamicProxyFactory_ service, which completes _Castle_ based interception implementation. This custom _ProxyFactory_ class is defined in the _NuGet_ package _"Dora.Interception.Castle"_. To install _Dora.Interception.AspNetCore_, run the following command in the _Package Manager Console_ 
```
PM>Install-Package Dora.Interception.Castle
```
The _CacheInterceptor_ is applied to the _SystemClock_ class, so the return value of _GetCurrentTime_ will be cached. After runing the above program, we will get the following output on the console. The reason why every 5 method invocations return the same value is that the absolute expiration time is configured to 5 seconds.
```
Current time: 3/17/2017 8:00:31 AM
Current time: 3/17/2017 8:00:31 AM
Current time: 3/17/2017 8:00:31 AM
Current time: 3/17/2017 8:00:31 AM
Current time: 3/17/2017 8:00:31 AM
Current time: 3/17/2017 8:00:37 AM
Current time: 3/17/2017 8:00:37 AM
Current time: 3/17/2017 8:00:37 AM
Current time: 3/17/2017 8:00:37 AM
Current time: 3/17/2017 8:00:37 AM
```
#### 3.2 Let ServiceProvider to create proxy
There is another way to get the proxy which can be injected. We can call the _BuilderInterceptableServiceProvider_ method (it is an extension method of _IServiceCollection_ interface) to create an _Interceptable_ ServiceProvider, which can directly create the proxy.
```charp
var clock = new ServiceCollection()
  .AddMemoryCache()
  .AddSingleton<ISystomClock, SystomClock>()
  .BuilderInterceptableServiceProvider()
  .GetRequiredService<ISystomClock>();

for (int i = 0; i < int.MaxValue; i++)
{
  Console.WriteLine($"Current time: {clock.GetCurrentTime()}");
  Task.Delay(1000).Wait();
}
```
### 4 Interception programming in ASP.NET Core
In _ASP.NET Core_ application, there are also two kinds of interception programming.
#### 4.1 Inject IInterceptable&lt;T&gt; service
In order to change the method invocation to target instance to the one to the specific proxy, we can inject the _IInterceptable&lt;T&gt;_ like this.
```csharp
public class HomeController: Controller
{
  private readonly ISystomClock _clock;
  public HomeController(IInterceptable<ISystomClock> interceptable)
  {
    _clock = interceptable.Proxy;
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
#### 4.2 Create an interceptable ServiceProvider 
If we do not want to inject _IInterceptable&lt;T&gt;_ service, we need to make the _interceptable_ ServiceProvider to provide the dependent services. This can be achieved by return such an interceptable ServiceProvider from Startup class's ConfigureServices method. As the following code snippet illustrated, we just need to create the afore-mentioned extension method _BuilderInterceptableServiceProvider_.
```charp
public class Program
{
    public static void Main(string[] args)
    {
        new WebHostBuilder()
                .UseKestrel()
                .UseStartup<Startup>()
                .Build()
                .Run();
    }
}

public class Startup
{
    public IServiceProvider ConfigureServices(IServiceCollection services)
    {
        services
            .AddScoped<ISystomClock, SystomClock>()
            .AddMvc();
        return services.BuilderInterceptableServiceProvider(builder => builder.SetDynamicProxyFactory());
    }
    public void Configure(IApplicationBuilder app)
    {
        app.UseMvc();
    }
}
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
