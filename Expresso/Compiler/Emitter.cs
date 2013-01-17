using System;

using Expresso.Ast;

namespace Expresso.Compiler
{
	/// <summary>
	/// Expressoの構文木を解釈してC#の式木などにコンパイルするための基底クラス。
	/// It is the base class for walking around AST nodes and emitting native codes from the AST of Expresso.
	/// </summary>
	public abstract class Emitter<ReturnType>
	{
		internal abstract ReturnType Emit(Argument node);

		internal abstract ReturnType Emit(AssertStatement node);

		internal abstract ReturnType Emit(Assignment node);

		internal abstract ReturnType Emit(BinaryExpression node);

		internal abstract ReturnType Emit(Block node);

		internal abstract ReturnType Emit(Call node);

		internal abstract ReturnType Emit(CastExpression node);

		internal abstract ReturnType Emit(Comprehension node);

		internal abstract ReturnType Emit(ComprehensionFor node);
		
		internal abstract ReturnType Emit(ComprehensionIf node);

		internal abstract ReturnType Emit(ConditionalExpression node);

		internal abstract ReturnType Emit(Constant node);

		internal abstract ReturnType Emit(BreakStatement node);

		internal abstract ReturnType Emit(ContinueStatement node);

		internal abstract ReturnType Emit(DefaultExpression node);

		internal abstract ReturnType Emit(EmptyStatement node);

		internal abstract ReturnType Emit(ExprStatement node);

		internal abstract ReturnType Emit(ForStatement node);

		internal abstract ReturnType Emit(FunctionDefinition node);

		internal abstract ReturnType Emit(Identifier node);

		internal abstract ReturnType Emit(IfStatement node);

		internal abstract ReturnType Emit(IntSeqExpression node);

		internal abstract ReturnType Emit(MemberReference node);

		internal abstract ReturnType Emit(ExpressoAst node);

		internal abstract ReturnType Emit(NewExpression node);

		internal abstract ReturnType Emit(SequenceInitializer node);

		internal abstract ReturnType Emit(PrintStatement node);

		internal abstract ReturnType Emit(RequireStatement node);

		internal abstract ReturnType Emit(ReturnStatement node);

		internal abstract ReturnType Emit(SwitchStatement node);

		internal abstract ReturnType Emit(CaseClause node);

		internal abstract ReturnType Emit(SequenceExpression node);

		internal abstract ReturnType Emit(ThrowStatement node);

		internal abstract ReturnType Emit(TryStatement node);

		internal abstract ReturnType Emit(CatchClause node);

		internal abstract ReturnType Emit(FinallyClause node);

		internal abstract ReturnType Emit(TypeDefinition node);

		internal abstract ReturnType Emit(UnaryExpression node);

		internal abstract ReturnType Emit(VarDeclaration node);

		internal abstract ReturnType Emit(WhileStatement node);

		internal abstract ReturnType Emit(WithStatement node);

		internal abstract ReturnType Emit(YieldStatement node);
	}
}

