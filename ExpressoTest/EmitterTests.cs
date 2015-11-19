using System;
using System.Collections.Generic;
using Expresso.CodeGen;
using NUnit.Framework;
using NUnit.Framework.SyntaxHelpers;
using System.Linq;

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
                BuildType = BuildType.Debug | BuildType.Executable
            };
            var emitter = new CSharpEmitter(parser, options);
            ast.AcceptWalker(emitter, null);

            var asm = emitter.AssemblyBuilder;
            var main_method = asm.GetModule("main").GetMethod("main");
            Assert.AreEqual(main_method.Name, "main");
            Assert.IsTrue(main_method.IsStatic);
            Assert.AreEqual(main_method.ReturnType, typeof(void));
            Assert.AreEqual(0, main_method.GetParameters().Length);
        }

        [Test]
        public void SimpleArithmetic()
        {
            var parser = new Parser(new Scanner("../../sources/for_unit_tests/simple_arithmetic.exs"));
            parser.DoPostParseProcessing = true;
            parser.Parse();

            var ast = parser.TopmostAst;
            var options = new ExpressoCompilerOptions{
                LibraryPaths = new List<string>{""},
                OutputPath = "../../test_executable",
                BuildType = BuildType.Debug | BuildType.Executable
            };
            var emitter = new CSharpEmitter(parser, options);
            ast.AcceptWalker(emitter, null);

            var asm = emitter.AssemblyBuilder;
            var main_method = asm.GetModule("<Module>").GetMethod("main");
            Assert.AreEqual(main_method.Name, "main");
            Assert.IsTrue(main_method.IsStatic);
            Assert.AreEqual(main_method.ReturnType, typeof(void));
            Assert.AreEqual(0, main_method.GetParameters().Length);
            Console.WriteLine("テスト実行");
            Console.Out.WriteLine(main_method.ToString());

            main_method.Invoke(null, new object[]{});
        }
    }
}

