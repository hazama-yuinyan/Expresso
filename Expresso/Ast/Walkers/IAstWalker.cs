using System;
using ICSharpCode.NRefactory.PatternMatching;


namespace Expresso.Ast
{
	/// <summary>
    /// Expresso Walker interface
	/// </summary>
    public interface IAstWalker
	{
        void VisitArgument(Argument arg);
        void VisitAssertStatement(AssertStatement assertStmt);
        void VisitAssignment(Assignment assignment);
        void VisitBinaryExpression(BinaryExpression binaryExpr);
        void VisitBlock(Block block);
        void WalkCallExpression(Call callExpr);
        void WalkCastExpression(CastExpression castExpr);
        void WalkComprehensionFor(ComprehensionFor compFor);
        void WalkComprehensionIf(ComprehensionIf compIf);
        void WalkConditional(ConditionalExpression condExpr);
        void WalkConstant(Constant constant);
        void WalkBreakStatement(BreakStatement breakStmt);
        void WalkContinueStatement(ContinueStatement continueStmt);
        void WalkDefaultExpression(DefaultExpression defaultExpr);
        void WalkEmptyStatement(EmptyStatement emptyStmt);
        void WalkExpressionStatement(ExprStatement exprStmt);
        void WalkForStatement(ForStatement forStmt);
        void WalkFunctionDefinition(FunctionDefinition funcDef);
        void WalkIdentifier(Identifier ident);
        void WalkIfStatement(IfStatement ifStmt);
        void WalkIntSequence(IntSeqExpression intSeq);
        void WalkLateBinding<T>(LateBindExpression<T> lateBinding) where T : class;
        void WalkMemberReference(MemberReference memRef);
        void WalkAst(ExpressoAst ast);
        void WalkNewExpression(NewExpression newExpr);
        void WalkSequenceInitializer(SequenceInitializer seqInitializer);
        void WalkRequireStatement(RequireStatement requireStmt);
        void WalkReturnStatement(ReturnStatement returnStmt);
        void WalkSwitchStatement(SwitchStatement switchStmt);
        void WalkCaseClause(CaseClause caseClause);
        void WalkThrowStatement(ThrowStatement throwStmt);
        void WalkTryStatement(TryStatement tryStmt);
        void WalkSequence(SequenceExpression seqExpr);
        void WalkCatchClause(CatchClause catchClause);
        void WalkFinallyClause(FinallyClause finallyClause);
        void WalkTypeDefinition(TypeDefinition typeDef);
        void WalkUnaryExpression(UnaryExpression unaryExpr);
        void WalkVarDeclaration(VarDeclaration varDecl);
        void WalkWhileStatement(WhileStatement whileStmt);
        void WalkWithStatement(WithStatement withStmt);
        void WalkYieldStatement(YieldStatement yieldStmt);

        void VisitNullNode(AstNode nullNode);
        void VisitPatternPlaceholder(AstNode placeholder, Pattern child);
	}

    public interface IAstWalker<out TResult>
    {
        TResult Walk(Argument arg);
        TResult Walk(AssertStatement assertStmt);
        TResult Walk(Assignment assignment);
        TResult Walk(BinaryExpression binaryExpr);
        TResult Walk(Block block);
        TResult Walk(Call call);
        TResult Walk(CastExpression castExpr);
        TResult Walk(ComprehensionFor compFor);
        TResult Walk(ComprehensionIf compIf);
        TResult Walk(ConditionalExpression condExpr);
        TResult Walk(Constant constant);
        TResult Walk(BreakStatement breakStmt);
        TResult Walk(ContinueStatement continueStmt);
        TResult Walk(DefaultExpression defaultExpr);
        TResult Walk(EmptyStatement emptyStmt);
        TResult Walk(ExprStatement exprStmt);
        TResult Walk(ForStatement forStmt);
        TResult Walk(FunctionDefinition funcDef);
        TResult Walk(Identifier ident);
        TResult Walk(IfStatement ifStmt);
        TResult Walk(IntSeqExpression intSeq);
        TResult Walk<T>(LateBindExpression<T> lateBinding);
        TResult Walk(MemberReference memRef);
        TResult Walk(ExpressoAst ast);
        TResult Walk(NewExpression newExpr);
        TResult Walk(SequenceInitializer seqInitializer);
        TResult Walk(RequireStatement requireStmt);
        TResult Walk(ReturnStatement returnStmt);
        TResult Walk(SwitchStatement switchStmt);
        TResult Walk(CaseClause caseClause);
        TResult Walk(SequenceExpression seqExpr);
        TResult Walk(ThrowStatement throwStmt);
        TResult Walk(TryStatement tryStmt);
        TResult Walk(CatchClause catchClause);
        TResult Walk(FinallyClause finallyClause);
        TResult Walk(TypeDefinition typeDef);
        TResult Walk(UnaryExpression unaryExpr);
        TResult Walk(VarDeclaration varDecl);
        TResult Walk(WhileStatement whileStmt);
        TResult Walk(WithStatement withStmt);
        TResult Walk(YieldStatement yieldStmt);

        TResult VisitNullNode(AstNode nullNode);
        TResult VisitPatternPlaceholder(AstNode placeholder, Pattern child);
    }

    public interface IAstWalker<in TData, out TResult>
    {
        TResult Visit(Argument arg, TData data);
        TResult Visit(AssertStatement assertStmt, TData data);
        TResult Visit(Assignment assignment, TData data);
        TResult Visit(BinaryExpression binaryExpr, TData data);
        TResult Visit(Block block, TData data);
        TResult Visit(Call call, TData data);
        TResult Visit(CastExpression castExpr, TData data);
        TResult Visit(ComprehensionFor compFor, TData data);
        TResult Visit(ComprehensionIf compIf, TData data);
        TResult Visit(ConditionalExpression condExpr, TData data);
        TResult Visit(Constant constant, TData data);
        TResult Visit(BreakStatement breakStmt, TData data);
        TResult Visit(ContinueStatement continueStmt, TData data);
        TResult Visit(DefaultExpression defaultExpr, TData data);
        TResult Visit(EmptyStatement emptyStmt, TData data);
        TResult Visit(ExprStatement exprStmt, TData data);
        TResult Visit(ForStatement forStmt, TData data);
        TResult Visit(FunctionDefinition funcDef, TData data);
        TResult Visit(Identifier ident, TData data);
        TResult Visit(IfStatement ifStmt, TData data);
        TResult Visit(IntSeqExpression intSeq, TData data);
        TResult Visit<T>(LateBindExpression<T> lateBinding, TData data);
        TResult Visit(MemberReference memRef, TData data);
        TResult Visit(ExpressoAst ast, TData data);
        TResult Visit(NewExpression newExpr, TData data);
        TResult Visit(SequenceInitializer seqInitializer, TData data);
        TResult Visit(RequireStatement requireStmt, TData data);
        TResult Visit(ReturnStatement returnStmt, TData data);
        TResult Visit(SwitchStatement switchStmt, TData data);
        TResult Visit(CaseClause caseClause, TData data);
        TResult Visit(SequenceExpression seqExpr, TData data);
        TResult Visit(ThrowStatement throwStmt, TData data);
        TResult Visit(TryStatement tryStmt, TData data);
        TResult Visit(CatchClause catchClause, TData data);
        TResult Visit(FinallyClause finallyClause, TData data);
        TResult Visit(TypeDefinition typeDef, TData data);
        TResult Visit(UnaryExpression unaryExpr, TData data);
        TResult Visit(VarDeclaration varDecl, TData data);
        TResult Visit(WhileStatement whileStmt, TData data);
        TResult Visit(WithStatement withStmt, TData data);
        TResult Visit(YieldStatement yieldStmt, TData data);

        TResult VisitNullNode(AstNode nullNode, TData data);
        TResult VisitPatternPlaceholder(AstNode placeholder, Pattern child, TData data);
    }
}

