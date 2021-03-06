﻿using System;
using NUnit.Framework;
using Expresso.Runtime.Builtins;
using System.Collections.Generic;
using System.Linq;

namespace Expresso.Test
{
    [TestFixture]
    public class NativeTests
    {
        [Test]
        public void TestFraction()
        {
            /*Fraction a = new Fraction(1, 3), b = new Fraction(2, 3), c = new Fraction(-1, 4), d = new Fraction(new BigInteger(3)), e = new Fraction(2.5);

            var expected = new object[]{
                new Fraction(3, 3),     //a + b
                new Fraction(-1, 3),    //a - b
                new Fraction(2, 9),     //a * b
                new Fraction(1, 2),     //a / b
                new Fraction(1, 12),    //a + c
                new Fraction(7, 12),    //a - c
                new Fraction(-1, 12),   //a * c
                new Fraction(-4, 3),    //a / c
                new Fraction(10, 3),    //a + d
                new Fraction(-8, 3),    //a - d
                new Fraction(3, 3),     //a * d
                new Fraction(1, 9),     //a / d
                new Fraction(17, 6),    //a + e
                new Fraction(-13, 6),   //a - e
                new Fraction(5, 6),     //a * e
                new Fraction(2, 15)     //a / e
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

            Helpers.DoTest(targets, expected);*/
        }

        [Test]
        public void IntegerSequence()
        {
            var seq1 = new ExpressoIntegerSequence(0, 10, 1, false);
            var seq2 = new ExpressoIntegerSequence(0, -10, -1, false);
            var seq3 = new ExpressoIntegerSequence(0, 100, 2, true);
            var seq4 = new ExpressoIntegerSequence(5, 15, 2, false);
            var seq5 = ExpressoIntegerSequence.CreateArrayFromIntSeq(seq1);
            var seq6 = ExpressoIntegerSequence.CreateListFromIntSeq(seq1);
            var seq7 = ExpressoIntegerSequence.CreateArrayFromIntSeq(seq2);
            var seq8 = ExpressoIntegerSequence.CreateArrayFromIntSeq(seq3);
            var seq9 = ExpressoIntegerSequence.CreateArrayFromIntSeq(seq4);

            List<int> expected = new List<int>{0, 1, 2, 3, 4, 5, 6, 7, 8, 9}, expected2 = new List<int>{0, -1, -2, -3, -4, -5, -6, -7, -8, -9};
            var expected3 =
                from i in Enumerable.Range(0, 101)
                    where i % 2 == 0
                select i;
            var expected4 = new List<int>{5, 7, 9, 11, 13};
            var expected5 = new []{0, 1, 2, 3, 4, 5, 6, 7, 8, 9};
            var expected6 = new List<int>{0, 1, 2, 3, 4, 5, 6, 7, 8, 9};
            var expected7 = new []{0, -1, -2, -3, -4, -5, -6, -7, -8, -9};
            var expected8 =
                from i in Enumerable.Range(0, 101)
                                    where i % 2 == 0
                                    select i;
            var expected9 = new []{5, 7, 9, 11, 13};
            Assert.IsTrue(seq1.IsSequential());
            Assert.IsFalse(seq1.Includes(10));
            Assert.IsFalse(seq2.Includes(-10));
            Assert.IsTrue(seq3.Includes(50));
            Assert.IsFalse(seq3.Includes(49));

            var seq1_list =
                from i in seq1
                select i;
            var seq2_list =
                from j in seq2
                select j;
            var seq3_list =
                from k in seq3
                select k;
            var seq4_list =
                from l in seq4
                select l;
            Assert.IsTrue(seq1_list.SequenceEqual(expected));
            Assert.IsTrue(seq2_list.SequenceEqual(expected2));
            Assert.IsTrue(seq3_list.SequenceEqual(expected3));
            Assert.IsTrue(seq4_list.SequenceEqual(expected4));
            Assert.IsTrue(seq5.GetType().IsArray);
            Assert.IsTrue(seq5.SequenceEqual(expected5));
            Assert.IsTrue(seq6.GetType().Name.StartsWith("List", StringComparison.CurrentCulture));
            Assert.IsTrue(seq6.SequenceEqual(expected6));
            Assert.IsTrue(seq7.SequenceEqual(expected7));
            Assert.IsTrue(seq8.SequenceEqual(expected8));
            Assert.IsTrue(seq9.SequenceEqual(expected9));
        }

        [Test]
        public void Slice()
        {
            var list = new List<int>{1, 2, 3, 4, 5, 6, 7, 8, 9, 10};
            var slice = new Slice<List<int>, int>(list, new ExpressoIntegerSequence(2, 4, 1, false));
            var slice2 = new Slice<List<int>, int>(list, new ExpressoIntegerSequence(2, 4, 1, true));
            var slice3 = new Slice<List<int>, int>(list, new ExpressoIntegerSequence(0, 9, 2, true));

            var expected = new List<int>{3, 4};
            var expected2 = new List<int>{3, 4, 5};
            var expected3 = new List<int>{1, 3, 5, 7, 9};

            var slice1_list = 
                from i in slice
                select i;
            var slice2_list =
                from j in slice2
                select j;
            var slice3_list =
                from k in slice3
                select k;

            Assert.IsTrue(slice1_list.SequenceEqual(expected));
            Assert.IsTrue(slice2_list.SequenceEqual(expected2));
            Assert.IsTrue(slice3_list.SequenceEqual(expected3));
        }
    }
}

