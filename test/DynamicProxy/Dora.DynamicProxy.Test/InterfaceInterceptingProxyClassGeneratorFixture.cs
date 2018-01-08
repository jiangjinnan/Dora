using System;
using System.Collections.Generic;
using System.Text;

namespace Dora.DynamicProxy.Test
{
    public class InterfaceInterceptingProxyClassGeneratorFixture
    {

        public interface ICalculator
        {
            double Add(double x, double y);
        }

        public class Calculator : ICalculator
        {
            public double Add(double x, double y)
            {
                throw new NotImplementedException();
            }
        }
    }
}
