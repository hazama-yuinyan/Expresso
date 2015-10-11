using System;
using System.IO;
using System.Collections.Generic;
using System.Numerics;
using System.Reflection;
using System.Linq;

using NUnit.Framework;

using Expresso.Ast;
using ICSharpCode.NRefactory;
using Expresso.Runtime.Builtins;

namespace Expresso.Test
{
	[TestFixture]
	public class ParserTests
	{
		[Test]
		public void SimpleLiterals()
		{
			var parser = new Parser(new Scanner("../../sources/for_unit_tests/simple_literals.exs"));
			parser.Parse();

            var ast = parser.TopmostAst;

            var expected_ast = AstNode.MakeModuleDef("main", new List<EntityDeclaration>{
                EntityDeclaration.MakeFunc("main",
                    Enumerable.Empty<ParameterDeclaration>(),
                    Statement.MakeBlock(new List<Statement>{
                        Statement.MakeVarDecl(
                            Helpers.MakeSeq(Helpers.MakeSomeIdent("a")),
                            Helpers.MakeSeq(Expression.MakeConstant("int", 255)),
                            Modifiers.Immutable
                        ),
                        Statement.MakeVarDecl(
                            Helpers.MakeSeq(Helpers.MakeSomeIdent("h_a")),
                            Helpers.MakeSeq(Expression.MakeConstant("int", 0xff)),
                            Modifiers.Immutable
                        ),
                        Statement.MakeVarDecl(
                            Helpers.MakeSeq(AstNode.MakeIdentifier("h_a_", Helpers.MakePrimitiveType("int"))),
                            Helpers.MakeSeq(Expression.MakeConstant("int", 0xff)),
                            Modifiers.Immutable
                        ),
                        Statement.MakeVarDecl(
                            Helpers.MakeSeq(Helpers.MakeSomeIdent("b")),
                            Helpers.MakeSeq(Expression.MakeConstant("double", 1000.0)),
                            Modifiers.Immutable
                        ),
                        Statement.MakeVarDecl(
                            Helpers.MakeSeq(Helpers.MakeSomeIdent("f_b")),
                            Helpers.MakeSeq(Expression.MakeConstant("double", 1.0e4)),
                            Modifiers.Immutable
                        ),
                        Statement.MakeVarDecl(
                            Helpers.MakeSeq(AstNode.MakeIdentifier("f_b_", Helpers.MakePrimitiveType("double"))),
                            Helpers.MakeSeq(Expression.MakeConstant("double", 1.0e4)),
                            Modifiers.Immutable
                        ),
                        Statement.MakeVarDecl(
                            Helpers.MakeSeq(Helpers.MakeSomeIdent("c")),
                            Helpers.MakeSeq(Expression.MakeConstant("double", 0.001)),
                            Modifiers.Immutable
                        ),
                        Statement.MakeVarDecl(
                            Helpers.MakeSeq(Helpers.MakeSomeIdent("f_c")),
                            Helpers.MakeSeq(Expression.MakeConstant("double", .1e-2)),
                            Modifiers.Immutable
                        ),
                        Statement.MakeVarDecl(
                            Helpers.MakeSeq(Helpers.MakeSomeIdent("f_c2")),
                            Helpers.MakeSeq(Expression.MakeConstant("double", .0001)),
                            Modifiers.Immutable
                        ),
                        Statement.MakeVarDecl(
                            Helpers.MakeSeq(Helpers.MakeSomeIdent("d")),
                            Helpers.MakeSeq(Expression.MakeConstant("bigint", BigInteger.Parse("10000000"))),
                            Modifiers.Immutable
                        ),
                        Statement.MakeVarDecl(
                            Helpers.MakeSeq(AstNode.MakeIdentifier("d_", Helpers.MakePrimitiveType("bigint"))),
                            Helpers.MakeSeq(Expression.MakeConstant("bigint", BigInteger.Parse("10000000"))),
                            Modifiers.Immutable
                        ),
                        Statement.MakeVarDecl(
                            Helpers.MakeSeq(Helpers.MakeSomeIdent("d2")),
                            Helpers.MakeSeq(Expression.MakeConstant("bigint", BigInteger.Parse("10000000"))),
                            Modifiers.Immutable
                        ),
                        Statement.MakeVarDecl(
                            Helpers.MakeSeq(Helpers.MakeSomeIdent("e")),
                            Helpers.MakeSeq(Expression.MakeConstant("string", "This is a test")),
                            Modifiers.Immutable
                        ),
                        Statement.MakeVarDecl(
                            Helpers.MakeSeq(Helpers.MakeSomeIdent("u")),
                            Helpers.MakeSeq(Expression.MakeConstant("uint", 1000u)),
                            Modifiers.Immutable
                        ),
                        Statement.MakeVarDecl(
                            Helpers.MakeSeq(Helpers.MakeSomeIdent("u_")),
                            Helpers.MakeSeq(Expression.MakeConstant("uint", 1000U)),
                            Modifiers.Immutable
                        ),
                        Statement.MakeVarDecl(
                            Helpers.MakeSeq(Helpers.MakeSomeIdent("f_a")),
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
                            Helpers.MakeSeq(Helpers.MakeSomeIdent("f_")),
                            Helpers.MakeSeq(Expression.MakeSequenceInitializer(Helpers.MakeGenericType("array", Helpers.MakePlaceholderType()),
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
                            Helpers.MakeSeq(Helpers.MakeSomeIdent("f2_")),
                            Helpers.MakeSeq(Expression.MakeSequenceInitializer(Helpers.MakeGenericType("vector", Helpers.MakePlaceholderType()),
                                Helpers.MakeSeq(
                                    Expression.MakeConstant("int", 1),
                                    Expression.MakeConstant("int", 2),
                                    Expression.MakeConstant("int", 3)
                                ))
                            ),
                            Modifiers.Immutable
                        ),
                        Statement.MakeVarDecl(
                            Helpers.MakeSeq(Helpers.MakeSomeIdent("f3")),
                            Helpers.MakeSeq(Expression.MakeParen(Expression.MakeSequence(null))),
                            Modifiers.Immutable
                        ),
                        Statement.MakeVarDecl(
                            Helpers.MakeSeq(Helpers.MakeSomeIdent("f3_")),
                            Helpers.MakeSeq(Expression.MakeParen(
                                Expression.MakeSequence(
                                    Expression.MakeConstant("int", 1),
                                    Expression.MakeConstant("string", "abc"),
                                    Expression.MakeConstant("bool", true)
                                )
                            )),
                            Modifiers.Immutable
                        ),
                        Statement.MakeVarDecl(
                            Helpers.MakeSeq(AstNode.MakeIdentifier("g",
                                Helpers.MakeGenericType("dictionary", Helpers.MakePrimitiveType("string"), Helpers.MakePrimitiveType("int")))
                            ),
                            Helpers.MakeSeq(Expression.MakeSequenceInitializer(
                                Helpers.MakeGenericType("dictionary", Helpers.MakePlaceholderType(), Helpers.MakePlaceholderType()),
                                Enumerable.Empty<Expression>()
                            )),
                            Modifiers.Immutable
                        ),
                        Statement.MakeVarDecl(
                            Helpers.MakeSeq(Helpers.MakeSomeIdent("g_")),
                            Helpers.MakeSeq(Expression.MakeSequenceInitializer(Helpers.MakeGenericType("dictionary", Helpers.MakePlaceholderType(), Helpers.MakePlaceholderType()),
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
                            Helpers.MakeSeq(Helpers.MakeSomeIdent("h")),
                            Helpers.MakeSeq(Expression.MakeConstant("string", "私変わっちゃうの・・？")),
                            Modifiers.None
                        ),
                        Statement.MakeVarDecl(
                            Helpers.MakeSeq(Helpers.MakeSomeIdent("h2")),
                            Helpers.MakeSeq(Expression.MakeConstant("string", "Yes, you can!")),
                            Modifiers.None
                        ),
                        Statement.MakeVarDecl(
                            Helpers.MakeSeq(Helpers.MakeSomeIdent("h3")),
                            Helpers.MakeSeq(Expression.MakeConstant("char", 'a')),
                            Modifiers.None
                        ),
                        Statement.MakeVarDecl(
                            Helpers.MakeSeq(Helpers.MakeSomeIdent("i")),
                            Helpers.MakeSeq(Expression.MakeConstant("string", "よかった。私変わらないんだね！・・え、変われないの？・・・なんだかフクザツ")),
                            Modifiers.Immutable
                        ),
                        Statement.MakeVarDecl(
                            Helpers.MakeSeq(Helpers.MakeSomeIdent("i2")),
                            Helpers.MakeSeq(Expression.MakeConstant("string", "Oh, you just can't...")),
                            Modifiers.Immutable
                        ),
                        Statement.MakeVarDecl(
                            Helpers.MakeSeq(Helpers.MakeSomeIdent("i3")),
                            Helpers.MakeSeq(Expression.MakeConstant("char", 'a')),
                            Modifiers.Immutable
                        ),
                        Statement.MakeVarDecl(
                            Helpers.MakeSeq(Helpers.MakeSomeIdent("i4")),
                            Helpers.MakeSeq(Expression.MakeConstant("string", "\u0041\u005a\u0061\u007A\u3042\u30A2")), //AZazあア
                            Modifiers.Immutable
                        ),
                        Statement.MakeVarDecl(
                            Helpers.MakeSeq(Helpers.MakeSomeIdent("i5")),
                            Helpers.MakeSeq(Expression.MakeConstant("char", '\u0041')), //A
                            Modifiers.Immutable
                        ),
                        Statement.MakeVarDecl(
                            Helpers.MakeSeq(Helpers.MakeSomeIdent("i6")),
                            Helpers.MakeSeq(Expression.MakeConstant("string", "This is a normal string.\n Seems 2 lines? Yes, you're right!")),
                            Modifiers.Immutable
                        ),
                        Statement.MakeVarDecl(
                            Helpers.MakeSeq(Helpers.MakeSomeIdent("i6_")),
                            Helpers.MakeSeq(Expression.MakeConstant("string", @"This is a raw string.\n Seems 2 lines? Nah, indeed.")),
                            Modifiers.Immutable
                        ),
                        Statement.MakeVarDecl(
                            Helpers.MakeSeq(Helpers.MakeSomeIdent("j")),
                            Helpers.MakeSeq(Expression.MakeIntSeq(Expression.MakeConstant("int", 1), Expression.MakeConstant("int", 10), Expression.MakeConstant("int", 1), false)),
                            Modifiers.Immutable
                        ),
                        Statement.MakeVarDecl(
                            Helpers.MakeSeq(Helpers.MakeSomeIdent("j2")),
                            Helpers.MakeSeq(Expression.MakeIntSeq(Expression.MakeConstant("int", 1), Expression.MakeConstant("int", 10), Expression.MakeConstant("int", 1), false)),
                            Modifiers.Immutable
                        ),
                        Statement.MakeVarDecl(
                            Helpers.MakeSeq(AstNode.MakeIdentifier("j3", Helpers.MakePrimitiveType("intseq"))),
                            Helpers.MakeSeq(Expression.MakeIntSeq(Expression.MakeConstant("int", 1), Expression.MakeConstant("int", 10), Expression.MakeConstant("int", 1), false)),
                            Modifiers.Immutable
                        ),
                        Statement.MakeVarDecl(
                            Helpers.MakeSeq(Helpers.MakeSomeIdent("j_2")),
                            Helpers.MakeSeq(Expression.MakeIntSeq(Expression.MakeUnaryExpr(OperatorType.Minus, Expression.MakeConstant("int", 5)), Expression.MakeUnaryExpr(OperatorType.Minus, Expression.MakeConstant("int", 10)), Expression.MakeUnaryExpr(OperatorType.Minus, Expression.MakeConstant("int", 1)), false)),
                            Modifiers.Immutable
                        ),
                        Statement.MakeVarDecl(
                            Helpers.MakeSeq(Helpers.MakeSomeIdent("j_2_")),
                            Helpers.MakeSeq(Expression.MakeIntSeq(Expression.MakeConstant("int", 0), Expression.MakeUnaryExpr(OperatorType.Minus, Expression.MakeConstant("int", 10)), Expression.MakeUnaryExpr(OperatorType.Minus, Expression.MakeConstant("int", 1)), true)),
                            Modifiers.Immutable
                        )
                    }),
                    Helpers.MakePlaceholderType(),
                    Modifiers.None
                )
            });

            Assert.IsNotNull(ast);
            var debug_output = new DebugOutputWalker(Console.Out);
            ast.AcceptWalker(debug_output);
            Helpers.AstStructuralEqual(ast, expected_ast);
		}

        [Test]
        public void GeneralExpressions()
        {
            var parser = new Parser(new Scanner("../../sources/for_unit_tests/general_expressions.exs"));
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
                            AstNode.MakeIdentifier("z", Helpers.MakePlaceholderType())
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
                                Statement.MakeReturnStmt(Expression.MakeSequence(Expression.MakeMemRef(
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
                        EntityDeclaration.MakeParameter("y", Helpers.MakePlaceholderType(), Expression.MakeConstant("int", 2)),
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
                            Helpers.MakeSeq(Helpers.MakeSomeIdent("ary")),
                            Helpers.MakeSeq(
                                Expression.MakeSequenceInitializer(
                                    Helpers.MakeGenericType("array", Helpers.MakePlaceholderType()),
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
                            Helpers.MakeSeq(Helpers.MakeSomeIdent("d")),
                            Helpers.MakeSeq(
                                Expression.MakeSequenceInitializer(
                                    Helpers.MakeGenericType("dictionary", Helpers.MakePlaceholderType(), Helpers.MakePlaceholderType()),
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
                            Helpers.MakeSeq(Helpers.MakeSomeIdent("k")),
                            Helpers.MakeSeq(Expression.MakeObjectCreation(
                                Helpers.MakeGenericType("Test"),
                                Helpers.MakeSeq(AstNode.MakeIdentifier("x"), AstNode.MakeIdentifier("y")),
                                Helpers.MakeSeq(Expression.MakeConstant("int", 1), Expression.MakeConstant("int", 2))
                            )),
                            Modifiers.Immutable
                        ),
                        Statement.MakeVarDecl(
                            Helpers.MakeSeq(Helpers.MakeSomeIdent("k2")),
                            Helpers.MakeSeq(Expression.MakeNewExpr(Expression.MakeObjectCreation(
                                Helpers.MakeGenericType("Test"),
                                Helpers.MakeSeq(AstNode.MakeIdentifier("x"), AstNode.MakeIdentifier("y")),
                                Helpers.MakeSeq(Expression.MakeConstant("int", 1), Expression.MakeConstant("int", 2))
                            ))),
                            Modifiers.Immutable
                        ),
                        Statement.MakeVarDecl(
                            Helpers.MakeSeq(Helpers.MakeSomeIdent("l")),
                            Helpers.MakeSeq(Expression.MakeCallExpr(
                                Expression.MakePath(Helpers.MakeSomeIdent("createTest")),
                                Expression.MakeConstant("int", 1),
                                Expression.MakeConstant("int", 2),
                                Expression.MakeConstant("int", 4)
                            )),
                            Modifiers.Immutable
                        ),
                        Statement.MakeVarDecl(
                            Helpers.MakeSeq(Helpers.MakeSomeIdent("m")),
                            Helpers.MakeSeq(Expression.MakeIndexer(Expression.MakePath(Helpers.MakeSomeIdent("ary")),
                                Expression.MakeConstant("int", 0)
                            )),
                            Modifiers.Immutable
                        ),
                        Statement.MakeVarDecl(
                            Helpers.MakeSeq(Helpers.MakeSomeIdent("m2")),
                            Helpers.MakeSeq(Expression.MakeIndexer(Expression.MakePath(Helpers.MakeSomeIdent("d")),
                                Expression.MakeConstant("string", "a")
                            )),
                            Modifiers.Immutable
                        ),
                        Statement.MakeVarDecl(
                            Helpers.MakeSeq(Helpers.MakeSomeIdent("n")),
                            Helpers.MakeSeq(Expression.MakeMemRef(
                                Expression.MakePath(Helpers.MakeSomeIdent("k")),
                                AstNode.MakeIdentifier("x"))
                            ),
                            Modifiers.Immutable
                        ),
                        Statement.MakeVarDecl(
                            Helpers.MakeSeq(Helpers.MakeSomeIdent("n2")),
                            Helpers.MakeSeq(
                                Expression.MakeCallExpr(Expression.MakeMemRef(
                                    Expression.MakePath(Helpers.MakeSeq(Helpers.MakeSomeIdent("k"))),
                                    AstNode.MakeIdentifier("getY")
                                ), Enumerable.Empty<Expression>())
                            ),
                            Modifiers.Immutable
                        ),
                        Statement.MakeVarDecl(
                            Helpers.MakeSeq(Helpers.MakeSomeIdent("x")),
                            Helpers.MakeSeq(Expression.MakeConstant("int", 100)),
                            Modifiers.Immutable
                        ),
                        Statement.MakeVarDecl(
                            Helpers.MakeSeq(Helpers.MakeSomeIdent("p")),
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
                            Helpers.MakeSeq(Helpers.MakeSomeIdent("q")),
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
                            Helpers.MakeSeq(Helpers.MakeSomeIdent("r")),
                            Helpers.MakeSeq(Expression.MakeBinaryExpr(OperatorType.BitwiseShiftRight,
                                Expression.MakePath(Helpers.MakeSomeIdent("x")),
                                Expression.MakePath(Helpers.MakeSomeIdent("p"))
                            )),
                            Modifiers.Immutable
                        ),
                        Statement.MakeVarDecl(
                            Helpers.MakeSeq(Helpers.MakeSomeIdent("s")),
                            Helpers.MakeSeq(Expression.MakeBinaryExpr(OperatorType.BitwiseShiftLeft,
                                Expression.MakePath(Helpers.MakeSomeIdent("x")),
                                Expression.MakeConstant("int", 2)
                            )),
                            Modifiers.Immutable
                        ),
                        Statement.MakeVarDecl(
                            Helpers.MakeSeq(Helpers.MakeSomeIdent("t")),
                            Helpers.MakeSeq(Expression.MakeBinaryExpr(OperatorType.BitwiseAnd,
                                Expression.MakePath(Helpers.MakeSomeIdent("r")),
                                Expression.MakePath(Helpers.MakeSomeIdent("s"))
                            )),
                            Modifiers.Immutable
                        ),
                        Statement.MakeVarDecl(
                            Helpers.MakeSeq(Helpers.MakeSomeIdent("v")),
                            Helpers.MakeSeq(Expression.MakeBinaryExpr(OperatorType.BitwiseOr,
                                Expression.MakePath(Helpers.MakeSomeIdent("x")),
                                Expression.MakePath(Helpers.MakeSomeIdent("t"))
                            )),
                            Modifiers.Immutable
                        ),
                        Statement.MakeVarDecl(
                            Helpers.MakeSeq(Helpers.MakeSomeIdent("w")),
                            Helpers.MakeSeq(Expression.MakeBinaryExpr(OperatorType.Plus,
                                Expression.MakePath(Helpers.MakeSomeIdent("r")),
                                Expression.MakePath(Helpers.MakeSomeIdent("s"))
                            )),
                            Modifiers.Immutable
                        ),
                        Statement.MakeVarDecl(
                            Helpers.MakeSeq(Helpers.MakeSomeIdent("y")),
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
                            Helpers.MakeSeq(Helpers.MakeSomeIdent("z")),
                            Helpers.MakeSeq(Expression.MakeBinaryExpr(OperatorType.Times,
                                Expression.MakePath(Helpers.MakeSomeIdent("v")),
                                Expression.MakePath(Helpers.MakeSomeIdent("w"))
                            )),
                            Modifiers.Immutable
                        )
                    }),
                    Helpers.MakePlaceholderType(),
                    Modifiers.None
                )
            });

            Assert.IsNotNull(ast);
            Helpers.AstStructuralEqual(ast, expected_ast);
        }

        [Test]
        public void Statements()
        {
            var parser = new Parser(new Scanner("../../sources/for_unit_tests/statements.exs"));
            parser.Parse();

            var ast = parser.TopmostAst;

            var expected_ast = AstNode.MakeModuleDef("main", new List<EntityDeclaration>{
                EntityDeclaration.MakeFunc("main",
                    Enumerable.Empty<ParameterDeclaration>(),
                    Statement.MakeBlock(
                        Statement.MakeVarDecl(
                            Helpers.MakeSeq(
                                Helpers.MakeSomeIdent("x"),
                                Helpers.MakeSomeIdent("y"),
                                Helpers.MakeSomeIdent("z"),
                                Helpers.MakeSomeIdent("w")
                            ),
                            Helpers.MakeSeq(
                                Expression.MakeConstant("int", 100),
                                Expression.MakeConstant("int", 50),
                                Expression.MakeConstant("int", 300),
                                Expression.MakeConstant("int", 400)
                            ),
                            Modifiers.Immutable
                        ),
                        Statement.MakeVarDecl(
                            Helpers.MakeSeq(AstNode.MakeIdentifier("flag", Helpers.MakePrimitiveType("bool"))),
                            Helpers.MakeSeq(Expression.Null),
                            Modifiers.None
                        ),
                        Statement.MakeExprStmt(
                            Expression.MakeSequence(
                                Expression.MakeCallExpr(
                                    Expression.MakePath(Helpers.MakeSomeIdent("print")),
                                    Expression.MakeConstant("string", "(x, y, z, w) = ({}, {}, {}, {})"),
                                    Expression.MakePath(Helpers.MakeSomeIdent("x")),
                                    Expression.MakePath(Helpers.MakeSomeIdent("y")),
                                    Expression.MakePath(Helpers.MakeSomeIdent("z")),
                                    Expression.MakePath(Helpers.MakeSomeIdent("w"))
                                )
                            )
                        ),
                        Statement.MakeIfStmt(
                            PatternConstruct.MakeExpressionPattern(
                                Expression.MakeBinaryExpr(OperatorType.Equality,
                                    Expression.MakePath(Helpers.MakeSomeIdent("x")),
                                    Expression.MakeConstant("int", 100)
                                )
                            ),
                            Statement.MakeBlock(
                                Helpers.MakeSeq(
                                    Statement.MakeExprStmt(Helpers.MakeAssignment(
                                        Helpers.MakeSeq(Expression.MakePath(Helpers.MakeSomeIdent("flag"))),
                                        Helpers.MakeSeq(Expression.MakeConstant("bool", true))
                                    ))
                                )
                            ),
                            Statement.MakeBlock(
                                Helpers.MakeSeq(
                                    Statement.MakeExprStmt(Helpers.MakeAssignment(
                                        Helpers.MakeSeq(Expression.MakePath(Helpers.MakeSomeIdent("flag"))),
                                        Helpers.MakeSeq(Expression.MakeConstant("bool", false))
                                    ))
                                )
                            )
                        ),
                        Statement.MakeVarDecl(
                            Helpers.MakeSeq(Helpers.MakeSomeIdent("sum")),
                            Helpers.MakeSeq(Expression.MakeConstant("int", 0)),
                            Modifiers.None
                        ),
                        Statement.MakeValueBindingForStmt(
                            Modifiers.Immutable,
                            Statement.MakeBlock(
                                Helpers.MakeSeq(
                                    Helpers.MakeAugmentedAssignment(OperatorType.Plus,
                                        Helpers.MakeSeq(Expression.MakePath(Helpers.MakeSomeIdent("sum"))),
                                        Helpers.MakeSeq(Expression.MakePath(Helpers.MakeSomeIdent("p")))
                                    ),
                                    Statement.MakeExprStmt(
                                        Expression.MakeSequence(
                                            Expression.MakeCallExpr(
                                                Expression.MakePath(Helpers.MakeSomeIdent("println")),
                                                Expression.MakeConstant("string", "{}, {}"),
                                                Expression.MakePath(Helpers.MakeSomeIdent("p")),
                                                Expression.MakePath(Helpers.MakeSomeIdent("sum"))
                                            )
                                        )
                                    )
                                )
                            ),
                            AstNode.MakeVariableInitializer(
                                Helpers.MakeSomeIdent("p"),
                                Expression.MakeIntSeq(
                                    Expression.MakeConstant("int", 0),
                                    Expression.MakePath(Helpers.MakeSomeIdent("y")),
                                    Expression.MakeConstant("int", 1),
                                    false
                                )
                            )
                        ),
                        Statement.MakeVarDecl(
                            Helpers.MakeSeq(Helpers.MakeSomeIdent("strs")),
                            Helpers.MakeSeq(Expression.MakeSequenceInitializer(
                                Helpers.MakeGenericType("array", Helpers.MakePlaceholderType()),
                                Expression.MakeConstant("string", "akarichan"),
                                Expression.MakeConstant("string", "chinatsu"),
                                Expression.MakeConstant("string", "kyoko"),
                                Expression.MakeConstant("string", "yui")
                            )),
                            Modifiers.Immutable
                        ),
                        Statement.MakeValueBindingForStmt(
                            Modifiers.Immutable,
                            Statement.MakeBlock(
                                Statement.MakeMatchStmt(Expression.MakePath(Helpers.MakeSomeIdent("tmp")),
                                    Statement.MakeMatchClause(null,
                                        Statement.MakeExprStmt(Expression.MakeSequence(
                                            Expression.MakeCallExpr(
                                                Expression.MakePath(Helpers.MakeSomeIdent("print")),
                                                Expression.MakeConstant("string", "kawakawa")
                                            ))
                                        ),
                                        PatternConstruct.MakeExpressionPattern(Expression.MakeConstant("string", "akarichan"))
                                    ),
                                    Statement.MakeMatchClause(null,
                                        Statement.MakeExprStmt(Expression.MakeSequence(
                                            Expression.MakeCallExpr(
                                                Expression.MakePath(Helpers.MakeSomeIdent("print")),
                                                Expression.MakeConstant("string", "ankokuthunder!")
                                            ))
                                        ),
                                        PatternConstruct.MakeExpressionPattern(Expression.MakeConstant("string", "chinatsu"))
                                    ),
                                    Statement.MakeMatchClause(null,
                                        Statement.MakeExprStmt(Expression.MakeSequence(
                                            Expression.MakeCallExpr(
                                                Expression.MakePath(Helpers.MakeSomeIdent("print")),
                                                Expression.MakeConstant("string", "gaichiban!")
                                            ))
                                        ),
                                        PatternConstruct.MakeExpressionPattern(Expression.MakeConstant("string", "kyoko"))
                                    ),
                                    Statement.MakeMatchClause(null,
                                        Statement.MakeExprStmt(Expression.MakeSequence(
                                            Expression.MakeCallExpr(
                                                Expression.MakePath(Helpers.MakeSomeIdent("print")),
                                                Expression.MakeConstant("string", "doyaxtu!")
                                            ))
                                        ),
                                        PatternConstruct.MakeExpressionPattern(Expression.MakeConstant("string", "yui"))
                                    )
                                )
                            ),
                            AstNode.MakeVariableInitializer(
                                Helpers.MakeSomeIdent("tmp"),
                                Expression.MakePath(Helpers.MakeSomeIdent("strs"))
                            )
                        ),
                        Statement.MakeVarDecl(
                            Helpers.MakeSeq(Helpers.MakeSomeIdent("fibs"), Helpers.MakeSomeIdent("a"), Helpers.MakeSomeIdent("b")),
                            Helpers.MakeSeq<Expression>(
                                Expression.MakeSequenceInitializer(
                                    Helpers.MakeGenericType("vector", Helpers.MakePlaceholderType())
                                ),
                                Expression.MakeConstant("int", 0),
                                Expression.MakeConstant("int", 1)
                            ),
                            Modifiers.None
                        ),
                        Statement.MakeWhileStmt(
                            Expression.MakeBinaryExpr(OperatorType.LessThan,
                                Expression.MakePath(Helpers.MakeSomeIdent("b")),
                                Expression.MakeConstant("int", 1000)
                            ),
                            Statement.MakeBlock(
                                Statement.MakeExprStmt(
                                    Expression.MakeSequence(
                                        Expression.MakeCallExpr(
                                            Expression.MakeMemRef(
                                                Expression.MakePath(Helpers.MakeSomeIdent("fibs")),
                                                AstNode.MakeIdentifier("add")
                                            ),
                                            Expression.MakePath(Helpers.MakeSomeIdent("b"))
                                        )
                                    )
                                ),
                                Statement.MakeExprStmt(
                                    Helpers.MakeAssignment(
                                        Helpers.MakeSeq(
                                            Expression.MakePath(Helpers.MakeSomeIdent("a")),
                                            Expression.MakePath(Helpers.MakeSomeIdent("b"))
                                        ),
                                        Helpers.MakeSeq<Expression>(
                                            Expression.MakePath(Helpers.MakeSomeIdent("b")),
                                            Expression.MakeBinaryExpr(OperatorType.Plus,
                                                Expression.MakePath(Helpers.MakeSomeIdent("a")),
                                                Expression.MakePath(Helpers.MakeSomeIdent("b"))
                                            )
                                        )
                                    )
                                )
                            )
                        ),
                        Statement.MakeVarDecl(
                            Helpers.MakeSeq(Helpers.MakeSomeIdent("vec")),
                            Helpers.MakeSeq(Expression.MakeSequenceInitializer(
                                Helpers.MakeGenericType("vector", Helpers.MakePlaceholderType())
                            )),
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
                                                Expression.MakeBinaryExpr(OperatorType.ConditionalAnd,
                                                    Expression.MakeBinaryExpr(OperatorType.Equality,
                                                        Expression.MakePath(Helpers.MakeSomeIdent("i")),
                                                        Expression.MakeConstant("int", 3)
                                                    ),
                                                    Expression.MakeBinaryExpr(OperatorType.Equality,
                                                        Expression.MakePath(Helpers.MakeSomeIdent("i")),
                                                        Expression.MakeConstant("int", 6)
                                                    )
                                                )
                                            ),
                                            Statement.MakeBlock(
                                                Statement.MakeBreakStmt(Expression.MakeConstant("int", 1))
                                            ),
                                            null
                                        ),
                                        Statement.MakeIfStmt(
                                            PatternConstruct.MakeExpressionPattern(
                                                Expression.MakeBinaryExpr(OperatorType.Equality,
                                                    Expression.MakePath(Helpers.MakeSomeIdent("j")),
                                                    Expression.MakeConstant("int", 8)
                                                )
                                            ),
                                            Statement.MakeBlock(
                                                Statement.MakeContinueStmt(Expression.MakeConstant("int", 2))
                                            ),
                                            null
                                        ),
                                        Statement.MakeExprStmt(Expression.MakeCallExpr(
                                            Expression.MakeMemRef(
                                                Expression.MakePath(Helpers.MakeSomeIdent("vec")),
                                                AstNode.MakeIdentifier("add")
                                            ),
                                            Expression.MakeParen(Expression.MakeSequence(
                                                Expression.MakePath(Helpers.MakeSomeIdent("i")),
                                                Expression.MakePath(Helpers.MakeSomeIdent("j"))
                                            ))
                                        ))
                                    ),
                                    AstNode.MakeVariableInitializer(
                                        Helpers.MakeSomeIdent("j"),
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
                                Helpers.MakeSomeIdent("i"),
                                Expression.MakeIntSeq(
                                    Expression.MakeConstant("int", 0),
                                    Expression.MakeConstant("int", 10),
                                    Expression.MakeConstant("int", 1),
                                    false
                                )
                            )
                        ),
                        Statement.MakeReturnStmt(Expression.MakeSequence(
                            Expression.MakeSequenceInitializer(Helpers.MakeGenericType("array", Helpers.MakePlaceholderType()),
                                Expression.MakePath(Helpers.MakeSomeIdent("x")),
                                Expression.MakePath(Helpers.MakeSomeIdent("y")),
                                Expression.MakePath(Helpers.MakeSomeIdent("z")),
                                Expression.MakePath(Helpers.MakeSomeIdent("w")),
                                Expression.MakePath(Helpers.MakeSomeIdent("flag")),
                                Expression.MakePath(Helpers.MakeSomeIdent("sum")),
                                Expression.MakePath(Helpers.MakeSomeIdent("strs")),
                                Expression.MakePath(Helpers.MakeSomeIdent("fibs")),
                                Expression.MakePath(Helpers.MakeSomeIdent("vec"))
                            )
                        ))
                    ),
                    Helpers.MakePlaceholderType(),
                    Modifiers.None
                )
            });

            Assert.IsNotNull(ast);
            Helpers.AstStructuralEqual(ast, expected_ast);
        }
	}
}
