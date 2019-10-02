using System;
using System.Collections.Generic;
using System.Text;

namespace Dora.Interception
{
    public class CodeGeneratorFactory : ICodeGeneratorFactory
    {
        public ICodeGenerator Create() => new CodeGenerator();
    }
}
