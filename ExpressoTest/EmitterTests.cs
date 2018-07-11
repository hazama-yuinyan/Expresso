using System;
using Expresso.CodeGen;
using NUnit.Framework;
using System.Reflection;
using Expresso.Ast;
using System.Linq;
using System.Diagnostics;

namespace Expresso.Test
{
    [TestFixture]
    public class EmitterTests
    {
        [TearDown]
        public void Cleanup()
        {
            CodeGenerator.Symbols.Clear();
        }

        [Test]
        public void SimpleLiterals()
        {
            Console.WriteLine("Test SimpleLiterals");

            var parser = new Parser(new Scanner("../../sources/for_unit_tests/simple_literals.exs"));
            parser.DoPostParseProcessing = true;
            parser.Parse();

            var ast = parser.TopmostAst;
            var options = new ExpressoCompilerOptions{
                OutputPath = "../../test_executables",
                BuildType = BuildType.Debug | BuildType.Executable,
                ExecutableName = "main"
            };
            var generator = new CodeGenerator(parser, options);
            ast.AcceptWalker(generator, null);

            var asm = generator.AssemblyBuilder;
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
            Console.WriteLine("Test SimpleArithmetic");

            var parser = new Parser(new Scanner("../../sources/for_unit_tests/simple_arithmetic.exs"));
            parser.DoPostParseProcessing = true;
            parser.Parse();

            var ast = parser.TopmostAst;
            var options = new ExpressoCompilerOptions{
                OutputPath = "../../test_executables",
                BuildType = BuildType.Debug | BuildType.Executable,
                ExecutableName = "main"
            };
            var generator = new CodeGenerator(parser, options);
            ast.AcceptWalker(generator, null);

            var asm = generator.AssemblyBuilder;
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
            Console.WriteLine("Test GeneralExpressions");

            var parser = new Parser(new Scanner("../../sources/for_unit_tests/general_expressions.exs"));
            parser.DoPostParseProcessing = true;
            parser.Parse();

            var ast = parser.TopmostAst;
            var options = new ExpressoCompilerOptions{
                OutputPath = "../../test_executables",
                BuildType = BuildType.Debug | BuildType.Executable,
                ExecutableName = "main"
            };
            var generator = new CodeGenerator(parser, options);
            ast.AcceptWalker(generator, null);

            var asm = generator.AssemblyBuilder;
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
            Console.WriteLine("Test BasicStatements");

            var parser = new Parser(new Scanner("../../sources/for_unit_tests/basic_statements.exs"));
            parser.DoPostParseProcessing = true;
            parser.Parse();

            var ast = parser.TopmostAst;
            var options = new ExpressoCompilerOptions{
                OutputPath = "../../test_executables",
                BuildType = BuildType.Debug | BuildType.Executable,
                ExecutableName = "main"
            };
            var generator = new CodeGenerator(parser, options);
            ast.AcceptWalker(generator, null);

            var asm = generator.AssemblyBuilder;
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
            Console.WriteLine("Test ComplextExpressions");

            var parser = new Parser(new Scanner("../../sources/for_unit_tests/complex_expressions.exs"));
            parser.DoPostParseProcessing = true;
            parser.Parse();

            var ast = parser.TopmostAst;
            var options = new ExpressoCompilerOptions{
                OutputPath = "../../test_executables",
                BuildType = BuildType.Debug | BuildType.Executable,
                ExecutableName = "main"
            };
            var generator = new CodeGenerator(parser, options);
            ast.AcceptWalker(generator, null);

            var asm = generator.AssemblyBuilder;
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
            Console.WriteLine("Test MatchStatements");

            var parser = new Parser(new Scanner("../../sources/for_unit_tests/match_statements.exs"));
            parser.DoPostParseProcessing = true;
            parser.Parse();

            var ast = parser.TopmostAst;
            var options = new ExpressoCompilerOptions{
                OutputPath = "../../test_executables",
                BuildType = BuildType.Debug | BuildType.Executable,
                ExecutableName = "main"
            };
            var generator = new CodeGenerator(parser, options);
            ast.AcceptWalker(generator, null);

            var asm = generator.AssemblyBuilder;
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
            Console.WriteLine("Test Functions");

            var parser = new Parser(new Scanner("../../sources/for_unit_tests/functions.exs"));
            parser.DoPostParseProcessing = true;
            parser.Parse();

            var ast = parser.TopmostAst;
            var options = new ExpressoCompilerOptions{
                OutputPath = "../../test_executables",
                BuildType = BuildType.Debug | BuildType.Executable,
                ExecutableName = "main"
            };
            var generator = new CodeGenerator(parser, options);
            ast.AcceptWalker(generator, null);

            var asm = generator.AssemblyBuilder;
            var main_method = asm.EntryPoint;
            Assert.AreEqual(main_method.Name, "main");
            Assert.IsTrue(main_method.IsStatic);
            Assert.AreEqual(typeof(void), main_method.ReturnType);
            Assert.AreEqual(0, main_method.GetParameters().Length);
            Console.Out.WriteLine("テスト実行");
            Console.Out.WriteLine(main_method.ToString());

            main_method.Invoke(null, new object[]{});
        }

        [Test]
        public void Class()
        {
            Console.WriteLine("Test Class");

            var parser = new Parser(new Scanner("../../sources/for_unit_tests/class.exs"));
            parser.DoPostParseProcessing = true;
            parser.Parse();

            var ast = parser.TopmostAst;
            var options = new ExpressoCompilerOptions{
                OutputPath = "../../test_executables",
                BuildType = BuildType.Debug | BuildType.Executable,
                ExecutableName = "main"
            };
            var generator = new CodeGenerator(parser, options);
            ast.AcceptWalker(generator, null);

            var asm = generator.AssemblyBuilder;
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
            Console.WriteLine("Test Module");

            var parser = new Parser(new Scanner("../../sources/for_unit_tests/module.exs"));
            parser.DoPostParseProcessing = true;
            parser.Parse();

            var ast = parser.TopmostAst;

            var options = new ExpressoCompilerOptions{
                OutputPath = "../../test_executables",
                BuildType = BuildType.Debug | BuildType.Executable,
                ExecutableName = "main"
            };
            var generator = new CodeGenerator(parser, options);
            ast.AcceptWalker(generator, null);

            var asm = generator.AssemblyBuilder;
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
            Console.WriteLine("Test Closures");

            var parser = new Parser(new Scanner("../../sources/for_unit_tests/closures.exs"));
            parser.DoPostParseProcessing = true;
            parser.Parse();

            var ast = parser.TopmostAst;

            var options = new ExpressoCompilerOptions{
                OutputPath = "../../test_executables",
                BuildType = BuildType.Debug | BuildType.Executable,
                ExecutableName = "main"
            };
            var generator = new CodeGenerator(parser, options);
            ast.AcceptWalker(generator, null);

            var asm = generator.AssemblyBuilder;
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
            Console.WriteLine("Test ClosuresWithCompoundStatements");

            var parser = new Parser(new Scanner("../../sources/for_unit_tests/closures_with_compound_statements.exs"));
            parser.DoPostParseProcessing = true;
            parser.Parse();

            var ast = parser.TopmostAst;

            var options = new ExpressoCompilerOptions{
                OutputPath = "../../test_executables",
                BuildType = BuildType.Debug | BuildType.Executable,
                ExecutableName = "main"
            };
            var generator = new CodeGenerator(parser, options);
            ast.AcceptWalker(generator, null);

            var asm = generator.AssemblyBuilder;
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
            Console.WriteLine("Test TryStatements");

            var parser = new Parser(new Scanner("../../sources/for_unit_tests/try_statements.exs"));
            parser.DoPostParseProcessing = true;
            parser.Parse();

            var ast = parser.TopmostAst;

            var options = new ExpressoCompilerOptions{
                OutputPath = "../../test_executables",
                BuildType = BuildType.Debug | BuildType.Executable,
                ExecutableName = "main"
            };
            var generator = new CodeGenerator(parser, options);
            ast.AcceptWalker(generator, null);

            var asm = generator.AssemblyBuilder;
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
            Console.WriteLine("Test BuiltinObjects");

            var parser = new Parser(new Scanner("../../sources/for_unit_tests/builtin_objects.exs"));
            parser.DoPostParseProcessing = true;
            parser.Parse();

            var ast = parser.TopmostAst;

            var options = new ExpressoCompilerOptions{
                OutputPath = "../../test_executables",
                BuildType = BuildType.Debug | BuildType.Executable,
                ExecutableName = "main"
            };
            var generator = new CodeGenerator(parser, options);
            ast.AcceptWalker(generator, null);

            var asm = generator.AssemblyBuilder;
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
            Console.WriteLine("Test Interface");

            var parser = new Parser(new Scanner("../../sources/for_unit_tests/interface.exs"));
            parser.DoPostParseProcessing = true;
            parser.Parse();

            var ast = parser.TopmostAst;

            var options = new ExpressoCompilerOptions{
                OutputPath = "../../test_executables",
                BuildType = BuildType.Debug | BuildType.Executable,
                ExecutableName = "main"
            };
            var generator = new CodeGenerator(parser, options);
            ast.AcceptWalker(generator, null);

            var asm = generator.AssemblyBuilder;
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
            Console.WriteLine("Test Slices");

            var parser = new Parser(new Scanner("../../sources/for_unit_tests/slices.exs"));
            parser.DoPostParseProcessing = true;
            parser.Parse();

            var ast = parser.TopmostAst;

            var options = new ExpressoCompilerOptions{
                OutputPath = "../../test_executables",
                BuildType = BuildType.Debug | BuildType.Executable,
                ExecutableName = "main"
            };
            var generator = new CodeGenerator(parser, options);
            ast.AcceptWalker(generator, null);

            var asm = generator.AssemblyBuilder;
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
            Console.WriteLine("Test TypeCast");

            var parser = new Parser(new Scanner("../../sources/for_unit_tests/type_cast.exs"));
            parser.DoPostParseProcessing = true;
            parser.Parse();

            var ast = parser.TopmostAst;

            var options = new ExpressoCompilerOptions{
                OutputPath = "../../test_executables",
                BuildType = BuildType.Debug | BuildType.Executable,
                ExecutableName = "main"
            };
            var generator = new CodeGenerator(parser, options);
            ast.AcceptWalker(generator, null);

            var asm = generator.AssemblyBuilder;
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
            Console.WriteLine("Test UseOfTheStandardLibrary");

            var parser = new Parser(new Scanner("../../sources/for_unit_tests/use_of_the_standard_library.exs"));
            parser.DoPostParseProcessing = true;
            parser.Parse();

            var ast = parser.TopmostAst;

            var options = new ExpressoCompilerOptions{
                OutputPath = "../../test_executables",
                BuildType = BuildType.Debug | BuildType.Executable,
                ExecutableName = "main"
            };
            var generator = new CodeGenerator(parser, options);
            ast.AcceptWalker(generator, null);

            var asm = generator.AssemblyBuilder;
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
            Console.WriteLine("Test VariousStrings");

            var parser = new Parser(new Scanner("../../sources/for_unit_tests/various_strings.exs"));
            parser.DoPostParseProcessing = true;
            parser.Parse();

            var ast = parser.TopmostAst;

            var options = new ExpressoCompilerOptions{
                OutputPath = "../../test_executables",
                BuildType = BuildType.Debug | BuildType.Executable,
                ExecutableName = "main"
            };
            var generator = new CodeGenerator(parser, options);
            ast.AcceptWalker(generator, null);

            var asm = generator.AssemblyBuilder;
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
            Console.WriteLine("Test Module2");

            var parser = new Parser(new Scanner("../../sources/for_unit_tests/module2.exs"));
            parser.DoPostParseProcessing = true;
            parser.Parse();

            var ast = parser.TopmostAst;

            var options = new ExpressoCompilerOptions{
                OutputPath = "../../test_executables",
                BuildType = BuildType.Debug | BuildType.Executable,
                ExecutableName = "main"
            };
            var generator = new CodeGenerator(parser, options);
            ast.AcceptWalker(generator, null);

            var asm = generator.AssemblyBuilder;
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
            Console.WriteLine("Test InteroperabilityTestWithCSharp");

            var parser = new Parser(new Scanner("../../sources/for_unit_tests/interoperability_test_with_csharp.exs"));
            parser.DoPostParseProcessing = true;
            parser.Parse();

            var ast = parser.TopmostAst;

            var options = new ExpressoCompilerOptions{
                OutputPath = "../../test_executables",
                BuildType = BuildType.Debug | BuildType.Executable,
                ExecutableName = "main"
            };
            var generator = new CodeGenerator(parser, options);
            ast.AcceptWalker(generator, null);

            var asm = generator.AssemblyBuilder;
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
            Console.WriteLine("Test AdvancedForLoops");

            var parser = new Parser(new Scanner("../../sources/for_unit_tests/advanced_for_loops.exs"));
            parser.DoPostParseProcessing = true;
            parser.Parse();

            var ast = parser.TopmostAst;

            var options = new ExpressoCompilerOptions{
                OutputPath = "../../test_executables",
                BuildType = BuildType.Debug | BuildType.Executable,
                ExecutableName = "main"
            };
            var generator = new CodeGenerator(parser, options);
            ast.AcceptWalker(generator, null);

            var asm = generator.AssemblyBuilder;
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
            Console.WriteLine("Test PropertyTests");

            var parser = new Parser(new Scanner("../../sources/for_unit_tests/property_tests.exs"));
            parser.DoPostParseProcessing = true;
            parser.Parse();

            var ast = parser.TopmostAst;

            var options = new ExpressoCompilerOptions{
                OutputPath = "../../test_executables",
                BuildType = BuildType.Debug | BuildType.Executable,
                ExecutableName = "main"
            };
            var generator = new CodeGenerator(parser, options);
            ast.AcceptWalker(generator, null);

            var asm = generator.AssemblyBuilder;
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
            Console.WriteLine("Test TestEnumInCSharp");

            var parser = new Parser(new Scanner("../../sources/for_unit_tests/test_enum_in_csharp.exs"));
            parser.DoPostParseProcessing = true;
            parser.Parse();

            var ast = parser.TopmostAst;

            var options = new ExpressoCompilerOptions{
                OutputPath = "../../test_executables",
                BuildType = BuildType.Debug | BuildType.Executable,
                ExecutableName = "main"
            };
            var generator = new CodeGenerator(parser, options);
            ast.AcceptWalker(generator, null);

            var asm = generator.AssemblyBuilder;
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
            Console.WriteLine("Test TestReference");

            var parser = new Parser(new Scanner("../../sources/for_unit_tests/test_reference.exs"));
            parser.DoPostParseProcessing = true;
            parser.Parse();

            var ast = parser.TopmostAst;

            var options = new ExpressoCompilerOptions{
                OutputPath = "../../test_executables",
                BuildType = BuildType.Debug | BuildType.Executable,
                ExecutableName = "main"
            };
            var generator = new CodeGenerator(parser, options);
            ast.AcceptWalker(generator, null);

            var asm = generator.AssemblyBuilder;
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
            Console.WriteLine("Test Attributes");

            var parser = new Parser(new Scanner("../../sources/for_unit_tests/attributes.exs"));
            parser.DoPostParseProcessing = true;
            parser.Parse();

            var ast = parser.TopmostAst;

            var options = new ExpressoCompilerOptions{
                OutputPath = "../../test_executables",
                BuildType = BuildType.Debug | BuildType.Executable,
                ExecutableName = "main"
            };
            var generator = new CodeGenerator(parser, options);
            ast.AcceptWalker(generator, null);

            var asm = generator.AssemblyBuilder;
            var main_method = asm.EntryPoint;
            Assert.AreEqual(main_method.Name, "main");
            Assert.IsTrue(main_method.IsStatic);
            Assert.AreEqual(typeof(void), main_method.ReturnType);
            Assert.AreEqual(0, main_method.GetParameters().Length);

            var attribute1 = asm.GetCustomAttribute<AssemblyDescriptionAttribute>();
            var type1 = attribute1.GetType();
            Assert.IsNotNull(attribute1);
            Assert.AreEqual(type1.Name, "AssemblyDescriptionAttribute");

#if WINDOWS
            var module = asm.GetModule("main.exe");
#else
            var module = asm.GetModule("main");
            #endif
            var author_attribute_type = module.GetType("AuthorAttribute");
            var attribute2 = module.GetCustomAttributes(true).First();
            var type2 = attribute2.GetType();
            Assert.IsNotNull(attribute2);
            Assert.AreEqual(type2.Name, "CLSCompliantAttribute");

            var test_type = module.GetType("AttributeTest");
            var attribute3 = test_type.GetCustomAttribute<SerializableAttribute>();
            var type3 = attribute3.GetType();
            Assert.IsNotNull(attribute3);
            Assert.AreEqual(type3.Name, "SerializableAttribute");

            var do_something_method = test_type.GetMethod("doSomething");
            var attribute4 = do_something_method.GetCustomAttribute<ObsoleteAttribute>();
            var type4 = attribute4.GetType();
            Assert.IsNotNull(attribute4);
            Assert.AreEqual(type4.Name, "ObsoleteAttribute");

            var x_field = test_type.GetField("x", BindingFlags.NonPublic | BindingFlags.Instance);
            var attribute5 = x_field.GetCustomAttribute<ConditionalAttribute>();
            var type5 = attribute5.GetType();
            Assert.IsNotNull(attribute5);
            Assert.AreEqual(type5.Name, "ConditionalAttribute");

            var module_type = module.GetType("Main");
            var do_something_in_module_method = module_type.GetMethod("doSomethingInModule", BindingFlags.NonPublic | BindingFlags.Static);
            var attribute6 = do_something_in_module_method.GetCustomAttribute<ObsoleteAttribute>();
            var type6 = attribute6.GetType();
            Assert.IsNotNull(attribute6);
            Assert.AreEqual(type6.Name, "ObsoleteAttribute");

            var y_field = module_type.GetField("y", BindingFlags.NonPublic | BindingFlags.Static);
            var attribute7 = y_field.GetCustomAttribute(author_attribute_type);
            var type7 = attribute7.GetType();
            Assert.IsNotNull(attribute7);
            Assert.AreEqual(type7.Name, "AuthorAttribute");

            var params_of_do_something_method = do_something_method.GetParameters();
            var attribute8 = params_of_do_something_method[0].GetCustomAttribute(author_attribute_type);
            var type8 = attribute8.GetType();
            Assert.IsNotNull(attribute8);
            Assert.AreEqual(type8.Name, "AuthorAttribute");

            var do_something2_method = test_type.GetMethod("doSomething2");
            var attribute9 = do_something2_method.ReturnParameter.GetCustomAttribute(author_attribute_type);
            var type9 = attribute9.GetType();
            Assert.IsNotNull(attribute9);
            Assert.AreEqual(type9.Name, "AuthorAttribute");

            main_method.Invoke(null, new object[]{});
        }

        [Test]
        public void Enum1()
        {
            Console.WriteLine("Test Enum1");

            var parser = new Parser(new Scanner("../../sources/for_unit_tests/enum1.exs"));
            parser.DoPostParseProcessing = true;
            parser.Parse();

            var ast = parser.TopmostAst;

            var options = new ExpressoCompilerOptions{
                OutputPath = "../../test_executables",
                BuildType = BuildType.Debug | BuildType.Executable,
                ExecutableName = "main"
            };
            var generator = new CodeGenerator(parser, options);
            ast.AcceptWalker(generator, null);

            var asm = generator.AssemblyBuilder;
            var main_method = asm.EntryPoint;
            Assert.AreEqual(main_method.Name, "main");
            Assert.IsTrue(main_method.IsStatic);
            Assert.AreEqual(typeof(void), main_method.ReturnType);
            Assert.AreEqual(0, main_method.GetParameters().Length);

            main_method.Invoke(null, new object[]{});
        }

        [Test]
        public void Enum2()
        {
            Console.WriteLine("Test Enum2");

            var parser = new Parser(new Scanner("../../sources/for_unit_tests/enum2.exs"));
            parser.DoPostParseProcessing = true;
            parser.Parse();

            var ast = parser.TopmostAst;

            var options = new ExpressoCompilerOptions{
                OutputPath = "../../test_executables",
                BuildType = BuildType.Debug | BuildType.Executable,
                ExecutableName = "main"
            };
            var generator = new CodeGenerator(parser, options);
            ast.AcceptWalker(generator, null);

            var asm = generator.AssemblyBuilder;
            var main_method = asm.EntryPoint;
            Assert.AreEqual(main_method.Name, "main");
            Assert.IsTrue(main_method.IsStatic);
            Assert.AreEqual(typeof(void), main_method.ReturnType);
            Assert.AreEqual(0, main_method.GetParameters().Length);

            main_method.Invoke(null, new object[]{});
        }

        [Test]
        public void GenericClass()
        {
            Console.WriteLine("Test GenericClass");

            var parser = new Parser(new Scanner("../../sources/for_unit_tests/generic_class.exs"));
            parser.DoPostParseProcessing = true;
            parser.Parse();

            var ast = parser.TopmostAst;

            var options = new ExpressoCompilerOptions{
                OutputPath = "../../test_executables",
                BuildType = BuildType.Debug | BuildType.Executable,
                ExecutableName = "main"
            };
            var generator = new CodeGenerator(parser, options);
            ast.AcceptWalker(generator, null);

            var asm = generator.AssemblyBuilder;
            var main_method = asm.EntryPoint;
            Assert.AreEqual(main_method.Name, "main");
            Assert.IsTrue(main_method.IsStatic);
            Assert.AreEqual(typeof(void), main_method.ReturnType);
            Assert.AreEqual(0, main_method.GetParameters().Length);

            main_method.Invoke(null, new object[]{});
        }

        [Test]
        public void ConditionalXXOperators()
        {
            Console.WriteLine("Test ConditionalXXOperators");

            var parser = new Parser(new Scanner("../../sources/for_unit_tests/conditional_xx_operators.exs"));
            parser.DoPostParseProcessing = true;
            parser.Parse();

            var ast = parser.TopmostAst;

            var options = new ExpressoCompilerOptions{
                OutputPath = "../../test_executables",
                BuildType = BuildType.Debug | BuildType.Executable,
                ExecutableName = "main"
            };
            var generator = new CodeGenerator(parser, options);
            ast.AcceptWalker(generator, null);

            var asm = generator.AssemblyBuilder;
            var main_method = asm.EntryPoint;
            Assert.AreEqual(main_method.Name, "main");
            Assert.IsTrue(main_method.IsStatic);
            Assert.AreEqual(typeof(void), main_method.ReturnType);
            Assert.AreEqual(0, main_method.GetParameters().Length);

            main_method.Invoke(null, new object[]{});
        }
    }
}

