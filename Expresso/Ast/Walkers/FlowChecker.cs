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
	class FlowDefiner : AstWalkerNonRecursive
	{
		readonly FlowChecker fc;
		
		public FlowDefiner(FlowChecker flowChecker)
		{
			fc = flowChecker;
		}
		
		public override bool Walk(Identifier ident)
		{
			fc.Define(ident.Name);
			return false;
		}
		
		public override bool Walk(MemberReference memRef)
		{
			memRef.Walk(fc);
			return false;
		}
		
		public override bool Walk(SequenceExpression seqExpr)
		{
			return true;
		}
		
		public override bool Walk(Argument arg)
		{
			fc.Define(arg.Name);
			return true;
		}
	}
	
	class FlowChecker : AstWalker
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
			foreach(var binding in variables)
				binding.Value.Offset = index++;
			
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
		public override bool Walk(Comprehension comprehension)
		{
			BitArray save = bits;
			bits = new BitArray(bits);
			
			comprehension.Body.Walk(this);
			comprehension.Item.Walk(this);
			
			bits = save;
			return false;
		}

		// ComprehensionFor
		public override bool Walk(ComprehensionFor compFor)
		{
			return base.Walk(compFor);
		}

		// ComprehensionIf
		public override bool Walk(ComprehensionIf compIf)
		{
			return base.Walk(compIf);
		}

		// SequenceInitializer
		public override bool Walk(SequenceInitializer seqInitializer)
		{
			foreach(var e in seqInitializer.Items)
				e.Walk(this);

			return false;
		}
		
		// Identifier
		public override bool Walk(Identifier ident)
		{
			ExpressoVariable binding;
			if(variables.TryGetValue(ident.Name, out binding)){
				ident.Assigned = IsAssigned(binding);
				
				if(!IsInitialized(binding)){
					binding.ReadBeforeInitialized = true;
				}
			}
			return true;
		}
		public override void PostWalk(Identifier node) { }
		
		// AssignStmt
		public override bool Walk(Assignment assignment)
		{
			assignment.Right.Walk(this);
			foreach(Expression e in assignment.Left)
				e.Walk(fdef);

			return false;
		}
		public override void PostWalk(Assignment node) { }
		
		// BreakStmt
		public override bool Walk(BreakStatement breakStmt)
		{
			BitArray exit = PeekLoop();
			if(exit != null){ // break outside loop
				exit.And(bits);
			}
			return true;
		}
		
		// TypeDef
		public override bool Walk(TypeDefinition typeDef)
		{
			if(scope == typeDef){
				// the class body is being analyzed, go deep:
				return true;
			}else{
				// analyze the type definition itself (it is visited while analyzing parent scope):
				Define(typeDef.Name);
				//foreach(Expression e in node.Bases){
				//	e.Walk(this);
				//}
				return false;
			}
		}
		
		// ContinueStmt
		public override bool Walk(ContinueStatement node) { return true; }
		
		// ForStmt
		public override bool Walk(ForStatement fotStmt)
		{
			// Walk the expression
			fotStmt.Target.Walk(this);
			
			BitArray opte = new BitArray(bits);
			BitArray exit = new BitArray(bits.Length, true);
			PushLoop(exit);
			
			// Define the lhs
			if(fotStmt.HasLet)
				fotStmt.Left.Walk(fdef);

			// Walk the body
			fotStmt.Body.Walk(this);
			
			PopLoop();
			
			bits.And(exit);
			
			// Intersect
			bits.And(opte);
			
			return false;
		}
		
		// RequireStmt
		public override bool Walk(RequireStatement requireStmt)
		{
			// A Require statement always introduce new variable(s) into the module scope.
			for(int i = 0; i < requireStmt.ModuleNames.Length; ++i)
				Define(requireStmt.AliasNames[i] != null ? requireStmt.AliasNames[i] : requireStmt.ModuleNames[i]);

			return true;
		}
		
		// FuncDef
		public override bool Walk(FunctionDefinition funcDef)
		{
			if(funcDef == scope){
				// the function body is being analyzed, go deep:
				foreach(Argument p in funcDef.Parameters)
					p.Walk(fdef);

				return true;
			}else{
				// analyze the function definition itself (it is visited while analyzing parent scope):
				Define(funcDef.Name);
				foreach(Argument p in funcDef.Parameters){
					if(p.Option != null)
						p.Option.Walk(this);
				} 
				return false;
			}
		}
		
		// IfStmt
		public override bool Walk(IfStatement ifStmt)
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
		public override bool Walk(WithStatement withStmt)
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
			return base.Walk(withStmt);
		}
		
		// TryStmt
		public override bool Walk(TryStatement tryStmt)
		{
			BitArray save = bits;
			bits = new BitArray(bits);
			
			// Flow the body
			tryStmt.Body.Walk(this);
			
			if(tryStmt.Catches != null){
				foreach(var handler in tryStmt.Catches){
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
			
			if(tryStmt.FinallyClause != null){
				// Flow finally - this executes no matter what
				tryStmt.FinallyClause.Walk(this);
			}
			
			return false;
		}
		
		// WhileStmt
		public override bool Walk(WhileStatement whileStmt)
		{
			// Walk the expression
			whileStmt.Condition.Walk(this);
			
			BitArray exit = new BitArray(bits.Length, true);
			
			PushLoop(exit);
			whileStmt.Body.Walk(this);
			PopLoop();
			
			bits.And(exit);
			
			return false;
		}

		// ExpressionStmt
		public override bool Walk(ExprStatement exprStmt)
		{
			foreach(var expr in exprStmt.Expressions)
				expr.Walk(this);

			return false;
		}

		// VariableDecl
		public override bool Walk(VarDeclaration varDecl)
		{
			foreach(var ident in varDecl.Left)
				ident.Walk(fdef);

			return false;
		}
		#endregion
	}
}

