using System;
using System.Collections.Generic;
using Expresso.Ast;
using Expresso.BuiltIns;
using Expresso.Compiler;

namespace Expresso.Interpreter
{
	/// <summary>
	/// Expressoのインタプリタ.
	/// </summary>
	public class Interpreter
	{
		public Ast.Block Root{get; internal set;}
		
		private Scope environ = new Scope();
		
		private VariableStore var_store = new VariableStore();
		
		public Interpreter(Block root)
		{
			Root = root;
		}
		
		/// <summary>
		/// main関数をエントリーポイントとしてプログラムを実行する。
		/// </summary>
		/// <exception cref='EvalException'>
		/// Is thrown when the eval exception.
		/// </exception>
		public void Run()
		{
			Ast.Function main_fn = environ.GetFunction("main");
			if(main_fn == null)
				throw new EvalException("No entry point");
			
			Ast.Call call = new Ast.Call{
				Function = main_fn,
				Arguments = new List<Ast.Expression>()
			};
			
			EvalExpression(call, var_store);
		}
		
		/// <summary>
		/// グローバルに存在する変数宣言や関数定義文を実行して
		/// グローバルの環境を初期化する。
		/// </summary>
		/// <exception cref='Exception'>
		/// Represents errors that occur during application execution.
		/// </exception>
		public void Initialize()
		{
			Ast.Block topmost = Root as Ast.Block;
			if(topmost == null)
				throw new Exception("Topmost block not found!");
			
			EvalBlock(topmost, var_store);
		}
		
		private object EvalBlock(Block block, VariableStore varTable)
		{
			object result = null;
			
			foreach (var stmt in block.Statements) {
				result = EvalStatement(stmt, varTable);
			}
			
			return result;
		}
		
		private object EvalStatement(Statement stmt, VariableStore localVars)
		{
			object result = null;
			
			switch(stmt.Type){
			case NodeType.Assignment:
				Ast.Assignment assign = (Assignment)stmt;
				object rvalue;
				for (int i = 0; i < assign.Targets.Count; ++i) {
					Parameter lvalue = (Parameter)assign.Targets[i];
					rvalue = EvalExpression(assign.Expressions[i], localVars);
					localVars.Assign(lvalue.Name, rvalue);
				}
				break;
				
			case NodeType.Function:
				Ast.Function func = (Function)stmt;
				environ.AddFunction(func);
				break;
				
			case NodeType.Print:
			{
				Ast.PrintStatement print = (PrintStatement)stmt;
				object obj = EvalExpression(print.Expression, localVars);
				Console.WriteLine(obj.ToString());
				break;
			}
				
			case NodeType.Return:
				Ast.Return return_stmt = (Return)stmt;
				result = EvalExpressions(return_stmt.Expressions, localVars);
				break;
				
			case NodeType.VarDecl:
			{
				Ast.VarDeclaration var_decl = (VarDeclaration)stmt;
				object obj;
				for (int i = 0; i < var_decl.Variables.Count; ++i) {
					obj = EvalExpression(var_decl.Expressions[i], localVars);
					localVars.Add(var_decl.Variables[i].Name, obj);
				}
				break;
			}
				
			case NodeType.ExprStatement:
				Ast.ExprStatement expr_stmt = (ExprStatement)stmt;
				for (int i = 0; i < expr_stmt.Expressions.Count; ++i) {
					EvalExpression(expr_stmt.Expressions[i], localVars);
				}
				break;
				
			case NodeType.IfStatement:
			{
				Ast.IfStatement if_stmt = (IfStatement)stmt;
				object obj = EvalExpression(if_stmt.Condition, localVars);
				Nullable<bool> bool_obj = obj as Nullable<bool>;
				if(bool_obj == null)
					throw new EvalException("Invalid expression! The condition of an if statement must yields a boolean!");
				
				EvalStatement(((bool)bool_obj) ? if_stmt.TrueBlock : if_stmt.FalseBlock, new VariableStore());
				break;
			}
				
			case NodeType.WhileStatement:
				EvalWhileStatement((WhileStatement)stmt, localVars);
				break;
				
			case NodeType.Block:
				EvalBlock((Block)stmt, localVars);
				break;
				
			default:
				throw new Exception("Unknown statement type!");
				break;
			}
			
			return result;
		}
		
		private object EvalExpressions(List<Expression> exprs, VariableStore localVars)
		{
			if(exprs.Count == 0){
				return new ExpressoTuple(new List<ExpressoObj>());
			}else if(exprs.Count == 1){
				return EvalExpression(exprs[0], localVars);
			}else{
				var objs = new List<ExpressoObj>();
				for (int i = 0; i < exprs.Count; ++i) {
					objs.Add((ExpressoObj)EvalExpression(exprs[i], localVars));
				}
				
				return new ExpressoTuple(objs);
			}
		}
		
		private object EvalExpression(Expression expr, VariableStore localVars = null)
		{
			object result = null;
			
			switch (expr.Type) {
			case NodeType.BinaryExpression:
				result = EvalBinaryExpr((BinaryExpression)expr, localVars);
				break;
				
			case NodeType.UnaryExpression:
				result = EvalUnaryExpr((UnaryExpression)expr, localVars);
				break;
				
			case NodeType.Call:
				result = EvalFunctionCall((Call)expr, localVars);
				break;
				
			case NodeType.ConditionalExpression:
				result = EvalCondExpr((ConditionalExpression)expr, localVars);
				break;
				
			case NodeType.Range:
				result = EvalRangeExpr((RangeExpression)expr);
				break;
				
			case NodeType.Constant:
				var constant = (Constant)expr;
				result = constant.Value.Value;
				break;
				
			case NodeType.Parameter:
				var param = (Parameter)expr;
				if(localVars == null)
					throw new EvalException("Can not find variable store");
				
				result = localVars.Get(param.Name);
				break;
				
			default:
				result = null;
				break;
			}
			
			return result;
		}
		
		private object EvalBinaryExpr(BinaryExpression expr, VariableStore localVars)
		{
			object first = EvalExpression(expr.Left, localVars), second = EvalExpression(expr.Right, localVars);
			if((int)expr.Operator <= (int)OperatorType.MOD){
				if(first is int){
					return BinaryExprAsInt((int)first, (int)second, expr.Operator);
				}else{
					return BinaryExprAsDouble((double)first, (double)second, expr.Operator);
				}
			}else if((int)expr.Operator < (int)OperatorType.AND){
				return EvalComparison(first as IComparable, second as IComparable, expr.Operator);
			}
			bool lhs = (bool)first, rhs = (bool)second;
			
			switch (expr.Operator) {
			case OperatorType.AND:
				return lhs && rhs;
				
			case OperatorType.OR:
				return lhs || rhs;
				
			default:
				throw new EvalException("Invalid operator type!");
			}
		}
		
		private int BinaryExprAsInt(int lhs, int rhs, OperatorType opType)
		{
			int result;
			
			switch (opType) {
			case OperatorType.PLUS:
				result = lhs + rhs;
				break;
				
			case OperatorType.MINUS:
				result = lhs - rhs;
				break;
				
			case OperatorType.TIMES:
				result = lhs * rhs;
				break;
				
			case OperatorType.DIV:
				result = lhs / rhs;
				break;
				
			case OperatorType.POWER:
				result = (int)Math.Pow(lhs, rhs);
				break;
				
			case OperatorType.MOD:
				result = lhs % rhs;
				break;
				
			default:
				throw new EvalException("Unreachable code");
			}
			
			return result;
		}
		
		private double BinaryExprAsDouble(double lhs, double rhs, OperatorType opType)
		{
			double result;
			
			switch (opType) {
			case OperatorType.PLUS:
				result = lhs + rhs;
				break;
				
			case OperatorType.MINUS:
				result = lhs - rhs;
				break;
				
			case OperatorType.TIMES:
				result = lhs * rhs;
				break;
				
			case OperatorType.DIV:
				result = lhs / rhs;
				break;
				
			case OperatorType.POWER:
				result = Math.Pow(lhs, rhs);
				break;
				
			case OperatorType.MOD:
				result = Math.IEEERemainder(lhs, rhs);
				break;
				
			default:
				throw new EvalException("Unreachable code");
			}
			
			return result;
		}
		
		private bool EvalComparison(IComparable lhs, IComparable rhs, OperatorType opType)
		{
			if(lhs == null || rhs == null)
				throw new EvalException("The operands can not be compared");
			
			switch (opType) {
			case OperatorType.EQUAL:
				return object.Equals(lhs, rhs);
				
			case OperatorType.GREAT:
				return lhs.CompareTo(rhs) > 0;
				
			case OperatorType.GRTE:
				return lhs.CompareTo(rhs) >= 0;
				
			case OperatorType.LESE:
				return lhs.CompareTo(rhs) <= 0;
				
			case OperatorType.LESS:
				return lhs.CompareTo(rhs) < 0;
				
			case OperatorType.NOTEQ:
				return !object.Equals(lhs, rhs);
				
			default:
				return false;
			}
		}
		
		private object EvalUnaryExpr(UnaryExpression expr, VariableStore localVars)
		{
			object ope = EvalExpression(expr.Operand, localVars);
			
			if(expr.Operator == OperatorType.MINUS){
				if(ope is int){
					return -(int)ope;
				}else if(ope is double){
					return -(double)ope;
				}else{
					throw new EvalException("The minus operator is not applicable to the operand!");
				}
			}
			
			return null;
		}
		
		private object EvalFunctionCall(Call expr, VariableStore parent)
		{
			Function fn = expr.Function;
			var local = new VariableStore{Parent = parent};
			for (int i = 0; i < expr.Arguments.Count; ++i) {
				//local.Add(fn.Parameters[i].Name, );
			}
			
			return Apply(fn, local);
		}
		
		private object Apply(Function fn, VariableStore localStore)
		{
			return EvalBlock(fn.Body, localStore);
		}
		
		private object EvalCondExpr(ConditionalExpression expr, VariableStore localVars)
		{
			if((bool)EvalExpression(expr.Condition, localVars)){
				return EvalExpression(expr.TrueExpression, localVars);
			}else{
				return EvalExpression(expr.FalseExpression, localVars);
			}
		}
		
		private object EvalRangeExpr(RangeExpression range)
		{
			return new ExpressoRange(range.Start, range.End, range.Step);
		}
		
		private void EvalWhileStatement(WhileStatement stmt, VariableStore local)
		{
			Nullable<bool> cond;
			
			while((cond = EvalExpression(stmt.Condition, local) as Nullable<bool>) != null && (bool)cond){
				EvalStatement(stmt.Body, local);
			}
			
			if(cond == null)
				throw new EvalException("Invalid expression! The condition of a while statement must yields a boolean!");
		}
	}
}

