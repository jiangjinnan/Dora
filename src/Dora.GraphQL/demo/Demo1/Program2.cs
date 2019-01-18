//using Dora.GraphQL;
//using Dora.GraphQL.GraphTypes;
//using Dora.GraphQL.Schemas;
//using System;
//using System.Threading.Tasks;

//namespace Demo1
//{
//    class Program
//    {
//        static void Main(string[] args)
//        {
//            var accessor = new AttributeAccessor();
//            var factory = new SchemaFactory(accessor, new GraphTypeProvider(accessor));
//            var schema = factory.Create(typeof(Program).Assembly);
//            Console.WriteLine(schema);
//        }
//    }

//    public class FoobarService : GraphServiceBase
//    {
//        [GraphOperation(OperationType.Query)]
//        public Task<Foobarbaz[]> GetFoobarbaz1(
//            [Argument]string foo,
//            [Argument]int bar) => null;

//        [GraphOperation(OperationType.Query)]
//        public Foobarbaz[] GetFoobarbaz2(
//           [Argument]string foo,
//           [Argument]int bar) => null;
//    }

//    public class Foobarbaz
//    {
//        public Foobar Foobar { get; set; }
//        public string Baz { get; set; }
//    }

//    public class Foobar
//    {
//        [Argument(Name = "foo1",Type= typeof(string))]
//        [Argument(Name = "foo2", Type = typeof(string), IsEnumerable = true)]
//        [Argument(Name = "foo3", Type = typeof(string), IsRequired = true)]
//        public string Foo { get; set; }
//        public string Bar { get; set; }
//    }
//}
