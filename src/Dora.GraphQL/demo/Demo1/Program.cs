using Dora.GraphQL;
using Dora.GraphQL.Executors;
using Dora.GraphQL.GraphTypes;
using Lib;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Console;
using GraphQL.Server.Ui.Voyager;
using GraphQL.Server.Ui.Playground;

namespace Demo1
{
    class Program
    {
        static void Main()
        {
            new WebHostBuilder()
                .UseUrls("http://0.0.0.0:4000")
                .UseKestrel()
                .UseStartup<Startup>()
                //.ConfigureLogging(buidler=>buidler
                //    .AddConsole()
                //    .AddFilter<ConsoleLoggerProvider>((category, level)=>category.StartsWith("Dora"))
                //    )
                .Build()
                .Run();
        }       
    }

    public class Startup
    {
        public void ConfigureServices(IServiceCollection services)
        {
            services
                .AddGraphQLServer(
                builder => builder.UseCamelCase()
                )
                .AddMvc();
        }

        public void Configure(IApplicationBuilder app)
        {
            app
                .UseDeveloperExceptionPage()
                .UseGraphQLServer()
                .UseGraphQLPlayground(new GraphQLPlaygroundOptions()
                {
                    Path = "/ui/playground"
                })
                .UseGraphQLVoyager(new GraphQLVoyagerOptions()
                {
                    GraphQLEndPoint = "/graphql",
                    Path = "/ui/voyager"
                })
                .UseMvc();
        }
    }

    [KnownTypes(typeof(Address1), typeof(Address2))]
    public class DemoGraphService : GraphServiceBase
    {
        public static Foobarbaz Instance = Foobarbaz.Create(5);

        //[GraphOperation(OperationType.Query, Name = "Foobarbaz")]
        //public Task<Foobarbaz> GetFoobarbaz() => Task.FromResult(Instance);

        [GraphOperation(OperationType.Mutation)]
        public Customer AddCustomer([Argument]Customer customer,
            HttpContext httpContext,
            GraphContext graphContext,
            CancellationToken cancellationToken,
            IServiceProvider serviceProvider)
        {
            Debug.Assert(httpContext != null && graphContext != null && cancellationToken != null && serviceProvider != null);
            return customer;
        }

        [GraphOperation(OperationType.Query)]
        public Task<Customer> GetCustomer([Argument]string name)
        {
         
            var customer = new Customer
            {
                Id = 123,
                Name = name,
                Type =  CustomerType.Vip,
                ContactInfo = new ContactInfo
                {
                    Email = $"{name}@ly.com",
                    PhoneNo = "123",
                    Addresses = new List<object> {
                              new Address1
                              {
                                   Province = "Jiangsu",
                                   City = "Suzhou",
                                   District = "IndustryPark",
                                   Street = "SR Xinghu"
                              },
                              new Address2
                              {
                                   Province = "SiChuan",
                                   City = "Chengdu",
                                   District = "IndustryPark",
                                   Street = "SR Xinghu"
                              }
                          }
                }
            };
            return Task.FromResult(customer);
        }
    }

    public  class HomeController : Controller
    {
        [HttpGet("/foobarbaz")]
        public string GetFoobarbaz() => JsonConvert.SerializeObject(DemoGraphService.Instance);
    }
}
