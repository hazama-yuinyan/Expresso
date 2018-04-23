using System;
using Expresso.Ast;
using System.Reflection;
using Expresso.CodeGen;


namespace Expresso
{
    /// <summary>
    /// Provides means of interacting with Expresso's compiler services.
    /// </summary>
    public static class ExpressoCompilerService
    {
        /// <summary>
        /// Parses the file as Expresso source code and returns the abstract syntax tree.
        /// </summary>
        /// <param name="filePath">File path.</param>
        public static ExpressoAst Parse(string filePath, bool doPostParseProcessing)
        {
            var parser = new Parser(new Scanner(filePath));
            parser.DoPostParseProcessing = doPostParseProcessing;
            parser.Parse();

            return parser.TopmostAst;
        }

        /// <summary>
        /// Compiles and assemble into an assembly object.
        /// </summary>
        /// <returns>An assembly object.</returns>
        /// <param name="filePath">Path to the file which will be compiled.</param>
        /// <param name="options">Compiler options to use for the compilation.</param>
        public static Assembly CompileToAssembly(string filePath, ExpressoCompilerOptions options)
        {
            var parser = new Parser(new Scanner(filePath));
            parser.DoPostParseProcessing = true;
            parser.Parse();
            var ast = parser.TopmostAst;
            var emitter = new CSharpEmitter(parser, options);
            ast.AcceptWalker(emitter, new CSharpEmitterContext());
            return emitter.AssemblyBuilder;
        }
    }
}

