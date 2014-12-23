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
 * The following list names statements that will introduce new variables into the enclosing scope:
 *    TypeDefinition,
 *    FunctionDefinition,
 *    VarDeclaration,
 *    ImportStatement,
 *    TryStatement(CatchClause),
 *    Comprehension(ComprehensionFor expression)
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
            if(forStmt.HasLet){
                forStmt.Target.AcceptWalker(binder);
                foreach(var ident in forStmt.Left.Items.Cast<Identifier>()){
                    ident.Reference = binder.Reference(ident.Name);
                    ident.Reference.Variable = binder.DefineName(ident.Name, InferenceHelper.InferTypeForForStatement(forStmt.Target));
                }
            }
        }

        public override bool VisitSequence(SequenceExpression seqExpr)
		{
			return true;
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
		internal ScopeStatement cur_scope;
		List<ScopeStatement> scopes = new List<ScopeStatement>();
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
			
			cur_scope = global_scope = unboundAst;
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
		
		void PushScope(ScopeStatement node)
		{
			node.Parent = cur_scope;
			cur_scope = node;
			finally_count.Add(0);
		}
		
		void PopScope()
		{
			scopes.Add(cur_scope);
			cur_scope = cur_scope.Parent;
			finally_count.RemoveAt(finally_count.Count - 1);
		}
		
		internal ExpressoReference Reference(string name)
		{
			return cur_scope.Reference(name);
		}
		
		internal ExpressoVariable DefineName(string name, TypeAnnotation type)
		{
			return cur_scope.EnsureVariable(name, type);
		}
		
		internal ExpressoVariable DefineParameter(string name, TypeAnnotation type)
		{
			return cur_scope.DefineParameter(name, type);
		}
		
        internal void ReportSyntaxWarning(string message, AstNode node)
		{
			if(parser != null)
                parser.SemanticError(message, node.StartLocation, -1);
		}
		
		/*internal void ReportSyntaxError(string message, Node node)
		{
			_context.Errors.Add(_context.SourceUnit, message, node.Span, -1, Severity.FatalError);
			throw new FatalError(message, _context.SourceUnit, node.Span, -1);
		}*/
		
		#region AstBinder Overrides

        public void VisitAst(ExpressoAst ast)
        {
            throw new NotImplementedException();
        }

        public void VisitBlock(BlockStatement block)
        {
            block.Statements.AcceptWalker(this);
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

        public void VisitForStatement(ForStatement forStmt)
        {
        }

        public void VisitIfStatement(IfStatement ifStmt)
        {
            throw new NotImplementedException();
        }

        public void VisitImportStatement(ImportStatement importStmt)
        {
            throw new NotImplementedException();
        }

        public void VisitReturnStatement(ReturnStatement returnStmt)
        {
            throw new NotImplementedException();
        }

        public void VisitSwitchStatement(SwitchStatement switchStmt)
        {
            throw new NotImplementedException();
        }

        public void VisitThrowStatement(ThrowStatement throwStmt)
        {
            throw new NotImplementedException();
        }

        public void VisitTryStatement(TryStatement tryStmt)
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

        public void VisitVariableDeclarationStatement(VariableDeclarationStatement varDecl)
        {
            throw new NotImplementedException();
        }

        public void VisitArgument(ParameterDeclaration arg)
        {
            throw new NotImplementedException();
        }

        public void VisitAssignment(AssignmentExpression assignment)
        {
            throw new NotImplementedException();
        }

        public void VisitBinaryExpression(BinaryExpression binaryExpr)
        {
            throw new NotImplementedException();
        }

        public void VisitCallExpression(CallExpression callExpr)
        {
            throw new NotImplementedException();
        }

        public void VisitCastExpression(CastExpression castExpr)
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

        public void VisitLiteralExpression(LiteralExpression literal)
        {
            throw new NotImplementedException();
        }

        public void VisitDefaultExpression(DefaultExpression defaultExpr)
        {
            throw new NotImplementedException();
        }

        public void VisitIdentifier(Identifier ident)
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

        public void VisitLateBinding<T>(LateBindExpression<T> lateBinding) where T : class
        {
            throw new NotImplementedException();
        }

        public void VisitMemberReference(MemberReference memRef)
        {
            throw new NotImplementedException();
        }

        public void VisitNewExpression(NewExpression newExpr)
        {
            throw new NotImplementedException();
        }

        public void VisitParenthesizedExpression(ParenthesizedExpression parensExpr)
        {
            throw new NotImplementedException();
        }

        public void VisitSequenceInitializer(SequenceInitializer seqInitializer)
        {
            throw new NotImplementedException();
        }

        public void VisitCaseClause(CaseClause caseClause)
        {
            throw new NotImplementedException();
        }

        public void VisitSequence(SequenceExpression seqExpr)
        {
            throw new NotImplementedException();
        }

        public void VisitCatchClause(CatchClause catchClause)
        {
            throw new NotImplementedException();
        }

        public void VisitFinallyClause(FinallyClause finallyClause)
        {
            throw new NotImplementedException();
        }

        public void VisitUnaryExpression(UnaryExpression unaryExpr)
        {
            throw new NotImplementedException();
        }

        public void VisitNullReferenceExpression(NullReferenceExpression nullRef)
        {
            throw new NotImplementedException();
        }

        public void VisitThisReferenceExpression(ThisReferenceExpression thisRef)
        {
            throw new NotImplementedException();
        }

        public void VisitBaseReferenceExpression(BaseReferenceExpression baseRef)
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

        public void VisitAstType(AstType typeNode)
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

        public void VisitFunctionDeclaration(FunctionDeclaration funcDecl)
        {
            throw new NotImplementedException();
        }

        public void VisitConstructorDeclaration(ConstructorDeclaration constructor)
        {
            throw new NotImplementedException();
        }

        public void VisitConstructorInitializer(ConstructorInitializer constructorInitializer)
        {
            throw new NotImplementedException();
        }

        public void VisitFieldDeclaration(FieldDeclaration fieldDecl)
        {
            throw new NotImplementedException();
        }

        public void VisitMethodDeclaration(MethodDeclaration methodDecl)
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

        public void VisitNullNode(AstNode nullNode)
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

        public void VisitPatternPlaceholder(AstNode placeholder, ICSharpCode.NRefactory.PatternMatching.Pattern child)
        {
            throw new NotImplementedException();
        }
		
		// TypeDefinition
		public override bool Walk(TypeDefinition node)
		{
			var type = new TypeAnnotation(node.TargetType == DeclType.Class ? ObjectTypes.TypeClass :
			                              node.TargetType == DeclType.Interface ? ObjectTypes.TypeInterface :
			                              ObjectTypes.TypeStruct, node.Name);
			node.ExpressoVariable = DefineName(node.Name, type);
			
			// Base references are in the outer context
			foreach(Expression b in node.Bases)
                b.AcceptWalker(this);
			
			// process the decorators in the outer context
			/*if (node.Decorators != null) {
				foreach (Expression dec in node.Decorators) {
					dec.Walk(this);
				}
			}*/
			
			PushScope(node);
			
			//node.ModuleNameVariable = _globalScope.EnsureGlobalVariable("__name__");
			
			// define the __doc__ and the __module__
			/*if (node.Body.Documentation != null) {
				node.DocVariable = DefineName("__doc__");
			}
			node.ModVariable = DefineName("__module__");
			*/
			// Walk the body
			foreach(var stmt in node.Body)
                stmt.AcceptWalker(this);

			return false;
		}
		
        public override void VisitExpressionStatement(ExpressionStatement exprStmt)
		{
			exprStmt.Parent = cur_scope;
		}
		
		public override bool Walk(BinaryExpression node)
		{
			node.Parent = cur_scope;
		}
		
		public override bool Walk(CallExpression node)
		{
			node.Parent = cur_scope;
			return base.Walk(node);
		}
		
		public override bool Walk(ConditionalExpression node)
		{
			node.Parent = cur_scope;
			return base.Walk(node);
		}
		
		public override bool Walk(ComprehensionExpression node)
		{
			node.Parent = cur_scope;
			return base.Walk(node);
		}
		
		public override bool Walk(ComprehensionIfClause node)
		{
			node.Parent = cur_scope;
			return base.Walk(node);
		}
		
		public override bool Walk(YieldStatement node)
		{
			node.Parent = cur_scope;
			return base.Walk(node);
		}
		
		public override bool Walk(UnaryExpression node)
		{
			node.Parent = cur_scope;
			return base.Walk(node);
		}
		
		public override bool Walk(IntegerSequenceExpression node)
		{
			node.Parent = cur_scope;
			return base.Walk(node);
		}
		
		public override void PostWalk(ConditionalExpression node)
		{
			node.Parent = cur_scope;
			base.PostWalk(node);
		}
		
		public override bool Walk(Constant node)
		{
			node.Parent = cur_scope;
			return base.Walk(node);
		}
		
		public override bool Walk(EmptyStatement node)
		{
			node.Parent = cur_scope;
			return base.Walk(node);
		}
		
		public override bool Walk(ThrowStatement node)
		{
			node.Parent = cur_scope;
			node.InFinally = finally_count[finally_count.Count - 1] != 0;
			return base.Walk(node);
		}
		
		public override bool Walk(BlockStatement node)
		{
			node.Parent = cur_scope;
			return base.Walk(node);
		}
		
		// ForStatement
		public override bool Walk(ForStatement node)
		{
			node.Parent = cur_scope;
			/*if (cur_scope is FunctionDefinition) {
				cur_scope.ShouldInterpret = false;
			}*/
			
			// we only push the loop for the body of the loop
			// so we need to walk the for statement ourselves
			if(node.HasLet)
				node.Left.Walk(define);
			
			if(node.Left != null)
				node.Left.Walk(this);

			if(node.Target != null)
				node.Target.Walk(this);
			
			if(node.Body != null)
				node.Body.Walk(this);
			
			return false;
		}
		
		public override bool Walk(WhileStatement node)
		{
			node.Parent = cur_scope;
			
			// we only push the loop for the body of the loop
			// so we need to walk the while statement ourselves
			if(node.Condition != null)
				node.Condition.Walk(this);
			
			if(node.Body != null)
				node.Body.Walk(this);
			
			return false;
		}
		
		public override bool Walk(BreakStatement node)
		{
			node.Parent = cur_scope;
			
			return base.Walk(node);
		}
		
		public override bool Walk(ContinueStatement node)
		{
			node.Parent = cur_scope;
			
			return base.Walk(node);
		}
		
		public override bool Walk(ReturnStatement node)
		{
			node.Parent = cur_scope;
			FunctionDeclaration func_def = cur_scope as FunctionDeclaration;
			if(func_def != null)
				func_def.HasReturn = true;

			return base.Walk(node);
		}

		public override bool Walk(MemberReference node)
		{
			node.Parent = cur_scope;
			var parent_ident = (Identifier)node.Target;
            return true;
		}

		/*public override bool Walk<T>(LateBindExpression<T> node)
		{
			return base.Walk(node);
		}*/
		
		// WithStatement
		public override bool Walk(WithStatement node)
		{
			/*node.Parent = cur_scope;
			cur_scope.ContainsExceptionHandling = true;
			
			if(node.Variable != null){
				node.Variable.Walk(_define);
			}*/
			return true;
		}
		
		// FromImportStatement
		/*public override bool Walk(FromImportStatement node)
        {
			node.Parent = _currentScope;
			
			if (node.Names != FromImportStatement.Star) {
				PythonVariable[] variables = new PythonVariable[node.Names.Count];
				for (int i = 0; i < node.Names.Count; i++) {
					string name = node.AsNames[i] != null ? node.AsNames[i] : node.Names[i];
					variables[i] = DefineName(name);
				}
				node.Variables = variables;
			} else {
				Debug.Assert(_currentScope != null);
				_currentScope.ContainsImportStar = true;
				_currentScope.NeedsLocalsDictionary = true;
				_currentScope.HasLateBoundVariableSets = true;
			}
			return true;
		}*/
		
		// FunctionDefinition
		public override bool Walk(FunctionDeclaration node)
		{
			//node._nameVariable = global_scope.EnsureGlobalVariable("__name__");
			
			// Name is defined in the enclosing context
			/*if(!node.IsLambda){
				node.PythonVariable = DefineName(node.Name);
			}*/
			
			// process the default arg values in the outer context
			/*foreach(var p in node.Parameters){
				if(p.Option != null){
					p.Option.Walk(this);
				}
			}*/
			// process the decorators in the outer context
			/*if (node.Decorators != null) {
				foreach (Expression dec in node.Decorators) {
					dec.Walk(this);
				}
			}*/
			
			PushScope(node);
			
			foreach(var p in node.Parameters)
				p.Walk(parameter);
			
			node.Body.Walk(this);
			return false;
		}
		
		// FunctionDefinition
		public override void PostWalk(FunctionDeclaration node)
        {
			PopScope();
		}
		
		public override bool Walk(Identifier node)
		{
			node.Parent = cur_scope;
			node.Reference = Reference(node.Name);
			return true;
		}
		
		public override bool Walk(IfStatement node)
		{
			node.Parent = cur_scope;
			return base.Walk(node);
		}
		
		public override bool Walk(AssertStatement node)
		{
			node.Parent = cur_scope;
			return base.Walk(node);
		}        
		
		// ExpressoAst
		public override bool Walk(ExpressoAst node)
		{
			/*if(node){
				node.NameVariable = DefineName("__name__");
				node.FileVariable = DefineName("__file__");
				node.DocVariable = DefineName("__doc__");
				
				// commonly used module variables that we want defined for optimization purposes
				DefineName("__path__");
				DefineName("__builtins__");
				DefineName("__package__");
			}*/
			return true;
		}
		
		// ExpressoAst
		public override void PostWalk(ExpressoAst node)
		{
			// Do not add the global suite to the list of processed nodes,
			// the publishing must be done after the class local binding.
			Debug.Assert(cur_scope == node);
			cur_scope = cur_scope.Parent;
			finally_count.RemoveAt(finally_count.Count - 1);
		}
		
		// ImportStatement
		public override bool Walk(ImportStatement node)
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

