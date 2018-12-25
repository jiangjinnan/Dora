using Dora.OAuthServer;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Server
{
    public class MyDbContext : OAuthDbContext
    {
        public MyDbContext(DbContextOptions options) : base(options)
        {
            Database.EnsureCreated();
        }
    }
}
