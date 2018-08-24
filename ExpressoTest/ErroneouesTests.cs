using Expresso.Ast.Analysis;
using NUnit.Framework;

namespace Expresso.Test
{
    [TestFixture]
    public class ErroneouesTests
    {
        [Test]
        public void Literals1()
        {
            var parser = new Parser(new Scanner("../../sources/for_unit_tests/erroneous/literals1.exs")){
                DoPostParseProcessing = true
            };

#if DEBUG
            Assert.That(() => parser.Parse(), Throws.TypeOf<ParserException>().With.Message.Contains("ES0003"));
#else
            Assert.That(() => parser.Parse(), Throws.TypeOf<FatalError>().With.Message.Contains("Invalid syntax found"));
#endif
            Assert.AreEqual(2, parser.errors.count);
        }

        [Test]
        public void Literals2()
        {
            var parser = new Parser(new Scanner("../../sources/for_unit_tests/erroneous/literals2.exs")) {
                DoPostParseProcessing = true
            };

#if DEBUG
            Assert.That(() => parser.Parse(), Throws.TypeOf<ParserException>().With.Message.Contains("ES0003"));
#else
            Assert.That(() => parser.Parse(), Throws.TypeOf<FatalError>().With.Message.Contains("Invalid syntax found"));
#endif
            Assert.AreEqual(3, parser.errors.count);
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

            Assert.That(() => parser.Parse(), Throws.TypeOf<ParserException>().With.Message.Contains("ES1910"));
            Assert.AreEqual(1, parser.errors.count);
        }

        [Test]
        public void UndefinedFunction()
        {
            var parser = new Parser(new Scanner("../../sources/for_unit_tests/erroneous/undefined_function.exs")){
                DoPostParseProcessing = true
            };

            Assert.That(() => parser.Parse(), Throws.TypeOf<ParserException>().With.Message.Contains("ES0102"));
            Assert.AreEqual(1, parser.errors.count);
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

                                                                                       // TODO: consider it again
            Assert.That(() => parser.Parse(), Throws.TypeOf<ParserException>().With.Message.Contains("ES1602"));
            Assert.AreEqual(2, parser.errors.count);
        }

        [Test]
        public void CantCallFunctionBeforeDeclared()
        {
            var parser = new Parser(new Scanner("../../sources/for_unit_tests/erroneous/cant_call_function_before_declared.exs")){
                DoPostParseProcessing = true
            };

            Assert.That(() => parser.Parse(), Throws.TypeOf<ParserException>().With.Message.Contains("ES0120"));
            Assert.AreEqual(1, parser.errors.count);
        }

        [Test]
        public void InvalidIterable()
        {
            var parser = new Parser(new Scanner("../../sources/for_unit_tests/erroneous/invalid_iterable.exs")){
                DoPostParseProcessing = true
            };

            Assert.That(() => parser.Parse(), Throws.TypeOf<ParserException>().With.Message.Contains("ES1301"));
            Assert.AreEqual(1, parser.errors.count);
        }

        [Test]
        public void NonBooleanConditionExpression()
        {
            var parser = new Parser(new Scanner("../../sources/for_unit_tests/erroneous/non_boolean_condition_expression.exs")){
                DoPostParseProcessing = true
            };

            Assert.That(() => parser.Parse(), Throws.TypeOf<ParserException>().With.Message.Contains("ES4000"));
            Assert.AreEqual(1, parser.errors.count);
        }

        [Test]
        public void ReassignDifferentType()
        {
            var parser = new Parser(new Scanner("../../sources/for_unit_tests/erroneous/reassign_different_type.exs")){
                DoPostParseProcessing = true
            };

            Assert.That(() => parser.Parse(), Throws.TypeOf<ParserException>().With.Message.Contains("ES1100"));
            Assert.AreEqual(1, parser.errors.count);
        }

        [Test]
        public void InvalidIntseqExpression()
        {
            var parser = new Parser(new Scanner("../../sources/for_unit_tests/erroneous/invalid_intseq_expression.exs")){
                DoPostParseProcessing = true
            };

            Assert.That(() => parser.Parse(), Throws.TypeOf<ParserException>().With.Message.Contains("ES4001"));
            Assert.AreEqual(1, parser.errors.count);
        }

        [Test]
        public void InvalidOptionalValue()
        {
            var parser = new Parser(new Scanner("../../sources/for_unit_tests/erroneous/invalid_optional_value.exs")){
                DoPostParseProcessing = true
            };

            Assert.That(() => parser.Parse(), Throws.TypeOf<ParserException>().With.Message.Contains("ES1110"));
            Assert.AreEqual(1, parser.errors.count);
        }

        [Test]
        public void InvalidUseOfNotOperator()
        {
            var parser = new Parser(new Scanner("../../sources/for_unit_tests/erroneous/invalid_use_of_not_operator.exs")){
                DoPostParseProcessing = true
            };

            Assert.That(() => parser.Parse(), Throws.TypeOf<ParserException>().With.Message.Contains("ES1200"));
            Assert.AreEqual(1, parser.errors.count);
        }

        [Test]
        public void InvalidUseOfPlusOperator()
        {
            var parser = new Parser(new Scanner("../../sources/for_unit_tests/erroneous/invalid_use_of_plus_operator.exs")){
                DoPostParseProcessing = true
            };

            Assert.That(() => parser.Parse(), Throws.TypeOf<ParserException>().With.Message.Contains("ES3200"));
            Assert.AreEqual(1, parser.errors.count);
        }

        [Test]
        public void TypeMismatchOnValueOfObjectCreation()
        {
            var parser = new Parser(new Scanner("../../sources/for_unit_tests/erroneous/type_mismatch_on_value_of_object_creation.exs")){
                DoPostParseProcessing = true
            };

            Assert.That(() => parser.Parse(), Throws.TypeOf<ParserException>().With.Message.Contains("ES2003"));
            Assert.AreEqual(1, parser.errors.count);
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
        public void MissingTypeDeclaration()
        {
            var parser = new Parser(new Scanner("../../sources/for_unit_tests/erroneous/missing_type_declaration.exs")){
                DoPostParseProcessing = true
            };

            Assert.That(() => parser.Parse(), Throws.TypeOf<ParserException>().With.Message.Contains("ES0101"));
            Assert.AreEqual(1, parser.errors.count);
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

            Assert.That(() => parser.Parse(), Throws.TypeOf<ParserException>().With.Message.Contains("ES1002"));
            Assert.AreEqual(1, parser.errors.count);
        }

        [Test]
        public void InconsistentTypes()
        {
            var parser = new Parser(new Scanner("../../sources/for_unit_tests/erroneous/inconsistent_types.exs")){
                DoPostParseProcessing = true
            };

            Assert.That(() => parser.Parse(), Throws.TypeOf<ParserException>().With.Message.Contains("ES1300"));
            Assert.AreEqual(1, parser.errors.count);
        }

        [Test]
        public void NotCompatibleWithRightHandSide()
        {
            var parser = new Parser(new Scanner("../../sources/for_unit_tests/erroneous/not_compatible_with_right_hand_side.exs")){
                DoPostParseProcessing = true
            };

            Assert.That(() => parser.Parse(), Throws.TypeOf<ParserException>().With.Message.Contains("ES1100"));
            Assert.AreEqual(1, parser.errors.count);
        }

        [Test]
        public void NotCompatibleOnConditionalExpression()
        {
            var parser = new Parser(new Scanner("../../sources/for_unit_tests/erroneous/not_compatible_on_conditional_expression.exs")){
                DoPostParseProcessing = true
            };

            Assert.That(() => parser.Parse(), Throws.TypeOf<ParserException>().With.Message.Contains("ES1006"));
            Assert.AreEqual(1, parser.errors.count);
        }

        [Test]
        public void TypesMismatchedOnMatchStatement()
        {
            var parser = new Parser(new Scanner("../../sources/for_unit_tests/erroneous/types_mismatched_on_match_statement.exs")){
                DoPostParseProcessing = true
            };

            Assert.That(() => parser.Parse(), Throws.TypeOf<ParserException>().With.Message.Contains("ES1023"));
            Assert.AreEqual(1, parser.errors.count);
        }

        [Test]
        public void InvalidConditionalOnIfStatement()
        {
            var parser = new Parser(new Scanner("../../sources/for_unit_tests/erroneous/invalid_conditional_on_if_statement.exs")){
                DoPostParseProcessing = true
            };

            Assert.That(() => parser.Parse(), Throws.TypeOf<ParserException>().With.Message.Contains("ES4000"));
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

            Assert.That(() => parser.Parse(), Throws.TypeOf<ParserException>().With.Message.Contains("ES1312"));
            Assert.AreEqual(1, parser.errors.count);
        }

        [Test]
        public void InvalidUnaryMinusOperator()
        {
            var parser = new Parser(new Scanner("../../sources/for_unit_tests/erroneous/invalid_unary_minus_operator.exs")){
                DoPostParseProcessing = true
            };

            Assert.That(() => parser.Parse(), Throws.TypeOf<ParserException>().With.Message.Contains("ES3200"));
            Assert.AreEqual(1, parser.errors.count);
        }

        [Test]
        public void InvalidUnaryPlusOperator()
        {
            var parser = new Parser(new Scanner("../../sources/for_unit_tests/erroneous/invalid_unary_plus_operator.exs")){
                DoPostParseProcessing = true
            };

            Assert.That(() => parser.Parse(), Throws.TypeOf<ParserException>().With.Message.Contains("ES3200"));
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

            Assert.That(() => parser.Parse(), Throws.TypeOf<ParserException>().With.Message.Contains("ES3300"));
            Assert.AreEqual(1, parser.errors.count);
        }

        [Test]
        public void ProtectedAccessibility()
        {
            var parser = new Parser(new Scanner("../../sources/for_unit_tests/erroneous/protected_accessibility.exs")){
                DoPostParseProcessing = true
            };

            Assert.That(() => parser.Parse(), Throws.TypeOf<ParserException>().With.Message.Contains("ES3301"));
            Assert.AreEqual(1, parser.errors.count);
        }

        [Test]
        public void MemberImmutability()
        {
            var parser = new Parser(new Scanner("../../sources/for_unit_tests/erroneous/member_immutability.exs")){
                DoPostParseProcessing = true
            };

            Assert.That(() => parser.Parse(), Throws.TypeOf<ParserException>().With.Message.Contains("ES1902"));
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

            Assert.That(() => parser.Parse(), Throws.TypeOf<ParserException>().With.Message.Contains("ES1913"));
            Assert.AreEqual(1, parser.errors.count);
        }

        [Test]
        public void CantBeDerivedFromVectorType()
        {
            var parser = new Parser(new Scanner("../../sources/for_unit_tests/erroneous/cant_be_derived_from_vector_type.exs")){
                DoPostParseProcessing = true
            };

            Assert.That(() => parser.Parse(), Throws.TypeOf<ParserException>().With.Message.Contains("ES1913"));
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
        public void IntseqWarning1()
        {
            var parser = new Parser(new Scanner("../../sources/for_unit_tests/erroneous/intseq_warning1.exs")){
                DoPostParseProcessing = true
            };

            Assert.That(() => parser.Parse(), Throws.TypeOf<ParserException>().With.Message.Contains("ES4003"));
            Assert.AreEqual(1, parser.errors.count);
        }

        [Test]
        public void IntseqWarning2()
        {
            var parser = new Parser(new Scanner("../../sources/for_unit_tests/erroneous/intseq_warning2.exs")){
                DoPostParseProcessing = true
            };

            Assert.That(() => parser.Parse(), Throws.TypeOf<ParserException>().With.Message.Contains("ES4004"));
            Assert.AreEqual(1, parser.errors.count);
        }

        [Test]
        public void IntseqWarning3()
        {
            var parser = new Parser(new Scanner("../../sources/for_unit_tests/erroneous/intseq_warning3.exs")){
                DoPostParseProcessing = true
            };

            Assert.That(() => parser.Parse(), Throws.TypeOf<ParserException>().With.Message.Contains("ES4005"));
            Assert.AreEqual(1, parser.errors.count);
        }

        [Test]
        public void IntseqWarning4()
        {
            var parser = new Parser(new Scanner("../../sources/for_unit_tests/erroneous/intseq_warning4.exs")){
                DoPostParseProcessing = true
            };

            Assert.That(() => parser.Parse(), Throws.TypeOf<ParserException>().With.Message.Contains("ES4006"));
            Assert.AreEqual(1, parser.errors.count);
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

        [Test]
        public void KeywordIdentifier()
        {
            var parser = new Parser(new Scanner("../../sources/for_unit_tests/erroneous/keyword_identifier.exs")){
                DoPostParseProcessing = true
            };

            parser.Parse();
            Assert.AreEqual(1, parser.errors.count);
        }

        [Test]
        public void ImportFromUnknownFile()
        {
            var parser = new Parser(new Scanner("../../sources/for_unit_tests/erroneous/import_from_unknown_file.exs")){
                DoPostParseProcessing = true
            };

            Assert.That(() => parser.Parse(), Throws.TypeOf<ParserException>().With.Message.Contains("ES0020"));
            Assert.AreEqual(1, parser.errors.count);
        }

        [Test]
        public void NonOptionalParameterAfterOptionalParameter()
        {
            var parser = new Parser(new Scanner("../../sources/for_unit_tests/erroneous/non_optional_parameter_after_optional_parameter.exs")){
                DoPostParseProcessing = true
            };

            Assert.That(() => parser.Parse(), Throws.TypeOf<ParserException>().With.Message.Contains("ES0002"));
            Assert.AreEqual(1, parser.errors.count);
        }

        [Test]
        public void InvalidVariadicParameter()
        {
            var parser = new Parser(new Scanner("../../sources/for_unit_tests/erroneous/invalid_variadic_parameter.exs")){
                DoPostParseProcessing = true
            };

            Assert.That(() => parser.Parse(), Throws.TypeOf<ParserException>().With.Message.Contains("ES0010"));
            Assert.AreEqual(1, parser.errors.count);
        }

        [Test]
        public void NonArrayVariadicParameter()
        {
            var parser = new Parser(new Scanner("../../sources/for_unit_tests/erroneous/non_array_variadic_parameter.exs")){
                DoPostParseProcessing = true
            };

            Assert.That(() => parser.Parse(), Throws.TypeOf<ParserException>().With.Message.Contains("ES0001"));
            Assert.AreEqual(1, parser.errors.count);
        }

        [Test]
        public void TypeAnnotationAndOptionalValueOmitted()
        {
            var parser = new Parser(new Scanner("../../sources/for_unit_tests/erroneous/type_annotation_and_optional_value_omitted.exs")){
                DoPostParseProcessing = true
            };

            Assert.That(() => parser.Parse(), Throws.TypeOf<ParserException>().With.Message.Contains("ES0004"));
            Assert.AreEqual(1, parser.errors.count);
        }

        [Test]
        public void FieldOfTuple()
        {
            var parser = new Parser(new Scanner("../../sources/for_unit_tests/erroneous/field_of_tuple.exs")){
                DoPostParseProcessing = true
            };

            Assert.That(() => parser.Parse(), Throws.TypeOf<ParserException>().With.Message.Contains("ES0021"));
            Assert.AreEqual(2, parser.errors.count);
        }

        [Test]
        public void NotBalancedAugmentedAssignment()
        {
            var parser = new Parser(new Scanner("../../sources/for_unit_tests/erroneous/not_balanced_augmented_assignment.exs")){
                DoPostParseProcessing = true
            };

            Assert.That(() => parser.Parse(), Throws.TypeOf<ParserException>().With.Message.Contains("ES0008"));
            Assert.AreEqual(1, parser.errors.count);
        }

        [Test]
        public void UntypedCatchIdentifier()
        {
            var parser = new Parser(new Scanner("../../sources/for_unit_tests/erroneous/untyped_catch_identifier.exs")){
                DoPostParseProcessing = true
            };

            Assert.That(() => parser.Parse(), Throws.TypeOf<ParserException>().With.Message.Contains("ES0011"));
            Assert.AreEqual(1, parser.errors.count);
        }

        [Test]
        public void NoContextLet()
        {
            var parser = new Parser(new Scanner("../../sources/for_unit_tests/erroneous/no_context_let.exs")){
                DoPostParseProcessing = true
            };

            Assert.That(() => parser.Parse(), Throws.TypeOf<ParserException>().With.Message.Contains("ES0003"));
            Assert.AreEqual(1, parser.errors.count);
        }

        [Test]
        public void UnbalancedRawString()
        {
            var parser = new Parser(new Scanner("../../sources/for_unit_tests/erroneous/unbalanced_raw_string.exs")){
                DoPostParseProcessing = true
            };

            Assert.That(() => parser.Parse(), Throws.TypeOf<ParserException>().With.Message.Contains("ES0005"));
            Assert.AreEqual(1, parser.errors.count);
        }

        [Test]
        public void MissingMainFunction()
        {
            var parser = new Parser(new Scanner("../../sources/for_unit_tests/erroneous/missing_main_function.exs")){
                DoPostParseProcessing = true
            };

            Assert.That(() => parser.Parse(), Throws.TypeOf<ParserException>().With.Message.Contains("ES4020"));
            Assert.AreEqual(1, parser.errors.count);
        }

        [Test]
        public void UseBeforeDeclared()
        {
            var parser = new Parser(new Scanner("../../sources/for_unit_tests/erroneous/use_before_declared.exs")){
                DoPostParseProcessing = true
            };

            Assert.That(() => parser.Parse(), Throws.TypeOf<ParserException>().With.Message.Contains("ES0120"));
            Assert.AreEqual(1, parser.errors.count);
        }

        [Test]
        public void ImportUndefinedTypeFromSystem()
        {
            var parser = new Parser(new Scanner("../../sources/for_unit_tests/erroneous/import_undefined_type_from_system.exs")){
                DoPostParseProcessing = true
            };

            Assert.That(() => parser.Parse(), Throws.TypeOf<ParserException>().With.Message.Contains("ES5000"));
            Assert.AreEqual(1, parser.errors.count);
        }

        [Test]
        public void ShadowModuleVariableWithLocalBinding()
        {
            var parser = new Parser(new Scanner("../../sources/for_unit_tests/erroneous/shadow_module_variable_with_local_binding.exs")){
                DoPostParseProcessing = true
            };

            Assert.That(() => parser.Parse(), Throws.TypeOf<ParserException>().With.Message.Contains("ES3100"));
            Assert.AreEqual(1, parser.errors.count);
        }

        [Test]
        public void ShadowModuleVariableWithForBinding()
        {
            var parser = new Parser(new Scanner("../../sources/for_unit_tests/erroneous/shadow_module_variable_with_for_binding.exs")){
                DoPostParseProcessing = true
            };

            Assert.That(() => parser.Parse(), Throws.TypeOf<ParserException>().With.Message.Contains("ES3100"));
            Assert.AreEqual(1, parser.errors.count);
        }

        [Test]
        public void StringInterpolationError()
        {
            var parser = new Parser(new Scanner("../../sources/for_unit_tests/erroneous/string_interpolation_error.exs")){
                DoPostParseProcessing = true
            };

            Assert.That(() => parser.Parse(), Throws.TypeOf<ParserException>().With.Message.Contains("ES0102"));
            Assert.AreEqual(1, parser.errors.count);
        }

        [Test]
        public void MissingClosureParameterTypes()
        {
            var parser = new Parser(new Scanner("../../sources/for_unit_tests/erroneous/missing_closure_parameter_types.exs")){
                DoPostParseProcessing = true
            };

            Assert.That(() => parser.Parse(), Throws.TypeOf<ParserException>().With.Message.Contains("ES3010"));
            Assert.AreEqual(1, parser.errors.count);
        }

        [Test]
        public void ObjectCreationOnAttribute()
        {
            var parser = new Parser(new Scanner("../../sources/for_unit_tests/erroneous/object_creation_on_attribute.exs")){
                DoPostParseProcessing = true
            };

            Assert.That(() => parser.Parse(), Throws.TypeOf<ParserException>().With.Message.Contains("ES4022"));
            Assert.AreEqual(1, parser.errors.count);
        }

        [Test]
        public void DuplicateRawValueEnumMember()
        {
            var parser = new Parser(new Scanner("../../sources/for_unit_tests/erroneous/duplicate_raw_value_enum_member.exs")){
                DoPostParseProcessing = true
            };

            Assert.That(() => parser.Parse(), Throws.TypeOf<ParserException>().With.Message.Contains("ES4030"));
            Assert.AreEqual(1, parser.errors.count);
        }

        [Test]
        public void MultipleStyleEnum()
        {
            var parser = new Parser(new Scanner("../../sources/for_unit_tests/erroneous/multiple_style_enum.exs")){
                DoPostParseProcessing = true
            };

            Assert.That(() => parser.Parse(), Throws.TypeOf<ParserException>().With.Message.Contains("ES4031"));
            Assert.AreEqual(1, parser.errors.count);
        }

        [Test]
        public void AssignmentOnParameter()
        {
            var parser = new Parser(new Scanner("../../sources/for_unit_tests/erroneous/assignment_on_parameter.exs")){
                DoPostParseProcessing = true
            };

            Assert.That(() => parser.Parse(), Throws.TypeOf<ParserException>().With.Message.Contains("ES1903"));
            Assert.AreEqual(1, parser.errors.count);
        }

        [Test]
        public void MultipleItemsOnAugmentedAssignment()
        {
            var parser = new Parser(new Scanner("../../sources/for_unit_tests/erroneous/multiple_items_on_augmented_assignment.exs")){
                DoPostParseProcessing = true
            };

            Assert.That(() => parser.Parse(), Throws.TypeOf<ParserException>().With.Message.Contains("ES2101"));
            Assert.AreEqual(1, parser.errors.count);
        }
    }
}

