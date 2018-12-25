using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Dora.OAuthServer.Mvc
{
    public abstract class OAuthControllerBase: Controller
    {
        protected virtual Task<IActionResult> SignInAsync()
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
            return Task.FromResult<IActionResult> (Redirect(url));
        }

        protected virtual async Task<IActionResult> ConsentAsync(string clientId,string userName, IEnumerable<string> scopes)
        {
            //TODO 
            var url = Request.Query["ReturnUrl"];
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

            var store = HttpContext.RequestServices.GetRequiredService<IDelegateConsentStore>();
            await store.AddAsync(clientId, userName, scopes);
            return Redirect(url);
        }
    }
}
