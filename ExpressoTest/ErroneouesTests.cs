using System;
using NUnit.Framework;

namespace Expresso.Test
{
    [TestFixture]
    public class ErroneouesTests
    {
        [Test]
        public void Literals()
        {
            var parser = new Parser(new Scanner("../../sources/for_unit_tests/erroneous/literals.exs"));
            parser.DoPostParseProcessing = true;
            parser.Parse();

            Assert.AreEqual(6, parser.errors.count);
        }

        [Test]
        public void Reassignment()
        {
            var parser = new Parser(new Scanner("../../sources/for_unit_tests/erroneous/reassignment.exs"));
            parser.DoPostParseProcessing = true;
            parser.Parse();

            Assert.AreEqual(1, parser.errors.count);
        }

        [Test]
        public void InterfaceNotImplemented()
        {
            var parser = new Parser(new Scanner("../../sources/for_unit_tests/erroneous/interface_not_implemented.exs"));
            parser.DoPostParseProcessing = true;
            parser.Parse();

            Assert.AreEqual(1, parser.errors.count);
        }

        [Test]
        public void UndefinedFunction()
        {
            var parser = new Parser(new Scanner("../../sources/for_unit_tests/erroneous/undefined_function.exs"));
            parser.DoPostParseProcessing = true;
            parser.Parse();

            Assert.AreEqual(3, parser.errors.count);
        }

        [Test]
        public void ReturningSelfType()
        {
            var parser = new Parser(new Scanner("../../sources/for_unit_tests/erroneous/returning_self_type.exs"));
            parser.DoPostParseProcessing = true;
            parser.Parse();

            Assert.AreEqual(1, parser.errors.count);
        }

        [Test]
        public void HasSelfParameter()
        {
            var parser = new Parser(new Scanner("../../sources/for_unit_tests/erroneous/has_self_parameter.exs"));
            parser.DoPostParseProcessing = true;
            parser.Parse();

            Assert.AreEqual(1, parser.errors.count);
        }

        [Test]
        public void HasNoReturnTypeInInterface()
        {
            var parser = new Parser(new Scanner("../../sources/for_unit_tests/erroneous/has_no_return_type_in_interface.exs"));
            parser.DoPostParseProcessing = true;
            parser.Parse();

            Assert.AreEqual(2, parser.errors.count);
        }

        [Test]
        public void FunctionAfterMainFunction()
        {
            var parser = new Parser(new Scanner("../../sources/for_unit_tests/erroneous/function_after_main_function.exs"));
            parser.DoPostParseProcessing = true;
            parser.Parse();

            Assert.AreEqual(3, parser.errors.count);
        }

        [Test]
        public void InvalidIterable()
        {
            var parser = new Parser(new Scanner("../../sources/for_unit_tests/erroneous/invalid_iterable.exs"));
            parser.DoPostParseProcessing = true;
            parser.Parse();

            Assert.AreEqual(1, parser.errors.count);
        }

        [Test]
        public void NonBooleanConditionExpression()
        {
            var parser = new Parser(new Scanner("../../sources/for_unit_tests/erroneous/non_boolean_condition_expression.exs"));
            parser.DoPostParseProcessing = true;
            parser.Parse();

            Assert.AreEqual(1, parser.errors.count);
        }

        [Test]
        public void ReassignDifferentType()
        {
            var parser = new Parser(new Scanner("../../sources/for_unit_tests/erroneous/reassign_different_type.exs"));
            parser.DoPostParseProcessing = true;
            parser.Parse();

            Assert.AreEqual(1, parser.errors.count);
        }

        [Test]
        public void InvalidIntseqExpression()
        {
            var parser = new Parser(new Scanner("../../sources/for_unit_tests/erroneous/invalid_intseq_expressions.exs"));
            parser.DoPostParseProcessing = true;
            parser.Parse();

            Assert.AreEqual(3, parser.errors.count);
        }
    }
}

