using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Expresso.Ast;
using ICSharpCode.NRefactory;
using ICSharpCode.NRefactory.PatternMatching;
using NUnit.Framework;

namespace Expresso.Test
{
    internal class Helpers
    {
        public static int CalcSum(int start, int max)
        {
            var result = 0;
            for(int i = start; i < max; ++i)
                result += i;

            return result;
        }

        public static int Fib(int n)
        {
            if(n < 2)
                return 1;
            else
                return Fib(n - 1) + Fib(n - 2);
        }

        public static void AstStructuralEqual(ExpressoAst target, ExpressoAst expect)
        {
            Assert.IsTrue(target.IsMatch(expect));
        }

        public static void AstStructuralEqual(Module mainModule, List<string> names, object[] expects)
        {
            var len = expects.Length;
            for(int i = 0; i < len; ++i){
                var target = mainModule.GetField(names[i]);
                var expect = expects[i];
                Type type_target = target.GetType(), type_expect = expect.GetType();
                Assert.IsTrue(type_target.FullName == type_expect.FullName);
                Assert.AreEqual(expect, target);
            }
        }

        /// <summary>
        /// Makes an array from some objects.
        /// </summary>
        /// <returns>An array.</returns>
        /// <param name="objs">Objects.</param>
        /// <typeparam name="T">The type of the objects.</typeparam>
        public static T[] MakeArray<T>(params T[] objs)
        {
            return objs;
        }

        /// <summary>
        /// Makes a list from a sequence of some objects.
        /// </summary>
        /// <returns>A <see cref="List{T}"/> instance.</returns>
        /// <param name="objs">Objects.</param>
        /// <typeparam name="T">The type of the objects.</typeparam>
        public static List<T> MakeList<T>(params T[] objs)
        {
            return new List<T>(objs);
        }

        /// <summary>
        /// Makes an IEnumerable instance from a sequence of some objects.
        /// </summary>
        /// <returns>An IEnumerable instance.</returns>
        /// <param name="objs">Objects.</param>
        /// <typeparam name="T">The type of the objects.</typeparam>
        public static IEnumerable<T> MakeSeq<T>(params T[] objs)
        {
            return objs.ToList();
        }

        /// <summary>
        /// Makes an identifier instance with a placeholder type.
        /// </summary>
        /// <returns>An Identifier instance. The type will be set to PlaceholderType.</returns>
        /// <param name="name">The name of the identifier.</param>
        public static Identifier MakeSomeIdent(string name)
        {
            return AstNode.MakeIdentifier(name, AstType.MakePlaceholderType());
        }

        /// <summary>
        /// Makes an identifier pattern with a placeholder type.
        /// </summary>
        /// <returns>An identifier pattern. The type will be set to a PlaceholderType.</returns>
        /// <param name="name">Name.</param>
        public static PatternWithType MakeSomePatternWithType(string name)
        {
            return PatternConstruct.MakePatternWithType(
                PatternConstruct.MakeIdentifierPattern(name, AstType.MakePlaceholderType()),
                AstType.MakePlaceholderType()
            );
        }

        /// <summary>
        /// Makes a PatternWithType node with type.
        /// </summary>
        /// <returns>The some identifier pattern.</returns>
        /// <param name="name">Name.</param>
        /// <param name="type">Type.</param>
        public static PatternWithType MakeSomePatternWithType(string name, AstType type)
        {
            return PatternConstruct.MakePatternWithType(
                PatternConstruct.MakeIdentifierPattern(name, AstType.MakePlaceholderType()),
                type
            );
        }

        /// <summary>
        /// Makes a <see name="PatternWithType"/> with a type associated with the identifier.
        /// </summary>
        /// <returns>The paticular pattern with type.</returns>
        /// <param name="name">Name.</param>
        /// <param name="type">Type.</param>
        public static PatternWithType MakePaticularPatternWithType(string name, AstType type)
        {
            return PatternConstruct.MakePatternWithType(
                PatternConstruct.MakeIdentifierPattern(name, type),
                AstType.MakePlaceholderType()
            );
        }

        /// <summary>
        /// Makes a <see cref="PatternWithType"/> with types all resolved.
        /// </summary>
        /// <returns>The exact pattern with type.</returns>
        /// <param name="name">Name.</param>
        /// <param name="type">Type.</param>
        public static PatternWithType MakeExactPatternWithType(string name, AstType type)
        {
            return PatternConstruct.MakePatternWithType(
                PatternConstruct.MakeIdentifierPattern(name, type),
                type.Clone()
            );
        }

        /// <summary>
        /// Makes a <see cref="PatternWithType"/> with fully qualified type.
        /// </summary>
        /// <returns>A <see cref="PatternWithType"/> instance with a fully qualified type.</returns>
        /// <param name="name">The name of the identifier.</param>
        /// <param name="fullyQualifiedType">The fully quialified type.</param>
        /// <param name="aliasTypeName">Alias type name.</param>
        public static PatternWithType MakeExactPatternWithType(string name, SimpleType fullyQualifiedType, string aliasTypeName)
        {
            return PatternConstruct.MakePatternWithType(
                PatternConstruct.MakeIdentifierPattern(name, fullyQualifiedType),
                AstType.MakeSimpleType(aliasTypeName)
            );
        }

        /// <summary>
        /// Makes a <see cref="PatternWithType"/> instance having a <see cref="PlaceholderType"/> as the types.
        /// </summary>
        /// <returns>A <see cref="PatternWithType"/> having <see cref="PlaceholderType"/>s as types.</returns>
        /// <param name="names">The names of the tuple fields.</param>
        public static PatternWithType MakeSomeTuplePatternWithType(params string[] names)
        {
            return PatternConstruct.MakePatternWithType(
                PatternConstruct.MakeTuplePattern(names.Select(n => PatternConstruct.MakeIdentifierPattern(n, AstType.MakePlaceholderType()))),
                AstType.MakePlaceholderType()
            );
        }

        /// <summary>
        /// Makes a <see cref="PatternWithType"/> instance having <see cref="AstType"/>s as the types.
        /// </summary>
        /// <returns>A <see cref="PatternWithType"/> having <see cref="AstType"/>s as the types</returns>
        /// <param name="names">The names of the tuple fields.</param>
        /// <param name="types">The types of the tuple fields.</param>
        public static PatternWithType MakePaticularTuplePatternWithType(IEnumerable<string> names, params AstType[] types)
        {
            return PatternConstruct.MakePatternWithType(
                PatternConstruct.MakeTuplePattern(names.Zip(types, (l, r) => new Tuple<string, AstType>(l, r))
                                                  .Select(pair => PatternConstruct.MakeIdentifierPattern(pair.Item1, pair.Item2))),
                MakeGenericType(
                    "tuple",
                    types.Select(t => t.Clone())
                )
            );
        }

        /// <summary>
        /// Makes a path expression from a string.
        /// </summary>
        /// <returns>A path expression</returns>
        /// <param name="name">Identifier.</param>
        public static PathExpression MakeIdentifierPath(string name)
        {
            return Expression.MakePath(MakeSomeIdent(name));
        }

        /// <summary>
        /// Makes a path expression from a string and a type.
        /// </summary>
        /// <returns>The identifier path.</returns>
        /// <param name="name">Identifier.</param>
        /// <param name="type">Type.</param>
        public static PathExpression MakeIdentifierPath(string name, AstType type)
        {
            return Expression.MakePath(AstNode.MakeIdentifier(name, type));
        }

        /// <summary>
        /// Makes an AstType with the specified identifier and optional type arguments.
        /// </summary>
        /// <returns>A generic type.</returns>
        /// <param name="name">Type identifier.</param>
        /// <param name="typeArgs">Type arguments.</param>
        public static SimpleType MakeGenericType(string name, params AstType[] typeArgs)
        {
            return AstType.MakeSimpleType(name, typeArgs);
        }

        public static SimpleType MakeGenericType(string name, IEnumerable<AstType> typeArgs)
        {
            return AstType.MakeSimpleType(name, typeArgs);
        }

        /// <summary>
        /// Makes a SimpleType with a fully qualified name and optional type arguments.
        /// </summary>
        /// <returns>A generic type with real name.</returns>
        /// <param name="identifier">Identifier.</param>
        /// <param name="realName">A fully qualified name.</param>
        /// <param name="typeArgs">Type arguments.</param>
        public static SimpleType MakeGenericTypeWithRealName(string identifier, string realName, params AstType[] typeArgs)
        {
            return AstType.MakeSimpleType(
                AstNode.MakeIdentifier(
                    identifier,
                    AstType.MakeSimpleType(realName)
                ),
                typeArgs
            );
        }

        /// <summary>
        /// Makes a primitive type.
        /// </summary>
        /// <returns>The primitive type.</returns>
        /// <param name="typeName">Type name.</param>
        public static PrimitiveType MakePrimitiveType(string typeName)
        {
            return new PrimitiveType(typeName, TextLocation.Empty);
        }

        /// <summary>
        /// Makes a placeholder type.
        /// </summary>
        /// <returns>The placeholder type.</returns>
        public static PlaceholderType MakePlaceholderType()
        {
            return new PlaceholderType(TextLocation.Empty);
        }

        public static ReturnStatement MakeReturnStatement(params Expression[] expressions)
        {
            return new ReturnStatement(Expression.MakeSequenceExpression(expressions), expressions.First().StartLocation);
        }

        /// <summary>
        /// Makes an assignment expression.
        /// </summary>
        /// <returns>An assignment.</returns>
        /// <param name="lhs">The left hand side expressions.</param>
        /// <param name="rhs">The right hand side expressions.</param>
        public static AssignmentExpression MakeAssignment(IEnumerable<Expression> lhs, IEnumerable<Expression> rhs)
        {
            return Expression.MakeAssignment(Expression.MakeSequenceExpression(lhs), Expression.MakeSequenceExpression(rhs));
        }

        /// <summary>
        /// Makes an augmented assignment expression.
        /// </summary>
        /// <returns>An augmented assignment.</returns>
        /// <param name="opType">The operator type.</param>
        /// <param name="lhs">The left hand side expressions.</param>
        /// <param name="rhs">The right hand side expressions.</param>
        public static ExpressionStatement MakeAugmentedAssignment(OperatorType opType,
            IEnumerable<Expression> lhs, IEnumerable<Expression> rhs)
        {
            return Statement.MakeAugmentedAssignment(opType, Expression.MakeSequenceExpression(lhs), Expression.MakeSequenceExpression(rhs));
        }

        public static CallExpression MakeCallExpression(Expression target, params Expression[] args)
        {
            return Expression.MakeCallExpr(target, args);
        }

        public static IndexerExpression MakeIndexerExpression(Expression target, params Expression[] args)
        {
            return Expression.MakeIndexer(target, args);
        }

        /// <summary>
        /// Makes a variable declaration that contains as many null expressions as the identifiers.
        /// </summary>
        /// <returns>A variable declaration.</returns>
        /// <param name="patterns">Identifiers.</param>
        /// <param name="modifiers">Modifiers.</param>
        public static VariableDeclarationStatement MakeVariableDeclaration(IEnumerable<PatternWithType> patterns, Modifiers modifiers)
        {
            var exprs =
                from ident in patterns
                select Expression.Null;

            return Statement.MakeVarDecl(patterns, exprs, modifiers);
        }

        /// <summary>
        /// Makes an AstType instance that identifies the void type.
        /// </summary>
        /// <returns>The void type.</returns>
        /// <param name="loc">Location.</param>
        public static AstType MakeVoidType(TextLocation loc = default)
        {
            return AstType.MakeSimpleType("tuple", loc);
        }

        /// <summary>
        /// Creates a <see cref="FunctionType"/> that represents a property setter.
        /// </summary>
        /// <returns>The property setter type.</returns>
        /// <param name="propertyName">Property name.</param>
        /// <param name="propertyType">Property type.</param>
        public static FunctionType MakePropertySetterType(string propertyName, AstType propertyType)
        {
            return AstType.MakeFunctionType("set_" + propertyName, MakeVoidType(), TextLocation.Empty, TextLocation.Empty, propertyType);
        }

        /// <summary>
        /// Creates a <see cref="FunctionType"/> that represents a property getter.
        /// </summary>
        /// <returns>The property getter type.</returns>
        /// <param name="propertyName">Property name.</param>
        /// <param name="propertyType">Property type.</param>
        public static FunctionType MakePropertyGetterType(string propertyName, AstType propertyType)
        {
            return AstType.MakeFunctionType("get_" + propertyName, propertyType, Enumerable.Empty<AstType>());
        }

        /// <summary>
        /// Creates an <see cref="Identifier"/> that represents a function.
        /// </summary>
        /// <returns>The function identifier.</returns>
        /// <param name="functionName">Function name.</param>
        /// <param name="returnType">Return type.</param>
        /// <param name="parameters">Parameters.</param>
        public static Identifier MakeFunctionIdentifier(string functionName, AstType returnType, params AstType[] parameters)
        {
            return AstNode.MakeIdentifier(functionName, AstType.MakeFunctionType(functionName, returnType, TextLocation.Empty, TextLocation.Empty, parameters));
        }

        /// <summary>
        /// Creates a <see cref="PathExpression"/> that represents a function.
        /// </summary>
        /// <returns>The function identifier path.</returns>
        /// <param name="functionName">Function name.</param>
        /// <param name="returnType">Return type.</param>
        /// <param name="parameters">Parameters.</param>
        public static PathExpression MakeFunctionIdentifierPath(string functionName, AstType returnType, params AstType[] parameters)
        {
            return MakeIdentifierPath(functionName, AstType.MakeFunctionType(functionName, returnType, TextLocation.Empty, TextLocation.Empty, parameters));
        }

        /// <summary>
        /// Makes a single-item return statement
        /// </summary>
        /// <returns>The single item return statement.</returns>
        /// <param name="expr">Expr.</param>
        public static ReturnStatement MakeSingleItemReturnStatement(Expression expr)
        {
            return Statement.MakeReturnStmt(Expression.MakeSequenceExpression(expr));
        }
    }
}

