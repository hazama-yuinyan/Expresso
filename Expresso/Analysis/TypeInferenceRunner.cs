using System;


namespace Expresso.Ast.Analysis
{
    /// <summary>
    /// The type inference runner is responsible for inferring types when asked.
    /// It does the job by inferring and replacing old nodes with the calculated type nodes
    /// in the symbol table. 
    /// </summary>
    /// <remarks>
    /// Currently, I must say it's just a temporal implementation since it messes around the AST itself
    /// in order to keep track of type relations.
    /// </remarks>
    public class TypeInferenceRunner : IAstWalker
    {
        SymbolTable symbols;

        public TypeInferenceRunner(SymbolTable table)
        {
            symbols = table;
        }

        #region IAstWalker implementation

        public void VisitAst(ExpressoAst ast)
        {
            throw new NotImplementedException("Can not work on that node!");
        }

        public void VisitBlock(BlockStatement block)
        {
            throw new NotImplementedException("Can not work on that node!");
        }

        public void VisitBreakStatement(BreakStatement breakStmt)
        {
            throw new NotImplementedException("Can not work on that node!");
        }

        public void VisitContinueStatement(ContinueStatement continueStmt)
        {
            throw new NotImplementedException("Can not work on that node!");
        }

        public void VisitEmptyStatement(EmptyStatement emptyStmt)
        {
            throw new NotImplementedException("Can not work on that node!");
        }

        public void VisitExpressionStatement(ExpressionStatement exprStmt)
        {
            throw new NotImplementedException("Can not work on that node!");
        }

        public void VisitForStatement(ForStatement forStmt)
        {
            throw new NotImplementedException("Can not work on that node!");
        }

        public void VisitIfStatement(IfStatement ifStmt)
        {
            throw new NotImplementedException("Can not work on that node!");
        }

        public void VisitReturnStatement(ReturnStatement returnStmt)
        {
            throw new NotImplementedException("Can not work on that node!");
        }

        public void VisitMatchStatement(MatchStatement matchStmt)
        {
            throw new NotImplementedException("Can not work on that node!");
        }

        public void VisitWhileStatement(WhileStatement whileStmt)
        {
            throw new NotImplementedException("Can not work on that node!");
        }

        public void VisitYieldStatement(YieldStatement yieldStmt)
        {
            throw new NotImplementedException("Can not work on that node!");
        }

        public void VisitVariableDeclarationStatement(VariableDeclarationStatement varDecl)
        {
            throw new NotImplementedException("Can not work on that node!");
        }

        public void VisitAssignment(AssignmentExpression assignment)
        {
            // In an assignment, we want to know the type of the left-hand-side
            // So let's take a look at the right-hand-side
            assignment.Right.AcceptWalker(this);
        }

        public void VisitBinaryExpression(BinaryExpression binaryExpr)
        {
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

