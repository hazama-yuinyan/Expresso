using System;
using System.Collections.Generic;
using Expresso.CodeGen;
using NUnit.Framework;
using System.Linq;
using System.Reflection;

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
            var main_method = asm.GetModule("main.exe")
                .GetType("ExsMain")
                .GetMethod("Main", BindingFlags.NonPublic | BindingFlags.Static);
            Assert.AreEqual(main_method.Name, "Main");
            Assert.IsTrue(main_method.IsStatic);
            Assert.AreEqual(typeof(void), main_method.ReturnType);
            Assert.AreEqual(0, main_method.GetParameters().Length);

            main_method.Invoke(null, new object[]{});
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
                OutputPath = "../../test_executables",
                BuildType = BuildType.Debug | BuildType.Executable
            };
            var emitter = new CSharpEmitter(parser, options);
            ast.AcceptWalker(emitter, null);

            var asm = emitter.AssemblyBuilder;
            var main_method = asm.GetModule("main.exe")
                .GetType("ExsMain")
                .GetMethod("Main", BindingFlags.NonPublic | BindingFlags.Static);
            Assert.AreEqual(main_method.Name, "Main");
            Assert.IsTrue(main_method.IsStatic);
            Assert.AreEqual(typeof(void), main_method.ReturnType);
            Assert.AreEqual(1, main_method.GetParameters().Length);
            //Assert.IsTrue(main_method.GetParameters().SequenceEqual(new []{typeof(string[])}));
            Console.Out.WriteLine("テスト実行");
            Console.Out.WriteLine(main_method.ToString());

            //main_method.Invoke(null, new object[]{});
        }

        [Test]
        public void GeneralExpressions()
        {
            var parser = new Parser(new Scanner("../../sources/for_unit_tests/general_expressions.exs"));
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
            var main_method = asm.GetModule("main.exe")
                .GetType("ExsMain")
                .GetMethod("Main", BindingFlags.NonPublic | BindingFlags.Static);
            Assert.AreEqual(main_method.Name, "Main");
            Assert.IsTrue(main_method.IsStatic);
            Assert.AreEqual(typeof(void), main_method.ReturnType);
            Assert.AreEqual(0, main_method.GetParameters().Length);
            //Assert.IsTrue(main_method.GetParameters().SequenceEqual(new []{typeof(string[])}));
            Console.Out.WriteLine("テスト実行");
            Console.Out.WriteLine(main_method.ToString());

            //main_method.Invoke(null, new object[]{});
        }

        [Test]
        public void BasicStatements()
        {
            var parser = new Parser(new Scanner("../../sources/for_unit_tests/basic_statements.exs"));
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
            var main_method = asm.GetModule("main.exe")
                .GetType("ExsMain")
                .GetMethod("Main", BindingFlags.NonPublic | BindingFlags.Static);
            Assert.AreEqual(main_method.Name, "Main");
            Assert.IsTrue(main_method.IsStatic);
            Assert.AreEqual(typeof(void), main_method.ReturnType);
            Assert.AreEqual(0, main_method.GetParameters().Length);
            //Assert.IsTrue(main_method.GetParameters().SequenceEqual(new []{typeof(string[])}));
            Console.Out.WriteLine("テスト実行");
            Console.Out.WriteLine(main_method.ToString());

            main_method.Invoke(null, new object[]{});
        }

        [Test]
        public void MatchStatements()
        {
            var parser = new Parser(new Scanner("../../sources/for_unit_tests/match_statements.exs"));
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
            var main_method = asm.GetModule("main.exe")
                .GetType("ExsMain")
                .GetMethod("Main", BindingFlags.NonPublic | BindingFlags.Static);
            Assert.AreEqual(main_method.Name, "Main");
            Assert.IsTrue(main_method.IsStatic);
            Assert.AreEqual(typeof(void), main_method.ReturnType);
            Assert.AreEqual(0, main_method.GetParameters().Length);
            //Assert.IsTrue(main_method.GetParameters().SequenceEqual(new []{typeof(string[])}));
            Console.Out.WriteLine("テスト実行");
            Console.Out.WriteLine(main_method.ToString());

            //main_method.Invoke(null, new object[]{});
        }

        [Test]
        public void Functions()
        {
            var parser = new Parser(new Scanner("../../sources/for_unit_tests/functions.exs"));
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
            var main_method = asm.GetModule("main.exe")
                .GetType("ExsMain")
                .GetMethod("Main", BindingFlags.NonPublic | BindingFlags.Static);
            Assert.AreEqual(main_method.Name, "Main");
            Assert.IsTrue(main_method.IsStatic);
            Assert.AreEqual(typeof(int), main_method.ReturnType);
            Assert.AreEqual(0, main_method.GetParameters().Length);
            //Assert.IsTrue(main_method.GetParameters().SequenceEqual(new []{typeof(string[])}));
            Console.Out.WriteLine("テスト実行");
            Console.Out.WriteLine(main_method.ToString());

            //main_method.Invoke(null, new object[]{});
        }

        [Test]
        public void Class()
        {
            var parser = new Parser(new Scanner("../../sources/for_unit_tests/class.exs"));
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
            var main_method = asm.GetModule("main.exe")
                .GetType("ExsMain")
                .GetMethod("Main", BindingFlags.NonPublic | BindingFlags.Static);
            Assert.AreEqual(main_method.Name, "Main");
            Assert.IsTrue(main_method.IsStatic);
            Assert.AreEqual(typeof(int), main_method.ReturnType);
            Assert.AreEqual(0, main_method.GetParameters().Length);
            //Assert.IsTrue(main_method.GetParameters().SequenceEqual(new []{typeof(string[])}));
            Console.Out.WriteLine("テスト実行");
            Console.Out.WriteLine(main_method.ToString());

            //main_method.Invoke(null, new object[]{});
        }
    }
}

