using System;
using ICSharpCode.NRefactory.PatternMatching;
using Expresso.Ast;

namespace Expresso.CodeGen
{
    using CSharpExpr = System.Linq.Expressions.Expression;

    public partial class CSharpEmitter : IAstWalker<CSharpEmitterContext, CSharpExpr>
    {
        /// <summary>
        /// This class is responsible for defining inner variables before inspecting the body statements of a match clause.
        /// </summary>
        public class MatchClauseIdentifierDefiner : IAstWalker
	    {
            public void VisitAliasDeclaration(AliasDeclaration aliasDecl)
            {
                throw new NotImplementedException("Can not work on that node");
            }

            public void VisitAssignment(AssignmentExpression assignment)
            {
                throw new NotImplementedException("Can not work on that node");
            }

            public void VisitAst(ExpressoAst ast)
            {
                throw new NotImplementedException("Can not work on that node");
            }

            public void VisitBinaryExpression(BinaryExpression binaryExpr)
            {
                throw new NotImplementedException("Can not work on that node");
            }

            public void VisitBlock(BlockStatement block)
            {
                throw new NotImplementedException("Can not work on that node");
            }

            public void VisitBreakStatement(BreakStatement breakStmt)
            {
                throw new NotImplementedException("Can not work on that node");
            }

            public void VisitCallExpression(CallExpression callExpr)
            {
                throw new NotImplementedException("Can not work on that node");
            }

            public void VisitCastExpression(CastExpression castExpr)
            {
                throw new NotImplementedException("Can not work on that node");
            }

            public void VisitCatchClause(CatchClause catchClause)
            {
                throw new NotImplementedException("Can not work on that node");
            }

            public void VisitClosureLiteralExpression(ClosureLiteralExpression closure)
            {
                throw new NotImplementedException("Can not work on that node");
            }

            public void VisitCollectionPattern(CollectionPattern collectionPattern)
            {
                collectionPattern.Items.AcceptWalker(this);
            }

            public void VisitCommentNode(CommentNode comment)
            {
                throw new NotImplementedException("Can not work on that node");
            }

            public void VisitComprehensionExpression(ComprehensionExpression comp)
            {
                throw new NotImplementedException("Can not work on that node");
            }

            public void VisitComprehensionForClause(ComprehensionForClause compFor)
            {
                throw new NotImplementedException("Can not work on that node");
            }

            public void VisitComprehensionIfClause(ComprehensionIfClause compIf)
            {
                throw new NotImplementedException("Can not work on that node");
            }

            public void VisitConditionalExpression(ConditionalExpression condExpr)
            {
                throw new NotImplementedException("Can not work on that node");
            }

            public void VisitContinueStatement(ContinueStatement continueStmt)
            {
                throw new NotImplementedException("Can not work on that node");
            }

            public void VisitDestructuringPattern(DestructuringPattern destructuringPattern)
            {
                destructuringPattern.Items.AcceptWalker(this);
            }

            public void VisitEmptyStatement(EmptyStatement emptyStmt)
            {
                throw new NotImplementedException("Can not work on that node");
            }

            public void VisitExpressionPattern(ExpressionPattern exprPattern)
            {
            }

            public void VisitExpressionStatement(ExpressionStatement exprStmt)
            {
                throw new NotImplementedException("Can not work on that node");
            }

            public void VisitExpressoTokenNode(ExpressoTokenNode tokenNode)
            {
                throw new NotImplementedException("Can not work on that node");
            }

            public void VisitFieldDeclaration(FieldDeclaration fieldDecl)
            {
                throw new NotImplementedException("Can not work on that node");
            }

            public void VisitFinallyClause(FinallyClause finallyClause)
            {
                throw new NotImplementedException("Can not work on that node");
            }

            public void VisitForStatement(ForStatement forStmt)
            {
                throw new NotImplementedException("Can not work on that node");
            }

            public void VisitFunctionDeclaration(FunctionDeclaration funcDecl)
            {
                throw new NotImplementedException("Can not work on that node");
            }

            public void VisitFunctionType(FunctionType funcType)
            {
                throw new NotImplementedException("Can not work on that node");
            }

            public void VisitIdentifier(Identifier ident)
            {
                throw new NotImplementedException("Can not work on that node");
            }

            public void VisitIdentifierPattern(IdentifierPattern identifierPattern)
            {
                var native_type = CSharpCompilerHelper.GetNativeType(identifierPattern.Identifier.Type);
                var native_param = CSharpExpr.Parameter(native_type, identifierPattern.Identifier.Name);
                CSharpEmitter.Symbols.Add(identifierPattern.Identifier.IdentifierId, new ExpressoSymbol{Parameter = native_param});
            }

            public void VisitIfStatement(IfStatement ifStmt)
            {
                throw new NotImplementedException("Can not work on that node");
            }

            public void VisitIgnoringRestPattern(IgnoringRestPattern restPattern)
            {
            }

            public void VisitImportDeclaration(ImportDeclaration importDecl)
            {
                throw new NotImplementedException("Can not work on that node");
            }

            public void VisitIndexerExpression(IndexerExpression indexExpr)
            {
                throw new NotImplementedException("Can not work on that node");
            }

            public void VisitIntegerSequenceExpression(IntegerSequenceExpression intSeq)
            {
                throw new NotImplementedException("Can not work on that node");
            }

            public void VisitKeyValueLikeExpression(KeyValueLikeExpression keyValue)
            {
                throw new NotImplementedException("Can not work on that node");
            }

            public void VisitKeyValuePattern(KeyValuePattern keyValuePattern)
            {
                keyValuePattern.Value.AcceptWalker(this);
            }

            public void VisitLiteralExpression(LiteralExpression literal)
            {
                throw new NotImplementedException("Can not work on that node");
            }

            public void VisitMatchClause(MatchPatternClause matchClause)
            {
                matchClause.Patterns.AcceptWalker(this);
            }

            public void VisitMatchStatement(MatchStatement matchStmt)
            {
                throw new NotImplementedException("Can not work on that node");
            }

            public void VisitMemberReference(MemberReferenceExpression memRef)
            {
                throw new NotImplementedException("Can not work on that node");
            }

            public void VisitMemberType(MemberType memberType)
            {
                throw new NotImplementedException("Can not work on that node");
            }

            public void VisitNewExpression(NewExpression newExpr)
            {
                throw new NotImplementedException("Can not work on that node");
            }

            public void VisitNewLine(NewLineNode newlineNode)
            {
                throw new NotImplementedException("Can not work on that node");
            }

            public void VisitNullNode(AstNode nullNode)
            {
                throw new NotImplementedException("Can not work on that node");
            }

            public void VisitObjectCreationExpression(ObjectCreationExpression creation)
            {
                throw new NotImplementedException("Can not work on that node");
            }

            public void VisitParameterDeclaration(ParameterDeclaration parameterDecl)
            {
                throw new NotImplementedException("Can not work on that node");
            }

            public void VisitParameterType(ParameterType paramType)
            {
                throw new NotImplementedException("Can not work on that node");
            }

            public void VisitParenthesizedExpression(ParenthesizedExpression parensExpr)
            {
                throw new NotImplementedException("Can not work on that node");
            }

            public void VisitPathExpression(PathExpression pathExpr)
            {
                throw new NotImplementedException("Can not work on that node");
            }

            public void VisitPatternPlaceholder(AstNode placeholder, Pattern child)
            {
                throw new NotImplementedException("Can not work on that node");
            }

            public void VisitPlaceholderType(PlaceholderType placeholderType)
            {
                throw new NotImplementedException("Can not work on that node");
            }

            public void VisitPrimitiveType(PrimitiveType primitiveType)
            {
                throw new NotImplementedException("Can not work on that node");
            }

            public void VisitReferenceType(ReferenceType referenceType)
            {
                throw new NotImplementedException("Can not work on that node");
            }

            public void VisitReturnStatement(ReturnStatement returnStmt)
            {
                throw new NotImplementedException("Can not work on that node");
            }

            public void VisitSelfReferenceExpression(SelfReferenceExpression selfRef)
            {
                throw new NotImplementedException("Can not work on that node");
            }

            public void VisitSequenceExpression(SequenceExpression seqExpr)
            {
                throw new NotImplementedException("Can not work on that node");
            }

            public void VisitSequenceInitializer(SequenceInitializer seqInitializer)
            {
                throw new NotImplementedException("Can not work on that node");
            }

            public void VisitSimpleType(SimpleType simpleType)
            {
                throw new NotImplementedException("Can not work on that node");
            }

            public void VisitSuperReferenceExpression(SuperReferenceExpression superRef)
            {
                throw new NotImplementedException("Can not work on that node");
            }

            public void VisitTextNode(TextNode textNode)
            {
                throw new NotImplementedException("Can not work on that node");
            }

            public void VisitThrowStatement(ThrowStatement throwStmt)
            {
                throw new NotImplementedException("Can not work on that node");
            }

            public void VisitTryStatement(TryStatement tryStmt)
            {
                throw new NotImplementedException("Can not work on that node");
            }

            public void VisitTuplePattern(TuplePattern tuplePattern)
            {
                tuplePattern.Patterns.AcceptWalker(this);
            }

            public void VisitTypeDeclaration(TypeDeclaration typeDecl)
            {
                throw new NotImplementedException("Can not work on that node");
            }

            public void VisitUnaryExpression(UnaryExpression unaryExpr)
            {
                throw new NotImplementedException("Can not work on that node");
            }

            public void VisitValueBindingForStatement(ValueBindingForStatement valueBindingForStmt)
            {
                throw new NotImplementedException("Can not work on that node");
            }

            public void VisitValueBindingPattern(ValueBindingPattern valueBindingPattern)
            {
                valueBindingPattern.Variables.AcceptWalker(this);
            }

            public void VisitVariableDeclarationStatement(VariableDeclarationStatement varDecl)
            {
                throw new NotImplementedException("Can not work on that node");
            }

            public void VisitVariableInitializer(VariableInitializer initializer)
            {
                var native_type = CSharpCompilerHelper.GetNativeType(initializer.NameToken.Type);
                var native_param = CSharpExpr.Parameter(native_type, initializer.Name);
                CSharpEmitter.Symbols.Add(initializer.NameToken.IdentifierId, new ExpressoSymbol{Parameter = native_param});
            }

            public void VisitWhileStatement(WhileStatement whileStmt)
            {
                throw new NotImplementedException("Can not work on that node");
            }

            public void VisitWhitespace(WhitespaceNode whitespaceNode)
            {
                throw new NotImplementedException("Can not work on that node");
            }

            public void VisitWildcardPattern(WildcardPattern wildcardPattern)
            {
            }

            public void VisitYieldStatement(YieldStatement yieldStmt)
            {
                throw new NotImplementedException("Can not work on that node");
            }
        }
    }
}
