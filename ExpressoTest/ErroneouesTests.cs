﻿using System;
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

        [Test]
        public void InvalidOptionalValue()
        {
            var parser = new Parser(new Scanner("../../sources/for_unit_tests/erroneous/invalid_optional_value.exs"));
            parser.DoPostParseProcessing = true;
            parser.Parse();

            Assert.AreEqual(1, parser.errors.count);
        }

        [Test]
        public void InvalidUseOfNotOperator()
        {
            var parser = new Parser(new Scanner("../../sources/for_unit_tests/erroneous/invalid_use_of_not_operator.exs"));
            parser.DoPostParseProcessing = true;
            parser.Parse();

            Assert.AreEqual(2, parser.errors.count);
        }

        [Test]
        public void InvalidUseOfPlusOperator()
        {
            var parser = new Parser(new Scanner("../../sources/for_unit_tests/erroneous/invalid_use_of_plus_operator.exs"));
            parser.DoPostParseProcessing = true;
            parser.Parse();

            Assert.AreEqual(1, parser.errors.count);
        }

        [Test]
        public void TypeMismatchOnValueOfObjectCreation()
        {
            var parser = new Parser(new Scanner("../../sources/for_unit_tests/erroneous/type_mismatch_on_value_of_object_creation.exs"));
            parser.DoPostParseProcessing = true;
            parser.Parse();

            Assert.AreEqual(2, parser.errors.count);
        }

        [Test]
        public void MissingField()
        {
            var parser = new Parser(new Scanner("../../sources/for_unit_tests/erroneous/missing_field.exs"));
            parser.DoPostParseProcessing = true;
            parser.Parse();

            Assert.AreEqual(1, parser.errors.count);
        }

        [Test]
        public void MissingType()
        {
            var parser = new Parser(new Scanner("../../sources/for_unit_tests/erroneous/missing_type.exs"));
            parser.DoPostParseProcessing = true;
            parser.Parse();

            Assert.AreEqual(2, parser.errors.count);
        }

        [Test]
        public void InvalidSliceOnDictionary()
        {
            var parser = new Parser(new Scanner("../../sources/for_unit_tests/erroneous/invalid_slice_on_dictionary.exs"));
            parser.DoPostParseProcessing = true;
            parser.Parse();

            Assert.AreEqual(1, parser.errors.count);
        }

        [Test]
        public void InvalidSliceOnCustomType()
        {
            var parser = new Parser(new Scanner("../../sources/for_unit_tests/erroneous/invalid_slice_on_custom_type.exs"));
            parser.DoPostParseProcessing = true;
            parser.Parse();

            Assert.AreEqual(1, parser.errors.count);
        }
    }
}

