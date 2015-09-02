using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Expresso.Ast;
using ICSharpCode.NRefactory;
using NUnit.Framework;
using ICSharpCode.NRefactory.PatternMatching;
using System.Threading;

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
            /*AstNode cur = target;
            while(cur != null){
                cur.Match
            }*/
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

        public static void TestOnType(object instance, List<string> privateMembers, List<FunctionAnnotation> methodAnnots)
        {
            /*foreach(var private_name in privateMembers){
                try{
                    instance(private_name, false);
                }
            }

            foreach(var method_annot in methodAnnots){
                var method = instance.AccessMemberWithName(method_annot.Name, false) as FunctionDeclaration;
                Assert.IsNotNull(method);
                Assert.AreEqual(method_annot.Name, method.Name);
                Assert.AreEqual(method_annot.ReturnType, method.ReturnType);
            }*/
        }

        public static T[] MakeArray<T>(params T[] objs)
        {
            return objs;
        }

        public static List<T> MakeList<T>(params T[] objs)
        {
            return new List<T>(objs);
        }

        public static IEnumerable<T> MakeSeq<T>(params T[] objs)
        {
            return objs.ToList();
        }

        public static Identifier MakeSomeIdent(string name)
        {
            return AstNode.MakeIdentifier(name, new PlaceholderType(TextLocation.Empty));
        }

        public static SimpleType MakeGenericType(string identifier, params AstType[] typeArgs)
        {
            return new SimpleType(identifier, typeArgs, TextLocation.Empty, TextLocation.Empty);
        }

        public static PrimitiveType MakePrimitiveType(string typeName)
        {
            return new PrimitiveType(typeName, TextLocation.Empty);
        }

        public static PlaceholderType MakePlaceholderType()
        {
            return new PlaceholderType(TextLocation.Empty);
        }

        public static ReturnStatement MakeReturnStatement(params Expression[] expressions)
        {
            return new ReturnStatement(Expression.MakeSequence(expressions));
        }

        public static AssignmentExpression MakeAssignment(IEnumerable<Expression> lhs, IEnumerable<Expression> rhs)
        {
            return Expression.MakeAssignment(Expression.MakeSequence(lhs), Expression.MakeSequence(rhs));
        }

        public static ExpressionStatement MakeAugmentedAssignment(OperatorType opType,
            IEnumerable<Expression> lhs, IEnumerable<Expression> rhs)
        {
            return Statement.MakeAugumentedAssignment(Expression.MakeSequence(lhs), Expression.MakeSequence(rhs), opType);
        }
    }

    internal class FunctionAnnotation
    {
        public string Name{get; set;}
        public AstType ReturnType{get; set;}

        public FunctionAnnotation(string name, AstType returnType)
        {
            Name = name;
            ReturnType = returnType;
        }
    }
}

