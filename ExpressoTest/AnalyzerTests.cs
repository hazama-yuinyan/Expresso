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
                new List<EntityDeclaration>{
                    EntityDeclaration.MakeFunc(
                        "main",
                        Enumerable.Empty<ParameterDeclaration>(),
                        Statement.MakeBlock(
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
                                Helpers.MakeSeq(
                                    AstNode.MakeIdentifier(
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
                                Helpers.MakeSeq(AstNode.MakeIdentifier("f_", Helpers.MakeGenericType("array", Helpers.MakePrimitiveType("int")))),
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
                                    AstNode.MakeIdentifier(
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
                                    AstNode.MakeIdentifier(
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
                             * Helpers.MakeSeq(AstNode.MakeIdentifier("f3", Helpers.MakeGenericType("tuple"))),
                             * Helpers.MakeSeq(Expression.MakeParen(Expression.MakeSequenceExpression(null))),
                             * Modifiers.Immutable
                             * ),*/
                            Statement.MakeVarDecl(
                                Helpers.MakeSeq(
                                    AstNode.MakeIdentifier(
                                        "f3_",
                                        Helpers.MakeGenericType(
                                            "tuple",
                                            Helpers.MakePrimitiveType("int"),
                                            Helpers.MakePrimitiveType("stirng"),
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
                                    AstNode.MakeIdentifier(
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
                                    AstNode.MakeIdentifier(
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
                                Helpers.MakeSeq(AstNode.MakeIdentifier("j2", Helpers.MakePrimitiveType("intseq"))),
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
                                Helpers.MakeSeq(AstNode.MakeIdentifier("j3", Helpers.MakePrimitiveType("intseq"))),
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
                                Helpers.MakeSeq(AstNode.MakeIdentifier("j_2", Helpers.MakePrimitiveType("intseq"))),
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
                                Helpers.MakeSeq(AstNode.MakeIdentifier("j_2_", Helpers.MakePrimitiveType("intseq"))),
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
                                Helpers.MakeSeq(
                                    AstNode.MakeIdentifier(
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
                                Helpers.MakeSeq(AstNode.MakeIdentifier("m", Helpers.MakePrimitiveType("int"))),
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
                                Helpers.MakeSeq(AstNode.MakeIdentifier("m2", Helpers.MakePrimitiveType("int"))),
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
                                AstNode.MakeIdentifier("a", Helpers.MakeGenericType("TestClass"))
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
                                AstNode.MakeIdentifier("c", Helpers.MakePrimitiveType("int"))
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
                                AstNode.MakeIdentifier("d", Helpers.MakePrimitiveType("int"))
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
                                AstNode.MakeIdentifier("e", Helpers.MakePrimitiveType("int"))
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
                                AstNode.MakeIdentifier("f", Helpers.MakePrimitiveType("int"))
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
                                Expression.MakeConstant("string", "(a.x, a.y, a.z) = ({0}, {1}, {2})\n"),
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
                        Statement.MakeVarDecl(
                            Helpers.MakeSeq(
                                AstNode.MakeIdentifier(
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
                                AstNode.MakeIdentifier(
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
                                            "print",
                                            AstType.MakeFunctionType(
                                                "print",
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
                                            "print",
                                            AstType.MakeFunctionType(
                                                "print",
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
                                            "print",
                                            AstType.MakeFunctionType(
                                                "print",
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
                                            "print",
                                            AstType.MakeFunctionType(
                                                "print",
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
                                AstNode.MakeIdentifier("tmp2", Helpers.MakePrimitiveType("int"))
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
                                                "print",
                                                AstType.MakeFunctionType(
                                                    "print",
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
                                                "print",
                                                AstType.MakeFunctionType(
                                                    "print",
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
                                            Expression.MakeConstant("string", "{0} is in the range of 3 to 10"),
                                            Helpers.MakeIdentifierPath("x", Helpers.MakePrimitiveType("int"))
                                        )
                                    )
                                ),
                                PatternConstruct.MakeIdentifierPattern(
                                    "x",
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
                                                "print",
                                                AstType.MakeFunctionType(
                                                    "print",
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
                                AstNode.MakeIdentifier("tmp3", Helpers.MakeGenericType("TestClass"))
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
                                            Expression.MakeConstant("string", "x is {0}"),
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
                        Statement.MakeVarDecl(
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
                                AstNode.MakeIdentifier(
                                    "tmp5",
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
                                "tmp5",
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
                                            Expression.MakeConstant("string", "x is {0}"),
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
                    "Test",
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
                                Helpers.MakeGenericType("Test"),
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
                    Helpers.MakeGenericType("Test"),
                    Modifiers.Export
                )
            });

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
                                AstNode.MakeIdentifier(
                                    "a",
                                    AstType.MakeMemberType(
                                        Helpers.MakeGenericType("TestModule"),
                                        Helpers.MakeGenericType("Test")
                                    )
                                )
                            ),
                            Helpers.MakeSeq(
                                Expression.MakeObjectCreation(
                                    AstType.MakeMemberType(
                                        Helpers.MakeGenericType("TestModule"),
                                        Helpers.MakeGenericType("Test")
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
                                AstNode.MakeIdentifier(
                                    "b",
                                    Helpers.MakeGenericType("Test")
                                )
                            ),
                            Helpers.MakeSeq(
                                Helpers.MakeCallExpression(
                                    Expression.MakePath(
                                        Helpers.MakeSomeIdent(
                                            "TestModule"
                                        ),
                                        AstNode.MakeIdentifier(
                                            "createTest",
                                            AstType.MakeFunctionType(
                                                "createTest",
                                                Helpers.MakeGenericType("Test"),
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
                                AstNode.MakeIdentifier(
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
                                    Helpers.MakeSomeIdent(
                                        "TestModule"
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
                                        Helpers.MakeGenericType("Test")
                                    )
                                ),
                                Helpers.MakeIdentifierPath(
                                    "b",
                                    Helpers.MakeGenericType("Test")
                                ),
                                Helpers.MakeIdentifierPath(
                                    "c",
                                    Helpers.MakeGenericType(
                                        "tuple",
                                        Helpers.MakePrimitiveType("int"),
                                        Helpers.MakePrimitiveType("int")
                                    )
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
                                AstNode.MakeIdentifier(
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
                                AstNode.MakeIdentifier(
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
                                                AstNode.MakeIdentifier(
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
                                AstNode.MakeIdentifier(
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
                                AstNode.MakeIdentifier(
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
                                AstNode.MakeIdentifier(
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
                                AstNode.MakeIdentifier(
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
                                AstNode.MakeIdentifier(
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
                                AstNode.MakeIdentifier(
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
    }
}

