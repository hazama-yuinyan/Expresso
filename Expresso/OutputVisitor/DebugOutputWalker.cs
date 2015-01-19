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

        void PrintList<TObject>(IEnumerable<TObject> list) where TObject : AstNode
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

        void PrintPairList<TObject>(IEnumerable<Tuple<TObject, TObject>> list, Func<string> connector)
            where TObject : AstNode
        {
            int length = list.Count();
            var enumerator = list.GetEnumerator();
            for(int i = 0; i < length; ++i){
                enumerator.MoveNext();
                Tuple<TObject, TObject> pair = enumerator.Current;
                pair.Item1.AcceptWalker(this);
                writer.Write(connector());
                pair.Item2.AcceptWalker(this);
                if(i + 1 != length)
                    writer.Write(", ");
            }
        }

        #region IAstWalker implementation

        public void VisitArgument(ParameterDeclaration arg)
        {
            writer.Write(arg.Name);
            writer.Write(" (- ");
            arg.ReturnType.AcceptWalker(this);
            if(arg.Option != null){
                writer.Write("[= ");
                arg.Option.AcceptWalker(this);
                writer.Write("]");
            }
        }

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
            foreach(var stmt in block.Statements)
                stmt.AcceptWalker(this);
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
            writer.Write(breakStmt.Count);
            writer.Write(">");
        }

        public void VisitContinueStatement(ContinueStatement continueStmt)
        {
            writer.Write("<continue upto ");
            writer.Write(continueStmt.Count);
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

        public void VisitFunctionDefinition(FunctionDeclaration funcDef)
        {
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
            writer.Write(intSeq.Lower);
            writer.Write(" ");
            writer.Write(intSeq.Upper);
            writer.Write(" ");
            writer.Write(intSeq.Step);
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
            throw new NotImplementedException();
        }

        public void VisitImportStatement(ImportDeclaration importStmt)
        {
            writer.Write("<Import: ");
            if(importStmt.AliasNames != null){
                var pairs = importStmt.ModuleNameTokens.Zip(importStmt.AliasNameTokens,
                    (module, alias) => new Tuple<Identifier, Identifier>(module, alias));
                PrintPairList(pairs, () => " as ");
            }else{
                PrintList(importStmt.ModuleNameTokens);
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
            foreach(var label in clause.Labels){
                label.AcceptWalker(this);
                writer.Write(":");
            }

        }

        public void VisitSequence(SequenceExpression seqExpr)
        {
            PrintList(seqExpr.Items);
        }

        /*public void VisitTypeDefinition(TypeDefinition typeDef)
        {
            throw new NotImplementedException();
        }*/

        public void VisitUnaryExpression(UnaryExpression unaryExpr)
        {
            unaryExpr.Operand.AcceptWalker(this);
        }

        public void VisitVarDeclaration(VariableDeclarationStatement varDecl)
        {
            throw new NotImplementedException();
        }

        public void VisitWhileStatement(WhileStatement whileStmt)
        {
            throw new NotImplementedException();
        }

        public void VisitYieldStatement(YieldStatement yieldStmt)
        {
            throw new NotImplementedException();
        }

        public void VisitAstType(AstType typeNode)
        {
            writer.Write(typeNode.ToString());
        }

        public void VisitNullNode(AstNode nullNode)
        {
        }

        public void VisitPatternPlaceholder(AstNode placeholder, ICSharpCode.NRefactory.PatternMatching.Pattern child)
        {
        }

        public void VisitVariableDeclarationStatement(VariableDeclarationStatement varDecl)
        {
            throw new NotImplementedException();
        }

        public void VisitComprehensionExpression(ComprehensionExpression comp)
        {
            throw new NotImplementedException();
        }

        public void VisitComprehensionForClause(ComprehensionForClause compFor)
        {
            throw new NotImplementedException();
        }

        public void VisitComprehensionIfClause(ComprehensionIfClause compIf)
        {
            throw new NotImplementedException();
        }

        public void VisitConditionalExpression(ConditionalExpression condExpr)
        {
            throw new NotImplementedException();
        }

        public void VisitKeyValueLikeExpression(KeyValueLikeExpression keyValue)
        {
            throw new NotImplementedException();
        }

        public void VisitLiteralExpression(LiteralExpression literal)
        {
            throw new NotImplementedException();
        }

        public void VisitIntgerSequenceExpression(IntegerSequenceExpression intSeq)
        {
            throw new NotImplementedException();
        }

        public void VisitIndexerExpression(IndexerExpression indexExpr)
        {
            throw new NotImplementedException();
        }

        public void VisitPathExpression(PathExpression pathExpr)
        {
            throw new NotImplementedException();
        }

        public void VisitParenthesizedExpression(ParenthesizedExpression parensExpr)
        {
            throw new NotImplementedException();
        }

        public void VisitObjectCreationExpression(ObjectCreationExpression creation)
        {
            throw new NotImplementedException();
        }

        public void VisitMatchClause(MatchPatternClause matchClause)
        {
            throw new NotImplementedException();
        }

        public void VisitSelfReferenceExpression(SelfReferenceExpression selfRef)
        {
            throw new NotImplementedException();
        }

        public void VisitSuperReferenceExpression(SuperReferenceExpression superRef)
        {
            throw new NotImplementedException();
        }

        public void VisitCommentNode(CommentNode comment)
        {
            throw new NotImplementedException();
        }

        public void VisitTextNode(TextNode textNode)
        {
            throw new NotImplementedException();
        }

        public void VisitSimpleType(SimpleType simpleType)
        {
            throw new NotImplementedException();
        }

        public void VisitPrimitiveType(PrimitiveType primitiveType)
        {
            throw new NotImplementedException();
        }

        public void VisitImportDeclaration(ImportDeclaration importDecl)
        {
            throw new NotImplementedException();
        }

        public void VisitFunctionDeclaration(FunctionDeclaration funcDecl)
        {
            throw new NotImplementedException();
        }

        public void VisitTypeDeclaration(TypeDeclaration typeDecl)
        {
            throw new NotImplementedException();
        }

        public void VisitFieldDeclaration(FieldDeclaration fieldDecl)
        {
            throw new NotImplementedException();
        }

        public void VisitParameterDeclaration(ParameterDeclaration parameterDecl)
        {
            throw new NotImplementedException();
        }

        public void VisitVariableInitializer(VariableInitializer initializer)
        {
            throw new NotImplementedException();
        }

        public void VisitWildcardPattern(WildcardPattern wildcardPattern)
        {
            throw new NotImplementedException();
        }

        public void VisitIdentifierPattern(IdentifierPattern identifierPattern)
        {
            throw new NotImplementedException();
        }

        public void VisitValueBindingPattern(ValueBindingPattern valueBindingPattern)
        {
            throw new NotImplementedException();
        }

        public void VisitTuplePattern(TuplePattern tuplePattern)
        {
            throw new NotImplementedException();
        }

        public void VisitExpressionPattern(ExpressionPattern exprPattern)
        {
            throw new NotImplementedException();
        }

        public void VisitNewLine(NewLineNode newlineNode)
        {
            throw new NotImplementedException();
        }

        public void VisitWhitespace(WhitespaceNode whitespaceNode)
        {
            throw new NotImplementedException();
        }

        public void VisitExpressoTokenNode(ExpressoTokenNode tokenNode)
        {
            throw new NotImplementedException();
        }

        #endregion
    }
}

