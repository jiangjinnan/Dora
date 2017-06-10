using Dora.ExceptionHandling.Configuration;
using System;
using System.Collections.Generic;
using System.Text;
using Xunit;

namespace Dora.ExceptionHandling.Test
{
    public class MatchTypeFilterFixture
    {
        [Fact]
        public void New()
        {
            Assert.Throws<ArgumentNullException>(() => new MatchTypeFilter(null));
            Assert.Throws<ArgumentException>(() => new MatchTypeFilter(typeof(string)));
            Assert.Same(typeof(ArgumentNullException), new MatchTypeFilter(typeof(ArgumentNullException)).ExceptionType);
        }

        [Fact]
        public void Match()
        {
            var filer = new MatchTypeFilter(typeof(ArgumentException));
            Assert.True(filer.Match(new ExceptionContext(new ArgumentException())));
            Assert.True(filer.Match(new ExceptionContext(new ArgumentNullException())));
            Assert.False(filer.Match(new ExceptionContext(new InvalidOperationException())));

        }
    }
}
