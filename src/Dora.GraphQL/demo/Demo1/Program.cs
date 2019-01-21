using Dora.GraphQL;
using Dora.GraphQL.Executors;
using Dora.GraphQL.GraphTypes;
using Dora.GraphQL.Schemas;
using Dora.GraphQL.Server;
using Dora.GraphQL.Server;
using GraphQL.Execution;
using Lib;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Demo1
{
    class Program
    {
        static void Main()
        {
            new WebHostBuilder()
                .UseKestrel()
                .ConfigureServices(svcs => svcs.AddGraphQLServer())
                .UseStartup<Startup>()
                .Build()
                .Run();
        }

        public class Startup
        {
            public Startup(IGraphSchemaProvider provider)
            {
                Console.WriteLine(provider.GetSchema().ToString());
            }

            public void Configure(IApplicationBuilder app)
            {
                app
                    .UseDeveloperExceptionPage()
                    .UseGraphQLServer();
            }
        }
    }

    public class DemoGraphService : GraphServiceBase
    {
        private static Foobarbaz _instance = Foobarbaz.Create(5);

        //[GraphOperation( OperationType.Query, Name  = "Foobarbaz")]
        //public Task<Foobarbaz> GetFoobarbaz() => Task.FromResult(_instance);

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
}
