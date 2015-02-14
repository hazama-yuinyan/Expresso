using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;


namespace Expresso.Ast
{
    /// <summary>
    /// An ast walker that outputs each node as a sequence of strings for debugging purpose.
    /// </summary>
    public class DebugOutputWalker : IAstWalker
    {
        readonly TextWriter writer;

        public DebugOutputWalker(TextWriter writer)
        {
            this.writer = writer;
        }

        void PrintList<TObject>(IEnumerable<TObject> list)
            where TObject : AstNode
        {
            int length = list.Count();
            var enumerator = list.GetEnumerator();
            for(int i = 0; i < length; ++i){
                enumerator.MoveNext();
                TObject obj = enumerator.Current;
                obj.AcceptWalker(this);
                if(i + 1 != length)
                    writer.Write(", ");
            }
        }

        void PrintPairList<T>(IEnumerable<Tuple<T, T>> list, Func<string> connector)
            where T : AstNode
        {
            int length = list.Count();
            var enumerator = list.GetEnumerator();
            for(int i = 0; i < length; ++i){
                enumerator.MoveNext();
                Tuple<T, T> pair = enumerator.Current;
                pair.Item1.AcceptWalker(this);
                writer.Write(connector());
                pair.Item2.AcceptWalker(this);
                if(i + 1 != length)
                    writer.Write(", ");
            }
        }

        void PrintPairList<T, U>(IEnumerable<Tuple<T, U>> list, string delimiter)
            where T : AstNode
            where U : AstNode
        {

        }

        #region IAstWalker implementation

        public void VisitAssignment(AssignmentExpression assignment)
        {
            assignment.Left.AcceptWalker(this);
            writer.Write(" = ");
            assignment.Right.AcceptWalker(this);
        }

        public void VisitBinaryExpression(BinaryExpression binaryExpr)
        {
            binaryExpr.Left.AcceptWalker(this);
            binaryExpr.OperatorToken.AcceptWalker(this);
            binaryExpr.Right.AcceptWalker(this);
        }

        public void VisitBlock(BlockStatement block)
        {
            writer.Write("{");
            writer.Write(block.Statements.Count);
            writer.Write("...}");
        }

        public void VisitCallExpression(CallExpression callExpr)
        {
            writer.Write("<Call: ");
            callExpr.Target.AcceptWalker(this);
            writer.Write("(");
            PrintList(callExpr.Arguments);
            writer.Write(")>");
        }

        public void VisitCastExpression(CastExpression castExpr)
        {
            writer.Write("(");
            castExpr.ToExpression.AcceptWalker(this);
            writer.Write(")");
            castExpr.Target.AcceptWalker(this);
        }

        public void VisitComprehensionFor(ComprehensionForClause compFor)
        {
            writer.Write("[");
            compFor.Body.AcceptWalker(this);
            writer.Write(" for ");
            compFor.Left.AcceptWalker(this);
            writer.Write(" in ");
            compFor.Target.AcceptWalker(this);
            writer.Write("]");
        }

        public void VisitComprehensionIf(ComprehensionIfClause compIf)
        {
            writer.Write("if ");
            compIf.Condition.AcceptWalker(this);
            compIf.Body.AcceptWalker(this);
        }

        public void VisitConditional(ConditionalExpression condExpr)
        {
            condExpr.Condition.AcceptWalker(this);
            writer.Write(" ? ");
            condExpr.TrueExpression.AcceptWalker(this);
            writer.Write(" : ");
            condExpr.FalseExpression.AcceptWalker(this);
        }

        public void VisitConstant(LiteralExpression literal)
        {
            writer.Write(literal.Value);
        }

        public void VisitBreakStatement(BreakStatement breakStmt)
        {
            writer.Write("<break upto ");
            breakStmt.Count.AcceptWalker(this);
            writer.Write(">");
        }

        public void VisitContinueStatement(ContinueStatement continueStmt)
        {
            writer.Write("<continue upto ");
            continueStmt.Count.AcceptWalker(this);
            writer.Write(">");
        }

        public void VisitEmptyStatement(EmptyStatement emptyStmt)
        {
        }

        public void VisitExpressionStatement(ExpressionStatement exprStmt)
        {
            exprStmt.Expression.AcceptWalker(this);
        }

        public void VisitForStatement(ForStatement forStmt)
        {
            writer.Write("<for: ");
            forStmt.Body.AcceptWalker(this);
            writer.Write(">");
        }

        public void VisitIdentifier(Identifier ident)
        {
            ident.Type.AcceptWalker(this);
            writer.Write(" ");
            writer.Write(ident.Name);
        }

        public void VisitIfStatement(IfStatement ifStmt)
        {
            writer.Write("<if: ");
            ifStmt.Condition.AcceptWalker(this);
            writer.Write(">");
        }

        public void VisitIntSequence(IntegerSequenceExpression intSeq)
        {
            writer.Write("<IntegerSequence: ");
            intSeq.Lower.AcceptWalker(this);
            writer.Write(" ");
            intSeq.Upper.AcceptWalker(this);
            writer.Write(" ");
            intSeq.Step.AcceptWalker(this);
            writer.Write(">");
        }

        public void VisitMemberReference(MemberReference memRef)
        {
            memRef.Target.AcceptWalker(this);
            memRef.Subscription.AcceptWalker(this);
        }

        public void VisitAst(ExpressoAst ast)
        {
            foreach(var decl in ast.Body)
                decl.AcceptWalker(this);
        }

        public void VisitNewExpression(NewExpression newExpr)
        {
            writer.Write("new ");
            newExpr.CreationExpression.AcceptWalker(this);
        }

        public void VisitSequenceInitializer(SequenceInitializer seqInitializer)
        {
        }

        public void VisitImportStatement(ImportDeclaration importStmt)
        {
            writer.Write("<Import: ");
            if(importStmt.AliasName != null){
                writer.Write(importStmt.ModuleName);
                writer.Write(" as ");
                writer.Write(importStmt.AliasName);
            }else{
                writer.Write(importStmt.ModuleName);
            }
        }

        public void VisitReturnStatement(ReturnStatement returnStmt)
        {
            writer.Write("<Return: ");
            returnStmt.Expression.AcceptWalker(this);
            writer.Write(">");
        }

        public void VisitMatchStatement(MatchStatement matchStmt)
        {
            writer.Write("<match: (");
            matchStmt.Target.AcceptWalker(this);
            writer.WriteLine("){");
            foreach(var clause in matchStmt.Clauses)
                clause.AcceptWalker(this);

            writer.Write("}");
        }

        public void VisitMatchPatternClause(MatchPatternClause clause)
        {
            foreach(var label in clause.Patterns){
                label.AcceptWalker(this);
                writer.Write(":");
            }

        }

        public void VisitSequence(SequenceExpression seqExpr)
        {
            PrintList(seqExpr.Items);
        }

        public void VisitUnaryExpression(UnaryExpression unaryExpr)
        {
            unaryExpr.Operand.AcceptWalker(this);
        }

        public void VisitWhileStatement(WhileStatement whileStmt)
        {
        }

        public void VisitYieldStatement(YieldStatement yieldStmt)
        {
        }

        public void VisitNullNode(AstNode nullNode)
        {
        }

        public void VisitPatternPlaceholder(AstNode placeholder, ICSharpCode.NRefactory.PatternMatching.Pattern child)
        {
        }

        public void VisitVariableDeclarationStatement(VariableDeclarationStatement varDecl)
        {
        }

        public void VisitComprehensionExpression(ComprehensionExpression comp)
        {
        }

        public void VisitComprehensionForClause(ComprehensionForClause compFor)
        {
        }

        public void VisitComprehensionIfClause(ComprehensionIfClause compIf)
        {
        }

        public void VisitConditionalExpression(ConditionalExpression condExpr)
        {
        }

        public void VisitKeyValueLikeExpression(KeyValueLikeExpression keyValue)
        {
        }

        public void VisitLiteralExpression(LiteralExpression literal)
        {
            writer.Write(literal.LiteralValue);
        }

        public void VisitIntgerSequenceExpression(IntegerSequenceExpression intSeq)
        {
        }

        public void VisitIndexerExpression(IndexerExpression indexExpr)
        {
        }

        public void VisitPathExpression(PathExpression pathExpr)
        {
            bool first = true;
            foreach(var item in pathExpr.Items){
                if(!first)
                    writer.Write("::");

                item.AcceptWalker(this);
            }
        }

        public void VisitParenthesizedExpression(ParenthesizedExpression parensExpr)
        {
            writer.Write("(");
            parensExpr.Expression.AcceptWalker(this);
            writer.Write(")");
        }

        public void VisitObjectCreationExpression(ObjectCreationExpression creation)
        {
        }

        public void VisitMatchClause(MatchPatternClause matchClause)
        {
        }

        public void VisitSelfReferenceExpression(SelfReferenceExpression selfRef)
        {
            writer.Write("self");
        }

        public void VisitSuperReferenceExpression(SuperReferenceExpression superRef)
        {
            writer.Write("super");
        }

        public void VisitCommentNode(CommentNode comment)
        {
        }

        public void VisitTextNode(TextNode textNode)
        {
        }

        public void VisitSimpleType(SimpleType simpleType)
        {
            writer.Write(simpleType.Identifier);
            if(simpleType.TypeArguments.HasChildren){
                writer.Write("<");
                PrintList(simpleType.TypeArguments);
                writer.Write(">");
            }
        }

        public void VisitPrimitiveType(PrimitiveType primitiveType)
        {
            //primitiveType.KeywordToken;
        }

        public void VisitReferenceType(ReferenceType referenceType)
        {
        }

        public void VisitMemberType(MemberType memberType)
        {
            memberType.Target.AcceptWalker(this);
            writer.Write("::");
            writer.Write(memberType.MemberName);
        }

        public void VisitPlaceholderType(PlaceholderType placeholderType)
        {
            writer.Write("<null>");
        }

        public void VisitAliasDeclaration(AliasDeclaration aliasDecl)
        {
            writer.Write("alias ");
            aliasDecl.Path.AcceptWalker(this);
            writer.Write(" as ");
            writer.Write(aliasDecl.AliasName);
        }

        public void VisitImportDeclaration(ImportDeclaration importDecl)
        {
        }

        public void VisitFunctionDeclaration(FunctionDeclaration funcDecl)
        {
            writer.Write("def ");
            writer.Write(funcDecl.Name);
            writer.Write("(");
            PrintList(funcDecl.Parameters);
            writer.Write(") -> ");
            funcDecl.ReturnType.AcceptWalker(this);
            writer.Write("{...}");
        }

        public void VisitTypeDeclaration(TypeDeclaration typeDecl)
        {
            writer.Write(typeDecl.ClassType.ToString());
            writer.Write(typeDecl.Name);
            if(typeDecl.BaseTypes.HasChildren){
                writer.Write(" : ");
                PrintList(typeDecl.BaseTypes);
            }
            writer.Write("{...}");
        }

        public void VisitFieldDeclaration(FieldDeclaration fieldDecl)
        {
        }

        public void VisitParameterDeclaration(ParameterDeclaration parameterDecl)
        {
        }

        public void VisitVariableInitializer(VariableInitializer initializer)
        {
        }

        public void VisitWildcardPattern(WildcardPattern wildcardPattern)
        {
        }

        public void VisitIdentifierPattern(IdentifierPattern identifierPattern)
        {
        }

        public void VisitValueBindingPattern(ValueBindingPattern valueBindingPattern)
        {
        }

        public void VisitCollectionPattern(CollectionPattern collectionPattern)
        {

        }

        public void VisitDestructuringPattern(DestructuringPattern destructuringPattern)
        {

        }

        public void VisitTuplePattern(TuplePattern tuplePattern)
        {
        }

        public void VisitExpressionPattern(ExpressionPattern exprPattern)
        {
        }

        public void VisitNewLine(NewLineNode newlineNode)
        {
        }

        public void VisitWhitespace(WhitespaceNode whitespaceNode)
        {
        }

        public void VisitExpressoTokenNode(ExpressoTokenNode tokenNode)
        {
        }

        #endregion
    }
}

