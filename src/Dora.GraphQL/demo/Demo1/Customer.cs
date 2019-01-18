using Dora.GraphQL.GraphTypes;
using Dora.GraphQL.Schemas;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Demo1
{
    public class Customer
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public ContactInfo ContactInfo { get; set; }
    }

    public class ContactInfo
    {
        public string PhoneNo { get; set; }
        [Argument(Name = "lowerCase",Type = typeof(bool))]
        [GraphMember(Resolver = nameof(Resolve))]
        public string Email { get; set; }
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

    public class Address
    {
        public string Province { get; set; }
        public string City { get; set; }
        public string District { get; set; }
        public string Street { get; set; }
    }
}
