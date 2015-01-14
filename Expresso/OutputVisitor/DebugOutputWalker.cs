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
            const int length = list.Count();
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
            const int length = list.Count();
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
            arg.Type.AcceptWalker(this);
            if(arg.Option != null){
                writer.Write("[= ");
                arg.Option.AcceptWalker(this);
                writer.Write("]");
            }
        }

        public void VisitAssertStatement(AssertStatement assertStmt)
        {
            writer.Write("<Assert: ");
            assertStmt.Test.AcceptWalker(this);
            writer.Write(">");
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

        public void VisitDefaultExpression(DefaultExpression defaultExpr)
        {
            writer.Write("<default: ");
            defaultExpr.TargetType.AcceptWalker(this);
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
            writer.Write(ident.ParamType.TypeName);
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

        public void VisitLateBinding<T>(LateBindExpression<T> lateBinding) where T : class
        {
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
            writer.Write("(");
            PrintList(newExpr.Arguments);
            writer.Write(")");
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

        public void VisitSwitchStatement(MatchStatement switchStmt)
        {
            writer.Write("<switch: (");
            switchStmt.Target.AcceptWalker(this);
            writer.WriteLine("){");
            foreach(var case_clause in switchStmt.Clauses)
                case_clause.AcceptWalker(this);

            writer.Write("}");
        }

        public void VisitCaseClause(MatchPatternClause caseClause)
        {
            writer.Write("case ");
            foreach(var label in caseClause.Labels){
                label.AcceptWalker(this);
                writer.Write(":");
            }

        }

        public void VisitThrowStatement(ThrowStatement throwStmt)
        {
            writer.Write("<Throw: ");
            throwStmt.Expression.AcceptWalker(this);
            writer.Write(">");
        }

        public void VisitTryStatement(TryStatement tryStmt)
        {
            writer.WriteLine("<Try: ");
            tryStmt.Body.AcceptWalker(this);
            foreach(var catch_clause in tryStmt.Catches)
                catch_clause.AcceptWalker(this);

            tryStmt.FinallyClause.AcceptWalker(this);
        }

        public void VisitSequence(SequenceExpression seqExpr)
        {
            PrintList(seqExpr.Items);
        }

        public void VisitCatchClause(CatchClause catchClause)
        {
            writer.Write("<Catch: ");
            catchClause.CatcherToken.AcceptWalker(this);
            catchClause.Body.AcceptWalker(this);
        }

        public void VisitFinallyClause(FinallyClause finallyClause)
        {
            writer.Write("<Finally: ");
            finallyClause.Body.AcceptWalker(this);
        }

        public void VisitTypeDefinition(TypeDefinition typeDef)
        {
            throw new NotImplementedException();
        }

        public void VisitUnaryExpression(UnaryExpression unaryExpr)
        {
            throw new NotImplementedException();
        }

        public void VisitVarDeclaration(VariableDeclarationStatement varDecl)
        {
            throw new NotImplementedException();
        }

        public void VisitWhileStatement(WhileStatement whileStmt)
        {
            throw new NotImplementedException();
        }

        public void VisitWithStatement(WithStatement withStmt)
        {
            throw new NotImplementedException();
        }

        public void VisitYieldStatement(YieldStatement yieldStmt)
        {
            throw new NotImplementedException();
        }

        public void VisitAstType(AstType typeNode)
        {
            throw new NotImplementedException();
        }

        public void VisitNullNode(AstNode nullNode)
        {
            throw new NotImplementedException();
        }

        public void VisitPatternPlaceholder(AstNode placeholder, ICSharpCode.NRefactory.PatternMatching.Pattern child)
        {
            throw new NotImplementedException();
        }

        #endregion
    }
}

