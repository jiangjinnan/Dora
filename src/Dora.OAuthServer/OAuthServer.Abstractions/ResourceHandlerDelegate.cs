using System.Threading.Tasks;

namespace Dora.OAuthServer
{
    public delegate Task ResourceHandlerDelegate(ResourceContext context);
}
