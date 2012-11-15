using System;
using System.IO;
using System.Collections.Generic;
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

			}
		}
	}

	[TestFixture]
	public class ParserTests
	{
		[Test]
		public void SimpleExpressions(string source, Node[] expects)
		{
			//var parser = new Parser(new Scanner(new StringReader(source)));
			
		}
		
		[Test]
		public void SimpleStatements(string source, Node[] expects)
		{
			
		}
	}

	[TestFixture]
	public class InterpreterTests
	{
		[TestCase]
		public void SimpleArithmetic()
		{
			var parser = new Parser(new Scanner("../../sources/simple_arithmetic.exs"));
			parser.Parse();
			var interp = new Expresso.Interpreter.Interpreter{Root = parser.root, MainFunc = Parser.main_func};
			interp.Initialize();
			var results = interp.Run() as ExpressoClass.ExpressoObj;
			Assert.IsNotNull(results);
			object x = results[0], a = results[1], b = results[2], c = results[3];
			object d = results[4], e = results[5], f = results[6], xp = results[7];
			object xm = results[8], xt = results[9], xd = results[10], xmod = results[11], xpower = results[12];

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

			Assert.IsTrue(x is int);
			Assert.AreEqual(expected[0], x);

			Assert.IsTrue(a is int);
			Assert.AreEqual(expected[1], a);

			Assert.IsTrue(b is int);
			Assert.AreEqual(expected[2], b);

			Assert.IsTrue(c is int);
			Assert.AreEqual(expected[3], c);

			Assert.IsTrue(d is int);
			Assert.AreEqual(expected[4], d);

			Assert.IsTrue(e is int);
			Assert.AreEqual(expected[5], e);

			Assert.IsTrue(f is int);
			Assert.AreEqual(expected[6], f);

			Assert.IsTrue(xp is int);
			Assert.AreEqual(expected[7], xp);

			Assert.IsTrue(xm is int);
			Assert.AreEqual(expected[8], xm);

			Assert.IsTrue(xt is int);
			Assert.AreEqual(expected[9], xt);

			Assert.IsTrue(xd is int);
			Assert.AreEqual(expected[10], xd);

			Assert.IsTrue(xmod is int);
			Assert.AreEqual(expected[11], xmod);

			Assert.IsTrue(xpower is int);
			Assert.AreEqual(expected[12], xpower);
		}

		[TestCase]
		public void BasicOperations()
		{
			var parser = new Parser(new Scanner("../../sources/basic_operations.exs"));
			parser.Parse();
			var interp = new Expresso.Interpreter.Interpreter{Root = parser.root, MainFunc = Parser.main_func};
			interp.Initialize();
			var results = interp.Run() as ExpressoClass.ExpressoObj;
			Assert.IsNotNull(results);
			object a = results[0], b = results[1], c = results[2], d = results[3];
			object x = results[4], p = results[5], q = results[6], r = results[7];
			object s = results[8], t = results[9], u = results[10], v = results[11];

			var expected_a = ExpressoFunctions.MakeList(new List<object>{1, 2, 3});
			var expected_b = ExpressoFunctions.MakeDict(new List<object>{"a", "b", "y"}, new List<object>{1, 4, 10});
			var expected_c = "akarichan";
			var expected_d = "chinatsu";
			var expected_x = 100;

			var expected_p = (int)expected_a[0] + (int)expected_a[1] + (int)expected_a[2];
			var expected_q = (int)expected_b.AccessMember("a") + (int)expected_b.AccessMember("b") +
				(int)expected_b.AccessMember("y");
			var expected_r = expected_x >> expected_p;
			var expected_s = expected_x << 2;
			var expected_t = expected_r & expected_s;
			var expected_u = expected_x | expected_t;
			var expected_v = expected_c + expected_d;

			Assert.IsTrue(a is ExpressoClass.ExpressoObj && ((ExpressoClass.ExpressoObj)a).Type == TYPES.LIST);
			Assert.AreEqual(expected_a, a);

			Assert.IsTrue(b is ExpressoClass.ExpressoObj && ((ExpressoClass.ExpressoObj)b).Type == TYPES.DICT);
			Assert.AreEqual(expected_b, b);

			Assert.IsTrue(c is string);
			Assert.AreEqual(expected_c, c);

			Assert.IsTrue(d is string);
			Assert.AreEqual(expected_d, d);

			Assert.IsTrue(x is int);
			Assert.AreEqual(expected_x, x);

			Assert.IsTrue(p is int);
			Assert.AreEqual(expected_p, p);

			Assert.IsTrue(q is int);
			Assert.AreEqual(expected_q, q);

			Assert.IsTrue(r is int);
			Assert.AreEqual(expected_r, r);

			Assert.IsTrue(s is int);
			Assert.AreEqual(expected_s, s);

			Assert.IsTrue(t is int);
			Assert.AreEqual(expected_t, t);

			Assert.IsTrue(u is int);
			Assert.AreEqual(expected_u, u);

			Assert.IsTrue(v is string);
			Assert.AreEqual(expected_v, v);
		}

		[TestCase]
		public void Statements()
		{
			var parser = new Parser(new Scanner("../../sources/statements.exs"));
			parser.Parse();
			var interp = new Expresso.Interpreter.Interpreter{Root = parser.root, MainFunc = Parser.main_func};
			interp.Initialize();
			var results = interp.Run() as ExpressoClass.ExpressoObj;
			Assert.IsNotNull(results);
			object x = results[0], y = results[1], z = results[2], w = results[3];
			object flag = results[4], sum = results[5], strs = results[6];

			var expected_x = 100;
			var expected_y = 200;
			var expected_z = 300;
			var expected_w = 400;
			var expected_flag = true;
			var expected_sum = Helpers.CalcSum(0, expected_y);
			var expected_strs = ExpressoFunctions.MakeList(new List<object>{"akarichan", "chinatsu", "kyoko", "yui"});

			Assert.IsTrue(x is int);
			Assert.AreEqual(expected_x, x);

			Assert.IsTrue(y is int);
			Assert.AreEqual(expected_y, y);

			Assert.IsTrue(z is int);
			Assert.AreEqual(expected_z, z);

			Assert.IsTrue(w is int);
			Assert.AreEqual(expected_w, w);

			Assert.IsTrue(flag is bool);
			Assert.AreEqual(expected_flag, flag);

			Assert.IsTrue(sum is int);
			Assert.AreEqual(expected_sum, sum);

			Assert.IsTrue(strs is ExpressoClass.ExpressoObj && ((ExpressoClass.ExpressoObj)strs).Type == TYPES.LIST);
			Assert.AreEqual(expected_strs, strs);
		}

		[TestCase]
		public void BuiltinObjects()
		{

		}

		[TestCase]
		public void ComplexExpressions()
		{
			var parser = new Parser(new Scanner("../../sources/complex_expressions.exs"));
			parser.Parse();
			var interp = new Expresso.Interpreter.Interpreter{Root = parser.root, MainFunc = Parser.main_func};
			interp.Initialize();
			var results = interp.Run() as ExpressoClass.ExpressoObj;
			Assert.IsNotNull(results);
			object x = results[0], y = results[1], z = results[2], a = results[3];
			object b = results[4], c = results[5], m = results[6];

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

			Assert.IsTrue(x is ExpressoClass.ExpressoObj);
			Assert.AreEqual(expected_x, x);

			Assert.IsTrue(y is ExpressoClass.ExpressoObj);
			Assert.AreEqual(expected_y, y);

			Assert.IsTrue(z is ExpressoClass.ExpressoObj);
			Assert.AreEqual(expected_z, z);

			Assert.IsTrue(a is int);
			Assert.AreEqual(expected[3], a);

			Assert.IsTrue(b is int);
			Assert.AreEqual(expected[4], b);

			Assert.IsTrue(c is int);
			Assert.AreEqual(expected[5], c);

			Assert.IsTrue(m is ExpressoClass.ExpressoObj);
			Assert.AreEqual(expected[6], m);
		}
	}
}
