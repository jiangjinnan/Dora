using System;
using System.Collections.Generic;
using System.Text;

namespace Dora.OAuthServer
{
    public class UserInfoAccessingContext
    {
        public string UserName { get; set; }
        public string ClientId { get; set; }
        public IEnumerable<string> Scopes { get;}
    }
}
