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
        public ContactInfo ContactInfo { get; set; }
    }

    public class ContactInfo
    {
        public string PhoneNo { get; set; }

        [Argument(Name = "lowerCase",Type = typeof(bool))]
        [GraphMember(Resolver = nameof(Resolve))]
        public string Email { get; set; }

        [UnionType(typeof(Address1), typeof(Address2))]
        public IList<object> Addresses { get; set; }
        public ValueTask<object> Resolve(ResolverContext context)
        {
            var lowerCase = context.GetArgument<bool>("lowerCase");
            var result = lowerCase
                ? Email.ToLower()
                : Email;
            return new ValueTask<object>(result);
        }
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
}
