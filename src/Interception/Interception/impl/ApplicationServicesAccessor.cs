namespace Dora.Interception
{
    internal class ApplicationServicesAccessor : IApplicationServicesAccessor
    {
        public IServiceProvider ApplicationServices { get; set; } = default!;
    }
}
