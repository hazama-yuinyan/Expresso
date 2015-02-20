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
			var parser = new Parser(new Scanner("../../sources/for_parser/simple_literals.exs"));
			parser.ParsingFileName = "simple_literals";
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
                            Helpers.MakeSeq(AstNode.MakeIdentifier("h_a_", new PrimitiveType("int", TextLocation.Empty))),
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
                            Helpers.MakeSeq(AstNode.MakeIdentifier("f_b_", new PrimitiveType("double", TextLocation.Empty))),
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
                            Helpers.MakeSeq(Helpers.MakeSomeIdent("d")),
                            Helpers.MakeSeq(Expression.MakeConstant("bigint", "10000000")),
                            Modifiers.Immutable
                        ),
                        Statement.MakeVarDecl(
                            Helpers.MakeSeq(AstNode.MakeIdentifier("d_", new PrimitiveType("bigint", TextLocation.Empty))),
                            Helpers.MakeSeq(Expression.MakeConstant("bigint", "10000000")),
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
                            Helpers.MakeSeq(AstNode.MakeIdentifier("f_a_", new PrimitiveType("float", TextLocation.Empty))),
                            Helpers.MakeSeq(Expression.MakeConstant("float", 1.0e4f)),
                            Modifiers.Immutable
                        ),
                        Statement.MakeVarDecl(
                            Helpers.MakeSeq(Helpers.MakeSomeIdent("f")),
                            Helpers.MakeSeq(Expression.MakeSeqInitializer(new PrimitiveType("array", TextLocation.Empty),
                                Enumerable.Empty<Expression>()
                            )),
                            Modifiers.Immutable
                        ),
                        Statement.MakeVarDecl(
                            Helpers.MakeSeq(Helpers.MakeSomeIdent("f_")),
                            Helpers.MakeSeq(Expression.MakeSeqInitializer(new PrimitiveType("array", TextLocation.Empty),
                                Helpers.MakeSeq(
                                    Expression.MakeConstant("int", 1),
                                    Expression.MakeConstant("int", 2),
                                    Expression.MakeConstant("int", 3)
                                )
                            )),
                            Modifiers.Immutable
                        ),
                        Statement.MakeVarDecl(
                            Helpers.MakeSeq(Helpers.MakeSomeIdent("f2")),
                            Helpers.MakeSeq(Expression.MakeSeqInitializer(new PrimitiveType("vector", TextLocation.Empty),
                                Enumerable.Empty<Expression>()
                            )),
                            Modifiers.Immutable
                        ),
                        Statement.MakeVarDecl(
                            Helpers.MakeSeq(Helpers.MakeSomeIdent("f2_")),
                            Helpers.MakeSeq(Expression.MakeSeqInitializer(new PrimitiveType("vector", TextLocation.Empty),
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
                            Helpers.MakeSeq(Expression.MakeSeqInitializer(new PrimitiveType("tuple", TextLocation.Empty), Enumerable.Empty<Expression>())),
                            Modifiers.Immutable
                        ),
                        Statement.MakeVarDecl(
                            Helpers.MakeSeq(Helpers.MakeSomeIdent("f3_")),
                            Helpers.MakeSeq(Expression.MakeSeqInitializer(new PrimitiveType("tuple", TextLocation.Empty),
                                Helpers.MakeSeq(
                                    Expression.MakeConstant("int", 1),
                                    Expression.MakeConstant("string", "abc")
                                )
                            )),
                            Modifiers.Immutable
                        ),
                        Statement.MakeVarDecl(
                            Helpers.MakeSeq(Helpers.MakeSomeIdent("g")),
                            Helpers.MakeSeq(Expression.MakeSeqInitializer(new PrimitiveType("dictionary", TextLocation.Empty), Enumerable.Empty<Expression>())),
                            Modifiers.Immutable
                        ),
                        Statement.MakeVarDecl(
                            Helpers.MakeSeq(Helpers.MakeSomeIdent("g_")),
                            Helpers.MakeSeq(Expression.MakeSeqInitializer(new PrimitiveType("dictionary", TextLocation.Empty),
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
                            Helpers.MakeSeq(AstNode.MakeIdentifier("j3", new PrimitiveType("intseq", TextLocation.Empty))),
                            Helpers.MakeSeq(Expression.MakeIntSeq(Expression.MakeConstant("int", 1), Expression.MakeConstant("int", 10), Expression.MakeConstant("int", 1), false)),
                            Modifiers.Immutable
                        ),
                        Statement.MakeVarDecl(
                            Helpers.MakeSeq(Helpers.MakeSomeIdent("j_2")),
                            Helpers.MakeSeq(Expression.MakeIntSeq(Expression.MakeConstant("int", -5), Expression.MakeConstant("int", -10), Expression.MakeConstant("int", -1), false)),
                            Modifiers.Immutable
                        ),
                        Statement.MakeVarDecl(
                            Helpers.MakeSeq(Helpers.MakeSomeIdent("j_2_")),
                            Helpers.MakeSeq(Expression.MakeIntSeq(Expression.MakeConstant("int", 0), Expression.MakeConstant("int", -10), Expression.MakeConstant("int", -1), true)),
                            Modifiers.Immutable
                        ),
                        Statement.MakeVarDecl(
                            Helpers.MakeSeq(Helpers.MakeSomeIdent("k")),
                            Helpers.MakeSeq(Expression.MakeObjectCreation(
                                Expression.MakePath(Helpers.MakeSeq(Helpers.MakeSomeIdent("Test"))),
                                Helpers.MakeSeq(Helpers.MakeSomeIdent("x"), Helpers.MakeSomeIdent("y")),
                                Helpers.MakeSeq(Expression.MakeConstant("int", 1), Expression.MakeConstant("int", 2))
                            )),
                            Modifiers.Immutable
                        ),
                        Statement.MakeVarDecl(
                            Helpers.MakeSeq(Helpers.MakeSomeIdent("k2")),
                            Helpers.MakeSeq(Expression.MakeNewExpr(Expression.MakeObjectCreation(
                                Expression.MakePath(Helpers.MakeSeq(Helpers.MakeSomeIdent("Test"))),
                                Helpers.MakeSeq(Helpers.MakeSomeIdent("x"), Helpers.MakeSomeIdent("y")),
                                Helpers.MakeSeq(Expression.MakeConstant("int", 1), Expression.MakeConstant("int", 2))
                            ))),
                            Modifiers.Immutable
                        ),
                        Statement.MakeVarDecl(
                            Helpers.MakeSeq(Helpers.MakeSomeIdent("l")),
                            Helpers.MakeSeq(Expression.MakeCallExpr(Expression.MakePath(Helpers.MakeSomeIdent("CreateTest")),
                                Expression.MakeConstant("int", 1), Expression.MakeConstant("int", 2)
                            )),
                            Modifiers.Immutable
                        ),
                        Statement.MakeVarDecl(
                            Helpers.MakeSeq(Helpers.MakeSomeIdent("m")),
                            Helpers.MakeSeq(Expression.MakeIndexer(Expression.MakePath(Helpers.MakeSomeIdent("f_")),
                                Expression.MakeConstant("int", 0)
                            )),
                            Modifiers.Immutable
                        ),
                        Statement.MakeVarDecl(
                            Helpers.MakeSeq(Helpers.MakeSomeIdent("m2")),
                            Helpers.MakeSeq(Expression.MakeIndexer(Expression.MakePath(Helpers.MakeSomeIdent("g_")),
                                Expression.MakeConstant("string", "akari")
                            )),
                            Modifiers.Immutable
                        ),
                        Statement.MakeVarDecl(
                            Helpers.MakeSeq(Helpers.MakeSomeIdent("n")),
                            Helpers.MakeSeq(Expression.MakeMemRef(
                                Expression.MakePath(Helpers.MakeSomeIdent("k")),
                                Helpers.MakeSomeIdent("x"))
                            ),
                            Modifiers.Immutable
                        ),
                        Statement.MakeVarDecl(
                            Helpers.MakeSeq(Helpers.MakeSomeIdent("n2")),
                            Helpers.MakeSeq(
                                Expression.MakeCallExpr(Expression.MakeMemRef(
                                    Expression.MakePath(Helpers.MakeSeq(Helpers.MakeSomeIdent("k"))),
                                    Helpers.MakeSomeIdent("getY")
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
                                Expression.MakeIndexer(Expression.MakePath(Helpers.MakeSomeIdent("f_")),
                                    Expression.MakeConstant("int", 0)
                                ),
                                Expression.MakeBinaryExpr(OperatorType.Plus,
                                    Expression.MakeIndexer(Expression.MakePath(Helpers.MakeSomeIdent("f_")),
                                        Expression.MakeConstant("int", 1)
                                    ),
                                    Expression.MakeIndexer(Expression.MakePath(Helpers.MakeSeq(Helpers.MakeSomeIdent("f_"))),
                                        Helpers.MakeSeq(Expression.MakeConstant("int", 2))
                                    )
                                ))
                            ),
                            Modifiers.Immutable
                        ),
                        Statement.MakeVarDecl(
                            Helpers.MakeSeq(Helpers.MakeSomeIdent("q")),
                            Helpers.MakeSeq(Expression.MakeBinaryExpr(OperatorType.Plus,
                                Expression.MakeIndexer(Expression.MakePath(Helpers.MakeSomeIdent("g_")),
                                    Expression.MakeConstant("string", "akari")
                                ),
                                Expression.MakeBinaryExpr(OperatorType.Plus,
                                    Expression.MakeIndexer(Expression.MakePath(Helpers.MakeSomeIdent("g_")),
                                        Expression.MakeConstant("string", "chinatsu")
                                    ),
                                    Expression.MakeIndexer(Expression.MakePath(Helpers.MakeSomeIdent("g_")),
                                        Expression.MakeConstant("string", "結衣")
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
                            Helpers.MakeSeq(Helpers.MakeSomeIdent("u")),
                            Helpers.MakeSeq(Expression.MakeBinaryExpr(OperatorType.BitwiseOr,
                                Expression.MakePath(Helpers.MakeSomeIdent("x")),
                                Expression.MakePath(Helpers.MakeSomeIdent("t"))
                            )),
                            Modifiers.Immutable
                        ),
                        Statement.MakeVarDecl(
                            Helpers.MakeSeq(Helpers.MakeSomeIdent("v")),
                            Helpers.MakeSeq(Expression.MakeBinaryExpr(OperatorType.Plus,
                                Expression.MakePath(Helpers.MakeSomeIdent("c")),
                                Expression.MakePath(Helpers.MakeSomeIdent("d"))
                            )),
                            Modifiers.Immutable
                        ),
                        Statement.MakeVarDecl(
                            Helpers.MakeSeq(Helpers.MakeSomeIdent("w")),
                            Helpers.MakeSeq(Expression.MakeBinaryExpr(OperatorType.Plus,
                                Expression.MakeIndexer(Expression.MakePath(Helpers.MakeSomeIdent("f_")),
                                    Expression.MakeConstant("int", 0)
                                ),
                                Expression.MakeBinaryExpr(OperatorType.Plus,
                                    Expression.MakeIndexer(Expression.MakePath(Helpers.MakeSomeIdent("f_")),
                                        Expression.MakeConstant("int", 1)
                                    ),
                                    Expression.MakeIndexer(Expression.MakePath(Helpers.MakeSeq(Helpers.MakeSomeIdent("g_"))),
                                        Helpers.MakeSeq(Expression.MakeConstant("string", "akari"))
                                    )
                                ))
                            ),
                            Modifiers.Immutable
                        ),
                        Statement.MakeVarDecl(
                            Helpers.MakeSeq(Helpers.MakeSomeIdent("y")),
                            Helpers.MakeSeq(Expression.MakeBinaryExpr(OperatorType.Times,
                                Expression.MakePath(Helpers.MakeSomeIdent("v")),
                                Expression.MakePath(Helpers.MakeSomeIdent("w"))
                            )),
                            Modifiers.Immutable
                        )
                    }, TextLocation.Empty, TextLocation.Empty),
                    new PlaceholderType(TextLocation.Empty),
                    Modifiers.Export,
                    TextLocation.Empty
                )
            }, Enumerable.Empty<ImportDeclaration>());

            Assert.IsNotNull(ast);
            Helpers.DoTest(ast, expected_ast);
		}
	}
}
