using System;
using System.IO;
using System.Collections.Generic;
using System.Reflection;
using NUnit.Framework;
using Expresso.Ast;
using Expresso.BuiltIns;
using Expresso.Interpreter;
using Expresso.Helpers;

namespace Expresso.Test
{
	internal class Helpers
	{
		public static Primitive AutoCast<Primitive>(object target)
		{
			if(target == null) throw new Exception("Something wrong has occurred. The target is null!");

			return (Primitive)target;
		}

		public static int CalcSum(int start, int max)
		{
			var result = 0;
			for(int i = start; i <= max; ++i)
				result += i;

			return result;
		}

		public static void DoTest(ExpressoClass.ExpressoObj targets, object[] expects)
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
				"This is a test",	//d
				ExpressoFunctions.MakeList(new List<object>()),	//e
				ExpressoFunctions.MakeDict(new List<object>(), new List<object>())	//f
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
			var results = interp.Run() as ExpressoClass.ExpressoObj;
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
			var results = interp.Run() as ExpressoClass.ExpressoObj;
			Assert.IsNotNull(results);

			var expected_a = ExpressoFunctions.MakeList(new List<object>{1, 2, 3});
			var expected_b = ExpressoFunctions.MakeDict(new List<object>{"a", "b", "y"}, new List<object>{1, 4, 10});
			var expected_x = 100;

			var expected_p = (int)expected_a[0] + (int)expected_a[1] + (int)expected_a[2];
			var expected_q = (int)expected_b.AccessMember("a", true) + (int)expected_b.AccessMember("b", true) +
				(int)expected_b.AccessMember("y", true);
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
			var results = interp.Run() as ExpressoClass.ExpressoObj;
			Assert.IsNotNull(results);

			var expected_y = 200;
			var expected_sum = Helpers.CalcSum(0, expected_y);
			var expected_strs = ExpressoFunctions.MakeList(new List<object>{"akarichan", "chinatsu", "kyoko", "yui"});

			var expected = new object[]{
				100,	//x
				expected_y,
				300,	//z
				400,	//w
				true,	//flag
				expected_sum,
				expected_strs
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
			var results = interp.Run() as ExpressoClass.ExpressoObj;
			Assert.IsNotNull(results);

			var expected_a = ExpressoFunctions.MakeList(new List<object>{1, 2, 3, 4, 5, 6, 7, 8});
			var expected_b = ExpressoFunctions.MakeDict(new List<object>{"akari", "chinatsu", "kyoko", "yui"},
				new List<object>{13, 13, 14, 14});
			var expected_c = ExpressoFunctions.MakeTuple(new List<object>{"akarichan", "kawakawa", "chinatsuchan", 2424});

			var tmp_seq = new ExpressoIntegerSequence(0, 3, 1);
			var expected_d = expected_a.Slice(tmp_seq);

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
			var results = interp.Run() as ExpressoClass.ExpressoObj;
			Assert.IsNotNull(results);

			var tmp_x = new List<object>(100);
			for(int i = 0; i < 100; ++i)
				tmp_x.Add(i);

			var expected_x = ExpressoFunctions.MakeList(tmp_x);

			var tmp_y = new List<object>(50);
			for(int j = 0; j <= 100; ++j){
				if(j % 2 == 0)
					tmp_y.Add(j);
			}

			var expected_y = ExpressoFunctions.MakeList(tmp_y);

			var tmp_z = new List<object>(50 * 100);
			for(int k = 0; k <= 100; ++k){
				if(k % 2 == 0){
					for(int l = 0; l < 100; ++l)
						tmp_z.Add(ExpressoFunctions.MakeTuple(new List<object>{k, l}));
				}
			}

			var expected_z = ExpressoFunctions.MakeList(tmp_z);

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
			var results = interp.Run() as ExpressoClass.ExpressoObj;
			Assert.IsNotNull(results);

			//var test_definition = new ExpressoClass.ClassDefinition("Test", privates, publics);
			//var expected_a = null;

			var expected = new object[]{
				//expected_a,
				1,		//b
				3,		//c
				101		//d
			};

			Helpers.DoTest(results, expected);
		}
	}
}
