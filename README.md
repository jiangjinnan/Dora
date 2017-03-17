## Dora.Interception
Dora.Interception provides an abstract interception model for AOP programming. Leveraging such an interception model, we can define -Interceptor- to conduct non-business cross-cutting functions in a very graceful programming style. Dora.Interception is designed for .NET Core, and the -Dependency Injection (Microsoft.Extensions.DependencyInjection)- is directly integrated.
There is only interception implementation based on Castle. You can provide your custom interception implementation if you want.
### 1. How to define an interceptor?
Considering consuming dependent services in a DI manner, we do not use an interface or abstract class to represent an interceptor. An interceptor class can be defined following the programming convention below:
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
        if (!this.InputArguments[index].Equals(another.InputArguments[index]))
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
The CacheInterceptor is a simple interceptor which help us to cache a method’s return value, and the key is generated based on the arguments. For the subsequent invocation against the same method, the cached value will be directly returned and target method will not be invoked. What is used for caching by this interceptor is an IMemoryCache object.
The CacheInterceptor illustrates the typical programming convention of interceptor class:
* The interceptor class must be an instance class, and static class is illegal.
* The interceptor must have such a public instance constructor:  
  * Its first parameter’s type must be InterceptDelegate, which is used to invoke the next interceptor or target method.  
  * It is allowed to own any number of parameters. (e.g. CacheInterceptor’s constructor have a parameter cache of IMemoryCache type).
* The interceptor must have such an InvokeAsync method to conduct specific cross-cutting functionalities:  
  * This method must be an instance method instead of static one.  
  * This method must be asynchronous method whose return type is Task.  
  * This method’s first parameter must be an InvocationContext object, which carries the contextual information about the method invocation, including the MethodInfo and arguments, etc. This context object can be also used to set return value or output parameters. 
  * It is allowed to own any number of parameters, which is bound in a DI manner, so the related service registrations must be added in advanced.  
  * If we need to proceed to next interceptor or target instance, we must invoke the InterceptDelegate delegate initialized in constructor.
### How to apply the interceptor?
The interceptor is applied to target method by declaring specific attribute to the method or class, and we call such an attribute as “interceptor provider attribute”. Each interceptor class has its specific provider attribute, which is usually derived from the InterceptorAttribute class. For the above CacheInterceptor, we can define its provider attribute like this:
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
Concrete interceptor provider attribute class must override the abstract Use method, which has a parameter of IInterceptorChainBuilder. We just need to call this IInterceptorChainBuilder object’s Use<TInterceptor> method to register the specific interceptor. Except for specify the interceptor type as the generate argument, we must specify the order which determine the position for the interceptor in the built interceptor chain. When interceptor is instantiated, the arguments of constructor can be provided in a DI manner. For the arguments which cannot be injected, we must explicitly specify them. This is very similar to register ASP.NET Core middleware.
```csharp
public static IInterceptorChainBuilder Use<TInterceptor>(this IInterceptorChainBuilder builder, int order, params object[] arguments)
```
The interceptor provider attribute can be declared to class or method. As illustrated in the following code snippet, we declare the CacheRetuenValueAttribute to the GetCurrentTime method of SystemClock class.
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
#### Suppress the interceptor providers
If the interceptor provider attribute is declared to a particular class, it means the specific interceptor is applied to all of its method. If this class has a method which cannot be injected, we can declare a NonInterceptableAttribute to this method. When declaring the NonInterceptableAttribute, we can specify the types of interceptor provider attribute to suppress. If no interceptor provider type is specified, it means to suppress all kinds of interceptor.
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
By default, the interceptor provider attributes declared in the base class can be inherited by its sub class. The NonInterceptableAttribute can also be used to suppress the interceptor provider attributes declared in base class.
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
