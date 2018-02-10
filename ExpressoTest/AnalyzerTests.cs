﻿using System;
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
                new List<EntityDeclaration>{
                    EntityDeclaration.MakeFunc(
                        "main",
                        Enumerable.Empty<ParameterDeclaration>(),
                        Statement.MakeBlock(
                            Statement.MakeVarDecl(
                                Helpers.MakeSeq(PatternConstruct.MakeIdentifierPattern("a", Helpers.MakePrimitiveType("int"))),
                                Helpers.MakeSeq(Expression.MakeConstant("int", 255)),
                                Modifiers.Immutable
                            ),
                            Statement.MakeVarDecl(
                                Helpers.MakeSeq(PatternConstruct.MakeIdentifierPattern("h_a", Helpers.MakePrimitiveType("int"))),
                                Helpers.MakeSeq(Expression.MakeConstant("int", 0xff)),
                                Modifiers.Immutable
                            ),
                            Statement.MakeVarDecl(
                                Helpers.MakeSeq(
                                    PatternConstruct.MakePatternWithType(
                                        PatternConstruct.MakeIdentifierPattern(
                                            "h_a_",
                                            Helpers.MakePrimitiveType("int")
                                        ),
                                        Helpers.MakePrimitiveType("int")
                                    )
                                ),
                                Helpers.MakeSeq(Expression.MakeConstant("int", 0xff)),
                                Modifiers.Immutable
                            ),
                            Statement.MakeVarDecl(
                                Helpers.MakeSeq(PatternConstruct.MakeIdentifierPattern("b", Helpers.MakePrimitiveType("double"))),
                                Helpers.MakeSeq(Expression.MakeConstant("double", 1000.0)),
                                Modifiers.Immutable
                            ),
                            Statement.MakeVarDecl(
                                Helpers.MakeSeq(PatternConstruct.MakeIdentifierPattern("f_b", Helpers.MakePrimitiveType("double"))),
                                Helpers.MakeSeq(Expression.MakeConstant("double", 1.0e4)),
                                Modifiers.Immutable
                            ),
                            Statement.MakeVarDecl(
                                Helpers.MakeSeq(
                                    PatternConstruct.MakePatternWithType(
                                        PatternConstruct.MakeIdentifierPattern(
                                            "f_b_",
                                            Helpers.MakePrimitiveType("double")
                                        ),
                                        Helpers.MakePrimitiveType("double")
                                    )
                                ),
                                Helpers.MakeSeq(Expression.MakeConstant("double", 1.0e4)),
                                Modifiers.Immutable
                            ),
                            Statement.MakeVarDecl(
                                Helpers.MakeSeq(PatternConstruct.MakeIdentifierPattern("c", Helpers.MakePrimitiveType("double"))),
                                Helpers.MakeSeq(Expression.MakeConstant("double", 0.001)),
                                Modifiers.Immutable
                            ),
                            Statement.MakeVarDecl(
                                Helpers.MakeSeq(PatternConstruct.MakeIdentifierPattern("f_c", Helpers.MakePrimitiveType("double"))),
                                Helpers.MakeSeq(Expression.MakeConstant("double", .1e-2)),
                                Modifiers.Immutable
                            ),
                            Statement.MakeVarDecl(
                                Helpers.MakeSeq(PatternConstruct.MakeIdentifierPattern("f_c2", Helpers.MakePrimitiveType("double"))),
                                Helpers.MakeSeq(Expression.MakeConstant("double", .0001)),
                                Modifiers.Immutable
                            ),
                            Statement.MakeVarDecl(
                                Helpers.MakeSeq(PatternConstruct.MakeIdentifierPattern("d", Helpers.MakePrimitiveType("bigint"))),
                                Helpers.MakeSeq(Expression.MakeConstant("bigint", BigInteger.Parse("10000000"))),
                                Modifiers.Immutable
                            ),
                            Statement.MakeVarDecl(
                                Helpers.MakeSeq(
                                    PatternConstruct.MakePatternWithType(
                                        PatternConstruct.MakeIdentifierPattern(
                                            "d_",
                                            Helpers.MakePrimitiveType("bigint")
                                        ),
                                        Helpers.MakePrimitiveType("bigint")
                                    )
                                ),
                                Helpers.MakeSeq(Expression.MakeConstant("bigint", BigInteger.Parse("10000000"))),
                                Modifiers.Immutable
                            ),
                            Statement.MakeVarDecl(
                                Helpers.MakeSeq(PatternConstruct.MakeIdentifierPattern("d2", Helpers.MakePrimitiveType("bigint"))),
                                Helpers.MakeSeq(Expression.MakeConstant("bigint", BigInteger.Parse("10000000"))),
                                Modifiers.Immutable
                            ),
                            Statement.MakeVarDecl(
                                Helpers.MakeSeq(PatternConstruct.MakeIdentifierPattern("e", Helpers.MakePrimitiveType("string"))),
                                Helpers.MakeSeq(Expression.MakeConstant("string", "This is a test")),
                                Modifiers.Immutable
                            ),
                            Statement.MakeVarDecl(
                                Helpers.MakeSeq(PatternConstruct.MakeIdentifierPattern("u", Helpers.MakePrimitiveType("uint"))),
                                Helpers.MakeSeq(Expression.MakeConstant("uint", 1000u)),
                                Modifiers.Immutable
                            ),
                            Statement.MakeVarDecl(
                                Helpers.MakeSeq(PatternConstruct.MakeIdentifierPattern("u_", Helpers.MakePrimitiveType("uint"))),
                                Helpers.MakeSeq(Expression.MakeConstant("uint", 1000U)),
                                Modifiers.Immutable
                            ),
                            Statement.MakeVarDecl(
                                Helpers.MakeSeq(PatternConstruct.MakeIdentifierPattern("f_a", Helpers.MakePrimitiveType("float"))),
                                Helpers.MakeSeq(Expression.MakeConstant("float", 1.0e4f)),
                                Modifiers.Immutable
                            ),
                            Statement.MakeVarDecl(
                                Helpers.MakeSeq(
                                    PatternConstruct.MakePatternWithType(
                                        PatternConstruct.MakeIdentifierPattern(
                                            "f_a_",
                                            Helpers.MakePrimitiveType("float")
                                        ),
                                        Helpers.MakePrimitiveType("float")
                                    )
                                ),
                                Helpers.MakeSeq(Expression.MakeConstant("float", 1.0e4f)),
                                Modifiers.Immutable
                            ),
                            Statement.MakeVarDecl(
                                Helpers.MakeSeq(
                                    PatternConstruct.MakePatternWithType(
                                        PatternConstruct.MakeIdentifierPattern(
                                            "f",
                                            Helpers.MakeGenericType(
                                                "array",
                                                Helpers.MakePrimitiveType("int")
                                            )
                                        ),
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
                                Helpers.MakeSeq(PatternConstruct.MakeIdentifierPattern("f_", Helpers.MakeGenericType("array", Helpers.MakePrimitiveType("int")))),
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
                                    PatternConstruct.MakePatternWithType(
                                        PatternConstruct.MakeIdentifierPattern(
                                            "f2",
                                            Helpers.MakeGenericType(
                                                "vector",
                                                Helpers.MakePrimitiveType("int")
                                            )
                                        ),
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
                                    PatternConstruct.MakeIdentifierPattern(
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
                                    PatternConstruct.MakeIdentifierPattern(
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
                                    PatternConstruct.MakeTuplePattern(
                                        PatternConstruct.MakeIdentifierPattern(
                                            "f3_a",
                                            Helpers.MakePrimitiveType("int")
                                        ),
                                        PatternConstruct.MakeIdentifierPattern(
                                            "f3_b",
                                            Helpers.MakePrimitiveType("string")
                                        ),
                                        PatternConstruct.MakeIdentifierPattern(
                                            "f3_c",
                                            Helpers.MakePrimitiveType("bool")
                                        )
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
                                    PatternConstruct.MakePatternWithType(
                                        PatternConstruct.MakeIdentifierPattern(
                                            "g",
                                            Helpers.MakeGenericType(
                                                "dictionary",
                                                Helpers.MakePrimitiveType("string"),
                                                Helpers.MakePrimitiveType("int")
                                            )
                                        ),
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
                                    PatternConstruct.MakeIdentifierPattern(
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
                                Helpers.MakeSeq(PatternConstruct.MakeIdentifierPattern("h", Helpers.MakePrimitiveType("string"))),
                                Helpers.MakeSeq(Expression.MakeConstant("string", "私変わっちゃうの・・？")),
                                Modifiers.None
                            ),
                            Statement.MakeVarDecl(
                                Helpers.MakeSeq(PatternConstruct.MakeIdentifierPattern("h2", Helpers.MakePrimitiveType("string"))),
                                Helpers.MakeSeq(Expression.MakeConstant("string", "Yes, you can!")),
                                Modifiers.None
                            ),
                            Statement.MakeVarDecl(
                                Helpers.MakeSeq(PatternConstruct.MakeIdentifierPattern("h3", Helpers.MakePrimitiveType("char"))),
                                Helpers.MakeSeq(Expression.MakeConstant("char", 'a')),
                                Modifiers.None
                            ),
                            Statement.MakeVarDecl(
                                Helpers.MakeSeq(PatternConstruct.MakeIdentifierPattern("i", Helpers.MakePrimitiveType("string"))),
                                Helpers.MakeSeq(Expression.MakeConstant("string", "よかった。私変わらないんだね！・・え、変われないの？・・・なんだかフクザツ")),
                                Modifiers.Immutable
                            ),
                            Statement.MakeVarDecl(
                                Helpers.MakeSeq(PatternConstruct.MakeIdentifierPattern("i2", Helpers.MakePrimitiveType("string"))),
                                Helpers.MakeSeq(Expression.MakeConstant("string", "Oh, you just can't...")),
                                Modifiers.Immutable
                            ),
                            Statement.MakeVarDecl(
                                Helpers.MakeSeq(PatternConstruct.MakeIdentifierPattern("i3", Helpers.MakePrimitiveType("char"))),
                                Helpers.MakeSeq(Expression.MakeConstant("char", 'a')),
                                Modifiers.Immutable
                            ),
                            Statement.MakeVarDecl(
                                Helpers.MakeSeq(PatternConstruct.MakeIdentifierPattern("i4", Helpers.MakePrimitiveType("string"))),
                                Helpers.MakeSeq(Expression.MakeConstant("string", "\u0041\u005a\u0061\u007A\u3042\u30A2")), //AZazあア
                                Modifiers.Immutable
                            ),
                            Statement.MakeVarDecl(
                                Helpers.MakeSeq(PatternConstruct.MakeIdentifierPattern("i5", Helpers.MakePrimitiveType("char"))),
                                Helpers.MakeSeq(Expression.MakeConstant("char", '\u0041')), //A
                                Modifiers.Immutable
                            ),
                            Statement.MakeVarDecl(
                                Helpers.MakeSeq(PatternConstruct.MakeIdentifierPattern("i6", Helpers.MakePrimitiveType("string"))),
                                Helpers.MakeSeq(Expression.MakeConstant("string", "This is a normal string.\n Seems 2 lines? Yes, you're right!")),
                                Modifiers.Immutable
                            ),
                            Statement.MakeVarDecl(
                                Helpers.MakeSeq(PatternConstruct.MakeIdentifierPattern("i6_", Helpers.MakePrimitiveType("string"))),
                                Helpers.MakeSeq(Expression.MakeConstant("string", @"This is a raw string.\n Seems 2 lines? Nah, indeed.")),
                                Modifiers.Immutable
                            ),
                            Statement.MakeVarDecl(
                                Helpers.MakeSeq(PatternConstruct.MakeIdentifierPattern("j", Helpers.MakePrimitiveType("intseq"))),
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
                                Helpers.MakeSeq(PatternConstruct.MakeIdentifierPattern("j2", Helpers.MakePrimitiveType("intseq"))),
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
                                    PatternConstruct.MakePatternWithType(
                                        PatternConstruct.MakeIdentifierPattern(
                                            "j3",
                                            Helpers.MakePrimitiveType("intseq")
                                        ),
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
                                Helpers.MakeSeq(PatternConstruct.MakeIdentifierPattern("j_2", Helpers.MakePrimitiveType("intseq"))),
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
                                Helpers.MakeSeq(PatternConstruct.MakeIdentifierPattern("j_2_", Helpers.MakePrimitiveType("intseq"))),
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
                                    Helpers.MakeIdentifierPath(
                                        "println",
                                        AstType.MakeFunctionType(
                                            "println",
                                            Helpers.MakeVoidType(),
                                            default(TextLocation),
                                            default(TextLocation),
                                            Helpers.MakePrimitiveType("string")
                                        )
                                    ),
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
                                    ),
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
                        ),
                        Helpers.MakeVoidType(),
                        Modifiers.None
                    )
                }
            );

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

            var expected_ast = AstNode.MakeModuleDef(
                "main",
                new List<EntityDeclaration>{
                    EntityDeclaration.MakeFunc(
                        "main",
                        Enumerable.Empty<ParameterDeclaration>(),
                        Statement.MakeBlock(
                            Statement.MakeVarDecl(
                                Helpers.MakeSeq(PatternConstruct.MakeIdentifierPattern("ary", Helpers.MakeGenericType("array", Helpers.MakePrimitiveType("int")))),
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
                                    PatternConstruct.MakeIdentifierPattern(
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
                                Helpers.MakeSeq(PatternConstruct.MakeIdentifierPattern("m", Helpers.MakePrimitiveType("int"))),
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
                                Helpers.MakeSeq(PatternConstruct.MakeIdentifierPattern("m2", Helpers.MakePrimitiveType("int"))),
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
                                Helpers.MakeSeq(PatternConstruct.MakeIdentifierPattern("x", Helpers.MakePrimitiveType("int"))),
                                Helpers.MakeSeq(Expression.MakeConstant("int", 100)),
                                Modifiers.Immutable
                            ),
                            Statement.MakeVarDecl(
                                Helpers.MakeSeq(PatternConstruct.MakeIdentifierPattern("p", Helpers.MakePrimitiveType("int"))),
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
                                Helpers.MakeSeq(PatternConstruct.MakeIdentifierPattern("q", Helpers.MakePrimitiveType("int"))),
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
                                Helpers.MakeSeq(PatternConstruct.MakeIdentifierPattern("r", Helpers.MakePrimitiveType("int"))),
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
                                Helpers.MakeSeq(PatternConstruct.MakeIdentifierPattern("s", Helpers.MakePrimitiveType("int"))),
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
                                Helpers.MakeSeq(PatternConstruct.MakeIdentifierPattern("t", Helpers.MakePrimitiveType("int"))),
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
                                Helpers.MakeSeq(PatternConstruct.MakeIdentifierPattern("v", Helpers.MakePrimitiveType("int"))),
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
                                Helpers.MakeSeq(PatternConstruct.MakeIdentifierPattern("w", Helpers.MakePrimitiveType("int"))),
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
                                Helpers.MakeSeq(PatternConstruct.MakeIdentifierPattern("y", Helpers.MakePrimitiveType("int"))),
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
                                Helpers.MakeSeq(PatternConstruct.MakeIdentifierPattern("z", Helpers.MakePrimitiveType("int"))),
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
                                    Helpers.MakeIdentifierPath(
                                        "println",
                                        AstType.MakeFunctionType(
                                            "println",
                                            Helpers.MakeVoidType(),
                                            default(TextLocation),
                                            default(TextLocation),
                                            Helpers.MakePrimitiveType("string")
                                        )
                                    ),
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
                        ),
                        Helpers.MakeVoidType(),
                        Modifiers.None
                    )
                }
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
                    Statement.MakeBlock(new List<Statement>{
                        Statement.MakeVarDecl(
                            Helpers.MakeSeq(
                                PatternConstruct.MakeIdentifierPattern("x", Helpers.MakePrimitiveType("int")),
                                PatternConstruct.MakeIdentifierPattern("xp", Helpers.MakePrimitiveType("int")),
                                PatternConstruct.MakeIdentifierPattern("xm", Helpers.MakePrimitiveType("int")),
                                PatternConstruct.MakeIdentifierPattern("xt", Helpers.MakePrimitiveType("int")),
                                PatternConstruct.MakeIdentifierPattern("xd", Helpers.MakePrimitiveType("int")),
                                PatternConstruct.MakeIdentifierPattern("xmod", Helpers.MakePrimitiveType("int")),
                                PatternConstruct.MakeIdentifierPattern("xpower", Helpers.MakePrimitiveType("int"))
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
                                PatternConstruct.MakeIdentifierPattern("a", Helpers.MakePrimitiveType("int"))
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
                                PatternConstruct.MakeIdentifierPattern("b", Helpers.MakePrimitiveType("int"))
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
                                PatternConstruct.MakeIdentifierPattern("c", Helpers.MakePrimitiveType("int"))
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
                                PatternConstruct.MakeIdentifierPattern("d", Helpers.MakePrimitiveType("int"))
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
                                PatternConstruct.MakeIdentifierPattern("e", Helpers.MakePrimitiveType("int"))
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
                                PatternConstruct.MakeIdentifierPattern("f", Helpers.MakePrimitiveType("int"))
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
                                Helpers.MakeIdentifierPath(
                                    "println",
                                    AstType.MakeFunctionType(
                                        "println",
                                        Helpers.MakeVoidType(),
                                        default(TextLocation),
                                        default(TextLocation),
                                        Helpers.MakePrimitiveType("string")
                                    )
                                ),
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
                    }),
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
                                PatternConstruct.MakeIdentifierPattern(
                                    "x", 
                                    Helpers.MakeGenericType("vector", Helpers.MakePrimitiveType("int"))
                                )
                            ),
                            Helpers.MakeSeq(
                                Expression.MakeComp(
                                    Helpers.MakeIdentifierPath("x", Helpers.MakePrimitiveType("int")),
                                    Expression.MakeCompFor(
                                        PatternConstruct.MakeIdentifierPattern(
                                            "x",
                                            Helpers.MakePrimitiveType("int"),
                                            null
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
                                PatternConstruct.MakeIdentifierPattern(
                                    "y", 
                                    Helpers.MakeGenericType("vector", Helpers.MakePrimitiveType("int"))
                                )
                            ),
                            Helpers.MakeSeq(
                                Expression.MakeComp(
                                    Helpers.MakeIdentifierPath("x", Helpers.MakePrimitiveType("int")),
                                    Expression.MakeCompFor(
                                        PatternConstruct.MakeIdentifierPattern(
                                            "x",
                                            Helpers.MakePrimitiveType("int"),
                                            null
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
                                PatternConstruct.MakeIdentifierPattern(
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
                                        PatternConstruct.MakeIdentifierPattern(
                                            "x",
                                            Helpers.MakePrimitiveType("int"),
                                            null
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
                                                PatternConstruct.MakeIdentifierPattern(
                                                    "y",
                                                    Helpers.MakePrimitiveType("int"),
                                                    null
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
                                PatternConstruct.MakeIdentifierPattern(
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
                                        PatternConstruct.MakeIdentifierPattern(
                                            "c",
                                            Helpers.MakePrimitiveType("int"),
                                            null
                                        ),
                                        Expression.MakeIntSeq(
                                            Expression.MakeConstant("int", 1),
                                            Expression.MakeConstant("int", 10),
                                            Expression.MakeConstant("int", 1),
                                            true
                                        ),
                                        Expression.MakeCompFor(
                                            PatternConstruct.MakeIdentifierPattern(
                                                "b",
                                                Helpers.MakePrimitiveType("int"),
                                                null
                                            ),
                                            Expression.MakeIntSeq(
                                                Expression.MakeConstant("int", 1),
                                                Helpers.MakeIdentifierPath("c"),
                                                Expression.MakeConstant("int", 1),
                                                true
                                            ),
                                            Expression.MakeCompFor(
                                                PatternConstruct.MakeIdentifierPattern(
                                                    "a",
                                                    Helpers.MakePrimitiveType("int"),
                                                    null
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
                                PatternConstruct.MakeIdentifierPattern(
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
                                        PatternConstruct.MakeIdentifierPattern(
                                            "c",
                                            Helpers.MakePrimitiveType("int"),
                                            null
                                        ),
                                        Expression.MakeIntSeq(
                                            Expression.MakeConstant("int", 1),
                                            Expression.MakeConstant("int", 10),
                                            Expression.MakeConstant("int", 1),
                                            true
                                        ),
                                        Expression.MakeCompFor(
                                            PatternConstruct.MakeIdentifierPattern(
                                                "b",
                                                Helpers.MakePrimitiveType("int"),
                                                null
                                            ),
                                            Expression.MakeIntSeq(
                                                Expression.MakeConstant("int", 1),
                                                Helpers.MakeIdentifierPath("c"),
                                                Expression.MakeConstant("int", 1),
                                                true
                                            ),
                                            Expression.MakeCompFor(
                                                PatternConstruct.MakeIdentifierPattern(
                                                    "a",
                                                    Helpers.MakePrimitiveType("int"),
                                                    null
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
                                PatternConstruct.MakeIdentifierPattern(
                                    "a",
                                    Helpers.MakePrimitiveType("int")
                                ),
                                PatternConstruct.MakeIdentifierPattern(
                                    "b",
                                    Helpers.MakePrimitiveType("int")
                                ),
                                PatternConstruct.MakeIdentifierPattern(
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
                                PatternConstruct.MakeIdentifierPattern(
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
                                PatternConstruct.MakeIdentifierPattern(
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
                                    AstNode.MakeIdentifier(
                                        "add",
                                        AstType.MakeFunctionType(
                                            "add",
                                            Helpers.MakeVoidType(),
                                            TextLocation.Empty,
                                            TextLocation.Empty,
                                            AstType.MakeParameterType("T")
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
                                Helpers.MakeIdentifierPath(
                                    "println", 
                                    AstType.MakeFunctionType(
                                        "println",
                                        Helpers.MakeVoidType(),
                                        TextLocation.Empty,
                                        TextLocation.Empty,
                                        Helpers.MakePrimitiveType("string")
                                    )
                                ),
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
                                PatternConstruct.MakeIdentifierPattern("x", Helpers.MakePrimitiveType("int")),
                                PatternConstruct.MakeIdentifierPattern("y", Helpers.MakePrimitiveType("int")),
                                PatternConstruct.MakeIdentifierPattern("z", Helpers.MakePrimitiveType("int")),
                                PatternConstruct.MakeIdentifierPattern("w", Helpers.MakePrimitiveType("int"))
                            ),
                            Helpers.MakeSeq(
                                Expression.MakeConstant("int", 100),
                                Expression.MakeConstant("int", 50),
                                Expression.MakeConstant("int", 300),
                                Expression.MakeConstant("int", 400)
                            ),
                            Modifiers.Immutable
                        ),
                        Helpers.MakeVariableDeclaration(
                            Helpers.MakeSeq(
                                PatternConstruct.MakeIdentifierPattern("flag", Helpers.MakePrimitiveType("bool"))
                            ),
                            Modifiers.None
                        ),
                        Statement.MakeExprStmt(
                            Helpers.MakeCallExpression(
                                Helpers.MakeIdentifierPath(
                                    "print",
                                    AstType.MakeFunctionType(
                                        "print",
                                        Helpers.MakeVoidType(),
                                        TextLocation.Empty,
                                        TextLocation.Empty,
                                        Helpers.MakePrimitiveType("string")
                                    )
                                ),
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
                        ),
                        Statement.MakeVarDecl(
                            Helpers.MakeSeq(
                                PatternConstruct.MakeIdentifierPattern(
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
                                        Helpers.MakeIdentifierPath(
                                            "println",
                                            AstType.MakeFunctionType(
                                                "println",
                                                Helpers.MakeVoidType(),
                                                TextLocation.Empty,
                                                TextLocation.Empty,
                                                Helpers.MakePrimitiveType("string")
                                            )
                                        ),
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
                            ),
                            TextLocation.Empty,
                            AstNode.MakeVariableInitializer(
                                PatternConstruct.MakeIdentifierPattern("p", Helpers.MakePrimitiveType("int")),
                                Expression.MakeIntSeq(
                                    Expression.MakeConstant("int", 0),
                                    Helpers.MakeIdentifierPath("y", Helpers.MakePrimitiveType("int")),
                                    Expression.MakeConstant("int", 1),
                                    false
                                )
                            )
                        ),
                        Statement.MakeVarDecl(
                            Helpers.MakeSeq(
                                PatternConstruct.MakeIdentifierPattern(
                                    "fibs",
                                    Helpers.MakeGenericType(
                                        "vector",
                                        Helpers.MakePrimitiveType("int")
                                    )
                                ),
                                PatternConstruct.MakeIdentifierPattern(
                                    "a",
                                    Helpers.MakePrimitiveType("int")
                                ),
                                PatternConstruct.MakeIdentifierPattern(
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
                                            AstNode.MakeIdentifier(
                                                "add",
                                                AstType.MakeFunctionType(
                                                    "add",
                                                    Helpers.MakeVoidType(),
                                                    TextLocation.Empty,
                                                    TextLocation.Empty,
                                                    AstType.MakeParameterType("T")
                                                )
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
                                PatternConstruct.MakeIdentifierPattern(
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
                            Statement.MakeBlock(
                                Statement.MakeValueBindingForStmt(
                                    Modifiers.Immutable,
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
                                                    Expression.MakeConstant("int", 8)
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
                                                    AstNode.MakeIdentifier(
                                                        "add",
                                                        AstType.MakeFunctionType(
                                                            "add",
                                                            Helpers.MakeVoidType(),
                                                            TextLocation.Empty,
                                                            TextLocation.Empty,
                                                            AstType.MakeParameterType("T")
                                                        )
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
                                                    AstNode.MakeIdentifier(
                                                        "add",
                                                        AstType.MakeFunctionType(
                                                            "add",
                                                            Helpers.MakeVoidType(),
                                                            TextLocation.Empty,
                                                            TextLocation.Empty,
                                                            AstType.MakeParameterType("T")
                                                        )
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
                                                Helpers.MakeIdentifierPath(
                                                    "println",
                                                    AstType.MakeFunctionType(
                                                        "println",
                                                        Helpers.MakeVoidType(),
                                                        TextLocation.Empty,
                                                        TextLocation.Empty,
                                                        Helpers.MakePrimitiveType("string")
                                                    )
                                                ),
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
                                    ),
                                    TextLocation.Empty,
                                    AstNode.MakeVariableInitializer(
                                        PatternConstruct.MakeIdentifierPattern(
                                            "j",
                                            Helpers.MakePrimitiveType("int")
                                        ),
                                        Expression.MakeIntSeq(
                                            Expression.MakeConstant("int", 0),
                                            Expression.MakeConstant("int", 10),
                                            Expression.MakeConstant("int", 1),
                                            false
                                        )
                                    )
                                )
                            ),
                            TextLocation.Empty,
                            AstNode.MakeVariableInitializer(
                                PatternConstruct.MakeIdentifierPattern(
                                    "i",
                                    Helpers.MakePrimitiveType("int")
                                ),
                                Expression.MakeSequenceInitializer(
                                    Helpers.MakeGenericType(
                                        "vector",
                                        Helpers.MakePrimitiveType("int")
                                    ),
                                    TextLocation.Empty,
                                    TextLocation.Empty,
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
                                )
                            )
                        ),
                        Statement.MakeExprStmt(
                            Helpers.MakeCallExpression(
                                Helpers.MakeIdentifierPath(
                                    "println",
                                    AstType.MakeFunctionType(
                                        "println",
                                        Helpers.MakeVoidType(),
                                        TextLocation.Empty,
                                        TextLocation.Empty,
                                        Helpers.MakePrimitiveType("string")
                                    )
                                ),
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
                    ),
                    Helpers.MakeVoidType(),
                    Modifiers.None
                )
            });

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
                                AstNode.MakeIdentifier("x", Helpers.MakePrimitiveType("int"))
                            ),
                            Helpers.MakeSeq<Expression>(
                                Expression.Null
                            ),
                            Modifiers.Public | Modifiers.Immutable
                        ),
                        EntityDeclaration.MakeField(
                            Helpers.MakeSeq(
                                AstNode.MakeIdentifier("y", Helpers.MakePrimitiveType("int")),
                                AstNode.MakeIdentifier("z", Helpers.MakePrimitiveType("int"))
                            ),
                            Helpers.MakeSeq(
                                Expression.Null,
                                Expression.MakeConstant("int", 3)
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
	                                            AstNode.MakeIdentifier(
	                                                "getX",
	                                                AstType.MakeFunctionType(
	                                                    "getX",
	                                                    Helpers.MakePrimitiveType("int"),
	                                                    Enumerable.Empty<AstType>()
	                                                )
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
                                PatternConstruct.MakeIdentifierPattern("a", Helpers.MakeGenericType("TestClass"))
                            ),
                            Helpers.MakeSeq(
                                Expression.MakeObjectCreation(
                                    Helpers.MakeGenericType("TestClass"),
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
                                PatternConstruct.MakeIdentifierPattern("c", Helpers.MakePrimitiveType("int"))
                            ),
                            Helpers.MakeSeq(
                                Expression.MakeCallExpr(
                                    Expression.MakeMemRef(
                                        Helpers.MakeIdentifierPath(
                                            "a",
                                            Helpers.MakeGenericType("TestClass")
                                        ),
                                        AstNode.MakeIdentifier(
                                            "getX",
                                            AstType.MakeFunctionType(
                                                "getX",
                                                Helpers.MakePrimitiveType("int"),
                                                Enumerable.Empty<ParameterType>()
                                            )
                                        )
                                    ),
                                    Enumerable.Empty<Expression>()
                                )
                            ),
                            Modifiers.Immutable
                        ),
                        Statement.MakeVarDecl(
                            Helpers.MakeSeq(
                                PatternConstruct.MakeIdentifierPattern("d", Helpers.MakePrimitiveType("int"))
                            ),
                            Helpers.MakeSeq(
                                Expression.MakeCallExpr(
                                    Expression.MakeMemRef(
                                        Helpers.MakeIdentifierPath(
                                            "a",
                                            Helpers.MakeGenericType("TestClass")
                                        ),
                                        AstNode.MakeIdentifier(
                                            "getY",
                                            AstType.MakeFunctionType(
                                                "getY",
                                                Helpers.MakePrimitiveType("int"),
                                                Enumerable.Empty<ParameterType>()
                                            )
                                        )
                                    ),
                                    Enumerable.Empty<Expression>()
                                )
                            ),
                            Modifiers.Immutable
                        ),
                        Statement.MakeVarDecl(
                            Helpers.MakeSeq(
                                PatternConstruct.MakeIdentifierPattern("e", Helpers.MakePrimitiveType("int"))
                            ),
                            Helpers.MakeSeq(
                                Helpers.MakeCallExpression(
                                    Expression.MakeMemRef(
                                        Helpers.MakeIdentifierPath(
                                            "a",
                                            Helpers.MakeGenericType("TestClass")
                                        ),
                                        AstNode.MakeIdentifier(
                                            "getXPlus",
                                            AstType.MakeFunctionType(
                                                "getXPlus",
                                                Helpers.MakePrimitiveType("int"),
                                                TextLocation.Empty,
                                                TextLocation.Empty,
                                                Helpers.MakePrimitiveType("int")
                                            )
                                        )
                                    ),
                                    Expression.MakeConstant("int", 100)
                                )
                            ),
                            Modifiers.Immutable
                        ),
                        Statement.MakeVarDecl(
                            Helpers.MakeSeq(
                                PatternConstruct.MakeIdentifierPattern("f", Helpers.MakePrimitiveType("int"))
                            ),
                            Helpers.MakeSeq(
                                Helpers.MakeCallExpression(
                                    Expression.MakeMemRef(
                                        Helpers.MakeIdentifierPath(
                                            "a",
                                            Helpers.MakeGenericType("TestClass")
                                        ),
                                        AstNode.MakeIdentifier(
                                            "getZ",
                                            AstType.MakeFunctionType(
                                                "getZ",
                                                Helpers.MakePrimitiveType("int"),
                                                Enumerable.Empty<AstType>()
                                            )
                                        )
                                    )
                                )
                            ),
                            Modifiers.Immutable
                        ),
                        Statement.MakeVarDecl(
                            Helpers.MakeSeq(
                                PatternConstruct.MakeIdentifierPattern("g", Helpers.MakePrimitiveType("int"))
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
                                Helpers.MakeIdentifierPath(
                                    "printFormat",
                                    AstType.MakeFunctionType(
                                        "printFormat",
                                        Helpers.MakeVoidType(),
                                        TextLocation.Empty,
                                        TextLocation.Empty,
                                        Helpers.MakePrimitiveType("string")
                                    )
                                ),
                                Expression.MakeConstant("string", "(a.x, a.y, a.z, x) = ({0}, {1}, {2}, {3})\n"),
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
                                PatternConstruct.MakeIdentifierPattern(
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
                                    Expression.MakeCallExpr(
                                        Helpers.MakeIdentifierPath(
                                            "test4",
                                            AstType.MakeFunctionType(
                                                "test4",
                                                Helpers.MakePrimitiveType("int"),
                                                TextLocation.Empty,
                                                TextLocation.Empty,
                                                Helpers.MakePrimitiveType("int")
                                            )
                                        ),
                                        TextLocation.Empty,
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
                    "main",
                    Enumerable.Empty<ParameterDeclaration>(),
                    Statement.MakeBlock(
                        Statement.MakeVarDecl(
                            Helpers.MakeSeq(
                                PatternConstruct.MakeIdentifierPattern(
                                    "a",
                                    Helpers.MakePrimitiveType("int")
                                )
                            ),
                            Helpers.MakeSeq(
                                Helpers.MakeCallExpression(
                                    Helpers.MakeIdentifierPath(
                                        "test",
                                        AstType.MakeFunctionType(
                                            "test",
                                            Helpers.MakePrimitiveType("int"),
                                            Enumerable.Empty<AstType>()
                                        )
                                    )
                                )
                            ),
                            Modifiers.Immutable
                        ),
                        Statement.MakeVarDecl(
                            Helpers.MakeSeq(
                                PatternConstruct.MakeIdentifierPattern(
                                    "b",
                                    Helpers.MakePrimitiveType("int")
                                )
                            ),
                            Helpers.MakeSeq(
                                Helpers.MakeCallExpression(
                                    Helpers.MakeIdentifierPath(
                                        "test2",
                                        AstType.MakeFunctionType(
                                            "test2",
                                            Helpers.MakePrimitiveType("int"),
                                            TextLocation.Empty,
                                            TextLocation.Empty,
                                            Helpers.MakePrimitiveType("int")
                                        )
                                    ),
                                    Expression.MakeConstant("int", 20)
                                )
                            ),
                            Modifiers.Immutable
                        ),
                        Statement.MakeVarDecl(
                            Helpers.MakeSeq(
                                PatternConstruct.MakeIdentifierPattern(
                                    "c",
                                    Helpers.MakePrimitiveType("int")
                                )
                            ),
                            Helpers.MakeSeq(
                                Helpers.MakeCallExpression(
                                    Helpers.MakeIdentifierPath(
                                        "test3",
                                        AstType.MakeFunctionType(
                                            "test3",
                                            Helpers.MakePrimitiveType("int"),
                                            TextLocation.Empty,
                                            TextLocation.Empty,
                                            Helpers.MakePrimitiveType("int")
                                        )
                                    ),
                                    Expression.MakeConstant("int", 20)
                                )
                            ),
                            Modifiers.Immutable
                        ),
                        Statement.MakeVarDecl(
                            Helpers.MakeSeq(
                                PatternConstruct.MakeIdentifierPattern(
                                    "d",
                                    Helpers.MakePrimitiveType("int")
                                )
                            ),
                            Helpers.MakeSeq(
                                Helpers.MakeCallExpression(
                                    Helpers.MakeIdentifierPath(
                                        "test4",
                                        AstType.MakeFunctionType(
                                            "test4",
                                            Helpers.MakePrimitiveType("int"),
                                            TextLocation.Empty,
                                            TextLocation.Empty,
                                            Helpers.MakePrimitiveType("int")
                                        )
                                    ),
                                    Expression.MakeConstant("int", 80)
                                )
                            ),
                            Modifiers.Immutable
                        ),
                        Statement.MakeExprStmt(
                            Helpers.MakeCallExpression(
                                Helpers.MakeIdentifierPath(
                                    "println",
                                    AstType.MakeFunctionType(
                                        "println",
                                        Helpers.MakeVoidType(),
                                        TextLocation.Empty,
                                        TextLocation.Empty,
                                        Helpers.MakePrimitiveType("string")
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
                                ),
                                Helpers.MakeIdentifierPath(
                                    "d",
                                    Helpers.MakePrimitiveType("int")
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
                    "TestClass",
                    Enumerable.Empty<AstType>(),
                    Helpers.MakeSeq<EntityDeclaration>(
                        EntityDeclaration.MakeField(
                            Helpers.MakeSeq(
                                AstNode.MakeIdentifier("x", Helpers.MakePrimitiveType("int")),
                                AstNode.MakeIdentifier("y", Helpers.MakePrimitiveType("int")),
                                AstNode.MakeIdentifier("z", Helpers.MakePrimitiveType("int"))
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
                                PatternConstruct.MakeIdentifierPattern(
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
                                        Helpers.MakeIdentifierPath(
                                            "println",
                                            AstType.MakeFunctionType(
                                                "println",
                                                Helpers.MakeVoidType(),
                                                TextLocation.Empty,
                                                TextLocation.Empty,
                                                Helpers.MakePrimitiveType("string")
                                            )
                                        ),
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
                                        Helpers.MakeIdentifierPath(
                                            "println",
                                            AstType.MakeFunctionType(
                                                "println",
                                                Helpers.MakeVoidType(),
                                                TextLocation.Empty,
                                                TextLocation.Empty,
                                                Helpers.MakePrimitiveType("string")
                                            )
                                        ),
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
                                        Helpers.MakeIdentifierPath(
                                            "println",
                                            AstType.MakeFunctionType(
                                                "println",
                                                Helpers.MakeVoidType(),
                                                TextLocation.Empty,
                                                TextLocation.Empty,
                                                Helpers.MakePrimitiveType("string")
                                            )
                                        ),
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
                                        Helpers.MakeIdentifierPath(
                                            "println",
                                            AstType.MakeFunctionType(
                                                "println",
                                                Helpers.MakeVoidType(),
                                                TextLocation.Empty,
                                                TextLocation.Empty,
                                                Helpers.MakePrimitiveType("string")
                                            )
                                        ),
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
                                PatternConstruct.MakeIdentifierPattern("tmp2", Helpers.MakePrimitiveType("int"))
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
                                            Helpers.MakeIdentifierPath(
                                                "println",
                                                AstType.MakeFunctionType(
                                                    "println",
                                                    Helpers.MakeVoidType(),
                                                    TextLocation.Empty,
                                                    TextLocation.Empty,
                                                    Helpers.MakePrimitiveType("string")
                                                )
                                            ),
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
                                            Helpers.MakeIdentifierPath(
                                                "println",
                                                AstType.MakeFunctionType(
                                                    "println",
                                                    Helpers.MakeVoidType(),
                                                    TextLocation.Empty,
                                                    TextLocation.Empty,
                                                    Helpers.MakePrimitiveType("string")
                                                )
                                            ),
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
                                            Helpers.MakeIdentifierPath(
                                                "printFormat",
                                                AstType.MakeFunctionType(
                                                    "printFormat",
                                                    Helpers.MakeVoidType(),
                                                    TextLocation.Empty,
                                                    TextLocation.Empty,
                                                    Helpers.MakePrimitiveType("string")
                                                )
                                            ),
                                            Expression.MakeConstant("string", "{0} is in the range of 3 to 10\n"),
                                            Helpers.MakeIdentifierPath("i", Helpers.MakePrimitiveType("int"))
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
                                            Helpers.MakeIdentifierPath(
                                                "println",
                                                AstType.MakeFunctionType(
                                                    "println",
                                                    Helpers.MakeVoidType(),
                                                    TextLocation.Empty,
                                                    TextLocation.Empty,
                                                    Helpers.MakePrimitiveType("string")
                                                )
                                            ),
                                            Expression.MakeConstant("string", "otherwise")
                                        )
                                    )
                                ),
                                PatternConstruct.MakeWildcardPattern()
                            )
                        ),
                        Statement.MakeVarDecl(
                            Helpers.MakeSeq(
                                PatternConstruct.MakeIdentifierPattern("tmp3", Helpers.MakeGenericType("TestClass"))
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
                        Statement.MakeMatchStmt(
                            Helpers.MakeIdentifierPath("tmp3", Helpers.MakeGenericType("TestClass")),
                            Statement.MakeMatchClause(
                                null,
                                Statement.MakeExprStmt(
                                    Expression.MakeSequenceExpression(
                                        Helpers.MakeCallExpression(
                                            Helpers.MakeIdentifierPath(
                                                "printFormat",
                                                AstType.MakeFunctionType(
                                                    "printFormat",
                                                    Helpers.MakeVoidType(),
                                                    TextLocation.Empty,
                                                    TextLocation.Empty,
                                                    Helpers.MakePrimitiveType("string")
                                                )
                                            ),
                                            Expression.MakeConstant("string", "x is {0}\n"),
                                            Helpers.MakeIdentifierPath(
                                                "x",
                                                Helpers.MakePrimitiveType("int")
                                            )
                                        )
                                    )
                                ),
                                PatternConstruct.MakeDestructuringPattern(
                                    Helpers.MakeGenericType("TestClass"),
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
                                            Helpers.MakeIdentifierPath(
                                                "printFormat",
                                                AstType.MakeFunctionType(
                                                    "printFormat",
                                                    Helpers.MakeVoidType(),
                                                    TextLocation.Empty,
                                                    TextLocation.Empty,
                                                    Helpers.MakePrimitiveType("string")
                                                )
                                            ),
                                            Expression.MakeConstant("string", "x is {0} and y is {1}\n"),
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
                                PatternConstruct.MakeDestructuringPattern(
                                    Helpers.MakeGenericType("TestClass"),
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
                                            Helpers.MakeIdentifierPath(
                                                "printFormat",
                                                AstType.MakeFunctionType(
                                                    "printFormat",
                                                    Helpers.MakeVoidType(),
                                                    TextLocation.Empty,
                                                    TextLocation.Empty,
                                                    Helpers.MakePrimitiveType("string")
                                                )
                                            ),
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
                                            Helpers.MakeIdentifierPath(
                                                "print",
                                                AstType.MakeFunctionType(
                                                    "print",
                                                    Helpers.MakeVoidType(),
                                                    TextLocation.Empty,
                                                    TextLocation.Empty,
                                                    Helpers.MakePrimitiveType("string")
                                                )
                                            ),
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
                                            Helpers.MakeIdentifierPath(
                                                "printFormat",
                                                AstType.MakeFunctionType(
                                                    "printFormat",
                                                    Helpers.MakeVoidType(),
                                                    TextLocation.Empty,
                                                    TextLocation.Empty,
                                                    Helpers.MakePrimitiveType("string")
                                                )
                                            ),
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
                                            Helpers.MakeIdentifierPath(
                                                "print",
                                                AstType.MakeFunctionType(
                                                    "print",
                                                    Helpers.MakeVoidType(),
                                                    TextLocation.Empty,
                                                    TextLocation.Empty,
                                                    Helpers.MakePrimitiveType("string")
                                                )
                                            ),
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
                                PatternConstruct.MakeIdentifierPattern(
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
                                            Helpers.MakeIdentifierPath(
                                                "printFormat",
                                                AstType.MakeFunctionType(
                                                    "printFormat",
                                                    Helpers.MakeVoidType(),
                                                    TextLocation.Empty,
                                                    TextLocation.Empty,
                                                    Helpers.MakePrimitiveType("string")
                                                )
                                            ),
                                            Expression.MakeConstant("string", "x is {0}\n"),
                                            Helpers.MakeIdentifierPath(
                                                "x",
                                                Helpers.MakePrimitiveType("int")
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
                                            Helpers.MakeIdentifierPath(
                                                "printFormat",
                                                AstType.MakeFunctionType(
                                                    "printFormat",
                                                    Helpers.MakeVoidType(),
                                                    TextLocation.Empty,
                                                    TextLocation.Empty,
                                                    Helpers.MakePrimitiveType("string")
                                                )
                                            ),
                                            Expression.MakeConstant("string", "x is {0} and y is {1}\n"),
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
        public void TestModule()
        {
            var parser = new Parser(new Scanner("../../sources/for_unit_tests/test_module.exs"));
            parser.DoPostParseProcessing = true;
            parser.Parse();

            var ast = parser.TopmostAst;

            var expected_ast = AstNode.MakeModuleDef("TestModule", new List<EntityDeclaration>{
                EntityDeclaration.MakeClassDecl(
                    "TestClass",
                    Enumerable.Empty<AstType>(),
                    Helpers.MakeSeq<EntityDeclaration>(
                        EntityDeclaration.MakeField(
                            Helpers.MakeSeq(
                                AstNode.MakeIdentifier(
                                    "x",
                                    Helpers.MakePrimitiveType("int")
                                ),
                                AstNode.MakeIdentifier(
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
                        AstNode.MakeIdentifier(
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
                                Helpers.MakeGenericType("TestClass"),
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
                    Helpers.MakeGenericType("TestClass"),
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
                                    AstNode.MakeIdentifier(
                                        "sin",
                                        AstType.MakeFunctionType(
                                            "sin",
                                            Helpers.MakePrimitiveType("double"), 
                                            TextLocation.Empty,
                                            TextLocation.Empty,
                                            Helpers.MakePrimitiveType("double")
                                        )
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
            }, Helpers.MakeSeq<ImportDeclaration>(
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
                                PatternConstruct.MakeIdentifierPattern(
                                    "a",
                                    AstType.MakeMemberType(
                                        Helpers.MakeGenericType("TestModule"),
                                        Helpers.MakeGenericType("TestClass")
                                    )
                                )
                            ),
                            Helpers.MakeSeq(
                                Expression.MakeObjectCreation(
                                    AstType.MakeMemberType(
                                        Helpers.MakeGenericType("TestModule"),
                                        Helpers.MakeGenericType("TestClass")
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
                                PatternConstruct.MakeIdentifierPattern(
                                    "b",
                                    Helpers.MakeGenericType("TestClass")
                                )
                            ),
                            Helpers.MakeSeq(
                                Helpers.MakeCallExpression(
                                    Expression.MakePath(
                                        AstNode.MakeIdentifier(
                                            "TestModule",
                                            Helpers.MakeGenericType("./test_module.exs")
                                        ),
                                        AstNode.MakeIdentifier(
                                            "createTest",
                                            AstType.MakeFunctionType(
                                                "createTest",
                                                Helpers.MakeGenericType("TestClass"),
                                                Helpers.MakeSeq(
                                                    Helpers.MakePrimitiveType("int"),
                                                    Helpers.MakePrimitiveType("int")
                                                )
                                            )
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
                                PatternConstruct.MakeIdentifierPattern(
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
                                    AstNode.MakeIdentifier(
                                        "TestModule",
                                        Helpers.MakeGenericType("./test_module.exs")
                                    ),
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
                                PatternConstruct.MakeIdentifierPattern(
                                    "d",
                                    Helpers.MakePrimitiveType("double")
                                )
                            ),
                            Helpers.MakeSeq(
                                Helpers.MakeCallExpression(
                                    Expression.MakePath(
                                        AstNode.MakeIdentifier(
                                            "TestModule",
                                            Helpers.MakeGenericType("./test_module.exs")
                                        ),
                                        AstNode.MakeIdentifier(
                                            "mySin",
                                            AstType.MakeFunctionType(
                                                "mySin",
                                                Helpers.MakePrimitiveType("double"),
                                                TextLocation.Empty,
                                                TextLocation.Empty,
                                                Helpers.MakePrimitiveType("double")
                                            )
                                        )
                                    ),
                                    Expression.MakeConstant("double", 0.0)
                                )
                            ),
                            Modifiers.Immutable
                        ),
                        Statement.MakeExprStmt(
                            Helpers.MakeCallExpression(
                                Helpers.MakeIdentifierPath(
                                    "println",
                                    AstType.MakeFunctionType(
                                        "println",
                                        Helpers.MakeVoidType(),
                                        TextLocation.Empty,
                                        TextLocation.Empty,
                                        Helpers.MakePrimitiveType("string")
                                    )
                                ),
                                Helpers.MakeIdentifierPath(
                                    "a",
                                    AstType.MakeMemberType(
                                        Helpers.MakeGenericType("TestModule"),
                                        Helpers.MakeGenericType("TestClass")
                                    )
                                ),
                                Helpers.MakeIdentifierPath(
                                    "b",
                                    Helpers.MakeGenericType("TestClass")
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
                                )
                            )
                        )
                    ),
                    Helpers.MakeVoidType(),
                    Modifiers.None
                )
            }, new List<ImportDeclaration>{
                AstNode.MakeImportDecl(AstNode.MakeIdentifier("test_module"), "TestModule")
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
                            AstType.MakeFunctionType(
                                "closure",
                                Helpers.MakePrimitiveType("int"),
                                TextLocation.Empty,
                                TextLocation.Empty,
                                Helpers.MakePrimitiveType("int")
                            )
                        )
                    ),
                    Statement.MakeBlock(
                        Helpers.MakeSingleItemReturnStatement(
                            Helpers.MakeCallExpression(
                                Helpers.MakeIdentifierPath(
                                    "addOne",
                                    AstType.MakeFunctionType(
                                        "closure",
                                        Helpers.MakePrimitiveType("int"),
                                        TextLocation.Empty,
                                        TextLocation.Empty,
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
                                PatternConstruct.MakeIdentifierPattern(
                                    "c",
                                    AstType.MakeFunctionType(
                                        "closure",
                                        Helpers.MakePrimitiveType("int"),
                                        TextLocation.Empty,
                                        TextLocation.Empty,
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
                                    TextLocation.Empty,
                                    new List<Identifier>{},
                                    EntityDeclaration.MakeParameter(
                                        "x",
                                        Helpers.MakePrimitiveType("int")
                                    )
                                )
                            ),
                            Modifiers.Immutable
                        ),
                        Statement.MakeVarDecl(
                            Helpers.MakeSeq(
                                PatternConstruct.MakeIdentifierPattern(
                                    "c2",
                                    AstType.MakeFunctionType(
                                        "closure",
                                        Helpers.MakePrimitiveType("int"),
                                        TextLocation.Empty,
                                        TextLocation.Empty,
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
                                                PatternConstruct.MakeIdentifierPattern(
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
                                    TextLocation.Empty,
                                    new List<Identifier>{},
                                    EntityDeclaration.MakeParameter(
                                        "x",
                                        Helpers.MakePrimitiveType("int")
                                    )
                                )
                            ),
                            Modifiers.Immutable
                        ),
                        Statement.MakeVarDecl(
                            Helpers.MakeSeq(
                                PatternConstruct.MakeIdentifierPattern(
                                    "a",
                                    Helpers.MakePrimitiveType("int")
                                )
                            ),
                            Helpers.MakeSeq(
                                Helpers.MakeCallExpression(
                                    Helpers.MakeIdentifierPath(
                                        "c",
                                        AstType.MakeFunctionType(
                                            "closure",
                                            Helpers.MakePrimitiveType("int"),
                                            TextLocation.Empty,
                                            TextLocation.Empty,
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
                                PatternConstruct.MakeIdentifierPattern(
                                    "b",
                                    Helpers.MakePrimitiveType("int")
                                )
                            ),
                            Helpers.MakeSeq(
                                Helpers.MakeCallExpression(
                                    Helpers.MakeIdentifierPath(
                                        "c2",
                                        AstType.MakeFunctionType(
                                            "closure",
                                            Helpers.MakePrimitiveType("int"),
                                            TextLocation.Empty,
                                            TextLocation.Empty,
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
                                PatternConstruct.MakeIdentifierPattern(
                                    "d",
                                    Helpers.MakePrimitiveType("int")
                                )
                            ),
                            Helpers.MakeSeq(
                                Helpers.MakeCallExpression(
                                    Helpers.MakeIdentifierPath(
                                        "addOneToOne",
                                        AstType.MakeFunctionType(
                                            "addOneToOne",
                                            Helpers.MakePrimitiveType("int"),
                                            TextLocation.Empty,
                                            TextLocation.Empty,
                                            AstType.MakeFunctionType(
                                                "closure",
                                                Helpers.MakePrimitiveType("int"),
                                                TextLocation.Empty,
                                                TextLocation.Empty,
                                                Helpers.MakePrimitiveType("int")
                                            )
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
                                        TextLocation.Empty,
                                        new List<Identifier>{},
                                        EntityDeclaration.MakeParameter(
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
                                PatternConstruct.MakeIdentifierPattern(
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
                                PatternConstruct.MakeIdentifierPattern(
                                    "c3",
                                    AstType.MakeFunctionType(
                                        "closure",
                                        Helpers.MakePrimitiveType("int"),
                                        TextLocation.Empty,
                                        TextLocation.Empty,
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
                                    TextLocation.Empty,
                                    new List<Identifier>{
                                        AstNode.MakeIdentifier(
                                            "e",
                                            Helpers.MakePrimitiveType("int")
                                        )
                                    },
                                    EntityDeclaration.MakeParameter(
                                        "x",
                                        Helpers.MakePrimitiveType("int")
                                    )
                                )
                            ),
                            Modifiers.Immutable
                        ),
                        Statement.MakeVarDecl(
                            Helpers.MakeSeq(
                                PatternConstruct.MakeIdentifierPattern(
                                    "f",
                                    Helpers.MakePrimitiveType("int")
                                )
                            ),
                            Helpers.MakeSeq(
                                Helpers.MakeCallExpression(
                                    Helpers.MakeIdentifierPath(
                                        "c3",
                                        AstType.MakeFunctionType(
                                            "closure",
                                            Helpers.MakePrimitiveType("int"),
                                            TextLocation.Empty,
                                            TextLocation.Empty,
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
                                Helpers.MakeIdentifierPath(
                                    "println",
                                    AstType.MakeFunctionType(
                                        "println",
                                        Helpers.MakeVoidType(),
                                        TextLocation.Empty,
                                        TextLocation.Empty,
                                        Helpers.MakePrimitiveType("string")
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
                                    "d",
                                    Helpers.MakePrimitiveType("int")
                                ),
                                Helpers.MakeIdentifierPath(
                                    "f",
                                    Helpers.MakePrimitiveType("int")
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
	                            PatternConstruct.MakeIdentifierPattern(
	                                "c",
	                                AstType.MakeFunctionType(
	                                    "closure",
	                                    Helpers.MakePrimitiveType("int"),
	                                    TextLocation.Empty,
	                                    TextLocation.Empty,
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
	                                TextLocation.Empty,
                                    new List<Identifier>(),
	                                EntityDeclaration.MakeParameter(
	                                    "f",
	                                    Helpers.MakePrimitiveType("bool")
	                                )
                                )
                            ),
                            Modifiers.Immutable
                        ),
                        Statement.MakeVarDecl(
                            Helpers.MakeSeq(
                                PatternConstruct.MakeIdentifierPattern(
                                    "a",
                                    Helpers.MakePrimitiveType("int")
                                )
                            ),
                            Helpers.MakeSeq(
                                Helpers.MakeCallExpression(
                                    Helpers.MakeIdentifierPath(
                                        "c",
                                        AstType.MakeFunctionType(
                                            "closure",
                                            Helpers.MakePrimitiveType("int"),
                                            TextLocation.Empty,
                                            TextLocation.Empty,
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
                                PatternConstruct.MakeIdentifierPattern(
                                    "c2",
                                    AstType.MakeFunctionType(
                                        "closure",
                                        Helpers.MakePrimitiveType("int"),
                                        TextLocation.Empty,
                                        TextLocation.Empty,
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
                                                PatternConstruct.MakeIdentifierPattern(
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
                                            ),
                                            TextLocation.Empty,
                                            AstNode.MakeVariableInitializer(
                                                PatternConstruct.MakeIdentifierPattern(
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
                                    TextLocation.Empty,
                                    new List<Identifier>(),
                                    EntityDeclaration.MakeParameter(
                                        "i",
                                        Helpers.MakePrimitiveType("int")
                                    )
                                )
                            ),
                            Modifiers.Immutable
                        ),
                        Statement.MakeVarDecl(
                            Helpers.MakeSeq(
                                PatternConstruct.MakeIdentifierPattern(
                                    "b",
                                    Helpers.MakePrimitiveType("int")
                                )
                            ),
                            Helpers.MakeSeq(
                                Helpers.MakeCallExpression(
                                    Helpers.MakeIdentifierPath(
                                        "c2",
                                        AstType.MakeFunctionType(
                                            "closure",
                                            Helpers.MakePrimitiveType("int"),
                                            TextLocation.Empty,
                                            TextLocation.Empty,
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
                                PatternConstruct.MakeIdentifierPattern(
                                    "c3",
                                    AstType.MakeFunctionType(
                                        "closure",
                                        Helpers.MakeVoidType(),
                                        TextLocation.Empty,
                                        TextLocation.Empty,
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
                                                PatternConstruct.MakeIdentifierPattern(
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
                                                        Helpers.MakeIdentifierPath(
                                                            "println",
                                                            AstType.MakeFunctionType(
                                                                "println",
                                                                Helpers.MakeVoidType(),
                                                                TextLocation.Empty,
                                                                TextLocation.Empty,
                                                                Helpers.MakePrimitiveType("string")
                                                            )
                                                        ),
                                                        Helpers.MakeIdentifierPath(
                                                            "j",
                                                            Helpers.MakePrimitiveType("int")
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
                                                Helpers.MakeIdentifierPath(
                                                    "println",
                                                    AstType.MakeFunctionType(
                                                        "println",
                                                        Helpers.MakeVoidType(),
                                                        TextLocation.Empty,
                                                        TextLocation.Empty,
                                                        Helpers.MakePrimitiveType("string")
                                                    )
                                                ),
                                                Expression.MakeConstant("string", "BOOM!")
                                            )
                                        )
                                    ),
                                    TextLocation.Empty,
                                    new List<Identifier>(),
                                    EntityDeclaration.MakeParameter(
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
                                    AstType.MakeFunctionType(
                                        "closure",
                                        Helpers.MakeVoidType(),
                                        TextLocation.Empty,
                                        TextLocation.Empty,
                                        Helpers.MakePrimitiveType("int")
                                    )
                                ),
                                Expression.MakeConstant("int", 3)
                            )
                        ),
                        Statement.MakeExprStmt(
                            Helpers.MakeCallExpression(
                                Helpers.MakeIdentifierPath(
                                    "println",
                                    AstType.MakeFunctionType(
                                        "println",
                                        Helpers.MakeVoidType(),
                                        TextLocation.Empty,
                                        TextLocation.Empty,
                                        Helpers.MakePrimitiveType("string")
                                    )
                                ),
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
                EntityDeclaration.MakeClassDecl(
                    "ExsException",
                    Helpers.MakeSeq<AstType>(
                        Helpers.MakeGenericType("Exception")
                    ),
                    Helpers.MakeSeq<EntityDeclaration>(
                        EntityDeclaration.MakeField(
                            Helpers.MakeSeq(
                                AstNode.MakeIdentifier("ExsMessage", AstType.MakePrimitiveType("string"))
                            ),
                            Helpers.MakeSeq<Expression>(
                                Expression.Null
                            ),
                            Modifiers.Public | Modifiers.Immutable
                        )
                    ),
                    Modifiers.None
                ),
                EntityDeclaration.MakeFunc(
                    "throwException",
                    Enumerable.Empty<ParameterDeclaration>(),
                    Statement.MakeBlock(
                        Statement.MakeThrowStmt(
                            Expression.MakeObjectCreation(
                                Helpers.MakeGenericType(
                                    "ExsException"
                                ),
                                Helpers.MakeSeq(
                                    AstNode.MakeIdentifier("ExsMessage")
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
                        Statement.MakeTryStmt(
                            Statement.MakeBlock(
                                Statement.MakeExprStmt(
                                    Helpers.MakeCallExpression(
                                        Helpers.MakeIdentifierPath(
                                            "println",
                                            AstType.MakeFunctionType(
                                                "println",
                                                Helpers.MakeVoidType(),
                                                TextLocation.Empty,
                                                TextLocation.Empty,
                                                Helpers.MakePrimitiveType("string")
                                            )
                                        ),
                                        Expression.MakeConstant("string", "First try block")
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
                            TextLocation.Empty,
                            Statement.MakeCatchClause(
                                AstNode.MakeIdentifier(
                                    "e",
                                    Helpers.MakeGenericType("ExsException")
                                ),
                                Statement.MakeBlock(
                                    Statement.MakeExprStmt(
                                        Helpers.MakeCallExpression(
                                            Helpers.MakeIdentifierPath(
                                                "println",
                                                AstType.MakeFunctionType(
                                                    "println",
                                                    Helpers.MakeVoidType(),
                                                    TextLocation.Empty,
                                                    TextLocation.Empty,
                                                    Helpers.MakePrimitiveType("string")
                                                )
                                            ),
                                            Expression.MakeConstant("string", "First catch block")
                                        )
                                    ),
                                    Statement.MakeExprStmt(
                                        Helpers.MakeCallExpression(
                                            Helpers.MakeIdentifierPath(
                                                "println",
                                                AstType.MakeFunctionType(
                                                    "println",
                                                    Helpers.MakeVoidType(),
                                                    TextLocation.Empty,
                                                    TextLocation.Empty,
                                                    Helpers.MakePrimitiveType("string")
                                                )
                                            ),
                                            Expression.MakeMemRef(
                                                Helpers.MakeIdentifierPath(
                                                    "e",
                                                    Helpers.MakeGenericType("ExsException")
                                                ),
                                                AstNode.MakeIdentifier(
                                                    "ExsMessage",
                                                    Helpers.MakePrimitiveType("string")
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
                                        Helpers.MakeIdentifierPath(
                                            "printFormat",
                                            AstType.MakeFunctionType(
                                                "printFormat",
                                                Helpers.MakeVoidType(),
                                                TextLocation.Empty,
                                                TextLocation.Empty,
                                                Helpers.MakePrimitiveType("string")
                                            )
                                        ),
                                        Expression.MakeConstant("string", "tmp is {0} at first\n"),
                                        Helpers.MakeIdentifierPath(
                                            "tmp",
                                            Helpers.MakePrimitiveType("int")
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
                                            Helpers.MakeIdentifierPath(
                                                "println",
                                                AstType.MakeFunctionType(
                                                    "println",
                                                    Helpers.MakeVoidType(),
                                                    TextLocation.Empty,
                                                    TextLocation.Empty,
                                                    Helpers.MakePrimitiveType("string")
                                                )
                                            ),
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
                                Helpers.MakeIdentifierPath(
                                    "printFormat",
                                    AstType.MakeFunctionType(
                                        "printFormat",
                                        Helpers.MakeVoidType(),
                                        TextLocation.Empty,
                                        TextLocation.Empty,
                                        Helpers.MakePrimitiveType("string")
                                    )
                                ),
                                Expression.MakeConstant("string", "tmp is {0} at last\n"),
                                Helpers.MakeIdentifierPath(
                                    "tmp",
                                    Helpers.MakePrimitiveType("int")
                                )
                            )
                        ),*/
                        Statement.MakeVarDecl(
                            Helpers.MakeSeq(
                                PatternConstruct.MakeIdentifierPattern(
                                    "tmp2",
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
                                        Helpers.MakeIdentifierPath(
                                            "printFormat",
                                            AstType.MakeFunctionType(
                                                "printFormat",
                                                Helpers.MakeVoidType(),
                                                TextLocation.Empty,
                                                TextLocation.Empty,
                                                Helpers.MakePrimitiveType("string")
                                            )
                                        ),
                                        Expression.MakeConstant("string", "tmp2 is {0} at first\n"),
                                        Helpers.MakeIdentifierPath(
                                            "tmp2",
                                            Helpers.MakePrimitiveType("int")
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
                            Statement.MakeFinallyClause(
                                Statement.MakeBlock(
                                    Statement.MakeExprStmt(
                                        Helpers.MakeCallExpression(
                                            Helpers.MakeIdentifierPath(
                                                "println",
                                                AstType.MakeFunctionType(
                                                    "println",
                                                    Helpers.MakeVoidType(),
                                                    TextLocation.Empty,
                                                    TextLocation.Empty,
                                                    Helpers.MakePrimitiveType("string")
                                                )
                                            ),
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
                            TextLocation.Empty,
                            Statement.MakeCatchClause(
                                AstNode.MakeIdentifier(
                                    "e",
                                    Helpers.MakeGenericType("ExsException")
                                ),
                                Statement.MakeBlock(
                                    Statement.MakeExprStmt(
                                        Helpers.MakeCallExpression(
                                            Helpers.MakeIdentifierPath(
                                                "println",
                                                AstType.MakeFunctionType(
                                                    "println",
                                                    Helpers.MakeVoidType(),
                                                    TextLocation.Empty,
                                                    TextLocation.Empty,
                                                    Helpers.MakePrimitiveType("string")
                                                )
                                            ),
                                            Expression.MakeConstant("string", "Second finally block")
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
                                            Helpers.MakeIdentifierPath(
                                                "println",
                                                AstType.MakeFunctionType(
                                                    "println",
                                                    Helpers.MakeVoidType(),
                                                    TextLocation.Empty,
                                                    TextLocation.Empty,
                                                    Helpers.MakePrimitiveType("string")
                                                )
                                            ),
                                            Expression.MakeMemRef(
                                                Helpers.MakeIdentifierPath(
                                                    "e",
                                                    Helpers.MakeGenericType("ExsException")
                                                ),
                                                AstNode.MakeIdentifier(
                                                    "Message",
                                                    Helpers.MakePrimitiveType("string")
                                                )
                                            )
                                        )
                                    )
                                )
                            )
                        ),
                        Statement.MakeExprStmt(
                            Helpers.MakeCallExpression(
                                Helpers.MakeIdentifierPath(
                                    "printFormat",
                                    AstType.MakeFunctionType(
                                        "printFormat",
                                        Helpers.MakeVoidType(),
                                        TextLocation.Empty,
                                        TextLocation.Empty,
                                        Helpers.MakePrimitiveType("string")
                                    )
                                ),
                                Expression.MakeConstant("string", "tmp2 is {0} at last\n"),
                                Helpers.MakeIdentifierPath(
                                    "tmp2",
                                    Helpers.MakePrimitiveType("int")
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
                                PatternConstruct.MakeIdentifierPattern(
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
                                    TextLocation.Empty,
                                    TextLocation.Empty,
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
                                PatternConstruct.MakeIdentifierPattern(
                                    "b",
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
                                    TextLocation.Empty,
                                    TextLocation.Empty,
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
                                PatternConstruct.MakeIdentifierPattern(
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
                                PatternConstruct.MakeIdentifierPattern(
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
                                Expression.MakeIndexer(
                                    Helpers.MakeIdentifierPath(
                                        "a",
                                        Helpers.MakeGenericType(
                                            "array",
                                            Helpers.MakePrimitiveType("int")
                                        )
                                    ),
                                    TextLocation.Empty,
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
                                PatternConstruct.MakeIdentifierPattern(
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
                            Statement.MakeBlock(
                                Statement.MakeExprStmt(
                                    Helpers.MakeCallExpression(
                                        Helpers.MakeIdentifierPath(
                                            "print",
                                            AstType.MakeFunctionType(
                                                "print",
                                                Helpers.MakeVoidType(),
                                                TextLocation.Empty,
                                                TextLocation.Empty,
                                                Helpers.MakePrimitiveType("string")
                                            )
                                        ),
                                        Helpers.MakeIdentifierPath(
                                            "x",
                                            Helpers.MakePrimitiveType("int")
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
                                            AstNode.MakeIdentifier(
                                                "add",
                                                AstType.MakeFunctionType(
                                                    "add",
                                                    Helpers.MakeVoidType(),
                                                    TextLocation.Empty,
                                                    TextLocation.Empty,
                                                    AstType.MakeParameterType("T")
                                                )
                                            )
                                        ),
                                        Helpers.MakeIdentifierPath(
                                            "x",
                                            Helpers.MakePrimitiveType("int")
                                        )
                                    )
                                )
                            ),
                            TextLocation.Empty,
                            AstNode.MakeVariableInitializer(
                                PatternConstruct.MakeIdentifierPattern(
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
                                )
                            )
                        ),
                        Statement.MakeExprStmt(
                            Helpers.MakeCallExpression(
                                Helpers.MakeIdentifierPath(
                                    "println",
                                    AstType.MakeFunctionType(
                                        "println",
                                        Helpers.MakeVoidType(),
                                        TextLocation.Empty,
                                        TextLocation.Empty,
                                        Helpers.MakePrimitiveType("string")
                                    )
                                ),
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
                                    "y",
                                    Helpers.MakeGenericType(
                                        "vector",
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
                                PatternConstruct.MakeIdentifierPattern(
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
                                PatternConstruct.MakeIdentifierPattern(
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
                                Expression.MakeIndexer(
                                    Helpers.MakeIdentifierPath(
                                        "a",
                                        Helpers.MakeGenericType(
                                            "array",
                                            Helpers.MakePrimitiveType("int")
                                        )
                                    ),
                                    TextLocation.Empty,
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
                                PatternConstruct.MakeIdentifierPattern(
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
                                PatternConstruct.MakeIdentifierPattern(
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
                                Expression.MakeIndexer(
                                    Helpers.MakeIdentifierPath(
                                        "c",
                                        Helpers.MakeGenericType(
                                            "vector",
                                            Helpers.MakePrimitiveType("int")
                                        )
                                    ),
                                    TextLocation.Empty,
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
                                Helpers.MakeIdentifierPath(
                                    "println",
                                    AstType.MakeFunctionType(
                                        "println",
                                        Helpers.MakeVoidType(),
                                        TextLocation.Empty,
                                        TextLocation.Empty,
                                        Helpers.MakePrimitiveType("string")
                                    )
                                ),
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
                                PatternConstruct.MakeIdentifierPattern(
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
                                PatternConstruct.MakeIdentifierPattern(
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
                                Helpers.MakeIdentifierPath(
                                    "println",
                                    AstType.MakeFunctionType(
                                        "println",
                                        Helpers.MakeVoidType(),
                                        TextLocation.Empty,
                                        TextLocation.Empty,
                                        Helpers.MakePrimitiveType("string")
                                    )
                                ),
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
                    ),
                    Helpers.MakeVoidType(),
                    Modifiers.None
                )
            });

            Assert.IsNotNull(ast);
            Helpers.AstStructuralEqual(ast, expected);
        }
    }
}

