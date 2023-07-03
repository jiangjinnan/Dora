using Microsoft.Extensions.DependencyInjection;
using System.Reflection;
using System.Text;

namespace Dora.Interception.CodeGeneration
{
    /// <summary>
    /// Interceptable proxy classes code generation context.
    /// </summary>
    public sealed class CodeGenerationContext
    {
        private readonly StringBuilder _writer = new();

        /// <summary>
        /// Gets the referenced assemblies.
        /// </summary>
        /// <value>
        /// The referenced assemblies.
        /// </value>
        public ISet<Assembly> References { get; } = new HashSet<Assembly> { 
            typeof(object).Assembly, 
            Assembly.Load(new AssemblyName("System.Runtime")),
            Assembly.Load(new AssemblyName("System.ComponentModel")),
            typeof(IServiceCollection).Assembly,
            typeof(CodeGenerationContext).Assembly
        };

        /// <summary>
        /// Add reference, support generic type
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        public ISet<Assembly> AddReference(Type type)
        {
            if (type.IsGenericType)
            {
                var types = type.GetGenericArguments();
                foreach (var t in types)
                   return AddReference(t);
            }
            else
                this.References.Add(type.Assembly);
            return References;
        }

        /// <summary>Gets the indent level.</summary>
        /// <value>The indent level.</value>
        public int IndentLevel { get; private set; }

        /// <summary>
        /// Gets the generated source code.
        /// </summary>
        /// <value>
        /// The generated source code.
        /// </value>
        public string SourceCode => _writer.ToString();

        /// <summary>Writes one or more lines of source code.</summary>
        /// <param name="lines">The source code to write.</param>
        /// <returns>The current <see cref="CodeGenerationContext"/></returns>
        public CodeGenerationContext WriteLines(params string[] lines)
        {
            if (lines.Length == 0)
            {
                _writer.AppendLine();
            }
            lines = lines.SelectMany(it => it.Split('\n')).ToArray();
            foreach (var line in lines)
            {
                _writer.AppendLine($"{new string(' ', 4 * IndentLevel)}{line}");
            }
            return this;
        }

        /// <summary>
        /// Create a code block with specified block mark.
        /// </summary>
        /// <param name="start">The start mark of the block, which is "{" by default.</param>
        /// <param name="end">The end mark of the block, which is "}" by default.</param>
        /// <returns>A <see cref="IDisposable"/> representing the code block.</returns>
        public IDisposable CodeBlock(string? start = null, string? end = null) => new CodeBlockScope(this, start??"{", end??"}");

        /// <summary>
        /// Create an indent block.
        /// </summary>
        /// <returns>A <see cref="IDisposable"/> representing the indent block.</returns>
        public IDisposable Indent() => new IndentScope(this);

        private class CodeBlockScope : IDisposable
        {
            private readonly CodeGenerationContext _context;
            private readonly string _end;            

            public CodeBlockScope(CodeGenerationContext context, string start, string end) 
            {
                _end = end;
                _context = context;
                _context.WriteLines(start);
                _context.IndentLevel++;
            }

            public void Dispose()
            {
                _context.IndentLevel--;
                _context.WriteLines(_end);
            }
        }

        private class IndentScope : IDisposable
        {
            private readonly CodeGenerationContext _context;

            public IndentScope(CodeGenerationContext context)
            {
                _context = context;
                _context.IndentLevel++;
            }

            public void Dispose()=> _context.IndentLevel--;
        }
    }
}
