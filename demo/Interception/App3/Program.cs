using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;

namespace App
{
    public class Program
    {
        public static void Main()
        {
            new WebHostBuilder()
                 .UseKestrel()
                 .UseStartup<Startup>()
                 .Build()
                 .Run();
        }
    }
}
