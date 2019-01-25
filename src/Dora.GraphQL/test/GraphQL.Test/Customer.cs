using Dora.GraphQL;
using Dora.GraphQL.GraphTypes;
using Dora.GraphQL.Schemas;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Demo1
{

    public class Customer
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public CustomerType Type {get;set;}
        public ContactInfo ContactInfo { get; set; }
    }

    public class ContactInfo
    {
        public string PhoneNo { get; set; }

        [Argument(Name = "lowerCase",Type = typeof(bool), IsRequired =true)]
        [GraphField(Resolver = nameof(Resolve))]
        public string Email { get; set; }

        //[UnionType(typeof(Address1), typeof(Address2))]
        public IList<Address> Addresses { get; set; }
        public ValueTask<object> Resolve(ResolverContext context)
        {
            var lowerCase = context.GetArgument<bool>("lowerCase");
            var result = lowerCase
                ? Email.ToLower()
                : Email;
            return new ValueTask<object>(result);
        }
    }

    public enum CustomerType
    {
        Vip,
        Normal
    }

    public class Address 
    {
        public string Province { get; set; }
        public string City { get; set; }
        public string District { get; set; }
        public string Street { get; set; }
    }

    [KnownTypes(typeof(Address1), typeof(Address2))]
    public interface IAddress
    {
         string Province { get; set; }
         string City { get; set; }
         string District { get; set; }
         string Street { get; set; }
    }

    public class Address1: IAddress
    {
        public string Province { get; set; }
        public string City { get; set; }
        public string District { get; set; }
        public string Street { get; set; }
    }

    public class Address2 : IAddress
    {
        public string Province { get; set; }
        public string City { get; set; }
        public string District { get; set; }
        public string Street { get; set; }
    }

    [KnownTypes(typeof(Address1), typeof(Address2))]
    public class DemoGraphService : GraphServiceBase
    {
        [GraphOperation(OperationType.Query)]
        public Task<Customer> GetCustomer([Argument]string name)
        {

            var customer = new Customer
            {
                Id = 123,
                Name = name,
                Type = CustomerType.Vip,
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