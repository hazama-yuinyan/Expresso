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
			var results = interp.Run() as List<object>;
			Assert.IsNotNull(results);
			object x = results[0], a = results[1], b = results[2], c = results[3];
			object d = results[4], e = results[5], f = results[6], xp = results[7];
			object xm = results[8], xt = results[9], xd = results[10], xmod = results[11], xpower = results[12];

			Assert.IsTrue(x is int);
			Assert.AreEqual(3, x);

			Assert.IsTrue(a is int);
			Assert.AreEqual(7, a);

			Assert.IsTrue(b is int);
			Assert.AreEqual(-1, b);

			Assert.IsTrue(c is int);
			Assert.AreEqual(12, c);

			Assert.IsTrue(d is int);
			Assert.AreEqual(2, d);

			Assert.IsTrue(e is int);
			Assert.AreEqual(0, e);

			Assert.IsTrue(f is int);
			Assert.AreEqual(9, f);

			Assert.IsTrue(xp is int);
			Assert.AreEqual(7, xp);

			Assert.IsTrue(xm is int);
			Assert.AreEqual(-1, xm);

			Assert.IsTrue(xt is int);
			Assert.AreEqual(12, xt);

			Assert.IsTrue(xd is int);
			Assert.AreEqual(2, xd);

			Assert.IsTrue(xmod is int);
			Assert.AreEqual(0, xmod);

			Assert.IsTrue(xpower is int);
			Assert.AreEqual(9, xpower);
		}

		[TestCase]
		public void BasicOperations()
		{
			var parser = new Parser(new Scanner("../../sources/basic_operations.exs"));
			parser.Parse();
			var interp = new Expresso.Interpreter.Interpreter{Root = parser.root, MainFunc = Parser.main_func};
			interp.Initialize();
			var results = interp.Run() as List<object>;
			Assert.IsNotNull(results);
			object a = results[0], b = results[1], c = results[2], d = results[3];
			object x = results[4], p = results[5], q = results[6], r = results[7];
			object s = results[8], t = results[9], u = results[10], v = results[11];

			var expected_a = new List<object>{1, 2, 3};
			var expected_b = new Dictionary<object, object>{{"a", 1}, {"b", 4}, {"y", 10}};
			var expected_c = "akarichan";
			var expected_d = "chinatsu";
			var expected_x = 100;

			var expected_p = (int)expected_a[0] + (int)expected_a[1] + (int)expected_a[2];
			var expected_q = (int)expected_b["a"] + (int)expected_b["b"] + (int)expected_b["y"];
			var expected_r = expected_x >> expected_p;
			var expected_s = expected_x << 2;
			var expected_t = expected_r & expected_s;
			var expected_u = expected_x | expected_t;
			var expected_v = expected_c + expected_d;

			Assert.IsTrue(a is List<object>);
			Assert.AreEqual(expected_a, a);

			Assert.IsTrue(b is Dictionary<object, object>);
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
			var results = interp.Run() as List<object>;
			Assert.IsNotNull(results);
			object x = results[0], y = results[1], z = results[2], w = results[3];
			object flag = results[4], sum = results[5], strs = results[6];

			var expected_x = 100;
			var expected_y = 200;
			var expected_z = 300;
			var expected_w = 400;
			var expected_flag = true;
			var expected_sum = Helpers.CalcSum(0, expected_y);
			var expected_strs = new List<object>{"akarichan", "chinatsu", "kyoko", "yui"};

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

			Assert.IsTrue(strs is List<object>);
			Assert.AreEqual(expected_strs, strs);
		}
	}
}
