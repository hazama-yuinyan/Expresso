using System;
using System.IO;
using System.Collections.Generic;
using System.Numerics;
using System.Reflection;
using System.Linq;

using NUnit.Framework;

using Expresso.Ast;
using Expresso.Builtins;
using Expresso.Interpreter;
using Expresso.Helpers;

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

		public static void DoTest(VariableStore store, object[] expects)
		{
			var len = expects.Length;
			for(int i = 0; i < len; ++i){
				var target = store.Get(i);
				var expect = expects[i];
				Type type_target = target.GetType(), type_expect = expect.GetType();
				Assert.IsTrue(type_target.FullName == type_expect.FullName);
				Assert.AreEqual(expect, target);
			}
		}
	}

	[TestFixture]
	public class ParserTests
	{
		[TestCase]
		public void SimpleLiterals()
		{
			var parser = new Parser(new Scanner("../../sources/for_parser/simple_literals.exs"));
			parser.Parse();
			var interp = new Expresso.Interpreter.Interpreter{Root = parser.root};
			interp.Initialize();
			var store = interp.GetGlobalVarStore();

			var expected = new object[]{
				255,	//a
				0xff,	//h_a
				1000.0,	//b
				1.0e4,	//f_b
				0.001,	//c
				.1e-2,	//f_c
				new BigInteger(10000000),	//d
				"This is a test",	//e
				ExpressoFunctions.MakeList(new List<object>()),	//f
				ExpressoFunctions.MakeDict(new List<object>(), new List<object>())	//g
			};

			Helpers.DoTest(store, expected);
		}
	}

	[TestFixture]
	public class InterpreterTests
	{
		[TestCase]
		public void SimpleArithmetic()
		{
			var parser = new Parser(new Scanner("../../sources/for_interpreter/simple_arithmetic.exs"));
			parser.Parse();
			var interp = new Expresso.Interpreter.Interpreter{Root = parser.root, MainFunc = Parser.main_func};
			interp.Initialize();
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

		[TestCase]
		public void BasicOperations()
		{
			var parser = new Parser(new Scanner("../../sources/for_interpreter/basic_operations.exs"));
			parser.Parse();
			var interp = new Expresso.Interpreter.Interpreter{Root = parser.root, MainFunc = Parser.main_func};
			interp.Initialize();
			var results = interp.Run() as List<object>;
			Assert.IsNotNull(results);

			var expected_a = new List<object>{1, 2, 3};
			var expected_b = ExpressoFunctions.MakeDict(new List<object>{"a", "b", "y"}, new List<object>{1, 4, 10});
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

		[TestCase]
		public void Statements()
		{
			var parser = new Parser(new Scanner("../../sources/for_interpreter/statements.exs"));
			parser.Parse();
			var interp = new Expresso.Interpreter.Interpreter{Root = parser.root, MainFunc = Parser.main_func};
			interp.Initialize();
			var results = interp.Run() as List<object>;
			Assert.IsNotNull(results);

			var expected_y = 200;
			var expected_sum = Helpers.CalcSum(0, expected_y);
			var expected_strs = ExpressoFunctions.MakeList(new List<object>{"akarichan", "chinatsu", "kyoko", "yui"});
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
				select ExpressoFunctions.MakeTuple(new object[]{i, j});

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

		[TestCase]
		public void BuiltinObjects()
		{
			var parser = new Parser(new Scanner("../../sources/for_interpreter/builtin_objects.exs"));
			parser.Parse();
			var interp = new Expresso.Interpreter.Interpreter{Root = parser.root, MainFunc = Parser.main_func};
			interp.Initialize();
			var results = interp.Run() as List<object>;
			Assert.IsNotNull(results);

			var expected_a = ExpressoFunctions.MakeList(new List<object>{1, 2, 3, 4, 5, 6, 7, 8});
			var expected_b = ExpressoFunctions.MakeDict(new List<object>{"akari", "chinatsu", "kyoko", "yui"},
				new List<object>{13, 13, 14, 14});
			var expected_c = ExpressoFunctions.MakeTuple(new List<object>{"akarichan", "kawakawa", "chinatsuchan", 2424});

			var tmp_seq = new ExpressoIntegerSequence(0, 3, 1);
			var expected_d = ImplementationHelpers.Slice(expected_a, tmp_seq);

			var expected = new object[]{
				expected_a,
				expected_b,
				expected_c,
				expected_d,
				expected_c[2]	//e
			};
		}

		[TestCase]
		public void ComplexExpressions()
		{
			var parser = new Parser(new Scanner("../../sources/for_interpreter/complex_expressions.exs"));
			parser.Parse();
			var interp = new Expresso.Interpreter.Interpreter{Root = parser.root, MainFunc = Parser.main_func};
			interp.Initialize();
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
				select ExpressoFunctions.MakeTuple(new object[]{k, l});

			var expected_z = new List<object>(tmp_z);

			var expected_m = ExpressoFunctions.MakeTuple(new List<object>{1, 3});

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

		[TestCase]
		public void Class()
		{
			var parser = new Parser(new Scanner("../../sources/for_interpreter/class.exs"));
			parser.Parse();
			var interp = new Expresso.Interpreter.Interpreter{Root = parser.root, MainFunc = Parser.main_func};
			interp.Initialize();
			var results = interp.Run() as List<object>;
			Assert.IsNotNull(results);

			var expected = new object[]{
				1,		//b
				3,		//c
				101		//d
			};

			var a = results[0] as ExpressoObj;
			Assert.IsNotNull(a);
			Assert.Throws(typeof(EvalException), () => a.AccessMember(new Identifier("x"), false));
			Assert.Throws(typeof(EvalException), () => a.AccessMember(new Identifier("y"), false));
			var ctor = a.AccessMember(new Identifier("constructor"), false) as Function;
			var func_getx = a.AccessMember(new Identifier("getX"), false) as Function;
			var func_gety = a.AccessMember(new Identifier("getY"), false) as Function;
			var func_getxplus = a.AccessMember(new Identifier("getXPlus"), false) as Function;

			Assert.IsNotNull(ctor);
			Assert.AreEqual("constructor", ctor.Name);
			Assert.AreEqual(TYPES.UNDEF, ctor.ReturnType.ObjType);

			Assert.IsNotNull(func_getx);
			Assert.AreEqual("getX", func_getx.Name);
			Assert.AreEqual(TYPES.VAR, func_getx.ReturnType.ObjType);

			Assert.IsNotNull(func_gety);
			Assert.AreEqual("getY", func_gety.Name);
			Assert.AreEqual(TYPES.INTEGER, func_gety.ReturnType.ObjType);

			Assert.IsNotNull(func_getxplus);
			Assert.AreEqual("getXPlus", func_getxplus.Name);
			Assert.AreEqual(TYPES.INTEGER, func_getxplus.ReturnType.ObjType);

			var numeric_results = ImplementationHelpers.Slice(results, new ExpressoIntegerSequence(1, results.Count, 1)) as List<object>;
			Helpers.DoTest(numeric_results, expected);
		}

		[TestCase]
		public void Library()
		{

		}
	}
}
