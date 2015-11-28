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
                            Helpers.MakeSeq(Expression.MakeSequenceInitializer(Helpers.MakeGenericType("array", Helpers.MakePlaceholderType()),
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
                            Helpers.MakeSeq(Expression.MakeSequenceInitializer(Helpers.MakeGenericType("vector", Helpers.MakePlaceholderType()),
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
                        Statement.MakeVarDecl(
                            Helpers.MakeSeq(AstNode.MakeIdentifier("f3", Helpers.MakeGenericType("tuple"))),
                            Helpers.MakeSeq(Expression.MakeParen(Expression.MakeSequenceExpression(null))),
                            Modifiers.Immutable
                        ),
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
                                    Helpers.MakePlaceholderType(),
                                    Helpers.MakePlaceholderType()
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
                        )
                    }),
                    AstType.Null,
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
                EntityDeclaration.MakeClassDecl("Test", Enumerable.Empty<AstType>(), Helpers.MakeSeq<EntityDeclaration>(
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
                        "getY",
                        Enumerable.Empty<ParameterDeclaration>(),
                        Statement.MakeBlock(
                            Helpers.MakeSeq(
                                Statement.MakeReturnStmt(Expression.MakeSequenceExpression(Expression.MakeMemRef(
                                    Expression.MakeSelfRef(TextLocation.Empty),
                                    AstNode.MakeIdentifier("y")
                                )))
                            )
                        ),
                        Helpers.MakePrimitiveType("int"),
                        Modifiers.Public
                    )),
                    Modifiers.None
                ),
                EntityDeclaration.MakeFunc("createTest",
                    Helpers.MakeSeq(
                        EntityDeclaration.MakeParameter("x", Helpers.MakePrimitiveType("int")),
                        EntityDeclaration.MakeParameter("y", Helpers.MakePrimitiveType("int"), Expression.MakeConstant("int", 2)),
                        EntityDeclaration.MakeParameter("z", Helpers.MakePrimitiveType("int"), Expression.MakeConstant("int", 3))
                    ),
                    Statement.MakeBlock(
                        Helpers.MakeSeq(
                            Helpers.MakeReturnStatement(Expression.MakeNewExpr(Expression.MakeObjectCreation(
                                Helpers.MakeGenericType("Test"),
                                Helpers.MakeSeq(
                                    AstNode.MakeIdentifier("x"),
                                    AstNode.MakeIdentifier("y"),
                                    AstNode.MakeIdentifier("z")
                                ),
                                Helpers.MakeSeq(
                                    Expression.MakePath(Helpers.MakeSomeIdent("x")),
                                    Expression.MakePath(Helpers.MakeSomeIdent("y")),
                                    Expression.MakePath(Helpers.MakeSomeIdent("z"))
                                )
                            )))
                        )
                    ), 
                    Helpers.MakeGenericType("Test"),
                    Modifiers.Export
                ),
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
                            Helpers.MakeSeq(AstNode.MakeIdentifier("k", Helpers.MakeGenericType("Test"))),
                            Helpers.MakeSeq(Expression.MakeObjectCreation(
                                Helpers.MakeGenericType("Test"),
                                Helpers.MakeSeq(AstNode.MakeIdentifier("x"), AstNode.MakeIdentifier("y")),
                                Helpers.MakeSeq(Expression.MakeConstant("int", 1), Expression.MakeConstant("int", 2))
                            )),
                            Modifiers.Immutable
                        ),
                        Statement.MakeVarDecl(
                            Helpers.MakeSeq(AstNode.MakeIdentifier("k2", Helpers.MakeGenericType("Test"))),
                            Helpers.MakeSeq(Expression.MakeNewExpr(Expression.MakeObjectCreation(
                                Helpers.MakeGenericType("Test"),
                                Helpers.MakeSeq(AstNode.MakeIdentifier("x"), AstNode.MakeIdentifier("y")),
                                Helpers.MakeSeq(Expression.MakeConstant("int", 1), Expression.MakeConstant("int", 2))
                            ))),
                            Modifiers.Immutable
                        ),
                        Statement.MakeVarDecl(
                            Helpers.MakeSeq(AstNode.MakeIdentifier("l", Helpers.MakeGenericType("Test"))),
                            Helpers.MakeSeq(Expression.MakeCallExpr(
                                Expression.MakePath(Helpers.MakeSomeIdent("createTest")),
                                Expression.MakeConstant("int", 1),
                                Expression.MakeConstant("int", 2),
                                Expression.MakeConstant("int", 4)
                            )),
                            Modifiers.Immutable
                        ),
                        Statement.MakeVarDecl(
                            Helpers.MakeSeq(AstNode.MakeIdentifier("m", Helpers.MakePrimitiveType("int"))),
                            Helpers.MakeSeq(Expression.MakeIndexer(Expression.MakePath(Helpers.MakeSomeIdent("ary")),
                                Expression.MakeConstant("int", 0)
                            )),
                            Modifiers.Immutable
                        ),
                        Statement.MakeVarDecl(
                            Helpers.MakeSeq(AstNode.MakeIdentifier("m2", Helpers.MakePrimitiveType("int"))),
                            Helpers.MakeSeq(Expression.MakeIndexer(Expression.MakePath(Helpers.MakeSomeIdent("d")),
                                Expression.MakeConstant("string", "a")
                            )),
                            Modifiers.Immutable
                        ),
                        Statement.MakeVarDecl(
                            Helpers.MakeSeq(AstNode.MakeIdentifier("n", Helpers.MakePrimitiveType("int"))),
                            Helpers.MakeSeq(Expression.MakeMemRef(
                                Expression.MakePath(Helpers.MakeSomeIdent("k")),
                                AstNode.MakeIdentifier("x"))
                            ),
                            Modifiers.Immutable
                        ),
                        Statement.MakeVarDecl(
                            Helpers.MakeSeq(AstNode.MakeIdentifier("n2", Helpers.MakePrimitiveType("int"))),
                            Helpers.MakeSeq(
                                Expression.MakeCallExpr(Expression.MakeMemRef(
                                    Expression.MakePath(Helpers.MakeSeq(Helpers.MakeSomeIdent("k"))),
                                    AstNode.MakeIdentifier("getY")
                                ), Enumerable.Empty<Expression>())
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
                            Helpers.MakeSeq(Expression.MakeBinaryExpr(OperatorType.Plus,
                                Expression.MakeIndexer(Expression.MakePath(Helpers.MakeSomeIdent("ary")),
                                    Expression.MakeConstant("int", 0)
                                ),
                                Expression.MakeBinaryExpr(OperatorType.Plus,
                                    Expression.MakeIndexer(Expression.MakePath(Helpers.MakeSomeIdent("ary")),
                                        Expression.MakeConstant("int", 1)
                                    ),
                                    Expression.MakeIndexer(Expression.MakePath(Helpers.MakeSeq(Helpers.MakeSomeIdent("ary"))),
                                        Helpers.MakeSeq(Expression.MakeConstant("int", 2))
                                    )
                                ))
                            ),
                            Modifiers.Immutable
                        ),
                        Statement.MakeVarDecl(
                            Helpers.MakeSeq(AstNode.MakeIdentifier("q", Helpers.MakePrimitiveType("int"))),
                            Helpers.MakeSeq(Expression.MakeBinaryExpr(OperatorType.Plus,
                                Expression.MakeIndexer(Expression.MakePath(Helpers.MakeSomeIdent("d")),
                                    Expression.MakeConstant("string", "a")
                                ),
                                Expression.MakeBinaryExpr(OperatorType.Plus,
                                    Expression.MakeIndexer(Expression.MakePath(Helpers.MakeSomeIdent("d")),
                                        Expression.MakeConstant("string", "b")
                                    ),
                                    Expression.MakeIndexer(Expression.MakePath(Helpers.MakeSomeIdent("d")),
                                        Expression.MakeConstant("string", "何か")
                                    )
                                ))
                            ),
                            Modifiers.Immutable
                        ),
                        Statement.MakeVarDecl(
                            Helpers.MakeSeq(AstNode.MakeIdentifier("r", Helpers.MakePrimitiveType("int"))),
                            Helpers.MakeSeq(Expression.MakeBinaryExpr(OperatorType.BitwiseShiftRight,
                                Expression.MakePath(Helpers.MakeSomeIdent("x")),
                                Expression.MakePath(Helpers.MakeSomeIdent("p"))
                            )),
                            Modifiers.Immutable
                        ),
                        Statement.MakeVarDecl(
                            Helpers.MakeSeq(AstNode.MakeIdentifier("s", Helpers.MakePrimitiveType("int"))),
                            Helpers.MakeSeq(Expression.MakeBinaryExpr(OperatorType.BitwiseShiftLeft,
                                Expression.MakePath(Helpers.MakeSomeIdent("x")),
                                Expression.MakeConstant("int", 2)
                            )),
                            Modifiers.Immutable
                        ),
                        Statement.MakeVarDecl(
                            Helpers.MakeSeq(AstNode.MakeIdentifier("t", Helpers.MakePrimitiveType("int"))),
                            Helpers.MakeSeq(Expression.MakeBinaryExpr(OperatorType.BitwiseAnd,
                                Expression.MakePath(Helpers.MakeSomeIdent("r")),
                                Expression.MakePath(Helpers.MakeSomeIdent("s"))
                            )),
                            Modifiers.Immutable
                        ),
                        Statement.MakeVarDecl(
                            Helpers.MakeSeq(AstNode.MakeIdentifier("v", Helpers.MakePrimitiveType("int"))),
                            Helpers.MakeSeq(Expression.MakeBinaryExpr(OperatorType.BitwiseOr,
                                Expression.MakePath(Helpers.MakeSomeIdent("x")),
                                Expression.MakePath(Helpers.MakeSomeIdent("t"))
                            )),
                            Modifiers.Immutable
                        ),
                        Statement.MakeVarDecl(
                            Helpers.MakeSeq(AstNode.MakeIdentifier("w", Helpers.MakePrimitiveType("int"))),
                            Helpers.MakeSeq(Expression.MakeBinaryExpr(OperatorType.Plus,
                                Expression.MakePath(Helpers.MakeSomeIdent("r")),
                                Expression.MakePath(Helpers.MakeSomeIdent("s"))
                            )),
                            Modifiers.Immutable
                        ),
                        Statement.MakeVarDecl(
                            Helpers.MakeSeq(AstNode.MakeIdentifier("y", Helpers.MakePrimitiveType("int"))),
                            Helpers.MakeSeq(Expression.MakeBinaryExpr(OperatorType.Plus,
                                Expression.MakeIndexer(Expression.MakePath(Helpers.MakeSomeIdent("ary")),
                                    Expression.MakeConstant("int", 0)
                                ),
                                Expression.MakeBinaryExpr(OperatorType.Plus,
                                    Expression.MakeIndexer(Expression.MakePath(Helpers.MakeSomeIdent("ary")),
                                        Expression.MakeConstant("int", 1)
                                    ),
                                    Expression.MakeIndexer(Expression.MakePath(Helpers.MakeSeq(Helpers.MakeSomeIdent("d"))),
                                        Helpers.MakeSeq(Expression.MakeConstant("string", "a"))
                                    )
                                ))
                            ),
                            Modifiers.Immutable
                        ),
                        Statement.MakeVarDecl(
                            Helpers.MakeSeq(AstNode.MakeIdentifier("z", Helpers.MakePrimitiveType("int"))),
                            Helpers.MakeSeq(Expression.MakeBinaryExpr(OperatorType.Times,
                                Expression.MakePath(Helpers.MakeSomeIdent("v")),
                                Expression.MakePath(Helpers.MakeSomeIdent("w"))
                            )),
                            Modifiers.Immutable
                        )
                    }),
                    AstType.Null,
                    Modifiers.None
                )
            });

            Assert.IsNotNull(ast);
            Helpers.AstStructuralEqual(ast, expected_ast);
        }
    }
}

