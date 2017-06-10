using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace Dora.ExceptionHandling.Test
{
    public class ExceptionPolicyFixture
    {
        [Theory]
        [InlineData(null, "1", "1")]
        [InlineData("1", null, "1")]
        [InlineData("1", "1", null)]
        public void New_Arguments_Not_Allow_Null(string entriesIndicator, string preHandlerIndicator, string postHandlerIndicator)
        {
            IEnumerable<ExceptionPolicyEntry> entries = entriesIndicator == null
                ? null
                : new ExceptionPolicyEntry[] { new ExceptionPolicyEntry(typeof(Exception), PostHandlingAction.None, _ => Task.CompletedTask) };
            Func<ExceptionContext, Task> preHandler = preHandlerIndicator == null
                ? null as Func<ExceptionContext, Task>
                : _ => Task.CompletedTask;
            Func<ExceptionContext, Task> postHandler = postHandlerIndicator == null
               ? null as Func<ExceptionContext, Task>
               : _ => Task.CompletedTask;

            Assert.Throws<ArgumentNullException>(() => new ExceptionPolicy(entries, preHandler, postHandler));
        }

        [Fact]
        public void New_Add_Global_Entry()
        {
            var policy = new ExceptionPolicy(new ExceptionPolicyEntry[0], _ => Task.CompletedTask, _ => Task.CompletedTask);
            Assert.True(policy.Entries.Any(it => it.ExceptionType == typeof(Exception)));
        }

        [Fact]
        public void New_Not_Allow_Duplicate_Exception_Type()
        {
            var entries = new ExceptionPolicyEntry[] {
                new ExceptionPolicyEntry(typeof(InvalidCastException), PostHandlingAction.None, _=>Task.CompletedTask),
                new ExceptionPolicyEntry(typeof(InvalidCastException), PostHandlingAction.None, _=>Task.CompletedTask),
            };
            Assert.Throws<ArgumentException>(() => new ExceptionPolicy(entries, _ => Task.CompletedTask, _ => Task.CompletedTask));
        }

        [Fact]
        public void New_Normal()
        {
            var entries = new ExceptionPolicyEntry[] { new ExceptionPolicyEntry(typeof(Exception), PostHandlingAction.None, _ => Task.CompletedTask) };
            Func<ExceptionContext, Task> preHandler = _ => Task.CompletedTask;
            Func<ExceptionContext, Task> postHanlder = _ => Task.CompletedTask;
            var policy = new ExceptionPolicy(entries, preHandler, postHanlder);
            Assert.Same(entries.Single(), policy.Entries.Single());
            Assert.Same(preHandler, policy.PreHandler);
            Assert.Same(postHanlder, policy.PostHandler);
        }

        [Fact]
        public void GetPolicyEntry_Arguments_Not_Allow_Null()
        {
            var entry = new ExceptionPolicyEntry(typeof(Exception), PostHandlingAction.None, _ => Task.CompletedTask);
            var policy = new ExceptionPolicy(new ExceptionPolicyEntry[] { entry }, _ => Task.CompletedTask, _ => Task.CompletedTask);
            Assert.Throws<ArgumentNullException>(() => policy.GetPolicyEntry(null));
        }

        [Fact]
        public void GetPolicyEntry_Normal()
        {
            var entry1 = new ExceptionPolicyEntry(typeof(Exception), PostHandlingAction.None, _ => Task.CompletedTask);
            var entry2 = new ExceptionPolicyEntry(typeof(FooException), PostHandlingAction.None, _ => Task.CompletedTask);
            var entry3 = new ExceptionPolicyEntry(typeof(BarException), PostHandlingAction.None, _ => Task.CompletedTask);
            var policy = new ExceptionPolicy(new ExceptionPolicyEntry[] { entry1, entry2, entry3 }, _ => Task.CompletedTask, _ => Task.CompletedTask);

            Assert.Same(entry1, policy.GetPolicyEntry(typeof(Exception)));
            Assert.Same(entry2, policy.GetPolicyEntry(typeof(FooException)));
            Assert.Same(entry3, policy.GetPolicyEntry(typeof(BarException)));
            Assert.Same(entry3, policy.GetPolicyEntry(typeof(BazException)));
            Assert.Same(entry1, policy.GetPolicyEntry(typeof(GuzException)));
        }    

        private class FooException : Exception { }
        private class BarException : FooException { }
        private class BazException : BarException { }
        private class GuzException : Exception { }

        [Fact]
        public void CreateExceptionHandler_Arguments_Not_Allow_Null()
        {
            var entry = new ExceptionPolicyEntry(typeof(Exception), PostHandlingAction.None, _ => Task.CompletedTask);
            var policy = new ExceptionPolicy(new ExceptionPolicyEntry[] { entry }, _ => Task.CompletedTask, _ => Task.CompletedTask);
            Assert.Throws<ArgumentNullException>(() => policy.CreateHandler(null, out PostHandlingAction action));
        }

        [Fact]
        public async void CreateExceptionHandler_Normal()
        {
            _flag = "";
            var entry = new ExceptionPolicyEntry(typeof(Exception), PostHandlingAction.ThrowOriginal, _ => { _flag += "1"; return Task.CompletedTask; });
            var policy = new ExceptionPolicy(new ExceptionPolicyEntry[] { entry }, _ => { _flag += "2"; return Task.CompletedTask; }, _ => { _flag += "3"; return Task.CompletedTask; });
            var handler = policy.CreateHandler(new Exception(), out PostHandlingAction action);
            await handler(new ExceptionContext(new Exception()));
            Assert.Equal("213", _flag);
            Assert.Equal(PostHandlingAction.ThrowOriginal, action);
        }
        private static string _flag = "";
    }
}
