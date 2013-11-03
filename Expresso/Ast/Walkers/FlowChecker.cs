using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;

using Expresso.Compiler;

/*
 * The data flow.
 * 
 * Each local name is represented as 2 bits:
 * One is for definitive assignment, the other is for uninitialized use detection.
 * The only difference between the two is behavior on delete.
 * On delete, the name is not assigned to meaningful value (we need to check at runtime if it's initialized),
 * but it is not uninitialized either (because delete statement will set it to Uninitialized.instance).
 * This way, codegen doesn't have to emit an explicit initialization for it.
 * 
 * Consider:
 * 
 * def f():
 *     print a  # uninitialized use
 *     a = 10
 * 
 * We compile this into:
 * 
 * static void f$f0() {
 *     object a = Uninitialized.instance; // explicit initialization because of the uninitialized use
 *     // print statement
 *     if(a == Uninitialized.instance)
 *       throw ThrowUnboundLocalError("a");
 *     else
 *       Ops.Print(a);
 *     // a = 10
 *     a = 10
 * }
 * 
 * Whereas:
 * 
 * def f():
 *     a = 10
 *     del a        # explicit deletion which will set to Uninitialized.instance
 *     print a
 * 
 * compiles into:
 * 
 * static void f$f0() {
 *     object a = 10;                        // a = 10
 *     a = Uninitialized.instance;           // del a
 *     if(a == Uninitialized.instance)       // print a
 *       throw ThrowUnboundLocalError("a");
 *     else
 *       Ops.Print(a);
 * }
 * 
 * The bit arrays in the flow checker hold the state and upon encountering NameExpr we figure
 * out whether the name has not yet been initialized at all (in which case we need to emit the
 * first explicit assignment to Uninitialized.instance and guard the use with an inlined check
 * or whether it is definitely assigned (we don't need to inline the check)
 * or whether it may be uninitialized, in which case we must only guard the use by inlining the Uninitialized check
 * 
 * More details on the bits.
 * 
 * First bit (variable is assigned a value):
 *  1 .. variable is definitely assigned to a value
 *  0 .. variable is not assigned to a value at this point (it could have been deleted or just not assigned yet)
 * Second bit (variable is assigned a value or is deleted):
 *  1 .. variable is definitely initialized (either by a value or by deletion)
 *  0 .. variable is not initialized at this point (neither assigned nor deleted)
 * 
 * Valid combinations:
 *  11 .. initialized
 *  01 .. deleted
 *  00 .. may not be initialized
 */

namespace Expresso.Ast
{
	class FlowDefiner : ExpressoWalkerNonRecursive
	{
		readonly FlowChecker fc;
		
		public FlowDefiner(FlowChecker flowChecker)
		{
			fc = flowChecker;
		}
		
		public override bool Walk(Identifier node)
		{
			fc.Define(node.Name);
			return false;
		}
		
		public override bool Walk(MemberReference node)
		{
			node.Walk(fc);
			return false;
		}
		
		public override bool Walk(SequenceExpression node)
		{
			return true;
		}
		
		public override bool Walk(Argument node)
		{
			fc.Define(node.Name);
			return true;
		}
	}
	
	class FlowChecker : ExpressoWalker
	{
		BitArray bits;
		Stack<BitArray> loops;
		Dictionary<string, ExpressoVariable> variables;
		
		readonly ScopeStatement scope;
		readonly FlowDefiner fdef;
		
		FlowChecker(ScopeStatement scopeStmt)
		{
			variables = scopeStmt.Variables;
			bits = new BitArray(variables.Count * 2);
			int index = 0;
			foreach(var binding in variables){
				binding.Value.Offset = index++;
			}
			scope = scopeStmt;
			fdef = new FlowDefiner(this);
		}
		
		[Conditional("DEBUG")]
		public void Dump(BitArray bits)
		{
			System.Text.StringBuilder sb = new System.Text.StringBuilder();
			sb.AppendFormat("FlowChecker ({0})", scope is FunctionDefinition ? ((FunctionDefinition)scope).Name :
			                scope is TypeDefinition ? ((TypeDefinition)scope).Name : "");
			sb.Append('{');
			bool comma = false;
			foreach(var binding in variables){
				if(comma) sb.Append(", ");
				else comma = true;

				int index = 2 * binding.Value.Offset;
				sb.AppendFormat("{0}:{1}{2}",
				                binding.Key,
				                bits.Get(index) ? "*" : "-",
				                bits.Get(index + 1) ? "-" : "*");
				if(binding.Value.ReadBeforeInitialized)
					sb.Append("#");
			}
			sb.Append('}');
			Debug.WriteLine(sb.ToString());
		}
		
		void SetAssigned(ExpressoVariable variable, bool value)
		{
			bits.Set(variable.Offset * 2, value);
		}
		
		void SetInitialized(ExpressoVariable variable, bool value)
		{
			bits.Set(variable.Offset * 2 + 1, value);
		}

		bool IsAssigned(ExpressoVariable variable)
		{
			return bits.Get(variable.Offset * 2);
		}
		
		bool IsInitialized(ExpressoVariable variable)
		{
			return bits.Get(variable.Offset * 2 + 1);
		}
		
		public static void Check(ScopeStatement scope)
		{
			if(scope.Variables != null){
				FlowChecker fc = new FlowChecker(scope);
				scope.Walk(fc);
			}
		}
		
		public void Define(string name)
		{
			ExpressoVariable binding;
			if(variables.TryGetValue(name, out binding)){
				SetAssigned(binding, true);
				SetInitialized(binding, true);
			}
		}
		
		public void Delete(string name)
		{
			ExpressoVariable binding;
			if(variables.TryGetValue(name, out binding)){
				SetAssigned(binding, false);
				SetInitialized(binding, true);
			}
		}
		
		void PushLoop(BitArray ba)
		{
			if(loops == null){
				loops = new Stack<BitArray>();
			}
			loops.Push(ba);
		}
		
		BitArray PeekLoop()
		{
			return loops != null && loops.Count > 0 ? loops.Peek() : null;
		}
		
		void PopLoop()
		{
			if(loops != null) loops.Pop();
		}
		
		#region AstWalker Methods
		// LambdaExpr
		//public override bool Walk(LambdaExpression node) { return false; }
		
		// Comprehension
		public override bool Walk(Comprehension node)
		{
			BitArray save = bits;
			bits = new BitArray(bits);
			
			node.Body.Walk(this);
			node.Item.Walk(this);
			
			bits = save;
			return false;
		}

		// ComprehensionFor
		public override bool Walk(ComprehensionFor node)
		{
			return base.Walk(node);
		}

		// ComprehensionIf
		public override bool Walk(ComprehensionIf node)
		{
			return base.Walk(node);
		}

		// SequenceInitializer
		public override bool Walk(SequenceInitializer node)
		{
			foreach(var e in node.Items)
				e.Walk(this);

			return false;
		}
		
		// Identifier
		public override bool Walk(Identifier node)
		{
			ExpressoVariable binding;
			if(variables.TryGetValue(node.Name, out binding)){
				node.Assigned = IsAssigned(binding);
				
				if(!IsInitialized(binding)){
					binding.ReadBeforeInitialized = true;
				}
			}
			return true;
		}
		public override void PostWalk(Identifier node) { }
		
		// AssignStmt
		public override bool Walk(Assignment node)
		{
			node.Right.Walk(this);
			foreach(Expression e in node.Left)
				e.Walk(fdef);

			return false;
		}
		public override void PostWalk(Assignment node) { }
		
		// BreakStmt
		public override bool Walk(BreakStatement node)
		{
			BitArray exit = PeekLoop();
			if(exit != null){ // break outside loop
				exit.And(bits);
			}
			return true;
		}
		
		// TypeDef
		public override bool Walk(TypeDefinition node)
		{
			if(scope == node){
				// the class body is being analyzed, go deep:
				return true;
			}else{
				// analyze the type definition itself (it is visited while analyzing parent scope):
				Define(node.Name);
				//foreach(Expression e in node.Bases){
				//	e.Walk(this);
				//}
				return false;
			}
		}
		
		// ContinueStmt
		public override bool Walk(ContinueStatement node) { return true; }
		
		// ForStmt
		public override bool Walk(ForStatement node)
		{
			// Walk the expression
			node.Target.Walk(this);
			
			BitArray opte = new BitArray(bits);
			BitArray exit = new BitArray(bits.Length, true);
			PushLoop(exit);
			
			// Define the lhs
			if(node.HasLet)
				node.Left.Walk(fdef);

			// Walk the body
			node.Body.Walk(this);
			
			PopLoop();
			
			bits.And(exit);
			
			// Intersect
			bits.And(opte);
			
			return false;
		}
		
		// RequireStmt
		public override bool Walk(RequireStatement node)
		{
			// A Require statement always introduce new variable(s) into the module scope.
			for(int i = 0; i < node.ModuleNames.Length; ++i)
				Define(node.AliasNames[i] != null ? node.AliasNames[i] : node.ModuleNames[i]);

			return true;
		}
		
		// FuncDef
		public override bool Walk(FunctionDefinition node)
		{
			if(node == scope){
				// the function body is being analyzed, go deep:
				foreach(Argument p in node.Parameters)
					p.Walk(fdef);

				return true;
			}else{
				// analyze the function definition itself (it is visited while analyzing parent scope):
				Define(node.Name);
				foreach(Argument p in node.Parameters){
					if(p.Option != null)
						p.Option.Walk(this);
				} 
				return false;
			}
		}
		
		// IfStmt
		public override bool Walk(IfStatement node)
		{
			//BitArray result = new BitArray(bits.Length, true);
			//BitArray save = bits;
			
			bits = new BitArray(bits.Length);
			
			/*foreach(IfStatementTest ist in node.Tests) {
				// Set the initial branch value to bits
				_bits.SetAll(false);
				_bits.Or(save);
				
				// Flow the test first
				ist.Test.Walk(this);
				// Flow the body
				ist.Body.Walk(this);
				// Intersect
				result.And(_bits);
			}
			
			// Set the initial branch value to bits
			_bits.SetAll(false);
			_bits.Or(save);
			
			if (node.ElseStatement != null) {
				// Flow the else_
				node.ElseStatement.Walk(this);
			}
			
			// Intersect
			result.And(_bits);
			
			_bits = save;
			
			// Remember the result
			_bits.SetAll(false);
			_bits.Or(result);*/
			return false;
		}
		
		public override void PostWalk(ReturnStatement node) { }
		
		// WithStmt
		public override bool Walk(WithStatement node)
		{
			// Walk the expression
			/*node.ContextManager.Walk(this);
			BitArray save = _bits;
			_bits = new BitArray(_bits);
			
			// Define the Rhs
			if (node.Variable != null)
				node.Variable.Walk(_fdef);
			
			// Flow the body
			node.Body.Walk(this);
			
			_bits = save;
			return false;*/
			return base.Walk(node);
		}
		
		// TryStmt
		public override bool Walk(TryStatement node)
		{
			BitArray save = bits;
			bits = new BitArray(bits);
			
			// Flow the body
			node.Body.Walk(this);
			
			if(node.Catches != null){
				foreach(var handler in node.Catches){
					// Restore to saved state
					bits.SetAll(false);
					bits.Or(save);
					
					// Define the target
					if(handler.Catcher != null){
						handler.Catcher.Walk(fdef);
					}
					
					// Flow the body
					handler.Body.Walk(this);
				}
			}
			
			bits = save;
			
			if(node.FinallyClause != null){
				// Flow finally - this executes no matter what
				node.FinallyClause.Walk(this);
			}
			
			return false;
		}
		
		// WhileStmt
		public override bool Walk(WhileStatement node)
		{
			// Walk the expression
			node.Condition.Walk(this);
			
			BitArray exit = new BitArray(bits.Length, true);
			
			PushLoop(exit);
			node.Body.Walk(this);
			PopLoop();
			
			bits.And(exit);
			
			return false;
		}

		// ExpressionStmt
		public override bool Walk(ExprStatement node)
		{
			foreach(var expr in node.Expressions)
				expr.Walk(this);

			return false;
		}

		// VariableDecl
		public override bool Walk(VarDeclaration node)
		{
			foreach(var ident in node.Left)
				ident.Walk(fdef);

			return false;
		}
		#endregion
	}
}

