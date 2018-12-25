using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using App.Data;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Lib;
using System.Security.Claims;

namespace App
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.Configure<CookiePolicyOptions>(options =>
            {
                // This lambda determines whether user consent for non-essential cookies is needed for a given request.
                options.CheckConsentNeeded = context => true;
                options.MinimumSameSitePolicy = SameSiteMode.None;
            });

            services.AddDbContext<ApplicationDbContext>(options =>
                options.UseSqlServer(
                    Configuration.GetConnectionString("DefaultConnection")));
            services.AddDefaultIdentity<IdentityUser>()
                .AddDefaultUI(UIFramework.Bootstrap4)
                .AddEntityFrameworkStores<ApplicationDbContext>();

            services.AddMvc().SetCompatibilityVersion(CompatibilityVersion.Version_2_2);
            services.AddAuthentication()
                .AddDora<Profile>("dora", "dora", options =>
                {
                    options.AuthorizationEndpoint = "http://oauth-server:5000/authorize";
                    options.TokenEndpoint = "http://oauth-server:5000/token";
                    options.UserInformationEndpoint = "http://oauth-server:5000/profile";
                    options.ClientId = "foobarapp";
                    options.ClientSecret = "123-456-789";
                    options.CallbackPath = "/dora";

                    options.Scope.Add("http://www.doranet.org/oauth/scope/account");
                    options.Scope.Add("http://www.doranet.org/oauth/scope/personal");

                    options
                        .MapJsonKey(ClaimTypes.NameIdentifier, profile => profile.UserName)
                        .MapJsonKey(ClaimTypes.Email, profile => profile.Email)
                        .MapJsonKey(ClaimTypes.Surname, profile => profile.Surname)
                        .MapJsonKey(ClaimTypes.GivenName, profile => profile.GivenName)
                        .MapJsonKey(ClaimTypes.DateOfBirth, profile => profile.BirthDate)
                        .MapJsonKey(ClaimTypes.Gender, profile => profile.Gender);
                });
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
           
            app
                  .UseDeveloperExceptionPage()
            .UseHttpsRedirection();
            app.UseStaticFiles();
            app.UseCookiePolicy();

            app.UseAuthentication();

            app.UseMvc(routes =>
            {
                routes.MapRoute(
                    name: "default",
                    template: "{controller=Home}/{action=Index}/{id?}");
            });

          
        }
    }
}
