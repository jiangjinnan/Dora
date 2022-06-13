using App;

var builder = WebApplication.CreateBuilder(args);
builder.Host.UseInterception();
builder.Services
    .AddLogging(logging=>logging.ClearProviders())
    .AddSingleton<Invoker>()
    .AddSingleton<SingletonService>()
    .AddScoped<ScopedService>()
    .AddTransient<TransientService>()
    .AddControllers();
var app = builder.Build();
app
    .UseRouting()
    .UseEndpoints(endpint => endpint.MapControllers());
app.Run();