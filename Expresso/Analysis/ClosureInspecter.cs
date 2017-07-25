using System;
using System.Collections.Generic;
using ICSharpCode.NRefactory.PatternMatching;

namespace Expresso.Ast.Analysis
{
    partial class TypeChecker
    {
	    /// <summary>
	    /// This class is responsible for marking captured identifier nodes and replacing captured field access nodes with identifier nodes.
	    /// </summary>
	    public class ClosureInspecter : IAstWalker
	    {
            Parser parser;
	        TypeChecker checker;
            List<Identifier> LiftedIdentifiers;

            public ClosureInspecter(Parser parser, TypeChecker checker, ClosureLiteralExpression closure)
	        {
                this.parser = parser;
	            this.checker = checker;
                LiftedIdentifiers = new List<Identifier>();
                closure.LiftedIdentifiers = LiftedIdentifiers;
	        }

	        public void VisitAliasDeclaration(AliasDeclaration aliasDecl)
	        {
	            throw new InvalidOperationException("Can not work on that node");
	        }

	        public void VisitAssignment(AssignmentExpression assignment)
	        {
	            assignment.Left.AcceptWalker(this);
	            assignment.Right.AcceptWalker(this);
	        }

	        public void VisitAst(ExpressoAst ast)
	        {
	            throw new InvalidOperationException("Can not work on that node");
	        }

	        public void VisitBinaryExpression(BinaryExpression binaryExpr)
	        {
	            binaryExpr.Left.AcceptWalker(this);
	            binaryExpr.Right.AcceptWalker(this);
	        }

	        public void VisitBlock(BlockStatement block)
	        {
	            block.Statements.AcceptWalker(this);
	        }

	        public void VisitBreakStatement(BreakStatement breakStmt)
	        {
	        }

	        public void VisitCallExpression(CallExpression callExpr)
	        {
	            callExpr.Target.AcceptWalker(this);
	            callExpr.Arguments.AcceptWalker(this);
	        }

	        public void VisitCastExpression(CastExpression castExpr)
	        {
	            castExpr.Target.AcceptWalker(this);
	        }

	        public void VisitCatchClause(CatchClause catchClause)
	        {
                VisitBlock(catchClause.Body);
	        }

	        public void VisitClosureLiteralExpression(ClosureLiteralExpression closure)
	        {
                VisitBlock(closure.Body);
	        }

	        public void VisitCollectionPattern(CollectionPattern collectionPattern)
	        {
	            collectionPattern.Items.AcceptWalker(this);
	        }

	        public void VisitCommentNode(CommentNode comment)
	        {
	        }

	        public void VisitComprehensionExpression(ComprehensionExpression comp)
	        {
	            comp.Item.AcceptWalker(this);
	            comp.Body.AcceptWalker(this);
	        }

	        public void VisitComprehensionForClause(ComprehensionForClause compFor)
	        {
	            compFor.Left.AcceptWalker(this);
	            compFor.Target.AcceptWalker(this);
	            compFor.Body.AcceptWalker(this);
	        }

	        public void VisitComprehensionIfClause(ComprehensionIfClause compIf)
	        {
	            compIf.Condition.AcceptWalker(this);
	            compIf.Body.AcceptWalker(this);
	        }

	        public void VisitConditionalExpression(ConditionalExpression condExpr)
	        {
	            condExpr.Condition.AcceptWalker(this);
	            condExpr.TrueExpression.AcceptWalker(this);
	            condExpr.FalseExpression.AcceptWalker(this);
	        }

	        public void VisitContinueStatement(ContinueStatement continueStmt)
	        {
	        }

	        public void VisitDestructuringPattern(DestructuringPattern destructuringPattern)
	        {
	            destructuringPattern.Items.AcceptWalker(this);
	        }

	        public void VisitEmptyStatement(EmptyStatement emptyStmt)
	        {
	        }

	        public void VisitExpressionPattern(ExpressionPattern exprPattern)
	        {
	            exprPattern.Expression.AcceptWalker(this);
	        }

	        public void VisitExpressionStatement(ExpressionStatement exprStmt)
	        {
	            exprStmt.Expression.AcceptWalker(this);
	        }

	        public void VisitExpressoTokenNode(ExpressoTokenNode tokenNode)
	        {
	        }

	        public void VisitFieldDeclaration(FieldDeclaration fieldDecl)
	        {
	            throw new InvalidOperationException("Can not work on that node");
	        }

	        public void VisitForStatement(ForStatement forStmt)
	        {
	            forStmt.Left.AcceptWalker(this);
	            forStmt.Target.AcceptWalker(this);
                VisitBlock(forStmt.Body);
	        }

	        public void VisitFunctionDeclaration(FunctionDeclaration funcDecl)
	        {
	            throw new InvalidOperationException("Can not work on that node");
	        }

	        public void VisitFunctionType(FunctionType funcType)
	        {
	        }

	        public void VisitIdentifier(Identifier ident)
	        {
                var local_symbol = checker.symbols.GetSymbol(ident.Name);
                if(local_symbol == null){
                    var parameter = checker.symbols.GetSymbolInNScopesAbove(ident.Name, 1);
                    if(parameter == null){
	                    var symbol = checker.symbols.GetSymbolInAnyScope(ident.Name);
	                    if(symbol == null){
	                        parser.ReportSemanticError(
	                            "Error ES0100: '{0}' turns out not to be declared or accessible in the current scope {1}!",
	                            ident,
	                            ident.Name, checker.symbols.Name
	                        );
	                    }else{
	                        LiftedIdentifiers.Add(symbol);

                            var self_ref = Expression.MakeSelfRef(ident.StartLocation);
                            var mem_ref = Expression.MakeMemRef(self_ref, (Identifier)ident.Clone());
                            ident.ReplaceWith(mem_ref);
	                    }
                    }else{
                        var self_ref = Expression.MakeSelfRef(ident.StartLocation);
                        var mem_ref = Expression.MakeMemRef(self_ref, (Identifier)ident.Clone());
                        ident.ReplaceWith(mem_ref);
                    }
                }
	        }

	        public void VisitIdentifierPattern(IdentifierPattern identifierPattern)
	        {
                VisitIdentifier(identifierPattern.Identifier);
	        }

	        public void VisitIfStatement(IfStatement ifStmt)
	        {
                ifStmt.Condition.AcceptWalker(this);
                VisitBlock(ifStmt.TrueBlock);
                VisitBlock(ifStmt.FalseBlock);
	        }

	        public void VisitIgnoringRestPattern(IgnoringRestPattern restPattern)
	        {
	        }

	        public void VisitImportDeclaration(ImportDeclaration importDecl)
	        {
                throw new InvalidOperationException("Can not work on that node");
	        }

	        public void VisitIndexerExpression (IndexerExpression indexExpr)
	        {
	            throw new NotImplementedException ();
	        }

	        public void VisitIntegerSequenceExpression(IntegerSequenceExpression intSeq)
	        {
                intSeq.Lower.AcceptWalker(this);
                intSeq.Upper.AcceptWalker(this);
                intSeq.Step.AcceptWalker(this);
	        }

	        public void VisitKeyValueLikeExpression(KeyValueLikeExpression keyValue)
	        {
                keyValue.KeyExpression.AcceptWalker(this);
                keyValue.ValueExpression.AcceptWalker(this);
	        }

	        public void VisitLiteralExpression(LiteralExpression literal)
	        {
	        }

	        public void VisitMatchClause(MatchPatternClause matchClause)
	        {
                matchClause.Patterns.AcceptWalker(this);
                matchClause.Body.AcceptWalker(this);
	        }

	        public void VisitMatchStatement(MatchStatement matchStmt)
	        {
                matchStmt.Target.AcceptWalker(this);
                matchStmt.Clauses.AcceptWalker(this);
	        }

	        public void VisitMemberReference(MemberReferenceExpression memRef)
	        {
                memRef.Target.AcceptWalker(this);
                VisitIdentifier(memRef.Member);
	        }

	        public void VisitMemberType(MemberType memberType)
	        {
	        }

	        public void VisitNewExpression(NewExpression newExpr)
	        {
                newExpr.CreationExpression.AcceptWalker(this);
	        }

	        public void VisitNewLine(NewLineNode newlineNode)
	        {
	        }

	        public void VisitNullNode(AstNode nullNode)
	        {
	        }

	        public void VisitObjectCreationExpression(ObjectCreationExpression creation)
	        {
                creation.Items.AcceptWalker(this);
	        }

	        public void VisitParameterDeclaration(ParameterDeclaration parameterDecl)
	        {
                throw new InvalidOperationException("Can not work on that node");
	        }

	        public void VisitParameterType(ParameterType paramType)
	        {
	        }

	        public void VisitParenthesizedExpression(ParenthesizedExpression parensExpr)
	        {
                parensExpr.Expression.AcceptWalker(this);
	        }

	        public void VisitPathExpression(PathExpression pathExpr)
	        {
                pathExpr.Items.AcceptWalker(this);
	        }

	        public void VisitPatternPlaceholder(AstNode placeholder, Pattern child)
	        {
	        }

	        public void VisitPlaceholderType(PlaceholderType placeholderType)
	        {
	        }

	        public void VisitPrimitiveType(PrimitiveType primitiveType)
	        {
	        }

	        public void VisitReferenceType(ReferenceType referenceType)
	        {
	        }

	        public void VisitReturnStatement(ReturnStatement returnStmt)
	        {
                returnStmt.Expression.AcceptWalker(this);
	        }

	        public void VisitSelfReferenceExpression(SelfReferenceExpression selfRef)
	        {
	        }

	        public void VisitSequenceExpression(SequenceExpression seqExpr)
	        {
                seqExpr.Items.AcceptWalker(this);
	        }

	        public void VisitSequenceInitializer(SequenceInitializer seqInitializer)
	        {
                seqInitializer.Items.AcceptWalker(this);
	        }

	        public void VisitSimpleType(SimpleType simpleType)
	        {
	        }

	        public void VisitSuperReferenceExpression(SuperReferenceExpression superRef)
	        {
	        }

	        public void VisitTextNode(TextNode textNode)
	        {
	        }

	        public void VisitThrowStatement(ThrowStatement throwStmt)
	        {
                VisitObjectCreationExpression(throwStmt.CreationExpression);
	        }

	        public void VisitTryStatement(TryStatement tryStmt)
	        {
                VisitBlock(tryStmt.EnclosingBlock);
                tryStmt.CatchClauses.AcceptWalker(this);
	        }

	        public void VisitTuplePattern(TuplePattern tuplePattern)
	        {
                tuplePattern.Patterns.AcceptWalker(this);
	        }

	        public void VisitTypeDeclaration(TypeDeclaration typeDecl)
	        {
                throw new InvalidOperationException("Can not work on that node");
	        }

	        public void VisitUnaryExpression(UnaryExpression unaryExpr)
	        {
                unaryExpr.Operand.AcceptWalker(this);
	        }

	        public void VisitValueBindingForStatement(ValueBindingForStatement valueBindingForStmt)
	        {
                valueBindingForStmt.Variables.AcceptWalker(this);
                VisitBlock(valueBindingForStmt.Body);
	        }

	        public void VisitValueBindingPattern(ValueBindingPattern valueBindingPattern)
	        {
                valueBindingPattern.Variables.AcceptWalker(this);
	        }

	        public void VisitVariableDeclarationStatement(VariableDeclarationStatement varDecl)
	        {
                varDecl.Variables.AcceptWalker(this);
	        }

	        public void VisitVariableInitializer(VariableInitializer initializer)
	        {
                initializer.Initializer.AcceptWalker(this);
	        }

	        public void VisitWhileStatement(WhileStatement whileStmt)
	        {
                whileStmt.Condition.AcceptWalker(this);
                VisitBlock(whileStmt.Body);
	        }

	        public void VisitWhitespace(WhitespaceNode whitespaceNode)
	        {
	        }

	        public void VisitWildcardPattern(WildcardPattern wildcardPattern)
	        {
	        }

	        public void VisitYieldStatement(YieldStatement yieldStmt)
	        {
                yieldStmt.Expression.AcceptWalker(this);
	        }
	    }
    }
}
