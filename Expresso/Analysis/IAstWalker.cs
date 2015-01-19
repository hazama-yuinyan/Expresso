using System;
using ICSharpCode.NRefactory.PatternMatching;


namespace Expresso.Ast
{
	/// <summary>
    /// Expresso Walker interface.
    /// An AST walker is a class that walks through all nodes 
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
        void VisitReturnStatement(ReturnStatement returnStmt);
        void VisitMatchStatement(MatchStatement matchStmt);
        void VisitWhileStatement(WhileStatement whileStmt);
        void VisitYieldStatement(YieldStatement yieldStmt);
        void VisitVariableDeclarationStatement(VariableDeclarationStatement varDecl);

        void VisitAssignment(AssignmentExpression assignment);
        void VisitBinaryExpression(BinaryExpression binaryExpr);
        void VisitCallExpression(CallExpression callExpr);
        void VisitCastExpression(CastExpression castExpr);
        void VisitComprehensionExpression(ComprehensionExpression comp);
        void VisitComprehensionForClause(ComprehensionForClause compFor);
        void VisitComprehensionIfClause(ComprehensionIfClause compIf);
        void VisitConditionalExpression(ConditionalExpression condExpr);
        void VisitKeyValueLikeExpression(KeyValueLikeExpression keyValue);
        void VisitLiteralExpression(LiteralExpression literal);
        void VisitIdentifier(Identifier ident);
        void VisitIntgerSequenceExpression(IntegerSequenceExpression intSeq);
        void VisitIndexerExpression(IndexerExpression indexExpr);
        void VisitMemberReference(MemberReference memRef);
        void VisitNewExpression(NewExpression newExpr);
        void VisitPathExpression(PathExpression pathExpr);
        void VisitParenthesizedExpression(ParenthesizedExpression parensExpr);
        void VisitObjectCreationExpression(ObjectCreationExpression creation);
        void VisitSequenceInitializer(SequenceInitializer seqInitializer);
        void VisitMatchClause(MatchPatternClause matchClause);
        void VisitSequence(SequenceExpression seqExpr);
        void VisitUnaryExpression(UnaryExpression unaryExpr);

        void VisitSelfReferenceExpression(SelfReferenceExpression selfRef);
        void VisitSuperReferenceExpression(SuperReferenceExpression superRef);

        void VisitCommentNode(CommentNode comment);
        void VisitTextNode(TextNode textNode);

        void VisitAstType(AstType typeNode);
        void VisitSimpleType(SimpleType simpleType);
        void VisitPrimitiveType(PrimitiveType primitiveType);

        void VisitImportDeclaration(ImportDeclaration importDecl);
        void VisitFunctionDeclaration(FunctionDeclaration funcDecl);
        void VisitTypeDeclaration(TypeDeclaration typeDecl);

        void VisitFieldDeclaration(FieldDeclaration fieldDecl);
        void VisitParameterDeclaration(ParameterDeclaration parameterDecl);
        void VisitVariableInitializer(VariableInitializer initializer);

        void VisitWildcardPattern(WildcardPattern wildcardPattern);
        void VisitIdentifierPattern(IdentifierPattern identifierPattern);
        void VisitValueBindingPattern(ValueBindingPattern valueBindingPattern);
        void VisitTuplePattern(TuplePattern tuplePattern);
        void VisitExpressionPattern(ExpressionPattern exprPattern);

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
        TResult VisitReturnStatement(ReturnStatement returnStmt);
        TResult VisitMatchStatement(MatchStatement matchStmt);
        TResult VisitWhileStatement(WhileStatement whileStmt);
        TResult VisitYieldStatement(YieldStatement yieldStmt);
        TResult VisitVariableDeclarationStatement(VariableDeclarationStatement varDecl);

        TResult VisitAssinment(AssignmentExpression assignment);
        TResult VisitBinaryExpression(BinaryExpression binaryExpr);
        TResult VisitCallExpression(CallExpression call);
        TResult VisitCastExpression(CastExpression castExpr);
        TResult VisitComprehensionExpression(ComprehensionExpression comp);
        TResult VisitComprehensionForClause(ComprehensionForClause compFor);
        TResult VisitComprehensionIfClause(ComprehensionIfClause compIf);
        TResult VisitConditionalExpression(ConditionalExpression condExpr);
        TResult VisitKeyValueLikeExpression(KeyValueLikeExpression keyValue);
        TResult VisitLiteralExpression(LiteralExpression literal);
        TResult VisitIdentifier(Identifier ident);
        TResult VisitIntegerSequenceExpression(IntegerSequenceExpression intSeq);
        TResult VisitIndexerExpression(IndexerExpression indexExpr);
        TResult VisitMemberReference(MemberReference memRef);
        TResult VisitNewExpression(NewExpression newExpr);
        TResult VisitPathExpression(PathExpression pathExpr);
        TResult VisitParenthesizedExpression(ParenthesizedExpression parensExpr);
        TResult VisitObjectCreationExpression(ObjectCreationExpression creation);
        TResult VisitSequenceInitializer(SequenceInitializer seqInitializer);
        TResult VisitMatchClause(MatchPatternClause matchClause);
        TResult VisitSequence(SequenceExpression seqExpr);
        TResult VisitUnaryExpression(UnaryExpression unaryExpr);

        TResult VisitSelfReferenceExpression(SelfReferenceExpression selfRef);
        TResult VisitSuperReferenceExpression(SuperReferenceExpression superRef);

        TResult VisitCommentNode(CommentNode comment);
        TResult VisitTextNode(TextNode textNode);

        TResult VisitAstType(AstType typeNode);
        TResult VisitSimpleType(SimpleType simpleType);
        TResult VisitPrimitiveType(PrimitiveType primitiveType);

        TResult VisitImportDeclaration(ImportDeclaration importDecl);
        TResult VisitFunctionDeclaration(FunctionDeclaration funcDecl);
        TResult VisitTypeDeclaration(TypeDeclaration typeDecl);

        TResult VisitFieldDeclaration(FieldDeclaration fieldDecl);
        TResult VisitParameterDeclaration(ParameterDeclaration parameterDecl);
        TResult VisitVariableInitializer(VariableInitializer initializer);

        TResult VisitWildcardPattern(WildcardPattern wildcardPattern);
        TResult VisitIdentifierPattern(IdentifierPattern identifierPattern);
        TResult VisitValueBindingPattern(ValueBindingPattern valueBindingPattern);
        TResult VisitTuplePattern(TuplePattern tuplePattern);
        TResult VisitExpressionPattern(ExpressionPattern exprPattern);

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
        TResult VisitReturnStatement(ReturnStatement returnStmt, TData data);
        TResult VisitMatchStatement(MatchStatement matchStmt, TData data);
        TResult VisitWhileStatement(WhileStatement whileStmt, TData data);
        TResult VisitYieldStatement(YieldStatement yieldStmt, TData data);
        TResult VisitVariableDeclarationStatement(VariableDeclarationStatement varDecl, TData data);

        TResult VisitAssignment(AssignmentExpression assignment, TData data);
        TResult VisitBinaryExpression(BinaryExpression binaryExpr, TData data);
        TResult VisitCallExpression(CallExpression call, TData data);
        TResult VisitCastExpression(CastExpression castExpr, TData data);
        TResult VisitComprehensionExpression(ComprehensionExpression comp, TData data);
        TResult VisitComprehensionForClause(ComprehensionForClause compFor, TData data);
        TResult VisitComprehensionIfClause(ComprehensionIfClause compIf, TData data);
        TResult VisitConditionalExpression(ConditionalExpression condExpr, TData data);
        TResult VisitKeyValueLikeExpression(KeyValueLikeExpression keyValue, TData data);
        TResult VisitLiteralExpression(LiteralExpression literal, TData data);
        TResult VisitIdentifier(Identifier ident, TData data);
        TResult VisitIntegerSequenceExpression(IntegerSequenceExpression intSeq, TData data);
        TResult VisitIndexerExpression(IndexerExpression indexExpr, TData data);
        TResult VisitMemberReference(MemberReference memRef, TData data);
        TResult VisitNewExpression(NewExpression newExpr, TData data);
        TResult VisitPathExpression(PathExpression pathExpr, TData data);
        TResult VisitParenthesizedExpression(ParenthesizedExpression parensExpr, TData data);
        TResult VisitObjectCreationExpression(ObjectCreationExpression creation, TData data);
        TResult VisitSequenceInitializer(SequenceInitializer seqInitializer, TData data);
        TResult VisitMatchClause(MatchPatternClause matchClause, TData data);
        TResult VisitSequence(SequenceExpression seqExpr, TData data);
        TResult VisitUnaryExpression(UnaryExpression unaryExpr, TData data);

        TResult VisitSelfReferenceExpression(SelfReferenceExpression selfRef, TData data);
        TResult VisitSuperReferenceExpression(SuperReferenceExpression superRef, TData data);

        TResult VisitCommentNode(CommentNode comment, TData data);
        TResult VisitTextNode(TextNode textNode, TData data);

        TResult VisitAstType(AstType typeNode, TData data);
        TResult VisitSimpleType(SimpleType simpleType, TData data);
        TResult VisitPrimitiveType(PrimitiveType primitiveType, TData data);

        TResult VisitImportDeclaration(ImportDeclaration importDecl, TData data);
        TResult VisitFunctionDeclaration(FunctionDeclaration funcDecl, TData data);
        TResult VisitTypeDeclaration(TypeDeclaration typeDecl, TData data);

        TResult VisitFieldDeclaration(FieldDeclaration fieldDecl, TData data);
        TResult VisitParameterDeclaration(ParameterDeclaration parameterDecl, TData data);
        TResult VisitVariableInitializer(VariableInitializer initializer, TData data);

        TResult VisitWildcardPattern(WildcardPattern wildcardPattern, TData data);
        TResult VisitIdentifierPattern(IdentifierPattern identifierPattern, TData data);
        TResult VisitValueBindingPattern(ValueBindingPattern valueBindingPattern, TData data);
        TResult VisitTuplePattern(TuplePattern tuplePattern, TData data);
        TResult VisitExpressionPattern(ExpressionPattern exprPattern, TData data);

        TResult VisitNullNode(AstNode nullNode, TData data);
        TResult VisitNewLine(NewLineNode newlineNode, TData data);
        TResult VisitWhitespace(WhitespaceNode whitespaceNode, TData data);
        TResult VisitExpressoTokenNode(ExpressoTokenNode tokenNode, TData data);
        TResult VisitPatternPlaceholder(AstNode placeholder, Pattern child, TData data);
    }
}
