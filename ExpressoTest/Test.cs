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
			object d = results[4], e = results[5], f = results[6];

			Assert.IsTrue(x is int);
			Assert.AreEqual(3, Helpers.AutoCast<int>(x));

			Assert.IsTrue(a is int);
			Assert.AreEqual(7, Helpers.AutoCast<int>(a));

			Assert.IsTrue(b is int);
			Assert.AreEqual(-1, Helpers.AutoCast<int>(b));

			Assert.IsTrue(c is int);
			Assert.AreEqual(12, Helpers.AutoCast<int>(c));

			Assert.IsTrue(d is int);
			Assert.AreEqual(2, Helpers.AutoCast<int>(d));

			Assert.IsTrue(e is int);
			Assert.AreEqual(0, Helpers.AutoCast<int>(e));

			Assert.IsTrue(f is int);
			Assert.AreEqual(9, Helpers.AutoCast<int>(f));
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
			Assert.AreEqual(a, expected_a);

			Assert.IsTrue(b is Dictionary<object, object>);
			Assert.AreEqual(b, expected_b);

			Assert.IsTrue(c is string);
			Assert.AreEqual(c, expected_c);

			Assert.IsTrue(d is string);
			Assert.AreEqual(d, expected_d);

			Assert.IsTrue(x is int);
			Assert.AreEqual(x, expected_x);

			Assert.IsTrue(p is int);
			Assert.AreEqual(p, expected_p);

			Assert.IsTrue(q is int);
			Assert.AreEqual(q, expected_q);

			Assert.IsTrue(r is int);
			Assert.AreEqual(r, expected_r);

			Assert.IsTrue(s is int);
			Assert.AreEqual(s, expected_s);

			Assert.IsTrue(t is int);
			Assert.AreEqual(t, expected_t);

			Assert.IsTrue(u is int);
			Assert.AreEqual(u, expected_u);

			Assert.IsTrue(v is string);
			Assert.AreEqual(v, expected_v);
		}
	}
}
