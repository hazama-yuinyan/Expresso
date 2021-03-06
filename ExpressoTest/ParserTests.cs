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
    [SetUpFixture]
    public class SetupClass
    {
        [OneTimeSetUp]
        public void RunBeforeAnyTest()
        {
            var dir = Path.GetDirectoryName(typeof(SetupClass).Assembly.Location);
            Directory.SetCurrentDirectory(dir);
        }
    }

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
                EntityDeclaration.MakeFunc(
                    "main",
                    Enumerable.Empty<ParameterDeclaration>(),
                    Statement.MakeBlock(
                        Statement.MakeVarDecl(
                            Helpers.MakeSeq(Helpers.MakeSomePatternWithType("a")),
                            Helpers.MakeSeq(Expression.MakeConstant("int", 255)),
                            Modifiers.Immutable
                        ),
                        Statement.MakeVarDecl(
                            Helpers.MakeSeq(Helpers.MakeSomePatternWithType("h_a")),
                            Helpers.MakeSeq(Expression.MakeConstant("int", 0xff)),
                            Modifiers.Immutable
                        ),
                        Statement.MakeVarDecl(
                            Helpers.MakeSeq(
                                Helpers.MakeSomePatternWithType(
                                    "h_a_",
                                    Helpers.MakePrimitiveType("int")
                                )
                            ),
                            Helpers.MakeSeq(Expression.MakeConstant("int", 0xff)),
                            Modifiers.Immutable
                        ),
                        Statement.MakeVarDecl(
                            Helpers.MakeSeq(Helpers.MakeSomePatternWithType("b")),
                            Helpers.MakeSeq(Expression.MakeConstant("double", 1000.0)),
                            Modifiers.Immutable
                        ),
                        Statement.MakeVarDecl(
                            Helpers.MakeSeq(Helpers.MakeSomePatternWithType("f_b")),
                            Helpers.MakeSeq(Expression.MakeConstant("double", 1.0e4)),
                            Modifiers.Immutable
                        ),
                        Statement.MakeVarDecl(
                            Helpers.MakeSeq(
                                Helpers.MakeSomePatternWithType(
                                    "f_b_",
                                    Helpers.MakePrimitiveType("double")
                                )
                            ),
                            Helpers.MakeSeq(Expression.MakeConstant("double", 1.0e4)),
                            Modifiers.Immutable
                        ),
                        Statement.MakeVarDecl(
                            Helpers.MakeSeq(Helpers.MakeSomePatternWithType("c")),
                            Helpers.MakeSeq(Expression.MakeConstant("double", 0.001)),
                            Modifiers.Immutable
                        ),
                        Statement.MakeVarDecl(
                            Helpers.MakeSeq(Helpers.MakeSomePatternWithType("f_c")),
                            Helpers.MakeSeq(Expression.MakeConstant("double", .1e-2)),
                            Modifiers.Immutable
                        ),
                        Statement.MakeVarDecl(
                            Helpers.MakeSeq(Helpers.MakeSomePatternWithType("f_c2")),
                            Helpers.MakeSeq(Expression.MakeConstant("double", .0001)),
                            Modifiers.Immutable
                        ),
                        Statement.MakeVarDecl(
                            Helpers.MakeSeq(Helpers.MakeSomePatternWithType("d")),
                            Helpers.MakeSeq(Expression.MakeConstant("bigint", BigInteger.Parse("10000000"))),
                            Modifiers.Immutable
                        ),
                        Statement.MakeVarDecl(
                            Helpers.MakeSeq(
                                Helpers.MakeSomePatternWithType(
                                    "d_",
                                    Helpers.MakePrimitiveType("bigint")
                                )
                            ),
                            Helpers.MakeSeq(Expression.MakeConstant("bigint", BigInteger.Parse("10000000"))),
                            Modifiers.Immutable
                        ),
                        Statement.MakeVarDecl(
                            Helpers.MakeSeq(Helpers.MakeSomePatternWithType("d2")),
                            Helpers.MakeSeq(Expression.MakeConstant("bigint", BigInteger.Parse("10000000"))),
                            Modifiers.Immutable
                        ),
                        Statement.MakeVarDecl(
                            Helpers.MakeSeq(Helpers.MakeSomePatternWithType("e")),
                            Helpers.MakeSeq(Expression.MakeConstant("string", "This is a test")),
                            Modifiers.Immutable
                        ),
                        Statement.MakeVarDecl(
                            Helpers.MakeSeq(Helpers.MakeSomePatternWithType("u")),
                            Helpers.MakeSeq(Expression.MakeConstant("uint", 1000u)),
                            Modifiers.Immutable
                        ),
                        Statement.MakeVarDecl(
                            Helpers.MakeSeq(Helpers.MakeSomePatternWithType("u_")),
                            Helpers.MakeSeq(Expression.MakeConstant("uint", 1000U)),
                            Modifiers.Immutable
                        ),
                        Statement.MakeVarDecl(
                            Helpers.MakeSeq(Helpers.MakeSomePatternWithType("f_a")),
                            Helpers.MakeSeq(Expression.MakeConstant("float", 1.0e4f)),
                            Modifiers.Immutable
                        ),
                        Statement.MakeVarDecl(
                            Helpers.MakeSeq(
                                Helpers.MakeSomePatternWithType(
                                    "f_a_",
                                    Helpers.MakePrimitiveType("float")
                                )
                            ),
                            Helpers.MakeSeq(Expression.MakeConstant("float", 1.0e4f)),
                            Modifiers.Immutable
                        ),
                        Statement.MakeVarDecl(
                            Helpers.MakeSeq(
                                Helpers.MakeSomePatternWithType(
                                    "f",
                                    Helpers.MakeGenericType("array", Helpers.MakePrimitiveType("int"))
                                )
                            ),
                            Helpers.MakeSeq(
                                Expression.MakeSequenceInitializer(
                                    Helpers.MakeGenericType("array", Helpers.MakePlaceholderType()),
                                    Enumerable.Empty<Expression>()
                                )
                            ),
                            Modifiers.Immutable
                        ),
                        Statement.MakeVarDecl(
                            Helpers.MakeSeq(Helpers.MakeSomePatternWithType("f_")),
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
                            Helpers.MakeSeq(
                                Helpers.MakeSomePatternWithType(
                                    "f2",
                                    Helpers.MakeGenericType("vector", Helpers.MakePrimitiveType("int"))
                                )
                            ),
                            Helpers.MakeSeq(
                                Expression.MakeSequenceInitializer(
                                    Helpers.MakeGenericType("vector", Helpers.MakePlaceholderType()),
                                    Enumerable.Empty<Expression>()
                                )
                            ),
                            Modifiers.Immutable
                        ),
                        Statement.MakeVarDecl(
                            Helpers.MakeSeq(Helpers.MakeSomePatternWithType("f2_")),
                            Helpers.MakeSeq(
                                Expression.MakeSequenceInitializer(
                                    Helpers.MakeGenericType("vector", Helpers.MakePlaceholderType()),
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
                            Helpers.MakeSeq(Helpers.MakeSomeIdentifierPattern("f3")),
                            Helpers.MakeSeq(Expression.MakeParen(Expression.MakeSequenceExpression(null))),
                            Modifiers.Immutable
                        ),*/
                        Statement.MakeVarDecl(
                            Helpers.MakeSeq(Helpers.MakeSomePatternWithType("f3_")),
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
                            Helpers.MakeSeq(
                                Helpers.MakeSomeTuplePatternWithType(
                                    "f3_a",
                                    "f3_b",
                                    "f3_c"
                                )
                            ),
                            Helpers.MakeSeq(Helpers.MakeIdentifierPath("f3_")),
                            Modifiers.Immutable
                        ),
                        Statement.MakeVarDecl(
                            Helpers.MakeSeq(
                                Helpers.MakeSomePatternWithType(
                                    "g",
                                    Helpers.MakeGenericType("dictionary", Helpers.MakePrimitiveType("string"), Helpers.MakePrimitiveType("int"))
                                )
                            ),
                            Helpers.MakeSeq(
                                Expression.MakeSequenceInitializer(
                                    Helpers.MakeGenericType("dictionary", Helpers.MakePlaceholderType(), Helpers.MakePlaceholderType()),
                                    Enumerable.Empty<Expression>()
                                )
                            ),
                            Modifiers.Immutable
                        ),
                        Statement.MakeVarDecl(
                            Helpers.MakeSeq(Helpers.MakeSomePatternWithType("g_")),
                            Helpers.MakeSeq(
                                Expression.MakeSequenceInitializer(
                                    Helpers.MakeGenericType("dictionary", Helpers.MakePlaceholderType(), Helpers.MakePlaceholderType()),
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
                            Helpers.MakeSeq(Helpers.MakeSomePatternWithType("h")),
                            Helpers.MakeSeq(Expression.MakeConstant("string", "私変わっちゃうの・・？")),
                            Modifiers.None
                        ),
                        Statement.MakeVarDecl(
                            Helpers.MakeSeq(Helpers.MakeSomePatternWithType("h2")),
                            Helpers.MakeSeq(Expression.MakeConstant("string", "Yes, you can!")),
                            Modifiers.None
                        ),
                        Statement.MakeVarDecl(
                            Helpers.MakeSeq(Helpers.MakeSomePatternWithType("h3")),
                            Helpers.MakeSeq(Expression.MakeConstant("char", 'a')),
                            Modifiers.None
                        ),
                        Statement.MakeVarDecl(
                            Helpers.MakeSeq(Helpers.MakeSomePatternWithType("i")),
                            Helpers.MakeSeq(Expression.MakeConstant("string", "よかった。私変わらないんだね！・・え、変われないの？・・・なんだかフクザツ")),
                            Modifiers.Immutable
                        ),
                        Statement.MakeVarDecl(
                            Helpers.MakeSeq(Helpers.MakeSomePatternWithType("i2")),
                            Helpers.MakeSeq(Expression.MakeConstant("string", "Oh, you just can't...")),
                            Modifiers.Immutable
                        ),
                        Statement.MakeVarDecl(
                            Helpers.MakeSeq(Helpers.MakeSomePatternWithType("i3")),
                            Helpers.MakeSeq(Expression.MakeConstant("char", 'a')),
                            Modifiers.Immutable
                        ),
                        Statement.MakeVarDecl(
                            Helpers.MakeSeq(Helpers.MakeSomePatternWithType("i4")),
                            Helpers.MakeSeq(Expression.MakeConstant("string", "\u0041\u005a\u0061\u007A\u3042\u30A2")), //AZazあア
                            Modifiers.Immutable
                        ),
                        Statement.MakeVarDecl(
                            Helpers.MakeSeq(Helpers.MakeSomePatternWithType("i5")),
                            Helpers.MakeSeq(Expression.MakeConstant("char", '\u0041')), //A
                            Modifiers.Immutable
                        ),
                        Statement.MakeVarDecl(
                            Helpers.MakeSeq(Helpers.MakeSomePatternWithType("i6")),
                            Helpers.MakeSeq(Expression.MakeConstant("string", "This is a normal string.\n Seems 2 lines? Yes, you're right!")),
                            Modifiers.Immutable
                        ),
                        Statement.MakeVarDecl(
                            Helpers.MakeSeq(Helpers.MakeSomePatternWithType("i6_")),
                            Helpers.MakeSeq(Expression.MakeConstant("string", @"This is a raw string.\n Seems 2 lines? Nah, indeed.")),
                            Modifiers.Immutable
                        ),
                        Statement.MakeVarDecl(
                            Helpers.MakeSeq(Helpers.MakeSomePatternWithType("j")),
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
                            Helpers.MakeSeq(Helpers.MakeSomePatternWithType("j2")),
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
                                Helpers.MakeSomePatternWithType(
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
                            Helpers.MakeSeq(Helpers.MakeSomePatternWithType("j_2")),
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
                            Helpers.MakeSeq(Helpers.MakeSomePatternWithType("j_2_")),
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
                                Helpers.MakeIdentifierPath("println"),
                                Expression.MakeConstant("string", "${a}, ${h_a}, ${h_a_}, ${b}, ${f_b}, ${f_b_}, ${c}, ${f_c}, ${f_c2}, ${d}, ${d_}, ${d2}, ${e}, ${u}, ${u_}, ${f_a}, ${f_a_}, ${f}, ${f_}, ${f2}, ${f2_}")
                            )
                        ),
                        Statement.MakeExprStmt(
                            Helpers.MakeCallExpression(
                                Helpers.MakeIdentifierPath("println"),
                                Expression.MakeConstant("string", "${f3_}, ${f3_a}, ${f3_b}, ${f3_c}, ${g}, ${g_}, ${h}, ${h2}, ${h3}, ${i}, ${i2}, ${i3}, ${i4}, ${i5}, ${i6}, ${i6_}, ${j}, ${j2}, ${j3}, ${j_2}, ${j_2_}")
                            )
                        )
                    ),
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
        public void SimpleArithmetic()
        {
            var parser = new Parser(new Scanner("../../sources/for_unit_tests/simple_arithmetic.exs"));
            parser.Parse();

            var ast = parser.TopmostAst;

            var expected_ast = AstNode.MakeModuleDef("main", new List<EntityDeclaration>{
                EntityDeclaration.MakeFunc(
                    "main",
                    Helpers.MakeSeq(
                        EntityDeclaration.MakeParameter(
                            "args",
                            Helpers.MakeGenericType(
                                "array", 
                                AstType.MakePrimitiveType("string")
                            )
                        )
                    ),
                    Statement.MakeBlock(
                        Statement.MakeVarDecl(
                            Helpers.MakeSeq(
                                Helpers.MakeSomePatternWithType("x"),
                                Helpers.MakeSomePatternWithType("xp"),
                                Helpers.MakeSomePatternWithType("xm"),
                                Helpers.MakeSomePatternWithType("xt"),
                                Helpers.MakeSomePatternWithType("xd"),
                                Helpers.MakeSomePatternWithType("xmod"),
                                Helpers.MakeSomePatternWithType("xpower")
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
                            Helpers.MakeSeq(Helpers.MakeSomePatternWithType("a")),
                            Helpers.MakeSeq(
                                Expression.MakeBinaryExpr(
                                    OperatorType.Plus,
                                    Helpers.MakeIdentifierPath("x"),
                                    Expression.MakeConstant("int", 4)
                                )
                            ),
                            Modifiers.Immutable
                        ),
                        Statement.MakeVarDecl(
                            Helpers.MakeSeq(Helpers.MakeSomePatternWithType("b")),
                            Helpers.MakeSeq(
                                Expression.MakeBinaryExpr(
                                    OperatorType.Minus,
                                    Helpers.MakeIdentifierPath("x"),
                                    Expression.MakeConstant("int", 4)
                                )
                            ),
                            Modifiers.Immutable
                        ),
                        Statement.MakeVarDecl(
                            Helpers.MakeSeq(Helpers.MakeSomePatternWithType("c")),
                            Helpers.MakeSeq(
                                Expression.MakeBinaryExpr(
                                    OperatorType.Times,
                                    Helpers.MakeIdentifierPath("x"),
                                    Expression.MakeConstant("int", 4)
                                )
                            ),
                            Modifiers.Immutable
                        ),
                        Statement.MakeVarDecl(
                            Helpers.MakeSeq(Helpers.MakeSomePatternWithType("d")),
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
                            Helpers.MakeSeq(Helpers.MakeSomePatternWithType("e")),
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
                            Helpers.MakeSeq(Helpers.MakeSomePatternWithType("f")),
                            Helpers.MakeSeq(
                                Expression.MakeBinaryExpr(
                                    OperatorType.Power,
                                    Helpers.MakeIdentifierPath("x"),
                                    Expression.MakeConstant("int", 2)
                                )
                            ),
                            Modifiers.Immutable
                        ),
                        Statement.MakeExprStmt(
                            Expression.MakeSingleAugmentedAssignment(
                                OperatorType.Plus,
                                Helpers.MakeIdentifierPath("xp"),
                                Expression.MakeConstant("int", 4)
                            )
                        ),
                        Statement.MakeExprStmt(
                            Expression.MakeSingleAugmentedAssignment(
                                OperatorType.Minus,
                                Helpers.MakeIdentifierPath("xm"),
                                Expression.MakeConstant("int", 4)
                            )
                        ),
                        Statement.MakeExprStmt(
                            Expression.MakeSingleAugmentedAssignment(
                                OperatorType.Times,
                                Helpers.MakeIdentifierPath("xt"),
                                Expression.MakeConstant("int", 4)
                            )
                        ),
                        Statement.MakeExprStmt(
                            Expression.MakeSingleAugmentedAssignment(
                                OperatorType.Divide,
                                Helpers.MakeIdentifierPath("xd"),
                                Expression.MakeConstant("int", 2)
                            )
                        ),
                        Statement.MakeExprStmt(
                            Expression.MakeSingleAugmentedAssignment(
                                OperatorType.Modulus,
                                Helpers.MakeIdentifierPath("xmod"),
                                Expression.MakeConstant("int", 2)
                            )
                        ),
                        Statement.MakeExprStmt(
                            Expression.MakeSingleAugmentedAssignment(
                                OperatorType.Power,
                                Helpers.MakeIdentifierPath("xpower"),
                                Expression.MakeConstant("int", 2)
                            )
                        ),
                        Statement.MakeExprStmt(
                            Helpers.MakeCallExpression(
                                Helpers.MakeIdentifierPath("println"),
                                Expression.MakeConstant("string", "${x}, ${a}, ${b}, ${c}, ${d}, ${e}, ${f}, ${xp}, ${xm}, ${xt}, ${xd}, ${xmod}, ${xpower}")
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
            parser.Parse();

            var ast = parser.TopmostAst;

            var expected_ast = AstNode.MakeModuleDef("main", new List<EntityDeclaration>{
                EntityDeclaration.MakeFunc(
                    "main",
                    Enumerable.Empty<ParameterDeclaration>(),
                    Statement.MakeBlock(
                        Statement.MakeVarDecl(
                            Helpers.MakeSeq(Helpers.MakeSomePatternWithType("ary")),
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
                            Helpers.MakeSeq(Helpers.MakeSomePatternWithType("d")),
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
                            Helpers.MakeSeq(Helpers.MakeSomePatternWithType("m")),
                            Helpers.MakeSeq(
                                Helpers.MakeIndexerExpression(
                                    Helpers.MakeIdentifierPath("ary"),
                                    Expression.MakeConstant("int", 0)
                                )
                            ),
                            Modifiers.Immutable
                        ),
                        Statement.MakeVarDecl(
                            Helpers.MakeSeq(Helpers.MakeSomePatternWithType("m2")),
                            Helpers.MakeSeq(
                                Helpers.MakeIndexerExpression(
                                    Helpers.MakeIdentifierPath("d"),
                                    Expression.MakeConstant("string", "a")
                                )
                            ),
                            Modifiers.Immutable
                        ),
                        Statement.MakeVarDecl(
                            Helpers.MakeSeq(Helpers.MakeSomePatternWithType("x")),
                            Helpers.MakeSeq(Expression.MakeConstant("int", 100)),
                            Modifiers.Immutable
                        ),
                        Statement.MakeVarDecl(
                            Helpers.MakeSeq(Helpers.MakeSomePatternWithType("p")),
                            Helpers.MakeSeq(
                                Expression.MakeBinaryExpr(
                                    OperatorType.Plus,
                                    Helpers.MakeIndexerExpression(
                                        Helpers.MakeIdentifierPath("ary"),
                                        Expression.MakeConstant("int", 0)
                                    ),
                                    Expression.MakeBinaryExpr(
                                        OperatorType.Plus,
                                        Helpers.MakeIndexerExpression(
                                            Helpers.MakeIdentifierPath("ary"),
                                            Expression.MakeConstant("int", 1)
                                        ),
                                        Helpers.MakeIndexerExpression(
                                            Helpers.MakeIdentifierPath("ary"),
                                            Expression.MakeConstant("int", 2)
                                        )
                                    )
                                )
                            ),
                            Modifiers.Immutable
                        ),
                        Statement.MakeVarDecl(
                            Helpers.MakeSeq(Helpers.MakeSomePatternWithType("q")),
                            Helpers.MakeSeq(
                                Expression.MakeBinaryExpr(
                                    OperatorType.Plus,
                                    Helpers.MakeIndexerExpression(
                                        Helpers.MakeIdentifierPath("d"),
                                        Expression.MakeConstant("string", "a")
                                    ),
                                    Expression.MakeBinaryExpr(
                                        OperatorType.Plus,
                                        Helpers.MakeIndexerExpression(
                                            Helpers.MakeIdentifierPath("d"),
                                            Expression.MakeConstant("string", "b")
                                        ),
                                        Helpers.MakeIndexerExpression(
                                            Helpers.MakeIdentifierPath("d"),
											Expression.MakeConstant("string", "何か")
                                        )
                                    )
                                )
                            ),
                            Modifiers.Immutable
                        ),
                        Statement.MakeVarDecl(
                            Helpers.MakeSeq(Helpers.MakeSomePatternWithType("r")),
                            Helpers.MakeSeq(
                                Expression.MakeBinaryExpr(
                                    OperatorType.BitwiseShiftRight,
                                    Helpers.MakeIdentifierPath("x"),
                                    Helpers.MakeIdentifierPath("p")
                                )
                            ),
                            Modifiers.Immutable
                        ),
                        Statement.MakeVarDecl(
                            Helpers.MakeSeq(Helpers.MakeSomePatternWithType("s")),
                            Helpers.MakeSeq(
                                Expression.MakeBinaryExpr(
                                    OperatorType.BitwiseShiftLeft,
                                    Helpers.MakeIdentifierPath("x"),
                                    Expression.MakeConstant("int", 2)
                                )
                            ),
                            Modifiers.Immutable
                        ),
                        Statement.MakeVarDecl(
                            Helpers.MakeSeq(Helpers.MakeSomePatternWithType("t")),
                            Helpers.MakeSeq(
                                Expression.MakeBinaryExpr(
                                    OperatorType.BitwiseAnd,
                                    Helpers.MakeIdentifierPath("r"),
                                    Helpers.MakeIdentifierPath("s")
                                )
                            ),
                            Modifiers.Immutable
                        ),
                        Statement.MakeVarDecl(
                            Helpers.MakeSeq(Helpers.MakeSomePatternWithType("v")),
                            Helpers.MakeSeq(
                                Expression.MakeBinaryExpr(
                                    OperatorType.BitwiseOr,
                                    Helpers.MakeIdentifierPath("x"),
                                    Helpers.MakeIdentifierPath("t")
                                )
                            ),
                            Modifiers.Immutable
                        ),
                        Statement.MakeVarDecl(
                            Helpers.MakeSeq(Helpers.MakeSomePatternWithType("w")),
                            Helpers.MakeSeq(
                                Expression.MakeBinaryExpr(
                                    OperatorType.Plus,
                                    Helpers.MakeIdentifierPath("r"),
                                    Helpers.MakeIdentifierPath("s")
                                )
                            ),
                            Modifiers.Immutable
                        ),
                        Statement.MakeVarDecl(
                            Helpers.MakeSeq(Helpers.MakeSomePatternWithType("y")),
                            Helpers.MakeSeq(
                                Expression.MakeBinaryExpr(
                                    OperatorType.Plus,
                                    Helpers.MakeIndexerExpression(
                                        Helpers.MakeIdentifierPath("ary"),
                                        Expression.MakeConstant("int", 0)
                                    ),
                                    Expression.MakeBinaryExpr(
                                        OperatorType.Plus,
                                        Helpers.MakeIndexerExpression(
                                            Helpers.MakeIdentifierPath("ary"),
                                            Expression.MakeConstant("int", 1)
                                        ),
                                        Helpers.MakeIndexerExpression(
                                            Helpers.MakeIdentifierPath("d"),
                                            Expression.MakeConstant("string", "a")
                                        )
                                    )
                                )
                            ),
                            Modifiers.Immutable
                        ),
                        Statement.MakeVarDecl(
                            Helpers.MakeSeq(Helpers.MakeSomePatternWithType("z")),
                            Helpers.MakeSeq(
                                Expression.MakeBinaryExpr(
                                    OperatorType.Times,
                                    Helpers.MakeIdentifierPath("v"),
                                    Helpers.MakeIdentifierPath("w")
                                )
                            ),
                            Modifiers.Immutable
                        ),
                        Statement.MakeExprStmt(
                            Helpers.MakeCallExpression(
                                Helpers.MakeIdentifierPath("println"),
                                Expression.MakeConstant("string", "${ary}, ${d}, ${m}, ${m2}, ${x}, ${p}, ${q}, ${r}, ${s}, ${t}, ${v}, ${w}, ${y}, ${z}")
                            )
                        )
                    ),
                    Helpers.MakePlaceholderType(),
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
            parser.Parse();

            var ast = parser.TopmostAst;

            var expected_ast = AstNode.MakeModuleDef("main", new List<EntityDeclaration>{
                EntityDeclaration.MakeFunc(
                    "main",
                    Enumerable.Empty<ParameterDeclaration>(),
                    Statement.MakeBlock(
                        Statement.MakeVarDecl(
                            Helpers.MakeSeq(
                                Helpers.MakeSomePatternWithType("x"),
                                Helpers.MakeSomePatternWithType("y"),
                                Helpers.MakeSomePatternWithType("z"),
                                Helpers.MakeSomePatternWithType("w")
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
                            Helpers.MakeSeq(Helpers.MakeSomePatternWithType("flag", Helpers.MakePrimitiveType("bool"))),
                            Modifiers.None
                        ),
                        Statement.MakeExprStmt(
                            Expression.MakeSequenceExpression(
                                Helpers.MakeCallExpression(
                                    Helpers.MakeIdentifierPath("print"),
                                    Expression.MakeConstant("string", "${x}, ${y}, ${z}, ${w}")
                                )
                            )
                        ),
                        Statement.MakeIfStmt(
                            PatternConstruct.MakeExpressionPattern(
                                Expression.MakeBinaryExpr(
                                    OperatorType.Equality,
                                    Helpers.MakeIdentifierPath("x"),
                                    Expression.MakeConstant("int", 100)
                                )
                            ),
                            Statement.MakeBlock(
                                Helpers.MakeSeq(
                                    Statement.MakeExprStmt(
                                        Expression.MakeSingleAssignment(
                                            Helpers.MakeIdentifierPath("flag"),
                                            Expression.MakeConstant("bool", true)
                                        )
                                    )
                                )
                            ),
                            Statement.MakeIfStmt(
                                PatternConstruct.MakeExpressionPattern(
                                    Expression.MakeBinaryExpr(
                                        OperatorType.Equality,
                                        Helpers.MakeIdentifierPath("x"),
                                        Expression.MakeConstant("int", 0)
                                    )
                                ),
                                Statement.MakeBlock(
                                    Statement.MakeExprStmt(
                                        Expression.MakeSingleAssignment(
                                            Helpers.MakeIdentifierPath("flag"),
                                            Expression.MakeConstant("bool", false)
                                        )
                                    )
                                ),
                                Statement.MakeBlock(
                                    Statement.MakeExprStmt(
                                        Expression.MakeSingleAssignment(
                                            Helpers.MakeIdentifierPath("flag"),
                                            Expression.MakeConstant("bool", false)
                                        )
                                    )
                                )
                            )
                        ),
                        Statement.MakeVarDecl(
                            Helpers.MakeSeq(Helpers.MakeSomePatternWithType("sum")),
                            Helpers.MakeSeq(Expression.MakeConstant("int", 0)),
                            Modifiers.None
                        ),
                        Statement.MakeValueBindingForStmt(
                            Modifiers.Immutable,
                            Helpers.MakeSomePatternWithType("p"),
                            Expression.MakeIntSeq(
                                Expression.MakeConstant("int", 0),
                                Helpers.MakeIdentifierPath("y"),
                                Expression.MakeConstant("int", 1),
                                false
                            ),
                            Statement.MakeBlock(
                                Helpers.MakeSeq(
                                    Helpers.MakeAugmentedAssignment(
                                        OperatorType.Plus,
                                        Helpers.MakeSeq(Helpers.MakeIdentifierPath("sum")),
                                        Helpers.MakeSeq(Helpers.MakeIdentifierPath("p"))
                                    ),
                                    Statement.MakeExprStmt(
                                        Expression.MakeSequenceExpression(
                                            Helpers.MakeCallExpression(
                                                Helpers.MakeIdentifierPath("println"),
                                                Expression.MakeConstant("string", "${p}, ${sum}")
                                            )
                                        )
                                    )
                                )
                            )
                        ),
                        Statement.MakeVarDecl(
                            Helpers.MakeSeq(
                                Helpers.MakeSomePatternWithType(
                                    "fibs",
                                    Helpers.MakeGenericType(
                                        "vector",
                                        AstType.MakePrimitiveType("int")
                                    )
                                ),
                                Helpers.MakeSomePatternWithType("a"),
                                Helpers.MakeSomePatternWithType("b")
                            ),
                            Helpers.MakeSeq<Expression>(
                                Expression.MakeSequenceInitializer(
                                    Helpers.MakeGenericType("vector", Helpers.MakePlaceholderType()),
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
                                Helpers.MakeIdentifierPath("b"),
                                Expression.MakeConstant("int", 1000)
                            ),
                            Statement.MakeBlock(
                                Statement.MakeExprStmt(
                                    Expression.MakeSequenceExpression(
                                        Helpers.MakeCallExpression(
                                            Expression.MakeMemRef(
                                                Helpers.MakeIdentifierPath("fibs"),
                                                Helpers.MakeSomeIdent("Add")
                                            ),
                                            Helpers.MakeIdentifierPath("b")
                                        )
                                    )
                                ),
                                Statement.MakeExprStmt(
                                    Helpers.MakeAssignment(
                                        Helpers.MakeSeq(
                                            Helpers.MakeIdentifierPath("a"),
                                            Helpers.MakeIdentifierPath("b")
                                        ),
                                        Helpers.MakeSeq<Expression>(
                                            Helpers.MakeIdentifierPath("b"),
                                            Expression.MakeBinaryExpr(
                                                OperatorType.Plus,
                                                Helpers.MakeIdentifierPath("a"),
                                                Helpers.MakeIdentifierPath("b")
                                            )
                                        )
                                    )
                                )
                            )
                        ),
                        Statement.MakeVarDecl(
                            Helpers.MakeSeq(
                                Helpers.MakeSomePatternWithType("n")
                            ),
                            Helpers.MakeSeq(
                                Expression.MakeConstant("int", 100)
                            ),
                            Modifiers.None
                        ),
                        Statement.MakeDoWhileStmt(
                            Expression.MakeBinaryExpr(
                                OperatorType.GreaterThan,
                                Helpers.MakeIdentifierPath("n"),
                                Expression.MakeConstant("int", 0)
                            ),
                            Statement.MakeBlock(
                                Statement.MakeExprStmt(
                                    Expression.MakeSingleAugmentedAssignment(
                                        OperatorType.Minus,
                                        Helpers.MakeIdentifierPath("n"),
                                        Expression.MakeConstant("int", 40)
                                    )
                                ),
                                Statement.MakeExprStmt(
                                    Helpers.MakeCallExpression(
                                        Helpers.MakeIdentifierPath("println"),
                                        Expression.MakeConstant("string", "${n}")
                                    )
                                )
                            )
                        ),
                        Statement.MakeVarDecl(
                            Helpers.MakeSeq(
                                Helpers.MakeSomePatternWithType(
                                    "vec",
                                    Helpers.MakeGenericType(
                                        "vector",
                                        AstType.MakePrimitiveType("int")
                                    )
                                )
                            ),
                            Helpers.MakeSeq(
                                Expression.MakeSequenceInitializer(
                                    Helpers.MakeGenericType("vector", Helpers.MakePlaceholderType()),
                                    Enumerable.Empty<Expression>()
                                )
                            ),
                            Modifiers.None
                        ),
                        Statement.MakeValueBindingForStmt(
                            Modifiers.Immutable,
                            Helpers.MakeSomePatternWithType("i"),
                            Helpers.MakeSequenceInitializer(
                                Helpers.MakeGenericType("vector", Helpers.MakePlaceholderType()),
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
                                        Helpers.MakeSomePatternWithType("MAX_J")
                                    ),
                                    Helpers.MakeSeq(
                                        Expression.MakeConstant("int", 8)
                                    ),
                                    Modifiers.Immutable
                                ),
                                Statement.MakeValueBindingForStmt(
                                    Modifiers.Immutable,
                                    Helpers.MakeSomePatternWithType("j"),
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
                                                    OperatorType.ConditionalAnd,
                                                    Expression.MakeBinaryExpr(
                                                        OperatorType.Equality,
                                                        Helpers.MakeIdentifierPath("i"),
                                                        Expression.MakeConstant("int", 3)
                                                    ),
                                                    Expression.MakeBinaryExpr(
                                                        OperatorType.Equality,
                                                        Helpers.MakeIdentifierPath("i"),
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
                                                Expression.MakeBinaryExpr(
                                                    OperatorType.Equality,
                                                    Helpers.MakeIdentifierPath("j"),
                                                    Helpers.MakeIdentifierPath("MAX_J")
                                                )
                                            ),
                                            Statement.MakeBlock(
                                                Statement.MakeContinueStmt(Expression.MakeConstant("int", 2))
                                            ),
                                            null
                                        ),
                                        Statement.MakeExprStmt(
                                            Helpers.MakeCallExpression(
                                                Expression.MakeMemRef(
                                                    Helpers.MakeIdentifierPath("vec"),
                                                    Helpers.MakeSomeIdent("Add")
                                                ),
                                                Helpers.MakeIdentifierPath("i")
                                            )
                                        ),
                                        Statement.MakeExprStmt(
                                            Helpers.MakeCallExpression(
                                                Expression.MakeMemRef(
                                                    Helpers.MakeIdentifierPath("vec"),
                                                    Helpers.MakeSomeIdent("Add")
                                                ),
                                                Helpers.MakeIdentifierPath("j")
                                            )
                                        ),
                                        Statement.MakeExprStmt(
                                            Helpers.MakeCallExpression(
                                                Helpers.MakeIdentifierPath("println"),
                                                Expression.MakeConstant("string", "${i}, ${j}")
                                            )
                                        )
                                    )
                                )
                            )
                        ),
                        Statement.MakeExprStmt(
                            Helpers.MakeCallExpression(
                                Helpers.MakeIdentifierPath("println"),
                                Expression.MakeConstant("string", "${flag}, ${sum}, ${a}, ${b}, ${fibs}, ${vec}")
                            )
                        )
                    ),
                    Helpers.MakePlaceholderType(),
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
            parser.Parse();

            var ast = parser.TopmostAst;

            var expected = AstNode.MakeModuleDef("main", new List<EntityDeclaration>{
                EntityDeclaration.MakeClassDecl(
                    "TestClass6",
                    Enumerable.Empty<AstType>(),
                    Helpers.MakeSeq<EntityDeclaration>(
                        EntityDeclaration.MakeField(
                            Helpers.MakeSeq(
                                Helpers.MakeSomePatternWithType("x", Helpers.MakePrimitiveType("int")),
                                Helpers.MakeSomePatternWithType("y", Helpers.MakePrimitiveType("int")),
                                Helpers.MakeSomePatternWithType("z", Helpers.MakePrimitiveType("int"))
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
                            Helpers.MakeSeq(Helpers.MakeSomePatternWithType("tmp")),
                            Helpers.MakeSeq(Expression.MakeConstant("string", "akarichan")),
                            Modifiers.Immutable
                        ),
                        Statement.MakeMatchStmt(
                            Helpers.MakeIdentifierPath("tmp"),
                            Statement.MakeMatchClause(
                                null,
                                Statement.MakeExprStmt(
                                    Expression.MakeSequenceExpression(
                                        Helpers.MakeCallExpression(
                                            Helpers.MakeIdentifierPath("println"),
                                            Expression.MakeConstant("string", "kawakawa")
                                        )
                                    )
                                ),
                                PatternConstruct.MakeExpressionPattern(Expression.MakeConstant("string", "akarichan"))
                            ),
                            Statement.MakeMatchClause(
                                Expression.MakeConstant("bool", true),
                                Statement.MakeExprStmt(
                                    Expression.MakeSequenceExpression(
                                        Helpers.MakeCallExpression(
                                            Helpers.MakeIdentifierPath("println"),
                                            Expression.MakeConstant("string", "ankokuthunder!")
                                        )
                                    )
                                ),
                                PatternConstruct.MakeExpressionPattern(Expression.MakeConstant("string", "chinatsu"))
                            ),
                            Statement.MakeMatchClause(
                                null,
                                Statement.MakeExprStmt(
                                    Expression.MakeSequenceExpression(
                                        Helpers.MakeCallExpression(
                                            Helpers.MakeIdentifierPath("println"),
                                            Expression.MakeConstant("string", "gaichiban!")
                                        )
                                    )
                                ),
                                PatternConstruct.MakeExpressionPattern(Expression.MakeConstant("string", "kyoko"))
                            ),
                            Statement.MakeMatchClause(
                                null,
                                Statement.MakeExprStmt(
                                    Expression.MakeSequenceExpression(
                                        Helpers.MakeCallExpression(
                                            Helpers.MakeIdentifierPath("println"),
                                            Expression.MakeConstant("string", "doyaxtu!")
                                        )
                                    )
                                ),
                                PatternConstruct.MakeExpressionPattern(Expression.MakeConstant("string", "yui"))
                            )
                        ),
                        Statement.MakeVarDecl(
                            Helpers.MakeSeq(Helpers.MakeSomePatternWithType("tmp2")),
                            Helpers.MakeSeq(
                                Expression.MakeConstant("int", 1)
                            ),
                            Modifiers.Immutable
                        ),
                        Statement.MakeMatchStmt(
                            Helpers.MakeIdentifierPath("tmp2"),
                            Statement.MakeMatchClause(
                                null,
                                Statement.MakeExprStmt(
                                    Expression.MakeSequenceExpression(
                                        Helpers.MakeCallExpression(
                                            Helpers.MakeIdentifierPath("println"),
                                            Expression.MakeConstant("string", "0")
                                        )
                                    )
                                ),
                                PatternConstruct.MakeExpressionPattern(Expression.MakeConstant("int", 0))
                            ),
                            Statement.MakeMatchClause(
                                null,
                                Statement.MakeExprStmt(
                                    Expression.MakeSequenceExpression(
                                        Helpers.MakeCallExpression(
                                            Helpers.MakeIdentifierPath("println"),
                                            Expression.MakeConstant("string", "1 or 2")
                                        )
                                    )
                                ),
                                PatternConstruct.MakeExpressionPattern(Expression.MakeConstant("int", 1)),
                                PatternConstruct.MakeExpressionPattern(Expression.MakeConstant("int", 2))
                            ),
                            Statement.MakeMatchClause(
                                null,
                                Statement.MakeExprStmt(
                                    Expression.MakeSequenceExpression(
                                        Helpers.MakeCallExpression(
                                            Helpers.MakeIdentifierPath("println"),
                                            Expression.MakeConstant("string", "${i} is in the range of 3 to 10")
                                        )
                                    )
                                ),
                                PatternConstruct.MakeIdentifierPattern(
                                    "i",
                                    Helpers.MakePlaceholderType(),
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
                                            Helpers.MakeIdentifierPath("println"),
                                            Expression.MakeConstant("string", "otherwise")
                                        )
                                    )
                                ),
                                PatternConstruct.MakeWildcardPattern()
                            )
                        ),
                        Statement.MakeVarDecl(
                            Helpers.MakeSeq(Helpers.MakeSomePatternWithType("tmp3")),
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
                            Helpers.MakeIdentifierPath("tmp3"),
                            Statement.MakeMatchClause(
                                null,
                                Statement.MakeExprStmt(
                                    Expression.MakeSequenceExpression(
                                        Helpers.MakeCallExpression(
                                            Helpers.MakeIdentifierPath("println"),
                                            Expression.MakeConstant("string", "x is ${x}")
                                        )
                                    )
                                ),
                                PatternConstruct.MakeDestructuringPattern(
                                    Helpers.MakeGenericType("TestClass6"),
                                    PatternConstruct.MakeIdentifierPattern(
                                        "x",
                                        Helpers.MakePlaceholderType(),
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
                                            Helpers.MakeIdentifierPath("println"),
                                            Expression.MakeConstant("string", "x is ${x} and y is ${y}")
                                        )
                                    )
                                ),
                                PatternConstruct.MakeDestructuringPattern(
                                    Helpers.MakeGenericType("TestClass6"),
                                    PatternConstruct.MakeIdentifierPattern(
                                        "x",
                                        Helpers.MakePlaceholderType(),
                                        null
                                    ),
                                    PatternConstruct.MakeIdentifierPattern(
                                        "y",
                                        Helpers.MakePlaceholderType(),
                                        null
                                    ),
                                    PatternConstruct.MakeWildcardPattern()
                                )
                            )
                        ),
                        /*Statement.MakeVarDecl(
                            Helpers.MakeSeq(
                                Helpers.MakeSomeIdent("tmp4")
                            ),
                            Helpers.MakeSeq(
                                Expression.MakeSequenceInitializer(
                                    Helpers.MakeGenericType("vector", Helpers.MakePlaceholderType()),
                                    Expression.MakeConstant("int", 1),
                                    Expression.MakeConstant("int", 2),
                                    Expression.MakeConstant("int", 3),
                                    Expression.MakeConstant("int", 4)
                                )
                            ),
                            Modifiers.Immutable
                        ),
                        Statement.MakeMatchStmt(
                            Helpers.MakeIdentifierPath("tmp4"),
                            Statement.MakeMatchClause(
                                null,
                                Statement.MakeExprStmt(
                                    Expression.MakeSequenceExpression(
                                        Helpers.MakeCallExpression(
                                            Helpers.MakeIdentifierPath("println"),
                                            Expression.MakeConstant("string", "x and y are both vector's elements and the values are ${x} and ${y} respectively")
                                        )
                                    )
                                ),
                                PatternConstruct.MakeCollectionPattern(
                                    true,
                                    PatternConstruct.MakeIdentifierPattern(
                                        "x",
                                        Helpers.MakePlaceholderType(),
                                        null
                                    ),
                                    PatternConstruct.MakeIdentifierPattern(
                                        "y",
                                        Helpers.MakePlaceholderType(),
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
                                            Helpers.MakeIdentifierPath("print"),
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
                                Helpers.MakeSomeIdent("tmp5")
                            ),
                            Helpers.MakeSeq(
                                Expression.MakeSequenceInitializer(
                                    Helpers.MakeGenericType("array", Helpers.MakePlaceholderType()),
                                    Expression.MakeConstant("int", 1),
                                    Expression.MakeConstant("int", 2),
                                    Expression.MakeConstant("int", 3),
                                    Expression.MakeConstant("int", 4)
                                )
                            ),
                            Modifiers.Immutable
                        ),
                        Statement.MakeMatchStmt(
                            Helpers.MakeIdentifierPath("tmp5"),
                            Statement.MakeMatchClause(
                                null,
                                Statement.MakeExprStmt(
                                    Expression.MakeSequenceExpression(
                                        Helpers.MakeCallExpression(
                                            Helpers.MakeIdentifierPath("println"),
                                            Expression.MakeConstant("string", "x and y are both array's elements and the values are ${x} and ${y} respectively")
                                        )
                                    )
                                ),
                                PatternConstruct.MakeCollectionPattern(
                                    false,
                                    PatternConstruct.MakeIdentifierPattern(
                                        "x",
                                        Helpers.MakePlaceholderType(),
                                        null
                                    ),
                                    PatternConstruct.MakeIdentifierPattern(
                                        "y",
                                        Helpers.MakePlaceholderType(),
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
                                            Helpers.MakeIdentifierPath("print"),
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
                                Helpers.MakeSomePatternWithType("tmp6")
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
                            Helpers.MakeIdentifierPath("tmp6"),
                            Statement.MakeMatchClause(
                                null,
                                Statement.MakeExprStmt(
                                    Expression.MakeSequenceExpression(
                                        Helpers.MakeCallExpression(
                                            Helpers.MakeIdentifierPath("println"),
                                            Expression.MakeConstant("string", "x is ${x}")
                                        )
                                    )
                                ),
                                PatternConstruct.MakeTuplePattern(
                                    PatternConstruct.MakeIdentifierPattern(
                                        "x",
                                        Helpers.MakePlaceholderType(),
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
                                            Helpers.MakeIdentifierPath("println"),
                                            Expression.MakeConstant("string", "x is ${x} and y is ${y}")
                                        )
                                    )
                                ),
                                PatternConstruct.MakeTuplePattern(
                                    PatternConstruct.MakeIdentifierPattern(
                                        "x",
                                        Helpers.MakePlaceholderType(),
                                        null
                                    ),
                                    PatternConstruct.MakeIdentifierPattern(
                                        "y",
                                        Helpers.MakePlaceholderType(),
                                        null
                                    )
                                )
                            )
                        )
                    ),
                    Helpers.MakePlaceholderType(),
                    Modifiers.None
                )
            });

            Assert.IsNotNull(ast);
            Helpers.AstStructuralEqual(ast, expected);
        }

        [Test]
        public void Functions()
        {
            var parser = new Parser(new Scanner("../../sources/for_unit_tests/functions.exs"));
            parser.Parse();

            var ast = parser.TopmostAst;

            var expected = AstNode.MakeModuleDef("main", new List<EntityDeclaration>{
                EntityDeclaration.MakeFunc(
                    "test",
                    Enumerable.Empty<ParameterDeclaration>(),
                    Statement.MakeBlock(
                        Statement.MakeVarDecl(
                            Helpers.MakeSeq(Helpers.MakeSomePatternWithType("a")),
                            Helpers.MakeSeq(Expression.MakeConstant("int", 10)),
                            Modifiers.Immutable
                        ),
                        Helpers.MakeSingleItemReturnStatement(
                            Expression.MakeBinaryExpr(
                                OperatorType.Plus,
                                Helpers.MakeIdentifierPath("a"),
                                Expression.MakeConstant("int", 10)
                            )
                        )
                    ),
                    Helpers.MakePlaceholderType(),
                    Modifiers.None
                ),
                EntityDeclaration.MakeFunc(
                    "test2",
                    Helpers.MakeSeq(
                        EntityDeclaration.MakeParameter("n", Helpers.MakePrimitiveType("int"))
                    ),
                    Statement.MakeBlock(
                        Helpers.MakeSingleItemReturnStatement(
                            Expression.MakeBinaryExpr(
                                OperatorType.Plus,
                                Helpers.MakeIdentifierPath("n"),
                                Expression.MakeConstant("int", 10)
                            )
                        )
                    ),
                    Helpers.MakePlaceholderType(),
                    Modifiers.None
                ),
                EntityDeclaration.MakeFunc(
                    "test3",
                    Helpers.MakeSeq(
                        EntityDeclaration.MakeParameter("n", Helpers.MakePrimitiveType("int"))
                    ),
                    Statement.MakeBlock(
                        Helpers.MakeSingleItemReturnStatement(
                            Expression.MakeBinaryExpr(
                                OperatorType.Plus,
                                Helpers.MakeIdentifierPath("n"),
                                Expression.MakeConstant("int", 20)
                            )
                        )
                    ),
                    AstType.MakePrimitiveType("int"),
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
                                    Helpers.MakeIdentifierPath("n"),
                                    Expression.MakeConstant("int", 100)
                                )
                            ),
                            Statement.MakeBlock(
                                Helpers.MakeSingleItemReturnStatement(
                                    Helpers.MakeIdentifierPath("n")
                                )
                            ),
                            Statement.MakeBlock(
                                Helpers.MakeSingleItemReturnStatement(
                                    Helpers.MakeCallExpression(
                                        Helpers.MakeIdentifierPath("test4"),
                                        Expression.MakeBinaryExpr(
                                            OperatorType.Plus,
                                            Helpers.MakeIdentifierPath("n"),
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
                            Helpers.MakeSomeIdent("n"),
                            Expression.MakeConstant("int", 100)
                        )
                    ),
                    Statement.MakeBlock(
                        Helpers.MakeSingleItemReturnStatement(
                            Helpers.MakeIdentifierPath("n")
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
                            Helpers.MakeSeq(Helpers.MakeSomePatternWithType("a")),
                            Helpers.MakeSeq(
                                Expression.MakeCallExpr(
                                    Helpers.MakeIdentifierPath("test"),
                                    Enumerable.Empty<Expression>()
                                )
                            ),
                            Modifiers.Immutable
                        ),
                        Statement.MakeVarDecl(
                            Helpers.MakeSeq(Helpers.MakeSomePatternWithType("b")),
                            Helpers.MakeSeq(
                                Helpers.MakeCallExpression(
                                    Helpers.MakeIdentifierPath("test2"),
                                    Expression.MakeConstant("int", 20)
                                )
                            ),
                            Modifiers.Immutable
                        ),
                        Statement.MakeVarDecl(
                            Helpers.MakeSeq(Helpers.MakeSomePatternWithType("c")),
                            Helpers.MakeSeq(
                                Helpers.MakeCallExpression(
                                    Helpers.MakeIdentifierPath("test3"),
                                    Expression.MakeConstant("int", 20)
                                )
                            ),
                            Modifiers.Immutable
                        ),
                        Statement.MakeVarDecl(
                            Helpers.MakeSeq(Helpers.MakeSomePatternWithType("d")),
                            Helpers.MakeSeq(
                                Helpers.MakeCallExpression(
                                    Helpers.MakeIdentifierPath("test4"),
                                    Expression.MakeConstant("int", 80)
                                )
                            ),
                            Modifiers.Immutable
                        ),
                        Statement.MakeVarDecl(
                            Helpers.MakeSeq(Helpers.MakeSomePatternWithType("e")),
                            Helpers.MakeSeq(
                                Helpers.MakeCallExpression(
                                    Helpers.MakeIdentifierPath("test5")
                                )
                            ),
                            Modifiers.Immutable
                        ),
                        Statement.MakeVarDecl(
                            Helpers.MakeSeq(Helpers.MakeSomePatternWithType("f")),
                            Helpers.MakeSeq(
                                Helpers.MakeCallExpression(
                                    Helpers.MakeIdentifierPath("test5"),
                                    Expression.MakeConstant("int", 90)
                                )
                            ),
                            Modifiers.Immutable
                        ),
                        Statement.MakeExprStmt(
                            Helpers.MakeCallExpression(
                                Helpers.MakeIdentifierPath("println"),
                                Expression.MakeConstant("string", "${a}, ${b}, ${c}, ${d}, ${e}, ${f}")
                            )
                        )
                    ),
                    Helpers.MakePlaceholderType(),
                    Modifiers.None
                )
            });

            Assert.IsNotNull(ast);

            Helpers.AstStructuralEqual(ast, expected);
        }

        [Test]
        public void ComplexExpressions()
        {
            var parser = new Parser(new Scanner("../../sources/for_unit_tests/complex_expressions.exs"));
            parser.Parse();

            var ast = parser.TopmostAst;

            var expected = AstNode.MakeModuleDef("main", new List<EntityDeclaration>{
                EntityDeclaration.MakeFunc(
                    "main",
                    Enumerable.Empty<ParameterDeclaration>(),
                    Statement.MakeBlock(
                        Statement.MakeVarDecl(
                            Helpers.MakeSeq(Helpers.MakeSomePatternWithType("x")),
                            Helpers.MakeSeq(
                                Expression.MakeComp(
                                    Helpers.MakeIdentifierPath("x"),
                                    Expression.MakeCompFor(
                                        Helpers.MakeSomePatternWithType("x"),
                                        Expression.MakeIntSeq(
                                            Expression.MakeConstant("int", 0),
                                            Expression.MakeConstant("int", 100),
                                            Expression.MakeConstant("int", 1),
                                            false
                                        ),
                                        null
                                    ),
                                    Helpers.MakeGenericType("vector", Helpers.MakePlaceholderType())
                                )
                            ),
                            Modifiers.Immutable
                        ),
                        Statement.MakeVarDecl(
                            Helpers.MakeSeq(Helpers.MakeSomePatternWithType("y")),
                            Helpers.MakeSeq(
                                Expression.MakeComp(
                                    Helpers.MakeIdentifierPath("x"),
                                    Expression.MakeCompFor(
                                        Helpers.MakeSomePatternWithType("x"),
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
                                    Helpers.MakeGenericType("vector", Helpers.MakePlaceholderType())
                                )
                            ),
                            Modifiers.Immutable
                        ),
                        Statement.MakeVarDecl(
                            Helpers.MakeSeq(Helpers.MakeSomePatternWithType("z")),
                            Helpers.MakeSeq(
                                Expression.MakeComp(
                                    Expression.MakeParen(
                                        Expression.MakeSequenceExpression(
                                            Helpers.MakeIdentifierPath("x"),
                                            Helpers.MakeIdentifierPath("y")
                                        )
                                    ),
                                    Expression.MakeCompFor(
                                        Helpers.MakeSomePatternWithType("x"),
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
                                                Helpers.MakeSomePatternWithType("y"),
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
                                    Helpers.MakeGenericType("vector", Helpers.MakePlaceholderType())
                                )
                            ),
                            Modifiers.Immutable
                        ),
                        Statement.MakeVarDecl(
                            Helpers.MakeSeq(Helpers.MakeSomePatternWithType("triangles")),
                            Helpers.MakeSeq(
                                Expression.MakeComp(
                                    Expression.MakeParen(
                                        Expression.MakeSequenceExpression(
                                            Helpers.MakeIdentifierPath("a"),
                                            Helpers.MakeIdentifierPath("b"),
                                            Helpers.MakeIdentifierPath("c")
                                        )
                                    ),
                                    Expression.MakeCompFor(
                                        Helpers.MakeSomePatternWithType("c"),
                                        Expression.MakeIntSeq(
                                            Expression.MakeConstant("int", 1),
                                            Expression.MakeConstant("int", 10),
                                            Expression.MakeConstant("int", 1),
                                            true
                                        ),
                                        Expression.MakeCompFor(
                                            Helpers.MakeSomePatternWithType("b"),
                                            Expression.MakeIntSeq(
                                                Expression.MakeConstant("int", 1),
                                                Helpers.MakeIdentifierPath("c"),
                                                Expression.MakeConstant("int", 1),
                                                true
                                            ),
                                            Expression.MakeCompFor(
                                                Helpers.MakeSomePatternWithType("a"),
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
                                    Helpers.MakeGenericType("vector", Helpers.MakePlaceholderType())
                                )
                            ),
                            Modifiers.Immutable
                        ),
                        Statement.MakeVarDecl(
                            Helpers.MakeSeq(Helpers.MakeSomePatternWithType("specific_triangles")),
                            Helpers.MakeSeq(
                                Expression.MakeComp(
                                    Expression.MakeParen(
                                        Expression.MakeSequenceExpression(
                                            Helpers.MakeIdentifierPath("a"),
                                            Helpers.MakeIdentifierPath("b"),
                                            Helpers.MakeIdentifierPath("c")
                                        )
                                    ),
                                    Expression.MakeCompFor(
                                        Helpers.MakeSomePatternWithType("c"),
                                        Expression.MakeIntSeq(
                                            Expression.MakeConstant("int", 1),
                                            Expression.MakeConstant("int", 10),
                                            Expression.MakeConstant("int", 1),
                                            true
                                        ),
                                        Expression.MakeCompFor(
                                            Helpers.MakeSomePatternWithType("b"),
                                            Expression.MakeIntSeq(
                                                Expression.MakeConstant("int", 1),
                                                Helpers.MakeIdentifierPath("c"),
                                                Expression.MakeConstant("int", 1),
                                                true
                                            ),
                                            Expression.MakeCompFor(
                                                Helpers.MakeSomePatternWithType("a"),
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
                                    Helpers.MakeGenericType("vector", Helpers.MakePlaceholderType())
                                )
                            ),
                            Modifiers.Immutable
                        ),
                        Helpers.MakeVariableDeclaration(
                            Helpers.MakeSeq(
                                Helpers.MakeSomePatternWithType("a", AstType.MakePrimitiveType("int")),
                                Helpers.MakeSomePatternWithType("b", AstType.MakePrimitiveType("int")),
                                Helpers.MakeSomePatternWithType("c", AstType.MakePrimitiveType("int"))
                            ),
                            Modifiers.None
                        ),
                        Statement.MakeExprStmt(
                            Expression.MakeMultipleAssignment(
                                Expression.MakeMultipleAssignment(
                                    Expression.MakeSingleAssignment(
                                        Helpers.MakeIdentifierPath("a"),
                                        Helpers.MakeIdentifierPath("b")
                                    ),
                                    Expression.MakeSequenceExpression(Helpers.MakeIdentifierPath("c"))
                                ),
                                Expression.MakeSequenceExpression(Expression.MakeConstant("int", 0))
                            )
                        ),
                        Statement.MakeExprStmt(
                            Expression.MakeAssignment(
                                Expression.MakeSequenceExpression(
                                    Helpers.MakeIdentifierPath("a"),
                                    Helpers.MakeIdentifierPath("b"),
                                    Helpers.MakeIdentifierPath("c")
                                ),
                                Expression.MakeSequenceExpression(
                                    Expression.MakeConstant("int", 1),
                                    Expression.MakeConstant("int", 2),
                                    Expression.MakeConstant("int", 3)
                                )
                            )
                        ),
                        Statement.MakeVarDecl(
                            Helpers.MakeSeq(
                                Helpers.MakeSomePatternWithType(
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
                                    Helpers.MakeGenericType("vector", Helpers.MakePlaceholderType()),
                                    Enumerable.Empty<Expression>()
                                )
                            ),
                            Modifiers.None
                        ),
                        Statement.MakeVarDecl(
                            Helpers.MakeSeq(Helpers.MakeSomePatternWithType("t")),
                            Helpers.MakeSeq(
                                Expression.MakeParen(
                                    Expression.MakeSequenceExpression(
                                        Helpers.MakeIdentifierPath("a"),
                                        Helpers.MakeIdentifierPath("b"),
                                        Helpers.MakeIdentifierPath("c")
                                    )
                                )
                            ),
                            Modifiers.Immutable
                        ),
                        Statement.MakeExprStmt(
                            Helpers.MakeCallExpression(
                                Expression.MakeMemRef(
                                    Helpers.MakeIdentifierPath("vec"),
                                    Helpers.MakeSomeIdent("add")
                                ),
                                Expression.MakeParen(
                                    Expression.MakeSequenceExpression(
                                        Helpers.MakeIdentifierPath("a"),
                                        Helpers.MakeIdentifierPath("b")
                                    )
                                )
                            )
                        ),
                        Statement.MakeExprStmt(
                            Helpers.MakeCallExpression(
                                Helpers.MakeIdentifierPath("println"),
                                Expression.MakeConstant("string", "${x}, ${y}, ${z}, ${triangles}, ${specific_triangles}, ${a}, ${b}, ${c}")
                            )
                        )
                    ),
                    Helpers.MakePlaceholderType(),
                    Modifiers.None
                )
            });

            Assert.IsNotNull(ast);
            Helpers.AstStructuralEqual(ast, expected);
        }

        [Test]
        public void TestModule()
        {
            var parser = new Parser(new Scanner("../../sources/for_unit_tests/test_module.exs"));
            parser.Parse();

            var ast = parser.TopmostAst;

            var expected_ast = AstNode.MakeModuleDef("test_module", new List<EntityDeclaration>{
                EntityDeclaration.MakeClassDecl(
                    "TestClass3",
                    Enumerable.Empty<AstType>(),
                    Helpers.MakeSeq<EntityDeclaration>(
                        EntityDeclaration.MakeField(
                            Helpers.MakeSeq(
                                Helpers.MakeSomePatternWithType("x", Helpers.MakePrimitiveType("int")),
                                Helpers.MakeSomePatternWithType("y", Helpers.MakePrimitiveType("int"))
                            ),
                            null,
                            Modifiers.Private | Modifiers.Immutable
                        ),
                        EntityDeclaration.MakeFunc(
                            "getX",
                            Enumerable.Empty<ParameterDeclaration>(),
                            Statement.MakeBlock(
                                Helpers.MakeSingleItemReturnStatement(
                                    Expression.MakeMemRef(
                                        Expression.MakeSelfRef(),
                                        Helpers.MakeSomeIdent("x")
                                    )
                                )
                            ),
                            Helpers.MakePlaceholderType(),
                            Modifiers.Public
                        ),
                        EntityDeclaration.MakeFunc(
                            "getY",
                            Enumerable.Empty<ParameterDeclaration>(),
                            Statement.MakeBlock(
                                Helpers.MakeSingleItemReturnStatement(
                                    Expression.MakeMemRef(
                                        Expression.MakeSelfRef(),
                                        Helpers.MakeSomeIdent("y")
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
                        Helpers.MakeSomePatternWithType("pair")
                    ),
                    Helpers.MakeSeq(
                        Expression.MakeParen(
                            Expression.MakeSequenceExpression(
                                Expression.MakeConstant("int", 100),
                                Expression.MakeConstant("int", 200)
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
                                    Helpers.MakeIdentifierPath("x"),
                                    Helpers.MakeIdentifierPath("y")
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
                                    Helpers.MakeIdentifierPath("Math"),
                                    Helpers.MakeSomeIdent("Sin")
                                ),
                                Helpers.MakeIdentifierPath("x")
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
            parser.Parse();

            var ast = parser.TopmostAst;

            var expected_ast = AstNode.MakeModuleDef("main", new List<EntityDeclaration>{
                EntityDeclaration.MakeFunc(
                    "main",
                    Enumerable.Empty<ParameterDeclaration>(),
                    Statement.MakeBlock(
                        Statement.MakeVarDecl(
                            Helpers.MakeSeq(
                                Helpers.MakeSomePatternWithType("a")
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
                                Helpers.MakeSomePatternWithType("b")
                            ),
                            Helpers.MakeSeq(
                                Helpers.MakeCallExpression(
                                    Expression.MakePath(
                                        Helpers.MakeSomeIdent("TestModule"),
                                        Helpers.MakeSomeIdent("createTest")
                                    ),
                                    Expression.MakeConstant("int", 50),
                                    Expression.MakeConstant("int", 100)
                                )
                            ),
                            Modifiers.Immutable
                        ),
                        Statement.MakeVarDecl(
                            Helpers.MakeSeq(
                                Helpers.MakeSomePatternWithType("c")
                            ),
                            Helpers.MakeSeq(
                                Expression.MakePath(
                                    Helpers.MakeSomeIdent("TestModule"),
                                    Helpers.MakeSomeIdent("pair")
                                )
                            ),
                            Modifiers.Immutable
                        ),
                        Statement.MakeVarDecl(
                            Helpers.MakeSeq(
                                Helpers.MakeSomePatternWithType("d")
                            ),
                            Helpers.MakeSeq(
                                Helpers.MakeCallExpression(
                                    Expression.MakePath(
                                        Helpers.MakeSomeIdent("TestModule"),
                                        Helpers.MakeSomeIdent("mySin")
                                    ),
                                    Expression.MakeConstant("double", 0.0)
                                )
                            ),
                            Modifiers.Immutable
                        ),
                        Statement.MakeVarDecl(
                            Helpers.MakeSeq(
                                Helpers.MakeSomePatternWithType("e")
                            ),
                            Helpers.MakeSeq(
                                Helpers.MakeCallExpression(
                                    Expression.MakeMemRef(
                                        Helpers.MakeIdentifierPath("a"),
                                        Helpers.MakeSomeIdent("getX")
                                    )
                                )
                            ),
                            Modifiers.Immutable
                        ),
                        Statement.MakeExprStmt(
                            Helpers.MakeCallExpression(
                                Helpers.MakeIdentifierPath("println"),
                                Expression.MakeConstant("string", "${a}, ${b}, ${c}, ${d}, ${e}")
                            )
                        )
                    ),
                    Helpers.MakePlaceholderType(),
                    Modifiers.None
                )
            }, Helpers.MakeSeq(
                AstNode.MakeImportDecl(AstNode.MakeIdentifier("test_module"), "TestModule", "./test_module.exs")
            ));

            Assert.IsNotNull(ast);

            Helpers.AstStructuralEqual(ast, expected_ast);
        }

        [Test]
        public void Class()
        {
            var parser = new Parser(new Scanner("../../sources/for_unit_tests/class.exs"));
            parser.Parse();

            var ast = parser.TopmostAst;

            var expected_ast = AstNode.MakeModuleDef("main", new List<EntityDeclaration>{
                EntityDeclaration.MakeClassDecl(
                    "TestClass",
                    Enumerable.Empty<AstType>(),
                    Helpers.MakeSeq<EntityDeclaration>(
                        EntityDeclaration.MakeField(
                            Helpers.MakeSeq(
                                Helpers.MakeSomePatternWithType("x", Helpers.MakePrimitiveType("int"))
                            ),
                            Helpers.MakeSeq<Expression>(
                                Expression.Null
                            ),
                            Modifiers.Public | Modifiers.Immutable
                        ),
                        EntityDeclaration.MakeField(
                            Helpers.MakeSeq(
                                Helpers.MakeSomePatternWithType("y", Helpers.MakePrimitiveType("int")),
                                Helpers.MakeSomePatternWithType("z", Helpers.MakePrimitiveType("int"))
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
                                        Helpers.MakeSomeIdent("x")
                                    )
                                )
                            ),
                            Helpers.MakePlaceholderType(),
                            Modifiers.Public
                        ),
                        EntityDeclaration.MakeFunc(
                            "getY",
                            Enumerable.Empty<ParameterDeclaration>(),
                            Statement.MakeBlock(
                                Helpers.MakeSingleItemReturnStatement(
                                    Expression.MakeMemRef(
                                        Expression.MakeSelfRef(),
                                        Helpers.MakeSomeIdent("y")
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
                                                Helpers.MakeSomeIdent("getX")
	                                        )
                                        ),
                                        Helpers.MakeIdentifierPath("n")
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
                                        Helpers.MakeSomeIdent("z")
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
                                Helpers.MakeSomePatternWithType("a")
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
                                Helpers.MakeSomeIdent("b")
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
                                Helpers.MakeSomePatternWithType("c")
                            ),
                            Helpers.MakeSeq(
                                Helpers.MakeCallExpression(
                                    Expression.MakeMemRef(
                                        Helpers.MakeIdentifierPath("a"),
                                        Helpers.MakeSomeIdent("getX")
                                    )
                                )
                            ),
                            Modifiers.Immutable
                        ),
                        Statement.MakeVarDecl(
                            Helpers.MakeSeq(
                                Helpers.MakeSomePatternWithType("d")
                            ),
                            Helpers.MakeSeq(
                                Helpers.MakeCallExpression(
                                    Expression.MakeMemRef(
                                        Helpers.MakeIdentifierPath("a"),
                                        Helpers.MakeSomeIdent("getY")
                                    )
                                )
                            ),
                            Modifiers.Immutable
                        ),
                        Statement.MakeVarDecl(
                            Helpers.MakeSeq(
                                Helpers.MakeSomePatternWithType("e")
                            ),
                            Helpers.MakeSeq(
                                Helpers.MakeCallExpression(
                                    Expression.MakeMemRef(
                                        Helpers.MakeIdentifierPath("a"),
                                        Helpers.MakeSomeIdent("getXPlus")
                                    ),
                                    Expression.MakeConstant("int", 100)
                                )
                            ),
                            Modifiers.Immutable
                        ),
                        Statement.MakeVarDecl(
                            Helpers.MakeSeq(
                                Helpers.MakeSomePatternWithType("f")
                            ),
                            Helpers.MakeSeq(
                                Helpers.MakeCallExpression(
                                    Expression.MakeMemRef(
                                        Helpers.MakeIdentifierPath("a"),
                                        Helpers.MakeSomeIdent("getZ")
                                    )
                                )
                            ),
                            Modifiers.Immutable
                        ),
                        Statement.MakeVarDecl(
                            Helpers.MakeSeq(
                                Helpers.MakeSomePatternWithType("g")
                            ),
                            Helpers.MakeSeq(
                                Expression.MakeMemRef(
                                    Helpers.MakeIdentifierPath("a"),
                                    Helpers.MakeSomeIdent("x")
                                )
                            ),
                            Modifiers.Immutable
                        ),
                        Statement.MakeExprStmt(
                            Helpers.MakeCallExpression(
                                Helpers.MakeIdentifierPath("println"),
                                Expression.MakeConstant("string", "(a.x, a.y, a.z, x) = (${c}, ${d}, ${f}, ${g})")
                            )
                        )
                    ),
                    Helpers.MakePlaceholderType(),
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
                                Helpers.MakeIdentifierPath("addOne"),
                                Expression.MakeConstant("int", 1)
                            )
                        )
                    ),
                    Helpers.MakePlaceholderType(),
                    Modifiers.None
                ),
                EntityDeclaration.MakeFunc(
                    "main",
                    Enumerable.Empty<ParameterDeclaration>(),
                    Statement.MakeBlock(
                        Statement.MakeVarDecl(
                            Helpers.MakeSeq(
                                Helpers.MakeSomePatternWithType("c")
                            ),
                            Helpers.MakeSeq<Expression>(
                                Expression.MakeClosureExpression(
                                    Helpers.MakePlaceholderType(),
                                    Statement.MakeBlock(
                                        Helpers.MakeSingleItemReturnStatement(
                                            Expression.MakeBinaryExpr(
                                                OperatorType.Plus,
                                                Helpers.MakeIdentifierPath("x"),
                                                Expression.MakeConstant("int", 1)
                                            )
                                        )
                                    ),
                                    liftedIdentifiers: null,
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
                                Helpers.MakeSomePatternWithType("c2")
                            ),
                            Helpers.MakeSeq<Expression>(
                                Expression.MakeClosureExpression(
                                    Helpers.MakePlaceholderType(),
                                    Statement.MakeBlock(
                                        Statement.MakeVarDecl(
                                            Helpers.MakeSeq(
                                                Helpers.MakeSomePatternWithType("y")
                                            ),
                                            Helpers.MakeSeq(
                                                Expression.MakeConstant("int", 1)
                                            ),
                                            Modifiers.Immutable
                                        ),
                                        Helpers.MakeSingleItemReturnStatement(
                                            Expression.MakeBinaryExpr(
                                                OperatorType.Plus,
                                                Helpers.MakeIdentifierPath("x"),
                                                Helpers.MakeIdentifierPath("y")
                                            )
                                        )
                                    ),
                                    liftedIdentifiers: null,
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
                                Helpers.MakeSomePatternWithType("a")
                            ),
                            Helpers.MakeSeq(
                                Helpers.MakeCallExpression(
                                    Helpers.MakeIdentifierPath("c"),
                                    Expression.MakeConstant("int", 1)
                                )
                            ),
                            Modifiers.Immutable
                        ),
                        Statement.MakeVarDecl(
                            Helpers.MakeSeq(
                                Helpers.MakeSomePatternWithType("b")
                            ),
                            Helpers.MakeSeq(
                                Helpers.MakeCallExpression(
                                    Helpers.MakeIdentifierPath("c2"),
                                    Expression.MakeConstant("int", 1)
                                )
                            ),
                            Modifiers.Immutable
                        ),
                        Statement.MakeVarDecl(
                            Helpers.MakeSeq(
                                Helpers.MakeSomePatternWithType("d")
                            ),
                            Helpers.MakeSeq(
                                Helpers.MakeCallExpression(
                                    Helpers.MakeIdentifierPath("addOneToOne"),
                                    Expression.MakeClosureExpression(
                                        Helpers.MakePlaceholderType(),
                                        Statement.MakeBlock(
                                            Helpers.MakeSingleItemReturnStatement(
                                                Expression.MakeBinaryExpr(
                                                    OperatorType.Plus,
                                                    Helpers.MakeIdentifierPath("x"),
                                                    Expression.MakeConstant("int", 1)
                                                )
                                            )
                                        ),
                                        liftedIdentifiers: null,
                                        parameters: EntityDeclaration.MakeParameter(
                                            "x",
                                            Helpers.MakePlaceholderType()
                                        )
                                    )
                                )
                            ),
                            Modifiers.Immutable
                        ),
                        Statement.MakeVarDecl(
                            Helpers.MakeSeq(
                                Helpers.MakeSomePatternWithType("e")
                            ),
                            Helpers.MakeSeq(
                                Expression.MakeConstant("int", 2)
                            ),
                            Modifiers.Immutable
                        ),
                        Statement.MakeVarDecl(
                            Helpers.MakeSeq(
                                Helpers.MakeSomePatternWithType("c3")
                            ),
                            Helpers.MakeSeq<Expression>(
                                Expression.MakeClosureExpression(
                                    Helpers.MakePlaceholderType(),
                                    Statement.MakeBlock(
                                        Helpers.MakeSingleItemReturnStatement(
                                            Expression.MakeBinaryExpr(
                                                OperatorType.Plus,
                                                Helpers.MakeIdentifierPath("x"),
                                                Helpers.MakeIdentifierPath("e")
                                            )
                                        )
                                    ),
                                    liftedIdentifiers: null,
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
                                Helpers.MakeSomePatternWithType("f")
                            ),
                            Helpers.MakeSeq(
                                Helpers.MakeCallExpression(
                                    Helpers.MakeIdentifierPath("c3"),
                                    Expression.MakeConstant("int", 1)
                                )
                            ),
                            Modifiers.Immutable
                        ),
                        Statement.MakeExprStmt(
                            Helpers.MakeCallExpression(
                                Helpers.MakeIdentifierPath("println"),
                                Expression.MakeConstant("string", "${a}, ${b}, ${d}, ${f}")
                            )
                        )
                    ),
                    Helpers.MakePlaceholderType(),
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
            parser.Parse();

            var ast = parser.TopmostAst;

            var expected = AstNode.MakeModuleDef("main", new List<EntityDeclaration>{
                EntityDeclaration.MakeFunc(
                    "main",
                    Enumerable.Empty<ParameterDeclaration>(),
                    Statement.MakeBlock(
                        Statement.MakeVarDecl(
                            Helpers.MakeSeq(
                                Helpers.MakeSomePatternWithType("c")
                            ),
                            Helpers.MakeSeq<Expression>(
                                Expression.MakeClosureExpression(
                                    Helpers.MakePlaceholderType(),
                                    Statement.MakeBlock(
                                        Statement.MakeIfStmt(
                                            PatternConstruct.MakeExpressionPattern(Helpers.MakeIdentifierPath("f")),
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
                                    liftedIdentifiers: null,
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
                                Helpers.MakeSomePatternWithType("a")
                            ),
                            Helpers.MakeSeq(
                                Helpers.MakeCallExpression(
                                    Helpers.MakeIdentifierPath("c"),
                                    Expression.MakeConstant("bool", true)
                                )
                            ),
                            Modifiers.Immutable
                        ),
                        Statement.MakeVarDecl(
                            Helpers.MakeSeq(
                                Helpers.MakeSomePatternWithType("c2")
                            ),
                            Helpers.MakeSeq(
                                Expression.MakeClosureExpression(
                                    Helpers.MakePlaceholderType(),
                                    Statement.MakeBlock(
                                        Statement.MakeVarDecl(
                                            Helpers.MakeSeq(
                                                Helpers.MakeSomePatternWithType("result")
                                            ),
                                            Helpers.MakeSeq(
                                                Expression.MakeConstant("int", 0)
                                            ),
                                            Modifiers.None
                                        ),
                                        Statement.MakeValueBindingForStmt(
                                            Modifiers.Immutable,
                                            Helpers.MakeSomePatternWithType("j"),
                                            Expression.MakeIntSeq(
                                                Expression.MakeConstant("int", 0),
                                                Helpers.MakeIdentifierPath("i"),
                                                Expression.MakeConstant("int", 1),
                                                false
                                            ),
                                            Statement.MakeBlock(
                                                Statement.MakeExprStmt(
                                                    Expression.MakeAugumentedAssignment(
                                                        OperatorType.Plus,
                                                        Expression.MakeSequenceExpression(Helpers.MakeIdentifierPath("result")),
                                                        Expression.MakeSequenceExpression(Helpers.MakeIdentifierPath("j"))
                                                    )
                                                )
                                            )
                                        ),
                                        Helpers.MakeSingleItemReturnStatement(
                                            Helpers.MakeIdentifierPath("result")
                                        )
                                    ),
                                    liftedIdentifiers: null,
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
                                Helpers.MakeSomePatternWithType("b")
                            ),
                            Helpers.MakeSeq(
                                Helpers.MakeCallExpression(
                                    Helpers.MakeIdentifierPath("c2"),
                                    Expression.MakeConstant("int", 10)
                                )
                            ),
                            Modifiers.Immutable
                        ),
                        Statement.MakeVarDecl(
                            Helpers.MakeSeq(
                                Helpers.MakeSomePatternWithType("c3")
                            ),
                            Helpers.MakeSeq(
                                Expression.MakeClosureExpression(
                                    Helpers.MakePlaceholderType(),
                                    Statement.MakeBlock(
                                        Statement.MakeVarDecl(
                                            Helpers.MakeSeq(
                                                Helpers.MakeSomePatternWithType("j")
                                            ),
                                            Helpers.MakeSeq(
                                                Helpers.MakeIdentifierPath("i")
                                            ),
                                            Modifiers.None
                                        ),
                                        Statement.MakeWhileStmt(
                                            Expression.MakeBinaryExpr(
                                                OperatorType.GreaterThan,
                                                Helpers.MakeIdentifierPath("j"),
                                                Expression.MakeConstant("int", 0)
                                            ),
                                            Statement.MakeBlock(
                                                Statement.MakeExprStmt(
                                                    Helpers.MakeCallExpression(
                                                        Helpers.MakeIdentifierPath("println"),
                                                        Expression.MakeConstant("string", "${j}")
                                                    )
                                                ),
                                                Statement.MakeExprStmt(
                                                    Expression.MakeSingleAugmentedAssignment(
                                                        OperatorType.Minus,
                                                        Helpers.MakeIdentifierPath("j"),
                                                        Expression.MakeConstant("int", 1)
                                                    )
                                                )
                                            )
                                        ),
                                        Statement.MakeExprStmt(
                                            Helpers.MakeCallExpression(
                                                Helpers.MakeIdentifierPath("println"),
                                                Expression.MakeConstant("string", "BOOM!")
                                            )
                                        )
                                    ),
                                    liftedIdentifiers: null,
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
                                Helpers.MakeIdentifierPath("c3"),
                                Expression.MakeConstant("int", 3)
                            )
                        ),
                        Statement.MakeExprStmt(
                            Helpers.MakeCallExpression(
                                Helpers.MakeIdentifierPath("println"),
                                Expression.MakeConstant("string", "${a}, ${b}")
                            )
                        )
                    ),
                    Helpers.MakePlaceholderType(),
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
            parser.Parse();

            var ast = parser.TopmostAst;

            var expected = AstNode.MakeModuleDef("main", new List<EntityDeclaration>{
                EntityDeclaration.MakeFunc(
                    "throwException",
                    Enumerable.Empty<ParameterDeclaration>(),
                    Statement.MakeBlock(
                        Statement.MakeThrowStmt(
                            Expression.MakeObjectCreation(
                                Helpers.MakeGenericType(
                                    "Exception"
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
                    Helpers.MakePlaceholderType(),
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
                                        Helpers.MakeIdentifierPath("println"),
                                        Expression.MakeConstant("string", "First try block")
                                    )
                                ),
                                Statement.MakeExprStmt(
                                    Helpers.MakeCallExpression(
                                        Helpers.MakeIdentifierPath("throwException")
                                    )
                                )
                            ),
                            null,
                            Statement.MakeCatchClause(
                                AstNode.MakeIdentifier(
                                    "e",
                                    Helpers.MakeGenericType("Exception")
                                ),
                                Statement.MakeBlock(
                                    Statement.MakeExprStmt(
                                        Helpers.MakeCallExpression(
                                            Helpers.MakeIdentifierPath("println"),
                                            Expression.MakeConstant("string", "First catch block")
                                        )
                                    ),
                                    Statement.MakeExprStmt(
                                        Helpers.MakeCallExpression(
                                            Helpers.MakeIdentifierPath("println"),
                                            Expression.MakeMemRef(
                                                Helpers.MakeIdentifierPath("e"),
                                                Helpers.MakeSomeIdent("Message")
                                            )
                                        )
                                    )
                                )
                            )
                        ),
                        /*Statement.MakeVarDecl(
                            Helpers.MakeSeq(
                                Helpers.MakeSomeIdent("tmp")
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
                                        Helpers.MakeIdentifierPath("println"),
                                        Expression.MakeConstant("string", "tmp is ${tmp} at first")
                                    )
                                ),
                                Statement.MakeExprStmt(
                                    Helpers.MakeCallExpression(
                                        Helpers.MakeIdentifierPath("throwException")
                                    )
                                )
                            ),
                            null,
                            Statement.MakeFinallyClause(
                                Statement.MakeBlock(
                                    Statement.MakeExprStmt(
                                        Expression.MakeSingleAssignment(
                                            Helpers.MakeIdentifierPath("tmp"), 
                                            Expression.MakeConstant("int", 2)
                                        )
                                    )
                                )
                            )
                        ),
                        Statement.MakeExprStmt(
                            Helpers.MakeCallExpression(
                                Helpers.MakeIdentifierPath("println"),
                                Expression.MakeConstant("string", "tmp is ${tmp} at last")
                            )
                        ),*/
                        Statement.MakeVarDecl(
                            Helpers.MakeSeq(
                                Helpers.MakeSomePatternWithType("tmp2")
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
                                        Helpers.MakeIdentifierPath("println"),
                                        Expression.MakeConstant("string", "tmp2 is ${tmp2} at first")
                                    )
                                ),
                                Statement.MakeExprStmt(
                                    Helpers.MakeCallExpression(
                                        Helpers.MakeIdentifierPath("throwException")
                                    )
                                )
                            ),
                            Statement.MakeFinallyClause(
                                Statement.MakeBlock(
                                    Statement.MakeExprStmt(
                                        Helpers.MakeCallExpression(
                                            Helpers.MakeIdentifierPath("println"),
                                            Expression.MakeConstant("string", "Second finally block")
                                        )
                                    ),
                                    Statement.MakeExprStmt(
                                        Expression.MakeSingleAssignment(
                                            Helpers.MakeIdentifierPath("tmp2"), 
                                            Expression.MakeConstant("int", 3)
                                        )
                                    )
                                )
                            ),
                            Statement.MakeCatchClause(
                                AstNode.MakeIdentifier(
                                    "e",
                                    Helpers.MakeGenericType("Exception")
                                ),
                                Statement.MakeBlock(
                                    Statement.MakeExprStmt(
                                        Helpers.MakeCallExpression(
                                            Helpers.MakeIdentifierPath("println"),
                                            Expression.MakeConstant("string", "Second catch block")
                                        )
                                    ),
                                    Statement.MakeExprStmt(
                                        Expression.MakeSingleAssignment(
                                            Helpers.MakeIdentifierPath("tmp2"),
                                            Expression.MakeConstant("int", 2)
                                        )
                                    ),
                                    Statement.MakeExprStmt(
                                        Helpers.MakeCallExpression(
                                            Helpers.MakeIdentifierPath("println"),
                                            Expression.MakeMemRef(
                                                Helpers.MakeIdentifierPath("e"),
                                                Helpers.MakeSomeIdent("Message")
                                            )
                                        )
                                    )
                                )
                            )
                        ),
                        Statement.MakeExprStmt(
                            Helpers.MakeCallExpression(
                                Helpers.MakeIdentifierPath("println"),
                                Expression.MakeConstant("string", "tmp2 is ${tmp2} at last")
                            )
                        )
                    ),
                    Helpers.MakePlaceholderType(),
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
            parser.Parse();

            var ast = parser.TopmostAst;

            var expected = AstNode.MakeModuleDef("main", new List<EntityDeclaration>{
                EntityDeclaration.MakeFunc(
                    "main",
                    Enumerable.Empty<ParameterDeclaration>(),
                    Statement.MakeBlock(
                        Statement.MakeVarDecl(
                            Helpers.MakeSeq(
                                Helpers.MakeSomePatternWithType("a")
                            ),
                            Helpers.MakeSeq(
                                Helpers.MakeSequenceInitializer(
                                    Helpers.MakeGenericType(
                                        "array",
                                        Helpers.MakePlaceholderType()
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
                                Helpers.MakeSomePatternWithType("b")
                            ),
                            Helpers.MakeSeq(
                                Helpers.MakeSequenceInitializer(
                                    Helpers.MakeGenericType(
                                        "dictionary",
                                        Helpers.MakePlaceholderType(),
                                        Helpers.MakePlaceholderType()
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
                                Helpers.MakeSomePatternWithType("c")
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
                                Helpers.MakeSomePatternWithType("d")
                            ),
                            Helpers.MakeSeq(
                                Helpers.MakeIndexer(
                                    Helpers.MakeIdentifierPath("a"),
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
                                Helpers.MakeSomePatternWithType("e")
                            ),
                            Helpers.MakeSeq(
                                Helpers.MakeSequenceInitializer(
                                    Helpers.MakeGenericType(
                                        "array",
                                        Helpers.MakePlaceholderType()
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
                                Helpers.MakeSomePatternWithType("f")
                            ),
                            Helpers.MakeSeq(
                                Helpers.MakeSequenceInitializer(
                                    Helpers.MakeGenericType(
                                        "vector",
                                        Helpers.MakePlaceholderType()
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
                                Helpers.MakeSomePatternWithType("g")
                            ),
                            Helpers.MakeSeq(
                                Helpers.MakeSequenceInitializer(
                                    Helpers.MakeGenericType(
                                        "array",
                                        Helpers.MakePlaceholderType()
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
                                Helpers.MakeSomePatternWithType("h")
                            ),
                            Helpers.MakeSeq(
                                Helpers.MakeSequenceInitializer(
                                    Helpers.MakeGenericType(
                                        "array",
                                        Helpers.MakePlaceholderType()
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
                                Helpers.MakeSomePatternWithType("i")
                            ),
                            Helpers.MakeSeq(
                                Helpers.MakeSequenceInitializer(
                                    Helpers.MakeGenericType(
                                        "array",
                                        Helpers.MakePlaceholderType()
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
                                Helpers.MakeSomePatternWithType(
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
                                        Helpers.MakePlaceholderType()
                                    ),
                                    Enumerable.Empty<Expression>()
                                )
                            ),
                            Modifiers.None
                        ),
                        Statement.MakeValueBindingForStmt(
                            Modifiers.Immutable,
                            Helpers.MakeSomePatternWithType("x"),
                            Helpers.MakeIdentifierPath("d"),
                            Statement.MakeBlock(
                                Statement.MakeExprStmt(
                                    Helpers.MakeCallExpression(
                                        Helpers.MakeIdentifierPath("print"),
                                        Expression.MakeConstant("string", "${x}")
                                    )
                                ),
                                Statement.MakeExprStmt(
                                    Helpers.MakeCallExpression(
                                        Expression.MakeMemRef(
                                            Helpers.MakeIdentifierPath("y"),
                                            Helpers.MakeSomeIdent("Add")
                                        ),
                                        Helpers.MakeIdentifierPath("x")
                                    )
                                )
                            )
                        ),
                        Statement.MakeExprStmt(
                            Helpers.MakeCallExpression(
                                Helpers.MakeIdentifierPath("println"),
                                Expression.MakeConstant("string", "${a}, ${b}, ${c}, ${d}, ${e}, ${f}, ${g}")
                            )
                        ),
                        Statement.MakeExprStmt(
                            Helpers.MakeCallExpression(
                                Helpers.MakeIdentifierPath("println"),
                                Expression.MakeConstant("string", "${h}, ${i}, ${y}")
                            )
                        )
                    ),
                    Helpers.MakePlaceholderType(),
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
            parser.Parse();

            var ast = parser.TopmostAst;

            var expected = AstNode.MakeModuleDef("main", new List<EntityDeclaration>{
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
                            Helpers.MakeSomePatternWithType(
                                "x",
                                Helpers.MakePrimitiveType("int")
                            )
                        ),
                        Helpers.MakeSeq(
                            Expression.Null
                        ),
                        Modifiers.Private | Modifiers.Immutable
                    ),
                    EntityDeclaration.MakeFunc(
                        "doSomeBehavior",
                        Enumerable.Empty<ParameterDeclaration>(),
                        Statement.MakeBlock(
                            Helpers.MakeSingleItemReturnStatement(
                                Expression.MakeMemRef(
                                    Expression.MakeSelfRef(),
                                    Helpers.MakeSomeIdent("x")
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
                                Helpers.MakeSomePatternWithType("t")
                            ),
                            Helpers.MakeSeq(
                                Expression.MakeObjectCreation(
                                    Helpers.MakeGenericType("TestClass2"),
                                    Helpers.MakeSeq(
                                        AstNode.MakeIdentifier("x")
                                    ),
                                    Helpers.MakeSeq<Expression>(
                                        Expression.MakeConstant("int", 1)
                                    )
                                )
                            ),
                            Modifiers.Immutable
                        ),
                        Statement.MakeVarDecl(
                            Helpers.MakeSeq(
                                Helpers.MakeSomePatternWithType("a")
                            ),
                            Helpers.MakeSeq(
                                Helpers.MakeCallExpression(
                                    Expression.MakeMemRef(
                                        Helpers.MakeIdentifierPath("t"),
                                        Helpers.MakeSomeIdent("doSomeBehavior")
                                    )
                                )
                            ),
                            Modifiers.Immutable
                        ),
                        Statement.MakeExprStmt(
                            Helpers.MakeCallExpression(
                                Helpers.MakeIdentifierPath("println"),
                                Expression.MakeConstant("string", "t.x = ${a}")
                            )
                        )
                    ),
                    Helpers.MakePlaceholderType(),
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
            parser.Parse();

            var ast = parser.TopmostAst;

            var expected = AstNode.MakeModuleDef("main", new List<EntityDeclaration>{
                EntityDeclaration.MakeFunc(
                    "main",
                    Enumerable.Empty<ParameterDeclaration>(),
                    Statement.MakeBlock(
                        Statement.MakeVarDecl(
                            Helpers.MakeSeq(
                                Helpers.MakeSomePatternWithType("a")
                            ),
                            Helpers.MakeSeq(
                                Expression.MakeSequenceInitializer(
                                    Helpers.MakeGenericType(
                                        "array",
                                        Helpers.MakePlaceholderType()
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
                                Helpers.MakeSomePatternWithType("b")
                            ),
                            Helpers.MakeSeq(
                                Helpers.MakeIndexer(
                                    Helpers.MakeIdentifierPath("a"),
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
                                Helpers.MakeSomePatternWithType("c")
                            ),
                            Helpers.MakeSeq(
                                Expression.MakeSequenceInitializer(
                                    Helpers.MakeGenericType(
                                        "vector",
                                        Helpers.MakePlaceholderType()
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
                                Helpers.MakeSomePatternWithType("d")
                            ),
                            Helpers.MakeSeq(
                                Helpers.MakeIndexer(
                                    Helpers.MakeIdentifierPath("c"),
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
                                Helpers.MakeIdentifierPath("println"),
                                Expression.MakeConstant("string", "${b}, ${d}")
                            )
                        )
                    ),
                    Helpers.MakePlaceholderType(),
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
            parser.Parse();

            var ast = parser.TopmostAst;

            var expected = AstNode.MakeModuleDef("main", new List<EntityDeclaration>{
                EntityDeclaration.MakeFunc(
                    "main",
                    Enumerable.Empty<ParameterDeclaration>(),
                    Statement.MakeBlock(
                        Statement.MakeVarDecl(
                            Helpers.MakeSeq(
                                Helpers.MakeSomePatternWithType("a")
                            ),
                            Helpers.MakeSeq(
                                Expression.MakeConstant("int", 10)
                            ),
                            Modifiers.Immutable
                        ),
                        Statement.MakeVarDecl(
                            Helpers.MakeSeq(
                                Helpers.MakeSomePatternWithType("b")
                            ),
                            Helpers.MakeSeq(
                                Expression.MakeCastExpr(
                                    Helpers.MakeIdentifierPath("a"),
                                    AstType.MakePrimitiveType("byte")
                                )
                            ),
                            Modifiers.Immutable
                        ),
                        Statement.MakeExprStmt(
                            Helpers.MakeCallExpression(
                                Helpers.MakeIdentifierPath("println"),
                                Expression.MakeConstant("string", "${a}, ${b}")
                            )
                        )
                    ),
                    Helpers.MakePlaceholderType(),
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
            parser.Parse();

            var ast = parser.TopmostAst;

            var expected = AstNode.MakeModuleDef("main", new List<EntityDeclaration>{
                EntityDeclaration.MakeFunc(
                    "main",
                    Enumerable.Empty<ParameterDeclaration>(),
                    Statement.MakeBlock(
                        Statement.MakeVarDecl(
                            Helpers.MakeSeq(
                                Helpers.MakeSomePatternWithType(
                                    "writer",
                                    Helpers.MakeGenericType("FileStream")
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
                                        Helpers.MakeIdentifierPath("writer"),
                                        Helpers.MakeCallExpression(
                                            Expression.MakeMemRef(
                                                Helpers.MakeIdentifierPath("File"),
                                                Helpers.MakeSomeIdent("OpenWrite")
                                            ),
                                            Expression.MakeConstant("string", "./some_text.txt")
                                        )
                                    )
                                ),
                                Statement.MakeVarDecl(
                                    Helpers.MakeSeq(
                                        Helpers.MakeSomePatternWithType("bytes") 
                                    ),
                                    Helpers.MakeSeq(
                                        Helpers.MakeCallExpression(
                                            Expression.MakeMemRef(
                                                Expression.MakeObjectCreation(
                                                    Helpers.MakeGenericType("UTF8Encoding"),
                                                    Helpers.MakeSeq(
                                                        AstNode.MakeIdentifier("encoderShouldEmitUTF8Identifier")
                                                    ),
                                                    Helpers.MakeSeq(
                                                        Expression.MakeConstant("bool", true)
                                                    )
                                                ),
                                                Helpers.MakeSomeIdent("GetBytes")
                                            ),
                                            Expression.MakeConstant("string", "This is to test writing a file")
                                        )
                                    ),
                                    Modifiers.Immutable
                                ),
                                Statement.MakeExprStmt(
                                    Helpers.MakeCallExpression(
                                        Expression.MakeMemRef(
                                            Helpers.MakeIdentifierPath("writer"),
                                            Helpers.MakeSomeIdent("Write")
                                        ),
                                        Helpers.MakeIdentifierPath("bytes"),
                                        Expression.MakeConstant("int", 0),
                                        Expression.MakeMemRef(
                                            Helpers.MakeIdentifierPath("bytes"),
                                            Helpers.MakeSomeIdent("Length")
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
                                                Helpers.MakeIdentifierPath("writer"),
                                                Expression.MakeNullRef()
                                            )
                                        ),
                                        Statement.MakeBlock(
                                            Statement.MakeExprStmt(
                                                Helpers.MakeCallExpression(
                                                    Expression.MakeMemRef(
                                                        Helpers.MakeIdentifierPath("writer"),
                                                        Helpers.MakeSomeIdent("Dispose")
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
                    Helpers.MakePlaceholderType(),
                    Modifiers.None
                )
            }, Helpers.MakeSeq(
                AstNode.MakeImportDecl(
                    Helpers.MakeSomeIdent("System.IO.File"),
                    "File"
                ),
                AstNode.MakeImportDecl(
                    Helpers.MakeSomeIdent("System.IO.FileStream"),
                    "FileStream"
                ),
                AstNode.MakeImportDecl(
                    Helpers.MakeSomeIdent("System.Text.UTF8Encoding"),
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
            parser.Parse();

            var ast = parser.TopmostAst;

            var expected = AstNode.MakeModuleDef("main", new List<EntityDeclaration>{
                Helpers.MakeClassDecl(
                    "TestClass5",
                    Enumerable.Empty<AstType>(),
                    Modifiers.None,
                    EntityDeclaration.MakeField(
                        Helpers.MakeSeq(
                            Helpers.MakeSomePatternWithType(
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
                            Helpers.MakeSomePatternWithType(
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
                                    Helpers.MakeSomeIdent("x")
                                )
                            )
                        ),
                        Helpers.MakePlaceholderType(),
                        Modifiers.Public
                    )
                ),
                EntityDeclaration.MakeFunc(
                    "main",
                    Enumerable.Empty<ParameterDeclaration>(),
                    Statement.MakeBlock(
                        Statement.MakeVarDecl(
                            Helpers.MakeSeq(
                                Helpers.MakeSomePatternWithType("x")
                            ),
                            Helpers.MakeSeq(
                                Expression.MakeConstant("int", 5)
                            ),
                            Modifiers.Immutable
                        ),
                        Statement.MakeVarDecl(
                            Helpers.MakeSeq(
                                Helpers.MakeSomePatternWithType("t")
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
                                Helpers.MakeSomePatternWithType("ary")
                            ),
                            Helpers.MakeSeq(
                                Helpers.MakeSequenceInitializer(
                                    Helpers.MakeGenericType(
                                        "array",
                                        Helpers.MakePlaceholderType()
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
                                Helpers.MakeSomePatternWithType("a")
                            ),
                            Helpers.MakeSeq(
                                Expression.MakeConstant("string", "some string")
                            ),
                            Modifiers.Immutable
                        ),
                        Statement.MakeVarDecl(
                            Helpers.MakeSeq(
                                Helpers.MakeSomePatternWithType("b")
                            ),
                            Helpers.MakeSeq(
                                Expression.MakeConstant("string", "some string containing templates: ${x + 1}")
                            ),
                            Modifiers.Immutable
                        ),
                        Statement.MakeVarDecl(
                            Helpers.MakeSeq(
                                Helpers.MakeSomePatternWithType("c")
                            ),
                            Helpers.MakeSeq(
                                Expression.MakeConstant("string", "another string containing templates: ${t.getX()}, ${t.y}")
                            ),
                            Modifiers.Immutable
                        ),
                        Statement.MakeVarDecl(
                            Helpers.MakeSeq(
                                Helpers.MakeSomePatternWithType("d")
                            ),
                            Helpers.MakeSeq(
                                Expression.MakeConstant("string", "the 6th fibonacci number is ${ary[5]}")
                            ),
                            Modifiers.Immutable
                        ),
                        Statement.MakeVarDecl(
                            Helpers.MakeSeq(
                                Helpers.MakeSomePatternWithType("e")
                            ),
                            Helpers.MakeSeq(
                                Expression.MakeConstant("string", "a string containing dollar symbol: $$x = ${x}")
                            ),
                            Modifiers.Immutable
                        ),
                        Statement.MakeExprStmt(
                            Helpers.MakeCallExpression(
                                Helpers.MakeIdentifierPath("println"),
                                Expression.MakeConstant("string", "${a}, ${b}, ${c}, ${d}, ${e}")
                            )
                        )
                    ),
                    Helpers.MakePlaceholderType(),
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
            parser.Parse();

            var ast = parser.TopmostAst;

            var expected_ast = AstNode.MakeModuleDef("main", new List<EntityDeclaration>{
                EntityDeclaration.MakeFunc(
                    "main",
                    Enumerable.Empty<ParameterDeclaration>(),
                    Statement.MakeBlock(
                        Statement.MakeVarDecl(
                            Helpers.MakeSeq(
                                Helpers.MakeSomePatternWithType("t")
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
                                Helpers.MakeSomePatternWithType("t2")
                            ),
                            Helpers.MakeSeq(
                                Helpers.MakeCallExpression(
                                    Helpers.MakeIdentifierPath("createTest"),
                                    Expression.MakeConstant("int", 3),
                                    Expression.MakeConstant("int", 4)
                                )
                            ),
                            Modifiers.Immutable
                        ),
                        Statement.MakeVarDecl(
                            Helpers.MakeSeq(
                                Helpers.MakeSomePatternWithType("x")
                            ),
                            Helpers.MakeSeq(
                                Helpers.MakeIdentifierPath("pair")
                            ),
                            Modifiers.Immutable
                        ),
                        Statement.MakeExprStmt(
                            Helpers.MakeCallExpression(
                                Helpers.MakeIdentifierPath("println"),
                                Expression.MakeConstant("string", "${t}, ${t2}, ${x}")
                            )
                        )
                    ),
                    Helpers.MakePlaceholderType(),
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
            parser.Parse();

            var ast = parser.TopmostAst;

            var expected = AstNode.MakeModuleDef("main", new List<EntityDeclaration>{
                EntityDeclaration.MakeFunc(
                    "main",
                    Enumerable.Empty<ParameterDeclaration>(),
                    Statement.MakeBlock(
                        Statement.MakeVarDecl(
                            Helpers.MakeSeq(
                                Helpers.MakeSomePatternWithType("t")
                            ),
                            Helpers.MakeSeq(
                                Expression.MakeObjectCreation(
                                    Helpers.MakeGenericType("InteroperabilityTest"),
                                    Enumerable.Empty<Identifier>(),
                                    Enumerable.Empty<Expression>()
                                )
                            ),
                            Modifiers.Immutable
                        ),
                        Statement.MakeExprStmt(
                            Helpers.MakeCallExpression(
                                Expression.MakeMemRef(
                                    Helpers.MakeIdentifierPath("t"),
                                    Helpers.MakeSomeIdent("DoSomething")
                                )
                            )
                        ),
                        Statement.MakeVarDecl(
                            Helpers.MakeSeq(
                                Helpers.MakeSomePatternWithType("i")
                            ),
                            Helpers.MakeSeq(
                                Helpers.MakeCallExpression(
                                    Expression.MakeMemRef(
                                        Helpers.MakeIdentifierPath("t"),
                                        Helpers.MakeSomeIdent("GetSomeInt")
                                    )
                                )
                            ),
                            Modifiers.Immutable
                        ),
                        Statement.MakeVarDecl(
                            Helpers.MakeSeq(
                                Helpers.MakeSomePatternWithType("list")
                            ),
                            Helpers.MakeSeq(
                                Helpers.MakeCallExpression(
                                    Expression.MakeMemRef(
                                        Helpers.MakeIdentifierPath("t"),
                                        Helpers.MakeSomeIdent("GetIntList")
                                    )
                                )
                            ),
                            Modifiers.Immutable
                        ),
                        Statement.MakeExprStmt(
                            Helpers.MakeCallExpression(
                                Expression.MakeMemRef(
                                    Helpers.MakeIdentifierPath("StaticTest"),
                                    Helpers.MakeSomeIdent("DoSomething")
                                )
                            )
                        ),
                        Statement.MakeVarDecl(
                            Helpers.MakeSeq(
                                Helpers.MakeSomePatternWithType("flag")
                            ),
                            Helpers.MakeSeq(
                                Helpers.MakeCallExpression(
                                    Expression.MakeMemRef(
                                        Helpers.MakeIdentifierPath("StaticTest"),
                                        Helpers.MakeSomeIdent("GetSomeBool")
                                    )
                                )
                            ),
                            Modifiers.Immutable
                        ),
                        Statement.MakeVarDecl(
                            Helpers.MakeSeq(
                                Helpers.MakeSomePatternWithType("seq")
                            ),
                            Helpers.MakeSeq(
                                Helpers.MakeCallExpression(
                                    Expression.MakeMemRef(
                                        Helpers.MakeIdentifierPath("StaticTest"),
                                        Helpers.MakeSomeIdent("GetSomeIntSeq")
                                    )
                                )
                            ),
                            Modifiers.Immutable
                        ),
                        Statement.MakeExprStmt(
                            Helpers.MakeCallExpression(
                                Helpers.MakeIdentifierPath("println"),
                                Expression.MakeConstant("string", "${i}, ${list}, ${flag}, ${seq}")
                            )
                        )
                    ),
                    Helpers.MakePlaceholderType(),
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
            parser.Parse();

            var ast = parser.TopmostAst;

            var expected = AstNode.MakeModuleDef("main", new List<EntityDeclaration>{
                EntityDeclaration.MakeFunc(
                    "main",
                    Enumerable.Empty<ParameterDeclaration>(),
                    Statement.MakeBlock(
                        Statement.MakeVarDecl(
                            Helpers.MakeSeq(
                                Helpers.MakeSomePatternWithType("dict")
                            ),
                            Helpers.MakeSeq(
                                Helpers.MakeSequenceInitializer(
                                    Helpers.MakeGenericType(
                                        "dictionary",
                                        Helpers.MakePlaceholderType(),
                                        Helpers.MakePlaceholderType()
                                    ),
                                    Expression.MakeKeyValuePair(Expression.MakeConstant("string", "akari"), Expression.MakeConstant("int", 13)),
                                    Expression.MakeKeyValuePair(Expression.MakeConstant("string", "chinatsu"), Expression.MakeConstant("int", 13)),
                                    Expression.MakeKeyValuePair(Expression.MakeConstant("string", "kyoko"), Expression.MakeConstant("int", 14)),
                                    Expression.MakeKeyValuePair(Expression.MakeConstant("string", "yui"), Expression.MakeConstant("int", 14))
                                )
                            ),
                            Modifiers.Immutable
                        ),
                        Statement.MakeVarDecl(
                            Helpers.MakeSeq(
                                Helpers.MakeSomePatternWithType("a")
                            ),
                            Helpers.MakeSeq(
                                Helpers.MakeSequenceInitializer(
                                    Helpers.MakeGenericType(
                                        "array",
                                        Helpers.MakePlaceholderType()
                                    ),
                                    Expression.MakeParen(
                                        Expression.MakeSequenceExpression(Expression.MakeConstant("int", 1), Expression.MakeConstant("int", 2))
                                    ),
                                    Expression.MakeParen(
                                        Expression.MakeSequenceExpression(Expression.MakeConstant("int", 3), Expression.MakeConstant("int", 4))
                                    ),
                                    Expression.MakeParen(
                                        Expression.MakeSequenceExpression(Expression.MakeConstant("int", 5), Expression.MakeConstant("int", 6))
                                    )
                                )
                            ),
                            Modifiers.Immutable
                        ),
                        Statement.MakeVarDecl(
                            Helpers.MakeSeq(
                                Helpers.MakeSomePatternWithType("v")
                            ),
                            Helpers.MakeSeq(
                                Helpers.MakeSequenceInitializer(
                                    Helpers.MakeGenericType(
                                        "vector",
                                        Helpers.MakePlaceholderType()
                                    ),
                                    Expression.MakeParen(
                                        Expression.MakeSequenceExpression(Expression.MakeConstant("int", 7), Expression.MakeConstant("int", 8))
                                    ),
                                    Expression.MakeParen(
                                        Expression.MakeSequenceExpression(Expression.MakeConstant("int", 9), Expression.MakeConstant("int", 10))
                                    ),
                                    Expression.MakeParen(
                                        Expression.MakeSequenceExpression(Expression.MakeConstant("int", 11), Expression.MakeConstant("int", 12))
                                    )
                                )
                            ),
                            Modifiers.Immutable
                        ),
                        Statement.MakeValueBindingForStmt(
                            Modifiers.Immutable,
                            Helpers.MakeSomeTuplePatternWithType("key", "value"),
                            Helpers.MakeIdentifierPath("dict"),
                            Statement.MakeBlock(
                                Statement.MakeExprStmt(
                                    Helpers.MakeCallExpression(
                                        Helpers.MakeIdentifierPath("println"),
                                        Expression.MakeConstant("string", "${key}: ${value}, ")
                                    )
                                )
                            )
                        ),
                        Statement.MakeValueBindingForStmt(
                            Modifiers.Immutable,
                            Helpers.MakeSomeTuplePatternWithType("first", "second"),
                            Helpers.MakeIdentifierPath("a"),
                            Statement.MakeBlock(
                                Statement.MakeExprStmt(
                                    Helpers.MakeCallExpression(
                                        Helpers.MakeIdentifierPath("println"),
                                        Expression.MakeConstant("string", "(${first}, ${second}), ")
                                    )
                                )
                            )
                        ),
                        Statement.MakeValueBindingForStmt(
                            Modifiers.Immutable,
                            Helpers.MakeSomeTuplePatternWithType("first2", "second2"),
                            Helpers.MakeIdentifierPath("v"),
                            Statement.MakeBlock(
                                Statement.MakeExprStmt(
                                    Helpers.MakeCallExpression(
                                        Helpers.MakeIdentifierPath("println"),
                                        Expression.MakeConstant("string", "(${first2}, ${second2}), ")
                                    )
                                )
                            )
                        )
                    ),
                    Helpers.MakePlaceholderType(),
                    Modifiers.None
                )
            });

            Assert.IsNotNull(ast);
            Helpers.AstStructuralEqual(ast, expected);
        }

        [Test]
        public void PropertyTests()
        {
            var parser = new Parser(new Scanner("../../sources/for_unit_tests/property_tests.exs"));
            parser.Parse();

            var ast = parser.TopmostAst;

            var expected = AstNode.MakeModuleDef("main", new List<EntityDeclaration>{
                EntityDeclaration.MakeFunc(
                    "main",
                    Enumerable.Empty<ParameterDeclaration>(),
                    Statement.MakeBlock(
                        Statement.MakeExprStmt(
                            Helpers.MakeCallExpression(
                                Helpers.MakeIdentifierPath("println"),
                                Expression.MakeConstant("string", "before: ${PropertyTest.SomeProperty}")
                            )
                        ),
                        Statement.MakeExprStmt(
                            Expression.MakeSingleAssignment(
                                Expression.MakeMemRef(
                                    Helpers.MakeIdentifierPath("PropertyTest"),
                                    Helpers.MakeSomeIdent("SomeProperty")
                                ),
                                Expression.MakeConstant("int", 100)
                            )
                        ),
                        Statement.MakeExprStmt(
                            Helpers.MakeCallExpression(
                                Helpers.MakeIdentifierPath("println"),
                                Expression.MakeConstant("string", "after: ${PropertyTest.SomeProperty}")
                            )
                        ),
                        Statement.MakeVarDecl(
                            Helpers.MakeSeq(
                                Helpers.MakeSomePatternWithType("inst")
                            ),
                            Helpers.MakeSeq(
                                Expression.MakeObjectCreation(
                                    Helpers.MakeGenericType("PropertyTest"),
                                    Enumerable.Empty<Identifier>(),
                                    Enumerable.Empty<Expression>()
                                )
                            ),
                            Modifiers.None
                        ),
                        Statement.MakeExprStmt(
                            Helpers.MakeCallExpression(
                                Helpers.MakeIdentifierPath("println"),
                                Expression.MakeConstant("string", "before: ${inst.SomeInstanceProperty}")
                            )
                        ),
                        Statement.MakeExprStmt(
                            Expression.MakeSingleAssignment(
                                Expression.MakeMemRef(
                                    Helpers.MakeIdentifierPath("inst"),
                                    Helpers.MakeSomeIdent("SomeInstanceProperty")
                                ),
                                Expression.MakeConstant("int", 1000)
                            )
                        ),
                        Statement.MakeExprStmt(
                            Helpers.MakeCallExpression(
                                Helpers.MakeIdentifierPath("println"),
                                Expression.MakeConstant("string", "after: ${inst.SomeInstanceProperty}")
                            )
                        )
                    ),
                    Helpers.MakePlaceholderType(),
                    Modifiers.None
                )
            });

            Assert.IsNotNull(ast);
            Helpers.AstStructuralEqual(ast, expected);
        }

        [Test]
        public void TestEnumInCSharp()
        {
            var parser = new Parser(new Scanner("../../sources/for_unit_tests/test_enum_in_csharp.exs"));
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
                                        Helpers.MakeIdentifierPath("EnumTest"),
                                        Helpers.MakeSomeIdent("TestEnumeration")
                                    ),
                                    Expression.MakeMemRef(
                                        Helpers.MakeIdentifierPath("TestEnum"),
                                        Helpers.MakeSomeIdent("SomeField")
                                    )
                                )
                            ),
                            Statement.MakeBlock(
                                Statement.MakeExprStmt(
                                    Helpers.MakeCallExpression(
                                        Helpers.MakeIdentifierPath("println"),
                                        Expression.MakeConstant("string", "matched!")
                                    )
                                )
                            )
                        ),
                        Statement.MakeVarDecl(
                            Helpers.MakeSeq(
                                Helpers.MakeSomePatternWithType("tester")
                            ),
                            Helpers.MakeSeq(
                                Expression.MakeObjectCreation(
                                    Helpers.MakeGenericType("EnumTest"),
                                    Enumerable.Empty<Identifier>(),
                                    Enumerable.Empty<Expression>()
                                )
                            ),
                            Modifiers.None
                        ),
                        Statement.MakeExprStmt(
                            Expression.MakeSingleAssignment(
                                Expression.MakeMemRef(
                                    Helpers.MakeIdentifierPath("tester"),
                                    Helpers.MakeSomeIdent("TestProperty")
                                ),
                                Expression.MakeMemRef(
                                    Helpers.MakeIdentifierPath("TestEnum"),
                                    Helpers.MakeSomeIdent("YetAnotherField")
                                )
                            )
                        ),
                        Statement.MakeIfStmt(
                            PatternConstruct.MakeExpressionPattern(
                                Helpers.MakeCallExpression(
                                    Expression.MakeMemRef(
                                        Helpers.MakeIdentifierPath("tester"),
                                        Helpers.MakeSomeIdent("TestEnumerationOnInstance")
                                    )
                                )
                            ),
                            Statement.MakeBlock(
                                Statement.MakeExprStmt(
                                    Helpers.MakeCallExpression(
                                        Helpers.MakeIdentifierPath("println"),
                                        Expression.MakeConstant("string", "matched again!")
                                    )
                                )
                            )
                        )
                    ),
                    Helpers.MakePlaceholderType(),
                    Modifiers.None
                )
            });

            Assert.IsNotNull(ast);
            Helpers.AstStructuralEqual(ast, expected);
        }

        [Test]
        public void Attributes()
        {
            var parser = new Parser(new Scanner("../../sources/for_unit_tests/attributes.exs"));
            parser.Parse();

            var ast = parser.TopmostAst;

            var expected = AstNode.MakeModuleDef("main", new List<EntityDeclaration>{
                Helpers.MakeClassDecl(
                    "AuthorAttribute",
                    Helpers.MakeSeq(
                        Helpers.MakeGenericType("Attribute")
                    ),
                    Modifiers.None,
                    AstNode.MakeAttributeSection(
                        null,
                        Expression.MakeObjectCreation(
                            Helpers.MakeGenericType("AttributeUsage"),
                            Helpers.MakeSeq(
                                AstNode.MakeIdentifier("validOn")
                            ),
                            Helpers.MakeSeq(
                                Expression.MakeMemRef(
                                    Helpers.MakeIdentifierPath("AttributeTargets"),
                                    Helpers.MakeSomeIdent("All")
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
                    Helpers.MakeSomePatternWithType("y"),
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
                    Helpers.MakePlaceholderType(),
                    Modifiers.None,
                    AstNode.MakeAttributeSection(
                        null,
                        Expression.MakeObjectCreation(
                            Helpers.MakeGenericType("ObsoleteAttribute")
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
                            Helpers.MakeGenericType("SerializableAttribute")
                        )
                    ),
                    EntityDeclaration.MakeField(
                        Helpers.MakeExactPatternWithType("x", AstType.MakePrimitiveType("int")),
                        Expression.Null,
                        Modifiers.Immutable | Modifiers.Private,
                        AstNode.MakeAttributeSection(
                            null,
                            Expression.MakeObjectCreation(
                                Helpers.MakeGenericType("ConditionalAttribute"),
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
                                    Helpers.MakeIdentifierPath("println"),
                                    Expression.MakeConstant("string", "Do something")
                                )
                            )
                        ),
                        Helpers.MakePlaceholderType(),
                        Modifiers.Public
                    ),
                    EntityDeclaration.MakeFunc(
                        "doSomething2",
                        Helpers.MakeSeq(
                            EntityDeclaration.MakeParameter(
                                "n",
                                Helpers.MakePlaceholderType(),
                                Expression.MakeConstant("int", 100)
                            )
                        ),
                        Statement.MakeBlock(
                            Statement.MakeExprStmt(
                                Helpers.MakeCallExpression(
                                    Helpers.MakeIdentifierPath("println"),
                                    Expression.MakeConstant("string", "${n}")
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
                                Helpers.MakeSomePatternWithType("x")
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
                                    Helpers.MakeIdentifierPath("x"),
                                    Helpers.MakeSomeIdent("doSomething")
                                ),
                                Expression.MakeConstant("string", "some string")
                            )
                        ),
                        Statement.MakeExprStmt(
                            Helpers.MakeCallExpression(
                                Expression.MakeMemRef(
                                    Helpers.MakeIdentifierPath("x"),
                                    Helpers.MakeSomeIdent("doSomething2")
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
                        Helpers.MakeGenericType("AssemblyDescriptionAttribute"),
                        AstNode.MakeIdentifier("description"),
                        Expression.MakeConstant("string", "test assembly for attributes")
                    )
                ),
                AstNode.MakeAttributeSection(
                    null,
                    Expression.MakeObjectCreation(
                        Helpers.MakeGenericType("CLSCompliantAttribute"),
                        AstNode.MakeIdentifier("isCompliant"),
                        Expression.MakeConstant("bool", true)
                    )
                )
            ));

            Assert.IsNotNull(ast);
            Helpers.AstStructuralEqual(ast, expected);
        }

        [Test]
        public void Enum1()
        {
            var parser = new Parser(new Scanner("../../sources/for_unit_tests/enum1.exs"));
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
                                            Helpers.MakeIdentifierPath("println"),
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
                                            Helpers.MakeIdentifierPath("println"),
                                            Expression.MakeConstant("string", "${some_int}, ${some_uint}")
                                        )
                                    ),
                                    PatternConstruct.MakeDestructuringPattern(
                                        AstType.MakeMemberType(
                                            Helpers.MakeGenericType("Union"),
                                            Helpers.MakeGenericType("B")
                                        ),
                                        PatternConstruct.MakeIdentifierPattern(
                                            Helpers.MakeSomeIdent("some_int")
                                        ),
                                        PatternConstruct.MakeIdentifierPattern(
                                            Helpers.MakeSomeIdent("some_uint")
                                        )
                                    )
                                ),
                                Statement.MakeMatchClause(
                                    null,
                                    Statement.MakeExprStmt(
                                        Helpers.MakeCallExpression(
                                            Helpers.MakeIdentifierPath("println"),
                                            Expression.MakeConstant("string", "${some_str}, ${some_char}")
                                        )
                                    ),
                                    PatternConstruct.MakeDestructuringPattern(
                                        AstType.MakeMemberType(
                                            Helpers.MakeGenericType("Union"),
                                            Helpers.MakeGenericType("C")
                                        ),
                                        PatternConstruct.MakeIdentifierPattern(
                                            Helpers.MakeSomeIdent("some_str")
                                        ),
                                        PatternConstruct.MakeIdentifierPattern(
                                            Helpers.MakeSomeIdent("some_char")
                                        )
                                    )
                                ),
                                Statement.MakeMatchClause(
                                    null,
                                    Statement.MakeExprStmt(
                                        Helpers.MakeCallExpression(
                                            Helpers.MakeIdentifierPath("println"),
                                            Expression.MakeConstant("string", "${some_intseq}")
                                        )
                                    ),
                                    PatternConstruct.MakeDestructuringPattern(
                                        AstType.MakeMemberType(
                                            Helpers.MakeGenericType("Union"),
                                            Helpers.MakeGenericType("D")
                                        ),
                                        PatternConstruct.MakeIdentifierPattern(
                                            Helpers.MakeSomeIdent("some_intseq")
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
                                Helpers.MakeSomePatternWithType("some_enum")
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
                                Helpers.MakeSomePatternWithType("some_enum2")
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
                                Helpers.MakeIdentifierPath("println"),
                                Expression.MakeConstant("string", "${some_enum}")
                            )
                        ),
                        Statement.MakeExprStmt(
                            Helpers.MakeCallExpression(
                                Helpers.MakeIdentifierPath("println"),
                                Expression.MakeConstant("string", "${some_enum2}")
                            )
                        ),
                        Statement.MakeMatchStmt(
                            Helpers.MakeIdentifierPath("some_enum"),
                            Statement.MakeMatchClause(
                                null,
                                Statement.MakeExprStmt(
                                    Helpers.MakeCallExpression(
                                        Helpers.MakeIdentifierPath("println"),
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
                                        Helpers.MakeIdentifierPath("println"),
                                        Expression.MakeConstant("string", "${some_int}, ${some_uint}")
                                    )
                                ),
                                PatternConstruct.MakeDestructuringPattern(
                                    AstType.MakeMemberType(
                                        Helpers.MakeGenericType("Union"),
                                        Helpers.MakeGenericType("B")
                                    ),
                                    PatternConstruct.MakeIdentifierPattern(
                                        Helpers.MakeSomeIdent("some_int")
                                    ),
                                    PatternConstruct.MakeIdentifierPattern(
                                        Helpers.MakeSomeIdent("some_uint")
                                    )
                                )
                            ),
                            Statement.MakeMatchClause(
                                null,
                                Statement.MakeExprStmt(
                                    Helpers.MakeCallExpression(
                                        Helpers.MakeIdentifierPath("println"),
                                        Expression.MakeConstant("string", "${some_str}, ${some_char}")
                                    )
                                ),
                                PatternConstruct.MakeDestructuringPattern(
                                    AstType.MakeMemberType(
                                        Helpers.MakeGenericType("Union"),
                                        Helpers.MakeGenericType("C")
                                    ),
                                    PatternConstruct.MakeIdentifierPattern(
                                        Helpers.MakeSomeIdent("some_str")
                                    ),
                                    PatternConstruct.MakeIdentifierPattern(
                                        Helpers.MakeSomeIdent("some_char")
                                    )
                                )
                            ),
                            Statement.MakeMatchClause(
                                null,
                                Statement.MakeExprStmt(
                                    Helpers.MakeCallExpression(
                                        Helpers.MakeIdentifierPath("println"),
                                        Expression.MakeConstant("string", "${some_intseq}")
                                    )
                                ),
                                PatternConstruct.MakeDestructuringPattern(
                                    AstType.MakeMemberType(
                                        Helpers.MakeGenericType("Union"),
                                        Helpers.MakeGenericType("D")
                                    ),
                                    PatternConstruct.MakeIdentifierPattern(
                                        Helpers.MakeSomeIdent("some_intseq")
                                    )
                                )
                            )
                        ),
                        Statement.MakeExprStmt(
                            Helpers.MakeCallExpression(
                                Expression.MakeMemRef(
                                    Helpers.MakeIdentifierPath("some_enum2"),
                                    Helpers.MakeSomeIdent("print")
                                )
                            )
                        )
                    ),
                    Helpers.MakePlaceholderType(),
                    Modifiers.None
                )
            ));

            Assert.IsNotNull(ast);
            Helpers.AstStructuralEqual(ast, expected);
        }

        [Test]
        public void Enum2()
        {
            var parser = new Parser(new Scanner("../../sources/for_unit_tests/enum2.exs"));
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
                                Expression.MakeSelfRef(),
                                Statement.MakeMatchClause(
                                    null,
                                    Statement.MakeExprStmt(
                                        Helpers.MakeCallExpression(
                                            Helpers.MakeIdentifierPath("println"),
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
                                            Helpers.MakeIdentifierPath("println"),
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
                                            Helpers.MakeIdentifierPath("println"),
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
                                        Expression.MakeSelfRef(),
                                        Expression.MakePath(
                                            AstNode.MakeIdentifier("SomeEnum"),
                                            Helpers.MakeSomeIdent("A")
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
                                    Expression.MakeSelfRef(),
                                    Helpers.MakeSomeIdent("x")
                                ),
                                Statement.MakeMatchClause(
                                    null,
                                    Statement.MakeExprStmt(
                                        Helpers.MakeCallExpression(
                                            Helpers.MakeIdentifierPath("println"),
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
                                            Helpers.MakeIdentifierPath("println"),
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
                                            Helpers.MakeIdentifierPath("println"),
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
                        Helpers.MakePlaceholderType(),
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
                                            Expression.MakeSelfRef(),
                                            Helpers.MakeSomeIdent("x")
                                        ),
                                        Expression.MakePath(
                                            AstNode.MakeIdentifier("SomeEnum"),
                                            Helpers.MakeSomeIdent("A")
                                        )
                                    )
                                ),
                                Statement.MakeBlock(
                                    Statement.MakeExprStmt(
                                        Helpers.MakeCallExpression(
                                            Helpers.MakeIdentifierPath("println"),
                                            Expression.MakeConstant("string", "A in ifEnum")
                                        )
                                    )
                                )
                            )
                        ),
                        Helpers.MakePlaceholderType(),
                        Modifiers.Public
                    )
                ),
                EntityDeclaration.MakeFunc(
                    "main",
                    Enumerable.Empty<ParameterDeclaration>(),
                    Statement.MakeBlock(
                        Statement.MakeVarDecl(
                            Helpers.MakeSeq(
                                Helpers.MakeSomePatternWithType("enum_a")
                            ),
                            Helpers.MakeSeq(
                                Expression.MakePath(
                                    Helpers.MakeSomeIdent("SomeEnum"),
                                    Helpers.MakeSomeIdent("A")
                                )
                            ),
                            Modifiers.Immutable
                        ),
                        Statement.MakeMatchStmt(
                            Helpers.MakeIdentifierPath("enum_a"),
                            Statement.MakeMatchClause(
                                null,
                                Statement.MakeExprStmt(
                                    Helpers.MakeCallExpression(
                                        Helpers.MakeIdentifierPath("println"),
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
                                        Helpers.MakeIdentifierPath("println"),
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
                                        Helpers.MakeIdentifierPath("println"),
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
                                    Helpers.MakeIdentifierPath("enum_a"),
                                    Helpers.MakeSomeIdent("print")
                                )
                            )
                        ),
                        Statement.MakeIfStmt(
                            PatternConstruct.MakeExpressionPattern(
                                Expression.MakeBinaryExpr(
                                    OperatorType.Equality,
                                    Helpers.MakeIdentifierPath("enum_a"),
                                    Expression.MakePath(
                                        AstNode.MakeIdentifier("SomeEnum"),
                                        Helpers.MakeSomeIdent("A")
                                    )
                                )
                            ),
                            Statement.MakeBlock(
                                Statement.MakeExprStmt(
                                    Helpers.MakeCallExpression(
                                        Helpers.MakeIdentifierPath("println"),
                                        Expression.MakeConstant("string", "A in if")
                                    )
                                )
                            )
                        ),
                        Statement.MakeExprStmt(
                            Helpers.MakeCallExpression(
                                Expression.MakeMemRef(
                                    Helpers.MakeIdentifierPath("enum_a"),
                                    Helpers.MakeSomeIdent("printUsingIf")
                                )
                            )
                        ),
                        Statement.MakeVarDecl(
                            Helpers.MakeSeq(
                                Helpers.MakeSomePatternWithType("some_class")
                            ),
                            Helpers.MakeSeq(
                                Expression.MakeObjectCreation(
                                    Helpers.MakeGenericType("SomeClass"),
                                    AstNode.MakeIdentifier("x"),
                                    Helpers.MakeIdentifierPath("enum_a")
                                )
                            ),
                            Modifiers.Immutable
                        ),
                        Statement.MakeExprStmt(
                            Helpers.MakeCallExpression(
                                Expression.MakeMemRef(
                                    Helpers.MakeIdentifierPath("some_class"),
                                    Helpers.MakeSomeIdent("matchEnum")
                                )
                            )
                        ),
                        Statement.MakeExprStmt(
                            Helpers.MakeCallExpression(
                                Expression.MakeMemRef(
                                    Helpers.MakeIdentifierPath("some_class"),
                                    Helpers.MakeSomeIdent("ifEnum")
                                )
                            )
                        )
                    ),
                    Helpers.MakePlaceholderType(),
                    Modifiers.None
                )
            ));

            Assert.IsNotNull(ast);
            Helpers.AstStructuralEqual(ast, expected);
        }

        [Test]
        public void GenericClass()
        {
            var parser = new Parser(new Scanner("../../sources/for_unit_tests/generic_class.exs"));
            parser.Parse();

            var ast = parser.TopmostAst;

            var expected = AstNode.MakeModuleDef("main", Helpers.MakeSeq<EntityDeclaration>(
                Helpers.MakeInterfaceDecl(
                    "SomeInterface",
                    Enumerable.Empty<AstType>(),
                    Modifiers.None,
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
                                    Helpers.MakeSomeIdent("x")
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
                                Helpers.MakeSomePatternWithType("a")
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
                                Helpers.MakeSomePatternWithType("b")
                            ),
                            Helpers.MakeSeq(
                                Expression.MakeObjectCreation(
                                    Helpers.MakeGenericType("GenericClass"),
                                    AstNode.MakeIdentifier("x"),
                                    Expression.MakeConstant("int", 20)
                                )
                            ),
                            Modifiers.Immutable
                        ),
                        Statement.MakeVarDecl(
                            Helpers.MakeSeq(
                                Helpers.MakeSomePatternWithType("c")
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
                                Helpers.MakeSomePatternWithType("d")
                            ),
                            Helpers.MakeSeq(
                                Expression.MakeObjectCreation(
                                    AstType.MakeMemberType(
                                        Helpers.MakeGenericType("MyOption"),
                                        Helpers.MakeGenericType("Ok")
                                    ),
                                    AstNode.MakeIdentifier("0"),
                                    Expression.MakeConstant("int", 20)
                                )
                            ),
                            Modifiers.Immutable
                        ),
                        Statement.MakeExprStmt(
                            Helpers.MakeCallExpression(
                                Helpers.MakeIdentifierPath("println"),
                                Expression.MakeConstant("string", "${a}, ${b}, ${c}, ${d}")
                            )
                        )
                    ),
                    Helpers.MakePlaceholderType(),
                    Modifiers.None
                )
            ));

            Assert.IsNotNull(ast);
            Helpers.AstStructuralEqual(ast, expected);
        }
    }
}
