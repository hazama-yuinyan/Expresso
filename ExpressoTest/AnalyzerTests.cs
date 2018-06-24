using System;
using NUnit.Framework;
using Expresso.Ast;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using ICSharpCode.NRefactory;

namespace Expresso.Test
{
    [TestFixture]
    public class AnalyzerTests
    {
        [Test]
        public void SimpleLiterals()
        {
            var parser = new Parser(new Scanner("../../sources/for_unit_tests/simple_literals.exs"));
            parser.DoPostParseProcessing = true;
            parser.Parse();

            var ast = parser.TopmostAst;

            var expected_ast = AstNode.MakeModuleDef(
                "main",
                Helpers.MakeSeq(
                    EntityDeclaration.MakeFunc(
                        "main",
                        Enumerable.Empty<ParameterDeclaration>(),
                        Statement.MakeBlock(
                            Statement.MakeVarDecl(
                                Helpers.MakeSeq(Helpers.MakePaticularPatternWithType("a", Helpers.MakePrimitiveType("int"))),
                                Helpers.MakeSeq(Expression.MakeConstant("int", 255)),
                                Modifiers.Immutable
                            ),
                            Statement.MakeVarDecl(
                                Helpers.MakeSeq(Helpers.MakePaticularPatternWithType("h_a", Helpers.MakePrimitiveType("int"))),
                                Helpers.MakeSeq(Expression.MakeConstant("int", 0xff)),
                                Modifiers.Immutable
                            ),
                            Statement.MakeVarDecl(
                                Helpers.MakeSeq(
                                    Helpers.MakeExactPatternWithType(
                                        "h_a_",
                                        Helpers.MakePrimitiveType("int")
                                    )
                                ),
                                Helpers.MakeSeq(Expression.MakeConstant("int", 0xff)),
                                Modifiers.Immutable
                            ),
                            Statement.MakeVarDecl(
                                Helpers.MakeSeq(Helpers.MakePaticularPatternWithType("b", Helpers.MakePrimitiveType("double"))),
                                Helpers.MakeSeq(Expression.MakeConstant("double", 1000.0)),
                                Modifiers.Immutable
                            ),
                            Statement.MakeVarDecl(
                                Helpers.MakeSeq(Helpers.MakePaticularPatternWithType("f_b", Helpers.MakePrimitiveType("double"))),
                                Helpers.MakeSeq(Expression.MakeConstant("double", 1.0e4)),
                                Modifiers.Immutable
                            ),
                            Statement.MakeVarDecl(
                                Helpers.MakeSeq(
                                    Helpers.MakeExactPatternWithType(
                                        "f_b_",
                                        Helpers.MakePrimitiveType("double")
                                    )
                                ),
                                Helpers.MakeSeq(Expression.MakeConstant("double", 1.0e4)),
                                Modifiers.Immutable
                            ),
                            Statement.MakeVarDecl(
                                Helpers.MakeSeq(Helpers.MakePaticularPatternWithType("c", Helpers.MakePrimitiveType("double"))),
                                Helpers.MakeSeq(Expression.MakeConstant("double", 0.001)),
                                Modifiers.Immutable
                            ),
                            Statement.MakeVarDecl(
                                Helpers.MakeSeq(Helpers.MakePaticularPatternWithType("f_c", Helpers.MakePrimitiveType("double"))),
                                Helpers.MakeSeq(Expression.MakeConstant("double", .1e-2)),
                                Modifiers.Immutable
                            ),
                            Statement.MakeVarDecl(
                                Helpers.MakeSeq(Helpers.MakePaticularPatternWithType("f_c2", Helpers.MakePrimitiveType("double"))),
                                Helpers.MakeSeq(Expression.MakeConstant("double", .0001)),
                                Modifiers.Immutable
                            ),
                            Statement.MakeVarDecl(
                                Helpers.MakeSeq(Helpers.MakePaticularPatternWithType("d", Helpers.MakePrimitiveType("bigint"))),
                                Helpers.MakeSeq(Expression.MakeConstant("bigint", BigInteger.Parse("10000000"))),
                                Modifiers.Immutable
                            ),
                            Statement.MakeVarDecl(
                                Helpers.MakeSeq(
                                    Helpers.MakeExactPatternWithType(
                                        "d_",
                                        Helpers.MakePrimitiveType("bigint")
                                    )
                                ),
                                Helpers.MakeSeq(Expression.MakeConstant("bigint", BigInteger.Parse("10000000"))),
                                Modifiers.Immutable
                            ),
                            Statement.MakeVarDecl(
                                Helpers.MakeSeq(Helpers.MakePaticularPatternWithType("d2", Helpers.MakePrimitiveType("bigint"))),
                                Helpers.MakeSeq(Expression.MakeConstant("bigint", BigInteger.Parse("10000000"))),
                                Modifiers.Immutable
                            ),
                            Statement.MakeVarDecl(
                                Helpers.MakeSeq(Helpers.MakePaticularPatternWithType("e", Helpers.MakePrimitiveType("string"))),
								Helpers.MakeSeq(Expression.MakeConstant("string", "This is a test")),
                                Modifiers.Immutable
                            ),
                            Statement.MakeVarDecl(
                                Helpers.MakeSeq(Helpers.MakePaticularPatternWithType("u", Helpers.MakePrimitiveType("uint"))),
                                Helpers.MakeSeq(Expression.MakeConstant("uint", 1000u)),
                                Modifiers.Immutable
                            ),
                            Statement.MakeVarDecl(
                                Helpers.MakeSeq(Helpers.MakePaticularPatternWithType("u_", Helpers.MakePrimitiveType("uint"))),
                                Helpers.MakeSeq(Expression.MakeConstant("uint", 1000U)),
                                Modifiers.Immutable
                            ),
                            Statement.MakeVarDecl(
                                Helpers.MakeSeq(Helpers.MakePaticularPatternWithType("f_a", Helpers.MakePrimitiveType("float"))),
                                Helpers.MakeSeq(Expression.MakeConstant("float", 1.0e4f)),
                                Modifiers.Immutable
                            ),
                            Statement.MakeVarDecl(
                                Helpers.MakeSeq(
                                    Helpers.MakeExactPatternWithType(
                                        "f_a_",
                                        Helpers.MakePrimitiveType("float")
                                    )
                                ),
                                Helpers.MakeSeq(Expression.MakeConstant("float", 1.0e4f)),
                                Modifiers.Immutable
                            ),
                            Statement.MakeVarDecl(
                                Helpers.MakeSeq(
                                    Helpers.MakeExactPatternWithType(
                                        "f",
                                        Helpers.MakeGenericType(
                                            "array",
                                            Helpers.MakePrimitiveType("int")
                                        )
                                    )
                                ),
                                Helpers.MakeSeq(
                                    Expression.MakeSequenceInitializer(
                                        Helpers.MakeGenericType(
                                            "array",
                                            Helpers.MakePrimitiveType("int")
                                        ),
                                        Enumerable.Empty<Expression>()
                                    )
                                ),
                                Modifiers.Immutable
                            ),
                            Statement.MakeVarDecl(
                                Helpers.MakeSeq(Helpers.MakePaticularPatternWithType("f_", Helpers.MakeGenericType("array", Helpers.MakePrimitiveType("int")))),
                                Helpers.MakeSeq(
                                    Expression.MakeSequenceInitializer(
                                        Helpers.MakeGenericType(
                                            "array",
                                            Helpers.MakePrimitiveType("int")
                                        ),
                                        Helpers.MakeSeq(
                                            Expression.MakeConstant("int", 1),
                                            Expression.MakeConstant("int", 2),
                                            Expression.MakeConstant("int", 3)
                                        )
                                    )
                                ),
                                Modifiers.Immutable
                            ),
                            Statement.MakeVarDecl(
                                Helpers.MakeSeq(
                                    Helpers.MakeExactPatternWithType(
                                        "f2",
                                        Helpers.MakeGenericType(
                                            "vector",
                                            Helpers.MakePrimitiveType("int")
                                        )
                                    )
                                ),
                                Helpers.MakeSeq(
                                    Expression.MakeSequenceInitializer(
                                        Helpers.MakeGenericType(
                                            "vector",
                                            Helpers.MakePrimitiveType("int")
                                        ),
                                        Enumerable.Empty<Expression>()
                                    )
                                ),
                                Modifiers.Immutable
                            ),
                            Statement.MakeVarDecl(
                                Helpers.MakeSeq(
                                    Helpers.MakePaticularPatternWithType(
                                        "f2_",
                                        Helpers.MakeGenericType(
                                            "vector",
                                            Helpers.MakePrimitiveType("int")
                                        )
                                    )
                                ),
                                Helpers.MakeSeq(
                                    Expression.MakeSequenceInitializer(
                                        Helpers.MakeGenericType(
                                            "vector",
                                            Helpers.MakePrimitiveType("int")
                                        ),
                                        Helpers.MakeSeq(
                                            Expression.MakeConstant("int", 1),
                                            Expression.MakeConstant("int", 2),
                                            Expression.MakeConstant("int", 3)
                                        )
                                    )
                                ),
                                Modifiers.Immutable
                            ),
                            /*Statement.MakeVarDecl(
                             * Helpers.MakeSeq(PatternConstruct.MakeIdentifierPattern("f3", Helpers.MakeGenericType("tuple"))),
                             * Helpers.MakeSeq(Expression.MakeParen(Expression.MakeSequenceExpression(null))),
                             * Modifiers.Immutable
                             * ),*/
                            Statement.MakeVarDecl(
                                Helpers.MakeSeq(
                                    Helpers.MakePaticularPatternWithType(
                                        "f3_",
                                        Helpers.MakeGenericType(
                                            "tuple",
                                            Helpers.MakePrimitiveType("int"),
                                            Helpers.MakePrimitiveType("string"),
                                            Helpers.MakePrimitiveType("bool")
                                        )
                                    )
                                ),
                                Helpers.MakeSeq(
                                    Expression.MakeParen(
                                        Expression.MakeSequenceExpression(
                                            Expression.MakeConstant("int", 1),
                                            Expression.MakeConstant("string", "abc"),
                                            Expression.MakeConstant("bool", true)

                                        )
                                    )
                                ),
                                Modifiers.Immutable
                            ),
                            Statement.MakeVarDecl(
                                Helpers.MakeSeq(
                                    Helpers.MakePaticularTuplePatternWithType(
                                        Helpers.MakeSeq(
                                            "f3_a",
                                            "f3_b",
                                            "f3_c"
                                        ),
                                        Helpers.MakePrimitiveType("int"),
                                        Helpers.MakePrimitiveType("string"),
                                        Helpers.MakePrimitiveType("bool")
                                    )
                                ),
                                Helpers.MakeSeq(
                                    Helpers.MakeIdentifierPath(
                                        "f3_",
                                        Helpers.MakeGenericType(
                                            "tuple",
                                            Helpers.MakePrimitiveType("int"),
                                            Helpers.MakePrimitiveType("string"),
                                            Helpers.MakePrimitiveType("bool")
                                        )
                                    )
                                ),
                                Modifiers.Immutable
                            ),
                            Statement.MakeVarDecl(
                                Helpers.MakeSeq(
                                    Helpers.MakeExactPatternWithType(
                                        "g",
                                        Helpers.MakeGenericType(
                                            "dictionary",
                                            Helpers.MakePrimitiveType("string"),
                                            Helpers.MakePrimitiveType("int")
                                        )
                                    )
                                ),
                                Helpers.MakeSeq(
                                    Expression.MakeSequenceInitializer(
                                        Helpers.MakeGenericType(
                                            "dictionary",
                                            Helpers.MakePrimitiveType("string"),
                                            Helpers.MakePrimitiveType("int")
                                        ),
                                        Enumerable.Empty<Expression>()
                                    )
                                ),
                                Modifiers.Immutable
                            ),
                            Statement.MakeVarDecl(
                                Helpers.MakeSeq(
                                    Helpers.MakePaticularPatternWithType(
                                        "g_",
                                        Helpers.MakeGenericType(
                                            "dictionary",
                                            Helpers.MakePrimitiveType("string"),
                                            Helpers.MakePrimitiveType("int")
                                        )
                                    )
                                ),
                                Helpers.MakeSeq(
                                    Expression.MakeSequenceInitializer(
                                        Helpers.MakeGenericType(
                                            "dictionary",
                                            Helpers.MakePrimitiveType("string"),
                                            Helpers.MakePrimitiveType("int")
                                        ),
                                        Helpers.MakeSeq(
                                            Expression.MakeKeyValuePair(Expression.MakeConstant("string", "akari"), Expression.MakeConstant("int", 13)),
                                            Expression.MakeKeyValuePair(Expression.MakeConstant("string", "chinatsu"), Expression.MakeConstant("int", 13)),
                                            Expression.MakeKeyValuePair(Expression.MakeConstant("string", "京子"), Expression.MakeConstant("int", 14)),
                                            Expression.MakeKeyValuePair(Expression.MakeConstant("string", "結衣"), Expression.MakeConstant("int", 14))
                                        )
                                    )
                                ),
                                Modifiers.Immutable
                            ),
                            Statement.MakeVarDecl(
                                Helpers.MakeSeq(Helpers.MakePaticularPatternWithType("h", Helpers.MakePrimitiveType("string"))),
                                Helpers.MakeSeq(Expression.MakeConstant("string", "私変わっちゃうの・・？")),
                                Modifiers.None
                            ),
                            Statement.MakeVarDecl(
                                Helpers.MakeSeq(Helpers.MakePaticularPatternWithType("h2", Helpers.MakePrimitiveType("string"))),
                                Helpers.MakeSeq(Expression.MakeConstant("string", "Yes, you can!")),
                                Modifiers.None
                            ),
                            Statement.MakeVarDecl(
                                Helpers.MakeSeq(Helpers.MakePaticularPatternWithType("h3", Helpers.MakePrimitiveType("char"))),
                                Helpers.MakeSeq(Expression.MakeConstant("char", 'a')),
                                Modifiers.None
                            ),
                            Statement.MakeVarDecl(
                                Helpers.MakeSeq(Helpers.MakePaticularPatternWithType("i", Helpers.MakePrimitiveType("string"))),
                                Helpers.MakeSeq(Expression.MakeConstant("string", "よかった。私変わらないんだね！・・え、変われないの？・・・なんだかフクザツ")),
                                Modifiers.Immutable
                            ),
                            Statement.MakeVarDecl(
                                Helpers.MakeSeq(Helpers.MakePaticularPatternWithType("i2", Helpers.MakePrimitiveType("string"))),
                                Helpers.MakeSeq(Expression.MakeConstant("string", "Oh, you just can't...")),
                                Modifiers.Immutable
                            ),
                            Statement.MakeVarDecl(
                                Helpers.MakeSeq(Helpers.MakePaticularPatternWithType("i3", Helpers.MakePrimitiveType("char"))),
                                Helpers.MakeSeq(Expression.MakeConstant("char", 'a')),
                                Modifiers.Immutable
                            ),
                            Statement.MakeVarDecl(
                                Helpers.MakeSeq(Helpers.MakePaticularPatternWithType("i4", Helpers.MakePrimitiveType("string"))),
                                Helpers.MakeSeq(Expression.MakeConstant("string", "\u0041\u005a\u0061\u007A\u3042\u30A2")), //AZazあア
                                Modifiers.Immutable
                            ),
                            Statement.MakeVarDecl(
                                Helpers.MakeSeq(Helpers.MakePaticularPatternWithType("i5", Helpers.MakePrimitiveType("char"))),
                                Helpers.MakeSeq(Expression.MakeConstant("char", '\u0041')), //A
                                Modifiers.Immutable
                            ),
                            Statement.MakeVarDecl(
                                Helpers.MakeSeq(Helpers.MakePaticularPatternWithType("i6", Helpers.MakePrimitiveType("string"))),
                                Helpers.MakeSeq(Expression.MakeConstant("string", "This is a normal string.\n Seems 2 lines? Yes, you're right!")),
                                Modifiers.Immutable
                            ),
                            Statement.MakeVarDecl(
                                Helpers.MakeSeq(Helpers.MakePaticularPatternWithType("i6_", Helpers.MakePrimitiveType("string"))),
                                Helpers.MakeSeq(Expression.MakeConstant("string", @"This is a raw string.\n Seems 2 lines? Nah, indeed.")),
                                Modifiers.Immutable
                            ),
                            Statement.MakeVarDecl(
                                Helpers.MakeSeq(Helpers.MakePaticularPatternWithType("j", Helpers.MakePrimitiveType("intseq"))),
                                Helpers.MakeSeq(
                                    Expression.MakeIntSeq(
                                        Expression.MakeConstant("int", 1),
                                        Expression.MakeConstant("int", 10),
                                        Expression.MakeConstant("int", 1),
                                        false
                                    )
                                ),
                                Modifiers.Immutable
                            ),
                            Statement.MakeVarDecl(
                                Helpers.MakeSeq(Helpers.MakePaticularPatternWithType("j2", Helpers.MakePrimitiveType("intseq"))),
                                Helpers.MakeSeq(
                                    Expression.MakeIntSeq(
                                        Expression.MakeConstant("int", 1),
                                        Expression.MakeConstant("int", 10),
                                        Expression.MakeConstant("int", 1),
                                        false
                                    )
                                ),
                                Modifiers.Immutable
                            ),
                            Statement.MakeVarDecl(
                                Helpers.MakeSeq(
                                    Helpers.MakeExactPatternWithType(
                                        "j3",
                                        Helpers.MakePrimitiveType("intseq")
                                    )
                                ),
                                Helpers.MakeSeq(
                                    Expression.MakeIntSeq(
                                        Expression.MakeConstant("int", 1),
                                        Expression.MakeConstant("int", 10),
                                        Expression.MakeConstant("int", 1),
                                        false
                                    )
                                ),
                                Modifiers.Immutable
                            ),
                            Statement.MakeVarDecl(
                                Helpers.MakeSeq(Helpers.MakePaticularPatternWithType("j_2", Helpers.MakePrimitiveType("intseq"))),
                                Helpers.MakeSeq(
                                    Expression.MakeIntSeq(
                                        Expression.MakeUnaryExpr(OperatorType.Minus, Expression.MakeConstant("int", 5)),
                                        Expression.MakeUnaryExpr(OperatorType.Minus, Expression.MakeConstant("int", 10)),
                                        Expression.MakeUnaryExpr(OperatorType.Minus, Expression.MakeConstant("int", 1)),
                                        false
                                    )
                                ),
                                Modifiers.Immutable
                            ),
                            Statement.MakeVarDecl(
                                Helpers.MakeSeq(Helpers.MakePaticularPatternWithType("j_2_", Helpers.MakePrimitiveType("intseq"))),
                                Helpers.MakeSeq(
                                    Expression.MakeIntSeq(
                                        Expression.MakeConstant("int", 0),
                                        Expression.MakeUnaryExpr(OperatorType.Minus, Expression.MakeConstant("int", 10)),
                                        Expression.MakeUnaryExpr(OperatorType.Minus, Expression.MakeConstant("int", 1)),
                                        true
                                    )
                                ),
                                Modifiers.Immutable
                            ),
                            Statement.MakeExprStmt(
                                Helpers.MakeCallExpression(
                                    Helpers.SomeWellKnownExpressions.Println,
                                    Helpers.MakeCallExpression(
                                        Helpers.SomeWellKnownExpressions.StringFormatN,
                                        Expression.MakeConstant("string", "{0}, {1}, {2}, {3}, {4}, {5}, {6}, {7}, {8}, {9}, {10}, {11}, {12}, {13}, {14}, {15}, {16}, {17}, {18}, {19}, {20}"),
                                        Helpers.MakeIdentifierPath(
                                            "a",
                                            Helpers.MakePrimitiveType("int")
                                        ),
                                        Helpers.MakeIdentifierPath(
                                            "h_a",
                                            Helpers.MakePrimitiveType("int")
                                        ),
                                        Helpers.MakeIdentifierPath(
                                            "h_a_",
                                            Helpers.MakePrimitiveType("int")
                                        ),
                                        Helpers.MakeIdentifierPath(
                                            "b",
                                            Helpers.MakePrimitiveType("double")
                                        ),
                                        Helpers.MakeIdentifierPath(
                                            "f_b",
                                            Helpers.MakePrimitiveType("double")
                                        ),
                                        Helpers.MakeIdentifierPath(
                                            "f_b_",
                                            Helpers.MakePrimitiveType("double")
                                        ),
                                        Helpers.MakeIdentifierPath(
                                            "c",
                                            Helpers.MakePrimitiveType("double")
                                        ),
                                        Helpers.MakeIdentifierPath(
                                            "f_c",
                                            Helpers.MakePrimitiveType("double")
                                        ),
                                        Helpers.MakeIdentifierPath(
                                            "f_c2",
                                            Helpers.MakePrimitiveType("double")
                                        ),
                                        Helpers.MakeIdentifierPath(
                                            "d",
                                            Helpers.MakePrimitiveType("bigint")
                                        ),
                                        Helpers.MakeIdentifierPath(
                                            "d_",
                                            Helpers.MakePrimitiveType("bigint")
                                        ),
                                        Helpers.MakeIdentifierPath(
                                            "d2",
                                            Helpers.MakePrimitiveType("bigint")
                                        ),
                                        Helpers.MakeIdentifierPath(
                                            "e",
                                            Helpers.MakePrimitiveType("string")
                                        ),
                                        Helpers.MakeIdentifierPath(
                                            "u",
                                            Helpers.MakePrimitiveType("uint")
                                        ),
                                        Helpers.MakeIdentifierPath(
                                            "u_",
                                            Helpers.MakePrimitiveType("uint")
                                        ),
                                        Helpers.MakeIdentifierPath(
                                            "f_a",
                                            Helpers.MakePrimitiveType("float")
                                        ),
                                        Helpers.MakeIdentifierPath(
                                            "f_a_",
                                            Helpers.MakePrimitiveType("float")
                                        ),
                                        Helpers.MakeIdentifierPath(
                                            "f",
                                            Helpers.MakeGenericType(
                                                "array",
                                                Helpers.MakePrimitiveType("int")
                                            )
                                        ),
                                        Helpers.MakeIdentifierPath(
                                            "f_",
                                            Helpers.MakeGenericType(
                                                "array",
                                                Helpers.MakePrimitiveType("int")
                                            )
                                        ),
                                        Helpers.MakeIdentifierPath(
                                            "f2",
                                            Helpers.MakeGenericType(
                                                "vector",
                                                Helpers.MakePrimitiveType("int")
                                            )
                                        ),
                                        Helpers.MakeIdentifierPath(
                                            "f2_",
                                            Helpers.MakeGenericType(
                                                "vector",
                                                Helpers.MakePrimitiveType("int")
                                            )
                                        )
                                    )
                                )
                            ),
                            Statement.MakeExprStmt(
                                Helpers.MakeCallExpression(
                                    Helpers.SomeWellKnownExpressions.Println,
                                    Helpers.MakeCallExpression(
                                        Helpers.SomeWellKnownExpressions.StringFormatN,
                                        Expression.MakeConstant("string", "{0}, {1}, {2}, {3}, {4}, {5}, {6}, {7}, {8}, {9}, {10}, {11}, {12}, {13}, {14}, {15}, {16}, {17}, {18}, {19}, {20}"),
                                        Helpers.MakeIdentifierPath(
                                            "f3_",
                                            Helpers.MakeGenericType(
                                                "tuple",
                                                Helpers.MakePrimitiveType("int"),
                                                Helpers.MakePrimitiveType("string"),
                                                Helpers.MakePrimitiveType("bool")
                                            )
                                        ),
                                        Helpers.MakeIdentifierPath(
                                            "f3_a",
                                            Helpers.MakePrimitiveType("int")
                                        ),
                                        Helpers.MakeIdentifierPath(
                                            "f3_b",
                                            Helpers.MakePrimitiveType("string")
                                        ),
                                        Helpers.MakeIdentifierPath(
                                            "f3_c",
                                            Helpers.MakePrimitiveType("bool")
                                        ),
                                        Helpers.MakeIdentifierPath(
                                            "g",
                                            Helpers.MakeGenericType(
                                                "dictionary",
                                                Helpers.MakePrimitiveType("string"),
                                                Helpers.MakePrimitiveType("int")
                                            )
                                        ),
                                        Helpers.MakeIdentifierPath(
                                            "g_",
                                            Helpers.MakeGenericType(
                                                "dictionary",
                                                Helpers.MakePrimitiveType("string"),
                                                Helpers.MakePrimitiveType("int")
                                            )
                                        ),
                                        Helpers.MakeIdentifierPath(
                                            "h",
                                            Helpers.MakePrimitiveType("string")
                                        ),
                                        Helpers.MakeIdentifierPath(
                                            "h2",
                                            Helpers.MakePrimitiveType("string")
                                        ),
                                        Helpers.MakeIdentifierPath(
                                            "h3",
                                            Helpers.MakePrimitiveType("char")
                                        ),
                                        Helpers.MakeIdentifierPath(
                                            "i",
                                            Helpers.MakePrimitiveType("string")
                                        ),
                                        Helpers.MakeIdentifierPath(
                                            "i2",
                                            Helpers.MakePrimitiveType("string")
                                        ),
                                        Helpers.MakeIdentifierPath(
                                            "i3",
                                            Helpers.MakePrimitiveType("char")
                                        ),
                                        Helpers.MakeIdentifierPath(
                                            "i4",
                                            Helpers.MakePrimitiveType("string")
                                        ),
                                        Helpers.MakeIdentifierPath(
                                            "i5",
                                            Helpers.MakePrimitiveType("char")
                                        ),
                                        Helpers.MakeIdentifierPath(
                                            "i6",
                                            Helpers.MakePrimitiveType("string")
                                        ),
                                        Helpers.MakeIdentifierPath(
                                            "i6_",
                                            Helpers.MakePrimitiveType("string")
                                        ),
                                        Helpers.MakeIdentifierPath(
                                            "j",
                                            Helpers.MakePrimitiveType("intseq")
                                        ),
                                        Helpers.MakeIdentifierPath(
                                            "j2",
                                            Helpers.MakePrimitiveType("intseq")
                                        ),
                                        Helpers.MakeIdentifierPath(
                                            "j3",
                                            Helpers.MakePrimitiveType("intseq")
                                        ),
                                        Helpers.MakeIdentifierPath(
                                            "j_2",
                                            Helpers.MakePrimitiveType("intseq")
                                        ),
                                        Helpers.MakeIdentifierPath(
                                            "j_2_",
                                            Helpers.MakePrimitiveType("intseq")
                                        )
                                    )
                                )
                            )
                        ),
                        Helpers.MakeVoidType(),
                        Modifiers.None
                    )
                )
            );

            Assert.IsNotNull(ast);
            Helpers.AstStructuralEqual(ast, expected_ast);
        }

        [Test]
        public void SimpleArithmetic()
        {
            var parser = new Parser(new Scanner("../../sources/for_unit_tests/simple_arithmetic.exs"));
            parser.DoPostParseProcessing = true;
            parser.Parse();

            var ast = parser.TopmostAst;

            var expected_ast = AstNode.MakeModuleDef("main", new List<EntityDeclaration>{
                EntityDeclaration.MakeFunc(
                    "main",
                    Helpers.MakeSeq(
                        EntityDeclaration.MakeParameter("args", Helpers.MakeGenericType("array", Helpers.MakePrimitiveType("string")))
                    ),
                    Statement.MakeBlock(
                        Statement.MakeVarDecl(
                            Helpers.MakeSeq(
                                Helpers.MakePaticularPatternWithType("x", Helpers.MakePrimitiveType("int")),
                                Helpers.MakePaticularPatternWithType("xp", Helpers.MakePrimitiveType("int")),
                                Helpers.MakePaticularPatternWithType("xm", Helpers.MakePrimitiveType("int")),
                                Helpers.MakePaticularPatternWithType("xt", Helpers.MakePrimitiveType("int")),
                                Helpers.MakePaticularPatternWithType("xd", Helpers.MakePrimitiveType("int")),
                                Helpers.MakePaticularPatternWithType("xmod", Helpers.MakePrimitiveType("int")),
                                Helpers.MakePaticularPatternWithType("xpower", Helpers.MakePrimitiveType("int"))
                            ),
                            Helpers.MakeSeq(
                                Expression.MakeConstant("int", 3),
                                Expression.MakeConstant("int", 3),
                                Expression.MakeConstant("int", 3),
                                Expression.MakeConstant("int", 3),
                                Expression.MakeConstant("int", 4),
                                Expression.MakeConstant("int", 4),
                                Expression.MakeConstant("int", 3)
                            ),
                            Modifiers.None
                        ),
                        Statement.MakeVarDecl(
                            Helpers.MakeSeq(
                                Helpers.MakePaticularPatternWithType("a", Helpers.MakePrimitiveType("int"))
                            ),
                            Helpers.MakeSeq(
                                Expression.MakeBinaryExpr(
                                    OperatorType.Plus,
                                    Helpers.MakeIdentifierPath("x", Helpers.MakePrimitiveType("int")),
                                    Expression.MakeConstant("int", 4)
                                )
                            ),
                            Modifiers.Immutable
                        ),
                        Statement.MakeVarDecl(
                            Helpers.MakeSeq(
                                Helpers.MakePaticularPatternWithType("b", Helpers.MakePrimitiveType("int"))
                            ),
                            Helpers.MakeSeq(
                                Expression.MakeBinaryExpr(
                                    OperatorType.Minus,
                                    Helpers.MakeIdentifierPath("x", Helpers.MakePrimitiveType("int")),
                                    Expression.MakeConstant("int", 4)
                                )
                            ),
                            Modifiers.Immutable
                        ),
                        Statement.MakeVarDecl(
                            Helpers.MakeSeq(
                                Helpers.MakePaticularPatternWithType("c", Helpers.MakePrimitiveType("int"))
                            ),
                            Helpers.MakeSeq(
                                Expression.MakeBinaryExpr(
                                    OperatorType.Times,
                                    Helpers.MakeIdentifierPath("x", Helpers.MakePrimitiveType("int")),
                                    Expression.MakeConstant("int", 4)
                                )
                            ),
                            Modifiers.Immutable
                        ),
                        Statement.MakeVarDecl(
                            Helpers.MakeSeq(
                                Helpers.MakePaticularPatternWithType("d", Helpers.MakePrimitiveType("int"))
                            ),
                            Helpers.MakeSeq(
                                Expression.MakeBinaryExpr(
                                    OperatorType.Divide,
                                    Expression.MakeConstant("int", 4),
                                    Expression.MakeConstant("int", 2)
                                )
                            ),
                            Modifiers.Immutable
                        ),
                        Statement.MakeVarDecl(
                            Helpers.MakeSeq(
                                Helpers.MakePaticularPatternWithType("e", Helpers.MakePrimitiveType("int"))
                            ),
                            Helpers.MakeSeq(
                                Expression.MakeBinaryExpr(
                                    OperatorType.Modulus,
                                    Expression.MakeConstant("int", 4),
                                    Expression.MakeConstant("int", 2)
                                )
                            ),
                            Modifiers.Immutable
                        ),
                        Statement.MakeVarDecl(
                            Helpers.MakeSeq(
                                Helpers.MakePaticularPatternWithType("f", Helpers.MakePrimitiveType("int"))
                            ),
                            Helpers.MakeSeq(
                                Expression.MakeBinaryExpr(
                                    OperatorType.Power,
                                    Helpers.MakeIdentifierPath("x", Helpers.MakePrimitiveType("int")),
                                    Expression.MakeConstant("int", 2)
                                )
                            ),
                            Modifiers.Immutable
                        ),
                        Statement.MakeExprStmt(
                            Expression.MakeSingleAugmentedAssignment(
                                OperatorType.Plus,
                                Helpers.MakeIdentifierPath("xp", Helpers.MakePrimitiveType("int")),
                                Expression.MakeConstant("int", 4)
                            )
                        ),
                        Statement.MakeExprStmt(
                            Expression.MakeSingleAugmentedAssignment(
                                OperatorType.Minus,
                                Helpers.MakeIdentifierPath("xm", Helpers.MakePrimitiveType("int")),
                                Expression.MakeConstant("int", 4)
                            )
                        ),
                        Statement.MakeExprStmt(
                            Expression.MakeSingleAugmentedAssignment(
                                OperatorType.Times,
                                Helpers.MakeIdentifierPath("xt", Helpers.MakePrimitiveType("int")),
                                Expression.MakeConstant("int", 4)
                            )
                        ),
                        Statement.MakeExprStmt(
                            Expression.MakeSingleAugmentedAssignment(
                                OperatorType.Divide,
                                Helpers.MakeIdentifierPath("xd", Helpers.MakePrimitiveType("int")),
                                Expression.MakeConstant("int", 2)
                            )
                        ),
                        Statement.MakeExprStmt(
                            Expression.MakeSingleAugmentedAssignment(
                                OperatorType.Modulus,
                                Helpers.MakeIdentifierPath("xmod", Helpers.MakePrimitiveType("int")),
                                Expression.MakeConstant("int", 2)
                            )
                        ),
                        Statement.MakeExprStmt(
                            Expression.MakeSingleAugmentedAssignment(
                                OperatorType.Power,
                                Helpers.MakeIdentifierPath("xpower", Helpers.MakePrimitiveType("int")),
                                Expression.MakeConstant("int", 2)
                            )
                        ),
                        Statement.MakeExprStmt(
                            Helpers.MakeCallExpression(
                                Helpers.SomeWellKnownExpressions.Println,
                                Helpers.MakeCallExpression(
                                    Helpers.SomeWellKnownExpressions.StringFormatN,
                                    Expression.MakeConstant("string", "{0}, {1}, {2}, {3}, {4}, {5}, {6}, {7}, {8}, {9}, {10}, {11}, {12}"),
                                    Helpers.MakeIdentifierPath(
                                        "x",
                                        Helpers.MakePrimitiveType("int")
                                    ),
                                    Helpers.MakeIdentifierPath(
                                        "a",
                                        Helpers.MakePrimitiveType("int")
                                    ),
                                    Helpers.MakeIdentifierPath(
                                        "b",
                                        Helpers.MakePrimitiveType("int")
                                    ),
                                    Helpers.MakeIdentifierPath(
                                        "c",
                                        Helpers.MakePrimitiveType("int")
                                    ),
                                    Helpers.MakeIdentifierPath(
                                        "d",
                                        Helpers.MakePrimitiveType("int")
                                    ),
                                    Helpers.MakeIdentifierPath(
                                        "e",
                                        Helpers.MakePrimitiveType("int")
                                    ),
                                    Helpers.MakeIdentifierPath(
                                        "f",
                                        Helpers.MakePrimitiveType("int")
                                    ),
                                    Helpers.MakeIdentifierPath(
                                        "xp",
                                        Helpers.MakePrimitiveType("int")
                                    ),
                                    Helpers.MakeIdentifierPath(
                                        "xm",
                                        Helpers.MakePrimitiveType("int")
                                    ),
                                    Helpers.MakeIdentifierPath(
                                        "xt",
                                        Helpers.MakePrimitiveType("int")
                                    ),
                                    Helpers.MakeIdentifierPath(
                                        "xd",
                                        Helpers.MakePrimitiveType("int")
                                    ),
                                    Helpers.MakeIdentifierPath(
                                        "xmod",
                                        Helpers.MakePrimitiveType("int")
                                    ),
                                    Helpers.MakeIdentifierPath(
                                        "xpower",
                                        Helpers.MakePrimitiveType("int")
                                    )
                                )
                            )
                        )
                    ),
                    Helpers.MakeVoidType(),
                    Modifiers.None
                )
            });

            Assert.IsNotNull(ast);
            Helpers.AstStructuralEqual(ast, expected_ast);
        }

        [Test]
        public void GeneralExpressions()
        {
            var parser = new Parser(new Scanner("../../sources/for_unit_tests/general_expressions.exs"));
            parser.DoPostParseProcessing = true;
            parser.Parse();

            var ast = parser.TopmostAst;

            var expected_ast = AstNode.MakeModuleDef("main", new List<EntityDeclaration>{
                EntityDeclaration.MakeFunc(
                    "main",
                    Enumerable.Empty<ParameterDeclaration>(),
                    Statement.MakeBlock(
                        Statement.MakeVarDecl(
                            Helpers.MakeSeq(Helpers.MakePaticularPatternWithType("ary", Helpers.MakeGenericType("array", Helpers.MakePrimitiveType("int")))),
                            Helpers.MakeSeq(
                                Expression.MakeSequenceInitializer(
                                    Helpers.MakeGenericType("array", Helpers.MakePrimitiveType("int")),
                                    Helpers.MakeSeq(
                                        Expression.MakeConstant("int", 1),
                                        Expression.MakeConstant("int", 2),
                                        Expression.MakeConstant("int", 3)
                                    )
                                )
                            ),
                            Modifiers.Immutable
                        ),
                        Statement.MakeVarDecl(
                            Helpers.MakeSeq(
                                Helpers.MakePaticularPatternWithType(
                                    "d",
                                    Helpers.MakeGenericType(
                                        "dictionary",
                                        Helpers.MakePrimitiveType("string"),
                                        Helpers.MakePrimitiveType("int")
                                    )
                                )
                            ),
                            Helpers.MakeSeq(
                                Expression.MakeSequenceInitializer(
                                    Helpers.MakeGenericType("dictionary", Helpers.MakePrimitiveType("string"), Helpers.MakePrimitiveType("int")),
                                    Helpers.MakeSeq(
                                        Expression.MakeKeyValuePair(Expression.MakeConstant("string", "a"), Expression.MakeConstant("int", 14)),
                                        Expression.MakeKeyValuePair(Expression.MakeConstant("string", "b"), Expression.MakeConstant("int", 13)),
                                        Expression.MakeKeyValuePair(Expression.MakeConstant("string", "何か"), Expression.MakeConstant("int", 100))
                                    )
                                )
                            ),
                            Modifiers.Immutable
                        ),
                        Statement.MakeVarDecl(
                            Helpers.MakeSeq(Helpers.MakePaticularPatternWithType("m", Helpers.MakePrimitiveType("int"))),
                            Helpers.MakeSeq(
                                Helpers.MakeIndexerExpression(
                                    Helpers.MakeIdentifierPath(
                                        "ary",
                                        Helpers.MakeGenericType("array", Helpers.MakePrimitiveType("int"))
                                    ),
                                    Expression.MakeConstant("int", 0)
                                )
                            ),
                            Modifiers.Immutable
                        ),
                        Statement.MakeVarDecl(
                            Helpers.MakeSeq(Helpers.MakePaticularPatternWithType("m2", Helpers.MakePrimitiveType("int"))),
                            Helpers.MakeSeq(
                                Helpers.MakeIndexerExpression(
                                    Helpers.MakeIdentifierPath(
                                        "d",
                                        Helpers.MakeGenericType("dictionary", Helpers.MakePrimitiveType("string"), Helpers.MakePrimitiveType("int"))
                                    ),
                                    Expression.MakeConstant("string", "a")
                                )
                            ),
                            Modifiers.Immutable
                        ),
                        Statement.MakeVarDecl(
                            Helpers.MakeSeq(Helpers.MakePaticularPatternWithType("x", Helpers.MakePrimitiveType("int"))),
                            Helpers.MakeSeq(Expression.MakeConstant("int", 100)),
                            Modifiers.Immutable
                        ),
                        Statement.MakeVarDecl(
                            Helpers.MakeSeq(Helpers.MakePaticularPatternWithType("p", Helpers.MakePrimitiveType("int"))),
                            Helpers.MakeSeq(
                                Expression.MakeBinaryExpr(
                                    OperatorType.Plus,
                                    Helpers.MakeIndexerExpression(
                                        Helpers.MakeIdentifierPath(
                                            "ary",
                                            Helpers.MakeGenericType("array", Helpers.MakePrimitiveType("int"))
                                        ),
                                        Expression.MakeConstant("int", 0)
                                    ),
                                    Expression.MakeBinaryExpr(
                                        OperatorType.Plus,
                                        Helpers.MakeIndexerExpression(
                                            Helpers.MakeIdentifierPath(
                                                "ary",
                                                Helpers.MakeGenericType("array", Helpers.MakePrimitiveType("int"))
                                            ),
                                            Expression.MakeConstant("int", 1)
                                        ),
                                        Expression.MakeIndexer(
                                            Helpers.MakeIdentifierPath(
                                                "ary",
                                                Helpers.MakeGenericType("array", Helpers.MakePrimitiveType("int"))
                                            ),
                                            Helpers.MakeSeq(Expression.MakeConstant("int", 2))
                                        )
                                    )
                                )
                            ),
                            Modifiers.Immutable
                        ),
                        Statement.MakeVarDecl(
                            Helpers.MakeSeq(Helpers.MakePaticularPatternWithType("q", Helpers.MakePrimitiveType("int"))),
                            Helpers.MakeSeq(
                                Expression.MakeBinaryExpr(
                                    OperatorType.Plus,
                                    Helpers.MakeIndexerExpression(
                                        Helpers.MakeIdentifierPath(
                                            "d",
                                            Helpers.MakeGenericType("dictionary", Helpers.MakePrimitiveType("string"), Helpers.MakePrimitiveType("int"))
                                        ),
                                        Expression.MakeConstant("string", "a")
                                    ),
                                    Expression.MakeBinaryExpr(
                                        OperatorType.Plus,
                                        Helpers.MakeIndexerExpression(
                                            Helpers.MakeIdentifierPath(
                                                "d",
                                                Helpers.MakeGenericType("dictionary", Helpers.MakePrimitiveType("string"), Helpers.MakePrimitiveType("int"))
                                            ),
                                            Expression.MakeConstant("string", "b")
                                        ),
                                        Helpers.MakeIndexerExpression(
                                            Helpers.MakeIdentifierPath(
                                                "d",
                                                Helpers.MakeGenericType("dictionary", Helpers.MakePrimitiveType("string"), Helpers.MakePrimitiveType("int"))
                                            ),
                                            Expression.MakeConstant("string", "何か")
                                        )
                                    )
                                )
                            ),
                            Modifiers.Immutable
                        ),
                        Statement.MakeVarDecl(
                            Helpers.MakeSeq(Helpers.MakePaticularPatternWithType("r", Helpers.MakePrimitiveType("int"))),
                            Helpers.MakeSeq(
                                Expression.MakeBinaryExpr(
                                    OperatorType.BitwiseShiftRight,
                                    Helpers.MakeIdentifierPath("x", Helpers.MakePrimitiveType("int")),
                                    Helpers.MakeIdentifierPath("p", Helpers.MakePrimitiveType("int"))
                                )
                            ),
                            Modifiers.Immutable
                        ),
                        Statement.MakeVarDecl(
                            Helpers.MakeSeq(Helpers.MakePaticularPatternWithType("s", Helpers.MakePrimitiveType("int"))),
                            Helpers.MakeSeq(
                                Expression.MakeBinaryExpr(
                                    OperatorType.BitwiseShiftLeft,
                                    Helpers.MakeIdentifierPath("x", Helpers.MakePrimitiveType("int")),
                                    Expression.MakeConstant("int", 2)
                                )
                            ),
                            Modifiers.Immutable
                        ),
                        Statement.MakeVarDecl(
                            Helpers.MakeSeq(Helpers.MakePaticularPatternWithType("t", Helpers.MakePrimitiveType("int"))),
                            Helpers.MakeSeq(
                                Expression.MakeBinaryExpr(
                                    OperatorType.BitwiseAnd,
                                    Helpers.MakeIdentifierPath("r", Helpers.MakePrimitiveType("int")),
                                    Helpers.MakeIdentifierPath("s", Helpers.MakePrimitiveType("int"))
                                )
                            ),
                            Modifiers.Immutable
                        ),
                        Statement.MakeVarDecl(
                            Helpers.MakeSeq(Helpers.MakePaticularPatternWithType("v", Helpers.MakePrimitiveType("int"))),
                            Helpers.MakeSeq(
                                Expression.MakeBinaryExpr(
                                    OperatorType.BitwiseOr,
                                    Helpers.MakeIdentifierPath("x", Helpers.MakePrimitiveType("int")),
                                    Helpers.MakeIdentifierPath("t", Helpers.MakePrimitiveType("int"))
                                )
                            ),
                            Modifiers.Immutable
                        ),
                        Statement.MakeVarDecl(
                            Helpers.MakeSeq(Helpers.MakePaticularPatternWithType("w", Helpers.MakePrimitiveType("int"))),
                            Helpers.MakeSeq(
                                Expression.MakeBinaryExpr(
                                    OperatorType.Plus,
                                    Helpers.MakeIdentifierPath("r", Helpers.MakePrimitiveType("int")),
                                    Helpers.MakeIdentifierPath("s", Helpers.MakePrimitiveType("int"))
                                )
                            ),
                            Modifiers.Immutable
                        ),
                        Statement.MakeVarDecl(
                            Helpers.MakeSeq(Helpers.MakePaticularPatternWithType("y", Helpers.MakePrimitiveType("int"))),
                            Helpers.MakeSeq(
                                Expression.MakeBinaryExpr(
                                    OperatorType.Plus,
                                    Helpers.MakeIndexerExpression(
                                        Helpers.MakeIdentifierPath("ary", Helpers.MakeGenericType("array", Helpers.MakePrimitiveType("int"))),
                                        Expression.MakeConstant("int", 0)
                                    ),
                                    Expression.MakeBinaryExpr(
                                        OperatorType.Plus,
                                        Helpers.MakeIndexerExpression(
                                            Helpers.MakeIdentifierPath("ary", Helpers.MakeGenericType("array", Helpers.MakePrimitiveType("int"))),
                                            Expression.MakeConstant("int", 1)
                                        ),
                                        Expression.MakeIndexer(
                                            Helpers.MakeIdentifierPath("d", Helpers.MakeGenericType("dictionary", Helpers.MakePrimitiveType("string"), Helpers.MakePrimitiveType("int"))),
                                            Helpers.MakeSeq(Expression.MakeConstant("string", "a"))
                                        )
                                    )
                                )
                            ),
                            Modifiers.Immutable
                        ),
                        Statement.MakeVarDecl(
                            Helpers.MakeSeq(Helpers.MakePaticularPatternWithType("z", Helpers.MakePrimitiveType("int"))),
                            Helpers.MakeSeq(
                                Expression.MakeBinaryExpr(
                                    OperatorType.Times,
                                    Helpers.MakeIdentifierPath("v", Helpers.MakePrimitiveType("int")),
                                    Helpers.MakeIdentifierPath("w", Helpers.MakePrimitiveType("int"))
                                )
                            ),
                            Modifiers.Immutable
                        ),
                        Statement.MakeExprStmt(
                            Helpers.MakeCallExpression(
                                Helpers.SomeWellKnownExpressions.Println,
                                Helpers.MakeCallExpression(
                                    Helpers.SomeWellKnownExpressions.StringFormatN,
                                    Expression.MakeConstant("string", "{0}, {1}, {2}, {3}, {4}, {5}, {6}, {7}, {8}, {9}, {10}, {11}, {12}, {13}"),
                                    Helpers.MakeIdentifierPath(
                                        "ary",
                                        Helpers.MakeGenericType(
                                            "array",
                                            Helpers.MakePrimitiveType("int")
                                        )
                                    ),
                                    Helpers.MakeIdentifierPath(
                                        "d",
                                        Helpers.MakeGenericType(
                                            "dictionary",
                                            Helpers.MakePrimitiveType("string"),
                                            Helpers.MakePrimitiveType("int")
                                        )
                                    ),
                                    Helpers.MakeIdentifierPath(
                                        "m",
                                        Helpers.MakePrimitiveType("int")
                                    ),
                                    Helpers.MakeIdentifierPath(
                                        "m2",
                                        Helpers.MakePrimitiveType("int")
                                    ),
                                    Helpers.MakeIdentifierPath(
                                        "x",
                                        Helpers.MakePrimitiveType("int")
                                    ),
                                    Helpers.MakeIdentifierPath(
                                        "p",
                                        Helpers.MakePrimitiveType("int")
                                    ),
                                    Helpers.MakeIdentifierPath(
                                        "q",
                                        Helpers.MakePrimitiveType("int")
                                    ),
                                    Helpers.MakeIdentifierPath(
                                        "r",
                                        Helpers.MakePrimitiveType("int")
                                    ),
                                    Helpers.MakeIdentifierPath(
                                        "s",
                                        Helpers.MakePrimitiveType("int")
                                    ),
                                    Helpers.MakeIdentifierPath(
                                        "t",
                                        Helpers.MakePrimitiveType("int")
                                    ),
                                    Helpers.MakeIdentifierPath(
                                        "v",
                                        Helpers.MakePrimitiveType("int")
                                    ),
                                    Helpers.MakeIdentifierPath(
                                        "w",
                                        Helpers.MakePrimitiveType("int")
                                    ),
                                    Helpers.MakeIdentifierPath(
                                        "y",
                                        Helpers.MakePrimitiveType("int")
                                    ),
                                    Helpers.MakeIdentifierPath(
                                        "z",
                                        Helpers.MakePrimitiveType("int")
                                    )
                                )
                            )
                        )
                    ),
                    Helpers.MakeVoidType(),
                    Modifiers.None
                )
            });

            Assert.IsNotNull(ast);
            Helpers.AstStructuralEqual(ast, expected_ast);
        }

        [Test]
        public void BasicStatements()
        {
            var parser = new Parser(new Scanner("../../sources/for_unit_tests/basic_statements.exs"));
            parser.DoPostParseProcessing = true;
            parser.Parse();

            var ast = parser.TopmostAst;

            var expected_ast = AstNode.MakeModuleDef("main", new List<EntityDeclaration>{
                EntityDeclaration.MakeFunc(
                    "main",
                    Enumerable.Empty<ParameterDeclaration>(),
                    Statement.MakeBlock(
                        Statement.MakeVarDecl(
                            Helpers.MakeSeq(
                                Helpers.MakePaticularPatternWithType("x", Helpers.MakePrimitiveType("int")),
                                Helpers.MakePaticularPatternWithType("y", Helpers.MakePrimitiveType("int")),
                                Helpers.MakePaticularPatternWithType("z", Helpers.MakePrimitiveType("int")),
                                Helpers.MakePaticularPatternWithType("w", Helpers.MakePrimitiveType("int"))
                            ),
                            Helpers.MakeSeq(
                                Expression.MakeConstant("int", 100),
                                Expression.MakeConstant("int", 20),
                                Expression.MakeConstant("int", 300),
                                Expression.MakeConstant("int", 400)
                            ),
                            Modifiers.Immutable
                        ),
                        Helpers.MakeVariableDeclaration(
                            Helpers.MakeSeq(
                                Helpers.MakeExactPatternWithType("flag", Helpers.MakePrimitiveType("bool"))
                            ),
                            Modifiers.None
                        ),
                        Statement.MakeExprStmt(
                            Helpers.MakeCallExpression(
                                Helpers.SomeWellKnownExpressions.Print,
                                Helpers.MakeCallExpression(
                                    Helpers.SomeWellKnownExpressions.StringFormatN,
                                    Expression.MakeConstant("string", "{0}, {1}, {2}, {3}"),
                                    Helpers.MakeIdentifierPath(
                                        "x",
                                        Helpers.MakePrimitiveType("int")
                                    ),
                                    Helpers.MakeIdentifierPath(
                                        "y",
                                        Helpers.MakePrimitiveType("int")
                                    ),
                                    Helpers.MakeIdentifierPath(
                                        "z",
                                        Helpers.MakePrimitiveType("int")
                                    ),
                                    Helpers.MakeIdentifierPath(
                                        "w",
                                        Helpers.MakePrimitiveType("int")
                                    )
                                )
                            )
                        ),
                        Statement.MakeIfStmt(
                            PatternConstruct.MakeExpressionPattern(
                                Expression.MakeBinaryExpr(
                                    OperatorType.Equality,
                                    Helpers.MakeIdentifierPath(
                                        "x",
                                        Helpers.MakePrimitiveType("int")
                                    ),
                                    Expression.MakeConstant("int", 100)
                                )
                            ),
                            Statement.MakeBlock(
                                Statement.MakeExprStmt(
                                    Expression.MakeSingleAssignment(
                                        Helpers.MakeIdentifierPath(
                                            "flag",
                                            Helpers.MakePrimitiveType("bool")
                                        ),
                                        Expression.MakeConstant("bool", true)
                                    )
                                )
                            ),
                            Statement.MakeIfStmt(
                                PatternConstruct.MakeExpressionPattern(
                                    Expression.MakeBinaryExpr(
                                        OperatorType.Equality,
                                        Helpers.MakeIdentifierPath(
                                            "x",
                                            Helpers.MakePrimitiveType("int")
                                        ),
                                        Expression.MakeConstant("int", 0)
                                    )
                                ),
                                Statement.MakeBlock(
                                    Statement.MakeExprStmt(
                                        Expression.MakeSingleAssignment(
                                            Helpers.MakeIdentifierPath(
                                                "flag",
                                                Helpers.MakePrimitiveType("bool")
                                            ),
                                            Expression.MakeConstant("bool", false)
                                        )
                                    )
                                ),
                                Statement.MakeBlock(
                                    Statement.MakeExprStmt(
                                        Expression.MakeSingleAssignment(
                                            Helpers.MakeIdentifierPath(
                                                "flag",
                                                Helpers.MakePrimitiveType("bool")
                                            ),
                                            Expression.MakeConstant("bool", false)
                                        )
                                    )
                                )
                            )
                        ),
                        Statement.MakeVarDecl(
                            Helpers.MakeSeq(
                                Helpers.MakePaticularPatternWithType(
                                    "sum",
                                    Helpers.MakePrimitiveType("int")
                                )
                            ),
                            Helpers.MakeSeq(
                                Expression.MakeConstant("int", 0)
                            ),
                            Modifiers.None
                        ),
                        Statement.MakeValueBindingForStmt(
                            Modifiers.Immutable,
                            Helpers.MakePaticularPatternWithType("p", Helpers.MakePrimitiveType("int")),
                            Expression.MakeIntSeq(
                                Expression.MakeConstant("int", 0),
                                Helpers.MakeIdentifierPath("y", Helpers.MakePrimitiveType("int")),
                                Expression.MakeConstant("int", 1),
                                false
                            ),
                            Statement.MakeBlock(
                                Statement.MakeExprStmt(
                                    Expression.MakeSingleAugmentedAssignment(
                                        OperatorType.Plus,
                                        Helpers.MakeIdentifierPath(
                                            "sum",
                                            Helpers.MakePrimitiveType("int")
                                        ),
                                        Helpers.MakeIdentifierPath(
                                            "p",
                                            Helpers.MakePrimitiveType("int")
                                        )
                                    )
                                ),
                                Statement.MakeExprStmt(
                                    Helpers.MakeCallExpression(
                                        Helpers.SomeWellKnownExpressions.Println,
                                        Helpers.MakeCallExpression(
                                            Helpers.SomeWellKnownExpressions.StringFormat2,
                                            Expression.MakeConstant("string", "{0}, {1}"),
                                            Helpers.MakeIdentifierPath(
                                                "p",
                                                Helpers.MakePrimitiveType("int")
                                            ),
                                            Helpers.MakeIdentifierPath(
                                                "sum",
                                                Helpers.MakePrimitiveType("int")
                                            )
                                        )
                                    )
                                )
                            )
                        ),
                        Statement.MakeVarDecl(
                            Helpers.MakeSeq(
                                Helpers.MakeExactPatternWithType(
                                    "fibs",
                                    Helpers.MakeGenericType(
                                        "vector",
                                        Helpers.MakePrimitiveType("int")
                                    )
                                ),
                                Helpers.MakePaticularPatternWithType(
                                    "a",
                                    Helpers.MakePrimitiveType("int")
                                ),
                                Helpers.MakePaticularPatternWithType(
                                    "b",
                                    Helpers.MakePrimitiveType("int")
                                )
                            ),
                            Helpers.MakeSeq<Expression>(
                                Expression.MakeSequenceInitializer(
                                    Helpers.MakeGenericType(
                                        "vector",
                                        Helpers.MakePrimitiveType("int")
                                    ),
                                    Enumerable.Empty<Expression>()
                                ),
                                Expression.MakeConstant("int", 0),
                                Expression.MakeConstant("int", 1)
                            ),
                            Modifiers.None
                        ),
                        Statement.MakeWhileStmt(
                            Expression.MakeBinaryExpr(
                                OperatorType.LessThan,
                                Helpers.MakeIdentifierPath(
                                    "b",
                                    Helpers.MakePrimitiveType("int")
                                ),
                                Expression.MakeConstant("int", 1000)
                            ),
                            Statement.MakeBlock(
                                Statement.MakeExprStmt(
                                    Helpers.MakeCallExpression(
                                        Expression.MakeMemRef(
                                            Helpers.MakeIdentifierPath(
                                                "fibs",
                                                Helpers.MakeGenericType(
                                                    "vector",
                                                    Helpers.MakePrimitiveType("int")
                                                )
                                            ),
                                            Helpers.MakeFunctionIdentifier(
                                                "Add",
                                                Helpers.MakeVoidType(),
                                                AstType.MakePrimitiveType("int")
                                            )
                                        ),
                                        Helpers.MakeIdentifierPath(
                                            "b",
                                            Helpers.MakePrimitiveType("int")
                                        )
                                    )
                                ),
                                Statement.MakeExprStmt(
                                    Helpers.MakeAssignment(
                                        Helpers.MakeSeq(
                                            Helpers.MakeIdentifierPath(
                                                "a",
                                                Helpers.MakePrimitiveType("int")
                                            ),
                                            Helpers.MakeIdentifierPath(
                                                "b",
                                                Helpers.MakePrimitiveType("int")
                                            )
                                        ),
                                        Helpers.MakeSeq<Expression>(
                                            Helpers.MakeIdentifierPath(
                                                "b",
                                                Helpers.MakePrimitiveType("int")
                                            ),
                                            Expression.MakeBinaryExpr(
                                                OperatorType.Plus,
                                                Helpers.MakeIdentifierPath(
                                                    "a",
                                                    Helpers.MakePrimitiveType("int")
                                                ),
                                                Helpers.MakeIdentifierPath(
                                                    "b",
                                                    Helpers.MakePrimitiveType("int")
                                                )
                                            )
                                        )
                                    )
                                )
                            )
                        ),
                        Statement.MakeVarDecl(
                            Helpers.MakeSeq(
                                Helpers.MakePaticularPatternWithType(
                                    "n",
                                    Helpers.MakePrimitiveType("int")
                                )
                            ),
                            Helpers.MakeSeq(
                                Expression.MakeConstant("int", 100)
                            ),
                            Modifiers.None
                        ),
                        Statement.MakeDoWhileStmt(
                            Expression.MakeBinaryExpr(
                                OperatorType.GreaterThan,
                                Helpers.MakeIdentifierPath(
                                    "n",
                                    Helpers.MakePrimitiveType("int")
                                ),
                                Expression.MakeConstant("int", 0)
                            ),
                            Statement.MakeBlock(
                                Statement.MakeExprStmt(
                                    Expression.MakeSingleAugmentedAssignment(
                                        OperatorType.Minus,
                                        Helpers.MakeIdentifierPath(
                                            "n",
                                            Helpers.MakePrimitiveType("int")
                                        ),
                                        Expression.MakeConstant("int", 40)
                                    )
                                ),
                                Statement.MakeExprStmt(
                                    Helpers.MakeCallExpression(
                                        Helpers.SomeWellKnownExpressions.Println,
                                        Helpers.MakeCallExpression(
                                            Helpers.SomeWellKnownExpressions.StringFormat1,
                                            Expression.MakeConstant("string", "{0}"),
                                            Helpers.MakeIdentifierPath(
                                                "n",
                                                Helpers.MakePrimitiveType("int")
                                            )
                                        )
                                    )
                                )
                            )
                        ),
                        Statement.MakeVarDecl(
                            Helpers.MakeSeq(
                                Helpers.MakeExactPatternWithType(
                                    "vec",
                                    Helpers.MakeGenericType(
                                        "vector",
                                        Helpers.MakePrimitiveType("int")
                                    )
                                )
                            ),
                            Helpers.MakeSeq(
                                Expression.MakeSequenceInitializer(
                                    Helpers.MakeGenericType(
                                        "vector",
                                        Helpers.MakePrimitiveType("int")
                                    ),
                                    Enumerable.Empty<Expression>()
                                )
                            ),
                            Modifiers.None
                        ),
                        Statement.MakeValueBindingForStmt(
                            Modifiers.Immutable,
                            Helpers.MakePaticularPatternWithType(
                                "i",
                                Helpers.MakePrimitiveType("int")
                            ),
                            Helpers.MakeSequenceInitializer(
                                Helpers.MakeGenericType(
                                    "vector",
                                    Helpers.MakePrimitiveType("int")
                                ),
                                Expression.MakeConstant("int", 0),
                                Expression.MakeConstant("int", 1),
                                Expression.MakeConstant("int", 2),
                                Expression.MakeConstant("int", 3),
                                Expression.MakeConstant("int", 4),
                                Expression.MakeConstant("int", 5),
                                Expression.MakeConstant("int", 6),
                                Expression.MakeConstant("int", 7),
                                Expression.MakeConstant("int", 8),
                                Expression.MakeConstant("int", 9)
                            ),
                            Statement.MakeBlock(
                                Statement.MakeVarDecl(
                                    Helpers.MakeSeq(
                                        Helpers.MakePaticularPatternWithType(
                                            "MAX_J",
                                            Helpers.MakePrimitiveType("int")
                                        )
                                    ),
                                    Helpers.MakeSeq(
                                        Expression.MakeConstant("int", 8)
                                    ),
                                    Modifiers.Immutable
                                ),
                                Statement.MakeValueBindingForStmt(
                                    Modifiers.Immutable,
                                    Helpers.MakePaticularPatternWithType(
                                        "j",
                                        Helpers.MakePrimitiveType("int")
                                    ),
                                    Expression.MakeIntSeq(
                                        Expression.MakeConstant("int", 0),
                                        Expression.MakeConstant("int", 10),
                                        Expression.MakeConstant("int", 1),
                                        false
                                    ),
                                    Statement.MakeBlock(
                                        Statement.MakeIfStmt(
                                            PatternConstruct.MakeExpressionPattern(
                                                Expression.MakeBinaryExpr(
                                                    OperatorType.ConditionalOr,
                                                    Expression.MakeBinaryExpr(
                                                        OperatorType.Equality,
                                                        Helpers.MakeIdentifierPath(
                                                            "i",
                                                            Helpers.MakePrimitiveType("int")
                                                        ),
                                                        Expression.MakeConstant("int", 3)
                                                    ),
                                                    Expression.MakeBinaryExpr(
                                                        OperatorType.Equality,
                                                        Helpers.MakeIdentifierPath(
                                                            "i",
                                                            Helpers.MakePrimitiveType("int")
                                                        ),
                                                        Expression.MakeConstant("int", 6)
                                                    )
                                                )
                                            ),
                                            Statement.MakeBlock(
                                                Statement.MakeBreakStmt(
                                                    Expression.MakeConstant("int", 1)
                                                )
                                            ),
                                            null
                                        ),
                                        Statement.MakeIfStmt(
                                            PatternConstruct.MakeExpressionPattern(
                                                Expression.MakeBinaryExpr(
                                                    OperatorType.Equality,
                                                    Helpers.MakeIdentifierPath(
                                                        "j",
                                                        Helpers.MakePrimitiveType("int")
                                                    ),
                                                    Helpers.MakeIdentifierPath(
                                                        "MAX_J",
                                                        Helpers.MakePrimitiveType("int")
                                                    )
                                                )
                                            ),
                                            Statement.MakeBlock(
                                                Statement.MakeContinueStmt(
                                                    Expression.MakeConstant("int", 2)
                                                )
                                            ),
                                            null
                                        ),
                                        Statement.MakeExprStmt(
                                            Helpers.MakeCallExpression(
                                                Expression.MakeMemRef(
                                                    Helpers.MakeIdentifierPath(
                                                        "vec",
                                                        Helpers.MakeGenericType(
                                                            "vector",
                                                            Helpers.MakePrimitiveType("int")
                                                        )
                                                    ),
                                                    Helpers.MakeFunctionIdentifier(
                                                        "Add",
                                                        Helpers.MakeVoidType(),
                                                        AstType.MakePrimitiveType("int")
                                                    )
                                                ),
                                                Helpers.MakeIdentifierPath(
                                                    "i",
                                                    Helpers.MakePrimitiveType("int")
                                                )
                                            )
                                        ),
                                        Statement.MakeExprStmt(
                                            Helpers.MakeCallExpression(
                                                Expression.MakeMemRef(
                                                    Helpers.MakeIdentifierPath(
                                                        "vec",
                                                        Helpers.MakeGenericType(
                                                            "vector",
                                                            Helpers.MakePrimitiveType("int")
                                                        )
                                                    ),
                                                    Helpers.MakeFunctionIdentifier(
                                                        "Add",
                                                        Helpers.MakeVoidType(),
                                                        AstType.MakePrimitiveType("int")
                                                    )
                                                ),
                                                Helpers.MakeIdentifierPath(
                                                    "j",
                                                    Helpers.MakePrimitiveType("int")
                                                )
                                            )
                                        ),
                                        Statement.MakeExprStmt(
                                            Helpers.MakeCallExpression(
                                                Helpers.SomeWellKnownExpressions.Println,
                                                Helpers.MakeCallExpression(
                                                    Helpers.SomeWellKnownExpressions.StringFormat2,
                                                    Expression.MakeConstant("string", "{0}, {1}"),
                                                    Helpers.MakeIdentifierPath(
                                                        "i",
                                                        Helpers.MakePrimitiveType("int")
                                                    ),
                                                    Helpers.MakeIdentifierPath(
                                                        "j",
                                                        Helpers.MakePrimitiveType("int")
                                                    )
                                                )
                                            )
                                        )
                                    )
                                )
                            )
                        ),
                        Statement.MakeExprStmt(
                            Helpers.MakeCallExpression(
                                Helpers.SomeWellKnownExpressions.Println,
                                Helpers.MakeCallExpression(
                                    Helpers.SomeWellKnownExpressions.StringFormatN,
                                    Expression.MakeConstant("string", "{0}, {1}, {2}, {3}, {4}, {5}"),
                                    Helpers.MakeIdentifierPath(
                                        "flag",
                                        Helpers.MakePrimitiveType("bool")
                                    ),
                                    Helpers.MakeIdentifierPath(
                                        "sum",
                                        Helpers.MakePrimitiveType("int")
                                    ),
                                    Helpers.MakeIdentifierPath(
                                        "a",
                                        Helpers.MakePrimitiveType("int")
                                    ),
                                    Helpers.MakeIdentifierPath(
                                        "b",
                                        Helpers.MakePrimitiveType("int")
                                    ),
                                    Helpers.MakeIdentifierPath(
                                        "fibs",
                                        Helpers.MakeGenericType(
                                            "vector",
                                            Helpers.MakePrimitiveType("int")
                                        )
                                    ),
                                    Helpers.MakeIdentifierPath(
                                        "vec",
                                        Helpers.MakeGenericType(
                                            "vector",
                                            Helpers.MakePrimitiveType("int")
                                        )
                                    )
                                )
                            )
                        )
                    ),
                    Helpers.MakeVoidType(),
                    Modifiers.None
                )
            });

            Assert.IsNotNull(ast);
            Helpers.AstStructuralEqual(ast, expected_ast);
        }

        [Test]
        public void MatchStatements()
        {
            var parser = new Parser(new Scanner("../../sources/for_unit_tests/match_statements.exs"));
            parser.DoPostParseProcessing = true;
            parser.Parse();

            var ast = parser.TopmostAst;

            var expected_ast = AstNode.MakeModuleDef("main", new List<EntityDeclaration>{
                EntityDeclaration.MakeClassDecl(
                    "TestClass6",
                    Enumerable.Empty<AstType>(),
                    Helpers.MakeSeq<EntityDeclaration>(
                        EntityDeclaration.MakeField(
                            Helpers.MakeSeq(
                                Helpers.MakeExactPatternWithType("x", Helpers.MakePrimitiveType("int")),
                                Helpers.MakeExactPatternWithType("y", Helpers.MakePrimitiveType("int")),
                                Helpers.MakeExactPatternWithType("z", Helpers.MakePrimitiveType("int"))
                            ),
                            Helpers.MakeSeq(
                                Expression.Null,
                                Expression.Null,
                                Expression.Null
                            ),
                            Modifiers.Public | Modifiers.Immutable
                        )
                    ),
                    Modifiers.None
                ),
                EntityDeclaration.MakeFunc(
                    "main",
                    Enumerable.Empty<ParameterDeclaration>(),
                    Statement.MakeBlock(
                        Statement.MakeVarDecl(
                            Helpers.MakeSeq(
                                Helpers.MakePaticularPatternWithType(
                                    "tmp",
                                    Helpers.MakePrimitiveType("string")
                                )
                            ),
                            Helpers.MakeSeq(
                                Expression.MakeConstant(
                                    "string",
                                    "akarichan"
                                )
                            ),
                            Modifiers.Immutable
                        ),
                        Statement.MakeMatchStmt(
                            Helpers.MakeIdentifierPath(
                                "tmp",
                                Helpers.MakePrimitiveType("string")
                            ),
                            Statement.MakeMatchClause(
                                null,
                                Statement.MakeExprStmt(
                                    Helpers.MakeCallExpression(
                                        Helpers.SomeWellKnownExpressions.Println,
                                        Expression.MakeConstant(
                                            "string",
                                            "kawakawa"
                                        )
                                    )
                                ),
                                PatternConstruct.MakeExpressionPattern(
                                    Expression.MakeConstant("string", "akarichan")
                                )
                            ),
                            Statement.MakeMatchClause(
                                Expression.MakeConstant("bool", true),
                                Statement.MakeExprStmt(
                                    Helpers.MakeCallExpression(
                                        Helpers.SomeWellKnownExpressions.Println,
                                        Expression.MakeConstant(
                                            "string",
                                            "ankokuthunder!"
                                        )
                                    )
                                ),
                                PatternConstruct.MakeExpressionPattern(
                                    Expression.MakeConstant("string", "chinatsu")
                                )
                            ),
                            Statement.MakeMatchClause(
                                null,
                                Statement.MakeExprStmt(
                                    Helpers.MakeCallExpression(
                                        Helpers.SomeWellKnownExpressions.Println,
                                        Expression.MakeConstant(
                                            "string",
                                            "gaichiban!"
                                        )
                                    )
                                ),
                                PatternConstruct.MakeExpressionPattern(
                                    Expression.MakeConstant("string", "kyoko")
                                )
                            ),
                            Statement.MakeMatchClause(
                                null,
                                Statement.MakeExprStmt(
                                    Helpers.MakeCallExpression(
                                        Helpers.SomeWellKnownExpressions.Println,
                                        Expression.MakeConstant(
                                            "string",
                                            "doyaxtu!"
                                        )
                                    )
                                ),
                                PatternConstruct.MakeExpressionPattern(
                                    Expression.MakeConstant("string", "yui")
                                )
                            )
                        ),
                        Statement.MakeVarDecl(
                            Helpers.MakeSeq(
                                Helpers.MakePaticularPatternWithType("tmp2", Helpers.MakePrimitiveType("int"))
                            ),
                            Helpers.MakeSeq(
                                Expression.MakeConstant("int", 1)
                            ),
                            Modifiers.Immutable
                        ),
                        Statement.MakeMatchStmt(
                            Helpers.MakeIdentifierPath("tmp2", Helpers.MakePrimitiveType("int")),
                            Statement.MakeMatchClause(
                                null,
                                Statement.MakeExprStmt(
                                    Expression.MakeSequenceExpression(
                                        Helpers.MakeCallExpression(
                                            Helpers.SomeWellKnownExpressions.Println,
                                            Expression.MakeConstant("string", "0")
                                        )
                                    )
                                ),
                                PatternConstruct.MakeExpressionPattern(
                                    Expression.MakeConstant("int", 0)
                                )
                            ),
                            Statement.MakeMatchClause(
                                null,
                                Statement.MakeExprStmt(
                                    Expression.MakeSequenceExpression(
                                        Helpers.MakeCallExpression(
                                            Helpers.SomeWellKnownExpressions.Println,
                                            Expression.MakeConstant("string", "1 or 2")
                                        )
                                    )
                                ),
                                PatternConstruct.MakeExpressionPattern(
                                    Expression.MakeConstant("int", 1)
                                ),
                                PatternConstruct.MakeExpressionPattern(
                                    Expression.MakeConstant("int", 2)
                                )
                            ),
                            Statement.MakeMatchClause(
                                null,
                                Statement.MakeExprStmt(
                                    Expression.MakeSequenceExpression(
                                        Helpers.MakeCallExpression(
                                            Helpers.SomeWellKnownExpressions.Println,
                                            Helpers.MakeCallExpression(
                                                Helpers.SomeWellKnownExpressions.StringFormat1,
                                                Expression.MakeConstant("string", "{0} is in the range of 3 to 10"),
                                                Helpers.MakeIdentifierPath(
                                                    "i",
                                                    Helpers.MakePrimitiveType("int")
                                                )
                                            )
                                        )
                                    )
                                ),
                                PatternConstruct.MakeIdentifierPattern(
                                    "i",
                                    Helpers.MakePrimitiveType("int"),
                                    PatternConstruct.MakeExpressionPattern(
                                        Expression.MakeIntSeq(
                                            Expression.MakeConstant("int", 3),
                                            Expression.MakeConstant("int", 10),
                                            Expression.MakeConstant("int", 1),
                                            true
                                        )
                                    )
                                )
                            ),
                            Statement.MakeMatchClause(
                                null,
                                Statement.MakeExprStmt(
                                    Expression.MakeSequenceExpression(
                                        Helpers.MakeCallExpression(
                                            Helpers.SomeWellKnownExpressions.Println,
                                            Expression.MakeConstant("string", "otherwise")
                                        )
                                    )
                                ),
                                PatternConstruct.MakeWildcardPattern()
                            )
                        ),
                        Statement.MakeVarDecl(
                            Helpers.MakeSeq(
                                Helpers.MakePaticularPatternWithType("tmp3", Helpers.MakeGenericType("TestClass6"))
                            ),
                            Helpers.MakeSeq(
                                Expression.MakeObjectCreation(
                                    Helpers.MakeGenericType("TestClass6"),
                                    Helpers.MakeSeq(
                                        AstNode.MakeIdentifier("x"),
                                        AstNode.MakeIdentifier("y"),
                                        AstNode.MakeIdentifier("z")
                                    ),
                                    Helpers.MakeSeq(
                                        Expression.MakeConstant("int", 1),
                                        Expression.MakeConstant("int", 2),
                                        Expression.MakeConstant("int", 3)
                                    )
                                )
                            ),
                            Modifiers.Immutable
                        ),
                        Statement.MakeMatchStmt(
                            Helpers.MakeIdentifierPath("tmp3", Helpers.MakeGenericType("TestClass6")),
                            Statement.MakeMatchClause(
                                null,
                                Statement.MakeExprStmt(
                                    Expression.MakeSequenceExpression(
                                        Helpers.MakeCallExpression(
                                            Helpers.SomeWellKnownExpressions.Println,
                                            Helpers.MakeCallExpression(
                                                Helpers.SomeWellKnownExpressions.StringFormat1,
                                                Expression.MakeConstant("string", "x is {0}"),
                                                Helpers.MakeIdentifierPath(
                                                    "x",
                                                    Helpers.MakePrimitiveType("int")
                                                )
                                            )
                                        )
                                    )
                                ),
                                PatternConstruct.MakeDestructuringPattern(
                                    Helpers.MakeGenericType("TestClass6"),
                                    PatternConstruct.MakeIdentifierPattern(
                                        "x",
                                        Helpers.MakePrimitiveType("int"),
                                        null
                                    ),
                                    PatternConstruct.MakeIgnoringRestPattern()
                                )
                            ),
                            Statement.MakeMatchClause(
                                null,
                                Statement.MakeExprStmt(
                                    Expression.MakeSequenceExpression(
                                        Helpers.MakeCallExpression(
                                            Helpers.SomeWellKnownExpressions.Println,
                                            Helpers.MakeCallExpression(
                                                Helpers.SomeWellKnownExpressions.StringFormat2,
                                                Expression.MakeConstant("string", "x is {0} and y is {1}"),
                                                Helpers.MakeIdentifierPath(
                                                    "x",
                                                    Helpers.MakePrimitiveType("int")
                                                ),
                                                Helpers.MakeIdentifierPath(
                                                    "y",
                                                    Helpers.MakePrimitiveType("int")
                                                )
                                            )
                                        )
                                    )
                                ),
                                PatternConstruct.MakeDestructuringPattern(
                                    Helpers.MakeGenericType("TestClass6"),
                                    PatternConstruct.MakeIdentifierPattern(
                                        "x",
                                        Helpers.MakePrimitiveType("int"),
                                        null
                                    ),
                                    PatternConstruct.MakeIdentifierPattern(
                                        "y",
                                        Helpers.MakePrimitiveType("int"),
                                        null
                                    ),
                                    PatternConstruct.MakeWildcardPattern()
                                )
                            )
                        ),
                        /*Statement.MakeVarDecl(
                            Helpers.MakeSeq(
                                AstNode.MakeIdentifier("tmp4", Helpers.MakeGenericType("vector", Helpers.MakePrimitiveType("int")))
                            ),
                            Helpers.MakeSeq(
                                Expression.MakeSequenceInitializer(
                                    Helpers.MakeGenericType("vector", Helpers.MakePrimitiveType("int")),
                                    Expression.MakeConstant("int", 1),
                                    Expression.MakeConstant("int", 2),
                                    Expression.MakeConstant("int", 3),
                                    Expression.MakeConstant("int", 4)
                                )
                            ),
                            Modifiers.Immutable
                        ),
                        Statement.MakeMatchStmt(
                            Helpers.MakeIdentifierPath(
                                "tmp4",
                                Helpers.MakeGenericType("vector", Helpers.MakePrimitiveType("int"))
                            ),
                            Statement.MakeMatchClause(
                                null,
                                Statement.MakeExprStmt(
                                    Expression.MakeSequenceExpression(
                                        Helpers.MakeCallExpression(
                                            Helpers.SomeWellKnownExpressions.Println,
                                            Helpers.MakeCallExpression(
                                                Helpers.SomeWellKnownExpressions.StringFormat2,
                                                Expression.MakeConstant("string", "x and y are both vector's elements and the values are {0} and {1} respectively"),
                                                Helpers.MakeIdentifierPath(
                                                    "x",
                                                    Helpers.MakePrimitiveType("int")
                                                ),
                                                Helpers.MakeIdentifierPath(
                                                    "y",
                                                    Helpers.MakePrimitiveType("int")
                                                )
                                            )
                                        )
                                    )
                                ),
                                PatternConstruct.MakeCollectionPattern(
                                    true,
                                    PatternConstruct.MakeIdentifierPattern(
                                        "x",
                                        Helpers.MakePrimitiveType("int"),
                                        null
                                    ),
                                    PatternConstruct.MakeIdentifierPattern(
                                        "y",
                                        Helpers.MakePrimitiveType("int"),
                                        null
                                    ),
                                    PatternConstruct.MakeIgnoringRestPattern()
                                )
                            ),
                            Statement.MakeMatchClause(
                                null,
                                Statement.MakeExprStmt(
                                    Expression.MakeSequenceExpression(
                                        Helpers.MakeCallExpression(
                                            Helpers.SomeWellKnownExpressions.Print,
                                            Expression.MakeConstant("string", "tmp4 is a vector")
                                        )
                                    )
                                ),
                                PatternConstruct.MakeCollectionPattern(
                                    true,
                                    PatternConstruct.MakeExpressionPattern(
                                        Expression.MakeConstant("int", 1)
                                    ),
                                    PatternConstruct.MakeExpressionPattern(
                                        Expression.MakeConstant("int", 2)
                                    ),
                                    PatternConstruct.MakeExpressionPattern(
                                        Expression.MakeConstant("int", 3)
                                    ),
                                    PatternConstruct.MakeWildcardPattern()
                                )
                            )
                        ),
                        Statement.MakeVarDecl(
                            Helpers.MakeSeq(
                                AstNode.MakeIdentifier("tmp5", Helpers.MakeGenericType("array", Helpers.MakePrimitiveType("int")))
                            ),
                            Helpers.MakeSeq(
                                Expression.MakeSequenceInitializer(
                                    Helpers.MakeGenericType("array", Helpers.MakePrimitiveType("int")),
                                    Expression.MakeConstant("int", 1),
                                    Expression.MakeConstant("int", 2),
                                    Expression.MakeConstant("int", 3),
                                    Expression.MakeConstant("int", 4)
                                )
                            ),
                            Modifiers.Immutable
                        ),
                        Statement.MakeMatchStmt(
                            Helpers.MakeIdentifierPath(
                                "tmp5",
                                Helpers.MakeGenericType("array", Helpers.MakePrimitiveType("int"))
                            ),
                            Statement.MakeMatchClause(
                                null,
                                Statement.MakeExprStmt(
                                    Expression.MakeSequenceExpression(
                                        Helpers.MakeCallExpression(
                                            Helpers.SomeWellKnownExpressions.Println,
                                            Helpers.MakeCallExpression(
                                                Helpers.SomeWellKnownExpressions.StringFormatN,
                                                Expression.MakeConstant("string", "x and y are both array's elements and the values are {0} and {1} respectively"),
                                                Helpers.MakeIdentifierPath(
                                                    "x",
                                                    Helpers.MakePrimitiveType("int")
                                                ),
                                                Helpers.MakeIdentifierPath(
                                                    "y",
                                                    Helpers.MakePrimitiveType("int")
                                                )
                                            )
                                        )
                                    )
                                ),
                                PatternConstruct.MakeCollectionPattern(
                                    false,
                                    PatternConstruct.MakeIdentifierPattern(
                                        "x",
                                        Helpers.MakePrimitiveType("int"),
                                        null
                                    ),
                                    PatternConstruct.MakeIdentifierPattern(
                                        "y",
                                        Helpers.MakePrimitiveType("int"),
                                        null
                                    ),
                                    PatternConstruct.MakeIgnoringRestPattern()
                                )
                            ),
                            Statement.MakeMatchClause(
                                null,
                                Statement.MakeExprStmt(
                                    Expression.MakeSequenceExpression(
                                        Helpers.MakeCallExpression(
                                            Helpers.SomeWellKnownExpressions.Print,
                                            Expression.MakeConstant("string", "tmp5 is an array")
                                        )
                                    )
                                ),
                                PatternConstruct.MakeCollectionPattern(
                                    false,
                                    PatternConstruct.MakeExpressionPattern(
                                        Expression.MakeConstant("int", 1)
                                    ),
                                    PatternConstruct.MakeExpressionPattern(
                                        Expression.MakeConstant("int", 2)
                                    ),
                                    PatternConstruct.MakeExpressionPattern(
                                        Expression.MakeConstant("int", 3)
                                    ),
                                    PatternConstruct.MakeWildcardPattern()
                                )
                            )
                        ),*/
                        Statement.MakeVarDecl(
                            Helpers.MakeSeq(
                                Helpers.MakePaticularPatternWithType(
                                    "tmp6",
                                    Helpers.MakeGenericType(
                                        "tuple",
                                        Helpers.MakePrimitiveType("int"),
                                        Helpers.MakePrimitiveType("int")
                                    )
                                )
                            ),
                            Helpers.MakeSeq(
                                Expression.MakeParen(
                                    Expression.MakeSequenceExpression(
                                        Expression.MakeConstant("int", 1),
                                        Expression.MakeConstant("int", 2)
                                    )
                                )
                            ),
                            Modifiers.Immutable
                        ),
                        Statement.MakeMatchStmt(
                            Helpers.MakeIdentifierPath(
                                "tmp6",
                                Helpers.MakeGenericType(
                                    "tuple",
                                    Helpers.MakePrimitiveType("int"),
                                    Helpers.MakePrimitiveType("int")
                                )
                            ),
                            Statement.MakeMatchClause(
                                null,
                                Statement.MakeExprStmt(
                                    Expression.MakeSequenceExpression(
                                        Helpers.MakeCallExpression(
                                            Helpers.SomeWellKnownExpressions.Println,
                                            Helpers.MakeCallExpression(
                                                Helpers.SomeWellKnownExpressions.StringFormat1,
                                                Expression.MakeConstant("string", "x is {0}"),
                                                Helpers.MakeIdentifierPath(
                                                    "x",
                                                    Helpers.MakePrimitiveType("int")
                                                )
                                            )
                                        ) 
                                    )
                                ),
                                PatternConstruct.MakeTuplePattern(
                                    PatternConstruct.MakeIdentifierPattern(
                                        "x",
                                        Helpers.MakePrimitiveType("int"),
                                        null
                                    ),
                                    PatternConstruct.MakeIgnoringRestPattern()
                                )
                            ),
                            Statement.MakeMatchClause(
                                null,
                                Statement.MakeExprStmt(
                                    Expression.MakeSequenceExpression(
                                        Helpers.MakeCallExpression(
                                            Helpers.SomeWellKnownExpressions.Println,
                                            Helpers.MakeCallExpression(
                                                Helpers.SomeWellKnownExpressions.StringFormat2,
                                                Expression.MakeConstant("string", "x is {0} and y is {1}"),
                                                Helpers.MakeIdentifierPath(
                                                    "x",
                                                    Helpers.MakePrimitiveType("int")
                                                ),
                                                Helpers.MakeIdentifierPath(
                                                    "y",
                                                    Helpers.MakePrimitiveType("int")
                                                )
                                            )
                                        )
                                    )
                                ),
                                PatternConstruct.MakeTuplePattern(
                                    PatternConstruct.MakeIdentifierPattern(
                                        "x",
                                        Helpers.MakePrimitiveType("int"),
                                        null
                                    ),
                                    PatternConstruct.MakeIdentifierPattern(
                                        "y",
                                        Helpers.MakePrimitiveType("int"),
                                        null
                                    )
                                )
                            )
                        )
                    ),
                    Helpers.MakeVoidType(),
                    Modifiers.None
                )
            });

            Assert.IsNotNull(ast);
            Helpers.AstStructuralEqual(ast, expected_ast);
        }

        [Test]
        public void Functions()
        {
            var parser = new Parser(new Scanner("../../sources/for_unit_tests/functions.exs"));
            parser.DoPostParseProcessing = true;
            parser.Parse();

            var ast = parser.TopmostAst;

            var expected_ast = AstNode.MakeModuleDef("main", new List<EntityDeclaration>{
                EntityDeclaration.MakeFunc(
                    "test",
                    Enumerable.Empty<ParameterDeclaration>(),
                    Statement.MakeBlock(
                        Statement.MakeVarDecl(
                            Helpers.MakeSeq(
                                Helpers.MakePaticularPatternWithType(
                                    "a",
                                    Helpers.MakePrimitiveType("int")
                                )
                            ),
                            Helpers.MakeSeq(
                                Expression.MakeConstant("int", 10)
                            ),
                            Modifiers.Immutable
                        ),
                        Helpers.MakeSingleItemReturnStatement(
                            Expression.MakeBinaryExpr(
                                OperatorType.Plus,
                                Helpers.MakeIdentifierPath(
                                    "a",
                                    Helpers.MakePrimitiveType("int")
                                ),
                                Expression.MakeConstant("int", 10)
                            )
                        )
                    ),
                    Helpers.MakePrimitiveType("int"),
                    Modifiers.None
                ),
                EntityDeclaration.MakeFunc(
                    "test2",
                    Helpers.MakeSeq(
                        EntityDeclaration.MakeParameter(
                            "n",
                            Helpers.MakePrimitiveType("int")
                        )
                    ),
                    Statement.MakeBlock(
                        Helpers.MakeSingleItemReturnStatement(
                            Expression.MakeBinaryExpr(
                                OperatorType.Plus,
                                Helpers.MakeIdentifierPath(
                                    "n",
                                    Helpers.MakePrimitiveType("int")
                                ),
                                Expression.MakeConstant("int", 10)
                            )
                        )
                    ),
                    Helpers.MakePrimitiveType("int"),
                    Modifiers.None
                ),
                EntityDeclaration.MakeFunc(
                    "test3",
                    Helpers.MakeSeq(
                        EntityDeclaration.MakeParameter(
                            "n",
                            Helpers.MakePrimitiveType("int")
                        )
                    ),
                    Statement.MakeBlock(
                        Helpers.MakeSingleItemReturnStatement(
                            Expression.MakeBinaryExpr(
                                OperatorType.Plus,
                                Helpers.MakeIdentifierPath(
                                    "n",
                                    Helpers.MakePrimitiveType("int")
                                ),
                                Expression.MakeConstant("int", 20)
                            )
                        )
                    ),
                    Helpers.MakePrimitiveType("int"),
                    Modifiers.None
                ),
                EntityDeclaration.MakeFunc(
                    "test4",
                    Helpers.MakeSeq(
                        EntityDeclaration.MakeParameter("n", Helpers.MakePrimitiveType("int"))
                    ),
                    Statement.MakeBlock(
                        Statement.MakeIfStmt(
                            PatternConstruct.MakeExpressionPattern(
                                Expression.MakeBinaryExpr(
                                    OperatorType.GreaterThanOrEqual,
                                    Helpers.MakeIdentifierPath(
                                        "n",
                                        Helpers.MakePrimitiveType("int")
                                    ),
                                    Expression.MakeConstant("int", 100)
                                )
                            ),
                            Statement.MakeBlock(
                                Helpers.MakeSingleItemReturnStatement(
                                    Helpers.MakeIdentifierPath(
                                        "n",
                                        Helpers.MakePrimitiveType("int")
                                    )
                                )
                            ),
                            Statement.MakeBlock(
                                Helpers.MakeSingleItemReturnStatement(
                                    Helpers.MakeCallExpression(
                                        Helpers.MakeFunctionIdentifierPath(
                                            "test4",
                                            Helpers.MakePrimitiveType("int"),
                                            Helpers.MakePrimitiveType("int")
                                        ),
                                        Expression.MakeBinaryExpr(
                                            OperatorType.Plus,
                                            Helpers.MakeIdentifierPath(
                                                "n",
                                                Helpers.MakePrimitiveType("int")
                                            ),
                                            Expression.MakeConstant("int", 10)
                                        )
                                    )
                                )
                            )
                        )
                    ),
                    Helpers.MakePrimitiveType("int"),
                    Modifiers.None
                ),
                EntityDeclaration.MakeFunc(
                    "test5",
                    Helpers.MakeSeq(
                        EntityDeclaration.MakeParameter(
                            "n",
                            Helpers.MakePrimitiveType("int"),
                            Expression.MakeConstant("int", 100)
                        )
                    ),
                    Statement.MakeBlock(
                        Helpers.MakeSingleItemReturnStatement(
                            Helpers.MakeIdentifierPath(
                                "n",
                                Helpers.MakePrimitiveType("int")
                            )
                        )
                    ),
                    Helpers.MakePrimitiveType("int"),
                    Modifiers.None
                ),
                EntityDeclaration.MakeFunc(
                    "main",
                    Enumerable.Empty<ParameterDeclaration>(),
                    Statement.MakeBlock(
                        Statement.MakeVarDecl(
                            Helpers.MakeSeq(
                                Helpers.MakePaticularPatternWithType(
                                    "a",
                                    Helpers.MakePrimitiveType("int")
                                )
                            ),
                            Helpers.MakeSeq(
                                Helpers.MakeCallExpression(
                                    Helpers.MakeFunctionIdentifierPath(
                                        "test",
                                        Helpers.MakePrimitiveType("int")
                                    )
                                )
                            ),
                            Modifiers.Immutable
                        ),
                        Statement.MakeVarDecl(
                            Helpers.MakeSeq(
                                Helpers.MakePaticularPatternWithType(
                                    "b",
                                    Helpers.MakePrimitiveType("int")
                                )
                            ),
                            Helpers.MakeSeq(
                                Helpers.MakeCallExpression(
                                    Helpers.MakeFunctionIdentifierPath(
                                        "test2",
                                        Helpers.MakePrimitiveType("int"),
                                        Helpers.MakePrimitiveType("int")
                                    ),
                                    Expression.MakeConstant("int", 20)
                                )
                            ),
                            Modifiers.Immutable
                        ),
                        Statement.MakeVarDecl(
                            Helpers.MakeSeq(
                                Helpers.MakePaticularPatternWithType(
                                    "c",
                                    Helpers.MakePrimitiveType("int")
                                )
                            ),
                            Helpers.MakeSeq(
                                Helpers.MakeCallExpression(
                                    Helpers.MakeFunctionIdentifierPath(
                                        "test3",
                                        Helpers.MakePrimitiveType("int"),
                                        Helpers.MakePrimitiveType("int")
                                    ),
                                    Expression.MakeConstant("int", 20)
                                )
                            ),
                            Modifiers.Immutable
                        ),
                        Statement.MakeVarDecl(
                            Helpers.MakeSeq(
                                Helpers.MakePaticularPatternWithType(
                                    "d",
                                    Helpers.MakePrimitiveType("int")
                                )
                            ),
                            Helpers.MakeSeq(
                                Helpers.MakeCallExpression(
                                    Helpers.MakeFunctionIdentifierPath(
                                        "test4",
                                        Helpers.MakePrimitiveType("int"),
                                        Helpers.MakePrimitiveType("int")
                                    ),
                                    Expression.MakeConstant("int", 80)
                                )
                            ),
                            Modifiers.Immutable
                        ),
                        Statement.MakeVarDecl(
                            Helpers.MakeSeq(
                                Helpers.MakePaticularPatternWithType(
                                    "e",
                                    Helpers.MakePrimitiveType("int")
                                )
                            ),
                            Helpers.MakeSeq(
                                Helpers.MakeCallExpression(
                                    Helpers.MakeFunctionIdentifierPath(
                                        "test5",
                                        Helpers.MakePrimitiveType("int"),
                                        Helpers.MakePrimitiveType("int")
                                    )
                                )
                            ),
                            Modifiers.Immutable
                        ),
                        Statement.MakeVarDecl(
                            Helpers.MakeSeq(
                                Helpers.MakePaticularPatternWithType(
                                    "f",
                                    Helpers.MakePrimitiveType("int")
                                )
                            ),
                            Helpers.MakeSeq(
                                Helpers.MakeCallExpression(
                                    Helpers.MakeFunctionIdentifierPath(
                                        "test5",
                                        Helpers.MakePrimitiveType("int"),
                                        Helpers.MakePrimitiveType("int")
                                    ),
                                    Expression.MakeConstant("int", 90)
                                )
                            ),
                            Modifiers.Immutable
                        ),
                        Statement.MakeExprStmt(
                            Helpers.MakeCallExpression(
                                Helpers.SomeWellKnownExpressions.Println,
                                Helpers.MakeCallExpression(
                                    Helpers.SomeWellKnownExpressions.StringFormatN,
                                    Expression.MakeConstant("string", "{0}, {1}, {2}, {3}, {4}, {5}"),
                                    Helpers.MakeIdentifierPath(
                                        "a",
                                        Helpers.MakePrimitiveType("int")
                                    ),
                                    Helpers.MakeIdentifierPath(
                                        "b",
                                        Helpers.MakePrimitiveType("int")
                                    ),
                                    Helpers.MakeIdentifierPath(
                                        "c",
                                        Helpers.MakePrimitiveType("int")
                                    ),
                                    Helpers.MakeIdentifierPath(
                                        "d",
                                        Helpers.MakePrimitiveType("int")
                                    ),
                                    Helpers.MakeIdentifierPath(
                                        "e",
                                        Helpers.MakePrimitiveType("int")
                                    ),
                                    Helpers.MakeIdentifierPath(
                                        "f",
                                        Helpers.MakePrimitiveType("int")
                                    )
                                )
                            )
                        )
                    ),
                    Helpers.MakeVoidType(),
                    Modifiers.None
                )
            });

            Assert.IsNotNull(ast);
            Helpers.AstStructuralEqual(ast, expected_ast);
        }

        [Test]
        public void ComplexExpressions()
        {
            var parser = new Parser(new Scanner("../../sources/for_unit_tests/complex_expressions.exs"));
            parser.DoPostParseProcessing = true;
            parser.Parse();

            var ast = parser.TopmostAst;

            var expected_ast = AstNode.MakeModuleDef("main", new List<EntityDeclaration>{
                EntityDeclaration.MakeFunc(
                    "main",
                    Enumerable.Empty<ParameterDeclaration>(),
                    Statement.MakeBlock(
                        Statement.MakeVarDecl(
                            Helpers.MakeSeq(
                                Helpers.MakePaticularPatternWithType(
                                    "x", 
                                    Helpers.MakeGenericType("vector", Helpers.MakePrimitiveType("int"))
                                )
                            ),
                            Helpers.MakeSeq(
                                Expression.MakeComp(
                                    Helpers.MakeIdentifierPath("x", Helpers.MakePrimitiveType("int")),
                                    Expression.MakeCompFor(
                                        Helpers.MakePaticularPatternWithType(
                                            "x",
                                            Helpers.MakePrimitiveType("int")
                                        ),
                                        Expression.MakeIntSeq(
                                            Expression.MakeConstant("int", 0),
                                            Expression.MakeConstant("int", 100),
                                            Expression.MakeConstant("int", 1),
                                            false
                                        ),
                                        null
                                    ),
                                    Helpers.MakeGenericType("vector", Helpers.MakePrimitiveType("int"))
                                )
                            ),
                            Modifiers.Immutable
                        ),
                        Statement.MakeVarDecl(
                            Helpers.MakeSeq(
                                Helpers.MakePaticularPatternWithType(
                                    "y", 
                                    Helpers.MakeGenericType("vector", Helpers.MakePrimitiveType("int"))
                                )
                            ),
                            Helpers.MakeSeq(
                                Expression.MakeComp(
                                    Helpers.MakeIdentifierPath("x", Helpers.MakePrimitiveType("int")),
                                    Expression.MakeCompFor(
                                        Helpers.MakePaticularPatternWithType(
                                            "x",
                                            Helpers.MakePrimitiveType("int")
                                        ),
                                        Expression.MakeIntSeq(
                                            Expression.MakeConstant("int", 0),
                                            Expression.MakeConstant("int", 100),
                                            Expression.MakeConstant("int", 1),
                                            false
                                        ),
                                        Expression.MakeCompIf(
                                            Expression.MakeBinaryExpr(
                                                OperatorType.Equality,
                                                Expression.MakeBinaryExpr(
                                                    OperatorType.Modulus,
                                                    Helpers.MakeIdentifierPath("x"),
                                                    Expression.MakeConstant("int", 2)
                                                ),
                                                Expression.MakeConstant("int", 0)
                                            ),
                                            null
                                        )
                                    ),
                                    Helpers.MakeGenericType("vector", Helpers.MakePrimitiveType("int"))
                                )
                            ),
                            Modifiers.Immutable
                        ),
                        Statement.MakeVarDecl(
                            Helpers.MakeSeq(
                                Helpers.MakePaticularPatternWithType(
                                    "z", 
                                    Helpers.MakeGenericType(
                                        "vector",
                                        Helpers.MakeGenericType(
                                            "tuple",
                                            Helpers.MakePrimitiveType("int"),
                                            Helpers.MakePrimitiveType("int")
                                        )
                                    )
                                )
                            ),
                            Helpers.MakeSeq(
                                Expression.MakeComp(
                                    Expression.MakeParen(
                                        Expression.MakeSequenceExpression(
                                            Helpers.MakeIdentifierPath("x", Helpers.MakePrimitiveType("int")),
                                            Helpers.MakeIdentifierPath("y", Helpers.MakePrimitiveType("int"))
                                        )
                                    ),
                                    Expression.MakeCompFor(
                                        Helpers.MakePaticularPatternWithType(
                                            "x",
                                            Helpers.MakePrimitiveType("int")
                                        ),
                                        Expression.MakeIntSeq(
                                            Expression.MakeConstant("int", 0),
                                            Expression.MakeConstant("int", 100),
                                            Expression.MakeConstant("int", 1),
                                            false
                                        ),
                                        Expression.MakeCompIf(
                                            Expression.MakeBinaryExpr(
                                                OperatorType.Equality,
                                                Expression.MakeBinaryExpr(
                                                    OperatorType.Modulus,
                                                    Helpers.MakeIdentifierPath("x"),
                                                    Expression.MakeConstant("int", 2)
                                                ),
                                                Expression.MakeConstant("int", 0)
                                            ),
                                            Expression.MakeCompFor(
                                                Helpers.MakePaticularPatternWithType(
                                                    "y",
                                                    Helpers.MakePrimitiveType("int")
                                                ),
                                                Expression.MakeIntSeq(
                                                    Expression.MakeConstant("int", 0),
                                                    Expression.MakeConstant("int", 100),
                                                    Expression.MakeConstant("int", 1),
                                                    false
                                                ),
                                                null
                                            )
                                        )
                                    ),
                                    Helpers.MakeGenericType("vector", Helpers.MakePrimitiveType("int"))
                                )
                            ),
                            Modifiers.Immutable
                        ),
                        Statement.MakeVarDecl(
                            Helpers.MakeSeq(
                                Helpers.MakePaticularPatternWithType(
                                    "triangles", 
                                    Helpers.MakeGenericType(
                                        "vector",
                                        Helpers.MakeGenericType(
                                            "tuple",
                                            Helpers.MakePrimitiveType("int"),
                                            Helpers.MakePrimitiveType("int"),
                                            Helpers.MakePrimitiveType("int")
                                        )
                                    )
                                )
                            ),
                            Helpers.MakeSeq(
                                Expression.MakeComp(
                                    Expression.MakeParen(
                                        Expression.MakeSequenceExpression(
                                            Helpers.MakeIdentifierPath("a", Helpers.MakePrimitiveType("int")),
                                            Helpers.MakeIdentifierPath("b", Helpers.MakePrimitiveType("int")),
                                            Helpers.MakeIdentifierPath("c", Helpers.MakePrimitiveType("int"))
                                        )
                                    ),
                                    Expression.MakeCompFor(
                                        Helpers.MakePaticularPatternWithType(
                                            "c",
                                            Helpers.MakePrimitiveType("int")
                                        ),
                                        Expression.MakeIntSeq(
                                            Expression.MakeConstant("int", 1),
                                            Expression.MakeConstant("int", 10),
                                            Expression.MakeConstant("int", 1),
                                            true
                                        ),
                                        Expression.MakeCompFor(
                                            Helpers.MakePaticularPatternWithType(
                                                "b",
                                                Helpers.MakePrimitiveType("int")
                                            ),
                                            Expression.MakeIntSeq(
                                                Expression.MakeConstant("int", 1),
                                                Helpers.MakeIdentifierPath("c"),
                                                Expression.MakeConstant("int", 1),
                                                true
                                            ),
                                            Expression.MakeCompFor(
                                                Helpers.MakePaticularPatternWithType(
                                                    "a",
                                                    Helpers.MakePrimitiveType("int")
                                                ),
                                                Expression.MakeIntSeq(
                                                    Expression.MakeConstant("int", 1),
                                                    Helpers.MakeIdentifierPath("b"),
                                                    Expression.MakeConstant("int", 1),
                                                    true
                                                ),
                                                Expression.MakeCompIf(
                                                    Expression.MakeBinaryExpr(
                                                        OperatorType.Equality,
                                                        Expression.MakeBinaryExpr(
                                                            OperatorType.Plus,
                                                            Expression.MakeBinaryExpr(
                                                                OperatorType.Power,
                                                                Helpers.MakeIdentifierPath("a"),
                                                                Expression.MakeConstant("int", 2)
                                                            ),
                                                            Expression.MakeBinaryExpr(
                                                                OperatorType.Power,
                                                                Helpers.MakeIdentifierPath("b"),
                                                                Expression.MakeConstant("int", 2)
                                                            )
                                                        ),
                                                        Expression.MakeBinaryExpr(
                                                            OperatorType.Power,
                                                            Helpers.MakeIdentifierPath("c"),
                                                            Expression.MakeConstant("int", 2)
                                                        )
                                                    ),
                                                    null
                                                )
                                            )
                                        )
                                    ),
                                    Helpers.MakeGenericType("vector", Helpers.MakePrimitiveType("int"))
                                )
                            ),
                            Modifiers.Immutable
						),
                        Statement.MakeVarDecl(
                            Helpers.MakeSeq(
                                Helpers.MakePaticularPatternWithType(
                                    "specific_triangles", 
                                    Helpers.MakeGenericType(
                                        "vector",
                                        Helpers.MakeGenericType(
                                            "tuple",
                                            Helpers.MakePrimitiveType("int"),
                                            Helpers.MakePrimitiveType("int"),
                                            Helpers.MakePrimitiveType("int")
                                        )
                                    )
                                )
                            ),
                            Helpers.MakeSeq(
                                Expression.MakeComp(
                                    Expression.MakeParen(
                                        Expression.MakeSequenceExpression(
                                            Helpers.MakeIdentifierPath("a", Helpers.MakePrimitiveType("int")),
                                            Helpers.MakeIdentifierPath("b", Helpers.MakePrimitiveType("int")),
                                            Helpers.MakeIdentifierPath("c", Helpers.MakePrimitiveType("int"))
                                        )
                                    ),
                                    Expression.MakeCompFor(
                                        Helpers.MakePaticularPatternWithType(
                                            "c",
                                            Helpers.MakePrimitiveType("int")
                                        ),
                                        Expression.MakeIntSeq(
                                            Expression.MakeConstant("int", 1),
                                            Expression.MakeConstant("int", 10),
                                            Expression.MakeConstant("int", 1),
                                            true
                                        ),
                                        Expression.MakeCompFor(
                                            Helpers.MakePaticularPatternWithType(
                                                "b",
                                                Helpers.MakePrimitiveType("int")
                                            ),
                                            Expression.MakeIntSeq(
                                                Expression.MakeConstant("int", 1),
                                                Helpers.MakeIdentifierPath("c"),
                                                Expression.MakeConstant("int", 1),
                                                true
                                            ),
                                            Expression.MakeCompFor(
                                                Helpers.MakePaticularPatternWithType(
                                                    "a",
                                                    Helpers.MakePrimitiveType("int")
                                                ),
                                                Expression.MakeIntSeq(
                                                    Expression.MakeConstant("int", 1),
                                                    Helpers.MakeIdentifierPath("b"),
                                                    Expression.MakeConstant("int", 1),
                                                    true
                                                ),
                                                Expression.MakeCompIf(
                                                    Expression.MakeBinaryExpr(
                                                        OperatorType.ConditionalAnd,
                                                        Expression.MakeBinaryExpr(
                                                            OperatorType.Equality,
                                                            Expression.MakeBinaryExpr(
                                                                OperatorType.Plus,
                                                                Expression.MakeBinaryExpr(
                                                                    OperatorType.Power,
                                                                    Helpers.MakeIdentifierPath("a"),
                                                                    Expression.MakeConstant("int", 2)
                                                                ),
                                                                Expression.MakeBinaryExpr(
                                                                    OperatorType.Power,
                                                                    Helpers.MakeIdentifierPath("b"),
                                                                    Expression.MakeConstant("int", 2)
                                                                )
                                                            ),
                                                            Expression.MakeBinaryExpr(
                                                                OperatorType.Power,
                                                                Helpers.MakeIdentifierPath("c"),
                                                                Expression.MakeConstant("int", 2)
                                                            )
                                                        ),
                                                        Expression.MakeBinaryExpr(
                                                            OperatorType.Equality,
                                                            Expression.MakeBinaryExpr(
                                                                OperatorType.Plus,
                                                                Helpers.MakeIdentifierPath("a"),
                                                                Expression.MakeBinaryExpr(
                                                                    OperatorType.Plus,
                                                                    Helpers.MakeIdentifierPath("b"),
                                                                    Helpers.MakeIdentifierPath("c")
                                                                )
                                                            ),
                                                            Expression.MakeConstant("int", 24)
                                                        )
                                                    ),
                                                    null
                                                )
                                            )
                                        )
                                    ),
                                    Helpers.MakeGenericType("vector", Helpers.MakePrimitiveType("int"))
                                )
                            ),
                            Modifiers.Immutable
                        ),
                        Helpers.MakeVariableDeclaration(
                            Helpers.MakeSeq(
                                Helpers.MakePaticularPatternWithType(
                                    "a",
                                    Helpers.MakePrimitiveType("int")
                                ),
                                Helpers.MakePaticularPatternWithType(
                                    "b",
                                    Helpers.MakePrimitiveType("int")
                                ),
                                Helpers.MakePaticularPatternWithType(
                                    "c",
                                    Helpers.MakePrimitiveType("int")
                                )
                            ),
                            Modifiers.None
                        ),
                        Statement.MakeExprStmt(
                            Expression.MakeMultipleAssignment(
                                Expression.MakeMultipleAssignment(
                                    Expression.MakeSingleAssignment(
                                        Helpers.MakeIdentifierPath("a", Helpers.MakePrimitiveType("int")),
                                        Helpers.MakeIdentifierPath("b", Helpers.MakePrimitiveType("int"))
                                    ),
                                    Expression.MakeSequenceExpression(Helpers.MakeIdentifierPath("c", Helpers.MakePrimitiveType("int")))
                                ),
                                Expression.MakeSequenceExpression(Expression.MakeConstant("int", 0))
                            )
                        ),
                        Statement.MakeExprStmt(
                            Helpers.MakeAssignment(
                                Helpers.MakeSeq(
                                    Helpers.MakeIdentifierPath("a", Helpers.MakePrimitiveType("int")),
                                    Helpers.MakeIdentifierPath("b", Helpers.MakePrimitiveType("int")),
                                    Helpers.MakeIdentifierPath("c", Helpers.MakePrimitiveType("int"))
                                ),
                                Helpers.MakeSeq(
                                    Expression.MakeConstant("int", 1),
                                    Expression.MakeConstant("int", 2),
                                    Expression.MakeConstant("int", 3)
                                )
                            )
                        ),
                        Statement.MakeVarDecl(
                            Helpers.MakeSeq(
                                Helpers.MakePaticularPatternWithType(
                                    "vec",
                                    Helpers.MakeGenericType(
                                        "vector",
                                        Helpers.MakeGenericType(
                                            "tuple",
                                            Helpers.MakePrimitiveType("int"),
                                            Helpers.MakePrimitiveType("int")
                                        )
                                    )
                                )
                            ),
                            Helpers.MakeSeq(
                                Expression.MakeSequenceInitializer(
                                    Helpers.MakeGenericType(
                                        "vector",
                                        Helpers.MakeGenericType(
                                            "tuple",
                                            Helpers.MakePrimitiveType("int"),
                                            Helpers.MakePrimitiveType("int")
                                        )
                                    ),
                                    Enumerable.Empty<Expression>()
                                )
                            ),
                            Modifiers.None
                        ),
                        Statement.MakeVarDecl(
                            Helpers.MakeSeq(
                                Helpers.MakePaticularPatternWithType(
                                    "t",
                                    Helpers.MakeGenericType(
                                        "tuple",
                                        Helpers.MakePrimitiveType("int"),
                                        Helpers.MakePrimitiveType("int"),
                                        Helpers.MakePrimitiveType("int")
                                    )
                                )
                            ),
                            Helpers.MakeSeq(
                                Expression.MakeParen(
                                    Expression.MakeSequenceExpression(
                                        Helpers.MakeIdentifierPath("a", Helpers.MakePrimitiveType("int")),
                                        Helpers.MakeIdentifierPath("b", Helpers.MakePrimitiveType("int")),
                                        Helpers.MakeIdentifierPath("c", Helpers.MakePrimitiveType("int"))
                                    )
                                )
                            ),
                            Modifiers.Immutable
                        ),
                        Statement.MakeExprStmt(
                            Helpers.MakeCallExpression(
                                Expression.MakeMemRef(
                                    Helpers.MakeIdentifierPath(
                                        "vec",
                                        Helpers.MakeGenericType(
                                            "vector",
                                            Helpers.MakeGenericType(
                                                "tuple",
                                                Helpers.MakePrimitiveType("int"),
                                                Helpers.MakePrimitiveType("int")
                                            )
                                        )
                                    ),
                                    Helpers.MakeFunctionIdentifier(
                                        "Add",
                                        Helpers.MakeVoidType(),
                                        Helpers.MakeGenericType(
                                            "tuple",
                                            AstType.MakePrimitiveType("int"),
                                            AstType.MakePrimitiveType("int")
                                        )
                                    )
                                ),
                                Expression.MakeParen(
                                    Expression.MakeSequenceExpression(
                                        Helpers.MakeIdentifierPath("a", Helpers.MakePrimitiveType("int")),
                                        Helpers.MakeIdentifierPath("b", Helpers.MakePrimitiveType("int"))
                                    )
                                )
                            )
                        ),
                        Statement.MakeExprStmt(
                            Helpers.MakeCallExpression(
                                Helpers.SomeWellKnownExpressions.Println,
                                Helpers.MakeCallExpression(
                                    Helpers.SomeWellKnownExpressions.StringFormatN,
                                    Expression.MakeConstant("string", "{0}, {1}, {2}, {3}, {4}, {5}, {6}, {7}"),
                                    Helpers.MakeIdentifierPath(
                                        "x",
                                        Helpers.MakeGenericType(
                                            "vector",
                                            Helpers.MakePrimitiveType("int")
                                        )
                                    ),
                                    Helpers.MakeIdentifierPath(
                                        "y",
                                        Helpers.MakeGenericType(
                                            "vector",
                                            Helpers.MakePrimitiveType("int")
                                        )
                                    ),
                                    Helpers.MakeIdentifierPath(
                                        "z",
                                        Helpers.MakeGenericType(
                                            "vector",
                                            Helpers.MakeGenericType(
                                                "tuple",
                                                Helpers.MakePrimitiveType("int"),
                                                Helpers.MakePrimitiveType("int")
                                            )
                                        )
                                    ),
                                    Helpers.MakeIdentifierPath(
                                        "triangles",
                                        Helpers.MakeGenericType(
                                            "vector",
                                            Helpers.MakeGenericType(
                                                "tuple",
                                                Helpers.MakePrimitiveType("int"),
                                                Helpers.MakePrimitiveType("int"),
                                                Helpers.MakePrimitiveType("int")
                                            )
                                        )
                                    ),
                                    Helpers.MakeIdentifierPath(
                                        "specific_triangles",
                                        Helpers.MakeGenericType(
                                            "vector",
                                            Helpers.MakeGenericType(
                                                "tuple",
                                                Helpers.MakePrimitiveType("int"),
                                                Helpers.MakePrimitiveType("int"),
                                                Helpers.MakePrimitiveType("int")
                                            )
                                        )
                                    ),
                                    Helpers.MakeIdentifierPath(
                                        "a",
                                        Helpers.MakePrimitiveType("int")
                                    ),
                                    Helpers.MakeIdentifierPath(
                                        "b",
                                        Helpers.MakePrimitiveType("int")
                                    ),
                                    Helpers.MakeIdentifierPath(
                                        "c",
                                        Helpers.MakePrimitiveType("int")
                                    )
                                )
                            )
                        )
                    ),
                    Helpers.MakeVoidType(),
                    Modifiers.None
                )
            });

            Assert.IsNotNull(ast);
            Helpers.AstStructuralEqual(ast, expected_ast);
        }

        [Test]
        public void GenericParams()
        {
            var parser = new Parser(new Scanner("../../sources/for_unit_tests/generic_params.exs"));
            parser.DoPostParseProcessing = true;
            parser.Parse();

            var ast = parser.TopmostAst;

            var expected_ast = AstNode.MakeModuleDef("main", new List<EntityDeclaration>{
                EntityDeclaration.MakeFunc(
                    "createList", 
                    Helpers.MakeSeq(
                        EntityDeclaration.MakeParameter("a", AstType.MakeParameterType("T")),
                        EntityDeclaration.MakeParameter("b", AstType.MakeParameterType("T")),
                        EntityDeclaration.MakeParameter("c", AstType.MakeParameterType("T"))
                    ),
                    Statement.MakeBlock(
                        Helpers.MakeSingleItemReturnStatement(
                            Helpers.MakeSequenceInitializer(
                                Helpers.MakeGenericType(
                                    "vector",
                                    AstType.MakeParameterType("T")
                                ),
                                Helpers.MakeIdentifierPath(
                                    "a",
                                    AstType.MakeParameterType("T")
                                ),
                                Helpers.MakeIdentifierPath(
                                    "b",
                                    AstType.MakeParameterType("T")
                                ),
                                Helpers.MakeIdentifierPath(
                                    "c",
                                    AstType.MakeParameterType("T")
                                )
                            )
                        )
                    ),
                    Helpers.MakeGenericType("vector", AstType.MakeParameterType("T")),
                    Modifiers.None
                ),
                EntityDeclaration.MakeFunc(
                    "main",
                    Enumerable.Empty<ParameterDeclaration>(),
                    Statement.MakeBlock(
                        Statement.MakeVarDecl(
                            Helpers.MakeSeq(
                                Helpers.MakePaticularPatternWithType(
                                    "a",
                                    Helpers.MakePrimitiveType("int")
                                ),
                                Helpers.MakePaticularPatternWithType(
                                    "b",
                                    Helpers.MakePrimitiveType("int")
                                ),
                                Helpers.MakePaticularPatternWithType(
                                    "c",
                                    Helpers.MakePrimitiveType("int")
                                )
                            ),
                            Helpers.MakeSeq(
                                Expression.MakeConstant("int", 1),
                                Expression.MakeConstant("int", 2),
                                Expression.MakeConstant("int", 3)
                            ),
                            Modifiers.Immutable
                        ),
                        Statement.MakeVarDecl(
                            Helpers.MakeSeq(
                                Helpers.MakePaticularPatternWithType(
                                    "vec",
                                    Helpers.MakeGenericType(
                                        "vector",
                                        Helpers.MakePrimitiveType("int")
                                    )
                                )
                            ),
                            Helpers.MakeSeq(
                                Helpers.MakeCallExpression(
                                    Helpers.MakeFunctionIdentifierPath(
                                        "createList",
                                        Helpers.MakeGenericType(
                                            "vector",
                                            AstType.MakeParameterType("T")
                                        ),
                                        AstType.MakeParameterType("T"),
                                        AstType.MakeParameterType("T"),
                                        AstType.MakeParameterType("T")
                                    ),
                                    Helpers.MakeIdentifierPath(
                                        "a",
                                        Helpers.MakePrimitiveType("int")
                                    ),
                                    Helpers.MakeIdentifierPath(
                                        "b",
                                        Helpers.MakePrimitiveType("int")
                                    ),
                                    Helpers.MakeIdentifierPath(
                                        "c",
                                        Helpers.MakePrimitiveType("int")
                                    )
                                )
                            ),
                            Modifiers.Immutable
                        ),
                        Statement.MakeExprStmt(
                            Helpers.MakeCallExpression(
                                Helpers.SomeWellKnownExpressions.Println,
                                Helpers.MakeCallExpression(
                                    Helpers.SomeWellKnownExpressions.StringFormat1,
                                    Expression.MakeConstant("string", "{0}"),
                                    Helpers.MakeIdentifierPath(
                                        "vec",
                                        Helpers.MakeGenericType(
                                            "vector",
                                            Helpers.MakePrimitiveType("int")
                                        )
                                    )
                                )
                            )
                        )
                    ),
                    Helpers.MakeVoidType(),
                    Modifiers.None
                )
            });

            Assert.IsNotNull(ast);
            Helpers.AstStructuralEqual(ast, expected_ast);
        }

        [Test]
        public void TestModule()
        {
            var parser = new Parser(new Scanner("../../sources/for_unit_tests/test_module.exs"));
            parser.DoPostParseProcessing = true;
            parser.Parse();

            var ast = parser.TopmostAst;

            var expected_ast = AstNode.MakeModuleDef("test_module", new List<EntityDeclaration>{
                EntityDeclaration.MakeClassDecl(
                    "TestClass3",
                    Enumerable.Empty<AstType>(),
                    Helpers.MakeSeq<EntityDeclaration>(
                        EntityDeclaration.MakeField(
                            Helpers.MakeSeq(
                                Helpers.MakeExactPatternWithType(
                                    "x",
                                    Helpers.MakePrimitiveType("int")
                                ),
                                Helpers.MakeExactPatternWithType(
                                    "y",
                                    Helpers.MakePrimitiveType("int")
                                )
                            ),
                            Helpers.MakeSeq(
                                Expression.Null,
                                Expression.Null
                            ),
                            Modifiers.Private | Modifiers.Immutable
                        ),
                        EntityDeclaration.MakeFunc(
                            "getX",
                            Enumerable.Empty<ParameterDeclaration>(),
                            Statement.MakeBlock(
                                Helpers.MakeSingleItemReturnStatement(
                                    Expression.MakeMemRef(
                                        Expression.MakeSelfRef(),
                                        AstNode.MakeIdentifier(
                                            "x",
                                            Helpers.MakePrimitiveType("int")
                                        )
                                    )
                                )
                            ),
                            Helpers.MakePrimitiveType("int"),
                            Modifiers.Public
                        ),
                        EntityDeclaration.MakeFunc(
                            "getY",
                            Enumerable.Empty<ParameterDeclaration>(),
                            Statement.MakeBlock(
                                Helpers.MakeSingleItemReturnStatement(
                                    Expression.MakeMemRef(
                                        Expression.MakeSelfRef(),
                                        AstNode.MakeIdentifier(
                                            "y",
                                            Helpers.MakePrimitiveType("int")
                                        )
                                    )
                                )
                            ),
                            Helpers.MakePrimitiveType("int"),
                            Modifiers.Public
                        )
                    ),
                    Modifiers.Export
                ),
                EntityDeclaration.MakeField(
                    Helpers.MakeSeq(
                        Helpers.MakePaticularPatternWithType(
                            "pair",
                            Helpers.MakeGenericType(
                                "tuple",
                                Helpers.MakePrimitiveType("int"),
                                Helpers.MakePrimitiveType("int")
                            )
                        )
                    ),
                    Helpers.MakeSeq(
                        Expression.MakeParen(
                            Expression.MakeSequenceExpression(
                                Expression.MakeConstant("int", 200),
                                Expression.MakeConstant("int", 300)
                            )
                        )
                    ),
                    Modifiers.Export | Modifiers.Immutable
                ),
                EntityDeclaration.MakeFunc(
                    "createTest",
                    Helpers.MakeSeq(
                        EntityDeclaration.MakeParameter(
                            "x",
                            Helpers.MakePrimitiveType("int")
                        ),
                        EntityDeclaration.MakeParameter(
                            "y",
                            Helpers.MakePrimitiveType("int")
                        )
                    ),
                    Statement.MakeBlock(
                        Helpers.MakeSingleItemReturnStatement(
                            Expression.MakeObjectCreation(
                                Helpers.MakeGenericType("TestClass3"),
                                Helpers.MakeSeq(
                                    AstNode.MakeIdentifier("x"),
                                    AstNode.MakeIdentifier("y")
                                ),
                                Helpers.MakeSeq(
                                    Helpers.MakeIdentifierPath(
                                        "x",
                                        Helpers.MakePrimitiveType("int")
                                    ),
                                    Helpers.MakeIdentifierPath(
                                        "y",
                                        Helpers.MakePrimitiveType("int")
                                    )
                                )
                            )
                        )
                    ),
                    Helpers.MakeGenericType("TestClass3"),
                    Modifiers.Export
                ),
                EntityDeclaration.MakeFunc(
                    "mySin",
                    Helpers.MakeSeq(
                        EntityDeclaration.MakeParameter(
                            "x",
                            Helpers.MakePrimitiveType("double")
                        )
                    ),
                    Statement.MakeBlock(
                        Helpers.MakeSingleItemReturnStatement(
                            Helpers.MakeCallExpression(
                                Expression.MakeMemRef(
                                    Helpers.MakeIdentifierPath(
                                        "Math",
                                        AstType.MakeSimpleType("System.Math")
                                    ),
                                    Helpers.MakeFunctionIdentifier(
                                        "Sin",
                                        Helpers.MakePrimitiveType("double"), 
                                        Helpers.MakePrimitiveType("double")
                                    )
                                ),
                                Helpers.MakeIdentifierPath(
                                    "x",
                                    Helpers.MakePrimitiveType("double")
                                )
                            )
                        )
                    ),
                    Helpers.MakePrimitiveType("double"),
                    Modifiers.Export
                )
            }, Helpers.MakeSeq(
                AstNode.MakeImportDecl(AstNode.MakeIdentifier("System.Math"), "Math")
            ));

            Assert.IsNotNull(ast);
            Helpers.AstStructuralEqual(ast, expected_ast);
        }

        [Test]
        public void Module()
        {
            var parser = new Parser(new Scanner("../../sources/for_unit_tests/module.exs"));
            parser.DoPostParseProcessing = true;
            parser.Parse();

            var ast = parser.TopmostAst;

            var expected_ast = AstNode.MakeModuleDef("main", new List<EntityDeclaration>{
                EntityDeclaration.MakeFunc(
                    "main",
                    Enumerable.Empty<ParameterDeclaration>(),
                    Statement.MakeBlock(
                        Statement.MakeVarDecl(
                            Helpers.MakeSeq(
                                Helpers.MakePaticularPatternWithType(
                                    "a",
                                    AstType.MakeMemberType(
                                        Helpers.MakeGenericType("TestModule"),
                                        Helpers.MakeGenericType("TestClass3")
                                    )
                                )
                            ),
                            Helpers.MakeSeq(
                                Expression.MakeObjectCreation(
                                    AstType.MakeMemberType(
                                        Helpers.MakeGenericType("TestModule"),
                                        Helpers.MakeGenericType("TestClass3")
                                    ),
                                    Helpers.MakeSeq(
                                        AstNode.MakeIdentifier("x"),
                                        AstNode.MakeIdentifier("y")
                                    ),
                                    Helpers.MakeSeq(
                                        Expression.MakeConstant("int", 100),
                                        Expression.MakeConstant("int", 300)
                                    )
                                )
                            ),
                            Modifiers.Immutable
                        ),
                        Statement.MakeVarDecl(
                            Helpers.MakeSeq(
                                Helpers.MakePaticularPatternWithType(
                                    "b",
                                    Helpers.MakeGenericTypeWithRealName(
                                        "TestClass3",
                                        "TestModule::TestClass3"
                                    )
                                )
                            ),
                            Helpers.MakeSeq(
                                Helpers.MakeCallExpression(
                                    Expression.MakePath(
                                        AstNode.MakeIdentifier("TestModule"),
                                        Helpers.MakeFunctionIdentifier(
                                            "createTest",
                                            Helpers.MakeGenericTypeWithRealName(
                                                "TestClass3",
                                                "TestModule::TestClass3"
                                            ),
                                            Helpers.MakePrimitiveType("int"),
                                            Helpers.MakePrimitiveType("int")
                                        )
                                    ),
                                    Expression.MakeConstant("int", 50),
                                    Expression.MakeConstant("int", 100)
                                )
                            ),
                            Modifiers.Immutable
                        ),
                        Statement.MakeVarDecl(
                            Helpers.MakeSeq(
                                Helpers.MakePaticularPatternWithType(
                                    "c",
                                    Helpers.MakeGenericType(
                                        "tuple",
                                        Helpers.MakePrimitiveType("int"),
                                        Helpers.MakePrimitiveType("int")
                                    )
                                )
                            ),
                            Helpers.MakeSeq(
                                Expression.MakePath(
                                    AstNode.MakeIdentifier("TestModule"),
                                    AstNode.MakeIdentifier(
                                        "pair",
                                        Helpers.MakeGenericType(
                                            "tuple",
                                            Helpers.MakePrimitiveType("int"),
                                            Helpers.MakePrimitiveType("int")
                                        )
                                    )
                                )
                            ),
                            Modifiers.Immutable
                        ),
                        Statement.MakeVarDecl(
                            Helpers.MakeSeq(
                                Helpers.MakePaticularPatternWithType(
                                    "d",
                                    Helpers.MakePrimitiveType("double")
                                )
                            ),
                            Helpers.MakeSeq(
                                Helpers.MakeCallExpression(
                                    Expression.MakePath(
                                        AstNode.MakeIdentifier("TestModule"),
                                        Helpers.MakeFunctionIdentifier(
                                            "mySin",
                                            Helpers.MakePrimitiveType("double"),
                                            Helpers.MakePrimitiveType("double")
                                        )
                                    ),
                                    Expression.MakeConstant("double", 0.0)
                                )
                            ),
                            Modifiers.Immutable
                        ),
                        Statement.MakeVarDecl(
                            Helpers.MakeSeq(
                                Helpers.MakePaticularPatternWithType(
                                    "e",
                                    Helpers.MakePrimitiveType("int")
                                )
                            ),
                            Helpers.MakeSeq(
                                Helpers.MakeCallExpression(
                                    Expression.MakeMemRef(
                                        Helpers.MakeIdentifierPath(
                                            "a",
                                            AstType.MakeMemberType(
                                                Helpers.MakeGenericType("TestModule"),
                                                Helpers.MakeGenericType("TestClass3")
                                            )
                                        ),
                                        Helpers.MakeFunctionIdentifier(
                                            "getX",
                                            Helpers.MakePrimitiveType("int")
                                        )
                                    )
                                )
                            ),
                            Modifiers.Immutable
                        ),
                        Statement.MakeExprStmt(
                            Helpers.MakeCallExpression(
                                Helpers.SomeWellKnownExpressions.Println,
                                Helpers.MakeCallExpression(
                                    Helpers.SomeWellKnownExpressions.StringFormatN,
                                    Expression.MakeConstant("string", "{0}, {1}, {2}, {3}, {4}"),
                                    Helpers.MakeIdentifierPath(
                                        "a",
                                        AstType.MakeMemberType(
                                            Helpers.MakeGenericType("TestModule"),
                                            Helpers.MakeGenericType("TestClass3")
                                        )
                                    ),
                                    Helpers.MakeIdentifierPath(
                                        "b",
                                        Helpers.MakeGenericTypeWithRealName(
                                            "TestClass3",
                                            "TestModule::TestClass3"
                                        )
                                    ),
                                    Helpers.MakeIdentifierPath(
                                        "c",
                                        Helpers.MakeGenericType(
                                            "tuple",
                                            Helpers.MakePrimitiveType("int"),
                                            Helpers.MakePrimitiveType("int")
                                        )
                                    ),
                                    Helpers.MakeIdentifierPath(
                                        "d",
                                        Helpers.MakePrimitiveType("double")
                                    ),
                                    Helpers.MakeIdentifierPath(
                                        "e",
                                        Helpers.MakePrimitiveType("int")
                                    )
                                )
                            )
                        )
                    ),
                    Helpers.MakeVoidType(),
                    Modifiers.None
                )
            }, Helpers.MakeSeq(
                AstNode.MakeImportDecl(
                    AstNode.MakeIdentifier(
                        "test_module",
                        Helpers.MakeGenericType("test_module")
                    ),
                    "./test_module.exs",
                    "TestModule"
                )
            ));

            Assert.IsNotNull(ast);
            Helpers.AstStructuralEqual(ast, expected_ast);
        }

        [Test]
        public void Class()
        {
            var parser = new Parser(new Scanner("../../sources/for_unit_tests/class.exs"));
            parser.DoPostParseProcessing = true;
            parser.Parse();

            var ast = parser.TopmostAst;

            var expected_ast = AstNode.MakeModuleDef("main", new List<EntityDeclaration>{
                EntityDeclaration.MakeClassDecl(
                    "TestClass",
                    Enumerable.Empty<AstType>(),
                    Helpers.MakeSeq<EntityDeclaration>(
                        EntityDeclaration.MakeField(
                            Helpers.MakeSeq(
                                Helpers.MakeExactPatternWithType("x", Helpers.MakePrimitiveType("int"))
                            ),
                            Helpers.MakeSeq<Expression>(
                                Expression.Null
                            ),
                            Modifiers.Public | Modifiers.Immutable
                        ),
                        EntityDeclaration.MakeField(
                            Helpers.MakeSeq(
                                Helpers.MakeExactPatternWithType("y", Helpers.MakePrimitiveType("int")),
                                Helpers.MakeExactPatternWithType("z", Helpers.MakePrimitiveType("int"))
                            ),
                            Helpers.MakeSeq(
                                Expression.Null,
                                Expression.Null
                            ),
                            Modifiers.Private | Modifiers.Immutable
                        ),
                        EntityDeclaration.MakeFunc(
                            "getX",
                            Enumerable.Empty<ParameterDeclaration>(),
                            Statement.MakeBlock(
                                Helpers.MakeSingleItemReturnStatement(
                                    Expression.MakeMemRef(
                                        Expression.MakeSelfRef(),
                                        AstNode.MakeIdentifier(
                                            "x",
                                            Helpers.MakePrimitiveType("int")
                                        )
                                    )
                                )
                            ),
                            Helpers.MakePrimitiveType("int"),
                            Modifiers.Public
                        ),
                        EntityDeclaration.MakeFunc(
                            "getY",
                            Enumerable.Empty<ParameterDeclaration>(),
                            Statement.MakeBlock(
                                Helpers.MakeSingleItemReturnStatement(
                                    Expression.MakeMemRef(
                                        Expression.MakeSelfRef(),
                                        AstNode.MakeIdentifier(
                                            "y",
                                            Helpers.MakePrimitiveType("int")
                                        )
                                    )
                                )
                            ),
                            Helpers.MakePrimitiveType("int"),
                            Modifiers.Public
                        ),
                        EntityDeclaration.MakeFunc(
                            "getXPlus",
                            Helpers.MakeSeq(
                                EntityDeclaration.MakeParameter("n", Helpers.MakePrimitiveType("int"))
                            ),
                            Statement.MakeBlock(
                                Helpers.MakeSingleItemReturnStatement(
                                    Expression.MakeBinaryExpr(
                                        OperatorType.Plus,
                                        Helpers.MakeCallExpression(
	                                        Expression.MakeMemRef(
	                                            Expression.MakeSelfRef(),
                                                Helpers.MakeFunctionIdentifier(
	                                                "getX",
                                                    Helpers.MakePrimitiveType("int")
	                                            )
                                            )
                                        ),
                                        Helpers.MakeIdentifierPath(
                                            "n",
                                            Helpers.MakePrimitiveType("int")
                                        )
                                    )
                                )
                            ), 
                            Helpers.MakePrimitiveType("int"),
                            Modifiers.Public
                        ),
                        EntityDeclaration.MakeFunc(
                            "getZ",
                            Enumerable.Empty<ParameterDeclaration>(),
                            Statement.MakeBlock(
                                Helpers.MakeSingleItemReturnStatement(
                                    Expression.MakeMemRef(
                                        Expression.MakeSelfRef(),
                                        AstNode.MakeIdentifier(
                                            "z",
                                            Helpers.MakePrimitiveType("int")
                                        )
                                    )
                                )
                            ),
                            Helpers.MakePrimitiveType("int"),
                            Modifiers.Public
                        )
                    ),
                    Modifiers.None
                ),
                EntityDeclaration.MakeFunc(
                    "main",
                    Enumerable.Empty<ParameterDeclaration>(),
                    Statement.MakeBlock(
                        Statement.MakeVarDecl(
                            Helpers.MakeSeq(
                                Helpers.MakePaticularPatternWithType("a", Helpers.MakeGenericType("TestClass"))
                            ),
                            Helpers.MakeSeq(
                                Expression.MakeObjectCreation(
                                    Helpers.MakeGenericType("TestClass"),
                                    Helpers.MakeSeq(
                                        AstNode.MakeIdentifier("x"),
                                        AstNode.MakeIdentifier("y"),
                                        AstNode.MakeIdentifier("z")
                                    ),
                                    Helpers.MakeSeq(
                                        Expression.MakeConstant("int", 1),
                                        Expression.MakeConstant("int", 2),
                                        Expression.MakeConstant("int", 3)
                                    )
                                )
                            ),
                            Modifiers.Immutable
                        ),
                        /*Statement.MakeVarDecl(
                            Helpers.MakeSeq(
                                AstNode.MakeIdentifier("b", Helpers.MakeGenericType("Test"))
                            ),
                            Helpers.MakeSeq(
                                Expression.MakeNewExpr(
                                    Expression.MakeObjectCreation(
                                        Helpers.MakeGenericType("Test"),
                                        Helpers.MakeSeq(
                                            AstNode.MakeIdentifier("x"),
                                            AstNode.MakeIdentifier("y")
                                        ),
                                        Helpers.MakeSeq(
                                            Expression.MakeConstant("int", 1),
                                            Expression.MakeConstant("int", 3)
                                        )
                                    )
                                )
                            ),
                            Modifiers.Immutable
                        ),*/
                        Statement.MakeVarDecl(
                            Helpers.MakeSeq(
                                Helpers.MakePaticularPatternWithType("c", Helpers.MakePrimitiveType("int"))
                            ),
                            Helpers.MakeSeq(
                                Expression.MakeCallExpr(
                                    Expression.MakeMemRef(
                                        Helpers.MakeIdentifierPath(
                                            "a",
                                            Helpers.MakeGenericType("TestClass")
                                        ),
                                        Helpers.MakeFunctionIdentifier(
                                            "getX",
                                            Helpers.MakePrimitiveType("int")
                                        )
                                    ),
                                    Enumerable.Empty<Expression>()
                                )
                            ),
                            Modifiers.Immutable
                        ),
                        Statement.MakeVarDecl(
                            Helpers.MakeSeq(
                                Helpers.MakePaticularPatternWithType("d", Helpers.MakePrimitiveType("int"))
                            ),
                            Helpers.MakeSeq(
                                Expression.MakeCallExpr(
                                    Expression.MakeMemRef(
                                        Helpers.MakeIdentifierPath(
                                            "a",
                                            Helpers.MakeGenericType("TestClass")
                                        ),
                                        Helpers.MakeFunctionIdentifier(
                                            "getY",
                                            Helpers.MakePrimitiveType("int")
                                        )
                                    ),
                                    Enumerable.Empty<Expression>()
                                )
                            ),
                            Modifiers.Immutable
                        ),
                        Statement.MakeVarDecl(
                            Helpers.MakeSeq(
                                Helpers.MakePaticularPatternWithType("e", Helpers.MakePrimitiveType("int"))
                            ),
                            Helpers.MakeSeq(
                                Helpers.MakeCallExpression(
                                    Expression.MakeMemRef(
                                        Helpers.MakeIdentifierPath(
                                            "a",
                                            Helpers.MakeGenericType("TestClass")
                                        ),
                                        Helpers.MakeFunctionIdentifier(
                                            "getXPlus",
                                            Helpers.MakePrimitiveType("int"),
                                            Helpers.MakePrimitiveType("int")
                                        )
                                    ),
                                    Expression.MakeConstant("int", 100)
                                )
                            ),
                            Modifiers.Immutable
                        ),
                        Statement.MakeVarDecl(
                            Helpers.MakeSeq(
                                Helpers.MakePaticularPatternWithType("f", Helpers.MakePrimitiveType("int"))
                            ),
                            Helpers.MakeSeq(
                                Helpers.MakeCallExpression(
                                    Expression.MakeMemRef(
                                        Helpers.MakeIdentifierPath(
                                            "a",
                                            Helpers.MakeGenericType("TestClass")
                                        ),
                                        Helpers.MakeFunctionIdentifier(
                                            "getZ",
                                            Helpers.MakePrimitiveType("int")
                                        )
                                    )
                                )
                            ),
                            Modifiers.Immutable
                        ),
                        Statement.MakeVarDecl(
                            Helpers.MakeSeq(
                                Helpers.MakePaticularPatternWithType("g", Helpers.MakePrimitiveType("int"))
                            ),
                            Helpers.MakeSeq(
                                Expression.MakeMemRef(
                                    Helpers.MakeIdentifierPath(
                                        "a",
                                        Helpers.MakeGenericType("TestClass")
                                    ),
                                    AstNode.MakeIdentifier(
                                        "x",
                                        Helpers.MakePrimitiveType("int")
                                    )
                                )
                            ),
                            Modifiers.Immutable
                        ),
                        Statement.MakeExprStmt(
                            Helpers.MakeCallExpression(
                                Helpers.SomeWellKnownExpressions.Println,
                                Helpers.MakeCallExpression(
                                    Helpers.SomeWellKnownExpressions.StringFormatN,
                                    Expression.MakeConstant("string", "(a.x, a.y, a.z, x) = ({0}, {1}, {2}, {3})"),
                                    Helpers.MakeIdentifierPath(
                                        "c",
                                        Helpers.MakePrimitiveType("int")
                                    ),
                                    Helpers.MakeIdentifierPath(
                                        "d",
                                        Helpers.MakePrimitiveType("int")
                                    ),
                                    Helpers.MakeIdentifierPath(
                                        "f",
                                        Helpers.MakePrimitiveType("int")
                                    ),
                                    Helpers.MakeIdentifierPath(
                                        "g",
                                        Helpers.MakePrimitiveType("int")
                                    )
                                )
                            )
                        )
                    ),
                    Helpers.MakeVoidType(),
                    Modifiers.None
                )
            });

            Assert.IsNotNull(ast);
            Helpers.AstStructuralEqual(ast, expected_ast);
        }

        [Test]
        public void Closures()
        {
            var parser = new Parser(new Scanner("../../sources/for_unit_tests/closures.exs"));
            parser.DoPostParseProcessing = true;
            parser.Parse();

            var ast = parser.TopmostAst;

            var expected = AstNode.MakeModuleDef("main", new List<EntityDeclaration>{
                EntityDeclaration.MakeFunc(
                    "addOneToOne",
                    Helpers.MakeSeq(
                        EntityDeclaration.MakeParameter(
                            "addOne",
                            Helpers.MakeFunctionType(
                                "closure",
                                Helpers.MakePrimitiveType("int"),
                                Helpers.MakePrimitiveType("int")
                            )
                        )
                    ),
                    Statement.MakeBlock(
                        Helpers.MakeSingleItemReturnStatement(
                            Helpers.MakeCallExpression(
                                Helpers.MakeIdentifierPath(
                                    "addOne",
                                    Helpers.MakeFunctionType(
                                        "closure",
                                        Helpers.MakePrimitiveType("int"),
                                        Helpers.MakePrimitiveType("int")
                                    )
                                ),
                                Expression.MakeConstant("int", 1)
                            )
                        )
                    ),
                    Helpers.MakePrimitiveType("int"),
                    Modifiers.None
                ),
                EntityDeclaration.MakeFunc(
                    "main",
                    Enumerable.Empty<ParameterDeclaration>(),
                    Statement.MakeBlock(
                        Statement.MakeVarDecl(
                            Helpers.MakeSeq(
                                Helpers.MakePaticularPatternWithType(
                                    "c",
                                    Helpers.MakeFunctionType(
                                        "closure",
                                        Helpers.MakePrimitiveType("int"),
                                        Helpers.MakePrimitiveType("int")
                                    )
                                )
                            ),
                            Helpers.MakeSeq<Expression>(
                                Expression.MakeClosureExpression(
                                    Helpers.MakePrimitiveType("int"),
                                    Statement.MakeBlock(
                                        Helpers.MakeSingleItemReturnStatement(
                                            Expression.MakeBinaryExpr(
                                                OperatorType.Plus,
                                                Helpers.MakeIdentifierPath(
                                                    "x",
                                                    Helpers.MakePrimitiveType("int")
                                                ),
                                                Expression.MakeConstant("int", 1)
                                            )
                                        )
                                    ),
                                    liftedIdentifiers: new List<Identifier>(),
                                    parameters: EntityDeclaration.MakeParameter(
                                        "x",
                                        Helpers.MakePrimitiveType("int")
                                    )
                                )
                            ),
                            Modifiers.Immutable
                        ),
                        Statement.MakeVarDecl(
                            Helpers.MakeSeq(
                                Helpers.MakePaticularPatternWithType(
                                    "c2",
                                    Helpers.MakeFunctionType(
                                        "closure",
                                        Helpers.MakePrimitiveType("int"),
                                        Helpers.MakePrimitiveType("int")
                                    )
                                )
                            ),
                            Helpers.MakeSeq<Expression>(
                                Expression.MakeClosureExpression(
                                    Helpers.MakePrimitiveType("int"),
                                    Statement.MakeBlock(
                                        Statement.MakeVarDecl(
                                            Helpers.MakeSeq(
                                                Helpers.MakePaticularPatternWithType(
                                                    "y",
                                                    Helpers.MakePrimitiveType("int")
                                                )
                                            ),
                                            Helpers.MakeSeq(
                                                Expression.MakeConstant("int", 1)
                                            ),
                                            Modifiers.Immutable
                                        ),
                                        Helpers.MakeSingleItemReturnStatement(
                                            Expression.MakeBinaryExpr(
                                                OperatorType.Plus,
                                                Helpers.MakeIdentifierPath(
                                                    "x",
                                                    Helpers.MakePrimitiveType("int")
                                                ),
                                                Helpers.MakeIdentifierPath(
                                                    "y",
                                                    Helpers.MakePrimitiveType("int")
                                                )
                                            )
                                        )
                                    ),
                                    liftedIdentifiers: new List<Identifier>{},
                                    parameters: EntityDeclaration.MakeParameter(
                                        "x",
                                        Helpers.MakePrimitiveType("int")
                                    )
                                )
                            ),
                            Modifiers.Immutable
                        ),
                        Statement.MakeVarDecl(
                            Helpers.MakeSeq(
                                Helpers.MakePaticularPatternWithType(
                                    "a",
                                    Helpers.MakePrimitiveType("int")
                                )
                            ),
                            Helpers.MakeSeq(
                                Helpers.MakeCallExpression(
                                    Helpers.MakeIdentifierPath(
                                        "c",
                                        Helpers.MakeFunctionType(
                                            "closure",
                                            Helpers.MakePrimitiveType("int"),
                                            Helpers.MakePrimitiveType("int")
                                        )
                                    ),
                                    Expression.MakeConstant("int", 1)
                                )
                            ),
                            Modifiers.Immutable
                        ),
                        Statement.MakeVarDecl(
                            Helpers.MakeSeq(
                                Helpers.MakePaticularPatternWithType(
                                    "b",
                                    Helpers.MakePrimitiveType("int")
                                )
                            ),
                            Helpers.MakeSeq(
                                Helpers.MakeCallExpression(
                                    Helpers.MakeIdentifierPath(
                                        "c2",
                                        Helpers.MakeFunctionType(
                                            "closure",
                                            Helpers.MakePrimitiveType("int"),
                                            Helpers.MakePrimitiveType("int")
                                        )
                                    ),
                                    Expression.MakeConstant("int", 1)
                                )
                            ),
                            Modifiers.Immutable
                        ),
                        Statement.MakeVarDecl(
                            Helpers.MakeSeq(
                                Helpers.MakePaticularPatternWithType(
                                    "d",
                                    Helpers.MakePrimitiveType("int")
                                )
                            ),
                            Helpers.MakeSeq(
                                Helpers.MakeCallExpression(
                                    Helpers.MakeFunctionIdentifierPath(
                                        "addOneToOne",
                                        Helpers.MakePrimitiveType("int"),
                                        Helpers.MakeFunctionType(
                                            "closure",
                                            Helpers.MakePrimitiveType("int"),
                                            Helpers.MakePrimitiveType("int")
                                        )
                                    ),
                                    Expression.MakeClosureExpression(
                                        Helpers.MakePrimitiveType("int"),
                                        Statement.MakeBlock(
                                            Helpers.MakeSingleItemReturnStatement(
                                                Expression.MakeBinaryExpr(
                                                    OperatorType.Plus,
                                                    Helpers.MakeIdentifierPath(
                                                        "x",
                                                        Helpers.MakePrimitiveType("int")
                                                    ),
                                                    Expression.MakeConstant("int", 1)
                                                )
                                            )
                                        ),
                                        liftedIdentifiers: new List<Identifier>{},
                                        parameters: EntityDeclaration.MakeParameter(
                                            "x",
                                            Helpers.MakePrimitiveType("int")
                                        )
                                    )
                                )
                            ),
                            Modifiers.Immutable
                        ),
                        Statement.MakeVarDecl(
                            Helpers.MakeSeq(
                                Helpers.MakePaticularPatternWithType(
                                    "e",
                                    Helpers.MakePrimitiveType("int")
                                )
                            ),
                            Helpers.MakeSeq(
                                Expression.MakeConstant("int", 2)
                            ),
                            Modifiers.Immutable
                        ),
                        Statement.MakeVarDecl(
                            Helpers.MakeSeq(
                                Helpers.MakePaticularPatternWithType(
                                    "c3",
                                    Helpers.MakeFunctionType(
                                        "closure",
                                        Helpers.MakePrimitiveType("int"),
                                        Helpers.MakePrimitiveType("int")
                                    )
                                )
                            ),
                            Helpers.MakeSeq<Expression>(
                                Expression.MakeClosureExpression(
                                    Helpers.MakePrimitiveType("int"),
                                    Statement.MakeBlock(
                                        Helpers.MakeSingleItemReturnStatement(
                                            Expression.MakeBinaryExpr(
                                                OperatorType.Plus,
                                                Helpers.MakeIdentifierPath(
                                                    "x",
                                                    Helpers.MakePrimitiveType("int")
                                                ),
                                                Helpers.MakeIdentifierPath(
                                                    "e", 
                                                    Helpers.MakePrimitiveType("int")
                                                )
                                            )
                                        )
                                    ),
                                    liftedIdentifiers: new List<Identifier>{
                                        AstNode.MakeIdentifier(
                                            "e",
                                            Helpers.MakePrimitiveType("int")
                                        )
                                    },
                                    parameters: EntityDeclaration.MakeParameter(
                                        "x",
                                        Helpers.MakePrimitiveType("int")
                                    )
                                )
                            ),
                            Modifiers.Immutable
                        ),
                        Statement.MakeVarDecl(
                            Helpers.MakeSeq(
                                Helpers.MakePaticularPatternWithType(
                                    "f",
                                    Helpers.MakePrimitiveType("int")
                                )
                            ),
                            Helpers.MakeSeq(
                                Helpers.MakeCallExpression(
                                    Helpers.MakeIdentifierPath(
                                        "c3",
                                        Helpers.MakeFunctionType(
                                            "closure",
                                            Helpers.MakePrimitiveType("int"),
                                            Helpers.MakePrimitiveType("int")
                                        )
                                    ),
                                    Expression.MakeConstant("int", 1)
                                )
                            ),
                            Modifiers.Immutable
                        ),
                        Statement.MakeExprStmt(
                            Helpers.MakeCallExpression(
                                Helpers.SomeWellKnownExpressions.Println,
                                Helpers.MakeCallExpression(
                                    Helpers.SomeWellKnownExpressions.StringFormatN,
                                    Expression.MakeConstant("string", "{0}, {1}, {2}, {3}"),
                                    Helpers.MakeIdentifierPath(
                                        "a",
                                        Helpers.MakePrimitiveType("int")
                                    ),
                                    Helpers.MakeIdentifierPath(
                                        "b",
                                        Helpers.MakePrimitiveType("int")
                                    ),
                                    Helpers.MakeIdentifierPath(
                                        "d",
                                        Helpers.MakePrimitiveType("int")
                                    ),
                                    Helpers.MakeIdentifierPath(
                                        "f",
                                        Helpers.MakePrimitiveType("int")
                                    )
                                )
                            )
                        )
                    ),
                    Helpers.MakeVoidType(),
                    Modifiers.None
                )
            });

            Assert.IsNotNull(ast);
            Helpers.AstStructuralEqual(ast, expected);
        }

        [Test]
        public void ClosuresWithCompoundStatements()
        {
            var parser = new Parser(new Scanner("../../sources/for_unit_tests/closures_with_compound_statements.exs"));
            parser.DoPostParseProcessing = true;
            parser.Parse();

            var ast = parser.TopmostAst;

            var expected = AstNode.MakeModuleDef("main", new List<EntityDeclaration>{
                EntityDeclaration.MakeFunc(
                    "main",
                    Enumerable.Empty<ParameterDeclaration>(),
                    Statement.MakeBlock(
                        Statement.MakeVarDecl(
                            Helpers.MakeSeq(
	                            Helpers.MakePaticularPatternWithType(
	                                "c",
                                    Helpers.MakeFunctionType(
	                                    "closure",
	                                    Helpers.MakePrimitiveType("int"),
	                                    Helpers.MakePrimitiveType("bool")
	                                )
	                            )
                            ),
                            Helpers.MakeSeq(
                                Expression.MakeClosureExpression(
	                                Helpers.MakePrimitiveType("int"),
	                                Statement.MakeBlock(
	                                    Statement.MakeIfStmt(
	                                        PatternConstruct.MakeExpressionPattern(
	                                            Helpers.MakeIdentifierPath(
	                                                "f",
	                                                Helpers.MakePrimitiveType("bool")
	                                            )
	                                        ),
	                                        Statement.MakeBlock(
	                                            Helpers.MakeSingleItemReturnStatement(
	                                                Expression.MakeConstant("int", 1)
	                                            )
	                                        ),
	                                        Statement.MakeBlock(
	                                            Helpers.MakeSingleItemReturnStatement(
	                                                Expression.MakeConstant("int", 0)
	                                            )
	                                        )
	                                    )
	                                ),
                                    liftedIdentifiers: new List<Identifier>(),
                                    parameters: EntityDeclaration.MakeParameter(
	                                    "f",
	                                    Helpers.MakePrimitiveType("bool")
	                                )
                                )
                            ),
                            Modifiers.Immutable
                        ),
                        Statement.MakeVarDecl(
                            Helpers.MakeSeq(
                                Helpers.MakePaticularPatternWithType(
                                    "a",
                                    Helpers.MakePrimitiveType("int")
                                )
                            ),
                            Helpers.MakeSeq(
                                Helpers.MakeCallExpression(
                                    Helpers.MakeIdentifierPath(
                                        "c",
                                        Helpers.MakeFunctionType(
                                            "closure",
                                            Helpers.MakePrimitiveType("int"),
                                            Helpers.MakePrimitiveType("bool")
                                        )
                                    ),
                                    Expression.MakeConstant("bool", true)
                                )
                            ),
                            Modifiers.Immutable
                        ),
                        Statement.MakeVarDecl(
                            Helpers.MakeSeq(
                                Helpers.MakePaticularPatternWithType(
                                    "c2",
                                    Helpers.MakeFunctionType(
                                        "closure",
                                        Helpers.MakePrimitiveType("int"),
                                        Helpers.MakePrimitiveType("int")
                                    )
                                )
                            ),
                            Helpers.MakeSeq(
                                Expression.MakeClosureExpression(
                                    Helpers.MakePrimitiveType("int"),
                                    Statement.MakeBlock(
                                        Statement.MakeVarDecl(
                                            Helpers.MakeSeq(
                                                Helpers.MakePaticularPatternWithType(
                                                    "result",
                                                    Helpers.MakePrimitiveType("int")
                                                )
                                            ),
                                            Helpers.MakeSeq(
                                                Expression.MakeConstant("int", 0)
                                            ),
                                            Modifiers.None
                                        ),
                                        Statement.MakeValueBindingForStmt(
                                            Modifiers.Immutable,
                                            Helpers.MakePaticularPatternWithType(
                                                "j",
                                                Helpers.MakePrimitiveType("int")
                                            ),
                                            Expression.MakeIntSeq(
                                                Expression.MakeConstant("int", 0),
                                                Helpers.MakeIdentifierPath(
                                                    "i",
                                                    Helpers.MakePrimitiveType("int")
                                                ),
                                                Expression.MakeConstant("int", 1),
                                                false
                                            ),
                                            Statement.MakeBlock(
                                                Statement.MakeExprStmt(
                                                    Expression.MakeSingleAugmentedAssignment(
                                                        OperatorType.Plus,
                                                        Helpers.MakeIdentifierPath(
                                                            "result",
                                                            Helpers.MakePrimitiveType("int")
                                                        ),
                                                        Helpers.MakeIdentifierPath(
                                                            "j",
                                                            Helpers.MakePrimitiveType("int")
                                                        )
                                                    )
                                                )
                                            )
                                        ),
                                        Helpers.MakeSingleItemReturnStatement(
                                            Helpers.MakeIdentifierPath(
                                                "result",
                                                Helpers.MakePrimitiveType("int")
                                            )
                                        )
                                    ),
                                    liftedIdentifiers: new List<Identifier>(),
                                    parameters: EntityDeclaration.MakeParameter(
                                        "i",
                                        Helpers.MakePrimitiveType("int")
                                    )
                                )
                            ),
                            Modifiers.Immutable
                        ),
                        Statement.MakeVarDecl(
                            Helpers.MakeSeq(
                                Helpers.MakePaticularPatternWithType(
                                    "b",
                                    Helpers.MakePrimitiveType("int")
                                )
                            ),
                            Helpers.MakeSeq(
                                Helpers.MakeCallExpression(
                                    Helpers.MakeIdentifierPath(
                                        "c2",
                                        Helpers.MakeFunctionType(
                                            "closure",
                                            Helpers.MakePrimitiveType("int"),
                                            Helpers.MakePrimitiveType("int")
                                        )
                                    ),
                                    Expression.MakeConstant("int", 10)
                                )
                            ),
                            Modifiers.Immutable
                        ),
                        Statement.MakeVarDecl(
                            Helpers.MakeSeq(
                                Helpers.MakePaticularPatternWithType(
                                    "c3",
                                    Helpers.MakeFunctionType(
                                        "closure",
                                        Helpers.MakeVoidType(),
                                        Helpers.MakePrimitiveType("int")
                                    )
                                )
                            ),
                            Helpers.MakeSeq(
                                Expression.MakeClosureExpression(
                                    Helpers.MakeVoidType(),
                                    Statement.MakeBlock(
                                        Statement.MakeVarDecl(
                                            Helpers.MakeSeq(
                                                Helpers.MakePaticularPatternWithType(
                                                    "j",
                                                    Helpers.MakePrimitiveType("int")
                                                )
                                            ),
                                            Helpers.MakeSeq(
                                                Helpers.MakeIdentifierPath(
                                                    "i",
                                                    Helpers.MakePrimitiveType("int")
                                                )
                                            ),
                                            Modifiers.None
                                        ),
                                        Statement.MakeWhileStmt(
                                            Expression.MakeBinaryExpr(
                                                OperatorType.GreaterThan,
                                                Helpers.MakeIdentifierPath(
                                                    "j",
                                                    Helpers.MakePrimitiveType("int")
                                                ),
                                                Expression.MakeConstant("int", 0)
                                            ),
                                            Statement.MakeBlock(
                                                Statement.MakeExprStmt(
                                                    Helpers.MakeCallExpression(
                                                        Helpers.SomeWellKnownExpressions.Println,
                                                        Helpers.MakeCallExpression(
                                                            Helpers.SomeWellKnownExpressions.StringFormat1,
                                                            Expression.MakeConstant("string", "{0}"),
                                                            Helpers.MakeIdentifierPath(
                                                                "j",
                                                                Helpers.MakePrimitiveType("int")
                                                            )
                                                        )
                                                    )
                                                ),
                                                Statement.MakeExprStmt(
                                                    Expression.MakeSingleAugmentedAssignment(
                                                        OperatorType.Minus,
                                                        Helpers.MakeIdentifierPath(
                                                            "j",
                                                            Helpers.MakePrimitiveType("int")
                                                        ),
                                                        Expression.MakeConstant("int", 1)
                                                    )
                                                )
                                            )
                                        ),
                                        Statement.MakeExprStmt(
                                            Helpers.MakeCallExpression(
                                                Helpers.SomeWellKnownExpressions.Println,
                                                Expression.MakeConstant("string", "BOOM!")
                                            )
                                        )
                                    ),
                                    liftedIdentifiers: new List<Identifier>(),
                                    parameters: EntityDeclaration.MakeParameter(
                                        "i",
                                        Helpers.MakePrimitiveType("int")
                                    )
                                )
                            ),
                            Modifiers.Immutable
                        ),
                        Statement.MakeExprStmt(
                            Helpers.MakeCallExpression(
                                Helpers.MakeIdentifierPath(
                                    "c3",
                                    Helpers.MakeFunctionType(
                                        "closure",
                                        Helpers.MakeVoidType(),
                                        Helpers.MakePrimitiveType("int")
                                    )
                                ),
                                Expression.MakeConstant("int", 3)
                            )
                        ),
                        Statement.MakeExprStmt(
                            Helpers.MakeCallExpression(
                                Helpers.SomeWellKnownExpressions.Println,
                                Helpers.MakeCallExpression(
                                    Helpers.SomeWellKnownExpressions.StringFormat2,
                                    Expression.MakeConstant("string", "{0}, {1}"),
                                    Helpers.MakeIdentifierPath(
                                        "a",
                                        Helpers.MakePrimitiveType("int")
                                    ),
                                    Helpers.MakeIdentifierPath(
                                        "b",
                                        Helpers.MakePrimitiveType("int")
                                    )
                                )
                            )
                        )
                    ),
                    Helpers.MakeVoidType(),
                    Modifiers.None
                )
            });

            Assert.IsNotNull(ast);
            Helpers.AstStructuralEqual(ast, expected);
        }

        [Test]
        public void TryStatements()
        {
            var parser = new Parser(new Scanner("../../sources/for_unit_tests/try_statements.exs"));
            parser.DoPostParseProcessing = true;
            parser.Parse();

            var ast = parser.TopmostAst;

            var expected = AstNode.MakeModuleDef("main", new List<EntityDeclaration>{
                EntityDeclaration.MakeFunc(
                    "throwException",
                    Enumerable.Empty<ParameterDeclaration>(),
                    Statement.MakeBlock(
                        Statement.MakeThrowStmt(
                            Expression.MakeObjectCreation(
                                Helpers.MakeGenericTypeWithRealName(
                                    "Exception",
                                    "System.Exception"
                                ),
                                Helpers.MakeSeq(
                                    AstNode.MakeIdentifier("message")
                                ),
                                Helpers.MakeSeq(
                                    Expression.MakeConstant("string", "An unknown error has occurred")
                                )
                            )
                        )
                    ),
                    Helpers.MakeVoidType(),
                    Modifiers.None
                ),
                EntityDeclaration.MakeFunc(
                    "main",
                    Enumerable.Empty<ParameterDeclaration>(),
                    Statement.MakeBlock(
                        Helpers.MakeTryStmt(
                            Statement.MakeBlock(
                                Statement.MakeExprStmt(
                                    Helpers.MakeCallExpression(
                                        Helpers.SomeWellKnownExpressions.Println,
                                        Expression.MakeConstant("string", "First try block")
                                    )
                                ),
                                Statement.MakeExprStmt(
                                    Helpers.MakeCallExpression(
                                        Helpers.MakeFunctionIdentifierPath(
                                            "throwException",
                                            Helpers.MakeVoidType()
                                        )
                                    )
                                )
                            ),
                            null,
                            Statement.MakeCatchClause(
                                AstNode.MakeIdentifier(
                                    "e",
                                    Helpers.MakeGenericTypeWithRealName(
                                        "Exception",
                                        "System.Exception"
                                    )
                                ),
                                Statement.MakeBlock(
                                    Statement.MakeExprStmt(
                                        Helpers.MakeCallExpression(
                                            Helpers.SomeWellKnownExpressions.Println,
                                            Expression.MakeConstant("string", "First catch block")
                                        )
                                    ),
                                    Statement.MakeExprStmt(
                                        Helpers.MakeCallExpression(
                                            Helpers.SomeWellKnownExpressions.Println,
                                            Expression.MakeMemRef(
                                                Helpers.MakeIdentifierPath(
                                                    "e",
                                                    Helpers.MakeGenericTypeWithRealName(
                                                        "Exception",
                                                        "System.Exception"
                                                    )
                                                ),
                                                AstNode.MakeIdentifier(
                                                    "Message",
                                                    Helpers.MakePropertyGetterType(
                                                        "Message",
                                                        Helpers.MakePrimitiveType("string")
                                                    )
                                                )
                                            )
                                        )
                                    )
                                )
                            )
                        ),
                        /*Statement.MakeVarDecl(
                            Helpers.MakeSeq(
                                AstNode.MakeIdentifier(
                                    "tmp",
                                    Helpers.MakePrimitiveType("int")
                                )
                            ),
                            Helpers.MakeSeq(
                                Expression.MakeConstant("int", 1)
                            ),
                            Modifiers.None
                        ),
                        Statement.MakeTryStmt(
                            Statement.MakeBlock(
                                Statement.MakeExprStmt(
                                    Helpers.MakeCallExpression(
                                        Helpers.SomeWellKnownExpressions.Println,
                                        Helpers.MakeCallExpression(
                                            Helpers.SomeWellKnownExpressions.StringFormat1,
                                            Expression.MakeConstant("string", "tmp is {0} at first"),
                                            Helpers.MakeIdentifierPath(
                                                "tmp",
                                                Helpers.MakePrimitiveType("int")
                                            )
                                        )
                                    )
                                ),
                                Statement.MakeExprStmt(
                                    Helpers.MakeCallExpression(
                                        Helpers.MakeIdentifierPath(
                                            "throwException",
                                            AstType.MakeFunctionType(
                                                "throwException",
                                                Helpers.MakeVoidType(),
                                                Enumerable.Empty<AstType>()
                                            )
                                        )
                                    )
                                )
                            ),
                            null,
                            Statement.MakeFinallyClause(
                                Statement.MakeBlock(
                                    Statement.MakeExprStmt(
                                        Helpers.MakeCallExpression(
                                            Helpers.SomeWellKnownExpressions.Println,
                                            Expression.MakeConstant("string", "First finally block")
                                        )
                                    ),
                                    Statement.MakeExprStmt(
                                        Expression.MakeSingleAssignment(
                                            Helpers.MakeIdentifierPath(
                                                "tmp",
                                                Helpers.MakePrimitiveType("int")
                                            ), 
                                            Expression.MakeConstant("int", 2)
                                        )
                                    )
                                )
                            )
                        ),
                        Statement.MakeExprStmt(
                            Helpers.MakeCallExpression(
                                Helpers.SomeWellKnownExpressions.Println,
                                Helpers.MakeCallExpression(
                                    Helpers.SomeWellKnownExpressions.StringFormat1,
                                    Expression.MakeConstant("string", "tmp is {0} at last"),
                                    Helpers.MakeIdentifierPath(
                                        "tmp",
                                        Helpers.MakePrimitiveType("int")
                                    )
                                )
                            )
                        ),*/
                        Statement.MakeVarDecl(
                            Helpers.MakeSeq(
                                Helpers.MakePaticularPatternWithType(
                                    "tmp2",
                                    Helpers.MakePrimitiveType("int")
                                )
                            ),
                            Helpers.MakeSeq(
                                Expression.MakeConstant("int", 1)
                            ),
                            Modifiers.None
                        ),
                        Helpers.MakeTryStmt(
                            Statement.MakeBlock(
                                Statement.MakeExprStmt(
                                    Helpers.MakeCallExpression(
                                        Helpers.SomeWellKnownExpressions.Println,
                                        Helpers.MakeCallExpression(
                                            Helpers.SomeWellKnownExpressions.StringFormat1,
                                            Expression.MakeConstant("string", "tmp2 is {0} at first"),
                                            Helpers.MakeIdentifierPath(
                                                "tmp2",
                                                Helpers.MakePrimitiveType("int")
                                            )
                                        )
                                    )
                                ),
                                Statement.MakeExprStmt(
                                    Helpers.MakeCallExpression(
                                        Helpers.MakeFunctionIdentifierPath(
                                            "throwException",
                                            Helpers.MakeVoidType()
                                        )
                                    )
                                )
                            ),
                            Statement.MakeFinallyClause(
                                Statement.MakeBlock(
                                    Statement.MakeExprStmt(
                                        Helpers.MakeCallExpression(
                                            Helpers.SomeWellKnownExpressions.Println,
                                            Expression.MakeConstant("string", "Second finally block")
                                        )
                                    ),
                                    Statement.MakeExprStmt(
                                        Expression.MakeSingleAssignment(
                                            Helpers.MakeIdentifierPath(
                                                "tmp2",
                                                Helpers.MakePrimitiveType("int")
                                            ), 
                                            Expression.MakeConstant("int", 3)
                                        )
                                    )
                                )
                            ),
                            Statement.MakeCatchClause(
                                AstNode.MakeIdentifier(
                                    "e",
                                    Helpers.MakeGenericTypeWithRealName(
                                        "Exception",
                                        "System.Exception"
                                    )
                                ),
                                Statement.MakeBlock(
                                    Statement.MakeExprStmt(
                                        Helpers.MakeCallExpression(
                                            Helpers.SomeWellKnownExpressions.Println,
                                            Expression.MakeConstant("string", "Second catch block")
                                        )
                                    ),
                                    Statement.MakeExprStmt(
                                        Expression.MakeSingleAssignment(
                                            Helpers.MakeIdentifierPath(
                                                "tmp2",
                                                Helpers.MakePrimitiveType("int")
                                            ),
                                            Expression.MakeConstant("int", 2)
                                        )
                                    ),
                                    Statement.MakeExprStmt(
                                        Helpers.MakeCallExpression(
                                            Helpers.SomeWellKnownExpressions.Println,
                                            Expression.MakeMemRef(
                                                Helpers.MakeIdentifierPath(
                                                    "e",
                                                    Helpers.MakeGenericTypeWithRealName(
                                                        "Exception",
                                                        "System.Exception"
                                                    )
                                                ),
                                                AstNode.MakeIdentifier(
                                                    "Message",
                                                    Helpers.MakePropertyGetterType(
                                                        "Message",
                                                        Helpers.MakePrimitiveType("string")
                                                    )
                                                )
                                            )
                                        )
                                    )
                                )
                            )
                        ),
                        Statement.MakeExprStmt(
                            Helpers.MakeCallExpression(
                                Helpers.SomeWellKnownExpressions.Println,
                                Helpers.MakeCallExpression(
                                    Helpers.SomeWellKnownExpressions.StringFormat1,
                                    Expression.MakeConstant("string", "tmp2 is {0} at last"),
                                    Helpers.MakeIdentifierPath(
                                        "tmp2",
                                        Helpers.MakePrimitiveType("int")
                                    )
                                )
                            )
                        )
                    ),
                    Helpers.MakeVoidType(),
                    Modifiers.None
                )
            });

            Assert.IsNotNull(ast);
            Helpers.AstStructuralEqual(ast, expected);
        }

        [Test]
        public void BuiltinObjects()
        {
            var parser = new Parser(new Scanner("../../sources/for_unit_tests/builtin_objects.exs"));
            parser.DoPostParseProcessing = true;
            parser.Parse();

            var ast = parser.TopmostAst;

            var expected = AstNode.MakeModuleDef("main", new List<EntityDeclaration>{
                EntityDeclaration.MakeFunc(
                    "main",
                    Enumerable.Empty<ParameterDeclaration>(),
                    Statement.MakeBlock(
                        Statement.MakeVarDecl(
                            Helpers.MakeSeq(
                                Helpers.MakePaticularPatternWithType(
                                    "a",
                                    Helpers.MakeGenericType(
                                        "array",
                                        Helpers.MakePrimitiveType("int")
                                    )
                                )
                            ),
                            Helpers.MakeSeq(
                                Helpers.MakeSequenceInitializer(
                                    Helpers.MakeGenericType(
                                        "array",
                                        Helpers.MakePrimitiveType("int")
                                    ),
                                    Expression.MakeConstant(
                                        "int",
                                        1
                                    ),
                                    Expression.MakeConstant(
                                        "int",
                                        2
                                    ),
                                    Expression.MakeConstant(
                                        "int",
                                        3
                                    ),
                                    Expression.MakeConstant(
                                        "int",
                                        4
                                    ),
                                    Expression.MakeConstant(
                                        "int",
                                        5
                                    ),
                                    Expression.MakeConstant(
                                        "int",
                                        6
                                    ),
                                    Expression.MakeConstant(
                                        "int",
                                        7
                                    ),
                                    Expression.MakeConstant(
                                        "int",
                                        8
                                    )
                                )
                            ),
                            Modifiers.Immutable
                        ),
                        Statement.MakeVarDecl(
                            Helpers.MakeSeq(
                                Helpers.MakePaticularPatternWithType(
                                    "b",
                                    Helpers.MakeGenericType(
                                        "dictionary",
                                        Helpers.MakePrimitiveType("string"),
                                        Helpers.MakePrimitiveType("int")
                                    )
                                )
                            ),
                            Helpers.MakeSeq(
                                Helpers.MakeSequenceInitializer(
                                    Helpers.MakeGenericType(
                                        "dictionary",
                                        Helpers.MakePrimitiveType("string"),
                                        Helpers.MakePrimitiveType("int")
                                    ),
                                    Expression.MakeKeyValuePair(
                                        Expression.MakeConstant(
                                            "string",
                                            "akari"
                                        ),
                                        Expression.MakeConstant(
                                            "int",
                                            13
                                        )
                                    ),
                                    Expression.MakeKeyValuePair(
                                        Expression.MakeConstant(
                                            "string",
                                            "chinatsu"
                                        ),
                                        Expression.MakeConstant(
                                            "int",
                                            13
                                        )
                                    ),
                                    Expression.MakeKeyValuePair(
                                        Expression.MakeConstant(
                                            "string",
                                            "kyoko"
                                        ),
                                        Expression.MakeConstant(
                                            "int",
                                            14
                                        )
                                    ),
                                    Expression.MakeKeyValuePair(
                                        Expression.MakeConstant(
                                            "string",
                                            "yui"
                                        ),
                                        Expression.MakeConstant(
                                            "int",
                                            14
                                        )
                                    )
                                )
                            ),
                            Modifiers.Immutable
                        ),
                        Statement.MakeVarDecl(
                            Helpers.MakeSeq(
                                Helpers.MakePaticularPatternWithType(
                                    "c",
                                    Helpers.MakeGenericType(
                                        "tuple",
                                        Helpers.MakePrimitiveType("string"),
                                        Helpers.MakePrimitiveType("string"),
                                        Helpers.MakePrimitiveType("string"),
                                        Helpers.MakePrimitiveType("int")
                                    )
                                )
                            ),
                            Helpers.MakeSeq(
                                Expression.MakeParen(
                                    Expression.MakeSequenceExpression(
                                        Expression.MakeConstant(
                                            "string",
                                            "akarichan"
                                        ),
                                        Expression.MakeConstant(
                                            "string",
                                            "kawakawa"
                                        ),
                                        Expression.MakeConstant(
                                            "string",
                                            "chinatsuchan"
                                        ),
                                        Expression.MakeConstant(
                                            "int",
                                            2424
                                        )
                                    )
                                )
                            ),
                            Modifiers.Immutable
                        ),
                        Statement.MakeVarDecl(
                            Helpers.MakeSeq(
                                Helpers.MakePaticularPatternWithType(
                                    "d",
                                    Helpers.MakeGenericType(
                                        "slice",
                                        Helpers.MakeGenericType(
                                            "array",
                                            Helpers.MakePrimitiveType("int")
                                        ),
                                        Helpers.MakePrimitiveType("int")
                                    )
                                )
                            ),
                            Helpers.MakeSeq(
                                Helpers.MakeIndexer(
                                    Helpers.MakeIdentifierPath(
                                        "a",
                                        Helpers.MakeGenericType(
                                            "array",
                                            Helpers.MakePrimitiveType("int")
                                        )
                                    ),
                                    Expression.MakeIntSeq(
                                        Expression.MakeConstant(
                                            "int",
                                            0
                                        ),
                                        Expression.MakeConstant(
                                            "int",
                                            3
                                        ),
                                        Expression.MakeConstant(
                                            "int",
                                            1
                                        ),
                                        false
                                    )
                                )
                            ),
                            Modifiers.Immutable
                        ),
                        Statement.MakeVarDecl(
                            Helpers.MakeSeq(
                                Helpers.MakePaticularPatternWithType(
                                    "e",
                                    Helpers.MakeGenericType(
                                        "array",
                                        Helpers.MakePrimitiveType("int")
                                    )
                                )
                            ),
                            Helpers.MakeSeq(
                                Helpers.MakeSequenceInitializer(
                                    Helpers.MakeGenericType(
                                        "array",
                                        Helpers.MakePrimitiveType("int")
                                    ),
                                    Expression.MakeIntSeq(
                                        Expression.MakeConstant("int", 0),
                                        Expression.MakeConstant("int", 10),
                                        Expression.MakeConstant("int", 1),
                                        false
                                    )
                                )
                            ),
                            Modifiers.Immutable
                        ),
                        Statement.MakeVarDecl(
                            Helpers.MakeSeq(
                                Helpers.MakePaticularPatternWithType(
                                    "f",
                                    Helpers.MakeGenericType(
                                        "vector",
                                        Helpers.MakePrimitiveType("int")
                                    )
                                )
                            ),
                            Helpers.MakeSeq(
                                Helpers.MakeSequenceInitializer(
                                    Helpers.MakeGenericType(
                                        "vector",
                                        Helpers.MakePrimitiveType("int")
                                    ),
                                    Expression.MakeIntSeq(
                                        Expression.MakeConstant("int", 0),
                                        Expression.MakeConstant("int", 10),
                                        Expression.MakeConstant("int", 1),
                                        false
                                    )
                                )
                            ),
                            Modifiers.Immutable
                        ),
                        Statement.MakeVarDecl(
                            Helpers.MakeSeq(
                                Helpers.MakePaticularPatternWithType(
                                    "g",
                                    Helpers.MakeGenericType(
                                        "array",
                                        Helpers.MakePrimitiveType("int")
                                    )
                                )
                            ),
                            Helpers.MakeSeq(
                                Helpers.MakeSequenceInitializer(
                                    Helpers.MakeGenericType(
                                        "array",
                                        Helpers.MakePrimitiveType("int")
                                    ),
                                    Expression.MakeIntSeq(
                                        Expression.MakeConstant("int", 0),
                                        Expression.MakeUnaryExpr(
                                            OperatorType.Minus,
                                            Expression.MakeConstant("int", 10)
                                        ),
                                        Expression.MakeUnaryExpr(
                                            OperatorType.Minus,
                                            Expression.MakeConstant("int", 1)
                                        ),
                                        false
                                    )
                                )
                            ),
                            Modifiers.Immutable
                        ),
                        Statement.MakeVarDecl(
                            Helpers.MakeSeq(
                                Helpers.MakePaticularPatternWithType(
                                    "h",
                                    Helpers.MakeGenericType(
                                        "array",
                                        Helpers.MakePrimitiveType("int")
                                    )
                                )
                            ),
                            Helpers.MakeSeq(
                                Helpers.MakeSequenceInitializer(
                                    Helpers.MakeGenericType(
                                        "array",
                                        Helpers.MakePrimitiveType("int")
                                    ),
                                    Expression.MakeIntSeq(
                                        Expression.MakeConstant("int", 0),
                                        Expression.MakeConstant("int", 100),
                                        Expression.MakeConstant("int", 2),
                                        true
                                    )
                                )
                            ),
                            Modifiers.Immutable
                        ),
                        Statement.MakeVarDecl(
                            Helpers.MakeSeq(
                                Helpers.MakePaticularPatternWithType(
                                    "i",
                                    Helpers.MakeGenericType(
                                        "array",
                                        Helpers.MakePrimitiveType("int")
                                    )
                                )
                            ),
                            Helpers.MakeSeq(
                                Helpers.MakeSequenceInitializer(
                                    Helpers.MakeGenericType(
                                        "array",
                                        Helpers.MakePrimitiveType("int")
                                    ),
                                    Expression.MakeIntSeq(
                                        Expression.MakeConstant("int", 5),
                                        Expression.MakeConstant("int", 15),
                                        Expression.MakeConstant("int", 2),
                                        false
                                    )
                                )
                            ),
                            Modifiers.Immutable
                        ),
                        Statement.MakeVarDecl(
                            Helpers.MakeSeq(
                                Helpers.MakeExactPatternWithType(
                                    "y",
                                    Helpers.MakeGenericType(
                                        "vector",
                                        Helpers.MakePrimitiveType("int")
                                    )
                                )
                            ),
                            Helpers.MakeSeq(
                                Expression.MakeSequenceInitializer(
                                    Helpers.MakeGenericType(
                                        "vector",
                                        Helpers.MakePrimitiveType("int")
                                    ),
                                    Enumerable.Empty<Expression>()
                                )
                            ),
                            Modifiers.None
                        ),
                        Statement.MakeValueBindingForStmt(
                            Modifiers.Immutable,
                            Helpers.MakePaticularPatternWithType(
                                "x",
                                Helpers.MakePrimitiveType("int")
                            ),
                            Helpers.MakeIdentifierPath(
                                "d",
                                Helpers.MakeGenericType(
                                    "slice",
                                    Helpers.MakeGenericType(
                                        "array",
                                        Helpers.MakePrimitiveType("int")
                                    ),
                                    Helpers.MakePrimitiveType("int")
                                )
                            ),
                            Statement.MakeBlock(
                                Statement.MakeExprStmt(
                                    Helpers.MakeCallExpression(
                                        Helpers.SomeWellKnownExpressions.Print,
                                        Helpers.MakeCallExpression(
                                            Helpers.SomeWellKnownExpressions.StringFormat1,
                                            Expression.MakeConstant("string", "{0}"),
                                            Helpers.MakeIdentifierPath(
                                                "x",
                                                Helpers.MakePrimitiveType("int")
                                            )
                                        )
                                    )
                                ),
                                Statement.MakeExprStmt(
                                    Helpers.MakeCallExpression(
                                        Expression.MakeMemRef(
                                            Helpers.MakeIdentifierPath(
                                                "y",
                                                Helpers.MakeGenericType(
                                                    "vector",
                                                    Helpers.MakePrimitiveType("int")
                                                )
                                            ),
                                            Helpers.MakeFunctionIdentifier(
                                                "Add",
                                                Helpers.MakeVoidType(),
                                                AstType.MakePrimitiveType("int")
                                            )
                                        ),
                                        Helpers.MakeIdentifierPath(
                                            "x",
                                            Helpers.MakePrimitiveType("int")
                                        )
                                    )
                                )
                            )
                        ),
                        Statement.MakeExprStmt(
                            Helpers.MakeCallExpression(
                                Helpers.SomeWellKnownExpressions.Println,
                                Helpers.MakeCallExpression(
                                    Helpers.SomeWellKnownExpressions.StringFormatN,
                                    Expression.MakeConstant("string", "{0}, {1}, {2}, {3}, {4}, {5}, {6}"),
                                    Helpers.MakeIdentifierPath(
                                        "a",
                                        Helpers.MakeGenericType(
                                            "array",
                                            Helpers.MakePrimitiveType("int")
                                        )
                                    ),
                                    Helpers.MakeIdentifierPath(
                                        "b",
                                        Helpers.MakeGenericType(
                                            "dictionary",
                                            Helpers.MakePrimitiveType("string"),
                                            Helpers.MakePrimitiveType("int")
                                        )
                                    ),
                                    Helpers.MakeIdentifierPath(
                                        "c",
                                        Helpers.MakeGenericType(
                                            "tuple",
                                            Helpers.MakePrimitiveType("string"),
                                            Helpers.MakePrimitiveType("string"),
                                            Helpers.MakePrimitiveType("string"),
                                            Helpers.MakePrimitiveType("int")
                                        )
                                    ),
                                    Helpers.MakeIdentifierPath(
                                        "d",
                                        Helpers.MakeGenericType(
                                            "slice",
                                            Helpers.MakeGenericType(
                                                "array",
                                                Helpers.MakePrimitiveType("int")
                                            ),
                                            Helpers.MakePrimitiveType("int")
                                        )
                                    ),
                                    Helpers.MakeIdentifierPath(
                                        "e",
                                        Helpers.MakeGenericType(
                                            "array",
                                            Helpers.MakePrimitiveType("int")
                                        )
                                    ),
                                    Helpers.MakeIdentifierPath(
                                        "f",
                                        Helpers.MakeGenericType(
                                            "vector",
                                            Helpers.MakePrimitiveType("int")
                                        )
                                    ),
                                    Helpers.MakeIdentifierPath(
                                        "g",
                                        Helpers.MakeGenericType(
                                            "array",
                                            Helpers.MakePrimitiveType("int")
                                        )
                                    )
                                )
                            )
                        ),
                        Statement.MakeExprStmt(
                            Helpers.MakeCallExpression(
                                Helpers.SomeWellKnownExpressions.Println,
                                Helpers.MakeCallExpression(
                                    Helpers.SomeWellKnownExpressions.StringFormat3,
                                    Expression.MakeConstant("string", "{0}, {1}, {2}"),
                                    Helpers.MakeIdentifierPath(
                                        "h",
                                        Helpers.MakeGenericType(
                                            "array",
                                            Helpers.MakePrimitiveType("int")
                                        )
                                    ),
                                    Helpers.MakeIdentifierPath(
                                        "i",
                                        Helpers.MakeGenericType(
                                            "array",
                                            Helpers.MakePrimitiveType("int")
                                        )
                                    ),
                                    Helpers.MakeIdentifierPath(
                                        "y",
                                        Helpers.MakeGenericType(
                                            "vector",
                                            Helpers.MakePrimitiveType("int")
                                        )
                                    )
                                )
                            )
                        )
                    ),
                    Helpers.MakeVoidType(),
                    Modifiers.None
                )
            });

            Assert.IsNotNull(ast);
            Helpers.AstStructuralEqual(ast, expected);
        }

        [Test]
        public void Interface()
        {
            var parser = new Parser(new Scanner("../../sources/for_unit_tests/interface.exs"));
            parser.DoPostParseProcessing = true;
            parser.Parse();

            var ast = parser.TopmostAst;

            var expected_ast = AstNode.MakeModuleDef("main", new List<EntityDeclaration>{
                Helpers.MakeInterfaceDecl(
                    "IInterface",
                    Enumerable.Empty<AstType>(),
                    Modifiers.None,
                    EntityDeclaration.MakeFunc(
                        "doSomeBehavior",
                        Enumerable.Empty<ParameterDeclaration>(),
                        null,
                        Helpers.MakePrimitiveType("int"),
                        Modifiers.Public
                    )
                ),
                Helpers.MakeClassDecl(
                    "TestClass2",
                    Helpers.MakeSeq<AstType>(
                        Helpers.MakeGenericType("IInterface")
                    ),
                    Modifiers.None,
                    EntityDeclaration.MakeField(
                        Helpers.MakeSeq(
                            Helpers.MakeExactPatternWithType(
                                "x",
                                Helpers.MakePrimitiveType("int")
                            )
                        ),
                        Helpers.MakeSeq(
                            Expression.Null
                        ),
                        Modifiers.Immutable | Modifiers.Private
                    ),
                    EntityDeclaration.MakeFunc(
                        "doSomeBehavior",
                        Enumerable.Empty<ParameterDeclaration>(),
                        Statement.MakeBlock(
                            Helpers.MakeSingleItemReturnStatement(
                                Expression.MakeMemRef(
                                    Expression.MakeSelfRef(),
                                    AstNode.MakeIdentifier(
                                        "x",
                                        Helpers.MakePrimitiveType("int")
                                    )
                                )
                            )
                        ),
                        Helpers.MakePrimitiveType("int"),
                        Modifiers.Public
                    )
                ),
                EntityDeclaration.MakeFunc(
                    "main",
                    Enumerable.Empty<ParameterDeclaration>(),
                    Statement.MakeBlock(
                        Statement.MakeVarDecl(
                            Helpers.MakeSeq(
                                Helpers.MakePaticularPatternWithType(
                                    "t",
                                    Helpers.MakeGenericType("TestClass2")
                                )
                            ),
                            Helpers.MakeSeq(
                                Expression.MakeObjectCreation(
                                    Helpers.MakeGenericType("TestClass2"),
                                    Helpers.MakeSeq(
                                        AstNode.MakeIdentifier("x")
                                    ),
                                    Helpers.MakeSeq(
                                        Expression.MakeConstant("int", 1)
                                    )
                                )
                            ),
                            Modifiers.Immutable
                        ),
                        Statement.MakeVarDecl(
                            Helpers.MakeSeq(
                                Helpers.MakePaticularPatternWithType(
                                    "a",
                                    Helpers.MakePrimitiveType("int")
                                )
                            ),
                            Helpers.MakeSeq(
                                Helpers.MakeCallExpression(
                                    Expression.MakeMemRef(
                                        Helpers.MakeIdentifierPath(
                                            "t",
                                            Helpers.MakeGenericType("TestClass2")
                                        ),
                                        Helpers.MakeFunctionIdentifier(
                                            "doSomeBehavior",
                                            Helpers.MakePrimitiveType("int")
                                        )
                                    )
                                )
                            ),
                            Modifiers.Immutable
                        ),
                        Statement.MakeExprStmt(
                            Helpers.MakeCallExpression(
                                Helpers.SomeWellKnownExpressions.Println,
                                Helpers.MakeCallExpression(
                                    Helpers.SomeWellKnownExpressions.StringFormat1,
                                    Expression.MakeConstant("string", "t.x = {0}"),
                                    Helpers.MakeIdentifierPath(
                                        "a",
                                        Helpers.MakePrimitiveType("int")
                                    )
                                )
                            )
                        )
                    ),
                    Helpers.MakeVoidType(),
                    Modifiers.None
                )
            });

            Assert.IsNotNull(ast);
            Helpers.AstStructuralEqual(ast, expected_ast);
        }

        [Test]
        public void Slices()
        {
            var parser = new Parser(new Scanner("../../sources/for_unit_tests/slices.exs"));
            parser.DoPostParseProcessing = true;
            parser.Parse();

            var ast = parser.TopmostAst;

            var expected = AstNode.MakeModuleDef("main", new List<EntityDeclaration>{
                EntityDeclaration.MakeFunc(
                    "main",
                    Enumerable.Empty<ParameterDeclaration>(),
                    Statement.MakeBlock(
                        Statement.MakeVarDecl(
                            Helpers.MakeSeq(
                                Helpers.MakePaticularPatternWithType(
                                    "a",
                                    Helpers.MakeGenericType(
                                        "array",
                                        Helpers.MakePrimitiveType("int")
                                    )
                                )
                            ),
                            Helpers.MakeSeq(
                                Expression.MakeSequenceInitializer(
                                    Helpers.MakeGenericType(
                                        "array",
                                        Helpers.MakePrimitiveType("int")
                                    ),
                                    Helpers.MakeSeq(
                                        Expression.MakeConstant(
                                            "int",
                                            0
                                        ),
                                        Expression.MakeConstant(
                                            "int",
                                            1
                                        ),
                                        Expression.MakeConstant(
                                            "int",
                                            2
                                        ),
                                        Expression.MakeConstant(
                                            "int",
                                            3
                                        ),
                                        Expression.MakeConstant(
                                            "int",
                                            4
                                        ),
                                        Expression.MakeConstant(
                                            "int",
                                            5
                                        ),
                                        Expression.MakeConstant(
                                            "int",
                                            6
                                        ),
                                        Expression.MakeConstant(
                                            "int",
                                            7
                                        ),
                                        Expression.MakeConstant(
                                            "int",
                                            8
                                        ),
                                        Expression.MakeConstant(
                                            "int",
                                            9
                                        )
                                    )
                                )
                            ),
                            Modifiers.Immutable
                        ),
                        Statement.MakeVarDecl(
                            Helpers.MakeSeq(
                                Helpers.MakePaticularPatternWithType(
                                    "b",
                                    Helpers.MakeGenericType(
                                        "slice",
                                        Helpers.MakeGenericType(
                                            "array",
                                            Helpers.MakePrimitiveType("int")
                                        ),
                                        Helpers.MakePrimitiveType("int")
                                    )
                                )
                            ),
                            Helpers.MakeSeq(
                                Helpers.MakeIndexer(
                                    Helpers.MakeIdentifierPath(
                                        "a",
                                        Helpers.MakeGenericType(
                                            "array",
                                            Helpers.MakePrimitiveType("int")
                                        )
                                    ),
                                    Expression.MakeIntSeq(
                                        Expression.MakeConstant("int", 2),
                                        Expression.MakeConstant("int", 4),
                                        Expression.MakeConstant("int", 1),
                                        true
                                    )
                                )
                            ),
                            Modifiers.Immutable
                        ),
                        Statement.MakeVarDecl(
                            Helpers.MakeSeq(
                                Helpers.MakePaticularPatternWithType(
                                    "c",
                                    Helpers.MakeGenericType(
                                        "vector",
                                        Helpers.MakePrimitiveType("int")
                                    )
                                )
                            ),
                            Helpers.MakeSeq(
                                Expression.MakeSequenceInitializer(
                                    Helpers.MakeGenericType(
                                        "vector",
                                        Helpers.MakePrimitiveType("int")
                                    ),
                                    Helpers.MakeSeq(
                                        Expression.MakeConstant(
                                            "int",
                                            0
                                        ),
                                        Expression.MakeConstant(
                                            "int",
                                            1
                                        ),
                                        Expression.MakeConstant(
                                            "int",
                                            2
                                        ),
                                        Expression.MakeConstant(
                                            "int",
                                            3
                                        ),
                                        Expression.MakeConstant(
                                            "int",
                                            4
                                        ),
                                        Expression.MakeConstant(
                                            "int",
                                            5
                                        ),
                                        Expression.MakeConstant(
                                            "int",
                                            6
                                        ),
                                        Expression.MakeConstant(
                                            "int",
                                            7
                                        ),
                                        Expression.MakeConstant(
                                            "int",
                                            8
                                        ),
                                        Expression.MakeConstant(
                                            "int",
                                            9
                                        )
                                    )
                                )
                            ),
                            Modifiers.Immutable
                        ),
                        Statement.MakeVarDecl(
                            Helpers.MakeSeq(
                                Helpers.MakePaticularPatternWithType(
                                    "d",
                                    Helpers.MakeGenericType(
                                        "slice",
                                        Helpers.MakeGenericType(
                                            "vector",
                                            Helpers.MakePrimitiveType("int")
                                        ),
                                        Helpers.MakePrimitiveType("int")
                                    )
                                )
                            ),
                            Helpers.MakeSeq(
                                Helpers.MakeIndexer(
                                    Helpers.MakeIdentifierPath(
                                        "c",
                                        Helpers.MakeGenericType(
                                            "vector",
                                            Helpers.MakePrimitiveType("int")
                                        )
                                    ),
                                    Expression.MakeIntSeq(
                                        Expression.MakeConstant("int", 2),
                                        Expression.MakeConstant("int", 4),
                                        Expression.MakeConstant("int", 1),
                                        true
                                    )
                                )
                            ),
                            Modifiers.Immutable
                        ),
                        Statement.MakeExprStmt(
                            Helpers.MakeCallExpression(
                                Helpers.SomeWellKnownExpressions.Println,
                                Helpers.MakeCallExpression(
                                    Helpers.SomeWellKnownExpressions.StringFormat2,
                                    Expression.MakeConstant("string", "{0}, {1}"),
                                    Helpers.MakeIdentifierPath(
                                        "b",
                                        Helpers.MakeGenericType(
                                            "slice",
                                            Helpers.MakeGenericType(
                                                "array",
                                                Helpers.MakePrimitiveType("int")
                                            ),
                                            Helpers.MakePrimitiveType("int")
                                        )
                                    ),
                                    Helpers.MakeIdentifierPath(
                                        "d",
                                        Helpers.MakeGenericType(
                                            "slice",
                                            Helpers.MakeGenericType(
                                                "vector",
                                                Helpers.MakePrimitiveType("int")
                                            ),
                                            Helpers.MakePrimitiveType("int")
                                        )
                                    )
                                )
                            )
                        )
                    ),
                    Helpers.MakeVoidType(),
                    Modifiers.None
                )
            });

            Assert.IsNotNull(ast);
            Helpers.AstStructuralEqual(ast, expected);
        }

        [Test]
        public void TypeCast()
        {
            var parser = new Parser(new Scanner("../../sources/for_unit_tests/type_cast.exs"));
            parser.DoPostParseProcessing = true;
            parser.Parse();

            var ast = parser.TopmostAst;

            var expected = AstNode.MakeModuleDef("main", new List<EntityDeclaration>{
                EntityDeclaration.MakeFunc(
                    "main",
                    Enumerable.Empty<ParameterDeclaration>(),
                    Statement.MakeBlock(
                        Statement.MakeVarDecl(
                            Helpers.MakeSeq(
                                Helpers.MakePaticularPatternWithType(
                                    "a",
                                    Helpers.MakePrimitiveType("int")
                                )
                            ),
                            Helpers.MakeSeq(
                                Expression.MakeConstant("int", 10)
                            ),
                            Modifiers.Immutable
                        ),
                        Statement.MakeVarDecl(
                            Helpers.MakeSeq(
                                Helpers.MakePaticularPatternWithType(
                                    "b",
                                    Helpers.MakePrimitiveType("byte")
                                )
                            ),
                            Helpers.MakeSeq(
                                Expression.MakeCastExpr(
                                    Helpers.MakeIdentifierPath(
                                        "a",
                                        Helpers.MakePrimitiveType("int")
                                    ),
                                    Helpers.MakePrimitiveType("byte")
                                )
                            ),
                            Modifiers.Immutable
                        ),
                        Statement.MakeExprStmt(
                            Helpers.MakeCallExpression(
                                Helpers.SomeWellKnownExpressions.Println,
                                Helpers.MakeCallExpression(
                                    Helpers.SomeWellKnownExpressions.StringFormat2,
                                    Expression.MakeConstant("string", "{0}, {1}"),
                                    Helpers.MakeIdentifierPath(
                                        "a",
                                        Helpers.MakePrimitiveType("int")
                                    ),
                                    Helpers.MakeIdentifierPath(
                                        "b",
                                        Helpers.MakePrimitiveType("byte")
                                    )
                                )
                            )
                        )
                    ),
                    Helpers.MakeVoidType(),
                    Modifiers.None
                )
            });

            Assert.IsNotNull(ast);
            Helpers.AstStructuralEqual(ast, expected);
        }

        [Test]
        public void UseOfTheStandardLibrary()
        {
            var parser = new Parser(new Scanner("../../sources/for_unit_tests/use_of_the_standard_library.exs"));
            parser.DoPostParseProcessing = true;
            parser.Parse();

            var ast = parser.TopmostAst;

            var expected = AstNode.MakeModuleDef("main", new List<EntityDeclaration>{
                EntityDeclaration.MakeFunc(
                    "main",
                    Enumerable.Empty<ParameterDeclaration>(),
                    Statement.MakeBlock(
                        Statement.MakeVarDecl(
                            Helpers.MakeSeq(
                                Helpers.MakeExactPatternWithType(
                                    "writer",
                                    Helpers.MakeGenericTypeWithRealName(
                                        "FileStream",
                                        "System.IO.FileStream"
                                    ),
                                    "FileStream"
                                )
                            ),
                            Helpers.MakeSeq(
                                Expression.Null
                            ),
                            Modifiers.None
                        ),
                        Helpers.MakeTryStmt(
                            Statement.MakeBlock(
                                Statement.MakeExprStmt(
                                    Expression.MakeSingleAssignment(
                                        Helpers.MakeIdentifierPath(
                                            "writer",
                                            Helpers.MakeGenericTypeWithRealName(
                                                "FileStream",
                                                "System.IO.FileStream"
                                            )
                                        ),
                                        Helpers.MakeCallExpression(
                                            Expression.MakeMemRef(
                                                Helpers.MakeIdentifierPath(
                                                    "File",
                                                    Helpers.MakeGenericType("System.IO.File")
                                                ),
                                                Helpers.MakeFunctionIdentifier(
                                                    "OpenWrite",
                                                    Helpers.MakeGenericType("FileStream"),
                                                    Helpers.MakePrimitiveType("string")
                                                )
                                            ),
                                            Expression.MakeConstant(
                                                "string",
                                                "./some_text.txt"
                                            )
                                        )
                                    )
                                ),
                                Statement.MakeVarDecl(
                                    Helpers.MakeSeq(
                                        Helpers.MakePaticularPatternWithType(
                                            "bytes",
                                            Helpers.MakeGenericType(
                                                "array",
                                                Helpers.MakePrimitiveType("byte")
                                            )
                                        ) 
                                    ),
                                    Helpers.MakeSeq(
                                        Helpers.MakeCallExpression(
                                            Expression.MakeMemRef(
                                                Expression.MakeObjectCreation(
                                                    Helpers.MakeGenericTypeWithRealName(
                                                        "UTF8Encoding",
                                                        "System.Text.UTF8Encoding"
                                                    ),
                                                    Helpers.MakeSeq(
                                                        AstNode.MakeIdentifier("encoderShouldEmitUTF8Identifier")
                                                    ),
                                                    Helpers.MakeSeq(
                                                        Expression.MakeConstant("bool", true)
                                                    )
                                                ),
                                                Helpers.MakeFunctionIdentifier(
                                                    "GetBytes",
                                                    Helpers.MakeGenericType(
                                                        "array",
                                                        Helpers.MakePrimitiveType("byte")
                                                    ),
                                                    Helpers.MakePrimitiveType("string")
                                                )
                                            ),
                                            Expression.MakeConstant(
                                                "string",
                                                "This is to test writing a file"
                                            )
                                        )
                                    ),
                                    Modifiers.Immutable
                                ),
                                Statement.MakeExprStmt(
                                    Helpers.MakeCallExpression(
                                        Expression.MakeMemRef(
                                            Helpers.MakeIdentifierPath(
                                                "writer",
                                                Helpers.MakeGenericTypeWithRealName(
                                                    "FileStream",
                                                    "System.IO.FileStream"
                                                )
                                            ),
                                            Helpers.MakeFunctionIdentifier(
                                                "Write",
                                                Helpers.MakeVoidType(),
                                                Helpers.MakeGenericType(
                                                    "array",
                                                    Helpers.MakePrimitiveType("byte")
                                                ),
                                                Helpers.MakePrimitiveType("int"),
                                                Helpers.MakePrimitiveType("int")
                                            )
                                        ),
                                        Helpers.MakeIdentifierPath(
                                            "bytes",
                                            Helpers.MakeGenericType(
                                                "array",
                                                Helpers.MakePrimitiveType("byte")
                                            )
                                        ),
                                        Expression.MakeConstant("int", 0),
                                        Expression.MakeMemRef(
                                            Helpers.MakeIdentifierPath(
                                                "bytes",
                                                Helpers.MakeGenericType(
                                                    "array",
                                                    Helpers.MakePrimitiveType("byte")
                                                )
                                            ),
                                            AstNode.MakeIdentifier(
                                                "Length",
                                                Helpers.MakePropertyGetterType(
                                                    "Length",
                                                    Helpers.MakePrimitiveType("int")
                                                )
                                            )
                                        )
                                    )
                                )
                            ),
                            Statement.MakeFinallyClause(
                                Statement.MakeBlock(
                                    Statement.MakeIfStmt(
                                        PatternConstruct.MakeExpressionPattern(
                                            Expression.MakeBinaryExpr(
                                                OperatorType.InEquality,
                                                Helpers.MakeIdentifierPath(
                                                    "writer",
                                                    Helpers.MakeGenericTypeWithRealName(
                                                        "FileStream",
                                                        "System.IO.FileStream"
                                                    )
                                                ),
                                                Expression.MakeNullRef()
                                            )
                                        ),
                                        Statement.MakeBlock(
                                            Statement.MakeExprStmt(
                                                Helpers.MakeCallExpression(
                                                    Expression.MakeMemRef(
                                                        Helpers.MakeIdentifierPath(
                                                            "writer",
                                                            Helpers.MakeGenericTypeWithRealName(
                                                                "FileStream",
                                                                "System.IO.FileStream"
                                                            )
                                                        ),
                                                        Helpers.MakeFunctionIdentifier(
                                                            "Dispose",
                                                            Helpers.MakeVoidType()
                                                        )
                                                    )
                                                )
                                            )
                                        ),
                                        BlockStatement.Null
                                    )
                                )
                            )
                        )
                    ),
                    Helpers.MakeVoidType(),
                    Modifiers.None
                )
            }, Helpers.MakeSeq(
                AstNode.MakeImportDecl(
                    AstNode.MakeIdentifier("System.IO.File"),
                    "File"
                ),
                AstNode.MakeImportDecl(
                    AstNode.MakeIdentifier("System.IO.FileStream"),
                    "FileStream"
                ),
                AstNode.MakeImportDecl(
                    AstNode.MakeIdentifier("System.Text.UTF8Encoding"),
                    "UTF8Encoding"
                )
            ));

            Assert.IsNotNull(ast);
            Helpers.AstStructuralEqual(ast, expected);
        }

        [Test]
        public void VariousStrings()
        {
            var parser = new Parser(new Scanner("../../sources/for_unit_tests/various_strings.exs"));
            parser.DoPostParseProcessing = true;
            parser.Parse();

            var ast = parser.TopmostAst;

            var expected = AstNode.MakeModuleDef("main", new List<EntityDeclaration>{
                Helpers.MakeClassDecl(
                    "TestClass5",
                    Enumerable.Empty<AstType>(),
                    Modifiers.None,
                    EntityDeclaration.MakeField(
                        Helpers.MakeSeq(
                            Helpers.MakeExactPatternWithType(
                                "x",
                                Helpers.MakePrimitiveType("int")
                            )
                        ),
                        Helpers.MakeSeq(
                            Expression.Null
                        ),
                        Modifiers.Immutable | Modifiers.Private
                    ),
                    EntityDeclaration.MakeField(
                        Helpers.MakeSeq(
                            Helpers.MakeExactPatternWithType(
                                "y",
                                Helpers.MakePrimitiveType("int")
                            )
                        ),
                        Helpers.MakeSeq(
                            Expression.Null
                        ),
                        Modifiers.Public | Modifiers.Immutable
                    ),
                    EntityDeclaration.MakeFunc(
                        "getX",
                        Enumerable.Empty<ParameterDeclaration>(),
                        Statement.MakeBlock(
                            Helpers.MakeSingleItemReturnStatement(
                                Expression.MakeMemRef(
                                    Expression.MakeSelfRef(),
                                    AstNode.MakeIdentifier(
                                        "x",
                                        Helpers.MakePrimitiveType("int")
                                    )
                                )
                            )
                        ),
                        Helpers.MakePrimitiveType("int"),
                        Modifiers.Public
                    )
                ),
                EntityDeclaration.MakeFunc(
                    "main",
                    Enumerable.Empty<ParameterDeclaration>(),
                    Statement.MakeBlock(
                        Statement.MakeVarDecl(
                            Helpers.MakeSeq(
                                Helpers.MakePaticularPatternWithType(
                                    "x",
                                    Helpers.MakePrimitiveType("int")
                                )
                            ),
                            Helpers.MakeSeq(
                                Expression.MakeConstant(
                                    "int",
                                    5
                                )
                            ),
                            Modifiers.Immutable
                        ),
                        Statement.MakeVarDecl(
                            Helpers.MakeSeq(
                                Helpers.MakePaticularPatternWithType(
                                    "t",
                                    Helpers.MakeGenericType("TestClass5")
                                )
                            ),
                            Helpers.MakeSeq(
                                Expression.MakeObjectCreation(
                                    Helpers.MakeGenericType("TestClass5"),
                                    Helpers.MakeSeq(
                                        AstNode.MakeIdentifier("x"),
                                        AstNode.MakeIdentifier("y")
                                    ),
                                    Helpers.MakeSeq(
                                        Expression.MakeConstant("int", 1),
                                        Expression.MakeConstant("int", 2)
                                    )
                                )
                            ),
                            Modifiers.Immutable
                        ),
                        Statement.MakeVarDecl(
                            Helpers.MakeSeq(
                                Helpers.MakePaticularPatternWithType(
                                    "ary",
                                    Helpers.MakeGenericType(
                                        "array",
                                        Helpers.MakePrimitiveType("int")
                                    )
                                )
                            ),
                            Helpers.MakeSeq(
                                Helpers.MakeSequenceInitializer(
                                    Helpers.MakeGenericType(
                                        "array",
                                        Helpers.MakePrimitiveType("int")
                                    ),
                                    Expression.MakeConstant("int", 1),
                                    Expression.MakeConstant("int", 1),
                                    Expression.MakeConstant("int", 2),
                                    Expression.MakeConstant("int", 3),
                                    Expression.MakeConstant("int", 5),
                                    Expression.MakeConstant("int", 8)
                                )
                            ),
                            Modifiers.Immutable
                        ),
                        Statement.MakeVarDecl(
                            Helpers.MakeSeq(
                                Helpers.MakePaticularPatternWithType(
                                    "a",
                                    Helpers.MakePrimitiveType("string")
                                )
                            ),
                            Helpers.MakeSeq(
                                Expression.MakeConstant(
                                    "string",
                                    "some string"
                                )
                            ),
                            Modifiers.Immutable
                        ),
                        Statement.MakeVarDecl(
                            Helpers.MakeSeq(
                                Helpers.MakePaticularPatternWithType(
                                    "b",
                                    Helpers.MakePrimitiveType("string")
                                )
                            ),
                            Helpers.MakeSeq(
                                Helpers.MakeCallExpression(
                                    Helpers.SomeWellKnownExpressions.StringFormat1,
                                    Expression.MakeConstant(
                                        "string",
                                        "some string containing templates: {0}"
                                    ),
                                    Expression.MakeBinaryExpr(
                                        OperatorType.Plus,
                                        Helpers.MakeIdentifierPath(
                                            "x",
                                            Helpers.MakePrimitiveType("int")
                                        ),
                                        Expression.MakeConstant(
                                            "int",
                                            1
                                        )
                                    )
                                )
                            ),
                            Modifiers.Immutable
                        ),
                        Statement.MakeVarDecl(
                            Helpers.MakeSeq(
                                Helpers.MakePaticularPatternWithType(
                                    "c",
                                    Helpers.MakePrimitiveType("string")
                                )
                            ),
                            Helpers.MakeSeq(
                                Helpers.MakeCallExpression(
                                    Helpers.SomeWellKnownExpressions.StringFormat2,
                                    Expression.MakeConstant(
                                        "string",
                                        "another string containing templates: {0}, {1}"
                                    ),
                                    Helpers.MakeCallExpression(
                                        Expression.MakeMemRef(
                                            Helpers.MakeIdentifierPath(
                                                "t",
                                                Helpers.MakeGenericType("TestClass5")
                                            ),
                                            Helpers.MakeFunctionIdentifier(
                                                "getX",
                                                Helpers.MakePrimitiveType("int")
                                            )
                                        )
                                    ),
                                    Expression.MakeMemRef(
                                        Helpers.MakeIdentifierPath(
                                            "t",
                                            Helpers.MakeGenericType("TestClass5")
                                        ),
                                        AstNode.MakeIdentifier(
                                            "y",
                                            Helpers.MakePrimitiveType("int")
                                        )
                                    )
                                )
                            ),
                            Modifiers.Immutable
                        ),
                        Statement.MakeVarDecl(
                            Helpers.MakeSeq(
                                Helpers.MakePaticularPatternWithType(
                                    "d",
                                    Helpers.MakePrimitiveType("string")
                                )
                            ),
                            Helpers.MakeSeq(
                                Helpers.MakeCallExpression(
                                    Helpers.SomeWellKnownExpressions.StringFormat1,
                                    Expression.MakeConstant(
                                        "string",
                                        "the 6th fibonacci number is {0}"
                                    ),
                                    Helpers.MakeIndexer(
                                        Helpers.MakeIdentifierPath(
                                            "ary",
                                            Helpers.MakeGenericType(
                                                "array",
                                                Helpers.MakePrimitiveType("int")
                                            )
                                        ),
                                        Expression.MakeConstant(
                                            "int",
                                            5
                                        )
                                    )
                                )
                            ),
                            Modifiers.Immutable
                        ),
                        Statement.MakeVarDecl(
                            Helpers.MakeSeq(
                                Helpers.MakePaticularPatternWithType(
                                    "e",
                                    Helpers.MakePrimitiveType("string")
                                )
                            ),
                            Helpers.MakeSeq(
                                Helpers.MakeCallExpression(
                                    Helpers.SomeWellKnownExpressions.StringFormat1,
                                    Expression.MakeConstant(
                                        "string",
                                        "a string containing dollar symbol: $x = {0}"
                                    ),
                                    Helpers.MakeIdentifierPath(
                                        "x",
                                        Helpers.MakePrimitiveType("int")
                                    )
                                )
                            ),
                            Modifiers.Immutable
                        ),
                        Statement.MakeExprStmt(
                            Helpers.MakeCallExpression(
                                Helpers.SomeWellKnownExpressions.Println,
                                Helpers.MakeCallExpression(
                                    Helpers.SomeWellKnownExpressions.StringFormatN,
                                    Expression.MakeConstant("string", "{0}, {1}, {2}, {3}, {4}"),
                                    Helpers.MakeIdentifierPath(
                                        "a",
                                        Helpers.MakePrimitiveType("string")
                                    ),
                                    Helpers.MakeIdentifierPath(
                                        "b",
                                        Helpers.MakePrimitiveType("string")
                                    ),
                                    Helpers.MakeIdentifierPath(
                                        "c",
                                        Helpers.MakePrimitiveType("string")
                                    ),
                                    Helpers.MakeIdentifierPath(
                                        "d",
                                        Helpers.MakePrimitiveType("string")
                                    ),
                                    Helpers.MakeIdentifierPath(
                                        "e",
                                        Helpers.MakePrimitiveType("string")
                                    )
                                )
                            )
                        )
                    ),
                    Helpers.MakeVoidType(),
                    Modifiers.None
                )
            });

            Assert.IsNotNull(ast);
            Helpers.AstStructuralEqual(ast, expected);
        }

        [Test]
        public void Module2()
        {
            var parser = new Parser(new Scanner("../../sources/for_unit_tests/module2.exs"));
            parser.DoPostParseProcessing = true;
            parser.Parse();

            var ast = parser.TopmostAst;

            var expected_ast = AstNode.MakeModuleDef("main", new List<EntityDeclaration>{
                EntityDeclaration.MakeFunc(
                    "main",
                    Enumerable.Empty<ParameterDeclaration>(),
                    Statement.MakeBlock(
                        Statement.MakeVarDecl(
                            Helpers.MakeSeq(
                                Helpers.MakePaticularPatternWithType(
                                    "t",
                                    Helpers.MakeGenericType("TestClass4")
                                )
                            ),
                            Helpers.MakeSeq(
                                Expression.MakeObjectCreation(
                                    Helpers.MakeGenericType("TestClass4"),
                                    Helpers.MakeSeq(
                                        AstNode.MakeIdentifier("x"),
                                        AstNode.MakeIdentifier("y")
                                    ),
                                    Helpers.MakeSeq(
                                        Expression.MakeConstant("int", 1),
                                        Expression.MakeConstant("int", 2)
                                    )
                                )
                            ),
                            Modifiers.Immutable
                        ),
                        Statement.MakeVarDecl(
                            Helpers.MakeSeq(
                                Helpers.MakePaticularPatternWithType(
                                    "t2",
                                    Helpers.MakeGenericType("TestClass4")
                                )
                            ),
                            Helpers.MakeSeq(
                                Helpers.MakeCallExpression(
                                    Helpers.MakeFunctionIdentifierPath(
                                        "createTest",
                                        Helpers.MakeGenericType("TestClass4"),
                                        Helpers.MakePrimitiveType("int"),
                                        Helpers.MakePrimitiveType("int")
                                    ),
                                    Expression.MakeConstant("int", 3),
                                    Expression.MakeConstant("int", 4)
                                )
                            ),
                            Modifiers.Immutable
                        ),
                        Statement.MakeVarDecl(
                            Helpers.MakeSeq(
                                Helpers.MakePaticularPatternWithType(
                                    "x",
                                    Helpers.MakeGenericType(
                                        "tuple",
                                        Helpers.MakePrimitiveType("int"),
                                        Helpers.MakePrimitiveType("int")
                                    )
                                )
                            ),
                            Helpers.MakeSeq(
                                Helpers.MakeIdentifierPath(
                                    "pair",
                                    Helpers.MakeGenericType(
                                        "tuple",
                                        Helpers.MakePrimitiveType("int"),
                                        Helpers.MakePrimitiveType("int")
                                    )
                                )
                            ),
                            Modifiers.Immutable
                        ),
                        Statement.MakeExprStmt(
                            Helpers.MakeCallExpression(
                                Helpers.SomeWellKnownExpressions.Println,
                                Helpers.MakeCallExpression(
                                    Helpers.SomeWellKnownExpressions.StringFormat3,
                                    Expression.MakeConstant("string", "{0}, {1}, {2}"),
                                    Helpers.MakeIdentifierPath(
                                        "t",
                                        Helpers.MakeGenericType("TestClass4")
                                    ),
                                    Helpers.MakeIdentifierPath(
                                        "t2",
                                        Helpers.MakeGenericType("TestClass4")
                                    ),
                                    Helpers.MakeIdentifierPath(
                                        "x",
                                        Helpers.MakeGenericType(
                                            "tuple",
                                            Helpers.MakePrimitiveType("int"),
                                            Helpers.MakePrimitiveType("int")
                                        )
                                    )
                                )
                            )
                        )
                    ),
                    Helpers.MakeVoidType(),
                    Modifiers.None
                )
            }, Helpers.MakeSeq(
                AstNode.MakeImportDecl(
                    Helpers.MakeSeq(
                        AstNode.MakeIdentifier("test_module2::TestClass4"),
                        AstNode.MakeIdentifier("test_module2::createTest"),
                        AstNode.MakeIdentifier("test_module2::pair")
                    ),
                    Helpers.MakeSeq(
                        "TestClass4",
                        "createTest",
                        "pair"
                    ),
                    "./test_module2.exs"
                )
            ));

            Assert.IsNotNull(ast);
            Helpers.AstStructuralEqual(ast, expected_ast);
        }

        [Test]
        public void InteroperabilityTestWithCSharp()
        {
            var parser = new Parser(new Scanner("../../sources/for_unit_tests/interoperability_test_with_csharp.exs"));
            parser.DoPostParseProcessing = true;
            parser.Parse();

            var ast = parser.TopmostAst;

            var expected = AstNode.MakeModuleDef("main", new List<EntityDeclaration>{
                EntityDeclaration.MakeFunc(
                    "main",
                    Enumerable.Empty<ParameterDeclaration>(),
                    Statement.MakeBlock(
                        Statement.MakeVarDecl(
                            Helpers.MakeSeq(
                                Helpers.MakePaticularPatternWithType(
                                    "t",
                                    Helpers.MakeGenericTypeWithRealName(
                                        "InteroperabilityTest",
                                        "InteroperabilityTest.InteroperabilityTest"
                                    )
                                )
                            ),
                            Helpers.MakeSeq(
                                Expression.MakeObjectCreation(
                                    Helpers.MakeGenericTypeWithRealName(
                                        "InteroperabilityTest",
                                        "InteroperabilityTest.InteroperabilityTest"
                                    ),
                                    Enumerable.Empty<Identifier>(),
                                    Enumerable.Empty<Expression>()
                                )
                            ),
                            Modifiers.Immutable
                        ),
                        Statement.MakeExprStmt(
                            Helpers.MakeCallExpression(
                                Expression.MakeMemRef(
                                    Helpers.MakeIdentifierPath(
                                        "t",
                                        Helpers.MakeGenericTypeWithRealName(
                                            "InteroperabilityTest",
                                            "InteroperabilityTest.InteroperabilityTest"
                                        )
                                    ),
                                    Helpers.MakeFunctionIdentifier(
                                        "DoSomething",
                                        Helpers.MakeVoidType()
                                    )
                                )
                            )
                        ),
                        Statement.MakeVarDecl(
                            Helpers.MakeSeq(
                                Helpers.MakePaticularPatternWithType(
                                    "i",
                                    Helpers.MakePrimitiveType("int")
                                )
                            ),
                            Helpers.MakeSeq(
                                Helpers.MakeCallExpression(
                                    Expression.MakeMemRef(
                                        Helpers.MakeIdentifierPath(
                                            "t",
                                            Helpers.MakeGenericTypeWithRealName(
                                                "InteroperabilityTest",
                                                "InteroperabilityTest.InteroperabilityTest"
                                            )
                                        ),
                                        Helpers.MakeFunctionIdentifier(
                                            "GetSomeInt",
                                            Helpers.MakePrimitiveType("int")
                                        )
                                    )
                                )
                            ),
                            Modifiers.Immutable
                        ),
                        Statement.MakeVarDecl(
                            Helpers.MakeSeq(
                                Helpers.MakePaticularPatternWithType(
                                    "list",
                                    Helpers.MakeGenericTypeWithRealName(
                                        "vector",
                                        "System.Collections.Generic.List`1[[System.Int32, mscorlib, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089]]",
                                        Helpers.MakePrimitiveType("int")
                                    )
                                )
                            ),
                            Helpers.MakeSeq(
                                Helpers.MakeCallExpression(
                                    Expression.MakeMemRef(
                                        Helpers.MakeIdentifierPath(
                                            "t",
                                            Helpers.MakeGenericTypeWithRealName(
                                                "InteroperabilityTest",
                                                "InteroperabilityTest.InteroperabilityTest"
                                            )
                                        ),
                                        Helpers.MakeFunctionIdentifier(
                                            "GetIntList",
                                            Helpers.MakeGenericTypeWithRealName(
                                                "vector",
                                                "Vector",
                                                Helpers.MakePrimitiveType("int")
                                            )
                                        )
                                    )
                                )
                            ),
                            Modifiers.Immutable
                        ),
                        Statement.MakeExprStmt(
                            Helpers.MakeCallExpression(
                                Expression.MakeMemRef(
                                    Helpers.MakeIdentifierPath(
                                        "StaticTest",
                                        Helpers.MakeGenericType(
                                            "InteroperabilityTest.StaticTest"
                                        )
                                    ),
                                    Helpers.MakeFunctionIdentifier(
                                        "DoSomething",
                                        Helpers.MakeVoidType()
                                    )
                                )
                            )
                        ),
                        Statement.MakeVarDecl(
                            Helpers.MakeSeq(
                                Helpers.MakePaticularPatternWithType(
                                    "flag",
                                    Helpers.MakePrimitiveType("bool")
                                )
                            ),
                            Helpers.MakeSeq(
                                Helpers.MakeCallExpression(
                                    Expression.MakeMemRef(
                                        Helpers.MakeIdentifierPath(
                                            "StaticTest",
                                            Helpers.MakeGenericType(
                                                "InteroperabilityTest.StaticTest"
                                            )
                                        ),
                                        Helpers.MakeFunctionIdentifier(
                                            "GetSomeBool",
                                            Helpers.MakePrimitiveType("bool")
                                        )
                                    )
                                )
                            ),
                            Modifiers.Immutable
                        ),
                        Statement.MakeVarDecl(
                            Helpers.MakeSeq(
                                Helpers.MakePaticularPatternWithType(
                                    "seq",
                                    Helpers.MakePrimitiveType("intseq")
                                )
                            ),
                            Helpers.MakeSeq(
                                Helpers.MakeCallExpression(
                                    Expression.MakeMemRef(
                                        Helpers.MakeIdentifierPath(
                                            "StaticTest",
                                            Helpers.MakeGenericType(
                                                "InteroperabilityTest.StaticTest"
                                            )
                                        ),
                                        Helpers.MakeFunctionIdentifier(
                                            "GetSomeIntSeq",
                                            Helpers.MakePrimitiveType("intseq")
                                        )
                                    )
                                )
                            ),
                            Modifiers.Immutable
                        ),
                        Statement.MakeExprStmt(
                            Helpers.MakeCallExpression(
                                Helpers.SomeWellKnownExpressions.Println,
                                Helpers.MakeCallExpression(
                                    Helpers.SomeWellKnownExpressions.StringFormatN,
                                    Expression.MakeConstant("string", "{0}, {1}, {2}, {3}"),
                                    Helpers.MakeIdentifierPath(
                                        "i",
                                        Helpers.MakePrimitiveType("int")
                                    ),
                                    Helpers.MakeIdentifierPath(
                                        "list",
                                        Helpers.MakeGenericTypeWithRealName(
                                            "vector",
                                            "System.Collections.Generic.List`1[[System.Int32, mscorlib, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089]]",
                                            Helpers.MakePrimitiveType("int")
                                        )
                                    ),
                                    Helpers.MakeIdentifierPath(
                                        "flag",
                                        Helpers.MakePrimitiveType("bool")
                                    ),
                                    Helpers.MakeIdentifierPath(
                                        "seq",
                                        Helpers.MakePrimitiveType("intseq")
                                    )
                                )
                            )
                        )
                    ),
                    Helpers.MakeVoidType(),
                    Modifiers.None
                )
            }, Helpers.MakeSeq(
                AstNode.MakeImportDecl(
                    Helpers.MakeSeq(
                        AstNode.MakeIdentifier("InteroperabilityTest.InteroperabilityTest"),
                        AstNode.MakeIdentifier("InteroperabilityTest.StaticTest")
                    ),
                    Helpers.MakeSeq(
                        "InteroperabilityTest",
                        "StaticTest"
                    ),
                    "./InteroperabilityTest.dll"
                )
            ));

            Assert.IsNotNull(ast);
            Helpers.AstStructuralEqual(ast, expected);
        }

        [Test]
        public void AdvancedForLoops()
        {
            var parser = new Parser(new Scanner("../../sources/for_unit_tests/advanced_for_loops.exs"));
            parser.DoPostParseProcessing = true;
            parser.Parse();

            var ast = parser.TopmostAst;

            var expected = AstNode.MakeModuleDef("main", new List<EntityDeclaration>{
                EntityDeclaration.MakeFunc(
                    "main",
                    Enumerable.Empty<ParameterDeclaration>(),
                    Statement.MakeBlock(
                        Statement.MakeVarDecl(
                            Helpers.MakeSeq(
                                Helpers.MakePaticularPatternWithType(
                                    "dict",
                                    Helpers.MakeGenericType(
                                        "dictionary",
                                        Helpers.MakePrimitiveType("string"),
                                        Helpers.MakePrimitiveType("int")
                                    )
                                )
                            ),
                            Helpers.MakeSeq(
                                Helpers.MakeSequenceInitializer(
                                    Helpers.MakeGenericType(
                                        "dictionary",
                                        Helpers.MakePrimitiveType("string"),
                                        Helpers.MakePrimitiveType("int")
                                    ),
                                    Expression.MakeKeyValuePair(
                                        Expression.MakeConstant("string", "akari"),
                                        Expression.MakeConstant("int", 13)
                                    ),
                                    Expression.MakeKeyValuePair(
                                        Expression.MakeConstant("string", "chinatsu"),
                                        Expression.MakeConstant("int", 13)
                                    ),
                                    Expression.MakeKeyValuePair(
                                        Expression.MakeConstant("string", "kyoko"),
                                        Expression.MakeConstant("int", 14)
                                    ),
                                    Expression.MakeKeyValuePair(
                                        Expression.MakeConstant("string", "yui"),
                                        Expression.MakeConstant("int", 14)
                                    )
                                )
                            ),
                            Modifiers.Immutable
                        ),
                        Statement.MakeVarDecl(
                            Helpers.MakeSeq(
                                Helpers.MakePaticularPatternWithType(
                                    "a",
                                    Helpers.MakeGenericType(
                                        "array",
                                        Helpers.MakeGenericType(
                                            "tuple",
                                            Helpers.MakePrimitiveType("int"),
                                            Helpers.MakePrimitiveType("int")
                                        )
                                    )
                                )
                            ),
                            Helpers.MakeSeq(
                                Helpers.MakeSequenceInitializer(
                                    Helpers.MakeGenericType(
                                        "array",
                                        Helpers.MakeGenericType(
                                            "tuple",
                                            Helpers.MakePrimitiveType("int"),
                                            Helpers.MakePrimitiveType("int")
                                        )
                                    ),
                                    Expression.MakeParen(
                                        Expression.MakeSequenceExpression(
                                            Expression.MakeConstant("int", 1),
                                            Expression.MakeConstant("int", 2)
                                        )
                                    ),
                                    Expression.MakeParen(
                                        Expression.MakeSequenceExpression(
                                            Expression.MakeConstant("int", 3),
                                            Expression.MakeConstant("int", 4)
                                        )
                                    ),
                                    Expression.MakeParen(
                                        Expression.MakeSequenceExpression(
                                            Expression.MakeConstant("int", 5),
                                            Expression.MakeConstant("int", 6)
                                        )
                                    )
                                )
                            ),
                            Modifiers.Immutable
                        ),
                        Statement.MakeVarDecl(
                            Helpers.MakeSeq(
                                Helpers.MakePaticularPatternWithType(
                                    "v",
                                    Helpers.MakeGenericType(
                                        "vector",
                                        Helpers.MakeGenericType(
                                            "tuple",
                                            Helpers.MakePrimitiveType("int"),
                                            Helpers.MakePrimitiveType("int")
                                        )
                                    )
                                )
                            ),
                            Helpers.MakeSeq(
                                Helpers.MakeSequenceInitializer(
                                    Helpers.MakeGenericType(
                                        "vector",
                                        Helpers.MakeGenericType(
                                            "tuple",
                                            Helpers.MakePrimitiveType("int"),
                                            Helpers.MakePrimitiveType("int")
                                        )
                                    ),
                                    Expression.MakeParen(
                                        Expression.MakeSequenceExpression(
                                            Expression.MakeConstant("int", 7),
                                            Expression.MakeConstant("int", 8)
                                        )
                                    ),
                                    Expression.MakeParen(
                                        Expression.MakeSequenceExpression(
                                            Expression.MakeConstant("int", 9),
                                            Expression.MakeConstant("int", 10)
                                        )
                                    ),
                                    Expression.MakeParen(
                                        Expression.MakeSequenceExpression(
                                            Expression.MakeConstant("int", 11),
                                            Expression.MakeConstant("int", 12)
                                        )
                                    )
                                )
                            ),
                            Modifiers.Immutable
                        ),
                        Statement.MakeValueBindingForStmt(
                            Modifiers.Immutable,
                            Helpers.MakePaticularTuplePatternWithType(
                                Helpers.MakeSeq(
                                    "key",
                                    "value"
                                ),
                                Helpers.MakePrimitiveType("string"),
                                Helpers.MakePrimitiveType("int")
                            ),
                            Helpers.MakeIdentifierPath(
                                "dict",
                                Helpers.MakeGenericType(
                                    "dictionary",
                                    Helpers.MakePrimitiveType("string"),
                                    Helpers.MakePrimitiveType("int")
                                )
                            ),
                            Statement.MakeBlock(
                                Statement.MakeExprStmt(
                                    Helpers.MakeCallExpression(
                                        Helpers.SomeWellKnownExpressions.Println,
                                        Helpers.MakeCallExpression(
                                            Helpers.SomeWellKnownExpressions.StringFormat2,
                                            Expression.MakeConstant(
                                                "string",
                                                "{0}: {1}, "
                                            ),
                                            Helpers.MakeIdentifierPath(
                                                "key",
                                                Helpers.MakePrimitiveType("string")
                                            ),
                                            Helpers.MakeIdentifierPath(
                                                "value",
                                                Helpers.MakePrimitiveType("int")
                                            )
                                        )
                                    )
                                )
                            )
                        ),
                        Statement.MakeValueBindingForStmt(
                            Modifiers.Immutable,
                            Helpers.MakePaticularTuplePatternWithType(
                                Helpers.MakeSeq(
                                    "first",
                                    "second"
                                ),
                                Helpers.MakePrimitiveType("int"),
                                Helpers.MakePrimitiveType("int")
                            ),
                            Helpers.MakeIdentifierPath(
                                "a",
                                Helpers.MakeGenericType(
                                    "array",
                                    Helpers.MakeGenericType(
                                        "tuple",
                                        Helpers.MakePrimitiveType("int"),
                                        Helpers.MakePrimitiveType("int")
                                    )
                                )
                            ),
                            Statement.MakeBlock(
                                Statement.MakeExprStmt(
                                    Helpers.MakeCallExpression(
                                        Helpers.SomeWellKnownExpressions.Println,
                                        Helpers.MakeCallExpression(
                                            Helpers.SomeWellKnownExpressions.StringFormat2,
                                            Expression.MakeConstant(
                                                "string",
                                                "({0}, {1}), "
                                            ),
                                            Helpers.MakeIdentifierPath(
                                                "first",
                                                Helpers.MakePrimitiveType("int")
                                            ),
                                            Helpers.MakeIdentifierPath(
                                                "second",
                                                Helpers.MakePrimitiveType("int")
                                            )
                                        )
                                    )
                                )
                            )
                        ),
                        Statement.MakeValueBindingForStmt(
                            Modifiers.Immutable,
                            Helpers.MakePaticularTuplePatternWithType(
                                Helpers.MakeSeq(
                                    "first2",
                                    "second2"
                                ),
                                Helpers.MakePrimitiveType("int"),
                                Helpers.MakePrimitiveType("int")
                            ),
                            Helpers.MakeIdentifierPath(
                                "v",
                                Helpers.MakeGenericType(
                                    "vector",
                                    Helpers.MakeGenericType(
                                        "tuple",
                                        Helpers.MakePrimitiveType("int"),
                                        Helpers.MakePrimitiveType("int")
                                    )
                                )
                            ),
                            Statement.MakeBlock(
                                Statement.MakeExprStmt(
                                    Helpers.MakeCallExpression(
                                        Helpers.SomeWellKnownExpressions.Println,
                                        Helpers.MakeCallExpression(
                                            Helpers.SomeWellKnownExpressions.StringFormat2,
                                            Expression.MakeConstant(
                                                "string",
                                                "({0}, {1}), "
                                            ),
                                            Helpers.MakeIdentifierPath(
                                                "first2",
                                                Helpers.MakePrimitiveType("int")
                                            ),
                                            Helpers.MakeIdentifierPath(
                                                "second2",
                                                Helpers.MakePrimitiveType("int")
                                            )
                                        )
                                    )
                                )
                            )
                        )
                    ),
                    Helpers.MakeVoidType(),
                    Modifiers.None
                )
            });

            Assert.IsNotNull(ast);
            Helpers.AstStructuralEqual(ast, expected);
        }

        [Test]
        public void PropertyTests()
        {
            var parser = new Parser(new Scanner("../../sources/for_unit_tests/property_tests.exs")){
                DoPostParseProcessing = true
            };
            parser.Parse();

            var ast = parser.TopmostAst;

            var expected = AstNode.MakeModuleDef("main", new List<EntityDeclaration>{
                EntityDeclaration.MakeFunc(
                    "main",
                    Enumerable.Empty<ParameterDeclaration>(),
                    Statement.MakeBlock(
                        Statement.MakeExprStmt(
                            Helpers.MakeCallExpression(
                                Helpers.SomeWellKnownExpressions.Println,
                                Helpers.MakeCallExpression(
                                    Helpers.SomeWellKnownExpressions.StringFormat1,
                                    Expression.MakeConstant(
                                        "string",
                                        "before: {0}"
                                    ),
                                    Expression.MakeMemRef(
                                        Helpers.MakeIdentifierPath(
                                            "PropertyTest",
                                            Helpers.MakeGenericType(
                                                "InteroperabilityTest.PropertyTest"
                                            )
                                        ),
                                        AstNode.MakeIdentifier(
                                            "SomeProperty",
                                            Helpers.MakePropertyGetterType(
                                                "SomeProperty",
                                                Helpers.MakePrimitiveType("int")
                                            )
                                        )
                                    )
                                )
                            )
                        ),
                        Statement.MakeExprStmt(
                            Expression.MakeSingleAssignment(
                                Expression.MakeMemRef(
                                    Helpers.MakeIdentifierPath(
                                        "PropertyTest",
                                        Helpers.MakeGenericType(
                                            "InteroperabilityTest.PropertyTest"
                                        )
                                    ),
                                    AstNode.MakeIdentifier(
                                        "SomeProperty",
                                        Helpers.MakePropertySetterType(
                                            "SomeProperty",
                                            Helpers.MakePrimitiveType("int")
                                        )
                                    )
                                ),
                                Expression.MakeConstant("int", 100)
                            )
                        ),
                        Statement.MakeExprStmt(
                            Helpers.MakeCallExpression(
                                Helpers.SomeWellKnownExpressions.Println,
                                Helpers.MakeCallExpression(
                                    Helpers.SomeWellKnownExpressions.StringFormat1,
                                    Expression.MakeConstant(
                                        "string",
                                        "after: {0}"
                                    ),
                                    Expression.MakeMemRef(
                                        Helpers.MakeIdentifierPath(
                                            "PropertyTest",
                                            Helpers.MakeGenericType(
                                                "InteroperabilityTest.PropertyTest"
                                            )
                                        ),
                                        AstNode.MakeIdentifier(
                                            "SomeProperty",
                                            Helpers.MakePropertyGetterType(
                                                "SomeProperty",
                                                Helpers.MakePrimitiveType("int")
                                            )
                                        )
                                    )
                                )
                            )
                        ),
                        Statement.MakeVarDecl(
                            Helpers.MakeSeq(
                                Helpers.MakePaticularPatternWithType(
                                    "inst",
                                    Helpers.MakeGenericTypeWithRealName(
                                        "PropertyTest",
                                        "InteroperabilityTest.PropertyTest"
                                    )
                                )
                            ),
                            Helpers.MakeSeq(
                                Expression.MakeObjectCreation(
                                    Helpers.MakeGenericTypeWithRealName(
                                        "PropertyTest",
                                        "InteroperabilityTest.PropertyTest"
                                    ),
                                    Enumerable.Empty<Identifier>(),
                                    Enumerable.Empty<Expression>()
                                )
                            ),
                            Modifiers.None
                        ),
                        Statement.MakeExprStmt(
                            Helpers.MakeCallExpression(
                                Helpers.SomeWellKnownExpressions.Println,
                                Helpers.MakeCallExpression(
                                    Helpers.SomeWellKnownExpressions.StringFormat1,
                                    Expression.MakeConstant(
                                        "string",
                                        "before: {0}"
                                    ),
                                    Expression.MakeMemRef(
                                        Helpers.MakeIdentifierPath(
                                            "inst",
                                            Helpers.MakeGenericTypeWithRealName(
                                                "PropertyTest",
                                                "InteroperabilityTest.PropertyTest"
                                            )
                                        ),
                                        AstNode.MakeIdentifier(
                                            "SomeInstanceProperty",
                                            Helpers.MakePropertyGetterType(
                                                "SomeInstanceProperty",
                                                Helpers.MakePrimitiveType("int")
                                            )
                                        )
                                    )
                                )
                            )
                        ),
                        Statement.MakeExprStmt(
                            Expression.MakeSingleAssignment(
                                Expression.MakeMemRef(
                                    Helpers.MakeIdentifierPath(
                                        "inst",
                                        Helpers.MakeGenericTypeWithRealName(
                                            "PropertyTest",
                                            "InteroperabilityTest.PropertyTest"
                                        )
                                    ),
                                    AstNode.MakeIdentifier(
                                        "SomeInstanceProperty",
                                        Helpers.MakePropertySetterType(
                                            "SomeInstanceProperty",
                                            Helpers.MakePrimitiveType("int")
                                        )
                                    )
                                ),
                                Expression.MakeConstant("int", 1000)
                            )
                        ),
                        Statement.MakeExprStmt(
                            Helpers.MakeCallExpression(
                                Helpers.SomeWellKnownExpressions.Println,
                                Helpers.MakeCallExpression(
                                    Helpers.SomeWellKnownExpressions.StringFormat1,
                                    Expression.MakeConstant(
                                        "string",
                                        "after: {0}"
                                    ),
                                    Expression.MakeMemRef(
                                        Helpers.MakeIdentifierPath(
                                            "inst",
                                            Helpers.MakeGenericTypeWithRealName(
                                                "PropertyTest",
                                                "InteroperabilityTest.PropertyTest"
                                            )
                                        ),
                                        AstNode.MakeIdentifier(
                                            "SomeInstanceProperty",
                                            Helpers.MakePropertyGetterType(
                                                "SomeInstanceProperty",
                                                Helpers.MakePrimitiveType("int")
                                            )
                                        )
                                    )
                                )
                            )
                        )
                    ),
                    Helpers.MakeVoidType(),
                    Modifiers.None
                )
            });

            Assert.IsNotNull(ast);
            Helpers.AstStructuralEqual(ast, expected);
        }

        [Test]
        public void TestEnumInCSharp()
        {
            var parser = new Parser(new Scanner("../../sources/for_unit_tests/test_enum_in_csharp.exs")){
                DoPostParseProcessing = true
            };
            parser.Parse();

            var ast = parser.TopmostAst;

            var expected = AstNode.MakeModuleDef("main", new List<EntityDeclaration>{
                EntityDeclaration.MakeFunc(
                    "main",
                    Enumerable.Empty<ParameterDeclaration>(),
                    Statement.MakeBlock(
                        Statement.MakeIfStmt(
                            PatternConstruct.MakeExpressionPattern(
                                Helpers.MakeCallExpression(
                                    Expression.MakeMemRef(
                                        Helpers.MakeIdentifierPath(
                                            "EnumTest",
                                            Helpers.MakeGenericType("InteroperabilityTest.EnumTest")
                                        ),
                                        Helpers.MakeFunctionIdentifier(
                                            "TestEnumeration",
                                            Helpers.MakePrimitiveType("bool"),
                                            Helpers.MakeGenericTypeWithRealName(
                                                "TestEnum",
                                                "InteroperabilityTest.TestEnum"
                                            )
                                        )
                                    ),
                                    Expression.MakeMemRef(
                                        Helpers.MakeIdentifierPath(
                                            "TestEnum",
                                            Helpers.MakeGenericType("InteroperabilityTest.TestEnum")
                                        ),
                                        AstNode.MakeIdentifier(
                                            "SomeField",
                                            Helpers.MakeGenericTypeWithRealName(
                                                "TestEnum",
                                                "InteroperabilityTest.TestEnum"
                                            )
                                        )
                                    )
                                )
                            ),
                            Statement.MakeBlock(
                                Statement.MakeExprStmt(
                                    Helpers.MakeCallExpression(
                                        Helpers.SomeWellKnownExpressions.Println,
                                        Expression.MakeConstant("string", "matched!")
                                    )
                                )
                            )
                        ),
                        Statement.MakeVarDecl(
                            Helpers.MakeSeq(
                                Helpers.MakePaticularPatternWithType(
                                    "tester",
                                    Helpers.MakeGenericTypeWithRealName(
                                        "EnumTest",
                                        "InteroperabilityTest.EnumTest"
                                    )
                                )
                            ),
                            Helpers.MakeSeq(
                                Expression.MakeObjectCreation(
                                    Helpers.MakeGenericTypeWithRealName(
                                        "EnumTest",
                                        "InteroperabilityTest.EnumTest"
                                    ),
                                    Enumerable.Empty<Identifier>(),
                                    Enumerable.Empty<Expression>()
                                )
                            ),
                            Modifiers.None
                        ),
                        Statement.MakeExprStmt(
                            Expression.MakeSingleAssignment(
                                Expression.MakeMemRef(
                                    Helpers.MakeIdentifierPath(
                                        "tester",
                                        Helpers.MakeGenericTypeWithRealName(
                                            "EnumTest",
                                            "InteroperabilityTest.EnumTest"
                                        )
                                    ),
                                    AstNode.MakeIdentifier(
                                        "TestProperty",
                                        Helpers.MakePropertySetterType(
                                            "TestProperty",
                                            Helpers.MakeGenericTypeWithRealName(
                                                "TestEnum",
                                                "InteroperabilityTest.TestEnum"
                                            )
                                        )
                                    )
                                ),
                                Expression.MakeMemRef(
                                    Helpers.MakeIdentifierPath(
                                        "TestEnum",
                                        Helpers.MakeGenericType("InteroperabilityTest.TestEnum")
                                    ),
                                    AstNode.MakeIdentifier(
                                        "YetAnotherField",
                                        Helpers.MakeGenericTypeWithRealName(
                                            "TestEnum",
                                            "InteroperabilityTest.TestEnum"
                                        )
                                    )
                                )
                            )
                        ),
                        Statement.MakeIfStmt(
                            PatternConstruct.MakeExpressionPattern(
                                Helpers.MakeCallExpression(
                                    Expression.MakeMemRef(
                                        Helpers.MakeIdentifierPath(
                                            "tester",
                                            Helpers.MakeGenericTypeWithRealName(
                                                "EnumTest",
                                                "InteroperabilityTest.EnumTest"
                                            )
                                        ),
                                        Helpers.MakeFunctionIdentifier(
                                            "TestEnumerationOnInstance",
                                            Helpers.MakePrimitiveType("bool")
                                        )
                                    )
                                )
                            ),
                            Statement.MakeBlock(
                                Statement.MakeExprStmt(
                                    Helpers.MakeCallExpression(
                                        Helpers.SomeWellKnownExpressions.Println,
                                        Expression.MakeConstant("string", "matched again!")
                                    )
                                )
                            )
                        )
                    ),
                    Helpers.MakeVoidType(),
                    Modifiers.None
                )
            });

            Assert.IsNotNull(ast);
            Helpers.AstStructuralEqual(ast, expected);
        }

        [Test]
        public void Attributes()
        {
            var parser = new Parser(new Scanner("../../sources/for_unit_tests/attributes.exs")){
                DoPostParseProcessing = true
            };
            parser.Parse();

            var ast = parser.TopmostAst;

            var expected = AstNode.MakeModuleDef("main", new List<EntityDeclaration>{
                Helpers.MakeClassDecl(
                    "AuthorAttribute",
                    Helpers.MakeSeq(
                        Helpers.MakeGenericTypeWithRealName(
                            "Attribute",
                            "System.Attribute"
                        )
                    ),
                    Modifiers.None,
                    AstNode.MakeAttributeSection(
                        null,
                        Expression.MakeObjectCreation(
                            Helpers.MakeGenericTypeWithRealName(
                                "AttributeUsageAttribute",
                                "System.AttributeUsageAttribute"
                            ),
                            Helpers.MakeSeq(
                                AstNode.MakeIdentifier("validOn")
                            ),
                            Helpers.MakeSeq(
                                Expression.MakeMemRef(
                                    Helpers.MakeIdentifierPath(
                                        "AttributeTargets",
                                        Helpers.MakeGenericType("System.AttributeTargets")
                                    ),
                                    AstNode.MakeIdentifier(
                                        "All",
                                        Helpers.MakeGenericTypeWithRealName(
                                            "AttributeTargets",
                                            "System.AttributeTargets"
                                        )
                                    )
                                )
                            )
                        )
                    ),
                    EntityDeclaration.MakeField(
                        Helpers.MakeExactPatternWithType("name", AstType.MakePrimitiveType("string")),
                        Expression.Null,
                        Modifiers.Immutable | Modifiers.Private
                    )
                ),
                EntityDeclaration.MakeField(
                    Helpers.MakePaticularPatternWithType(
                        "y",
                        Helpers.MakePrimitiveType("int")
                    ),
                    Expression.MakeConstant("int", 100),
                    Modifiers.Immutable,
                    AstNode.MakeAttributeSection(
                        null,
                        Expression.MakeObjectCreation(
                            Helpers.MakeGenericType("AuthorAttribute"),
                            AstNode.MakeIdentifier("name"),
                            Expression.MakeConstant("string", "train12")
                        )
                    )
                ),
                Helpers.MakeFunc(
                    "doSomethingInModule",
                    Enumerable.Empty<ParameterDeclaration>(),
                    Statement.MakeBlock(
                        Statement.MakeEmptyStmt()
                    ),
                    Helpers.MakeVoidType(),
                    Modifiers.None,
                    AstNode.MakeAttributeSection(
                        null,
                        Expression.MakeObjectCreation(
                            Helpers.MakeGenericTypeWithRealName(
                                "ObsoleteAttribute",
                                "System.ObsoleteAttribute"
                            )
                        )
                    )
                ),
                Helpers.MakeClassDecl(
                    "AttributeTest",
                    Enumerable.Empty<AstType>(),
                    Modifiers.None,
                    AstNode.MakeAttributeSection(
                        null,
                        Expression.MakeObjectCreation(
                            Helpers.MakeGenericTypeWithRealName(
                                "SerializableAttribute",
                                "System.SerializableAttribute"
                            )
                        )
                    ),
                    EntityDeclaration.MakeField(
                        Helpers.MakeExactPatternWithType("x", AstType.MakePrimitiveType("int")),
                        Expression.Null,
                        Modifiers.Immutable | Modifiers.Private,
                        AstNode.MakeAttributeSection(
                            null,
                            Expression.MakeObjectCreation(
                                Helpers.MakeGenericTypeWithRealName(
                                    "ConditionalAttribute",
                                    "System.Diagnostics.ConditionalAttribute"
                                ),
                                AstNode.MakeIdentifier("conditionString"),
                                Expression.MakeConstant("string", "DEBUG")
                            )
                        )
                    ),
                    EntityDeclaration.MakeFunc(
                        "doSomething",
                        Helpers.MakeSeq(
                            EntityDeclaration.MakeParameter(
                                "dummy",
                                AstType.MakePrimitiveType("string"),
                                Expression.Null,
                                AstNode.MakeAttributeSection(
                                    null,
                                    Expression.MakeObjectCreation(
                                        Helpers.MakeGenericType("AuthorAttribute"),
                                        AstNode.MakeIdentifier("name"),
                                        Expression.MakeConstant("string", "train12")
                                    )
                                )
                            )
                        ),
                        Statement.MakeBlock(
                            Statement.MakeExprStmt(
                                Helpers.MakeCallExpression(
                                    Helpers.SomeWellKnownExpressions.Println,
                                    Expression.MakeConstant("string", "Do something")
                                )
                            )
                        ),
                        Helpers.MakeVoidType(),
                        Modifiers.Public
                    ),
                    EntityDeclaration.MakeFunc(
                        "doSomething2",
                        Helpers.MakeSeq(
                            EntityDeclaration.MakeParameter(
                                "n",
                                Helpers.MakePrimitiveType("int"),
                                Expression.MakeConstant("int", 100)
                            )
                        ),
                        Statement.MakeBlock(
                            Statement.MakeExprStmt(
                                Helpers.MakeCallExpression(
                                    Helpers.SomeWellKnownExpressions.Println,
                                    Helpers.MakeCallExpression(
                                        Helpers.SomeWellKnownExpressions.StringFormat1,
                                        Expression.MakeConstant(
                                            "string",
                                            "{0}"
                                        ),
                                        Helpers.MakeIdentifierPath(
                                            "n",
                                            Helpers.MakePrimitiveType("int")
                                        )
                                    )
                                )
                            ),
                            Helpers.MakeSingleItemReturnStatement(
                                Expression.MakeConstant("int", 10)
                            )
                        ),
                        AstType.MakePrimitiveType("int"),
                        Modifiers.Public
                    )
                ),
                EntityDeclaration.MakeFunc(
                    "main",
                    Enumerable.Empty<ParameterDeclaration>(),
                    Statement.MakeBlock(
                        Statement.MakeVarDecl(
                            Helpers.MakeSeq(
                                Helpers.MakePaticularPatternWithType(
                                    "x",
                                    Helpers.MakeGenericType("AttributeTest")
                                )
                            ),
                            Helpers.MakeSeq(
                                Expression.MakeObjectCreation(
                                    Helpers.MakeGenericType("AttributeTest"),
                                    AstNode.MakeIdentifier("x"),
                                    Expression.MakeConstant("int", 10)
                                )
                            ),
                            Modifiers.Immutable
                        ),
                        Statement.MakeExprStmt(
                            Helpers.MakeCallExpression(
                                Expression.MakeMemRef(
                                    Helpers.MakeIdentifierPath(
                                        "x",
                                        Helpers.MakeGenericType("AttributeTest")
                                    ),
                                    Helpers.MakeFunctionIdentifier(
                                        "doSomething",
                                        Helpers.MakeVoidType(),
                                        Helpers.MakePrimitiveType("string")
                                    )
                                ),
                                Expression.MakeConstant("string", "some string")
                            )
                        ),
                        Statement.MakeExprStmt(
                            Helpers.MakeCallExpression(
                                Expression.MakeMemRef(
                                    Helpers.MakeIdentifierPath(
                                        "x",
                                        Helpers.MakeGenericType("AttributeTest")
                                    ),
                                    Helpers.MakeFunctionIdentifier(
                                        "doSomething2",
                                        Helpers.MakePrimitiveType("int"),
                                        Helpers.MakePrimitiveType("int")
                                    )
                                )
                            )
                        )
                    ),
                    Helpers.MakePlaceholderType(),
                    Modifiers.None
                )
            }, Helpers.MakeSeq(
                AstNode.MakeImportDecl(
                    Helpers.MakeSeq(
                        AstNode.MakeIdentifier("System.SerializeAttribute"),
                        AstNode.MakeIdentifier("System.CLSCompliantAttribute"),
                        AstNode.MakeIdentifier("System.ObsoleteAttribute"),
                        AstNode.MakeIdentifier("System.Attribute"),
                        AstNode.MakeIdentifier("System.AttributeUsageAttribute"),
                        AstNode.MakeIdentifier("System.AttributeTargets")
                    ),
                    Helpers.MakeSeq(
                        AstNode.MakeIdentifier("SearializeAttribute"),
                        AstNode.MakeIdentifier("CLSCompiantAttribute"),
                        AstNode.MakeIdentifier("ObsoleteAttribute"),
                        AstNode.MakeIdentifier("Attribute"),
                        AstNode.MakeIdentifier("AttributeUsageAttribute"),
                        AstNode.MakeIdentifier("AttributeTargets")
                    )
                ),
                AstNode.MakeImportDecl(
                    Helpers.MakeSeq(
                        AstNode.MakeIdentifier("System.Diagnostics.ConditionalAttribute")
                    ),
                    Helpers.MakeSeq(
                        AstNode.MakeIdentifier("ConditionalAttribute")
                    )
                ),
                AstNode.MakeImportDecl(
                    Helpers.MakeSeq(
                        AstNode.MakeIdentifier("System.Reflection.AssemblyDescriptionAttribute")
                    ),
                    Helpers.MakeSeq(
                        AstNode.MakeIdentifier("AssemblyDescriptionAttribute")
                    )
                )
            ), Helpers.MakeSeq(
                AstNode.MakeAttributeSection(
                    "asm",
                    Expression.MakeObjectCreation(
                        Helpers.MakeGenericTypeWithRealName(
                            "AssemblyDescriptionAttribute",
                            "System.Reflection.AssemblyDescriptionAttribute"
                        ),
                        AstNode.MakeIdentifier("description"),
                        Expression.MakeConstant("string", "test assembly for attributes")
                    )
                ),
                AstNode.MakeAttributeSection(
                    null,
                    Expression.MakeObjectCreation(
                        Helpers.MakeGenericTypeWithRealName(
                            "CLSCompliantAttribute",
                            "System.CLSCompliantAttribute"
                        ),
                        AstNode.MakeIdentifier("isCompliant"),
                        Expression.MakeConstant("bool", true)
                    )
                )
            ));
        }

        [Test]
        public void Enum1()
        {
            var parser = new Parser(new Scanner("../../sources/for_unit_tests/enum1.exs")){
                DoPostParseProcessing = true
            };
            parser.Parse();

            var ast = parser.TopmostAst;

            var expected = AstNode.MakeModuleDef("main", Helpers.MakeSeq<EntityDeclaration>(
                Helpers.MakeEnumDecl(
                    "Union",
                    Modifiers.None,
                    Helpers.MakeTupleStyleMember(
                        "A",
                        "Union"
                    ),
                    Helpers.MakeTupleStyleMember(
                        "B",
                        "Union",
                        null,
                        Helpers.MakePrimitiveType("int"),
                        Helpers.MakePrimitiveType("uint")
                    ),
                    Helpers.MakeTupleStyleMember(
                        "C",
                        "Union",
                        null,
                        Helpers.MakePrimitiveType("string"),
                        Helpers.MakePrimitiveType("char")
                    ),
                    Helpers.MakeTupleStyleMember(
                        "D",
                        "Union",
                        null,
                        Helpers.MakePrimitiveType("intseq")
                    ),
                    EntityDeclaration.MakeFunc(
                        "print",
                        Enumerable.Empty<ParameterDeclaration>(),
                        Statement.MakeBlock(
                            Statement.MakeMatchStmt(
                                Expression.MakeSelfRef(),
                                Statement.MakeMatchClause(
                                    null,
                                    Statement.MakeExprStmt(
                                        Helpers.MakeCallExpression(
                                            Helpers.SomeWellKnownExpressions.Println,
                                            Expression.MakeConstant("string", "A")
                                        )
                                    ),
                                    PatternConstruct.MakeDestructuringPattern(
                                        AstType.MakeMemberType(
                                            Helpers.MakeGenericType("Union"),
                                            Helpers.MakeGenericType("A")
                                        )
                                    )
                                ),
                                Statement.MakeMatchClause(
                                    null,
                                    Statement.MakeExprStmt(
                                        Helpers.MakeCallExpression(
                                            Helpers.SomeWellKnownExpressions.Println,
                                            Helpers.MakeCallExpression(
                                                Helpers.SomeWellKnownExpressions.StringFormat2,
                                                Expression.MakeConstant("string", "{0}, {1}"),
                                                Helpers.MakeIdentifierPath(
                                                    "some_int",
                                                    Helpers.MakePrimitiveType("int")
                                                ),
                                                Helpers.MakeIdentifierPath(
                                                    "some_uint",
                                                    Helpers.MakePrimitiveType("uint")
                                                )
                                            )
                                        )
                                    ),
                                    PatternConstruct.MakeDestructuringPattern(
                                        AstType.MakeMemberType(
                                            Helpers.MakeGenericType("Union"),
                                            Helpers.MakeGenericType("B")
                                        ),
                                        PatternConstruct.MakeIdentifierPattern(
                                            AstNode.MakeIdentifier(
                                                "some_int",
                                                Helpers.MakePrimitiveType("int")
                                            )
                                        ),
                                        PatternConstruct.MakeIdentifierPattern(
                                            AstNode.MakeIdentifier(
                                                "some_uint",
                                                Helpers.MakePrimitiveType("uint")
                                            )
                                        )
                                    )
                                ),
                                Statement.MakeMatchClause(
                                    null,
                                    Statement.MakeExprStmt(
                                        Helpers.MakeCallExpression(
                                            Helpers.SomeWellKnownExpressions.Println,
                                            Helpers.MakeCallExpression(
                                                Helpers.SomeWellKnownExpressions.StringFormat2,
                                                Expression.MakeConstant("string", "{0}, {1}"),
                                                Helpers.MakeIdentifierPath(
                                                    "some_str",
                                                    Helpers.MakePrimitiveType("string")
                                                ),
                                                Helpers.MakeIdentifierPath(
                                                    "some_char",
                                                    Helpers.MakePrimitiveType("char")
                                                )
                                            )
                                        )
                                    ),
                                    PatternConstruct.MakeDestructuringPattern(
                                        AstType.MakeMemberType(
                                            Helpers.MakeGenericType("Union"),
                                            Helpers.MakeGenericType("C")
                                        ),
                                        PatternConstruct.MakeIdentifierPattern(
                                            AstNode.MakeIdentifier(
                                                "some_str",
                                                Helpers.MakePrimitiveType("string")
                                            )
                                        ),
                                        PatternConstruct.MakeIdentifierPattern(
                                            AstNode.MakeIdentifier(
                                                "some_char",
                                                Helpers.MakePrimitiveType("char")
                                            )
                                        )
                                    )
                                ),
                                Statement.MakeMatchClause(
                                    null,
                                    Statement.MakeExprStmt(
                                        Helpers.MakeCallExpression(
                                            Helpers.SomeWellKnownExpressions.Println,
                                            Helpers.MakeCallExpression(
                                                Helpers.SomeWellKnownExpressions.StringFormat1,
                                                Expression.MakeConstant("string", "{0}"),
                                                Helpers.MakeIdentifierPath(
                                                    "some_intseq",
                                                    Helpers.MakePrimitiveType("intseq")
                                                )
                                            )
                                        )
                                    ),
                                    PatternConstruct.MakeDestructuringPattern(
                                        AstType.MakeMemberType(
                                            Helpers.MakeGenericType("Union"),
                                            Helpers.MakeGenericType("D")
                                        ),
                                        PatternConstruct.MakeIdentifierPattern(
                                            AstNode.MakeIdentifier(
                                                "some_intseq",
                                                Helpers.MakePrimitiveType("intseq")
                                            )
                                        )
                                    )
                                )
                            )
                        ),
                        Helpers.MakeVoidType(),
                        Modifiers.Public
                    )
                ),
                EntityDeclaration.MakeFunc(
                    "main",
                    Enumerable.Empty<ParameterDeclaration>(),
                    Statement.MakeBlock(
                        Statement.MakeVarDecl(
                            Helpers.MakeSeq(
                                Helpers.MakePaticularPatternWithType(
                                    "some_enum",
                                    Helpers.MakeGenericTypeWithAnotherType(
                                        "Union",
                                        Helpers.MakeGenericType("tuple")
                                    )
                                )
                            ),
                            Helpers.MakeSeq(
                                Expression.MakeObjectCreation(
                                    AstType.MakeMemberType(
                                        Helpers.MakeGenericType("Union"),
                                        Helpers.MakeGenericType("A")
                                    )
                                )
                            ),
                            Modifiers.Immutable
                        ),
                        Statement.MakeVarDecl(
                            Helpers.MakeSeq(
                                Helpers.MakePaticularPatternWithType(
                                    "some_enum2",
                                    Helpers.MakeGenericTypeWithAnotherType(
                                        "Union",
                                        Helpers.MakeGenericType(
                                            "tuple",
                                            Helpers.MakePrimitiveType("int"),
                                            Helpers.MakePrimitiveType("uint")
                                        )
                                    )
                                )
                            ),
                            Helpers.MakeSeq(
                                Expression.MakeObjectCreation(
                                    AstType.MakeMemberType(
                                        Helpers.MakeGenericType("Union"),
                                        Helpers.MakeGenericType("B")
                                    ),
                                    Helpers.MakeSeq(
                                        AstNode.MakeIdentifier("0"),
                                        AstNode.MakeIdentifier("1")
                                    ),
                                    Helpers.MakeSeq(
                                        Expression.MakeConstant("int", 1),
                                        Expression.MakeConstant("uint", 2u)
                                    )
                                )
                            ),
                            Modifiers.Immutable
                        ),
                        Statement.MakeExprStmt(
                            Helpers.MakeCallExpression(
                                Helpers.SomeWellKnownExpressions.Println,
                                Helpers.MakeCallExpression(
                                    Helpers.SomeWellKnownExpressions.StringFormat1,
                                    Expression.MakeConstant("string", "{0}"),
                                    Helpers.MakeIdentifierPath(
                                        "some_enum",
                                        Helpers.MakeGenericTypeWithAnotherType(
                                            "Union",
                                            Helpers.MakeGenericType("tuple")
                                        )
                                    )
                                )
                            )
                        ),
                        Statement.MakeExprStmt(
                            Helpers.MakeCallExpression(
                                Helpers.SomeWellKnownExpressions.Println,
                                Helpers.MakeCallExpression(
                                    Helpers.SomeWellKnownExpressions.StringFormat1,
                                    Expression.MakeConstant("string", "{0}"),
                                    Helpers.MakeIdentifierPath(
                                        "some_enum2",
                                        Helpers.MakeGenericTypeWithAnotherType(
                                            "Union",
                                            Helpers.MakeGenericType(
                                                "tuple",
                                                Helpers.MakePrimitiveType("int"),
                                                Helpers.MakePrimitiveType("uint")
                                            )
                                        )
                                    )
                                )
                            )
                        ),
                        Statement.MakeMatchStmt(
                            Helpers.MakeIdentifierPath(
                                "some_enum",
                                Helpers.MakeGenericTypeWithAnotherType(
                                    "Union",
                                    Helpers.MakeGenericType("tuple")
                                )
                            ),
                            Statement.MakeMatchClause(
                                null,
                                Statement.MakeExprStmt(
                                    Helpers.MakeCallExpression(
                                        Helpers.SomeWellKnownExpressions.Println,
                                        Expression.MakeConstant("string", "A")
                                    )
                                ),
                                PatternConstruct.MakeDestructuringPattern(
                                    AstType.MakeMemberType(
                                        Helpers.MakeGenericType("Union"),
                                        Helpers.MakeGenericType("A")
                                    )
                                )
                            ),
                            Statement.MakeMatchClause(
                                null,
                                Statement.MakeExprStmt(
                                    Helpers.MakeCallExpression(
                                        Helpers.SomeWellKnownExpressions.Println,
                                        Helpers.MakeCallExpression(
                                            Helpers.SomeWellKnownExpressions.StringFormat2,
                                            Expression.MakeConstant("string", "{0}, {1}"),
                                            Helpers.MakeIdentifierPath(
                                                "some_int",
                                                Helpers.MakePrimitiveType("int")
                                            ),
                                            Helpers.MakeIdentifierPath(
                                                "some_uint",
                                                Helpers.MakePrimitiveType("uint")
                                            )
                                        )
                                    )
                                ),
                                PatternConstruct.MakeDestructuringPattern(
                                    AstType.MakeMemberType(
                                        Helpers.MakeGenericType("Union"),
                                        Helpers.MakeGenericType("B")
                                    ),
                                    PatternConstruct.MakeIdentifierPattern(
                                        AstNode.MakeIdentifier(
                                            "some_int",
                                            Helpers.MakePrimitiveType("int")
                                        )
                                    ),
                                    PatternConstruct.MakeIdentifierPattern(
                                        AstNode.MakeIdentifier(
                                            "some_uint",
                                            Helpers.MakePrimitiveType("uint")
                                        )
                                    )
                                )
                            ),
                            Statement.MakeMatchClause(
                                null,
                                Statement.MakeExprStmt(
                                    Helpers.MakeCallExpression(
                                        Helpers.SomeWellKnownExpressions.Println,
                                        Helpers.MakeCallExpression(
                                            Helpers.SomeWellKnownExpressions.StringFormat2,
                                            Expression.MakeConstant("string", "{0}, {1}"),
                                            Helpers.MakeIdentifierPath(
                                                "some_str",
                                                Helpers.MakePrimitiveType("string")
                                            ),
                                            Helpers.MakeIdentifierPath(
                                                "some_char",
                                                Helpers.MakePrimitiveType("char")
                                            )
                                        )
                                    )
                                ),
                                PatternConstruct.MakeDestructuringPattern(
                                    AstType.MakeMemberType(
                                        Helpers.MakeGenericType("Union"),
                                        Helpers.MakeGenericType("C")
                                    ),
                                    PatternConstruct.MakeIdentifierPattern(
                                        AstNode.MakeIdentifier(
                                            "some_str",
                                            Helpers.MakePrimitiveType("string")
                                        )
                                    ),
                                    PatternConstruct.MakeIdentifierPattern(
                                        AstNode.MakeIdentifier(
                                            "some_char",
                                            Helpers.MakePrimitiveType("char")
                                        )
                                    )
                                )
                            ),
                            Statement.MakeMatchClause(
                                null,
                                Statement.MakeExprStmt(
                                    Helpers.MakeCallExpression(
                                        Helpers.SomeWellKnownExpressions.Println,
                                        Helpers.MakeCallExpression(
                                            Helpers.SomeWellKnownExpressions.StringFormat1,
                                            Expression.MakeConstant("string", "{0}"),
                                            Helpers.MakeIdentifierPath(
                                                "some_intseq",
                                                Helpers.MakePrimitiveType("intseq")
                                            )
                                        )
                                    )
                                ),
                                PatternConstruct.MakeDestructuringPattern(
                                    AstType.MakeMemberType(
                                        Helpers.MakeGenericType("Union"),
                                        Helpers.MakeGenericType("D")
                                    ),
                                    PatternConstruct.MakeIdentifierPattern(
                                        AstNode.MakeIdentifier(
                                            "some_intseq",
                                            Helpers.MakePrimitiveType("intseq")
                                        )
                                    )
                                )
                            )
                        ),
                        Statement.MakeExprStmt(
                            Helpers.MakeCallExpression(
                                Expression.MakeMemRef(
                                    Helpers.MakeIdentifierPath(
                                        "some_enum2",
                                        Helpers.MakeGenericTypeWithAnotherType(
                                            "Union",
                                            Helpers.MakeGenericType(
                                                "tuple",
                                                Helpers.MakePrimitiveType("int"),
                                                Helpers.MakePrimitiveType("uint")
                                            )
                                        )
                                    ),
                                    AstNode.MakeIdentifier(
                                        "print",
                                        Helpers.MakeFunctionType(
                                            "print",
                                            Helpers.MakeVoidType()
                                        )
                                    )
                                )
                            )
                        )
                    ),
                    Helpers.MakeVoidType(),
                    Modifiers.None
                )
            ));

            Assert.IsNotNull(ast);
            Helpers.AstStructuralEqual(ast, expected);
        }

        [Test]
        public void Enum2()
        {
            var parser = new Parser(new Scanner("../../sources/for_unit_tests/enum2.exs")){
                DoPostParseProcessing = true
            };
            parser.Parse();

            var ast = parser.TopmostAst;

            var expected = AstNode.MakeModuleDef("main", Helpers.MakeSeq<EntityDeclaration>(
                Helpers.MakeEnumDecl(
                    "SomeEnum",
                    Modifiers.None,
                    EntityDeclaration.MakeField(
                        PatternConstruct.MakePatternWithType(
                            PatternConstruct.MakeIdentifierPattern(
                                "A",
                                Helpers.MakeGenericTypeWithAnotherType(
                                    "SomeEnum",
                                    Helpers.MakePrimitiveType("int")
                                )
                            ),
                            Helpers.MakePrimitiveType("int")
                        ),
                        Expression.MakeConstant("int", 1),
                        Modifiers.Public | Modifiers.Static
                    ),
                    EntityDeclaration.MakeField(
                        PatternConstruct.MakePatternWithType(
                            PatternConstruct.MakeIdentifierPattern(
                                "B",
                                Helpers.MakeGenericTypeWithAnotherType(
                                    "SomeEnum",
                                    Helpers.MakePrimitiveType("int")
                                )
                            ),
                            Helpers.MakePrimitiveType("int")
                        ),
                        Expression.MakeConstant("int", 2),
                        Modifiers.Public | Modifiers.Static
                    ),
                    EntityDeclaration.MakeField(
                        PatternConstruct.MakePatternWithType(
                            PatternConstruct.MakeIdentifierPattern(
                                "C",
                                Helpers.MakeGenericTypeWithAnotherType(
                                    "SomeEnum",
                                    Helpers.MakePrimitiveType("int")
                                )
                            ),
                            Helpers.MakePrimitiveType("int")
                        ),
                        Expression.MakeConstant("int", 3),
                        Modifiers.Public | Modifiers.Static
                    ),
                    EntityDeclaration.MakeFunc(
                        "print",
                        Enumerable.Empty<ParameterDeclaration>(),
                        Statement.MakeBlock(
                            Statement.MakeMatchStmt(
                                Expression.MakeMemRef(
                                    Expression.MakeSelfRef(),
                                    Helpers.MakeSomeIdent(Utilities.RawValueEnumValueFieldName)
                                ),
                                Statement.MakeMatchClause(
                                    null,
                                    Statement.MakeExprStmt(
                                        Helpers.MakeCallExpression(
                                            Helpers.SomeWellKnownExpressions.Println,
                                            Expression.MakeConstant("string", "A")
                                        )
                                    ),
                                    PatternConstruct.MakeTypePathPattern(
                                        AstType.MakeMemberType(
                                            Helpers.MakeGenericType("SomeEnum"),
                                            Helpers.MakeGenericType("A")
                                        )
                                    )
                                ),
                                Statement.MakeMatchClause(
                                    null,
                                    Statement.MakeExprStmt(
                                        Helpers.MakeCallExpression(
                                            Helpers.SomeWellKnownExpressions.Println,
                                            Expression.MakeConstant("string", "B")
                                        )
                                    ),
                                    PatternConstruct.MakeTypePathPattern(
                                        AstType.MakeMemberType(
                                            Helpers.MakeGenericType("SomeEnum"),
                                            Helpers.MakeGenericType("B")
                                        )
                                    )
                                ),
                                Statement.MakeMatchClause(
                                    null,
                                    Statement.MakeExprStmt(
                                        Helpers.MakeCallExpression(
                                            Helpers.SomeWellKnownExpressions.Println,
                                            Expression.MakeConstant("string", "C")
                                        )
                                    ),
                                    PatternConstruct.MakeTypePathPattern(
                                        AstType.MakeMemberType(
                                            Helpers.MakeGenericType("SomeEnum"),
                                            Helpers.MakeGenericType("C")
                                        )
                                    )
                                )
                            )
                        ),
                        Helpers.MakeVoidType(),
                        Modifiers.Public
                    ),
                    EntityDeclaration.MakeFunc(
                        "printUsingIf",
                        Enumerable.Empty<ParameterDeclaration>(),
                        Statement.MakeBlock(
                            Statement.MakeIfStmt(
                                PatternConstruct.MakeExpressionPattern(
                                    Expression.MakeBinaryExpr(
                                        OperatorType.Equality,
                                        Expression.MakeMemRef(
                                            Expression.MakeSelfRef(),
                                            Helpers.MakeSomeIdent(Utilities.RawValueEnumValueFieldName)
                                        ),
                                        Expression.MakePath(
                                            AstNode.MakeIdentifier("SomeEnum"),
                                            AstNode.MakeIdentifier(
                                                "A",
                                                Helpers.MakeGenericTypeWithAnotherType(
                                                    "SomeEnum",
                                                    Helpers.MakePrimitiveType("int")
                                                )
                                            )
                                        )
                                    )
                                ),
                                Statement.MakeBlock(
                                    Statement.MakeExprStmt(
                                        Helpers.MakeCallExpression(
                                            Helpers.SomeWellKnownExpressions.Println,
                                            Expression.MakeConstant("string", "A in if")
                                        )
                                    )
                                )
                            )
                        ),
                        Helpers.MakeVoidType(),
                        Modifiers.Public
                    )
                ),
                Helpers.MakeClassDecl(
                    "SomeClass",
                    Enumerable.Empty<AstType>(),
                    Modifiers.None,
                    EntityDeclaration.MakeField(
                        Helpers.MakeExactPatternWithType(
                            "x",
                            Helpers.MakeGenericType("SomeEnum")
                        ),
                        Expression.Null,
                        Modifiers.Private | Modifiers.Immutable
                    ),
                    EntityDeclaration.MakeFunc(
                        "matchEnum",
                        Enumerable.Empty<ParameterDeclaration>(),
                        Statement.MakeBlock(
                            Statement.MakeMatchStmt(
                                Expression.MakeMemRef(
                                    Expression.MakeMemRef(
                                        Expression.MakeSelfRef(),
                                        AstNode.MakeIdentifier(
                                            "x",
                                            Helpers.MakeGenericType("SomeEnum")
                                        )
                                    ),
                                    Helpers.MakeSomeIdent(Utilities.RawValueEnumValueFieldName)
                                ),
                                Statement.MakeMatchClause(
                                    null,
                                    Statement.MakeExprStmt(
                                        Helpers.MakeCallExpression(
                                            Helpers.SomeWellKnownExpressions.Println,
                                            Expression.MakeConstant("string", "A in matchEnum")
                                        )
                                    ),
                                    PatternConstruct.MakeTypePathPattern(
                                        AstType.MakeMemberType(
                                            Helpers.MakeGenericType("SomeEnum"),
                                            Helpers.MakeGenericType("A")
                                        )
                                    )
                                ),
                                Statement.MakeMatchClause(
                                    null,
                                    Statement.MakeExprStmt(
                                        Helpers.MakeCallExpression(
                                            Helpers.SomeWellKnownExpressions.Println,
                                            Expression.MakeConstant("string", "B in matchEnum")
                                        )
                                    ),
                                    PatternConstruct.MakeTypePathPattern(
                                        AstType.MakeMemberType(
                                            Helpers.MakeGenericType("SomeEnum"),
                                            Helpers.MakeGenericType("B")
                                        )
                                    )
                                ),
                                Statement.MakeMatchClause(
                                    null,
                                    Statement.MakeExprStmt(
                                        Helpers.MakeCallExpression(
                                            Helpers.SomeWellKnownExpressions.Println,
                                            Expression.MakeConstant("string", "C in matchEnum")
                                        )
                                    ),
                                    PatternConstruct.MakeTypePathPattern(
                                        AstType.MakeMemberType(
                                            Helpers.MakeGenericType("SomeEnum"),
                                            Helpers.MakeGenericType("C")
                                        )
                                    )
                                )
                            )
                        ),
                        Helpers.MakeVoidType(),
                        Modifiers.Public
                    ),
                    EntityDeclaration.MakeFunc(
                        "ifEnum",
                        Enumerable.Empty<ParameterDeclaration>(),
                        Statement.MakeBlock(
                            Statement.MakeIfStmt(
                                PatternConstruct.MakeExpressionPattern(
                                    Expression.MakeBinaryExpr(
                                        OperatorType.Equality,
                                        Expression.MakeMemRef(
                                            Expression.MakeMemRef(
                                                Expression.MakeSelfRef(),
                                                AstNode.MakeIdentifier(
                                                    "x",
                                                    Helpers.MakeGenericType("SomeEnum")
                                                )
                                            ),
                                            Helpers.MakeSomeIdent(Utilities.RawValueEnumValueFieldName)
                                        ),
                                        Expression.MakePath(
                                            AstNode.MakeIdentifier("SomeEnum"),
                                            AstNode.MakeIdentifier(
                                                "A",
                                                Helpers.MakeGenericTypeWithAnotherType(
                                                    "SomeEnum",
                                                    Helpers.MakePrimitiveType("int")
                                                )
                                            )
                                        )
                                    )
                                ),
                                Statement.MakeBlock(
                                    Statement.MakeExprStmt(
                                        Helpers.MakeCallExpression(
                                            Helpers.SomeWellKnownExpressions.Println,
                                            Expression.MakeConstant("string", "A in ifEnum")
                                        )
                                    )
                                )
                            )
                        ),
                        Helpers.MakeVoidType(),
                        Modifiers.Public
                    )
                ),
                EntityDeclaration.MakeFunc(
                    "main",
                    Enumerable.Empty<ParameterDeclaration>(),
                    Statement.MakeBlock(
                        Statement.MakeVarDecl(
                            Helpers.MakeSeq(
                                Helpers.MakePaticularPatternWithType(
                                    "enum_a",
                                    Helpers.MakeGenericType("SomeEnum")
                                )
                            ),
                            Helpers.MakeSeq(
                                Expression.MakePath(
                                    AstNode.MakeIdentifier("SomeEnum"),
                                    AstNode.MakeIdentifier(
                                        "A",
                                        Helpers.MakeGenericTypeWithAnotherType(
                                            "SomeEnum",
                                            Helpers.MakePrimitiveType("int")
                                        )
                                    )
                                )
                            ),
                            Modifiers.Immutable
                        ),
                        Statement.MakeMatchStmt(
                            Expression.MakeMemRef(
                                Helpers.MakeIdentifierPath(
                                    "enum_a",
                                    Helpers.MakeGenericType("SomeEnum")
                                ),
                                Helpers.MakeSomeIdent(Utilities.RawValueEnumValueFieldName)
                            ),
                            Statement.MakeMatchClause(
                                null,
                                Statement.MakeExprStmt(
                                    Helpers.MakeCallExpression(
                                        Helpers.SomeWellKnownExpressions.Println,
                                        Expression.MakeConstant("string", "A")
                                    )
                                ),
                                PatternConstruct.MakeTypePathPattern(
                                    AstType.MakeMemberType(
                                        Helpers.MakeGenericType("SomeEnum"),
                                        Helpers.MakeGenericType("A")
                                    )
                                )
                            ),
                            Statement.MakeMatchClause(
                                null,
                                Statement.MakeExprStmt(
                                    Helpers.MakeCallExpression(
                                        Helpers.SomeWellKnownExpressions.Println,
                                        Expression.MakeConstant("string", "B")
                                    )
                                ),
                                PatternConstruct.MakeTypePathPattern(
                                    AstType.MakeMemberType(
                                        Helpers.MakeGenericType("SomeEnum"),
                                        Helpers.MakeGenericType("B")
                                    )
                                )
                            ),
                            Statement.MakeMatchClause(
                                null,
                                Statement.MakeExprStmt(
                                    Helpers.MakeCallExpression(
                                        Helpers.SomeWellKnownExpressions.Println,
                                        Expression.MakeConstant("string", "C")
                                    )
                                ),
                                PatternConstruct.MakeTypePathPattern(
                                    AstType.MakeMemberType(
                                        Helpers.MakeGenericType("SomeEnum"),
                                        Helpers.MakeGenericType("C")
                                    )
                                )
                            )
                        ),
                        Statement.MakeExprStmt(
                            Helpers.MakeCallExpression(
                                Expression.MakeMemRef(
                                    Helpers.MakeIdentifierPath(
                                        "enum_a",
                                        Helpers.MakeGenericType("SomeEnum")
                                    ),
                                    AstNode.MakeIdentifier(
                                        "print",
                                        Helpers.MakeFunctionType(
                                            "print",
                                            Helpers.MakeVoidType()
                                        )
                                    )
                                )
                            )
                        ),
                        Statement.MakeIfStmt(
                            PatternConstruct.MakeExpressionPattern(
                                Expression.MakeBinaryExpr(
                                    OperatorType.Equality,
                                    Expression.MakeMemRef(
                                        Helpers.MakeIdentifierPath(
                                            "enum_a",
                                            Helpers.MakeGenericType("SomeEnum")
                                        ),
                                        Helpers.MakeSomeIdent(Utilities.RawValueEnumValueFieldName)
                                    ),
                                    Expression.MakePath(
                                        AstNode.MakeIdentifier("SomeEnum"),
                                        AstNode.MakeIdentifier(
                                            "A",
                                            Helpers.MakeGenericTypeWithAnotherType(
                                                "SomeEnum",
                                                Helpers.MakePrimitiveType("int")
                                            )
                                        )
                                    )
                                )
                            ),
                            Statement.MakeBlock(
                                Statement.MakeExprStmt(
                                    Helpers.MakeCallExpression(
                                        Helpers.SomeWellKnownExpressions.Println,
                                        Expression.MakeConstant("string", "A in if")
                                    )
                                )
                            )
                        ),
                        Statement.MakeExprStmt(
                            Helpers.MakeCallExpression(
                                Expression.MakeMemRef(
                                    Helpers.MakeIdentifierPath(
                                        "enum_a",
                                        Helpers.MakeGenericType("SomeEnum")
                                    ),
                                    AstNode.MakeIdentifier(
                                        "printUsingIf",
                                        Helpers.MakeFunctionType(
                                            "printUsingIf",
                                            Helpers.MakeVoidType()
                                        )
                                    )
                                )
                            )
                        ),
                        Statement.MakeVarDecl(
                            Helpers.MakeSeq(
                                Helpers.MakePaticularPatternWithType(
                                    "some_class",
                                    Helpers.MakeGenericType("SomeClass")
                                )
                            ),
                            Helpers.MakeSeq(
                                Expression.MakeObjectCreation(
                                    Helpers.MakeGenericType("SomeClass"),
                                    AstNode.MakeIdentifier("x"),
                                    Helpers.MakeIdentifierPath(
                                        "enum_a",
                                        Helpers.MakeGenericType("SomeEnum")
                                    )
                                )
                            ),
                            Modifiers.Immutable
                        ),
                        Statement.MakeExprStmt(
                            Helpers.MakeCallExpression(
                                Expression.MakeMemRef(
                                    Helpers.MakeIdentifierPath(
                                        "some_class",
                                        Helpers.MakeGenericType("SomeClass")
                                    ),
                                    AstNode.MakeIdentifier(
                                        "matchEnum",
                                        Helpers.MakeFunctionType(
                                            "matchEnum",
                                            Helpers.MakeVoidType()
                                        )
                                    )
                                )
                            )
                        ),
                        Statement.MakeExprStmt(
                            Helpers.MakeCallExpression(
                                Expression.MakeMemRef(
                                    Helpers.MakeIdentifierPath(
                                        "some_class",
                                        Helpers.MakeGenericType("SomeClass")
                                    ),
                                    AstNode.MakeIdentifier(
                                        "ifEnum",
                                        Helpers.MakeFunctionType(
                                            "ifEnum",
                                            Helpers.MakeVoidType()
                                        )
                                    )
                                )
                            )
                        )
                    ),
                    Helpers.MakeVoidType(),
                    Modifiers.None
                )
            ));

            Assert.IsNotNull(ast);
            Helpers.AstStructuralEqual(ast, expected);
        }

        [Test]
        public void GenericClass()
        {
            var parser = new Parser(new Scanner("../../sources/for_unit_tests/generic_class.exs")){
                DoPostParseProcessing = true
            };
            parser.Parse();

            var ast = parser.TopmostAst;

            var expected = AstNode.MakeModuleDef("main", Helpers.MakeSeq<EntityDeclaration>(
                Helpers.MakeInterfaceDecl(
                    "SomeInterface",
                    Enumerable.Empty<AstType>(),
                    Modifiers.None,
                    Helpers.MakeSeq(
                        AstNode.MakeTypeConstraint(
                            AstType.MakeParameterType("T"),
                            null
                        )
                    ),
                    EntityDeclaration.MakeFunc(
                        "getX",
                        Enumerable.Empty<ParameterDeclaration>(),
                        null,
                        AstType.MakeParameterType("T"),
                        Modifiers.Public
                    )
                ),
                Helpers.MakeClassDecl(
                    "GenericClass",
                    Helpers.MakeSeq(
                        Helpers.MakeGenericType(
                            "SomeInterface",
                            AstType.MakeParameterType("T")
                        )
                    ),
                    Modifiers.None,
                    Helpers.MakeSeq(
                        AstNode.MakeTypeConstraint(
                            AstType.MakeParameterType("T"),
                            null
                        )
                    ),
                    EntityDeclaration.MakeField(
                        Helpers.MakeExactPatternWithType(
                            "x",
                            AstType.MakeParameterType("T")
                        ),
                        Expression.Null,
                        Modifiers.Private | Modifiers.Immutable
                    ),
                    EntityDeclaration.MakeFunc(
                        "getX",
                        Enumerable.Empty<ParameterDeclaration>(),
                        Statement.MakeBlock(
                            Helpers.MakeSingleItemReturnStatement(
                                Expression.MakeMemRef(
                                    Expression.MakeSelfRef(),
                                    AstNode.MakeIdentifier(
                                        "x",
                                        AstType.MakeParameterType("T")
                                    )
                                )
                            )
                        ),
                        AstType.MakeParameterType("T"),
                        Modifiers.Public
                    )
                ),
                Helpers.MakeEnumDecl(
                    "MyOption",
                    Modifiers.None,
                    Helpers.MakeSeq(
                        AstNode.MakeTypeConstraint(
                            AstType.MakeParameterType("T"),
                            null
                        )
                    ),
                    Helpers.MakeTupleStyleMember(
                        "Ok",
                        "MyOption",
                        null,
                        AstType.MakeParameterType("T")
                    ),
                    Helpers.MakeTupleStyleMember(
                        "Error",
                        "MyOption"
                    )
                ),
                EntityDeclaration.MakeFunc(
                    "main",
                    Enumerable.Empty<ParameterDeclaration>(),
                    Statement.MakeBlock(
                        Statement.MakeVarDecl(
                            Helpers.MakeSeq(
                                Helpers.MakePaticularPatternWithType(
                                    "a",
                                    Helpers.MakeGenericType("GenericClass")
                                )
                            ),
                            Helpers.MakeSeq(
                                Expression.MakeObjectCreation(
                                    Helpers.MakeGenericType("GenericClass"),
                                    AstNode.MakeIdentifier("x"),
                                    Expression.MakeConstant("int", 10),
                                    AstType.MakeKeyValueType(
                                        AstType.MakeParameterType("T"),
                                        AstType.MakePrimitiveType("int")
                                    )
                                )
                            ),
                            Modifiers.Immutable
                        ),
                        Statement.MakeVarDecl(
                            Helpers.MakeSeq(
                                Helpers.MakePaticularPatternWithType(
                                    "b",
                                    Helpers.MakeGenericType("GenericClass")
                                )
                            ),
                            Helpers.MakeSeq(
                                Expression.MakeObjectCreation(
                                    Helpers.MakeGenericType("GenericClass"),
                                    AstNode.MakeIdentifier("x"),
                                    Expression.MakeConstant("int", 20),
                                    AstType.MakeKeyValueType(
                                        AstType.MakeParameterType("T"),
                                        AstType.MakePrimitiveType("int")
                                    )
                                )
                            ),
                            Modifiers.Immutable
                        ),
                        Statement.MakeVarDecl(
                            Helpers.MakeSeq(
                                Helpers.MakePaticularPatternWithType(
                                    "c",
                                    Helpers.MakeGenericTypeWithAnotherType(
                                        "MyOption",
                                        Helpers.MakeGenericType(
                                            "tuple",
                                            AstType.MakeParameterType("T")
                                        )
                                    )
                                )
                            ),
                            Helpers.MakeSeq(
                                Expression.MakeObjectCreation(
                                    AstType.MakeMemberType(
                                        Helpers.MakeGenericType("MyOption"),
                                        Helpers.MakeGenericType("Ok")
                                    ),
                                    AstNode.MakeIdentifier("0"),
                                    Expression.MakeConstant("int", 10),
                                    AstType.MakeKeyValueType(
                                        AstType.MakeParameterType("T"),
                                        AstType.MakePrimitiveType("int")
                                    )
                                )
                            ),
                            Modifiers.Immutable
                        ),
                        Statement.MakeVarDecl(
                            Helpers.MakeSeq(
                                Helpers.MakePaticularPatternWithType(
                                    "d",
                                    Helpers.MakeGenericTypeWithAnotherType(
                                        "MyOption",
                                        Helpers.MakeGenericType(
                                            "tuple",
                                            AstType.MakeParameterType("T")
                                        )
                                    )
                                )
                            ),
                            Helpers.MakeSeq(
                                Expression.MakeObjectCreation(
                                    AstType.MakeMemberType(
                                        Helpers.MakeGenericType("MyOption"),
                                        Helpers.MakeGenericType("Ok")
                                    ),
                                    AstNode.MakeIdentifier("0"),
                                    Expression.MakeConstant("int", 20),
                                    AstType.MakeKeyValueType(
                                        AstType.MakeParameterType("T"),
                                        AstType.MakePrimitiveType("int")
                                    )
                                )
                            ),
                            Modifiers.Immutable
                        ),
                        Statement.MakeExprStmt(
                            Helpers.MakeCallExpression(
                                Helpers.SomeWellKnownExpressions.Println,
                                Helpers.MakeCallExpression(
                                    Helpers.SomeWellKnownExpressions.StringFormatN,
                                    Expression.MakeConstant("string", "{0}, {1}, {2}, {3}"),
                                    Helpers.MakeIdentifierPath(
                                        "a",
                                        Helpers.MakeGenericType("GenericClass")
                                    ),
                                    Helpers.MakeIdentifierPath(
                                        "b",
                                        Helpers.MakeGenericType("GenericClass")
                                    ),
                                    Helpers.MakeIdentifierPath(
                                        "c",
                                        Helpers.MakeGenericTypeWithAnotherType(
                                            "MyOption",
                                            Helpers.MakeGenericType(
                                                "tuple",
                                                AstType.MakeParameterType("T")
                                            )
                                        )
                                    ),
                                    Helpers.MakeIdentifierPath(
                                        "d",
                                        Helpers.MakeGenericTypeWithAnotherType(
                                            "MyOption",
                                            Helpers.MakeGenericType(
                                                "tuple",
                                                AstType.MakeParameterType("T")
                                            )
                                        )
                                    )
                                )
                            )
                        )
                    ),
                    Helpers.MakeVoidType(),
                    Modifiers.None
                )
            ));

            Assert.IsNotNull(ast);
            Helpers.AstStructuralEqual(ast, expected);
        }
    }
}

