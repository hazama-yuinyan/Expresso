using System;
using ICSharpCode.NRefactory.PatternMatching;


namespace Expresso.Ast
{
	/// <summary>
    /// Expresso Walker interface.
    /// An AST walker is a class that walks through all nodes and processes each node.
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
        void VisitValueBindingForStatement(ValueBindingForStatement valueBindingForStmt);
        void VisitIfStatement(IfStatement ifStmt);
        void VisitReturnStatement(ReturnStatement returnStmt);
        void VisitMatchStatement(MatchStatement matchStmt);
        void VisitWhileStatement(WhileStatement whileStmt);
        void VisitYieldStatement(YieldStatement yieldStmt);
        void VisitTryStatement(TryStatement tryStmt);
        void VisitThrowStatement(ThrowStatement throwStmt);
        void VisitVariableDeclarationStatement(VariableDeclarationStatement varDecl);

        void VisitAssignment(AssignmentExpression assignment);
        void VisitBinaryExpression(BinaryExpression binaryExpr);
        void VisitCallExpression(CallExpression callExpr);
        void VisitCastExpression(CastExpression castExpr);
        void VisitCatchClause(CatchClause catchClause);
        void VisitClosureLiteralExpression(ClosureLiteralExpression closure);
        void VisitComprehensionExpression(ComprehensionExpression comp);
        void VisitComprehensionForClause(ComprehensionForClause compFor);
        void VisitComprehensionIfClause(ComprehensionIfClause compIf);
        void VisitConditionalExpression(ConditionalExpression condExpr);
        void VisitKeyValueLikeExpression(KeyValueLikeExpression keyValue);
        void VisitLiteralExpression(LiteralExpression literal);
        void VisitIdentifier(Identifier ident);
        void VisitIntegerSequenceExpression(IntegerSequenceExpression intSeq);
        void VisitIndexerExpression(IndexerExpression indexExpr);
        void VisitMemberReference(MemberReferenceExpression memRef);
        void VisitNewExpression(NewExpression newExpr);
        void VisitPathExpression(PathExpression pathExpr);
        void VisitParenthesizedExpression(ParenthesizedExpression parensExpr);
        void VisitObjectCreationExpression(ObjectCreationExpression creation);
        void VisitSequenceInitializer(SequenceInitializer seqInitializer);
        void VisitMatchClause(MatchPatternClause matchClause);
        void VisitSequenceExpression(SequenceExpression seqExpr);
        void VisitUnaryExpression(UnaryExpression unaryExpr);

        void VisitSelfReferenceExpression(SelfReferenceExpression selfRef);
        void VisitSuperReferenceExpression(SuperReferenceExpression superRef);

        void VisitCommentNode(CommentNode comment);
        void VisitTextNode(TextNode textNode);

        void VisitSimpleType(SimpleType simpleType);
        void VisitPrimitiveType(PrimitiveType primitiveType);
        void VisitReferenceType(ReferenceType referenceType);
        void VisitMemberType(MemberType memberType);
        void VisitFunctionType(FunctionType funcType);
        void VisitParameterType(ParameterType paramType);
        void VisitPlaceholderType(PlaceholderType placeholderType);

        void VisitImportDeclaration(ImportDeclaration importDecl);
        void VisitFunctionDeclaration(FunctionDeclaration funcDecl);
        void VisitTypeDeclaration(TypeDeclaration typeDecl);
        void VisitAliasDeclaration(AliasDeclaration aliasDecl);

        void VisitFieldDeclaration(FieldDeclaration fieldDecl);
        void VisitParameterDeclaration(ParameterDeclaration parameterDecl);
        void VisitVariableInitializer(VariableInitializer initializer);

        void VisitWildcardPattern(WildcardPattern wildcardPattern);
        void VisitIdentifierPattern(IdentifierPattern identifierPattern);
        void VisitValueBindingPattern(ValueBindingPattern valueBindingPattern);
        void VisitCollectionPattern(CollectionPattern collectionPattern);
        void VisitDestructuringPattern(DestructuringPattern destructuringPattern);
        void VisitTuplePattern(TuplePattern tuplePattern);
        void VisitExpressionPattern(ExpressionPattern exprPattern);
        void VisitIgnoringRestPattern(IgnoringRestPattern restPattern);
        void VisitKeyValuePattern(KeyValuePattern keyValuePattern);

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
        TResult VisitValueBindingForStatement(ValueBindingForStatement valueBindingForStatment);
        TResult VisitIfStatement(IfStatement ifStmt);
        TResult VisitReturnStatement(ReturnStatement returnStmt);
        TResult VisitMatchStatement(MatchStatement matchStmt);
        TResult VisitWhileStatement(WhileStatement whileStmt);
        TResult VisitYieldStatement(YieldStatement yieldStmt);
        TResult VisitThrowStatement(ThrowStatement throwStmt);
        TResult VisitTryStatement(TryStatement tryStmt);
        TResult VisitVariableDeclarationStatement(VariableDeclarationStatement varDecl);

        TResult VisitAssignment(AssignmentExpression assignment);
        TResult VisitBinaryExpression(BinaryExpression binaryExpr);
        TResult VisitCallExpression(CallExpression call);
        TResult VisitCastExpression(CastExpression castExpr);
        TResult VisitCatchClause(CatchClause catchClause);
        TResult VisitClosureLiteralExpression(ClosureLiteralExpression closure);
        TResult VisitComprehensionExpression(ComprehensionExpression comp);
        TResult VisitComprehensionForClause(ComprehensionForClause compFor);
        TResult VisitComprehensionIfClause(ComprehensionIfClause compIf);
        TResult VisitConditionalExpression(ConditionalExpression condExpr);
        TResult VisitKeyValueLikeExpression(KeyValueLikeExpression keyValue);
        TResult VisitLiteralExpression(LiteralExpression literal);
        TResult VisitIdentifier(Identifier ident);
        TResult VisitIntegerSequenceExpression(IntegerSequenceExpression intSeq);
        TResult VisitIndexerExpression(IndexerExpression indexExpr);
        TResult VisitMemberReference(MemberReferenceExpression memRef);
        TResult VisitNewExpression(NewExpression newExpr);
        TResult VisitPathExpression(PathExpression pathExpr);
        TResult VisitParenthesizedExpression(ParenthesizedExpression parensExpr);
        TResult VisitObjectCreationExpression(ObjectCreationExpression creation);
        TResult VisitSequenceInitializer(SequenceInitializer seqInitializer);
        TResult VisitMatchClause(MatchPatternClause matchClause);
        TResult VisitSequenceExpression(SequenceExpression seqExpr);
        TResult VisitUnaryExpression(UnaryExpression unaryExpr);

        TResult VisitSelfReferenceExpression(SelfReferenceExpression selfRef);
        TResult VisitSuperReferenceExpression(SuperReferenceExpression superRef);

        TResult VisitCommentNode(CommentNode comment);
        TResult VisitTextNode(TextNode textNode);

        TResult VisitSimpleType(SimpleType simpleType);
        TResult VisitPrimitiveType(PrimitiveType primitiveType);
        TResult VisitReferenceType(ReferenceType referenceType);
        TResult VisitMemberType(MemberType memberType);
        TResult VisitFunctionType(FunctionType funcType);
        TResult VisitParameterType(ParameterType paramType);
        TResult VisitPlaceholderType(PlaceholderType placeholderType);

        TResult VisitImportDeclaration(ImportDeclaration importDecl);
        TResult VisitFunctionDeclaration(FunctionDeclaration funcDecl);
        TResult VisitTypeDeclaration(TypeDeclaration typeDecl);
        TResult VisitAliasDeclaration(AliasDeclaration aliasDecl);

        TResult VisitFieldDeclaration(FieldDeclaration fieldDecl);
        TResult VisitParameterDeclaration(ParameterDeclaration parameterDecl);
        TResult VisitVariableInitializer(VariableInitializer initializer);

        TResult VisitWildcardPattern(WildcardPattern wildcardPattern);
        TResult VisitIdentifierPattern(IdentifierPattern identifierPattern);
        TResult VisitValueBindingPattern(ValueBindingPattern valueBindingPattern);
        TResult VisitCollectionPattern(CollectionPattern collectionPattern);
        TResult VisitDestructuringPattern(DestructuringPattern destructuringPattern);
        TResult VisitTuplePattern(TuplePattern tuplePattern);
        TResult VisitExpressionPattern(ExpressionPattern exprPattern);
        TResult VisitIgnoringRestPattern(IgnoringRestPattern restPattern);
        TResult VisitKeyValuePattern(KeyValuePattern keyValuePattern);

        TResult VisitNullNode(AstNode nullNode);
        TResult VisitNewLine(NewLineNode newlineNode);
        TResult VisitWhitespace(WhitespaceNode whitespaceNode);
        TResult VisitExpressoTokenNode(ExpressoTokenNode tokenNode);
        TResult VisitPatternPlaceholder(AstNode placeholder, Pattern child);
    }

    /// <summary>
    /// Expresso Walker interface.
    /// An AST walker is a class that walks through all nodes.
    /// </summary>
    /// <typeparam>
    /// TData
    /// </typeparam>
    /// <typeparam>
    /// TResult
    /// </typeparam>
    public interface IAstWalker<in TData, out TResult>
    {
        TResult VisitAst(ExpressoAst ast, TData data);

        TResult VisitBlock(BlockStatement block, TData data);
        TResult VisitBreakStatement(BreakStatement breakStmt, TData data);
        TResult VisitContinueStatement(ContinueStatement continueStmt, TData data);
        TResult VisitEmptyStatement(EmptyStatement emptyStmt, TData data);
        TResult VisitExpressionStatement(ExpressionStatement exprStmt, TData data);
        TResult VisitForStatement(ForStatement forStmt, TData data);
        TResult VisitValueBindingForStatement(ValueBindingForStatement valueBindingForStatement, TData data);
        TResult VisitIfStatement(IfStatement ifStmt, TData data);
        TResult VisitReturnStatement(ReturnStatement returnStmt, TData data);
        TResult VisitMatchStatement(MatchStatement matchStmt, TData data);
        TResult VisitWhileStatement(WhileStatement whileStmt, TData data);
        TResult VisitYieldStatement(YieldStatement yieldStmt, TData data);
        TResult VisitTryStatement(TryStatement tryStmt, TData data);
        TResult VisitThrowStatement(ThrowStatement throwStmt, TData data);
        TResult VisitVariableDeclarationStatement(VariableDeclarationStatement varDecl, TData data);

        TResult VisitAssignment(AssignmentExpression assignment, TData data);
        TResult VisitBinaryExpression(BinaryExpression binaryExpr, TData data);
        TResult VisitCallExpression(CallExpression call, TData data);
        TResult VisitCastExpression(CastExpression castExpr, TData data);
        TResult VisitCatchClause(CatchClause catchClause, TData data);
        TResult VisitClosureLiteralExpression(ClosureLiteralExpression closure, TData data);
        TResult VisitComprehensionExpression(ComprehensionExpression comp, TData data);
        TResult VisitComprehensionForClause(ComprehensionForClause compFor, TData data);
        TResult VisitComprehensionIfClause(ComprehensionIfClause compIf, TData data);
        TResult VisitConditionalExpression(ConditionalExpression condExpr, TData data);
        TResult VisitKeyValueLikeExpression(KeyValueLikeExpression keyValue, TData data);
        TResult VisitLiteralExpression(LiteralExpression literal, TData data);
        TResult VisitIdentifier(Identifier ident, TData data);
        TResult VisitIntegerSequenceExpression(IntegerSequenceExpression intSeq, TData data);
        TResult VisitIndexerExpression(IndexerExpression indexExpr, TData data);
        TResult VisitMemberReference(MemberReferenceExpression memRef, TData data);
        TResult VisitNewExpression(NewExpression newExpr, TData data);
        TResult VisitPathExpression(PathExpression pathExpr, TData data);
        TResult VisitParenthesizedExpression(ParenthesizedExpression parensExpr, TData data);
        TResult VisitObjectCreationExpression(ObjectCreationExpression creation, TData data);
        TResult VisitSequenceInitializer(SequenceInitializer seqInitializer, TData data);
        TResult VisitMatchClause(MatchPatternClause matchClause, TData data);
        TResult VisitSequenceExpression(SequenceExpression seqExpr, TData data);
        TResult VisitUnaryExpression(UnaryExpression unaryExpr, TData data);

        TResult VisitSelfReferenceExpression(SelfReferenceExpression selfRef, TData data);
        TResult VisitSuperReferenceExpression(SuperReferenceExpression superRef, TData data);

        TResult VisitCommentNode(CommentNode comment, TData data);
        TResult VisitTextNode(TextNode textNode, TData data);

        TResult VisitSimpleType(SimpleType simpleType, TData data);
        TResult VisitPrimitiveType(PrimitiveType primitiveType, TData data);
        TResult VisitReferenceType(ReferenceType referenceType, TData data);
        TResult VisitMemberType(MemberType memberType, TData data);
        TResult VisitFunctionType(FunctionType funcType, TData data);
        TResult VisitParameterType(ParameterType paramType, TData data);
        TResult VisitPlaceholderType(PlaceholderType placeholderType, TData data);

        TResult VisitImportDeclaration(ImportDeclaration importDecl, TData data);
        TResult VisitFunctionDeclaration(FunctionDeclaration funcDecl, TData data);
        TResult VisitTypeDeclaration(TypeDeclaration typeDecl, TData data);
        TResult VisitAliasDeclaration(AliasDeclaration aliasDecl, TData data);

        TResult VisitFieldDeclaration(FieldDeclaration fieldDecl, TData data);
        TResult VisitParameterDeclaration(ParameterDeclaration parameterDecl, TData data);
        TResult VisitVariableInitializer(VariableInitializer initializer, TData data);

        TResult VisitWildcardPattern(WildcardPattern wildcardPattern, TData data);
        TResult VisitIdentifierPattern(IdentifierPattern identifierPattern, TData data);
        TResult VisitValueBindingPattern(ValueBindingPattern valueBindingPattern, TData data);
        TResult VisitCollectionPattern(CollectionPattern collectionPattern, TData data);
        TResult VisitDestructuringPattern(DestructuringPattern destructuringPattern, TData data);
        TResult VisitTuplePattern(TuplePattern tuplePattern, TData data);
        TResult VisitExpressionPattern(ExpressionPattern exprPattern, TData data);
        TResult VisitIgnoringRestPattern(IgnoringRestPattern restPattern, TData data);
        TResult VisitKeyValuePattern(KeyValuePattern keyValuePattern, TData data);

        TResult VisitNullNode(AstNode nullNode, TData data);
        TResult VisitNewLine(NewLineNode newlineNode, TData data);
        TResult VisitWhitespace(WhitespaceNode whitespaceNode, TData data);
        TResult VisitExpressoTokenNode(ExpressoTokenNode tokenNode, TData data);
        TResult VisitPatternPlaceholder(AstNode placeholder, Pattern child, TData data);
    }
}

