using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

using Expresso.Compiler;
using Expresso.Compiler.Meta;

/*
 * The name binding and type resolving:
 *
 * The name binding happens in 2 passes.
 * In the first pass (full recursive walk of the AST) we resolve locals and inference types,
 * meaning when reaching a variable whose type is expressed as inference type, it will try to
 * infer the type of that variable.
 * The second pass uses the "processed" list of all context statements (functions and class
 * bodies) and has each context statement resolve its free variables to determine whether
 * they are globals or references to lexically enclosing scopes.
 *
 * The second pass happens in post-order (the context statement is added into the "processed"
 * list after processing its nested functions/statements). This way, when the function is
 * processing its free variables, it also knows already which of its locals are being lifted
 * to the closure and can report error if such closure variable is being deleted.
 * 
 * The following list names constructs that will introduce new variables into the enclosing scope:
 *    TypeDeclaration,
 *    FunctionDeclaration,
 *    VariableDeclarationStatement,
 *    ImportDeclaration,
 *    Comprehension(ComprehensionForClause expression)
 */

namespace Expresso.Ast.Analysis
{
    class ExpressoNameBinder : IAstWalker
	{
		ExpressoAst global_scope;
        SymbolTable symbol_table;
		Parser parser;
		
		ExpressoNameBinder(Parser parentParser)
		{
			parser = parentParser;
            symbol_table = new SymbolTable();
		}
		
		#region Public surface
		internal static void BindAst(ExpressoAst ast, Parser parser)
		{
            Debug.Assert(ast != null);
			
			ExpressoNameBinder binder = new ExpressoNameBinder(parser);
			binder.Bind(ast);
		}
		#endregion
		
		void Bind(ExpressoAst unboundAst)
		{
            Debug.Assert(unboundAst != null);
			
			global_scope = unboundAst;
			
			// Find all scopes and variables
            unboundAst.AcceptWalker(this);
			
			// Bind
			foreach(ScopeStatement scope in scopes)
				scope.Bind(this);
			
			// Bind the globals
			unboundAst.Bind(this);
			
			// Finish Binding w/ outer most scopes first.
			for(int i = scopes.Count - 1; i >= 0; --i)
				scopes[i].FinishBind(this);
			
			// Finish the globals
			unboundAst.FinishBind(this);
			
			// Run flow checker
			foreach(ScopeStatement scope in scopes)
				FlowChecker.Check(scope);
		}
		
        void PushScope(AstNode node)
		{
            //node.Parent = cur_scope;
            //cur_scope = node;
		}
		
		void PopScope()
		{
            //scopes.Add(cur_scope);
            //cur_scope = cur_scope.Parent;
		}
		
        internal void ReportSyntaxWarning(string message, AstNode node)
		{
            if(parser != null){
                var real_message = "{0} " + message;
                parser.SemanticError(real_message, node.StartLocation);
            }
		}
		
		/*internal void ReportSyntaxError(string message, Node node)
		{
			_context.Errors.Add(_context.SourceUnit, message, node.Span, -1, Severity.FatalError);
			throw new FatalError(message, _context.SourceUnit, node.Span, -1);
		}*/
		
		#region AstBinder Overrides

        public void VisitAst(ExpressoAst ast)
        {
            ast.Imports.AcceptWalker(this);
            ast.Body.AcceptWalker(this);
        }

        public void VisitBlock(BlockStatement block)
        {
            block.Statements.AcceptWalker(this);
        }

        public void VisitBreakStatement(BreakStatement breakStmt)
        {
            //no op
        }

        public void VisitContinueStatement(ContinueStatement continueStmt)
        {
            //no op
        }

        public void VisitEmptyStatement(EmptyStatement emptyStmt)
        {
            //no op
        }

        public void VisitForStatement(ForStatement forStmt)
        {
            forStmt.Left.AcceptWalker(this);
            forStmt.Target.AcceptWalker(this);
            forStmt.Body.AcceptWalker(this);
        }

        public void VisitIfStatement(IfStatement ifStmt)
        {
            ifStmt.Condition.AcceptWalker(this);
            ifStmt.TrueBlock.AcceptWalker(this);
            ifStmt.FalseBlock.AcceptWalker(this);
        }

        public void VisitImportStatement(ImportDeclaration importStmt)
        {
            importStmt.ModuleNameTokens.AcceptWalker(this);
            importStmt.AliasNameTokens.AcceptWalker(this);
        }

        public void VisitReturnStatement(ReturnStatement returnStmt)
        {
            returnStmt.Expression.AcceptWalker(this);
        }

        public void VisitMatchStatement(MatchStatement matchStmt)
        {
            matchStmt.Target.AcceptWalker(this);
            matchStmt.Clauses.AcceptWalker(this);
        }

        public void VisitWhileStatement(WhileStatement whileStmt)
        {
            whileStmt.Condition.AcceptWalker(this);
            whileStmt.Body.AcceptWalker(this);
        }

        public void VisitYieldStatement(YieldStatement yieldStmt)
        {
            yieldStmt.Expression.AcceptWalker(this);
        }

        public void VisitVariableDeclarationStatement(VariableDeclarationStatement varDecl)
        {
            varDecl.Variables.AcceptWalker(this);
        }

        public void VisitArgument(ParameterDeclaration arg)
        {
        }

        public void VisitAssignment(AssignmentExpression assignment)
        {
            assignment.Left.AcceptWalker(this);
            assignment.Right.AcceptWalker(this);
        }

        public void VisitBinaryExpression(BinaryExpression binaryExpr)
        {
            binaryExpr.Left.AcceptWalker(this);
            binaryExpr.Right.AcceptWalker(this);
        }

        public void VisitCallExpression(CallExpression callExpr)
        {
            callExpr.Target.AcceptWalker(this);
            callExpr.Arguments.AcceptWalker(this);
        }

        public void VisitCastExpression(CastExpression castExpr)
        {
            castExpr.Target.AcceptWalker(this);
            castExpr.ToExpression.AcceptWalker(this);
        }

        public void VisitComprehensionExpression(ComprehensionExpression comp)
        {
            comp.Item.AcceptWalker(this);
            comp.Body.Left.AcceptWalker(define);
            comp.Body.Target.AcceptWalker(this);
            comp.Body.Body.AcceptWalker(this);
        }

        public void VisitComprehensionForClause(ComprehensionForClause compFor)
        {
            compFor.Left.AcceptWalker(this);
            compFor.Target.AcceptWalker(this);
            compFor.Body.AcceptWalker(this);
        }

        public void VisitComprehensionIfClause(ComprehensionIfClause compIf)
        {
            compIf.Condition.AcceptWalker(this);
            compIf.Body.AcceptWalker(this);
        }

        public void VisitConditionalExpression(ConditionalExpression condExpr)
        {
            condExpr.Condition.AcceptWalker(this);
            condExpr.TrueExpression.AcceptWalker(this);
            condExpr.FalseExpression.AcceptWalker(this);
        }

        public void VisitLiteralExpression(LiteralExpression literal)
        {
            //no op
        }

        public void VisitIdentifier(Identifier ident)
        {
            if(!symbol_table.IsSymbol(ident.Name))
                ReportSyntaxWarning(string.Format("Unknown identifier {0}", ident.Name), ident);
        }

        public void VisitIntgerSequenceExpression(IntegerSequenceExpression intSeq)
        {
            intSeq.Lower.AcceptWalker(this);
            intSeq.Upper.AcceptWalker(this);
            intSeq.Step.AcceptWalker(this);
        }

        public void VisitIndexerExpression(IndexerExpression indexExpr)
        {
            indexExpr.Target.AcceptWalker(this);
            indexExpr.Arguments.AcceptWalker(this);
        }

        public void VisitMemberReference(MemberReference memRef)
        {
            memRef.Target.AcceptWalker(this);
            memRef.Subscription.AcceptWalker(this);
        }

        public void VisitNewExpression(NewExpression newExpr)
        {
            newExpr.CreationExpression.AcceptWalker(this);
        }

        public void VisitParenthesizedExpression(ParenthesizedExpression parensExpr)
        {
            parensExpr.Expression.AcceptWalker(this);
        }

        public void VisitSequenceInitializer(SequenceInitializer seqInitializer)
        {
            seqInitializer.Items.AcceptWalker(this);
        }

        public void VisitCaseClause(MatchPatternClause caseClause)
        {
            caseClause.Labels.AcceptWalker(this);
            caseClause.Body.AcceptWalker(this);
        }

        public void VisitSequence(SequenceExpression seqExpr)
        {
            seqExpr.Items.AcceptWalker(this);
        }

        public void VisitUnaryExpression(UnaryExpression unaryExpr)
        {
            unaryExpr.Operand.AcceptWalker(this);
        }

        public void VisitThisReferenceExpression(SelfReferenceExpression thisRef)
        {
        }

        public void VisitBaseReferenceExpression(SuperReferenceExpression baseRef)
        {
        }

        public void VisitCommentNode(CommentNode comment)
        {
        }

        public void VisitTextNode(TextNode textNode)
        {
        }

        public void VisitAstType(AstType typeNode)
        {
        }

        public void VisitSimpleType(SimpleType simpleType)
        {
        }

        public void VisitPrimitiveType(PrimitiveType primitiveType)
        {
        }

        public void VisitFunctionDeclaration(FunctionDeclaration funcDecl)
        {
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

        public void VisitNullNode(AstNode nullNode)
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

        public void VisitPatternPlaceholder(AstNode placeholder, ICSharpCode.NRefactory.PatternMatching.Pattern child)
        {
        }

        public void VisitExpressionStatement(ExpressionStatement exprStmt)
        {
            throw new NotImplementedException();
        }

        public void VisitKeyValueLikeExpression(KeyValueLikeExpression keyValue)
        {
            throw new NotImplementedException();
        }

        public void VisitPathExpression(PathExpression pathExpr)
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

        public void VisitImportDeclaration(ImportDeclaration importDecl)
        {
            throw new NotImplementedException();
        }

        public void VisitTypeDeclaration(TypeDeclaration typeDecl)
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
		#endregion
	}
}

