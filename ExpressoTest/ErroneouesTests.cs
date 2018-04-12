﻿using Expresso.Ast.Analysis;
using NUnit.Framework;

namespace Expresso.Test
{
    [TestFixture]
    public class ErroneouesTests
    {
        [Test]
        public void Literals()
        {
            var parser = new Parser(new Scanner("../../sources/for_unit_tests/erroneous/literals.exs")){
                DoPostParseProcessing = true
            };
            parser.Parse();

            //Assert.That(() => parser.Parse(), Throws.TypeOf<FatalError>().With.Message.Contains("Invalid syntax found"));
            Assert.AreEqual(9, parser.errors.count);
        }

        [Test]
        public void Reassignment()
        {
            var parser = new Parser(new Scanner("../../sources/for_unit_tests/erroneous/reassignment.exs")){
                DoPostParseProcessing = true
            };

            Assert.That(() => parser.Parse(), Throws.TypeOf<ParserException>().With.Message.Contains("ES1900"));
            Assert.AreEqual(1, parser.errors.count);
        }

        [Test]
        public void InterfaceNotImplemented()
        {
            var parser = new Parser(new Scanner("../../sources/for_unit_tests/erroneous/interface_not_implemented.exs")){
                DoPostParseProcessing = true
            };
            parser.Parse();

            // Assert.That isn't needed here because the above code doesn't throw an exception
            Assert.AreEqual(1, parser.errors.count);
        }

        [Test]
        public void UndefinedFunction()
        {
            var parser = new Parser(new Scanner("../../sources/for_unit_tests/erroneous/undefined_function.exs")){
                DoPostParseProcessing = true
            };

            Assert.That(() => parser.Parse(), Throws.TypeOf<ParserException>().With.Message.Contains("ES1000"));
            Assert.AreEqual(2, parser.errors.count);
        }

        [Test]
        public void ReturningSelfType()
        {
            var parser = new Parser(new Scanner("../../sources/for_unit_tests/erroneous/returning_self_type.exs")){
                DoPostParseProcessing = true
            };

            Assert.That(() => parser.Parse(), Throws.TypeOf<ParserException>().With.Message.Contains("ES1021"));
            Assert.AreEqual(1, parser.errors.count);
        }

        [Test]
        public void HasSelfParameter()
        {
            var parser = new Parser(new Scanner("../../sources/for_unit_tests/erroneous/has_self_parameter.exs")){
                DoPostParseProcessing = true
            };

            Assert.That(() => parser.Parse(), Throws.TypeOf<ParserException>().With.Message.Contains("ES1020"));
            Assert.AreEqual(1, parser.errors.count);
        }

        [Test]
        public void HasNoReturnTypeInInterface()
        {
            var parser = new Parser(new Scanner("../../sources/for_unit_tests/erroneous/has_no_return_type_in_interface.exs")){
                DoPostParseProcessing = true
            };

            Assert.That(() => parser.Parse(), Throws.TypeOf<ParserException>().With.Message.Contains("ES1602"));
            Assert.AreEqual(2, parser.errors.count);
        }

        [Test]
        public void FunctionAfterMainFunction()
        {
            var parser = new Parser(new Scanner("../../sources/for_unit_tests/erroneous/function_after_main_function.exs")){
                DoPostParseProcessing = true
            };

            Assert.That(() => parser.Parse(), Throws.TypeOf<ParserException>().With.Message.Contains("ES1805"));
            Assert.AreEqual(3, parser.errors.count);
        }

        [Test]
        public void InvalidIterable()
        {
            var parser = new Parser(new Scanner("../../sources/for_unit_tests/erroneous/invalid_iterable.exs")){
                DoPostParseProcessing = true
            };

            // Assert.That isn't needed here because the above code doesn't throw an exception
            Assert.That(() => parser.Parse(), Throws.TypeOf<ParserException>().With.Message.Contains("ES1301"));
            Assert.AreEqual(1, parser.errors.count);
        }

        [Test]
        public void NonBooleanConditionExpression()
        {
            var parser = new Parser(new Scanner("../../sources/for_unit_tests/erroneous/non_boolean_condition_expression.exs")){
                DoPostParseProcessing = true
            };
            parser.Parse();

            // Assert.That isn't needed here because the above code doesn't throw an exception
            Assert.AreEqual(1, parser.errors.count);
        }

        [Test]
        public void ReassignDifferentType()
        {
            var parser = new Parser(new Scanner("../../sources/for_unit_tests/erroneous/reassign_different_type.exs")){
                DoPostParseProcessing = true
            };
            parser.Parse();

            // Assert.That isn't needed here because the above code doesn't throw an exception
            Assert.AreEqual(1, parser.errors.count);
        }

        [Test]
        public void InvalidIntseqExpression()
        {
            var parser = new Parser(new Scanner("../../sources/for_unit_tests/erroneous/invalid_intseq_expression.exs")){
                DoPostParseProcessing = true
            };

            // Assert.That isn't needed here because the above code doesn't throw an exception
            Assert.That(() => parser.Parse(), Throws.TypeOf<ParserException>().With.Message.Contains("ES4001"));
            Assert.AreEqual(1, parser.errors.count);
        }

        [Test]
        public void InvalidOptionalValue()
        {
            var parser = new Parser(new Scanner("../../sources/for_unit_tests/erroneous/invalid_optional_value.exs")){
                DoPostParseProcessing = true
            };
            parser.Parse();

            // Assert.That isn't needed here because the above code doesn't throw an exception
            Assert.AreEqual(1, parser.errors.count);
        }

        [Test]
        public void InvalidUseOfNotOperator()
        {
            var parser = new Parser(new Scanner("../../sources/for_unit_tests/erroneous/invalid_use_of_not_operator.exs")){
                DoPostParseProcessing = true
            };
            parser.Parse();

            // Assert.That isn't needed here because the above code doesn't throw an exception
            Assert.AreEqual(1, parser.errors.count);
        }

        [Test]
        public void InvalidUseOfPlusOperator()
        {
            var parser = new Parser(new Scanner("../../sources/for_unit_tests/erroneous/invalid_use_of_plus_operator.exs")){
                DoPostParseProcessing = true
            };
            parser.Parse();

            // Assert.That isn't needed here because the above code doesn't throw an exception
            Assert.AreEqual(2, parser.errors.count);
        }

        [Test]
        public void TypeMismatchOnValueOfObjectCreation()
        {
            var parser = new Parser(new Scanner("../../sources/for_unit_tests/erroneous/type_mismatch_on_value_of_object_creation.exs")){
                DoPostParseProcessing = true
            };

            Assert.That(() => parser.Parse(), Throws.TypeOf<ParserException>().With.Message.Contains("ES2010"));
            Assert.AreEqual(2, parser.errors.count);
        }

        [Test]
        public void MissingField()
        {
            var parser = new Parser(new Scanner("../../sources/for_unit_tests/erroneous/missing_field.exs")){
                DoPostParseProcessing = true
            };

            Assert.That(() => parser.Parse(), Throws.TypeOf<ParserException>().With.Message.Contains("ES1502"));
            Assert.AreEqual(1, parser.errors.count);
        }

        [Test]
        public void MissingType()
        {
            var parser = new Parser(new Scanner("../../sources/for_unit_tests/erroneous/missing_type.exs")){
                DoPostParseProcessing = true
            };

            Assert.That(() => parser.Parse(), Throws.TypeOf<ParserException>().With.Message.Contains("ES1501"));
            Assert.AreEqual(2, parser.errors.count);
        }

        [Test]
        public void InvalidSliceOnDictionary()
        {
            var parser = new Parser(new Scanner("../../sources/for_unit_tests/erroneous/invalid_slice_on_dictionary.exs")){
                DoPostParseProcessing = true
            };

            Assert.That(() => parser.Parse(), Throws.TypeOf<ParserException>().With.Message.Contains("ES3012"));
            Assert.AreEqual(1, parser.errors.count);
        }

        [Test]
        public void InvalidSliceOnCustomType()
        {
            var parser = new Parser(new Scanner("../../sources/for_unit_tests/erroneous/invalid_slice_on_custom_type.exs")){
                DoPostParseProcessing = true
            };

            Assert.That(() => parser.Parse(), Throws.TypeOf<ParserException>().With.Message.Contains("ES3011"));
            Assert.AreEqual(1, parser.errors.count);
        }

        [Test]
        public void InvalidCasting()
        {
            var parser = new Parser(new Scanner("../../sources/for_unit_tests/erroneous/invalid_casting.exs")){
                DoPostParseProcessing = true
            };

            Assert.That(() => parser.Parse(), Throws.TypeOf<ParserException>().With.Message.Contains("ES1003"));
            Assert.AreEqual(1, parser.errors.count);
        }

        [Test]
        public void CallAVariable()
        {
            var parser = new Parser(new Scanner("../../sources/for_unit_tests/erroneous/call_a_variable.exs")){
                DoPostParseProcessing = true
            };

            Assert.That(() => parser.Parse(), Throws.TypeOf<ParserException>().With.Message.Contains("ES1805"));
            Assert.AreEqual(1, parser.errors.count);
        }

        [Test]
        public void InvalidBinaryOperator()
        {
            var parser = new Parser(new Scanner("../../sources/for_unit_tests/erroneous/invalid_binary_operator.exs")){
                DoPostParseProcessing = true
            };
            parser.Parse();

            // Assert.That isn't needed here because the above code doesn't throw an exception
            Assert.AreEqual(1, parser.errors.count);
        }

        [Test]
        public void InconsistentTypes()
        {
            var parser = new Parser(new Scanner("../../sources/for_unit_tests/erroneous/inconsistent_types.exs")){
                DoPostParseProcessing = true
            };
            parser.Parse();

            // Assert.That isn't needed here because the above code doesn't throw an exception
            Assert.AreEqual(2, parser.errors.count);
        }

        [Test]
        public void NotCompatibleWithRightHandSide()
        {
            var parser = new Parser(new Scanner("../../sources/for_unit_tests/erroneous/not_compatible_with_right_hand_side.exs")){
                DoPostParseProcessing = true
            };
            parser.Parse();

            // Assert.That isn't needed here because the above code doesn't throw an exception
            Assert.AreEqual(1, parser.errors.count);
        }

        [Test]
        public void NotCompatibleOnConditionalExpression()
        {
            var parser = new Parser(new Scanner("../../sources/for_unit_tests/erroneous/not_compatible_on_conditional_expression.exs")){
                DoPostParseProcessing = true
            };
            parser.Parse();

            // Assert.That isn't needed here because the above code doesn't throw an exception
            Assert.AreEqual(2, parser.errors.count);
        }

        [Test]
        public void TypesMismatchedOnMatchStatement()
        {
            var parser = new Parser(new Scanner("../../sources/for_unit_tests/erroneous/types_mismatched_on_match_statement.exs")){
                DoPostParseProcessing = true
            };
            parser.Parse();

            // Assert.That isn't needed here because the above code doesn't throw an exception
            Assert.AreEqual(1, parser.errors.count);
        }

        [Test]
        public void InvalidConditionalOnIfStatement()
        {
            var parser = new Parser(new Scanner("../../sources/for_unit_tests/erroneous/invalid_conditional_on_if_statement.exs")){
                DoPostParseProcessing = true
            };
            parser.Parse();

            // Assert.That isn't needed here because the above code doesn't throw an exception
            Assert.AreEqual(1, parser.errors.count);
        }

        [Test]
        public void InvalidBreakStatement()
        {
            var parser = new Parser(new Scanner("../../sources/for_unit_tests/erroneous/invalid_break_statement.exs")){
                DoPostParseProcessing = true
            };

            Assert.That(() => parser.Parse(), Throws.TypeOf<ParserException>().With.Message.Contains("ES4010"));
            Assert.AreEqual(1, parser.errors.count);
        }

        [Test]
        public void InvalidContinueStatement()
        {
            var parser = new Parser(new Scanner("../../sources/for_unit_tests/erroneous/invalid_continue_statement.exs")){
                DoPostParseProcessing = true
            };

            Assert.That(() => parser.Parse(), Throws.TypeOf<ParserException>().With.Message.Contains("ES4011"));
            Assert.AreEqual(1, parser.errors.count);
        }

        [Test]
        public void MissingElementType()
        {
            var parser = new Parser(new Scanner("../../sources/for_unit_tests/erroneous/missing_element_type.exs")){
                DoPostParseProcessing = true
            };
            parser.Parse();

            // Assert.That isn't needed here because the above code doesn't throw an exception
            Assert.AreEqual(1, parser.errors.count);
        }

        [Test]
        public void FunctionWithoutOptions()
        {
            var parser = new Parser(new Scanner("../../sources/for_unit_tests/erroneous/function_without_options.exs")){
                DoPostParseProcessing = true
            };
            parser.Parse();

            // Assert.That isn't needed here because the above code doesn't throw an exception
            Assert.AreEqual(2, parser.errors.count);
        }

        [Test]
        public void InvalidUnaryMinusPlusOperators()
        {
            var parser = new Parser(new Scanner("../../sources/for_unit_tests/erroneous/invalid_unary_minus_plus_operators.exs")){
                DoPostParseProcessing = true
            };
            parser.Parse();

            // Assert.That isn't needed here because the above code doesn't throw an exception
            Assert.AreEqual(4, parser.errors.count);
        }

        [Test]
        public void InvalidUnaryNotOperator()
        {
            var parser = new Parser(new Scanner("../../sources/for_unit_tests/erroneous/invalid_unary_not_operator.exs")){
                DoPostParseProcessing = true
            };
            parser.Parse();

            // Assert.That isn't needed here because the above code doesn't throw an exception
            Assert.AreEqual(1, parser.errors.count);
        }

        [Test]
        public void UnknownField()
        {
            var parser = new Parser(new Scanner("../../sources/for_unit_tests/erroneous/unknown_field.exs")){
                DoPostParseProcessing = true
            };

            Assert.That(() => parser.Parse(), Throws.TypeOf<ParserException>().With.Message.Contains("ES2001"));
            Assert.AreEqual(1, parser.errors.count);
        }

        [Test]
        public void PrivateAccessibility()
        {
            var parser = new Parser(new Scanner("../../sources/for_unit_tests/erroneous/private_accessibility.exs")){
                DoPostParseProcessing = true
            };

            Assert.That(() => parser.Parse(), Throws.TypeOf<FatalError>().With.Message.Contains("Accessibility or immutability error"));
            Assert.AreEqual(1, parser.errors.count);
        }

        [Test]
        public void ProtectedAccessibility()
        {
            var parser = new Parser(new Scanner("../../sources/for_unit_tests/erroneous/protected_accessibility.exs")){
                DoPostParseProcessing = true
            };

            Assert.That(() => parser.Parse(), Throws.TypeOf<FatalError>().With.Message.Contains("Accessibility or immutability error"));
            Assert.AreEqual(1, parser.errors.count);
        }

        [Test]
        public void MemberImmutability()
        {
            var parser = new Parser(new Scanner("../../sources/for_unit_tests/erroneous/member_immutability.exs")){
                DoPostParseProcessing = true
            };

            Assert.That(() => parser.Parse(), Throws.TypeOf<FatalError>().With.Message.Contains("Accessibility or immutability error"));
            Assert.AreEqual(1, parser.errors.count);
        }

        [Test]
        public void DifferentAccessModifierOnMethods()
        {
            var parser = new Parser(new Scanner("../../sources/for_unit_tests/erroneous/different_access_modifier_on_methods.exs")){
                DoPostParseProcessing = true
            };

            Assert.That(() => parser.Parse(), Throws.TypeOf<ParserException>().With.Message.Contains("ES1030"));
            Assert.AreEqual(1, parser.errors.count);
        }

        [Test]
        public void TypesMismatchedOnArgument()
        {
            var parser = new Parser(new Scanner("../../sources/for_unit_tests/erroneous/types_mismatched_on_argument.exs")){
                DoPostParseProcessing = true
            };

            Assert.That(() => parser.Parse(), Throws.TypeOf<ParserException>().With.Message.Contains("ES1303"));
            Assert.AreEqual(1, parser.errors.count);
        }

        [Test]
        public void PrimitiveTypesCantBeDerived()
        {
            var parser = new Parser(new Scanner("../../sources/for_unit_tests/erroneous/primitive_types_cant_be_derived.exs")){
                DoPostParseProcessing = true
            };

            Assert.That(() => parser.Parse(), Throws.TypeOf<ParserException>().With.Message.Contains("ES1911"));
            Assert.AreEqual(1, parser.errors.count);
        }

        [Test]
        public void CantBeDerivedFromStandardLibrariesType()
        {
            var parser = new Parser(new Scanner("../../sources/for_unit_tests/erroneous/cant_be_derived_from_standard_libraries_type.exs")){
                DoPostParseProcessing = true
            };

            Assert.That(() => parser.Parse(), Throws.TypeOf<ParserException>().With.Message.Contains("ES1912"));
            Assert.AreEqual(1, parser.errors.count);
        }

        [Test]
        public void InvalidUseOfNull()
        {
            var parser = new Parser(new Scanner("../../sources/for_unit_tests/erroneous/invalid_use_of_null.exs")){
                DoPostParseProcessing = true
            };

            Assert.That(() => parser.Parse(), Throws.TypeOf<ParserException>().With.Message.Contains("ES1022"));
            Assert.AreEqual(1, parser.errors.count);
        }

        [Test]
        public void CallMutatingMethodOnImmutableVarible()
        {
            var parser = new Parser(new Scanner("../../sources/for_unit_tests/erroneous/call_mutating_method_on_immutable_variable.exs")){
                DoPostParseProcessing = true
            };

            Assert.That(() => parser.Parse(), Throws.TypeOf<ParserException>().With.Message.Contains("ES2100"));
            Assert.AreEqual(1, parser.errors.count);
        }

        [Test]
        public void ImportUnexportedItem()
        {
            var parser = new Parser(new Scanner("../../sources/for_unit_tests/erroneous/import_unexported_item.exs")){
                DoPostParseProcessing = true
            };

            Assert.That(() => parser.Parse(), Throws.TypeOf<ParserException>().With.Message.Contains("ES3303"));
            Assert.AreEqual(1, parser.errors.count);
        }

        [Test]
        public void ImportUndefiedSymbol()
        {
            var parser = new Parser(new Scanner("../../sources/for_unit_tests/erroneous/import_undefined_symbol.exs")){
                DoPostParseProcessing = true
            };

            Assert.That(() => parser.Parse(), Throws.TypeOf<ParserException>().With.Message.Contains("ES0103"));
            Assert.AreEqual(1, parser.errors.count);
        }

        [Test]
        public void OutOfIntRangeIntSeq()
        {
            var parser = new Parser(new Scanner("../../sources/for_unit_tests/erroneous/out_of_int_range_intseq.exs")){
                DoPostParseProcessing = true
            };

            Assert.That(() => parser.Parse(), Throws.TypeOf<ParserException>().With.Message.Contains("ES4002"));
            Assert.AreEqual(1, parser.errors.count);
        }

        [Test]
        public void IntseqWarnings()
        {
            var parser = new Parser(new Scanner("../../sources/for_unit_tests/erroneous/intseq_warnings.exs")){
                DoPostParseProcessing = true
            };
            parser.Parse();
        }

        [Test]
        public void InvalidUseOfIndexerOperator()
        {
            var parser = new Parser(new Scanner("../../sources/for_unit_tests/erroneous/invalid_use_of_indexer_operator.exs")){
                DoPostParseProcessing = true
            };

            Assert.That(() => parser.Parse(), Throws.TypeOf<ParserException>().With.Message.Contains("ES3013"));
            Assert.AreEqual(1, parser.errors.count);
        }

        /*[Test]
        public void MemberOfNotAType()
        {
            var parser = new Parser(new Scanner("../../sources/for_unit_tests/erroneous/member_of_not_a_type.exs")){
                DoPostParseProcessing = true
            };
            parser.Parse();

            Assert.AreEqual(2, parser.errors.count);
        }*/
    }
}

