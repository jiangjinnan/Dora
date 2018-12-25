using Dora.OAuthServer;
using Dora.OAuthServer.Mvc;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Security.Principal;
using System.Threading.Tasks;

namespace Server
{
    public class OAuthController : OAuthControllerBase
    {
        private IDelegateScopeStore _scopeStore;
        private IEnumerable<DelegateScope> _scopes;
        public OAuthController(IDelegateScopeStore  scopeStore)
        {
            _scopeStore = scopeStore;
        }

        [HttpGet("/account/logon")]
        public IActionResult SignIn()
        {
            var url= Request.Query["ReturnUrl"];
            var count = 0;
            foreach (var key in Request.Query.Keys)
            {
                if (string.Compare(key, "ReturnUrl", true) != 0)
                {
                    if (count == 0)
                    {
                        url += $"?{key}={Request.Query[key]}";
                    }
                    else
                    {
                        url += $"&{key}={Request.Query[key]}";
                    }
                    count++;
                }
            }
            ViewBag.ReturnUrl = url;
            return View();
        }

        [HttpPost("/account/logon")]
        public async Task<IActionResult> SignIn(string userName, string password)
        {
            var identity = new GenericIdentity(userName, "Passord");
            var principal = new ClaimsPrincipal(identity);
            await HttpContext.SignInAsync(principal,new AuthenticationProperties { IsPersistent = false });
            return await SignInAsync();
        }

        [HttpGet("/account/delegateconsent")]
        public async Task<IActionResult> Consent()
        {
            var url = Request.Query["ReturnUrl"];
            var count = 0;
            foreach (var key in Request.Query.Keys)
            {
                if (string.Compare(key, "ReturnUrl", true) != 0)
                {
                    if (count == 0)
                    {
                        url += $"&{key}={Request.Query[key]}";
                    }
                    else
                    {
                        url += $"&{key}={Request.Query[key]}";
                    }
                    count++;
                }
            }
            ViewBag.ReturnUrl = url;
            var scopes = _scopes ?? (_scopes = await _scopeStore.GetAllAsync());
            return View(scopes);
        }

        [HttpPost("/account/delegateconsent")]
        public async Task<IActionResult> Consent(string[] scopes)
        {
            var clientId = Request.Query["client_id"];
            return await ConsentAsync(clientId,User.Identity.Name,scopes);
        }

        [HttpGet("/account/logout")]
        public async Task SignOutAsync()
        {
            await HttpContext.SignOutAsync();
        }
    }
}
