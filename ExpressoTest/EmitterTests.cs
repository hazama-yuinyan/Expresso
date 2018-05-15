using System;
using Expresso.CodeGen;
using NUnit.Framework;
using System.Reflection;
using Expresso.Ast;

namespace Expresso.Test
{
    [TestFixture]
    public class EmitterTests
    {
        [TearDown]
        public void Cleanup()
        {
            CSharpEmitter.Symbols.Clear();
        }

        [Test]
        public void SimpleLiterals()
        {
            var parser = new Parser(new Scanner("../../sources/for_unit_tests/simple_literals.exs"));
            parser.DoPostParseProcessing = true;
            parser.Parse();

            var ast = parser.TopmostAst;
            var options = new ExpressoCompilerOptions{
                OutputPath = "../../test_executables",
                BuildType = BuildType.Debug | BuildType.Executable,
                ExecutableName = "main"
            };
            var emitter = new CSharpEmitter(parser, options);
            ast.AcceptWalker(emitter, null);

            var asm = emitter.AssemblyBuilder;
            var main_method = asm.EntryPoint;
            Assert.AreEqual(main_method.Name, "main");
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
                OutputPath = "../../test_executables",
                BuildType = BuildType.Debug | BuildType.Executable,
                ExecutableName = "main"
            };
            var emitter = new CSharpEmitter(parser, options);
            ast.AcceptWalker(emitter, null);

            var asm = emitter.AssemblyBuilder;
            var main_method = asm.EntryPoint;
            Assert.AreEqual(main_method.Name, "main");
            Assert.IsTrue(main_method.IsStatic);
            Assert.AreEqual(typeof(void), main_method.ReturnType);
            Assert.AreEqual(1, main_method.GetParameters().Length);
            //Assert.IsTrue(main_method.GetParameters().SequenceEqual(new []{typeof(string[])}));
            Console.Out.WriteLine("テスト実行");
            Console.Out.WriteLine(main_method.ToString());

            main_method.Invoke(null, new []{new string[]{"abc"}});
        }

        [Test]
        public void GeneralExpressions()
        {
            var parser = new Parser(new Scanner("../../sources/for_unit_tests/general_expressions.exs"));
            parser.DoPostParseProcessing = true;
            parser.Parse();

            var ast = parser.TopmostAst;
            var options = new ExpressoCompilerOptions{
                OutputPath = "../../test_executables",
                BuildType = BuildType.Debug | BuildType.Executable,
                ExecutableName = "main"
            };
            var emitter = new CSharpEmitter(parser, options);
            ast.AcceptWalker(emitter, null);

            var asm = emitter.AssemblyBuilder;
            var main_method = asm.EntryPoint;
            Assert.AreEqual(main_method.Name, "main");
            Assert.IsTrue(main_method.IsStatic);
            Assert.AreEqual(typeof(void), main_method.ReturnType);
            Assert.AreEqual(0, main_method.GetParameters().Length);
            //Assert.IsTrue(main_method.GetParameters().SequenceEqual(new []{typeof(string[])}));
            Console.Out.WriteLine("テスト実行");
            Console.Out.WriteLine(main_method.ToString());

            main_method.Invoke(null, new object[]{});
        }

        [Test]
        public void BasicStatements()
        {
            var parser = new Parser(new Scanner("../../sources/for_unit_tests/basic_statements.exs"));
            parser.DoPostParseProcessing = true;
            parser.Parse();

            var ast = parser.TopmostAst;
            var options = new ExpressoCompilerOptions{
                OutputPath = "../../test_executables",
                BuildType = BuildType.Debug | BuildType.Executable,
                ExecutableName = "main"
            };
            var emitter = new CSharpEmitter(parser, options);
            ast.AcceptWalker(emitter, null);

            var asm = emitter.AssemblyBuilder;
            var main_method = asm.EntryPoint;
            Assert.AreEqual(main_method.Name, "main");
            Assert.IsTrue(main_method.IsStatic);
            Assert.AreEqual(typeof(void), main_method.ReturnType);
            Assert.AreEqual(0, main_method.GetParameters().Length);
            //Assert.IsTrue(main_method.GetParameters().SequenceEqual(new []{typeof(string[])}));
            Console.Out.WriteLine("テスト実行");
            Console.Out.WriteLine(main_method.ToString());

            main_method.Invoke(null, new object[]{});
        }

        [Test]
        public void ComplexExpressions()
        {
            var parser = new Parser(new Scanner("../../sources/for_unit_tests/complex_expressions.exs"));
            parser.DoPostParseProcessing = true;
            parser.Parse();

            var ast = parser.TopmostAst;
            var options = new ExpressoCompilerOptions{
                OutputPath = "../../test_executables",
                BuildType = BuildType.Debug | BuildType.Executable,
                ExecutableName = "main"
            };
            var emitter = new CSharpEmitter(parser, options);
            ast.AcceptWalker(emitter, null);

            var asm = emitter.AssemblyBuilder;
            var main_method = asm.EntryPoint;
            Assert.AreEqual(main_method.Name, "main");
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
                OutputPath = "../../test_executables",
                BuildType = BuildType.Debug | BuildType.Executable,
                ExecutableName = "main"
            };
            var emitter = new CSharpEmitter(parser, options);
            ast.AcceptWalker(emitter, null);

            var asm = emitter.AssemblyBuilder;
            var main_method = asm.EntryPoint;
            Assert.AreEqual(main_method.Name, "main");
            Assert.IsTrue(main_method.IsStatic);
            Assert.AreEqual(typeof(void), main_method.ReturnType);
            Assert.AreEqual(0, main_method.GetParameters().Length);
            //Assert.IsTrue(main_method.GetParameters().SequenceEqual(new []{typeof(string[])}));
            Console.Out.WriteLine("テスト実行");
            Console.Out.WriteLine(main_method.ToString());

            main_method.Invoke(null, new object[]{});
        }

        [Test]
        public void Functions()
        {
            var parser = new Parser(new Scanner("../../sources/for_unit_tests/functions.exs"));
            parser.DoPostParseProcessing = true;
            parser.Parse();

            var ast = parser.TopmostAst;
            var options = new ExpressoCompilerOptions{
                OutputPath = "../../test_executables",
                BuildType = BuildType.Debug | BuildType.Executable,
                ExecutableName = "main"
            };
            var emitter = new CSharpEmitter(parser, options);
            ast.AcceptWalker(emitter, null);

            var asm = emitter.AssemblyBuilder;
            var main_method = asm.EntryPoint;
            Assert.AreEqual(main_method.Name, "main");
            Assert.IsTrue(main_method.IsStatic);
            Assert.AreEqual(typeof(void), main_method.ReturnType);
            Assert.AreEqual(0, main_method.GetParameters().Length);
            //Assert.IsTrue(main_method.GetParameters().SequenceEqual(new []{typeof(string[])}));
            Console.Out.WriteLine("テスト実行");
            Console.Out.WriteLine(main_method.ToString());

            try{
                main_method.Invoke(null, new object[]{});
            }catch(Exception e){
                Console.WriteLine(e.Message);
                Console.WriteLine(e.StackTrace);
                Console.WriteLine(e.InnerException);
            }
        }

        [Test]
        public void Class()
        {
            var parser = new Parser(new Scanner("../../sources/for_unit_tests/class.exs"));
            parser.DoPostParseProcessing = true;
            parser.Parse();

            var ast = parser.TopmostAst;
            var options = new ExpressoCompilerOptions{
                OutputPath = "../../test_executables",
                BuildType = BuildType.Debug | BuildType.Executable,
                ExecutableName = "main"
            };
            var emitter = new CSharpEmitter(parser, options);
            ast.AcceptWalker(emitter, null);

            var asm = emitter.AssemblyBuilder;
            var main_method = asm.EntryPoint;
            Assert.AreEqual(main_method.Name, "main");
            Assert.IsTrue(main_method.IsStatic);
            Assert.AreEqual(typeof(void), main_method.ReturnType);
            Assert.AreEqual(0, main_method.GetParameters().Length);
            //Assert.IsTrue(main_method.GetParameters().SequenceEqual(new []{typeof(string[])}));
            Console.Out.WriteLine("テスト実行");
            Console.Out.WriteLine(main_method.ToString());

            main_method.Invoke(null, new object[]{});
        }

        [Test]
        public void Module()
        {
            var parser = new Parser(new Scanner("../../sources/for_unit_tests/module.exs"));
            parser.DoPostParseProcessing = true;
            parser.Parse();

            var ast = parser.TopmostAst;

            var options = new ExpressoCompilerOptions{
                OutputPath = "../../test_executables",
                BuildType = BuildType.Debug | BuildType.Executable,
                ExecutableName = "main"
            };
            var emitter = new CSharpEmitter(parser, options);
            ast.AcceptWalker(emitter, null);

            var asm = emitter.AssemblyBuilder;
            var main_method = asm.EntryPoint;
            Assert.AreEqual(main_method.Name, "main");
            Assert.IsTrue(main_method.IsStatic);
            Assert.AreEqual(typeof(void), main_method.ReturnType);
            Assert.AreEqual(0, main_method.GetParameters().Length);

            main_method.Invoke(null, new object[]{});
        }

        [Test]
        public void Closures()
        {
            var parser = new Parser(new Scanner("../../sources/for_unit_tests/closures.exs"));
            parser.DoPostParseProcessing = true;
            parser.Parse();

            var ast = parser.TopmostAst;

            var options = new ExpressoCompilerOptions{
                OutputPath = "../../test_executables",
                BuildType = BuildType.Debug | BuildType.Executable,
                ExecutableName = "main"
            };
            var emitter = new CSharpEmitter(parser, options);
            ast.AcceptWalker(emitter, null);

            var asm = emitter.AssemblyBuilder;
            var main_method = asm.EntryPoint;
            Assert.AreEqual(main_method.Name, "main");
            Assert.IsTrue(main_method.IsStatic);
            Assert.AreEqual(typeof(void), main_method.ReturnType);
            Assert.AreEqual(0, main_method.GetParameters().Length);

            main_method.Invoke(null, new object[]{});
        }

        [Test]
        public void ClosuresWithCompoundStatements()
        {
            var parser = new Parser(new Scanner("../../sources/for_unit_tests/closures_with_compound_statements.exs"));
            parser.DoPostParseProcessing = true;
            parser.Parse();

            var ast = parser.TopmostAst;

            var options = new ExpressoCompilerOptions{
                OutputPath = "../../test_executables",
                BuildType = BuildType.Debug | BuildType.Executable,
                ExecutableName = "main"
            };
            var emitter = new CSharpEmitter(parser, options);
            ast.AcceptWalker(emitter, null);

            var asm = emitter.AssemblyBuilder;
            var main_method = asm.EntryPoint;
            Assert.AreEqual(main_method.Name, "main");
            Assert.IsTrue(main_method.IsStatic);
            Assert.AreEqual(typeof(void), main_method.ReturnType);
            Assert.AreEqual(0, main_method.GetParameters().Length);

            main_method.Invoke(null, new object[]{});
        }

        [Test]
        public void TryStatements()
        {
            var parser = new Parser(new Scanner("../../sources/for_unit_tests/try_statements.exs"));
            parser.DoPostParseProcessing = true;
            parser.Parse();

            var ast = parser.TopmostAst;

            var options = new ExpressoCompilerOptions{
                OutputPath = "../../test_executables",
                BuildType = BuildType.Debug | BuildType.Executable,
                ExecutableName = "main"
            };
            var emitter = new CSharpEmitter(parser, options);
            ast.AcceptWalker(emitter, null);

            var asm = emitter.AssemblyBuilder;
            var main_method = asm.EntryPoint;
            Assert.AreEqual(main_method.Name, "main");
            Assert.IsTrue(main_method.IsStatic);
            Assert.AreEqual(typeof(void), main_method.ReturnType);
            Assert.AreEqual(0, main_method.GetParameters().Length);

            main_method.Invoke(null, new object[]{});
        }

        [Test]
        public void BuiltinObjects()
        {
            var parser = new Parser(new Scanner("../../sources/for_unit_tests/builtin_objects.exs"));
            parser.DoPostParseProcessing = true;
            parser.Parse();

            var ast = parser.TopmostAst;

            var options = new ExpressoCompilerOptions{
                OutputPath = "../../test_executables",
                BuildType = BuildType.Debug | BuildType.Executable,
                ExecutableName = "main"
            };
            var emitter = new CSharpEmitter(parser, options);
            ast.AcceptWalker(emitter, null);

            var asm = emitter.AssemblyBuilder;
            var main_method = asm.EntryPoint;
            Assert.AreEqual(main_method.Name, "main");
            Assert.IsTrue(main_method.IsStatic);
            Assert.AreEqual(typeof(void), main_method.ReturnType);
            Assert.AreEqual(0, main_method.GetParameters().Length);

            main_method.Invoke(null, new object[]{});
        }

        [Test]
        public void Interface()
        {
            var parser = new Parser(new Scanner("../../sources/for_unit_tests/interface.exs"));
            parser.DoPostParseProcessing = true;
            parser.Parse();

            var ast = parser.TopmostAst;

            var options = new ExpressoCompilerOptions{
                OutputPath = "../../test_executables",
                BuildType = BuildType.Debug | BuildType.Executable,
                ExecutableName = "main"
            };
            var emitter = new CSharpEmitter(parser, options);
            ast.AcceptWalker(emitter, null);

            var asm = emitter.AssemblyBuilder;
            var main_method = asm.EntryPoint;
            Assert.AreEqual(main_method.Name, "main");
            Assert.IsTrue(main_method.IsStatic);
            Assert.AreEqual(typeof(void), main_method.ReturnType);
            Assert.AreEqual(0, main_method.GetParameters().Length);

            main_method.Invoke(null, new object[]{});
        }

        [Test]
        public void Slices()
        {
            var parser = new Parser(new Scanner("../../sources/for_unit_tests/slices.exs"));
            parser.DoPostParseProcessing = true;
            parser.Parse();

            var ast = parser.TopmostAst;

            var options = new ExpressoCompilerOptions{
                OutputPath = "../../test_executables",
                BuildType = BuildType.Debug | BuildType.Executable,
                ExecutableName = "main"
            };
            var emitter = new CSharpEmitter(parser, options);
            ast.AcceptWalker(emitter, null);

            var asm = emitter.AssemblyBuilder;
            var main_method = asm.EntryPoint;
            Assert.AreEqual(main_method.Name, "main");
            Assert.IsTrue(main_method.IsStatic);
            Assert.AreEqual(typeof(void), main_method.ReturnType);
            Assert.AreEqual(0, main_method.GetParameters().Length);

            main_method.Invoke(null, new object[]{});
        }

        [Test]
        public void TypeCast()
        {
            var parser = new Parser(new Scanner("../../sources/for_unit_tests/type_cast.exs"));
            parser.DoPostParseProcessing = true;
            parser.Parse();

            var ast = parser.TopmostAst;

            var options = new ExpressoCompilerOptions{
                OutputPath = "../../test_executables",
                BuildType = BuildType.Debug | BuildType.Executable,
                ExecutableName = "main"
            };
            var emitter = new CSharpEmitter(parser, options);
            ast.AcceptWalker(emitter, null);

            var asm = emitter.AssemblyBuilder;
            var main_method = asm.EntryPoint;
            Assert.AreEqual(main_method.Name, "main");
            Assert.IsTrue(main_method.IsStatic);
            Assert.AreEqual(typeof(void), main_method.ReturnType);
            Assert.AreEqual(0, main_method.GetParameters().Length);

            main_method.Invoke(null, new object[]{});
        }

        [Test]
        public void UseOfTheStandardLibrary()
        {
            var parser = new Parser(new Scanner("../../sources/for_unit_tests/use_of_the_standard_library.exs"));
            parser.DoPostParseProcessing = true;
            parser.Parse();

            var ast = parser.TopmostAst;

            var options = new ExpressoCompilerOptions{
                OutputPath = "../../test_executables",
                BuildType = BuildType.Debug | BuildType.Executable,
                ExecutableName = "main"
            };
            var emitter = new CSharpEmitter(parser, options);
            ast.AcceptWalker(emitter, null);

            var asm = emitter.AssemblyBuilder;
            var main_method = asm.EntryPoint;
            Assert.AreEqual(main_method.Name, "main");
            Assert.IsTrue(main_method.IsStatic);
            Assert.AreEqual(typeof(void), main_method.ReturnType);
            Assert.AreEqual(0, main_method.GetParameters().Length);

            main_method.Invoke(null, new object[]{});
        }

        [Test]
        public void VariousStrings()
        {
            var parser = new Parser(new Scanner("../../sources/for_unit_tests/various_strings.exs"));
            parser.DoPostParseProcessing = true;
            parser.Parse();

            var ast = parser.TopmostAst;

            var options = new ExpressoCompilerOptions{
                OutputPath = "../../test_executables",
                BuildType = BuildType.Debug | BuildType.Executable,
                ExecutableName = "main"
            };
            var emitter = new CSharpEmitter(parser, options);
            ast.AcceptWalker(emitter, null);

            var asm = emitter.AssemblyBuilder;
            var main_method = asm.EntryPoint;
            Assert.AreEqual(main_method.Name, "main");
            Assert.IsTrue(main_method.IsStatic);
            Assert.AreEqual(typeof(void), main_method.ReturnType);
            Assert.AreEqual(0, main_method.GetParameters().Length);

            main_method.Invoke(null, new object[]{});
        }

        [Test]
        public void Module2()
        {
            var parser = new Parser(new Scanner("../../sources/for_unit_tests/module2.exs"));
            parser.DoPostParseProcessing = true;
            parser.Parse();

            var ast = parser.TopmostAst;

            var options = new ExpressoCompilerOptions{
                OutputPath = "../../test_executables",
                BuildType = BuildType.Debug | BuildType.Executable,
                ExecutableName = "main"
            };
            var emitter = new CSharpEmitter(parser, options);
            ast.AcceptWalker(emitter, null);

            var asm = emitter.AssemblyBuilder;
            var main_method = asm.EntryPoint;
            Assert.AreEqual(main_method.Name, "main");
            Assert.IsTrue(main_method.IsStatic);
            Assert.AreEqual(typeof(void), main_method.ReturnType);
            Assert.AreEqual(0, main_method.GetParameters().Length);

            main_method.Invoke(null, new object[]{});
        }

        [Test]
        public void InteroperabilityTestWithCSharp()
        {
            var parser = new Parser(new Scanner("../../sources/for_unit_tests/interoperability_test_with_csharp.exs"));
            parser.DoPostParseProcessing = true;
            parser.Parse();

            var ast = parser.TopmostAst;

            var options = new ExpressoCompilerOptions{
                OutputPath = "../../test_executables",
                BuildType = BuildType.Debug | BuildType.Executable,
                ExecutableName = "main"
            };
            var emitter = new CSharpEmitter(parser, options);
            ast.AcceptWalker(emitter, null);

            var asm = emitter.AssemblyBuilder;
            var main_method = asm.EntryPoint;
            Assert.AreEqual(main_method.Name, "main");
            Assert.IsTrue(main_method.IsStatic);
            Assert.AreEqual(typeof(void), main_method.ReturnType);
            Assert.AreEqual(0, main_method.GetParameters().Length);

            main_method.Invoke(null, new object[]{});
        }

        [Test]
        public void AdvancedForLoops()
        {
            var parser = new Parser(new Scanner("../../sources/for_unit_tests/advanced_for_loops.exs"));
            parser.DoPostParseProcessing = true;
            parser.Parse();

            var ast = parser.TopmostAst;

            var options = new ExpressoCompilerOptions{
                OutputPath = "../../test_executables",
                BuildType = BuildType.Debug | BuildType.Executable,
                ExecutableName = "main"
            };
            var emitter = new CSharpEmitter(parser, options);
            ast.AcceptWalker(emitter, null);

            var asm = emitter.AssemblyBuilder;
            var main_method = asm.EntryPoint;
            Assert.AreEqual(main_method.Name, "main");
            Assert.IsTrue(main_method.IsStatic);
            Assert.AreEqual(typeof(void), main_method.ReturnType);
            Assert.AreEqual(0, main_method.GetParameters().Length);

            main_method.Invoke(null, new object[]{});
        }

        [Test]
        public void PropertyTests()
        {
            var parser = new Parser(new Scanner("../../sources/for_unit_tests/property_tests.exs"));
            parser.DoPostParseProcessing = true;
            parser.Parse();

            var ast = parser.TopmostAst;

            var options = new ExpressoCompilerOptions{
                OutputPath = "../../test_executables",
                BuildType = BuildType.Debug | BuildType.Executable,
                ExecutableName = "main"
            };
            var emitter = new CSharpEmitter(parser, options);
            ast.AcceptWalker(emitter, null);

            var asm = emitter.AssemblyBuilder;
            var main_method = asm.EntryPoint;
            Assert.AreEqual(main_method.Name, "main");
            Assert.IsTrue(main_method.IsStatic);
            Assert.AreEqual(typeof(void), main_method.ReturnType);
            Assert.AreEqual(0, main_method.GetParameters().Length);

            main_method.Invoke(null, new object[]{});
        }

        [Test]
        public void TestEnumInCSharp()
        {
            var parser = new Parser(new Scanner("../../sources/for_unit_tests/test_enum_in_csharp.exs"));
            parser.DoPostParseProcessing = true;
            parser.Parse();

            var ast = parser.TopmostAst;

            var options = new ExpressoCompilerOptions{
                OutputPath = "../../test_executables",
                BuildType = BuildType.Debug | BuildType.Executable,
                ExecutableName = "main"
            };
            var emitter = new CSharpEmitter(parser, options);
            ast.AcceptWalker(emitter, null);

            var asm = emitter.AssemblyBuilder;
            var main_method = asm.EntryPoint;
            Assert.AreEqual(main_method.Name, "main");
            Assert.IsTrue(main_method.IsStatic);
            Assert.AreEqual(typeof(void), main_method.ReturnType);
            Assert.AreEqual(0, main_method.GetParameters().Length);

            main_method.Invoke(null, new object[]{});
        }

        [Test]
        public void TestReference()
        {
            var parser = new Parser(new Scanner("../../sources/for_unit_tests/test_reference.exs"));
            parser.DoPostParseProcessing = true;
            parser.Parse();

            var ast = parser.TopmostAst;

            var options = new ExpressoCompilerOptions{
                OutputPath = "../../test_executables",
                BuildType = BuildType.Debug | BuildType.Executable,
                ExecutableName = "main"
            };
            var emitter = new CSharpEmitter(parser, options);
            ast.AcceptWalker(emitter, null);

            var asm = emitter.AssemblyBuilder;
            var main_method = asm.EntryPoint;
            Assert.AreEqual(main_method.Name, "main");
            Assert.IsTrue(main_method.IsStatic);
            Assert.AreEqual(typeof(void), main_method.ReturnType);
            Assert.AreEqual(0, main_method.GetParameters().Length);

            main_method.Invoke(null, new object[]{});
        }

        [Test]
        public void Attributes()
        {
            var parser = new Parser(new Scanner("../../sources/for_unit_tests/attributes.exs"));
            parser.DoPostParseProcessing = true;
            parser.Parse();

            var ast = parser.TopmostAst;

            var options = new ExpressoCompilerOptions{
                OutputPath = "../../test_executables",
                BuildType = BuildType.Debug | BuildType.Executable,
                ExecutableName = "main"
            };
            var emitter = new CSharpEmitter(parser, options);
            ast.AcceptWalker(emitter, null);

            var asm = emitter.AssemblyBuilder;
            var main_method = asm.EntryPoint;
            Assert.AreEqual(main_method.Name, "main");
            Assert.IsTrue(main_method.IsStatic);
            Assert.AreEqual(typeof(void), main_method.ReturnType);
            Assert.AreEqual(0, main_method.GetParameters().Length);

            var attribute1 = asm.GetCustomAttribute<AssemblyDescriptionAttribute>();
            Assert.IsNotNull(attribute1);

            var module = asm.GetModule("main");
            var author_attribute_type = CSharpCompilerHelpers.GetNativeType(AstType.MakeSimpleType("AuthorAttribute"));
            var attribute2 = module.GetCustomAttribute(author_attribute_type);
            Assert.IsNotNull(attribute2);

            main_method.Invoke(null, new object[]{});
        }
    }
}

