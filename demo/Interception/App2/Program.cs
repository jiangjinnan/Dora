using App2;

var builder = WebApplication.CreateBuilder(args);
builder.Host.UseInterception();
builder.Services
    .AddHttpContextAccessor()
    .AddMemoryCache()
    .AddSingleton<ISystemTimeProvider, SystemTimeProvider>()
    .AddControllers();
var app = builder.Build();
app
    .UseRouting()
    .UseEndpoints(endpint => endpint.MapControllers());
app.Run();