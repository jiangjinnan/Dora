using System;

namespace Dora.OAuthServer
{
    public class TokenValueGenerator : ITokenValueGenerator
    {
        public string GenerateAccessToken() => Guid.NewGuid().ToString();
        public string GenerateAuthorizationCode() => Guid.NewGuid().ToString();
        public string GenerateRefreshToken() => Guid.NewGuid().ToString();
    }
}
