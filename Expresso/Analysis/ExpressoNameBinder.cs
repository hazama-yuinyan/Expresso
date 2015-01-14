using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

using Expresso.Compiler;
using Expresso.Compiler.Meta;
using Expresso.Parsing;
using Expresso.Utils;

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
 *    TryStatement(CatchClause),
 *    Comprehension(ComprehensionForClause expression)
 */

namespace Expresso.Ast
{
    class DefineBinder : IAstWalker
	{
		ExpressoNameBinder binder;
		public DefineBinder(ExpressoNameBinder nameBinder)
		{
			binder = nameBinder;
		}

        public bool VisitIdentifier(Identifier ident)
		{
            binder.DefineName(ident.Name, ident.Type);
			return false;
		}

        public bool VisitMemberReference(MemberReference memRef)
        {
            //return new LateBindExpression<Runtime.Types.BuiltinFunction>(node);
            return true;
        }

        public bool VisitVarDecl(VariableDeclarationStatement varDecl)
        {
            foreach(var item in varDecl.Left.Zip(varDecl.Expressions, (lhs, rhs) => new Tuple<Identifier, Expression>(lhs, rhs))){
                item.Item1.Reference = binder.Reference(item.Item1.Name);
                item.Item2.AcceptWalker(binder);
                item.Item1.Reference.Variable = binder.DefineName(item.Item1.Name, InferenceHelper.InferType(item.Item2));
            }
            return false;
        }

        public bool VisitForStatement(ForStatement forStmt)
        {
            forStmt.Target.AcceptWalker(binder);
            foreach(var ident in forStmt.Left.Items.Cast<Identifier>()){
                ident.Reference = binder.Reference(ident.Name);
                ident.Reference.Variable = binder.DefineName(ident.Name, InferenceHelper.InferTypeForForStatement(forStmt.Target));
            }
        }

        public override bool VisitSequence(SequenceExpression seqExpr)
		{
			return true;
		}

        public void VisitAst(ExpressoAst ast)
        {
        }

        public void VisitBlock(BlockStatement block)
        {
        }

        public void VisitBreakStatement(BreakStatement breakStmt)
        {
        }

        public void VisitContinueStatement(ContinueStatement continueStmt)
        {
        }

        public void VisitEmptyStatement(EmptyStatement emptyStmt)
        {
        }

        public void VisitExpressionStatement(ExpressionStatement exprStmt)
        {
        }

        public void VisitIfStatement(IfStatement ifStmt)
        {
        }

        public void VisitReturnStatement(ReturnStatement returnStmt)
        {
        }

        public void VisitSwitchStatement(MatchStatement switchStmt)
        {
        }

        public void VisitThrowStatement(ThrowStatement throwStmt)
        {
        }

        public void VisitTryStatement(TryStatement tryStmt)
        {
        }

        public void VisitWhileStatement(WhileStatement whileStmt)
        {
        }

        public void VisitWithStatement(WithStatement withStmt)
        {
        }

        public void VisitYieldStatement(YieldStatement yieldStmt)
        {
        }

        public void VisitVariableDeclarationStatement(VariableDeclarationStatement varDecl)
        {
        }

        public void VisitArgument(ParameterDeclaration arg)
        {
            binder.DefineName(arg.Name, arg.Type);
        }

        public void VisitAssignment(AssignmentExpression assignment)
        {
        }

        public void VisitBinaryExpression(BinaryExpression binaryExpr)
        {
        }

        public void VisitCallExpression(CallExpression callExpr)
        {
        }

        public void VisitCastExpression(CastExpression castExpr)
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

        public void VisitLiteralExpression(LiteralExpression literal)
        {
        }

        public void VisitIntgerSequenceExpression(IntegerSequenceExpression intSeq)
        {
        }

        public void VisitIndexerExpression(IndexerExpression indexExpr)
        {
        }

        public void VisitNewExpression(NewExpression newExpr)
        {
        }

        public void VisitParenthesizedExpression(ParenthesizedExpression parensExpr)
        {
        }

        public void VisitObjectCreationExpression(ObjectCreationExpression creation)
        {
        }

        public void VisitSequenceInitializer(SequenceInitializer seqInitializer)
        {
        }

        public void VisitCaseClause(MatchPatternClause caseClause)
        {
        }

        public void VisitCatchClause(CatchClause catchClause)
        {
        }

        public void VisitFinallyClause(FinallyClause finallyClause)
        {
        }

        public void VisitUnaryExpression(UnaryExpression unaryExpr)
        {
        }

        public void VisitSelfReferenceExpression(SelfReferenceExpression selfRef)
        {
        }

        public void VisitSuperReferenceExpression(SuperReferenceExpression superRef)
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

        public void VisitImportDeclaration(ImportDeclaration importDecl)
        {
            foreach(var pair in importDecl.ModuleNames.Zip(importDecl.AliasNames,
                (first, second) => new Tuple<string, string>(first, second))){
                binder.DefineName(pair.Item1, );
            }
        }

        public void VisitFunctionDeclaration(FunctionDeclaration funcDecl)
        {
        }

        public void VisitTypeDeclaration(TypeDeclaration typeDecl)
        {
            binder.DefineName(typeDecl.Name, typeDecl.ReturnType);
        }

        public void VisitFieldDeclaration(FieldDeclaration fieldDecl)
        {
            binder.DefineName(fieldDecl.Name, fieldDecl.ReturnType);
        }

        public void VisitMethodDeclaration(MethodDeclaration methodDecl)
        {
        }

        public void VisitParameterDeclaration(ParameterDeclaration parameterDecl)
        {
            binder.DefineName(parameterDecl.Name, parameterDecl.Type);
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
	}
	
    class ParameterBinder : IAstWalker
	{
		ExpressoNameBinder binder;
		public ParameterBinder(ExpressoNameBinder nameBinder)
		{
			binder = nameBinder;
		}
		
        public override bool VisitParameter(ParameterDeclaration node)
		{
			node.ExpressoVariable = binder.DefineParameter(node.Name,
			                                               node.ParamType == TypeAnnotation.InferenceType ? TypeAnnotation.VariantType :
			                                               		node.ParamType);
			return false;
		}
		/*public override bool Walk(SublistParameter node) {
			node.PythonVariable = _binder.DefineParameter(node.Name);
			// we walk the node by hand to avoid walking the default values.
			WalkTuple(node.Tuple);
			return false;
		}
		
		void WalkTuple(TupleExpression tuple) {
			tuple.Parent = _binder._currentScope;
			foreach (Expression innerNode in tuple.Items) {
				NameExpression name = innerNode as NameExpression;
				if (name != null) {
					_binder.DefineName(name.Name);
					name.Parent = _binder._currentScope;
					name.Reference = _binder.Reference(name.Name);
				} else {                    
					WalkTuple((TupleExpression)innerNode);
				}
			}
		}
		public override bool Walk(TupleExpression node) {
			node.Parent = _binder._currentScope;
			return true;
		}*/
	}
	
    class ExpressoNameBinder : IAstWalker
	{
		ExpressoAst global_scope;
        SymbolTable symbol_table;
		List<int> finally_count = new List<int>();
		Parser parser;
		
		#region Recursive binders
		DefineBinder define;
		ParameterBinder parameter;
		#endregion
		
		ExpressoNameBinder(Parser parentParser)
		{
			define = new DefineBinder(this);
			//_delete = new DeleteBinder(this);
			parameter = new ParameterBinder(this);
			parser = parentParser;
            symbol_table = new SymbolTable();
		}
		
		#region Public surface
		internal static void BindAst(ExpressoAst ast, Parser parser)
		{
			Assert.NotNull(ast);
			
			ExpressoNameBinder binder = new ExpressoNameBinder(parser);
			binder.Bind(ast);
		}
		#endregion
		
		void Bind(ExpressoAst unboundAst)
		{
			Assert.NotNull(unboundAst);
			
			global_scope = unboundAst;
			finally_count.Add(0);
			
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
            symbol_table.AddScope();
			finally_count.Add(0);
		}
		
		void PopScope()
		{
            //scopes.Add(cur_scope);
            //cur_scope = cur_scope.Parent;
            symbol_table.RemoveScope();
			finally_count.RemoveAt(finally_count.Count - 1);
		}
		
        internal void DefineName(string name, AstType type)
		{
            symbol_table.AddSymbol(name, type);
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
            foreach(var scope_var in forStmt.Left.Items)
                scope_var.AcceptWalker(define);

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

        public void VisitSwitchStatement(MatchStatement switchStmt)
        {
            switchStmt.Target.AcceptWalker(this);
            switchStmt.Clauses.AcceptWalker(this);
        }

        public void VisitThrowStatement(ThrowStatement throwStmt)
        {
            throwStmt.Expression.AcceptWalker(this);
        }

        public void VisitTryStatement(TryStatement tryStmt)
        {
            tryStmt.Catches.AcceptWalker(this);
            tryStmt.Body.AcceptWalker(this);
            tryStmt.FinallyClause.AcceptWalker(this);
        }

        public void VisitWhileStatement(WhileStatement whileStmt)
        {
            whileStmt.Condition.AcceptWalker(this);
            whileStmt.Body.AcceptWalker(this);
        }

        public void VisitWithStatement(WithStatement withStmt)
        {
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

        public void VisitDefaultExpression(DefaultExpression defaultExpr)
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
            newExpr.Arguments.AcceptWalker(this);
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

        public void VisitCatchClause(CatchClause catchClause)
        {
            catchClause.AcceptWalker(define);
            catchClause.Body.AcceptWalker(this);
        }

        public void VisitFinallyClause(FinallyClause finallyClause)
        {
            finallyClause.Body.AcceptWalker(this);
        }

        public void VisitUnaryExpression(UnaryExpression unaryExpr)
        {
            unaryExpr.Operand.AcceptWalker(this);
        }

        public void VisitNullReferenceExpression(NullReferenceExpression nullRef)
        {
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

        public void VisitConstructorDeclaration(ConstructorDeclaration constructor)
        {
        }

        public void VisitConstructorInitializer(ConstructorInitializer constructorInitializer)
        {
        }

        public void VisitFieldDeclaration(FieldDeclaration fieldDecl)
        {
            fieldDecl.AcceptWalker(define);
        }

        public void VisitMethodDeclaration(MethodDeclaration methodDecl)
        {
            methodDecl.AcceptWalker(define);
        }

        public void VisitParameterDeclaration(ParameterDeclaration parameterDecl)
        {
            parameterDecl.AcceptWalker(define);
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
		
		public override bool Walk(ThrowStatement node)
		{
			node.Parent = cur_scope;
			node.InFinally = finally_count[finally_count.Count - 1] != 0;
			return base.Walk(node);
		}
		
		public override void PostWalk(ExpressoAst node)
		{
			// Do not add the global suite to the list of processed nodes,
			// the publishing must be done after the class local binding.
			Debug.Assert(cur_scope == node);
			cur_scope = cur_scope.Parent;
			finally_count.RemoveAt(finally_count.Count - 1);
		}
		
		// ImportStatement
		public override bool Walk(ImportDeclaration node)
		{
			node.Parent = cur_scope;
			
			ExpressoVariable[] variables = new ExpressoVariable[node.ModuleNames.Length];
			for(int i = 0; i < node.ModuleNames.Length; ++i){
				string name = node.AliasNames[i] != null ? node.AliasNames[i] : node.ModuleNames[i];
				variables[i] = DefineName(name, new TypeAnnotation(ObjectTypes.TypeModule, node.ModuleNames[i]));
			}
			node.Variables = variables;
			return true;
		}
		
		// TryStatement
		public override bool Walk(TryStatement node)
		{
			// we manually walk the TryStatement so we can track finally blocks.
			node.Parent = cur_scope;
			cur_scope.ContainsExceptionHandling = true;
			
			node.Body.Walk(this);
			
			if(node.Catches != null){
				foreach(var handler in node.Catches){
					if(handler.Catcher != null)
						handler.Catcher.Walk(define);
					
					handler.Walk(this);
				}
			}
			
			if(node.FinallyClause != null){
				finally_count[finally_count.Count - 1]++;
				node.FinallyClause.Walk(this);
				finally_count[finally_count.Count - 1]--;
			}
			
			return false;
		}

		// VarDecl
		public override bool Walk(VariableDeclarationStatement node)
		{
			node.Parent = cur_scope;
            node.Walk(define);
			return true;
		}
		
		// ListComprehensionFor
		public override bool Walk(ComprehensionForClause node)
		{
			node.Parent = cur_scope;
			node.Left.Walk(define);
			return true;
		}
		#endregion
	}
}

