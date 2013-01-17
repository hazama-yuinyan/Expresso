using System;
using System.Collections.Generic;
using System.Linq;

using Expresso.Ast;
using Expresso.Builtins;
using Expresso.Compiler.Meta;
using Expresso.Interpreter;
using Expresso.Runtime.Operations;

namespace Expresso.Compiler
{
	using ExprTree = System.Linq.Expressions;
	using CSharpExpr = System.Linq.Expressions.Expression;
	using Helper = Expresso.Runtime.ImplementationHelpers;

	/// <summary>
	/// Expressoの構文木を解釈してC#の式木にコンパイルするクラス。
	/// It emitts C#'s expression tree nodes from the AST of Expresso.
	/// </summary>
	public class CSharpEmitter : Emitter<CSharpExpr>
	{
		ExprTree.LabelTarget return_target = null;
		bool has_default_clause;
		bool has_continue;
		CSharpExpr default_body = null, comprehension_generator = null;
		List<ExprTree.LabelTarget> break_targets = new List<ExprTree.LabelTarget>();
		List<ExprTree.LabelTarget> continue_targets = new List<ExprTree.LabelTarget>();

		internal override CSharpExpr Emit(Argument node)
		{
			return CSharpExpr.Parameter(ExpressoOps.GetNativeType(node.ParamType.ObjType), node.Name);
		}

		internal override CSharpExpr Emit(AssertStatement node)
		{
			throw new System.NotImplementedException ();
		}

		internal override CSharpExpr Emit(Assignment node)
		{
			/*var rvalues = node.Rhs.Compile(this);
			var results = new List<CSharpExpr>();

			var _self = this;
			foreach(var rvalue in node.Targets.Zip(rvalues,
			                                       (first, second) => new Tuple<CSharpExpr, CSharpExpr>(first.Compile(_self), second))){
				results.Add(CSharpExpr.Assign(rvalue.Item1, rvalue.Item2));
			}

			return CSharpExpr.Block(results);	//x, y, z... = a, b, c...; => x = a, y = b, z = c...;
			*/
			return null;
		}

		internal override CSharpExpr Emit(BinaryExpression node)
		{
			//var lhs = node.Left.Compile(this);
			//var rhs = node.Right.Compile(this);
			throw new System.NotImplementedException();
		}

		internal override CSharpExpr Emit(Block node)
		{
			var compiled_locals =
				from local in node.LocalVariables
				let param = (ExprTree.ParameterExpression)Emit(local)
				select param;
			var children =
				from child in node.Statements
				select child.Compile(this);

			return CSharpExpr.Block(compiled_locals, children);	//{children...}
		}

		internal override CSharpExpr Emit(Call node)
		{
			/*var args =
				from expr in node.Arguments
				select expr.Compile(this);*/

			return null;
			/*if(node.Function != null){
				var func = node.Function.Compile(this);
				return CSharpExpr.Invoke(func, args);		//func(args);
			}else{
				var method = node.Reference.Compile(this);
				return CSharpExpr.Invoke(method, args);		//Parent.Subscript(args)
			}*/
		}

		internal override CSharpExpr Emit(CastExpression node)
		{
			throw new System.NotImplementedException ();
		}

		internal override CSharpExpr Emit(Comprehension node)
		{
			throw new System.NotImplementedException();
		}

		internal override CSharpExpr Emit(ComprehensionFor node)
		{
			throw new System.NotImplementedException("Can not call this method directly. Use the Emit(Comprehension) method instead.");
		}

		internal override CSharpExpr Emit(ComprehensionIf node)
		{
			throw new System.NotImplementedException("Can not call this method directly. Use the Emit(Comprehension) method instead.");
		}

		internal override CSharpExpr Emit(ConditionalExpression node)
		{
			return CSharpExpr.Condition(node.Condition.Compile(this), node.TrueExpression.Compile(this),
			                     node.FalseExpression.Compile(this));	//Condition ? TrueExpression : FalseExpression
		}

		internal override CSharpExpr Emit(Constant node)
		{
			return CSharpExpr.Constant(node.Value, node.Value.GetType());
		}

		internal override CSharpExpr Emit(BreakStatement node)
		{
			if(node.Count > break_targets.Count)
				throw new EmitterException("Can not break out of loops that many times!");

			//break upto Count; => goto label;
			return CSharpExpr.Break(break_targets[break_targets.Count - node.Count]);
		}

		internal override CSharpExpr Emit(ContinueStatement node)
		{
			if(node.Count > continue_targets.Count)
				throw new EmitterException("Can not track up loops that many times!");

			//continue upto Count; => goto label;
			return CSharpExpr.Continue(continue_targets[continue_targets.Count - node.Count]);
		}

		internal override CSharpExpr Emit(DefaultExpression node)
		{
			return CSharpExpr.Default(ExpressoOps.GetNativeType(node.TargetType.ObjType));
		}

		internal override CSharpExpr Emit(EmptyStatement node)
		{
			return CSharpExpr.Empty();
		}

		internal override CSharpExpr Emit(ExprStatement node)
		{
			throw new System.NotImplementedException();
		}

		internal override CSharpExpr Emit(ForStatement node)
		{
			throw new System.NotImplementedException();
		}

		internal override CSharpExpr Emit(FunctionDefinition node)
		{
			throw new System.NotImplementedException();
		}

		internal override CSharpExpr Emit(Identifier node)
		{
			return CSharpExpr.Parameter(ExpressoOps.GetNativeType(node.ParamType.ObjType), node.Name);	//ObjType Name;
		}

		internal override CSharpExpr Emit(IfStatement node)
		{
			var condition = node.Condition.Compile(this);
			var true_block = node.TrueBlock.Compile(this);

			if(node.FalseBlock == null)
				return CSharpExpr.IfThen(condition, true_block);	//if(condition) true_block
			else
				return CSharpExpr.IfThenElse(condition, true_block,
				                             node.FalseBlock.Compile(this));	//if(condition) true_block else false_block
		}

		internal override CSharpExpr Emit(IntSeqExpression node)
		{
			var int_seq_ctor = typeof(ExpressoIntegerSequence).GetConstructor(new Type[]{typeof(int), typeof(int), typeof(int)});
			var args = new List<CSharpExpr>{
				node.Lower.Compile(this),
				node.Upper.Compile(this),
				node.Step.Compile(this)
			};
			return CSharpExpr.New(int_seq_ctor, args);		//new ExpressoIntegerSequence(Start, End, Step)
		}

		internal override CSharpExpr Emit(MemberReference node)
		{
			throw new System.NotImplementedException();
		}

		internal override CSharpExpr Emit(ExpressoAst node)
		{
			throw new System.NotImplementedException ();
		}

		internal override CSharpExpr Emit(NewExpression node)
		{
			throw new System.NotImplementedException();
		}

		internal override CSharpExpr Emit(SequenceInitializer node)
		{
			throw new System.NotImplementedException();
		}

		internal override CSharpExpr Emit(PrintStatement node)
		{
			throw new System.NotImplementedException();
		}

		internal override CSharpExpr Emit(RequireStatement node)
		{
			throw new System.NotImplementedException();
		}

		internal override CSharpExpr Emit(ReturnStatement node)
		{
			if(return_target == null)
				throw new EmitterException("Invalid return target");

			var return_value = node.Expression.Compile(this);
			return CSharpExpr.Return(return_target, return_value);	//return expression;
		}

		internal override CSharpExpr Emit(SwitchStatement node)
		{
			has_default_clause = false;
			var cases = 
				from case_clause in node.Cases
				let compiled = CompileCase(case_clause)
				where compiled != null
				select compiled;

			return has_default_clause ? CSharpExpr.Switch(node.Target.Compile(this), default_body, cases.ToArray()) :
				CSharpExpr.Switch(node.Target.Compile(this), cases.ToArray());	//switch(Target){...}
		}

		internal override CSharpExpr Emit(CaseClause node)
		{
			throw new NotImplementedException("Can not call this method directory. Use Emit(SwitchStatement) method.");
		}

		internal override CSharpExpr Emit(SequenceExpression node)
		{
			throw new System.NotImplementedException ();
		}

		internal override CSharpExpr Emit(ThrowStatement node)
		{
			return CSharpExpr.Throw(node.Expression.Compile(this));		//throw Expression;
		}

		internal override CSharpExpr Emit(TryStatement node)
		{
			if(node.Catches == null && node.FinallyClause == null)
				throw new EmitterException("Both the catch clause and the finally clause are empty! A try statement must have at least a catch clause or the finally clause.");

			var body = node.Body.Compile(this);
			if(node.FinallyClause == null){
				var param_e = CSharpExpr.Parameter(typeof(ExpressoThrowException), "exs_e");
				var catch_body = CompileCatch(node.Catches, param_e);
				var catch_clause = CSharpExpr.Catch(param_e, catch_body);
				return CSharpExpr.TryCatch(body,				//try{
				                           catch_clause);		//}catch(...){...}
			}else if(node.Catches == null){
				return CSharpExpr.TryFinally(body,								//try{
				                             node.FinallyClause.Compile(this));	//}finally{...}
			}else{
				var param_e = CSharpExpr.Parameter(typeof(ExpressoThrowException), "exs_e");
				var catch_body = CompileCatch(node.Catches, param_e);
				var catch_clause = CSharpExpr.Catch(param_e, catch_body);
				return CSharpExpr.TryCatchFinally(body,									//try{
				                                  node.FinallyClause.Compile(this),		//}catch(...){...}
				                                  catch_clause);						//finally{...}
			}
		}

		internal override CSharpExpr Emit(CatchClause node)
		{
			throw new System.NotImplementedException("Can not call this method directly. Use Emit(TryStatement) method.");
		}

		internal override CSharpExpr Emit(FinallyClause node)
		{
			throw new System.NotImplementedException("Can not call this method directly. Use Emit(TryStatement) method.");
		}

		internal override CSharpExpr Emit(TypeDefinition node)
		{
			throw new System.NotImplementedException();
		}

		internal override CSharpExpr Emit(UnaryExpression node)
		{
			var operand = node.Operand.Compile(this);
			return ConstructUnaryOpe(operand, node.Operator);
		}

		internal override CSharpExpr Emit(VarDeclaration node)
		{
			throw new System.NotImplementedException();
		}

		internal override CSharpExpr Emit(WhileStatement node)
		{
			has_continue = false;

			var end_loop = CSharpExpr.Label("__EndWhile" + break_targets.Count);
			var continue_loop = CSharpExpr.Label("__BeginWhile" + continue_targets.Count);
			break_targets.Add(end_loop);
			continue_targets.Add(continue_loop);

			var condition = CSharpExpr.IfThen(node.Condition.Compile(this),
			                                  CSharpExpr.Break(end_loop));
			var body = CSharpExpr.Block(new CSharpExpr[]{
				condition,
				node.Body.Compile(this)
			});
			Helper.RemoveLast(break_targets);
			Helper.RemoveLast(continue_targets);

			return has_continue ? CSharpExpr.Loop(body, end_loop, continue_loop) :
				CSharpExpr.Loop(body, end_loop);		//while(condition){body...}
		}

		internal override CSharpExpr Emit(WithStatement node)
		{
			throw new System.NotImplementedException();
		}

		internal override CSharpExpr Emit(YieldStatement node)
		{
			throw new System.NotImplementedException();
		}

		#region private methods
		private CSharpExpr CreateForeachLoop(ExprTree.ParameterExpression[] variables, CSharpExpr target, CSharpExpr body)
		{
			if(variables == null)
				throw new ArgumentNullException("variables", "For statement takes at least one variable!");
			if(target == null)
				throw new ArgumentNullException("target", "Can not iterate over a null object.");
			if(body == null)
				throw new ArgumentNullException("body", "I can't understand what job do you ask me for in that loop!");

			has_continue = false;

			var false_body = CSharpExpr.Block();
			var end_loop = break_targets[break_targets.Count - 1];
			if(has_continue){
				var continue_loop = continue_targets[continue_targets.Count - 1];
				return CSharpExpr.Loop(false_body, end_loop, continue_loop);
			}else
				return CSharpExpr.Loop(false_body, end_loop);
		}

		private ExprTree.SwitchCase CompileCase(CaseClause node)
		{
			var labels = new List<CSharpExpr>();
			foreach(var label in node.Labels){
				if(label is Constant && ((Constant)label).ValType == ObjectTypes._CASE_DEFAULT){
					has_default_clause = true;
					default_body = node.Body.Compile(this);
					return null;
				}else
					labels.Add(label.Compile(this));
			}
			
			return CSharpExpr.SwitchCase(node.Body.Compile(this),	//case label:
			                             labels);					//case label2: body;
		}

		private CSharpExpr CompileCatch(CatchClause[] clauses, ExprTree.ParameterExpression caughtException)
		{
			return null;
		}

		private CSharpExpr CompileCompIf(ComprehensionIf node)
		{
			if(node.Body == null)		//[generator...if Condition] -> ...if(Condition) seq.Add(generator);
				return CSharpExpr.IfThen(node.Condition.Compile(this), comprehension_generator);
			else						//[...if Condition...] -> ...if(Condition){...}
				return CSharpExpr.IfThen(node.Condition.Compile(this), node.Body.Compile(this));
		}

		private CSharpExpr CompileCompFor(ComprehensionFor node)
		{
			return null;
		}

		private CSharpExpr ConstructBinaryOpe(CSharpExpr lhs, CSharpExpr rhs, OperatorType opType)
		{
			switch(opType){
			case OperatorType.AND:
				return CSharpExpr.MakeBinary(ExprTree.ExpressionType.AndAlso, lhs, rhs);

			case OperatorType.BIT_AND:
				return CSharpExpr.MakeBinary(ExprTree.ExpressionType.And, lhs, rhs);

			case OperatorType.BIT_LSHIFT:
				return CSharpExpr.MakeBinary(ExprTree.ExpressionType.LeftShift, lhs, rhs);

			case OperatorType.BIT_OR:
				return CSharpExpr.MakeBinary(ExprTree.ExpressionType.Or, lhs, rhs);

			case OperatorType.BIT_RSHIFT:
				return CSharpExpr.MakeBinary(ExprTree.ExpressionType.RightShift, lhs, rhs);

			case OperatorType.BIT_XOR:
				return CSharpExpr.MakeBinary(ExprTree.ExpressionType.ExclusiveOr, lhs, rhs);

			case OperatorType.DIV:
				return CSharpExpr.MakeBinary(ExprTree.ExpressionType.Divide, lhs, rhs);

			case OperatorType.EQUAL:
				return CSharpExpr.MakeBinary(ExprTree.ExpressionType.Equal, lhs, rhs);

			case OperatorType.GREAT:
				return CSharpExpr.MakeBinary(ExprTree.ExpressionType.GreaterThan, lhs, rhs);

			case OperatorType.GRTE:
				return CSharpExpr.MakeBinary(ExprTree.ExpressionType.GreaterThanOrEqual, lhs, rhs);

			case OperatorType.LESE:
				return CSharpExpr.MakeBinary(ExprTree.ExpressionType.LessThanOrEqual, lhs, rhs);

			case OperatorType.LESS:
				return CSharpExpr.MakeBinary(ExprTree.ExpressionType.LessThan, lhs, rhs);

			case OperatorType.MINUS:
				return CSharpExpr.MakeBinary(ExprTree.ExpressionType.Subtract, lhs, rhs);

			case OperatorType.MOD:
				return CSharpExpr.MakeBinary(ExprTree.ExpressionType.Modulo, lhs, rhs);

			case OperatorType.NOTEQ:
				return CSharpExpr.MakeBinary(ExprTree.ExpressionType.NotEqual, lhs, rhs);

			case OperatorType.OR:
				return CSharpExpr.MakeBinary(ExprTree.ExpressionType.OrElse, lhs, rhs);

			case OperatorType.PLUS:
				return CSharpExpr.MakeBinary(ExprTree.ExpressionType.Add, lhs, rhs);

			case OperatorType.POWER:
				return CSharpExpr.MakeBinary(ExprTree.ExpressionType.Power, lhs, rhs);

			case OperatorType.TIMES:
				return CSharpExpr.MakeBinary(ExprTree.ExpressionType.Multiply, lhs, rhs);

			default:
				throw new EmitterException("Unknown binary operator!");
			}
		}

		private CSharpExpr ConstructUnaryOpe(CSharpExpr operand, OperatorType opeType)
		{
			switch(opeType){
			case OperatorType.PLUS:
				return CSharpExpr.UnaryPlus(operand);

			case OperatorType.MINUS:
				return CSharpExpr.Negate(operand);

			case OperatorType.NOT:
				return CSharpExpr.Not(operand);

			default:
				throw new EmitterException("Unknown unary operator!");
			}
		}
		#endregion
	}
}

