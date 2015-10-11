using System;
using System.Collections.Generic;
using System.Linq;
using Expresso.Ast;
using Expresso.Runtime.Builtins;
using ICSharpCode.NRefactory;
using NUnit.Framework;

namespace Expresso.Test
{
    [TestFixture]
    public class EvaluationTests
    {
        [Test]
        public void SimpleArithmetic()
        {
            var parser = new Parser(new Scanner("../../sources/for_unit_tests/simple_arithmetic.exs"));
            parser.DoPostParseProcessing = true;
            parser.Parse();

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

            //Helpers.DoTest(results, expected);
        }

        [Test]
        public void GeneralOperations()
        {
            var parser = new Parser(new Scanner("../../sources/for_unit_tests/general_operations.exs"));
            parser.DoPostParseProcessing = true;
            parser.Parse();

            var expected_a = new List<int>{1, 2, 3};
            var expected_b = new Dictionary<string, int>{
                {"a", 1},
                {"b", 4},
                {"y", 10}
            };
            var expected_x = 100;

            var expected_p = expected_a[0] + expected_a[1] + expected_a[2];
            var expected_q = expected_b["a"] + expected_b["b"] + expected_b["y"];
            var expected_r = expected_x >> expected_p;
            var expected_s = expected_x << 2;
            var expected_t = expected_r & expected_s;
            var expected_u = expected_x | expected_t;

            var expected = new object[]{
                expected_a,
                expected_b,
                "akarichan",    //c
                "chinatsu",     //d
                expected_x,
                expected_p,
                expected_q,
                expected_r,
                expected_s,
                expected_t,
                expected_u,
                "akarichan" + "chinatsu"    //v
            };

            //Helpers.DoTest(results, expected);
        }

        [Test]
        public void Statements()
        {
            var parser = new Parser(new Scanner("../../sources/for_unit_tests/statements.exs"));
            parser.DoPostParseProcessing = true;
            parser.Parse();

            var expected_x = 100;
            var expected_y = 200;
            var expected_z = 300;
            var expected_w = 400;
            var expected_sum = Helpers.CalcSum(0, expected_y);
            var expected_strs = new List<string>{"akarichan", "chinatsu", "kyoko", "yui"};
            var expected_fibs = new List<int>();
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
                select Tuple.Create(i, j);

            var expected = new object[]{
                expected_x,
                expected_y,
                expected_z,
                expected_w,
                true,   //flag
                expected_sum,
                expected_strs,
                expected_fibs,
                new List<object>(expected_ary)
            };

            //Helpers.DoTest(results, expected);
        }

        [Test]
        public void BuiltinObjects()
        {
            var parser = new Parser(new Scanner("../../sources/for_unit_tests/builtin_objects.exs"));
            parser.DoPostParseProcessing = true;
            parser.Parse();

            var expected_a = new List<int>{1, 2, 3, 4, 5, 6, 7, 8};
            var expected_b = new Dictionary<string, int>{
                {"akari", 13},
                {"chinatsu", 13},
                {"kyoko", 14},
                {"yui", 14}
            };
            var expected_c = Tuple.Create("akarichan", "kawakawa", "chinatsuchan", 2424);

            var expected_d = expected_a.Select((x, i) => i < 3);

            var expected = new object[]{
                expected_a,
                expected_b,
                expected_c,
                expected_d,
                expected_c.Item3    //e
            };

            //Helpers.DoTest(results, expected);
        }

        [Test]
        public void ComplexExpressions()
        {
            var parser = new Parser(new Scanner("../../sources/for_unit_tests/complex_expressions.exs"));
            parser.Parse();

            var tmp_x =
                from i in Enumerable.Range(0, 100)
                select i;

            var expected_x = tmp_x.ToList();

            var tmp_y =
                from j in Enumerable.Range(0, 100)
                    where j % 2 == 0
                select j;

            var expected_y = tmp_y.ToList();

            var tmp_z =
                from k in Enumerable.Range(0, 100)
                    where k % 2 == 0
                from l in Enumerable.Range(0, 100)
                select Tuple.Create(k, l);

            var expected_z = new List<object>(tmp_z);

            var expected_m = Tuple.Create(1, 3);

            var expected = new object[]{
                expected_x,
                expected_y,
                expected_z,
                0,      //a
                0,      //b
                0,      //c
                expected_m
            };

            //Helpers.DoTest(results, expected);
        }

        [Test]
        public void Class()
        {
            var parser = new Parser(new Scanner("../../sources/for_unit_tests/class.exs"));
            parser.Parse();

            var expected = new object[]{
                1,      //b
                3,      //c
                101     //d
            };

            var private_names = new List<string>{
                "x",
                "y"
            };

            var method_annots = new List<FunctionAnnotation>{
                new FunctionAnnotation("getX", new PrimitiveType("int", TextLocation.Empty)),
                new FunctionAnnotation("getY", new PrimitiveType("int", TextLocation.Empty)),
                new FunctionAnnotation("getXPlus", new PrimitiveType("int", TextLocation.Empty))
            };

            //var a = results[0] as ExpressoObj;
            //Assert.IsNotNull(a);
            //Helpers.TestOnType(a, private_names, method_annots);

            //var numeric_results = ExpressoOps.Slice(results, new ExpressoIntegerSequence(1, results.Count, 1)) as List<object>;
            //Helpers.DoTest(numeric_results, expected);
        }

        [Test]
        public void Library()
        {

        }

        [Test]
        public void Module()
        {
            var parser = new Parser(new Scanner("../../sources/for_unit_tests/module.exs"));
            parser.Parse();

            var private_names = new List<string>{
                "x",
                "y"
            };

            var method_annots = new List<FunctionAnnotation>{
                new FunctionAnnotation("getX", new PrimitiveType("int", TextLocation.Empty)),
                new FunctionAnnotation("getY", new PrimitiveType("int", TextLocation.Empty)),
            };

            /*var a = results[0] as ExpressoObj;
            Assert.IsNotNull(a);
            Helpers.TestOnType(a, private_names, method_annots);

            var b = results[1] as ExpressoObj;
            Assert.IsNotNull(b);
            Helpers.TestOnType(b, private_names, method_annots);

            var c = results[2] as ExpressoTuple;
            Assert.IsNotNull(c);
            Assert.AreEqual(c, ExpressoOps.MakeTuple(new object[]{200, 300}));*/
        }
    }
}

