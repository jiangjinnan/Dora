namespace Dora.Interception
{
    [NonInterceptable]
    internal class ApplicationServicesAccessor : IApplicationServicesAccessor
    {
        public IServiceProvider ApplicationServices { get; set; } = default!;
    }
}
