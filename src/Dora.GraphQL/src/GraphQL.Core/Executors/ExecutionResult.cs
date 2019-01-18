using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;

namespace Dora.GraphQL.Executors
{
    public class  ExecutionResult
    {
        public object Data { get; set; }
        public ICollection<string> Errors { get; }
        public ExecutionResult()
        {
            Errors = new Collection<string>();
        }
    }
}
