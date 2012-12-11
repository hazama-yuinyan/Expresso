using System;

using Expresso.Ast;

namespace Expresso.Compiler
{
	using ExprTree = System.Linq.Expressions;
	using Helper = Expresso.Helpers.ImplementationHelpers;

	public class Emitter
	{
		public Emitter ()
		{
		}

		internal ExprTree.Expression Emit(Argument node)
		{
			throw new System.NotImplementedException();
		}

		internal ExprTree.Expression Emit(Assignment node)
		{
			throw new System.NotImplementedException();
		}

		internal ExprTree.Expression Emit(BinaryExpression node)
		{
			throw new System.NotImplementedException();
		}

		internal ExprTree.Expression Emit(Block node)
		{
			throw new System.NotImplementedException();
		}

		internal ExprTree.Expression Emit(Call node)
		{
			throw new System.NotImplementedException();
		}

		internal ExprTree.Expression Emit(Comprehension node)
		{
			throw new System.NotImplementedException();
		}

		internal ExprTree.Expression Emit(ConditionalExpression node)
		{
			throw new System.NotImplementedException();
		}

		internal ExprTree.Expression Emit(Constant node)
		{
			return ExprTree.Expression.Constant(node.Value, node.Value.GetType());
		}

		internal ExprTree.Expression Emit(BreakStatement node)
		{
			throw new System.NotImplementedException();
		}

		internal ExprTree.Expression Emit(ContinueStatement node)
		{
			throw new System.NotImplementedException();
		}

		internal ExprTree.Expression Emit(ExprStatement node)
		{
			throw new System.NotImplementedException();
		}

		internal ExprTree.Expression Emit(ForStatement node)
		{
			throw new System.NotImplementedException();
		}

		internal ExprTree.Expression Emit(Function node)
		{
			throw new System.NotImplementedException();
		}

		internal ExprTree.Expression Emit(Identifier node)
		{
			return ExprTree.Expression.Parameter(Helper.GetNativeType(node.ParamType.ObjType), node.Name);
		}

		internal ExprTree.Expression Emit(IfStatement node)
		{
			throw new System.NotImplementedException();
		}

		internal ExprTree.Expression Emit(IntSeqExpression node)
		{
			throw new System.NotImplementedException();
		}

		internal ExprTree.Expression Emit(MemberReference node)
		{
			throw new System.NotImplementedException();
		}

		internal ExprTree.Expression Emit(NewExpression node)
		{
			throw new System.NotImplementedException();
		}

		internal ExprTree.Expression Emit(ObjectInitializer node)
		{
			throw new System.NotImplementedException();
		}

		internal ExprTree.Expression Emit(PrintStatement node)
		{
			throw new System.NotImplementedException();
		}

		internal ExprTree.Expression Emit(RequireExpression node)
		{
			throw new System.NotImplementedException();
		}

		internal ExprTree.Expression Emit(ReturnStatement node)
		{
			throw new System.NotImplementedException();
		}

		internal ExprTree.Expression Emit(SwitchStatement node)
		{
			throw new System.NotImplementedException();
		}

		internal ExprTree.Expression Emit(CaseClause node)
		{
			throw new System.NotImplementedException();
		}

		internal ExprTree.Expression Emit(ThrowStatement node)
		{
			throw new System.NotImplementedException();
		}

		internal ExprTree.Expression Emit(TryStatement node)
		{
			throw new System.NotImplementedException();
		}

		internal ExprTree.Expression Emit(CatchClause node)
		{
			throw new System.NotImplementedException();
		}

		internal ExprTree.Expression Emit(FinallyClause node)
		{
			throw new System.NotImplementedException();
		}

		internal ExprTree.Expression Emit(TypeDeclaration node)
		{
			throw new System.NotImplementedException();
		}

		internal ExprTree.Expression Emit(UnaryExpression node)
		{
			throw new System.NotImplementedException();
		}

		internal ExprTree.Expression Emit(VarDeclaration node)
		{
			throw new System.NotImplementedException();
		}

		internal ExprTree.Expression Emit(WhileStatement node)
		{
			throw new System.NotImplementedException();
		}

		internal ExprTree.Expression Emit(WithStatement node)
		{
			throw new System.NotImplementedException();
		}
	}
}

