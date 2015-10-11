using System;
using System.Collections.Generic;
using Expresso.CodeGen;
using NUnit.Framework;

namespace Expresso.Test
{
    [TestFixture]
    public class EmitterTests
    {
        [Test]
        public void SimpleLiterals()
        {
            var parser = new Parser(new Scanner("../../sources/for_unit_tests/simple_literals.exs"));
            parser.DoPostParseProcessing = true;
            parser.Parse();

            var ast = parser.TopmostAst;
            var options = new ExpressoCompilerOptions{
                LibraryPaths = new List<string>{""},
                OutputPath = "../../test_executables",
                BuildType = BuildType.Debug
            };
            var emitter = new CSharpEmitter(parser, options);
            ast.AcceptWalker(emitter, null);

            var asm = emitter.AssemblyBuilder;
            var main_method = asm.GetModule("main").GetMethod("main");
        }
    }
}

