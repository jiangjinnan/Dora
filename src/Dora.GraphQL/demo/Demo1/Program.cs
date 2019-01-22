using Dora.GraphQL;
using Dora.GraphQL.Options;
using Dora.GraphQL.Schemas;
using Lib;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Threading.Tasks;

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
                .Build()
                .Run();
        }       
    }

    public class Startup
    {
        public void ConfigureServices(IServiceCollection services)
        {
            services
                .AddGraphQLServer(options=>options.FieldNamingConvention = FieldNamingConvention.CamelCase)
                .AddMvc();
        }

        public void Configure(IApplicationBuilder app)
        {
            app
                .UseDeveloperExceptionPage()
                .UseGraphQLServer()
                .UseMvc();
        }
    }

    public class DemoGraphService : GraphServiceBase
    {
        public static Foobarbaz Instance = Foobarbaz.Create(5);

        //[GraphOperation(OperationType.Query, Name = "Foobarbaz")]
        //public Task<Foobarbaz> GetFoobarbaz() => Task.FromResult(Instance);

        [GraphOperation(OperationType.Mutation)]
        public Customer AddCustomer([Argument]Customer customer) =>  customer;

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
                    Addresses = new List<Address> {
                              new Address
                              {
                                   Province = "Jiangsu",
                                   City = "Suzhou",
                                   District = "IndustryPark",
                                   Street = "SR Xinghu"
                              },
                              new Address
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
