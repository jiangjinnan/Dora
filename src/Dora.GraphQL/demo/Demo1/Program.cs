using Dora.GraphQL;
using Dora.GraphQL.Executors;
using Dora.GraphQL.GraphTypes;
using Dora.GraphQL.Schemas;
using Dora.GraphQL.Server;
using GraphQL.Execution;
using Lib;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Dora.GraphQL.Server.impl;

namespace Demo1
{
    class Program
    {
        static void Main()
        {
            new WebHostBuilder()
                .UseKestrel()
                .ConfigureServices(svcs => svcs
                    .AddHttpContextAccessor()
                    .AddSingleton<ISchemaFactory, SchemaFactory>()
                    .AddSingleton<IAttributeAccessor, AttributeAccessor>()
                    .AddSingleton<IGraphTypeProvider, GraphTypeProvider>()
                    .AddSingleton<IGraphContextFactory, DefaultGraphContextFactory>()
                    .AddSingleton<IDocumentBuilder, GraphQLDocumentBuilder>()
                    .AddSingleton<IGraphSchemaProvider, GraphSchemaProvider>()
                    .AddSingleton<IGraphExecutor, DefaultGraphExecutor>())
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
                    .UseMiddleware<GraphMiddleware>();
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
}
