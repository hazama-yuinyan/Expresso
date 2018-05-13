using System;
using ICSharpCode.NRefactory.PatternMatching;

namespace Expresso.Ast.Analysis
{
    partial class TypeChecker : IAstWalker<AstType>
    {
        /// <summary>
        /// This class is responsible for determining context and prohibiting the use of null in contexts without .NET
        /// </summary>
        public class NullCheckWalker : IAstWalker
        {
            TypeChecker checker;

            internal NullCheckWalker(TypeChecker checker)
            {
                this.checker = checker;
            }

            public void VisitAliasDeclaration(AliasDeclaration aliasDecl)
            {
                throw new InvalidOperationException("Can not work on that node");
            }

            public void VisitAssignment(AssignmentExpression assignment)
            {
                throw new InvalidOperationException("Can not work on that node");
            }

            public void VisitAst(ExpressoAst ast)
            {
                throw new InvalidOperationException("Can not work on that node");
            }

            public void VisitAttributeSection(AttributeSection section)
            {
                throw new InvalidOperationException("Can not work on that node");
            }

            public void VisitBinaryExpression(BinaryExpression binaryExpr)
            {
                var left_type = binaryExpr.Left.AcceptWalker(checker);
                var table = checker.symbols.GetTypeTable(!left_type.IdentifierNode.Type.IsNull ? left_type.IdentifierNode.Type.Name : left_type.Name);
                if(!table.IsForeignType){
                    throw new ParserException(
                        "In Expresso, null literals can only be used in contexts with .NET.",
                        "ES1022",
                        binaryExpr
                    );
                }
            }

            public void VisitBlock(BlockStatement block)
            {
                throw new InvalidOperationException("Can not work on that node");
            }

            public void VisitBreakStatement(BreakStatement breakStmt)
            {
                throw new InvalidOperationException("Can not work on that node");
            }

            public void VisitCallExpression(CallExpression callExpr)
            {
                var target_type = callExpr.Target.AcceptWalker(checker);
                var table = checker.symbols.GetTypeTable(!target_type.IdentifierNode.Type.IsNull ? target_type.IdentifierNode.Type.Name : target_type.Name);
                if(!table.IsForeignType){
                    throw new ParserException(
                        "In Expresso, null literals can only be used in contexts with .NET.",
                        "ES1022",
                        callExpr
                    );
                }
            }

            public void VisitCastExpression(CastExpression castExpr)
            {
                throw new InvalidOperationException("Can not work on that node");
            }

            public void VisitCatchClause(CatchClause catchClause)
            {
                throw new InvalidOperationException("Can not work on that node");
            }

            public void VisitClosureLiteralExpression(ClosureLiteralExpression closure)
            {
                throw new InvalidOperationException("Can not work on that node");
            }

            public void VisitCollectionPattern(CollectionPattern collectionPattern)
            {
                throw new InvalidOperationException("Can not work on that node");
            }

            public void VisitCommentNode(CommentNode comment)
            {
                throw new InvalidOperationException("Can not work on that node");
            }

            public void VisitComprehensionExpression(ComprehensionExpression comp)
            {
                throw new InvalidOperationException("Can not work on that node");
            }

            public void VisitComprehensionForClause(ComprehensionForClause compFor)
            {
                throw new InvalidOperationException("Can not work on that node");
            }

            public void VisitComprehensionIfClause(ComprehensionIfClause compIf)
            {
                throw new InvalidOperationException("Can not work on that node");
            }

            public void VisitConditionalExpression(ConditionalExpression condExpr)
            {
                throw new InvalidOperationException("Can not work on that node");
            }

            public void VisitContinueStatement(ContinueStatement continueStmt)
            {
                throw new InvalidOperationException("Can not work on that node");
            }

            public void VisitDestructuringPattern(DestructuringPattern destructuringPattern)
            {
                throw new InvalidOperationException("Can not work on that node");
            }

            public void VisitDoWhileStatement(DoWhileStatement doWhileStmt)
            {
                throw new InvalidOperationException("Can not work on that node");
            }

            public void VisitEmptyStatement(EmptyStatement emptyStmt)
            {
                throw new InvalidOperationException("Can not work on that node");
            }

            public void VisitExpressionPattern(ExpressionPattern exprPattern)
            {
                throw new InvalidOperationException("Can not work on that node");
            }

            public void VisitExpressionStatement(ExpressionStatement exprStmt)
            {
                throw new InvalidOperationException("Can not work on that node");
            }

            public void VisitExpressoTokenNode(ExpressoTokenNode tokenNode)
            {
                throw new InvalidOperationException("Can not work on that node");
            }

            public void VisitFieldDeclaration(FieldDeclaration fieldDecl)
            {
                throw new InvalidOperationException("Can not work on that node");
            }

            public void VisitFinallyClause(FinallyClause finallyClause)
            {
                throw new InvalidOperationException("Can not work on that node");
            }

            public void VisitForStatement(ForStatement forStmt)
            {
                throw new InvalidOperationException("Can not work on that node");
            }

            public void VisitFunctionDeclaration(FunctionDeclaration funcDecl)
            {
                throw new InvalidOperationException("Can not work on that node");
            }

            public void VisitFunctionType(FunctionType funcType)
            {
                throw new InvalidOperationException("Can not work on that node");
            }

            public void VisitIdentifier(Identifier ident)
            {
                throw new InvalidOperationException("Can not work on that node");
            }

            public void VisitIdentifierPattern(IdentifierPattern identifierPattern)
            {
                throw new InvalidOperationException("Can not work on that node");
            }

            public void VisitIfStatement(IfStatement ifStmt)
            {
                throw new InvalidOperationException("Can not work on that node");
            }

            public void VisitIgnoringRestPattern(IgnoringRestPattern restPattern)
            {
                throw new InvalidOperationException("Can not work on that node");
            }

            public void VisitImportDeclaration(ImportDeclaration importDecl)
            {
                throw new InvalidOperationException("Can not work on that node");
            }

            public void VisitIndexerExpression(IndexerExpression indexExpr)
            {
                throw new InvalidOperationException("Can not work on that node");
            }

            public void VisitIntegerSequenceExpression(IntegerSequenceExpression intSeq)
            {
                throw new InvalidOperationException("Can not work on that node");
            }

            public void VisitKeyValueLikeExpression(KeyValueLikeExpression keyValue)
            {
                throw new InvalidOperationException("Can not work on that node");
            }

            public void VisitKeyValuePattern(KeyValuePattern keyValuePattern)
            {
                throw new InvalidOperationException("Can not work on that node");
            }

            public void VisitLiteralExpression(LiteralExpression literal)
            {
                throw new InvalidOperationException("Can not work on that node");
            }

            public void VisitMatchClause(MatchPatternClause matchClause)
            {
                throw new InvalidOperationException("Can not work on that node");
            }

            public void VisitMatchStatement(MatchStatement matchStmt)
            {
                throw new InvalidOperationException("Can not work on that node");
            }

            public void VisitMemberReference(MemberReferenceExpression memRef)
            {
                throw new InvalidOperationException("Can not work on that node");
            }

            public void VisitMemberType(MemberType memberType)
            {
                throw new InvalidOperationException("Can not work on that node");
            }

            public void VisitNewLine(NewLineNode newlineNode)
            {
                throw new InvalidOperationException("Can not work on that node");
            }

            public void VisitNullNode(AstNode nullNode)
            {
                throw new InvalidOperationException("Can not work on that node");
            }

            public void VisitNullReferenceExpression(NullReferenceExpression nullRef)
            {
                nullRef.Parent.AcceptWalker(this);
            }

            public void VisitObjectCreationExpression(ObjectCreationExpression creation)
            {
                throw new InvalidOperationException("Can not work on that node");
            }

            public void VisitParameterDeclaration(ParameterDeclaration parameterDecl)
            {
                throw new InvalidOperationException("Can not work on that node");
            }

            public void VisitParameterType(ParameterType paramType)
            {
                throw new InvalidOperationException("Can not work on that node");
            }

            public void VisitParenthesizedExpression(ParenthesizedExpression parensExpr)
            {
                throw new InvalidOperationException("Can not work on that node");
            }

            public void VisitPathExpression(PathExpression pathExpr)
            {
                throw new InvalidOperationException("Can not work on that node");
            }

            public void VisitPatternPlaceholder(AstNode placeholder, Pattern child)
            {
                throw new InvalidOperationException("Can not work on that node");
            }

            public void VisitPatternWithType(PatternWithType pattern)
            {
                throw new InvalidOperationException("Can not work on that node");
            }

            public void VisitPlaceholderType(PlaceholderType placeholderType)
            {
                throw new InvalidOperationException("Can not work on that node");
            }

            public void VisitPrimitiveType(PrimitiveType primitiveType)
            {
                throw new InvalidOperationException("Can not work on that node");
            }

            public void VisitReferenceType(ReferenceType referenceType)
            {
                throw new InvalidOperationException("Can not work on that node");
            }

            public void VisitReturnStatement(ReturnStatement returnStmt)
            {
                throw new InvalidOperationException("Can not work on that node");
            }

            public void VisitSelfReferenceExpression(SelfReferenceExpression selfRef)
            {
                throw new InvalidOperationException("Can not work on that node");
            }

            public void VisitSequenceExpression(SequenceExpression seqExpr)
            {
                throw new InvalidOperationException("Can not work on that node");
            }

            public void VisitSequenceInitializer(SequenceInitializer seqInitializer)
            {
                throw new InvalidOperationException("Can not work on that node");
            }

            public void VisitSimpleType(SimpleType simpleType)
            {
                throw new InvalidOperationException("Can not work on that node");
            }

            public void VisitSuperReferenceExpression(SuperReferenceExpression superRef)
            {
                throw new InvalidOperationException("Can not work on that node");
            }

            public void VisitTextNode(TextNode textNode)
            {
                throw new InvalidOperationException("Can not work on that node");
            }

            public void VisitThrowStatement(ThrowStatement throwStmt)
            {
                throw new InvalidOperationException("Can not work on that node");
            }

            public void VisitTryStatement(TryStatement tryStmt)
            {
                throw new InvalidOperationException("Can not work on that node");
            }

            public void VisitTuplePattern(TuplePattern tuplePattern)
            {
                throw new InvalidOperationException("Can not work on that node");
            }

            public void VisitTypeDeclaration(TypeDeclaration typeDecl)
            {
                throw new InvalidOperationException("Can not work on that node");
            }

            public void VisitUnaryExpression(UnaryExpression unaryExpr)
            {
                throw new InvalidOperationException("Can not work on that node");
            }

            public void VisitValueBindingForStatement(ValueBindingForStatement valueBindingForStmt)
            {
                throw new InvalidOperationException("Can not work on that node");
            }

            public void VisitVariableDeclarationStatement(VariableDeclarationStatement varDecl)
            {
                throw new InvalidOperationException("Can not work on that node");
            }

            public void VisitVariableInitializer(VariableInitializer initializer)
            {
                throw new InvalidOperationException("Can not work on that node");
            }

            public void VisitWhileStatement(WhileStatement whileStmt)
            {
                throw new InvalidOperationException("Can not work on that node");
            }

            public void VisitWhitespace(WhitespaceNode whitespaceNode)
            {
                throw new InvalidOperationException("Can not work on that node");
            }

            public void VisitWildcardPattern(WildcardPattern wildcardPattern)
            {
                throw new InvalidOperationException("Can not work on that node");
            }

            public void VisitYieldStatement(YieldStatement yieldStmt)
            {
                throw new InvalidOperationException("Can not work on that node");
            }
        }
    }
}
