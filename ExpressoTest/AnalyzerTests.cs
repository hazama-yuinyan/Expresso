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

            var expected_ast = AstNode.MakeModuleDef("main", new List<EntityDeclaration>{
                EntityDeclaration.MakeFunc("main",
                    Enumerable.Empty<ParameterDeclaration>(),
                    Statement.MakeBlock(new List<Statement>{
                        Statement.MakeVarDecl(
                            Helpers.MakeSeq(AstNode.MakeIdentifier("a", Helpers.MakePrimitiveType("int"))),
                            Helpers.MakeSeq(Expression.MakeConstant("int", 255)),
                            Modifiers.Immutable
                        ),
                        Statement.MakeVarDecl(
                            Helpers.MakeSeq(AstNode.MakeIdentifier("h_a", Helpers.MakePrimitiveType("int"))),
                            Helpers.MakeSeq(Expression.MakeConstant("int", 0xff)),
                            Modifiers.Immutable
                        ),
                        Statement.MakeVarDecl(
                            Helpers.MakeSeq(AstNode.MakeIdentifier("h_a_", Helpers.MakePrimitiveType("int"))),
                            Helpers.MakeSeq(Expression.MakeConstant("int", 0xff)),
                            Modifiers.Immutable
                        ),
                        Statement.MakeVarDecl(
                            Helpers.MakeSeq(AstNode.MakeIdentifier("b", Helpers.MakePrimitiveType("double"))),
                            Helpers.MakeSeq(Expression.MakeConstant("double", 1000.0)),
                            Modifiers.Immutable
                        ),
                        Statement.MakeVarDecl(
                            Helpers.MakeSeq(AstNode.MakeIdentifier("f_b", Helpers.MakePrimitiveType("double"))),
                            Helpers.MakeSeq(Expression.MakeConstant("double", 1.0e4)),
                            Modifiers.Immutable
                        ),
                        Statement.MakeVarDecl(
                            Helpers.MakeSeq(AstNode.MakeIdentifier("f_b_", Helpers.MakePrimitiveType("double"))),
                            Helpers.MakeSeq(Expression.MakeConstant("double", 1.0e4)),
                            Modifiers.Immutable
                        ),
                        Statement.MakeVarDecl(
                            Helpers.MakeSeq(AstNode.MakeIdentifier("c", Helpers.MakePrimitiveType("double"))),
                            Helpers.MakeSeq(Expression.MakeConstant("double", 0.001)),
                            Modifiers.Immutable
                        ),
                        Statement.MakeVarDecl(
                            Helpers.MakeSeq(AstNode.MakeIdentifier("f_c", Helpers.MakePrimitiveType("double"))),
                            Helpers.MakeSeq(Expression.MakeConstant("double", .1e-2)),
                            Modifiers.Immutable
                        ),
                        Statement.MakeVarDecl(
                            Helpers.MakeSeq(AstNode.MakeIdentifier("f_c2", Helpers.MakePrimitiveType("double"))),
                            Helpers.MakeSeq(Expression.MakeConstant("double", .0001)),
                            Modifiers.Immutable
                        ),
                        Statement.MakeVarDecl(
                            Helpers.MakeSeq(AstNode.MakeIdentifier("d", Helpers.MakePrimitiveType("bigint"))),
                            Helpers.MakeSeq(Expression.MakeConstant("bigint", BigInteger.Parse("10000000"))),
                            Modifiers.Immutable
                        ),
                        Statement.MakeVarDecl(
                            Helpers.MakeSeq(AstNode.MakeIdentifier("d_", Helpers.MakePrimitiveType("bigint"))),
                            Helpers.MakeSeq(Expression.MakeConstant("bigint", BigInteger.Parse("10000000"))),
                            Modifiers.Immutable
                        ),
                        Statement.MakeVarDecl(
                            Helpers.MakeSeq(AstNode.MakeIdentifier("d2", Helpers.MakePrimitiveType("bigint"))),
                            Helpers.MakeSeq(Expression.MakeConstant("bigint", BigInteger.Parse("10000000"))),
                            Modifiers.Immutable
                        ),
                        Statement.MakeVarDecl(
                            Helpers.MakeSeq(AstNode.MakeIdentifier("e", Helpers.MakePrimitiveType("string"))),
                            Helpers.MakeSeq(Expression.MakeConstant("string", "This is a test")),
                            Modifiers.Immutable
                        ),
                        Statement.MakeVarDecl(
                            Helpers.MakeSeq(AstNode.MakeIdentifier("u", Helpers.MakePrimitiveType("uint"))),
                            Helpers.MakeSeq(Expression.MakeConstant("uint", 1000u)),
                            Modifiers.Immutable
                        ),
                        Statement.MakeVarDecl(
                            Helpers.MakeSeq(AstNode.MakeIdentifier("u_", Helpers.MakePrimitiveType("uint"))),
                            Helpers.MakeSeq(Expression.MakeConstant("uint", 1000U)),
                            Modifiers.Immutable
                        ),
                        Statement.MakeVarDecl(
                            Helpers.MakeSeq(AstNode.MakeIdentifier("f_a", Helpers.MakePrimitiveType("float"))),
                            Helpers.MakeSeq(Expression.MakeConstant("float", 1.0e4f)),
                            Modifiers.Immutable
                        ),
                        Statement.MakeVarDecl(
                            Helpers.MakeSeq(AstNode.MakeIdentifier("f_a_", Helpers.MakePrimitiveType("float"))),
                            Helpers.MakeSeq(Expression.MakeConstant("float", 1.0e4f)),
                            Modifiers.Immutable
                        ),
                        Statement.MakeVarDecl(
                            Helpers.MakeSeq(AstNode.MakeIdentifier("f", Helpers.MakeGenericType("array", Helpers.MakePrimitiveType("int")))),
                                Helpers.MakeSeq(Expression.MakeSequenceInitializer(Helpers.MakeGenericType("array", Helpers.MakePrimitiveType("int")),
                                Enumerable.Empty<Expression>()
                            )),
                            Modifiers.Immutable
                        ),
                        Statement.MakeVarDecl(
                            Helpers.MakeSeq(AstNode.MakeIdentifier("f_", Helpers.MakeGenericType("array", Helpers.MakePrimitiveType("int")))),
                            Helpers.MakeSeq(Expression.MakeSequenceInitializer(Helpers.MakeGenericType("array", Helpers.MakePrimitiveType("int")),
                                Helpers.MakeSeq(
                                    Expression.MakeConstant("int", 1),
                                    Expression.MakeConstant("int", 2),
                                    Expression.MakeConstant("int", 3)
                                )
                            )),
                            Modifiers.Immutable
                        ),
                        Statement.MakeVarDecl(
                            Helpers.MakeSeq(AstNode.MakeIdentifier("f2", Helpers.MakeGenericType("vector", Helpers.MakePrimitiveType("int")))),
                                Helpers.MakeSeq(Expression.MakeSequenceInitializer(Helpers.MakeGenericType("vector", Helpers.MakePrimitiveType("int")),
                                Enumerable.Empty<Expression>()
                            )),
                            Modifiers.Immutable
                        ),
                        Statement.MakeVarDecl(
                            Helpers.MakeSeq(AstNode.MakeIdentifier("f2_", Helpers.MakeGenericType("vector", Helpers.MakePrimitiveType("int")))),
                            Helpers.MakeSeq(Expression.MakeSequenceInitializer(Helpers.MakeGenericType("vector", Helpers.MakePrimitiveType("int")),
                                Helpers.MakeSeq(
                                    Expression.MakeConstant("int", 1),
                                    Expression.MakeConstant("int", 2),
                                    Expression.MakeConstant("int", 3)
                                ))
                            ),
                            Modifiers.Immutable
                        ),
                        /*Statement.MakeVarDecl(
                            Helpers.MakeSeq(AstNode.MakeIdentifier("f3", Helpers.MakeGenericType("tuple"))),
                            Helpers.MakeSeq(Expression.MakeParen(Expression.MakeSequenceExpression(null))),
                            Modifiers.Immutable
                        ),*/
                        Statement.MakeVarDecl(
                            Helpers.MakeSeq(AstNode.MakeIdentifier("f3_",
                                Helpers.MakeGenericType("tuple",
                                    Helpers.MakePrimitiveType("int"),
                                    Helpers.MakePrimitiveType("stirng"),
                                    Helpers.MakePrimitiveType("bool")
                                )
                            )),
                            Helpers.MakeSeq(Expression.MakeParen(
                                Expression.MakeSequenceExpression(
                                    Expression.MakeConstant("int", 1),
                                    Expression.MakeConstant("string", "abc"),
                                    Expression.MakeConstant("bool", true)
                                )
                            )),
                            Modifiers.Immutable
                        ),
                        Statement.MakeVarDecl(
                            Helpers.MakeSeq(AstNode.MakeIdentifier("g",
                                Helpers.MakeGenericType("dictionary",
                                    Helpers.MakePrimitiveType("string"),
                                    Helpers.MakePrimitiveType("int")
                                ))
                            ),
                            Helpers.MakeSeq(Expression.MakeSequenceInitializer(
                                Helpers.MakeGenericType("dictionary",
                                                        Helpers.MakePrimitiveType("string"),
                                                        Helpers.MakePrimitiveType("int")
                                ),
                                Enumerable.Empty<Expression>()
                            )),
                            Modifiers.Immutable
                        ),
                        Statement.MakeVarDecl(
                            Helpers.MakeSeq(AstNode.MakeIdentifier("g_",
                                Helpers.MakeGenericType("dictionary",
                                    Helpers.MakePrimitiveType("string"),
                                    Helpers.MakePrimitiveType("int")
                                )
                            )),
                            Helpers.MakeSeq(Expression.MakeSequenceInitializer(Helpers.MakeGenericType("dictionary",
                                Helpers.MakePrimitiveType("string"),
                                Helpers.MakePrimitiveType("int")
                            ),
                                Helpers.MakeSeq(
                                    Expression.MakeKeyValuePair(Expression.MakeConstant("string", "akari"), Expression.MakeConstant("int", 13)),
                                    Expression.MakeKeyValuePair(Expression.MakeConstant("string", "chinatsu"), Expression.MakeConstant("int", 13)),
                                    Expression.MakeKeyValuePair(Expression.MakeConstant("string", "京子"), Expression.MakeConstant("int", 14)),
                                    Expression.MakeKeyValuePair(Expression.MakeConstant("string", "結衣"), Expression.MakeConstant("int", 14))
                                )
                            )),
                            Modifiers.Immutable
                        ),
                        Statement.MakeVarDecl(
                            Helpers.MakeSeq(AstNode.MakeIdentifier("h", Helpers.MakePrimitiveType("string"))),
                            Helpers.MakeSeq(Expression.MakeConstant("string", "私変わっちゃうの・・？")),
                            Modifiers.None
                        ),
                        Statement.MakeVarDecl(
                            Helpers.MakeSeq(AstNode.MakeIdentifier("h2", Helpers.MakePrimitiveType("string"))),
                            Helpers.MakeSeq(Expression.MakeConstant("string", "Yes, you can!")),
                            Modifiers.None
                        ),
                        Statement.MakeVarDecl(
                            Helpers.MakeSeq(AstNode.MakeIdentifier("h3", Helpers.MakePrimitiveType("char"))),
                            Helpers.MakeSeq(Expression.MakeConstant("char", 'a')),
                            Modifiers.None
                        ),
                        Statement.MakeVarDecl(
                            Helpers.MakeSeq(AstNode.MakeIdentifier("i", Helpers.MakePrimitiveType("string"))),
                            Helpers.MakeSeq(Expression.MakeConstant("string", "よかった。私変わらないんだね！・・え、変われないの？・・・なんだかフクザツ")),
                            Modifiers.Immutable
                        ),
                        Statement.MakeVarDecl(
                            Helpers.MakeSeq(AstNode.MakeIdentifier("i2", Helpers.MakePrimitiveType("string"))),
                            Helpers.MakeSeq(Expression.MakeConstant("string", "Oh, you just can't...")),
                            Modifiers.Immutable
                        ),
                        Statement.MakeVarDecl(
                            Helpers.MakeSeq(AstNode.MakeIdentifier("i3", Helpers.MakePrimitiveType("char"))),
                            Helpers.MakeSeq(Expression.MakeConstant("char", 'a')),
                            Modifiers.Immutable
                        ),
                        Statement.MakeVarDecl(
                            Helpers.MakeSeq(AstNode.MakeIdentifier("i4", Helpers.MakePrimitiveType("string"))),
                            Helpers.MakeSeq(Expression.MakeConstant("string", "\u0041\u005a\u0061\u007A\u3042\u30A2")), //AZazあア
                            Modifiers.Immutable
                        ),
                        Statement.MakeVarDecl(
                            Helpers.MakeSeq(AstNode.MakeIdentifier("i5", Helpers.MakePrimitiveType("char"))),
                            Helpers.MakeSeq(Expression.MakeConstant("char", '\u0041')), //A
                            Modifiers.Immutable
                        ),
                        Statement.MakeVarDecl(
                            Helpers.MakeSeq(AstNode.MakeIdentifier("i6", Helpers.MakePrimitiveType("string"))),
                            Helpers.MakeSeq(Expression.MakeConstant("string", "This is a normal string.\n Seems 2 lines? Yes, you're right!")),
                            Modifiers.Immutable
                        ),
                        Statement.MakeVarDecl(
                            Helpers.MakeSeq(AstNode.MakeIdentifier("i6_", Helpers.MakePrimitiveType("string"))),
                            Helpers.MakeSeq(Expression.MakeConstant("string", @"This is a raw string.\n Seems 2 lines? Nah, indeed.")),
                            Modifiers.Immutable
                        ),
                        Statement.MakeVarDecl(
                            Helpers.MakeSeq(AstNode.MakeIdentifier("j", Helpers.MakePrimitiveType("intseq"))),
                            Helpers.MakeSeq(Expression.MakeIntSeq(Expression.MakeConstant("int", 1), Expression.MakeConstant("int", 10), Expression.MakeConstant("int", 1), false)),
                            Modifiers.Immutable
                        ),
                        Statement.MakeVarDecl(
                            Helpers.MakeSeq(AstNode.MakeIdentifier("j2", Helpers.MakePrimitiveType("intseq"))),
                            Helpers.MakeSeq(Expression.MakeIntSeq(Expression.MakeConstant("int", 1), Expression.MakeConstant("int", 10), Expression.MakeConstant("int", 1), false)),
                            Modifiers.Immutable
                        ),
                        Statement.MakeVarDecl(
                            Helpers.MakeSeq(AstNode.MakeIdentifier("j3", Helpers.MakePrimitiveType("intseq"))),
                            Helpers.MakeSeq(Expression.MakeIntSeq(Expression.MakeConstant("int", 1), Expression.MakeConstant("int", 10), Expression.MakeConstant("int", 1), false)),
                            Modifiers.Immutable
                        ),
                        Statement.MakeVarDecl(
                            Helpers.MakeSeq(AstNode.MakeIdentifier("j_2", Helpers.MakePrimitiveType("intseq"))),
                            Helpers.MakeSeq(Expression.MakeIntSeq(Expression.MakeUnaryExpr(OperatorType.Minus, Expression.MakeConstant("int", 5)), Expression.MakeUnaryExpr(OperatorType.Minus, Expression.MakeConstant("int", 10)), Expression.MakeUnaryExpr(OperatorType.Minus, Expression.MakeConstant("int", 1)), false)),
                            Modifiers.Immutable
                        ),
                        Statement.MakeVarDecl(
                            Helpers.MakeSeq(AstNode.MakeIdentifier("j_2_", Helpers.MakePrimitiveType("intseq"))),
                            Helpers.MakeSeq(Expression.MakeIntSeq(Expression.MakeConstant("int", 0), Expression.MakeUnaryExpr(OperatorType.Minus, Expression.MakeConstant("int", 10)), Expression.MakeUnaryExpr(OperatorType.Minus, Expression.MakeConstant("int", 1)), true)),
                            Modifiers.Immutable
                        ),
                        Statement.MakeExprStmt(Helpers.MakeCallExpression(
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
                            Helpers.MakeIdentifierPath("a"),
                            Helpers.MakeIdentifierPath("h_a"),
                            Helpers.MakeIdentifierPath("h_a_"),
                            Helpers.MakeIdentifierPath("b"),
                            Helpers.MakeIdentifierPath("f_b"),
                            Helpers.MakeIdentifierPath("f_b_"),
                            Helpers.MakeIdentifierPath("c"),
                            Helpers.MakeIdentifierPath("f_c"),
                            Helpers.MakeIdentifierPath("f_c2"),
                            Helpers.MakeIdentifierPath("d"),
                            Helpers.MakeIdentifierPath("d_"),
                            Helpers.MakeIdentifierPath("d2"),
                            Helpers.MakeIdentifierPath("e"),
                            Helpers.MakeIdentifierPath("u"),
                            Helpers.MakeIdentifierPath("u_"),
                            Helpers.MakeIdentifierPath("f_a"),
                            Helpers.MakeIdentifierPath("f_a_"),
                            Helpers.MakeIdentifierPath("f"),
                            Helpers.MakeIdentifierPath("f_"),
                            Helpers.MakeIdentifierPath("f2"),
                            Helpers.MakeIdentifierPath("f2_"),
                            Helpers.MakeIdentifierPath("f3_"),
                            Helpers.MakeIdentifierPath("g"),
                            Helpers.MakeIdentifierPath("g_"),
                            Helpers.MakeIdentifierPath("h"),
                            Helpers.MakeIdentifierPath("h2"),
                            Helpers.MakeIdentifierPath("h3"),
                            Helpers.MakeIdentifierPath("i"),
                            Helpers.MakeIdentifierPath("i2"),
                            Helpers.MakeIdentifierPath("i3"),
                            Helpers.MakeIdentifierPath("i4"),
                            Helpers.MakeIdentifierPath("i5"),
                            Helpers.MakeIdentifierPath("i6"),
                            Helpers.MakeIdentifierPath("i6_"),
                            Helpers.MakeIdentifierPath("j"),
                            Helpers.MakeIdentifierPath("j2"),
                            Helpers.MakeIdentifierPath("j3"),
                            Helpers.MakeIdentifierPath("j_2"),
                            Helpers.MakeIdentifierPath("j_2_")
                        ))
                    }),
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
                EntityDeclaration.MakeFunc("main",
                    Enumerable.Empty<ParameterDeclaration>(),
                    Statement.MakeBlock(new List<Statement>{
                        Statement.MakeVarDecl(
                            Helpers.MakeSeq(AstNode.MakeIdentifier("ary", Helpers.MakeGenericType("array", Helpers.MakePrimitiveType("int")))),
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
                            Helpers.MakeSeq(AstNode.MakeIdentifier("d",
                                Helpers.MakeGenericType("dictionary",
                                    Helpers.MakePrimitiveType("string"),
                                    Helpers.MakePrimitiveType("int")
                                )
                            )),
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
                            Helpers.MakeSeq(AstNode.MakeIdentifier("m", Helpers.MakePrimitiveType("int"))),
                            Helpers.MakeSeq(Helpers.MakeIndexerExpression(
                                Helpers.MakeIdentifierPath(
                                    "ary",
                                    Helpers.MakeGenericType("array", Helpers.MakePrimitiveType("int"))
                                ),
                                Expression.MakeConstant("int", 0)
                            )),
                            Modifiers.Immutable
                        ),
                        Statement.MakeVarDecl(
                            Helpers.MakeSeq(AstNode.MakeIdentifier("m2", Helpers.MakePrimitiveType("int"))),
                            Helpers.MakeSeq(Helpers.MakeIndexerExpression(
                                Helpers.MakeIdentifierPath(
                                    "d",
                                    Helpers.MakeGenericType("dictionary", Helpers.MakePrimitiveType("string"), Helpers.MakePrimitiveType("int"))
                                ),
                                Expression.MakeConstant("string", "a")
                            )),
                            Modifiers.Immutable
                        ),
                        Statement.MakeVarDecl(
                            Helpers.MakeSeq(AstNode.MakeIdentifier("x", Helpers.MakePrimitiveType("int"))),
                            Helpers.MakeSeq(Expression.MakeConstant("int", 100)),
                            Modifiers.Immutable
                        ),
                        Statement.MakeVarDecl(
                            Helpers.MakeSeq(AstNode.MakeIdentifier("p", Helpers.MakePrimitiveType("int"))),
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
                            Helpers.MakeSeq(AstNode.MakeIdentifier("q", Helpers.MakePrimitiveType("int"))),
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
                            Helpers.MakeSeq(AstNode.MakeIdentifier("r", Helpers.MakePrimitiveType("int"))),
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
                            Helpers.MakeSeq(AstNode.MakeIdentifier("s", Helpers.MakePrimitiveType("int"))),
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
                            Helpers.MakeSeq(AstNode.MakeIdentifier("t", Helpers.MakePrimitiveType("int"))),
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
                            Helpers.MakeSeq(AstNode.MakeIdentifier("v", Helpers.MakePrimitiveType("int"))),
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
                            Helpers.MakeSeq(AstNode.MakeIdentifier("w", Helpers.MakePrimitiveType("int"))),
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
                            Helpers.MakeSeq(AstNode.MakeIdentifier("y", Helpers.MakePrimitiveType("int"))),
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
                            Helpers.MakeSeq(AstNode.MakeIdentifier("z", Helpers.MakePrimitiveType("int"))),
                            Helpers.MakeSeq(
                                Expression.MakeBinaryExpr(
                                    OperatorType.Times,
                                    Helpers.MakeIdentifierPath("v", Helpers.MakePrimitiveType("int")),
                                    Helpers.MakeIdentifierPath("w", Helpers.MakePrimitiveType("int"))
                                )
                            ),
                            Modifiers.Immutable
                        ),
                        Statement.MakeExprStmt(Helpers.MakeCallExpression(
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
                            Helpers.MakeIdentifierPath("ary"),
                            Helpers.MakeIdentifierPath("d"),
                            Helpers.MakeIdentifierPath("m"),
                            Helpers.MakeIdentifierPath("m2"),
                            Helpers.MakeIdentifierPath("x"),
                            Helpers.MakeIdentifierPath("p"),
                            Helpers.MakeIdentifierPath("q"),
                            Helpers.MakeIdentifierPath("r"),
                            Helpers.MakeIdentifierPath("s"),
                            Helpers.MakeIdentifierPath("t"),
                            Helpers.MakeIdentifierPath("v"),
                            Helpers.MakeIdentifierPath("w"),
                            Helpers.MakeIdentifierPath("y"),
                            Helpers.MakeIdentifierPath("z")
                        ))
                    }),
                                           Helpers.MakeVoidType(),
                    Modifiers.None
                )
            });

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
                                AstNode.MakeIdentifier("x", Helpers.MakePrimitiveType("int")),
                                AstNode.MakeIdentifier("xp", Helpers.MakePrimitiveType("int")),
                                AstNode.MakeIdentifier("xm", Helpers.MakePrimitiveType("int")),
                                AstNode.MakeIdentifier("xt", Helpers.MakePrimitiveType("int")),
                                AstNode.MakeIdentifier("xd", Helpers.MakePrimitiveType("int")),
                                AstNode.MakeIdentifier("xmod", Helpers.MakePrimitiveType("int")),
                                AstNode.MakeIdentifier("xpower", Helpers.MakePrimitiveType("int"))
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
                                AstNode.MakeIdentifier("a", Helpers.MakePrimitiveType("int"))
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
                                AstNode.MakeIdentifier("b", Helpers.MakePrimitiveType("int"))
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
                                AstNode.MakeIdentifier("c", Helpers.MakePrimitiveType("int"))
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
                                AstNode.MakeIdentifier("d", Helpers.MakePrimitiveType("int"))
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
                                AstNode.MakeIdentifier("e", Helpers.MakePrimitiveType("int"))
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
                                AstNode.MakeIdentifier("f", Helpers.MakePrimitiveType("int"))
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
                                Helpers.MakeIdentifierPath("x"),
                                Helpers.MakeIdentifierPath("a"),
                                Helpers.MakeIdentifierPath("b"),
                                Helpers.MakeIdentifierPath("c"),
                                Helpers.MakeIdentifierPath("d"),
                                Helpers.MakeIdentifierPath("e"),
                                Helpers.MakeIdentifierPath("f"),
                                Helpers.MakeIdentifierPath("xp"),
                                Helpers.MakeIdentifierPath("xm"),
                                Helpers.MakeIdentifierPath("xt"),
                                Helpers.MakeIdentifierPath("xd"),
                                Helpers.MakeIdentifierPath("xmod"),
                                Helpers.MakeIdentifierPath("xpower")
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
                                AstNode.MakeIdentifier(
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
                                AstNode.MakeIdentifier(
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
                                AstNode.MakeIdentifier(
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
                                AstNode.MakeIdentifier(
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
                                AstNode.MakeIdentifier(
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
                                AstNode.MakeIdentifier(
                                    "a",
                                    Helpers.MakePrimitiveType("int")
                                ),
                                AstNode.MakeIdentifier(
                                    "b",
                                    Helpers.MakePrimitiveType("int")
                                ),
                                AstNode.MakeIdentifier(
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
                                AstNode.MakeIdentifier(
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
                                AstNode.MakeIdentifier(
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
                                AstNode.MakeIdentifier("x", Helpers.MakePrimitiveType("int")),
                                AstNode.MakeIdentifier("y", Helpers.MakePrimitiveType("int")),
                                AstNode.MakeIdentifier("z", Helpers.MakePrimitiveType("int")),
                                AstNode.MakeIdentifier("w", Helpers.MakePrimitiveType("int"))
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
                                AstNode.MakeIdentifier("flag", Helpers.MakePrimitiveType("bool"))
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
                                AstNode.MakeIdentifier(
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
                            AstNode.MakeVariableInitializer(
                                AstNode.MakeIdentifier("p", Helpers.MakePrimitiveType("int")),
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
                                AstNode.MakeIdentifier(
                                    "fibs",
                                    Helpers.MakeGenericType(
                                        "vector",
                                        Helpers.MakePrimitiveType("int")
                                    )
                                ),
                                AstNode.MakeIdentifier(
                                    "a",
                                    Helpers.MakePrimitiveType("int")
                                ),
                                AstNode.MakeIdentifier(
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
                                AstNode.MakeIdentifier(
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
                                    AstNode.MakeVariableInitializer(
                                        AstNode.MakeIdentifier(
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
                            AstNode.MakeVariableInitializer(
                                AstNode.MakeIdentifier(
                                    "i",
                                    Helpers.MakePrimitiveType("int")
                                ),
                                Expression.MakeSequenceInitializer(
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
                    "Test",
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
                                        Expression.MakeMemRef(
                                            Expression.MakeSelfRef(),
                                            AstNode.MakeIdentifier(
                                                "x",
                                                Helpers.MakePrimitiveType("int")
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
                                AstNode.MakeIdentifier("a", Helpers.MakeGenericType("Test"))
                            ),
                            Helpers.MakeSeq(
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
                            ),
                            Modifiers.Immutable
                        ),
                        Statement.MakeVarDecl(
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
                        ),
                        Statement.MakeVarDecl(
                            Helpers.MakeSeq(
                                AstNode.MakeIdentifier("c", Helpers.MakePrimitiveType("int"))
                            ),
                            Helpers.MakeSeq(
                                Expression.MakeCallExpr(
                                    Expression.MakeMemRef(
                                        Helpers.MakeIdentifierPath(
                                            "a",
                                            Helpers.MakeGenericType("Test")
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
                                AstNode.MakeIdentifier("d", Helpers.MakePrimitiveType("int"))
                            ),
                            Helpers.MakeSeq(
                                Expression.MakeCallExpr(
                                    Expression.MakeMemRef(
                                        Helpers.MakeIdentifierPath(
                                            "a",
                                            Helpers.MakeGenericType("Test")
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
                                AstNode.MakeIdentifier("e", Helpers.MakePrimitiveType("int"))
                            ),
                            Helpers.MakeSeq(
                                Helpers.MakeCallExpression(
                                    Expression.MakeMemRef(
                                        Helpers.MakeIdentifierPath(
                                            "a",
                                            Helpers.MakeGenericType("Test")
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
                                Expression.MakeConstant("string", "(a.x, a.y) = ({0}, {1})\n"),
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
                                AstNode.MakeIdentifier(
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
                    "main",
                    Enumerable.Empty<ParameterDeclaration>(),
                    Statement.MakeBlock(
                        Statement.MakeVarDecl(
                            Helpers.MakeSeq(
                                AstNode.MakeIdentifier(
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
                                AstNode.MakeIdentifier(
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
                                AstNode.MakeIdentifier(
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
    }
}

