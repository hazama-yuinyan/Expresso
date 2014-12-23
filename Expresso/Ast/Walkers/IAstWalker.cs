using System;
using ICSharpCode.NRefactory.PatternMatching;


namespace Expresso.Ast
{
	/// <summary>
    /// Expresso Walker interface
	/// </summary>
    public interface IAstWalker
	{
        void VisitAst(ExpressoAst ast);

        void VisitBlock(BlockStatement block);
        void VisitBreakStatement(BreakStatement breakStmt);
        void VisitContinueStatement(ContinueStatement continueStmt);
        void VisitEmptyStatement(EmptyStatement emptyStmt);
        void VisitExpressionStatement(ExpressionStatement exprStmt);
        void VisitForStatement(ForStatement forStmt);
        void VisitIfStatement(IfStatement ifStmt);
        void VisitImportStatement(ImportStatement importStmt);
        void VisitReturnStatement(ReturnStatement returnStmt);
        void VisitSwitchStatement(SwitchStatement switchStmt);
        void VisitThrowStatement(ThrowStatement throwStmt);
        void VisitTryStatement(TryStatement tryStmt);
        void VisitWhileStatement(WhileStatement whileStmt);
        void VisitWithStatement(WithStatement withStmt);
        void VisitYieldStatement(YieldStatement yieldStmt);
        void VisitVariableDeclarationStatement(VariableDeclarationStatement varDecl);

        void VisitArgument(ParameterDeclaration arg);
        void VisitAssignment(AssignmentExpression assignment);
        void VisitBinaryExpression(BinaryExpression binaryExpr);
        void VisitCallExpression(CallExpression callExpr);
        void VisitCastExpression(CastExpression castExpr);
        void VisitComprehensionExpression(ComprehensionExpression comp);
        void VisitComprehensionForClause(ComprehensionForClause compFor);
        void VisitComprehensionIfClause(ComprehensionIfClause compIf);
        void VisitConditionalExpression(ConditionalExpression condExpr);
        void VisitLiteralExpression(LiteralExpression literal);
        void VisitDefaultExpression(DefaultExpression defaultExpr);
        void VisitIdentifier(Identifier ident);
        void VisitIntgerSequenceExpression(IntegerSequenceExpression intSeq);
        void VisitIndexerExpression(IndexerExpression indexExpr);
        void VisitMemberReference(MemberReference memRef);
        void VisitNewExpression(NewExpression newExpr);
        void VisitParenthesizedExpression(ParenthesizedExpression parensExpr);
        void VisitSequenceInitializer(SequenceInitializer seqInitializer);
        void VisitCaseClause(CaseClause caseClause);
        void VisitSequence(SequenceExpression seqExpr);
        void VisitCatchClause(CatchClause catchClause);
        void VisitFinallyClause(FinallyClause finallyClause);
        void VisitUnaryExpression(UnaryExpression unaryExpr);

        void VisitNullReferenceExpression(NullReferenceExpression nullRef);
        void VisitThisReferenceExpression(ThisReferenceExpression thisRef);
        void VisitBaseReferenceExpression(BaseReferenceExpression baseRef);

        void VisitCommentNode(CommentNode comment);
        void VisitTextNode(TextNode textNode);

        void VisitAstType(AstType typeNode);
        void VisitSimpleType(SimpleType simpleType);
        void VisitPrimitiveType(PrimitiveType primitiveType);

        void VisitFunctionDeclaration(FunctionDeclaration funcDecl);

        void VisitConstructorDeclaration(ConstructorDeclaration constructor);
        void VisitConstructorInitializer(ConstructorInitializer constructorInitializer);
        void VisitFieldDeclaration(FieldDeclaration fieldDecl);
        void VisitMethodDeclaration(MethodDeclaration methodDecl);
        void VisitParameterDeclaration(ParameterDeclaration parameterDecl);
        void VisitVariableInitializer(VariableInitializer initializer);

        void VisitNullNode(AstNode nullNode);
        void VisitNewLine(NewLineNode newlineNode);
        void VisitWhitespace(WhitespaceNode whitespaceNode);
        void VisitExpressoTokenNode(ExpressoTokenNode tokenNode);
        void VisitPatternPlaceholder(AstNode placeholder, Pattern child);
	}

    public interface IAstWalker<out TResult>
    {
        TResult VisitAst(ExpressoAst ast);

        TResult VisitBlock(BlockStatement block);
        TResult VisitBreakStatement(BreakStatement breakStmt);
        TResult VisitContinueStatement(ContinueStatement continueStmt);
        TResult VisitEmptyStatement(EmptyStatement emptyStmt);
        TResult VisitExpressionStatement(ExpressionStatement exprStmt);
        TResult VisitForStatement(ForStatement forStmt);
        TResult VisitIfStatement(IfStatement ifStmt);
        TResult VisitImportStatement(ImportStatement importStmt);
        TResult VisitReturnStatement(ReturnStatement returnStmt);
        TResult VisitSwitchStatement(SwitchStatement switchStmt);
        TResult VisitThrowStatement(ThrowStatement throwStmt);
        TResult VisitTryStatement(TryStatement tryStmt);
        TResult VisitWhileStatement(WhileStatement whileStmt);
        TResult VisitWithStatement(WithStatement withStmt);
        TResult VisitYieldStatement(YieldStatement yieldStmt);
        TResult VisitVariableDeclarationStatement(VariableDeclarationStatement varDecl);

        TResult VisitArgument(ParameterDeclaration arg);
        TResult VisitAssinment(AssignmentExpression assignment);
        TResult VisitBinaryExpression(BinaryExpression binaryExpr);
        TResult VisitCallExpression(CallExpression call);
        TResult VisitCastExpression(CastExpression castExpr);
        TResult VisitComprehensionExpression(ComprehensionExpression comp);
        TResult VisitComprehensionForClause(ComprehensionForClause compFor);
        TResult VisitComprehensionIfClause(ComprehensionIfClause compIf);
        TResult VisitConditionalExpression(ConditionalExpression condExpr);
        TResult VisitLiteralExpression(LiteralExpression literal);
        TResult VisitDefaultExpression(DefaultExpression defaultExpr);
        TResult VisitIdentifier(Identifier ident);
        TResult VisitIntegerSequenceExpression(IntegerSequenceExpression intSeq);
        TResult VisitIndexerExpression(IndexerExpression indexExpr);
        TResult VisitMemberReference(MemberReference memRef);
        TResult VisitNewExpression(NewExpression newExpr);
        TResult VisitParenthesizedExpression(ParenthesizedExpression parensExpr);
        TResult VisitSequenceInitializer(SequenceInitializer seqInitializer);
        TResult VisitCaseClause(CaseClause caseClause);
        TResult VisitSequence(SequenceExpression seqExpr);
        TResult VisitCatchClause(CatchClause catchClause);
        TResult VisitFinallyClause(FinallyClause finallyClause);
        TResult VisitUnaryExpression(UnaryExpression unaryExpr);

        TResult VisitNullReferenceExpression(NullReferenceExpression nullRef);
        TResult VisitThisReferenceExpression(ThisReferenceExpression thisRef);
        TResult VisitBaseReferenceExpression(BaseReferenceExpression baseRef);

        TResult VisitCommentNode(CommentNode comment);
        TResult VisitTextNode(TextNode textNode);

        TResult VisitAstType(AstType typeNode);
        TResult VisitSimpleType(SimpleType simpleType);
        TResult VisitPrimitiveType(PrimitiveType primitiveType);

        TResult VisitFunctionDeclaration(FunctionDeclaration funcDecl);

        TResult VisitConstructorDeclaration(ConstructorDeclaration constructor);
        TResult VisitConstructorInitializer(ConstructorInitializer constructorInitializer);
        TResult VisitFieldDeclaration(FieldDeclaration fieldDecl);
        TResult VisitMethodDeclaration(MethodDeclaration methodDecl);
        TResult VisitParameterDeclaration(ParameterDeclaration parameterDecl);
        TResult VisitVariableInitializer(VariableInitializer initializer);

        TResult VisitNullNode(AstNode nullNode);
        TResult VisitNewLine(NewLineNode newlineNode);
        TResult VisitWhitespace(WhitespaceNode whitespaceNode);
        TResult VisitExpressoTokenNode(ExpressoTokenNode tokenNode);
        TResult VisitPatternPlaceholder(AstNode placeholder, Pattern child);
    }

    public interface IAstWalker<in TData, out TResult>
    {
        TResult VisitAst(ExpressoAst ast, TData data);

        TResult VisitBlock(BlockStatement block, TData data);
        TResult VisitBreakStatement(BreakStatement breakStmt, TData data);
        TResult VisitContinueStatement(ContinueStatement continueStmt, TData data);
        TResult VisitEmptyStatement(EmptyStatement emptyStmt, TData data);
        TResult VisitExpressionStatement(ExpressionStatement exprStmt, TData data);
        TResult VisitForStatement(ForStatement forStmt, TData data);
        TResult VisitIfStatement(IfStatement ifStmt, TData data);
        TResult VisitImportStatement(ImportStatement importStmt, TData data);
        TResult VisitReturnStatement(ReturnStatement returnStmt, TData data);
        TResult VisitSwitchStatement(SwitchStatement switchStmt, TData data);
        TResult VisitThrowStatement(ThrowStatement throwStmt, TData data);
        TResult VisitTryStatement(TryStatement tryStmt, TData data);
        TResult VisitWhileStatement(WhileStatement whileStmt, TData data);
        TResult VisitWithStatement(WithStatement withStmt, TData data);
        TResult VisitYieldStatement(YieldStatement yieldStmt, TData data);
        TResult VisitVariableDeclarationStatement(VariableDeclarationStatement varDecl, TData data);

        TResult VisitArgument(ParameterDeclaration arg, TData data);
        TResult VisitAssignment(AssignmentExpression assignment, TData data);
        TResult VisitBinaryExpression(BinaryExpression binaryExpr, TData data);
        TResult VisitCallExpression(CallExpression call, TData data);
        TResult VisitCastExpression(CastExpression castExpr, TData data);
        TResult VisitComprehensionExpression(ComprehensionExpression comp, TData data);
        TResult VisitComprehensionForClause(ComprehensionForClause compFor, TData data);
        TResult VisitComprehensionIfClause(ComprehensionIfClause compIf, TData data);
        TResult VisitConditionalExpression(ConditionalExpression condExpr, TData data);
        TResult VisitLiteralExpression(LiteralExpression literal, TData data);
        TResult VisitDefaultExpression(DefaultExpression defaultExpr, TData data);
        TResult VisitIdentifier(Identifier ident, TData data);
        TResult VisitIntegerSequenceExpression(IntegerSequenceExpression intSeq, TData data);
        TResult VisitIndexerExpression(IndexerExpression indexExpr, TData data);
        TResult VisitMemberReference(MemberReference memRef, TData data);
        TResult VisitNewExpression(NewExpression newExpr, TData data);
        TResult VisitParenthesizedExpression(ParenthesizedExpression parensExpr, TData data);
        TResult VisitSequenceInitializer(SequenceInitializer seqInitializer, TData data);
        TResult VisitCaseClause(CaseClause caseClause, TData data);
        TResult VisitSequence(SequenceExpression seqExpr, TData data);
        TResult VisitCatchClause(CatchClause catchClause, TData data);
        TResult VisitFinallyClause(FinallyClause finallyClause, TData data);
        TResult VisitUnaryExpression(UnaryExpression unaryExpr, TData data);

        TResult VisitNullReferenceExpression(NullReferenceExpression nullRef, TData data);
        TResult VisitThisReferenceExpression(ThisReferenceExpression thisRef, TData data);
        TResult VisitBaseReferenceExpression(BaseReferenceExpression baseRef, TData data);

        TResult VisitCommentNode(CommentNode comment, TData data);
        TResult VisitTextNode(TextNode textNode, TData data);

        TResult VisitAstType(AstType typeNode, TData data);
        TResult VisitSimpleType(SimpleType simpleType, TData data);
        TResult VisitPrimitiveType(PrimitiveType primitiveType, TData data);

        TResult VisitFunctionDeclaration(FunctionDeclaration funcDecl, TData data);

        TResult VisitConstructorDeclaration(ConstructorDeclaration constructor, TData data);
        TResult VisitConstructorInitializer(ConstructorInitializer constructorInitializer, TData data);
        TResult VisitFieldDeclaration(FieldDeclaration fieldDecl, TData data);
        TResult VisitMethodDeclaration(MethodDeclaration methodDecl, TData data);
        TResult VisitParameterDeclaration(ParameterDeclaration parameterDecl, TData data);
        TResult VisitVariableInitializer(VariableInitializer initializer, TData data);

        TResult VisitNullNode(AstNode nullNode, TData data);
        TResult VisitNewLine(NewLineNode newlineNode, TData data);
        TResult VisitWhitespace(WhitespaceNode whitespaceNode, TData data);
        TResult VisitExpressoTokenNode(ExpressoTokenNode tokenNode, TData data);
        TResult VisitPatternPlaceholder(AstNode placeholder, Pattern child, TData data);
    }
}

