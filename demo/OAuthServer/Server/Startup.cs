using Dora.OAuthServer;
using Lib;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace Server
{
    public class Startup
    {
        // This method gets called by the runtime. Use this method to add services to the container.
        // For more information on how to configure your application, visit https://go.microsoft.com/fwlink/?LinkID=398940
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddAuthentication(options => options.DefaultScheme =CookieAuthenticationDefaults.AuthenticationScheme).AddCookie();
            services.AddDataProtection();

            services
                .AddEntityFrameworkSqlServer()
                .AddDbContext<MyDbContext>(builder => builder.UseSqlServer("server=.;database=oauth;uid=sa;pwd=password"))
                .AddOAuthServer<Profile>(builder =>
                {
                    var scope4Account = "http://www.doranet.org/oauth/scope/account";
                    var scope4Personal = "http://www.doranet.org/oauth/scope/personal";
                    builder
                        .AddScope(scope4Account, "Account", "Get account", null, false)
                        .AddScope(scope4Personal, "Personal Information", "Get personal information", null, true)                    
                        .AddUserInfoEndpoint("/profile", GetUserInfo, scope4Account, scope4Personal)
                        .AddEntityFrameworkStore<MyDbContext>();
                })
                .AddMvc();
        }

        public void Configure(IApplicationBuilder app, IHostingEnvironment env, IApplicationStore appStore)
        {
            app
                .UseDeveloperExceptionPage()
                .UseAuthentication()
                .UseOAuthServer()
                .UseMvc();
            //appStore.CreateAsync(new Application("foobarapp", "Foobar", "123-456-789", new Uri[] { new Uri("http://localhost:3721") }, ClientType.Confidential, "foobar")).Wait();
        }

        private Task<Profile> GetUserInfo(ResourceContext context)
        {
            var userName = context.OAuthTicket.UserName;
            var profile = new Profile
            {
                UserName = userName,
                Email = $"{userName}@ly.com"
            };

            if (context.Scopes.Contains("http://www.doranet.org/oauth/scope/personal"))
            {
                profile.BirthDate = "1981-08-24";
                profile.Surname = "Mao";
                profile.GivenName = userName;
                profile.Gender = "Male";
            }
            return Task.FromResult(profile);
        }
    }
}
