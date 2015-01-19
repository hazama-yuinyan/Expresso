using System;
using System.Collections.Generic;


namespace Expresso.Ast.Analysis
{
    /// <summary>
    /// A type checker is responsible for type validity check as well as type inference, if needed.
    /// </summary>
    class TypeChecker : IAstWalker
    {
        Parser parser;
        SymbolTable table;  //keep a SymbolTable reference in a private field for convenience

        public TypeChecker(Parser parser)
        {
            this.parser = parser;
            table = parser.Symbols;
        }

        void GoDownScope()
        {
            table = table.Child;
        }

        void GoUpScope()
        {
            table = table.Parent;
        }

        #region IAstWalker implementation

        public void VisitAst(ExpressoAst ast)
        {
            ast.Body.AcceptWalker(this);
        }

        public void VisitBlock(BlockStatement block)
        {
            GoDownScope();
            block.Statements.AcceptWalker(this);
            GoUpScope();
        }

        public void VisitBreakStatement(BreakStatement breakStmt)
        {
            // no op
        }

        public void VisitContinueStatement(ContinueStatement continueStmt)
        {
            // no op
        }

        public void VisitEmptyStatement(EmptyStatement emptyStmt)
        {
            // no op
        }

        public void VisitExpressionStatement(ExpressionStatement exprStmt)
        {
            exprStmt.Expression.AcceptWalker(this);
        }

        public void VisitForStatement(ForStatement forStmt)
        {
            forStmt.Left.AcceptWalker(this);
            forStmt.Target.AcceptWalker(this);
            forStmt.Body.AcceptWalker(this);
        }

        public void VisitIfStatement(IfStatement ifStmt)
        {
            ifStmt.Condition.AcceptWalker(this);
            ifStmt.TrueBlock.AcceptWalker(this);
            ifStmt.FalseBlock.AcceptWalker(this);
        }

        public void VisitReturnStatement(ReturnStatement returnStmt)
        {
            returnStmt.Expression.AcceptWalker(this);
        }

        public void VisitMatchStatement(MatchStatement matchStmt)
        {
            matchStmt.Target.AcceptWalker(this);
            matchStmt.Clauses.AcceptWalker(this);
        }

        public void VisitWhileStatement(WhileStatement whileStmt)
        {
            whileStmt.Condition.AcceptWalker(this);
            whileStmt.Body.AcceptWalker(this);
        }

        public void VisitYieldStatement(YieldStatement yieldStmt)
        {
            yieldStmt.Expression.AcceptWalker(this);
        }

        public void VisitVariableDeclarationStatement(VariableDeclarationStatement varDecl)
        {
            varDecl.Variables.AcceptWalker(this);
        }

        public void VisitAssignment(AssignmentExpression assignment)
        {
            assignment.Right.AcceptWalker(this);
        }

        public void VisitBinaryExpression(BinaryExpression binaryExpr)
        {
            throw new NotImplementedException();
        }

        public void VisitCallExpression(CallExpression callExpr)
        {
            throw new NotImplementedException();
        }

        public void VisitCastExpression(CastExpression castExpr)
        {
            throw new NotImplementedException();
        }

        public void VisitComprehensionExpression(ComprehensionExpression comp)
        {
            throw new NotImplementedException();
        }

        public void VisitComprehensionForClause(ComprehensionForClause compFor)
        {
            throw new NotImplementedException();
        }

        public void VisitComprehensionIfClause(ComprehensionIfClause compIf)
        {
            throw new NotImplementedException();
        }

        public void VisitConditionalExpression(ConditionalExpression condExpr)
        {
            throw new NotImplementedException();
        }

        public void VisitKeyValueLikeExpression(KeyValueLikeExpression keyValue)
        {

        }

        public void VisitLiteralExpression(LiteralExpression literal)
        {
            throw new NotImplementedException();
        }

        public void VisitIdentifier(Identifier ident)
        {
            throw new NotImplementedException();
        }

        public void VisitIntgerSequenceExpression(IntegerSequenceExpression intSeq)
        {
            throw new NotImplementedException();
        }

        public void VisitIndexerExpression(IndexerExpression indexExpr)
        {
            throw new NotImplementedException();
        }

        public void VisitMemberReference(MemberReference memRef)
        {
            throw new NotImplementedException();
        }

        public void VisitNewExpression(NewExpression newExpr)
        {
            throw new NotImplementedException();
        }

        public void VisitPathExpression(PathExpression pathExpr)
        {
            throw new NotImplementedException();
        }

        public void VisitParenthesizedExpression(ParenthesizedExpression parensExpr)
        {
            throw new NotImplementedException();
        }

        public void VisitObjectCreationExpression(ObjectCreationExpression creation)
        {
            throw new NotImplementedException();
        }

        public void VisitSequenceInitializer(SequenceInitializer seqInitializer)
        {
            throw new NotImplementedException();
        }

        public void VisitMatchClause(MatchPatternClause matchClause)
        {
            throw new NotImplementedException();
        }

        public void VisitSequence(SequenceExpression seqExpr)
        {
            throw new NotImplementedException();
        }

        public void VisitUnaryExpression(UnaryExpression unaryExpr)
        {
            throw new NotImplementedException();
        }

        public void VisitSelfReferenceExpression(SelfReferenceExpression selfRef)
        {
            throw new NotImplementedException();
        }

        public void VisitSuperReferenceExpression(SuperReferenceExpression superRef)
        {
            throw new NotImplementedException();
        }

        public void VisitCommentNode(CommentNode comment)
        {
            throw new NotImplementedException();
        }

        public void VisitTextNode(TextNode textNode)
        {
            throw new NotImplementedException();
        }

        public void VisitAstType(AstType typeNode)
        {
            throw new NotImplementedException();
        }

        public void VisitSimpleType(SimpleType simpleType)
        {
            throw new NotImplementedException();
        }

        public void VisitPrimitiveType(PrimitiveType primitiveType)
        {
            throw new NotImplementedException();
        }

        public void VisitImportDeclaration(ImportDeclaration importDecl)
        {
            throw new NotImplementedException();
        }

        public void VisitFunctionDeclaration(FunctionDeclaration funcDecl)
        {
            throw new NotImplementedException();
        }

        public void VisitTypeDeclaration(TypeDeclaration typeDecl)
        {
            throw new NotImplementedException();
        }

        public void VisitFieldDeclaration(FieldDeclaration fieldDecl)
        {
            throw new NotImplementedException();
        }

        public void VisitParameterDeclaration(ParameterDeclaration parameterDecl)
        {
            throw new NotImplementedException();
        }

        public void VisitVariableInitializer(VariableInitializer initializer)
        {
            throw new NotImplementedException();
        }

        public void VisitWildcardPattern(WildcardPattern wildcardPattern)
        {
            throw new NotImplementedException();
        }

        public void VisitIdentifierPattern(IdentifierPattern identifierPattern)
        {
            throw new NotImplementedException();
        }

        public void VisitValueBindingPattern(ValueBindingPattern valueBindingPattern)
        {
            throw new NotImplementedException();
        }

        public void VisitTuplePattern(TuplePattern tuplePattern)
        {
            throw new NotImplementedException();
        }

        public void VisitExpressionPattern(ExpressionPattern exprPattern)
        {
            throw new NotImplementedException();
        }

        public void VisitNullNode(AstNode nullNode)
        {
            throw new NotImplementedException();
        }

        public void VisitNewLine(NewLineNode newlineNode)
        {
            throw new NotImplementedException();
        }

        public void VisitWhitespace(WhitespaceNode whitespaceNode)
        {
            throw new NotImplementedException();
        }

        public void VisitExpressoTokenNode(ExpressoTokenNode tokenNode)
        {
            throw new NotImplementedException();
        }

        public void VisitPatternPlaceholder(AstNode placeholder, ICSharpCode.NRefactory.PatternMatching.Pattern child)
        {
            throw new NotImplementedException();
        }

        #endregion
    }
}

