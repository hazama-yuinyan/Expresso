using System;

using Expresso.Ast;

namespace Expresso.Compiler
{
	/// <summary>
	/// Expresso Walker class - (default result is true)
	/// </summary>
	public class ExpressoWalker
	{
		public virtual bool Walk(Argument node){return true;}
		public virtual void PostWalk(Argument node){}

		public virtual bool Walk(AssertStatement node){return true;}
		public virtual void PostWalk(AssertStatement node){}

		public virtual bool Walk(Assignment node){return true;}
		public virtual void PostWalk(Assignment node){}

		public virtual bool Walk(BinaryExpression node){return true;}
		public virtual void PostWalk(BinaryExpression node){}

		public virtual bool Walk(Block node){return true;}
		public virtual void PostWalk(Block node){}

		public virtual bool Walk(Call node){return true;}
		public virtual void PostWalk(Call node){}

		public virtual bool Walk(CastExpression node){return true;}
		public virtual void PostWalk(CastExpression node){}

		public virtual bool Walk(Comprehension node){return true;}
		public virtual void PostWalk(Comprehension node){}

		public virtual bool Walk(ComprehensionFor node){return true;}
		public virtual void PostWalk(ComprehensionFor node){}

		public virtual bool Walk(ComprehensionIf node){return true;}
		public virtual void PostWalk(ComprehensionIf node){}

		public virtual bool Walk(ConditionalExpression node){return true;}
		public virtual void PostWalk(ConditionalExpression node){}

		public virtual bool Walk(Constant node){return true;}
		public virtual void PostWalk(Constant node){}

		public virtual bool Walk(BreakStatement node){return true;}
		public virtual void PostWalk(BreakStatement node){}

		public virtual bool Walk(ContinueStatement node){return true;}
		public virtual void PostWalk(ContinueStatement node){}

		public virtual bool Walk(DefaultExpression node){return true;}
		public virtual void PostWalk(DefaultExpression node){}

		public virtual bool Walk(EmptyStatement node){return true;}
		public virtual void PostWalk(EmptyStatement node){}

		public virtual bool Walk(ExprStatement node){return true;}
		public virtual void PostWalk(ExprStatement node){}

		public virtual bool Walk(ForStatement node){return true;}
		public virtual void PostWalk(ForStatement node){}

		public virtual bool Walk(FunctionDefinition node){return true;}
		public virtual void PostWalk(FunctionDefinition node){}

		public virtual bool Walk(Identifier node){return true;}
		public virtual void PostWalk(Identifier node){}

		public virtual bool Walk(IfStatement node){return true;}
		public virtual void PostWalk(IfStatement node){}

		public virtual bool Walk(IntSeqExpression node){return true;}
		public virtual void PostWalk(IntSeqExpression node){}
	
		public virtual bool Walk(MemberReference node){return true;}
		public virtual void PostWalk(MemberReference node){}

		public virtual bool Walk(ExpressoAst node){return true;}
		public virtual void PostWalk(ExpressoAst node){}

		public virtual bool Walk(NewExpression node){return true;}
		public virtual void PostWalk(NewExpression node){}

		public virtual bool Walk(SequenceInitializer node){return true;}
		public virtual void PostWalk(SequenceInitializer node){}

		public virtual bool Walk(PrintStatement node){return true;}
		public virtual void PostWalk(PrintStatement node){}

		public virtual bool Walk(RequireStatement node){return true;}
		public virtual void PostWalk(RequireStatement node){}

		public virtual bool Walk(ReturnStatement node){return true;}
		public virtual void PostWalk(ReturnStatement node){}

		public virtual bool Walk(SwitchStatement node){return true;}
		public virtual void PostWalk(SwitchStatement node){}

		public virtual bool Walk(CaseClause node){return true;}
		public virtual void PostWalk(CaseClause node){}

		public virtual bool Walk(ThrowStatement node){return true;}
		public virtual void PostWalk(ThrowStatement node){}

		public virtual bool Walk(TryStatement node){return true;}
		public virtual void PostWalk(TryStatement node){}

		public virtual bool Walk(SequenceExpression node){return true;}
		public virtual void PostWalk(SequenceExpression node){}

		public virtual bool Walk(CatchClause node){return true;}
		public virtual void PostWalk(CatchClause node){}

		public virtual bool Walk(FinallyClause node){return true;}
		public virtual void PostWalk(FinallyClause node){}

		public virtual bool Walk(TypeDefinition node){return true;}
		public virtual void PostWalk(TypeDefinition node){}

		public virtual bool Walk(UnaryExpression node){return true;}
		public virtual void PostWalk(UnaryExpression node){}

		public virtual bool Walk(VarDeclaration node){return true;}
		public virtual void PostWalk(VarDeclaration node){}

		public virtual bool Walk(WhileStatement node){return true;}
		public virtual void PostWalk(WhileStatement node){}

		public virtual bool Walk(WithStatement node){return true;}
		public virtual void PostWalk(WithStatement node){}

		public virtual bool Walk(YieldStatement node){return true;}
		public virtual void PostWalk(YieldStatement node){}
	}

	/// <summary>
	/// Expresso Walker class - (default result is false)
	/// </summary>
	public class ExpressoWalkerNonRecursive : ExpressoWalker
	{
		public override bool Walk(Argument node){return false;}
		public override void PostWalk(Argument node){}

		public override bool Walk(AssertStatement node){return false;}
		public override void PostWalk(AssertStatement node){}

		public override bool Walk(Assignment node){return false;}
		public override void PostWalk(Assignment node){}

		public override bool Walk(BinaryExpression node){return false;}
		public override void PostWalk(BinaryExpression node){}
		
		public override bool Walk(Block node){return false;}
		public override void PostWalk(Block node){}
		
		public override bool Walk(Call node){return false;}
		public override void PostWalk(Call node){}
		
		public override bool Walk(CastExpression node){return false;}
		public override void PostWalk(CastExpression node){}
		
		public override bool Walk(Comprehension node){return false;}
		public override void PostWalk(Comprehension node){}
		
		public override bool Walk(ComprehensionFor node){return false;}
		public override void PostWalk(ComprehensionFor node){}
		
		public override bool Walk(ComprehensionIf node){return false;}
		public override void PostWalk(ComprehensionIf node){}
		
		public override bool Walk(ConditionalExpression node){return false;}
		public override void PostWalk(ConditionalExpression node){}
		
		public override bool Walk(Constant node){return false;}
		public override void PostWalk(Constant node){}
		
		public override bool Walk(BreakStatement node){return false;}
		public override void PostWalk(BreakStatement node){}
		
		public override bool Walk(ContinueStatement node){return false;}
		public override void PostWalk(ContinueStatement node){}

		public override bool Walk(DefaultExpression node){return false;}
		public override void PostWalk(DefaultExpression node){}

		public override bool Walk(EmptyStatement node){return false;}
		public override void PostWalk(EmptyStatement node){}

		public override bool Walk(ExprStatement node){return false;}
		public override void PostWalk(ExprStatement node){}
		
		public override bool Walk(ForStatement node){return false;}
		public override void PostWalk(ForStatement node){}
		
		public override bool Walk(FunctionDefinition node){return false;}
		public override void PostWalk(FunctionDefinition node){}
		
		public override bool Walk(Identifier node){return false;}
		public override void PostWalk(Identifier node){}
		
		public override bool Walk(IfStatement node){return false;}
		public override void PostWalk(IfStatement node){}
		
		public override bool Walk(IntSeqExpression node){return false;}
		public override void PostWalk(IntSeqExpression node){}
		
		public override bool Walk(MemberReference node){return false;}
		public override void PostWalk(MemberReference node){}
		
		public override bool Walk(ExpressoAst node){return false;}
		public override void PostWalk(ExpressoAst node){}
		
		public override bool Walk(NewExpression node){return false;}
		public override void PostWalk(NewExpression node){}
		
		public override bool Walk(SequenceInitializer node){return false;}
		public override void PostWalk(SequenceInitializer node){}
		
		public override bool Walk(PrintStatement node){return false;}
		public override void PostWalk(PrintStatement node){}
		
		public override bool Walk(RequireStatement node){return false;}
		public override void PostWalk(RequireStatement node){}
		
		public override bool Walk(ReturnStatement node){return false;}
		public override void PostWalk(ReturnStatement node){}
		
		public override bool Walk(SwitchStatement node){return false;}
		public override void PostWalk(SwitchStatement node){}
		
		public override bool Walk(CaseClause node){return false;}
		public override void PostWalk(CaseClause node){}

		public override bool Walk(SequenceExpression node){return false;}
		public override void PostWalk(SequenceExpression node){}
		
		public override bool Walk(ThrowStatement node){return false;}
		public override void PostWalk(ThrowStatement node){}
		
		public override bool Walk(TryStatement node){return false;}
		public override void PostWalk(TryStatement node){}
		
		public override bool Walk(CatchClause node){return false;}
		public override void PostWalk(CatchClause node){}
		
		public override bool Walk(FinallyClause node){return false;}
		public override void PostWalk(FinallyClause node){}
		
		public override bool Walk(TypeDefinition node){return false;}
		public override void PostWalk(TypeDefinition node){}
		
		public override bool Walk(UnaryExpression node){return false;}
		public override void PostWalk(UnaryExpression node){}
		
		public override bool Walk(VarDeclaration node){return false;}
		public override void PostWalk(VarDeclaration node){}
		
		public override bool Walk(WhileStatement node){return false;}
		public override void PostWalk(WhileStatement node){}
		
		public override bool Walk(WithStatement node){return false;}
		public override void PostWalk(WithStatement node){}
		
		public override bool Walk(YieldStatement node){return false;}
		public override void PostWalk(YieldStatement node){}
	}
}

