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
 *    RequireStatement,
 *    TryStatement(CatchClause),
 *    Comprehension(ComprehensionFor expression)
 */

namespace Expresso.Ast
{
	class DefineBinder : AstWalkerNonRecursive
	{
		ExpressoNameBinder binder;
		public DefineBinder(ExpressoNameBinder nameBinder)
		{
			binder = nameBinder;
		}
		public override bool Walk(Identifier ident)
		{
            binder.DefineName(ident.Name, ident.ParamType);
			return false;
		}

        public override bool Walk(MemberReference memRef)
        {
            //return new LateBindExpression<Runtime.Types.BuiltinFunction>(node);
            return true;
        }

        public override bool Walk(VarDeclaration varDecl)
        {
            foreach(var item in varDecl.Left.Zip(varDecl.Expressions, (lhs, rhs) => new Tuple<Identifier, Expression>(lhs, rhs))){
                item.Item1.Reference = binder.Reference(item.Item1.Name);
                item.Item2.Walk(binder);
                item.Item1.Reference.Variable = binder.DefineName(item.Item1.Name, InferenceHelper.InferType(item.Item2));
            }
            return false;
        }

        public override bool Walk(ForStatement forStmt)
        {
            if(forStmt.HasLet){
                forStmt.Target.Walk(binder);
                foreach(var ident in forStmt.Left.Items.Cast<Identifier>()){
                    ident.Reference = binder.Reference(ident.Name);
                    ident.Reference.Variable = binder.DefineName(ident.Name, InferenceHelper.InferTypeForForStatement(forStmt.Target));
                }
            }
            return base.Walk(forStmt);
        }

		public override bool Walk(SequenceExpression seqExpr)
		{
			return true;
		}
	}
	
	class ParameterBinder : AstWalkerNonRecursive
	{
		ExpressoNameBinder binder;
		public ParameterBinder(ExpressoNameBinder nameBinder)
		{
			binder = nameBinder;
		}
		
		public override bool Walk(Argument node)
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
	
	class ExpressoNameBinder : AstWalker
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
			unboundAst.Walk(this);
			
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
		
		internal void ReportSyntaxWarning(string message, Node node)
		{
			if(parser != null)
				parser.SemanticError(message, node.Span, -1);
		}
		
		/*internal void ReportSyntaxError(string message, Node node)
		{
			_context.Errors.Add(_context.SourceUnit, message, node.Span, -1, Severity.FatalError);
			throw new FatalError(message, _context.SourceUnit, node.Span, -1);
		}*/
		
		#region AstBinder Overrides
		// AssignmentStatement
		public override bool Walk(Assignment node)
		{
			node.Parent = cur_scope;
			return base.Walk(node);
		}
		
		/*public override bool Walk( node) {
			node.Parent = _currentScope;
			node.Left.Walk(_define);
			return true;
		}*/
		
		public override void PostWalk(Call node)
		{
			/*if(node.NeedsLocalsDictionary()){
				_currentScope.NeedsLocalsDictionary = true;
			}*/
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
                b.Walk(this);
			
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
				stmt.Walk(this);

			return false;
		}
		
		// TypeDefinition
		public override void PostWalk(TypeDefinition node)
		{
			Debug.Assert(node == cur_scope);
			PopScope();
		}
		
		public override bool Walk(ExprStatement node)
		{
			node.Parent = cur_scope;
			return base.Walk(node);
		}
		
		public override bool Walk(BinaryExpression node)
		{
			node.Parent = cur_scope;
			return base.Walk(node);
		}
		
		public override bool Walk(Call node)
		{
			node.Parent = cur_scope;
			return base.Walk(node);
		}
		
		public override bool Walk(ConditionalExpression node)
		{
			node.Parent = cur_scope;
			return base.Walk(node);
		}
		
		public override bool Walk(Comprehension node)
		{
			node.Parent = cur_scope;
			return base.Walk(node);
		}
		
		public override bool Walk(ComprehensionIf node)
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
		
		public override bool Walk(IntSeqExpression node)
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
		
		public override bool Walk(Block node)
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
			FunctionDefinition func_def = cur_scope as FunctionDefinition;
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
		public override bool Walk(FunctionDefinition node)
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
		public override void PostWalk(FunctionDefinition node)
        {
			PopScope();
		}
		
		public override bool Walk(Identifier node)
		{
			node.Parent = cur_scope;
			node.Reference = Reference(node.Name);
			return true;
		}
		
		public override bool Walk(PrintStatement node)
		{
			node.Parent = cur_scope;
			return base.Walk(node);
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
		
		// RequireStatement
		public override bool Walk(RequireStatement node)
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
		public override bool Walk(VarDeclaration node)
		{
			node.Parent = cur_scope;
            node.Walk(define);
			return true;
		}
		
		// ListComprehensionFor
		public override bool Walk(ComprehensionFor node)
		{
			node.Parent = cur_scope;
			node.Left.Walk(define);
			return true;
		}
		#endregion
	}
}

