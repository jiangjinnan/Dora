using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Lib
{
    public class Foobarbaz
    {
        public Foo[] Foo1 { get; set; } 
        public Foo[] Foo2 { get; set; } 
        public Foo[] Foo3 { get; set; } 

        public static Foobarbaz Create(int elements)=> new Foobarbaz
        {
            Foo1 = Enumerable.Range(1, elements).Select(_ => new Foo()).ToArray(),
            Foo2 = Enumerable.Range(1, elements).Select(_ => new Foo()).ToArray(),
            Foo3 = Enumerable.Range(1, elements).Select(_ => new Foo()).ToArray()
        };
    }
}
