using System;
using System.IO;
using System.Collections.Generic;
using System.Numerics;
using System.Reflection;
using System.Linq;

using NUnit.Framework;

using Expresso.Ast;
using Expresso.Builtins;
using Expresso.Compiler.Meta;
using Expresso.Interpreter;
using Expresso.Runtime;
using Expresso.Runtime.Operations;
using Expresso.Runtime.Exceptions;

namespace Expresso.Test
{
	internal class Helpers
	{
		public static int CalcSum(int start, int max)
		{
			var result = 0;
			for(int i = start; i < max; ++i)
				result += i;

			return result;
		}

		public static int Fib(int n)
		{
			if(n < 2)
				return 1;
			else
				return Fib(n - 1) + Fib(n - 2);
		}

		public static void DoTest(List<object> targets, object[] expects)
		{
			for(int i = 0; i < expects.Length; ++i){
				var target = targets[i];
				var expect = expects[i];
				Type type_target = target.GetType(), type_expect = expect.GetType();
				Assert.IsTrue(type_target.FullName == type_expect.FullName);
				Assert.AreEqual(expect, target);
			}
		}

		public static void DoTest(ExpressoModule mainModule, List<string> names, object[] expects)
		{
			var len = expects.Length;
			for(int i = 0; i < len; ++i){
				var target = mainModule.LookupMember(names[i]);
				var expect = expects[i];
				Type type_target = target.GetType(), type_expect = expect.GetType();
				Assert.IsTrue(type_target.FullName == type_expect.FullName);
				Assert.AreEqual(expect, target);
			}
		}

		public static void TestOnType(ExpressoObj instance, List<string> privateMembers, List<FunctionAnnotation> methodAnnots)
		{
			foreach(var private_name in privateMembers){
				try{
					instance.AccessMemberWithName(private_name, false);
				}
				catch(Exception e){
                    Assert.IsInstanceOfType(typeof(ReferenceException), e);
				}
			}

			foreach(var method_annot in methodAnnots){
				var method = instance.AccessMemberWithName(method_annot.Name, false) as FunctionDeclaration;
				Assert.IsNotNull(method);
				Assert.AreEqual(method_annot.Name, method.Name);
				Assert.AreEqual(method_annot.ReturnType, method.ReturnType);
			}
		}
	}

	internal class FunctionAnnotation
	{
		public string Name{get; set;}
		public TypeAnnotation ReturnType{get; set;}

		public FunctionAnnotation(string name, TypeAnnotation returnType)
		{
			Name = name;
			ReturnType = returnType;
		}
	}

	[TestFixture]
	public class ParserTests
	{
		[Test]
		public void SimpleLiterals()
		{
			var parser = new Parser(new Scanner("../../sources/for_parser/simple_literals.exs"));
			parser.ParsingFileName = "simple_literals";
			parser.Parse();
			var interp = new Expresso.Interpreter.Interpreter();
			Expresso.Interpreter.Interpreter.MainModule = parser.TopmostAst;
            interp.Run(parser.TopmostAst, true);
			var main_module = interp.GlobalContext.GetModule("simple_literals");

			var expected = new object[]{
				255,	//a
				0xff,	//h_a
				1000.0,	//b
				1.0e4,	//f_b
				0.001,	//c
				.1e-2,	//f_c
				new BigInteger(10000000),	//d
				"This is a test",	//e
				ExpressoOps.MakeList(new List<object>()),	//f
				ExpressoOps.MakeDict(new List<object>(), new List<object>())	//g
			};

			var names = new List<string>{
				"a",
				"h_a",
				"b",
				"f_b",
				"c",
				"f_c",
				"d",
				"e",
				"f",
				"g"
			};

			Assert.IsNotNull(main_module);
			Helpers.DoTest(main_module, names, expected);
		}
	}

	[TestFixture]
	public class InterpreterTests
	{
		[Test]
		public void SimpleArithmetic()
		{
			var parser = new Parser(new Scanner("../../sources/for_interpreter/simple_arithmetic.exs"));
			parser.Parse();
			var interp = new Expresso.Interpreter.Interpreter();
			Expresso.Interpreter.Interpreter.MainModule = parser.TopmostAst;
			var results = interp.Run() as List<object>;
			Assert.IsNotNull(results);

			var expected = new object[]{
				3,
				7,
				-1,
				12,
				2,
				0,
				9,
				7,
				-1,
				12,
				2,
				0,
				9
			};

			Helpers.DoTest(results, expected);
		}

		[Test]
		public void BasicOperations()
		{
			var parser = new Parser(new Scanner("../../sources/for_interpreter/basic_operations.exs"));
			parser.Parse();
			var interp = new Expresso.Interpreter.Interpreter();
			Expresso.Interpreter.Interpreter.MainModule = parser.TopmostAst;
			var results = interp.Run() as List<object>;
			Assert.IsNotNull(results);

			var expected_a = new List<object>{1, 2, 3};
			var expected_b = ExpressoOps.MakeDict(new List<object>{"a", "b", "y"}, new List<object>{1, 4, 10});
			var expected_x = 100;

			var expected_p = (int)expected_a[0] + (int)expected_a[1] + (int)expected_a[2];
			var expected_q = (int)expected_b["a"] + (int)expected_b["b"] + (int)expected_b["y"];
			var expected_r = expected_x >> expected_p;
			var expected_s = expected_x << 2;
			var expected_t = expected_r & expected_s;
			var expected_u = expected_x | expected_t;

			var expected = new object[]{
				expected_a,
				expected_b,
				"akarichan",	//c
				"chinatsu",		//d
				expected_x,
				expected_p,
				expected_q,
				expected_r,
				expected_s,
				expected_t,
				expected_u,
				"akarichan" + "chinatsu"	//v
			};

			Helpers.DoTest(results, expected);
		}

		[Test]
		public void Statements()
		{
			var parser = new Parser(new Scanner("../../sources/for_interpreter/statements.exs"));
			parser.Parse();
			var interp = new Expresso.Interpreter.Interpreter();
			Expresso.Interpreter.Interpreter.MainModule = parser.TopmostAst;
			var results = interp.Run() as List<object>;
			Assert.IsNotNull(results);

			var expected_y = 200;
			var expected_sum = Helpers.CalcSum(0, expected_y);
			var expected_strs = ExpressoOps.MakeList(new List<object>{"akarichan", "chinatsu", "kyoko", "yui"});
			var expected_fibs = new List<object>();
			for(int i = 0; ; ++i){
				var fib = Helpers.Fib(i);
				if(fib >= 1000) break;
				expected_fibs.Add(fib);
			}

			var expected_ary = 
				from i in Enumerable.Range(0, 10)
				where i != 3 && i != 6
				from j in Enumerable.Range(0, 10)
				where j < 8
				select ExpressoOps.MakeTuple(new object[]{i, j});

			var expected = new object[]{
				100,	//x
				expected_y,
				300,	//z
				400,	//w
				true,	//flag
				expected_sum,
				expected_strs,
				expected_fibs,
				new List<object>(expected_ary)
			};

			Helpers.DoTest(results, expected);
		}

		[Test]
		public void BuiltinObjects()
		{
			var parser = new Parser(new Scanner("../../sources/for_interpreter/builtin_objects.exs"));
			parser.Parse();
			var interp = new Expresso.Interpreter.Interpreter();
			Expresso.Interpreter.Interpreter.MainModule = parser.TopmostAst;
			var results = interp.Run() as List<object>;
			Assert.IsNotNull(results);

			var expected_a = ExpressoOps.MakeList(new List<object>{1, 2, 3, 4, 5, 6, 7, 8});
			var expected_b = ExpressoOps.MakeDict(new List<object>{"akari", "chinatsu", "kyoko", "yui"},
				new List<object>{13, 13, 14, 14});
			var expected_c = ExpressoOps.MakeTuple(new List<object>{"akarichan", "kawakawa", "chinatsuchan", 2424});

			var tmp_seq = new ExpressoIntegerSequence(0, 3, 1);
			var expected_d = ExpressoOps.Slice(expected_a, tmp_seq);

			var expected = new object[]{
				expected_a,
				expected_b,
				expected_c,
				expected_d,
				expected_c[2]	//e
			};

            Helpers.DoTest(results, expected);
		}

		[Test]
		public void ComplexExpressions()
		{
			var parser = new Parser(new Scanner("../../sources/for_interpreter/complex_expressions.exs"));
			parser.Parse();
			var interp = new Expresso.Interpreter.Interpreter();
			Expresso.Interpreter.Interpreter.MainModule = parser.TopmostAst;
			var results = interp.Run() as List<object>;
			Assert.IsNotNull(results);

			var tmp_x =
				from i in Enumerable.Range(0, 100)
				select i;

			var expected_x = new List<object>(tmp_x.Cast<object>());

			var tmp_y =
				from j in Enumerable.Range(0, 100)
				where j % 2 == 0
				select j;

			var expected_y = new List<object>(tmp_y.Cast<object>());

			var tmp_z =
				from k in Enumerable.Range(0, 100)
				where k % 2 == 0
				from l in Enumerable.Range(0, 100)
				select ExpressoOps.MakeTuple(new object[]{k, l});

			var expected_z = new List<object>(tmp_z);

			var expected_m = ExpressoOps.MakeTuple(new List<object>{1, 3});

			var expected = new object[]{
				expected_x,
				expected_y,
				expected_z,
				0,		//a
				0,		//b
				0,		//c
				expected_m
			};

			Helpers.DoTest(results, expected);
		}

		[Test]
		public void Class()
		{
			var parser = new Parser(new Scanner("../../sources/for_interpreter/class.exs"));
			parser.Parse();
			var interp = new Expresso.Interpreter.Interpreter();
			Expresso.Interpreter.Interpreter.MainModule = parser.TopmostAst;
			var results = interp.Run() as List<object>;
			Assert.IsNotNull(results);

			var expected = new object[]{
				1,		//b
				3,		//c
				101		//d
			};

			var private_names = new List<string>{
				"x",
				"y"
			};

			var method_annots = new List<FunctionAnnotation>{
				new FunctionAnnotation("constructor", TypeAnnotation.VoidType),
				new FunctionAnnotation("getX", TypeAnnotation.VariantType),
				new FunctionAnnotation("getY", new TypeAnnotation(ObjectTypes.Integer)),
				new FunctionAnnotation("getXPlus", new TypeAnnotation(ObjectTypes.Integer))
			};

			var a = results[0] as ExpressoObj;
			Assert.IsNotNull(a);
			Helpers.TestOnType(a, private_names, method_annots);

			var numeric_results = ExpressoOps.Slice(results, new ExpressoIntegerSequence(1, results.Count, 1)) as List<object>;
			Helpers.DoTest(numeric_results, expected);
		}

		[Test]
		public void Library()
		{

		}

		[Test]
		public void Module()
		{
			var parser = new Parser(new Scanner("../../sources/for_interpreter/module.exs"));
			parser.Parse();
			var interp = new Expresso.Interpreter.Interpreter();
			interp.CurrentOpenedSourceFileName = "../../sources/for_interpreter/module.exs";
			Expresso.Interpreter.Interpreter.MainModule = parser.TopmostAst;
			var results = interp.Run() as List<object>;
			Assert.IsNotNull(results);
			
			var private_names = new List<string>{
				"x",
				"y"
			};
			
			var method_annots = new List<FunctionAnnotation>{
				new FunctionAnnotation("constructor", TypeAnnotation.VoidType),
				new FunctionAnnotation("getX", TypeAnnotation.VariantType),
				new FunctionAnnotation("getY", new TypeAnnotation(ObjectTypes.Integer)),
			};

			var a = results[0] as ExpressoObj;
			Assert.IsNotNull(a);
			Helpers.TestOnType(a, private_names, method_annots);

			var b = results[1] as ExpressoObj;
			Assert.IsNotNull(b);
			Helpers.TestOnType(b, private_names, method_annots);

			var c = results[2] as ExpressoTuple;
			Assert.IsNotNull(c);
			Assert.AreEqual(c, ExpressoOps.MakeTuple(new object[]{200, 300}));
		}
	}

	[TestFixture]
	public class NativeTests
	{
		[Test]
		public void TestFraction()
		{
			Fraction a = new Fraction(1, 3), b = new Fraction(2, 3), c = new Fraction(-1, 4), d = new Fraction(new BigInteger(3)), e = new Fraction(2.5);

			var expected = new object[]{
				new Fraction(3, 3),		//a + b
				new Fraction(-1, 3),	//a - b
				new Fraction(2, 9),		//a * b
				new Fraction(1, 2),		//a / b
				new Fraction(1, 12),	//a + c
				new Fraction(7, 12),	//a - c
				new Fraction(-1, 12),	//a * c
				new Fraction(-4, 3),	//a / c
				new Fraction(10, 3),	//a + d
				new Fraction(-8, 3),	//a - d
				new Fraction(3, 3),		//a * d
				new Fraction(1, 9),		//a / d
				new Fraction(17, 6),	//a + e
				new Fraction(-13, 6),	//a - e
				new Fraction(5, 6),		//a * e
				new Fraction(2, 15)		//a / e
			};

			var targets = new List<object>{
				a + b,
				a - b,
				a * b,
				a / b,
				a + c,
				a - c,
				a * c,
				a / c,
				a + d,
				a - d,
				a * d,
				a / d,
				a + e,
				a - e,
				a * e,
				a / e
			};

			Assert.IsTrue(a.Numerator == 1 && a.Denominator == 3);
			Assert.IsTrue(b.Numerator == 2 && b.Denominator == 3);
			Assert.IsTrue(c.Numerator == -1 && c.Denominator == 4);
			Assert.IsTrue(d.Numerator == 3 && d.Denominator == 1);
			Assert.IsTrue(e.Numerator == 5 && e.Denominator == 2);

			Helpers.DoTest(targets, expected);
		}
	}
}
