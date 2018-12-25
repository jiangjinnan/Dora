namespace Dora.OAuthServer
{
    public class OAuthServerOptions
    {
        public AuthorizationServerOptions AuthorizationServer { get; set; }
        public ResourceServerOptions ResourceServer { get; set; }

        public OAuthServerOptions()
        {
            AuthorizationServer = new AuthorizationServerOptions();
            ResourceServer = new ResourceServerOptions();
        }
    }
}
