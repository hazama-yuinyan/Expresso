using System;
using System.Collections.Generic;
using ICSharpCode.NRefactory.PatternMatching;

namespace Expresso.Ast.Analysis
{
    partial class TypeChecker
    {
	    /// <summary>
	    /// This class is responsible for marking captured identifier nodes.
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

            public void VisitAttributeSection(AttributeSection section)
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
                int tmp_counter = checker.scope_counter;
                checker.DescendScope();
                checker.scope_counter = 0;

	            block.Statements.AcceptWalker(this);

                checker.AscendScope();
                checker.scope_counter = tmp_counter;
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
                // Don't descend scope because type checker already do that
                //int tmp_counter = checker.scope_counter;
                //checker.DescendScope();
                //checker.scope_counter = 0;

                VisitBlock(closure.Body);

                //checker.AscendScope();
                //checker.scope_counter = tmp_counter;
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

            public void VisitDoWhileStatement(DoWhileStatement doWhileStmt)
            {
                VisitWhileStatement(doWhileStmt.Delegator);
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

            public void VisitFinallyClause(FinallyClause finallyClause)
            {
                VisitBlock(finallyClause.Body);
            }

	        public void VisitForStatement(ForStatement forStmt)
	        {
                int tmp_counter = checker.scope_counter;
                checker.DescendScope();
                checker.scope_counter = 0;

	            forStmt.Left.AcceptWalker(this);
	            forStmt.Target.AcceptWalker(this);
                VisitBlock(forStmt.Body);

                checker.AscendScope();
                checker.scope_counter = tmp_counter;
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
                var table = AscendScopesUntil("closure");
                var parameter = table.GetSymbolInNScopesAbove(ident.Name, 1);
                if(parameter == null){
                    var symbol = table.GetSymbolInAnyScopeWithoutNative(ident.Name, out var natives_searched);
                    if(symbol == null){
                        if(!natives_searched){
	                        parser.ReportSemanticError(
	                            "'{0}' turns out not to be declared or accessible in the current scope {1}!",
                                "ES0100",
	                            ident,
	                            ident.Name, checker.symbols.Name
	                        );
                        }
                    }else{
                        LiftedIdentifiers.Add(symbol);
                    }
                }
	        }

	        public void VisitIdentifierPattern(IdentifierPattern identifierPattern)
	        {
                VisitIdentifier(identifierPattern.Identifier);
	        }

	        public void VisitIfStatement(IfStatement ifStmt)
	        {
                int tmp_counter = checker.scope_counter;
                checker.DescendScope();
                checker.scope_counter = 0;

                ifStmt.Condition.AcceptWalker(this);
                VisitBlock(ifStmt.TrueBlock);
                ifStmt.FalseStatement.AcceptWalker(this);

                checker.AscendScope();
                checker.scope_counter = tmp_counter;
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
                intSeq.Start.AcceptWalker(this);
                intSeq.End.AcceptWalker(this);
                intSeq.Step.AcceptWalker(this);
	        }

	        public void VisitKeyValueLikeExpression(KeyValueLikeExpression keyValue)
	        {
                keyValue.KeyExpression.AcceptWalker(this);
                keyValue.ValueExpression.AcceptWalker(this);
	        }

            public void VisitKeyValuePattern(KeyValuePattern keyValuePattern)
            {
                keyValuePattern.Value.AcceptWalker(this);
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
                int tmp_counter = checker.scope_counter;
                checker.DescendScope();
                checker.scope_counter = 0;

                matchStmt.Target.AcceptWalker(this);
                matchStmt.Clauses.AcceptWalker(this);

                checker.AscendScope();
                checker.scope_counter = tmp_counter;
	        }

	        public void VisitMemberReference(MemberReferenceExpression memRef)
	        {
                memRef.Target.AcceptWalker(this);
                VisitIdentifier(memRef.Member);
	        }

	        public void VisitMemberType(MemberType memberType)
	        {
	        }

	        public void VisitNewLine(NewLineNode newlineNode)
	        {
	        }

	        public void VisitNullNode(AstNode nullNode)
	        {
	        }

            public void VisitNullReferenceExpression(NullReferenceExpression nullRef)
            {
            }

	        public void VisitObjectCreationExpression(ObjectCreationExpression creation)
	        {
                creation.Items.AcceptWalker(this);
	        }

            public void VisitPatternWithType(PatternWithType pattern)
            {
                pattern.Pattern.AcceptWalker(this);
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
                int tmp_counter = checker.scope_counter;
                checker.DescendScope();
                checker.scope_counter = 0;

                valueBindingForStmt.Initializer.AcceptWalker(this);
                VisitBlock(valueBindingForStmt.Body);

                checker.AscendScope();
                checker.scope_counter = tmp_counter;
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
                int tmp_counter = checker.scope_counter;
                checker.DescendScope();
                checker.scope_counter = 0;

                whileStmt.Condition.AcceptWalker(this);
                VisitBlock(whileStmt.Body);

                checker.AscendScope();
                checker.scope_counter = tmp_counter;
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

            SymbolTable AscendScopesUntil(string parentName)
            {
                var table = checker.symbols;
                while(!table.Parent.Name.StartsWith(parentName, StringComparison.CurrentCulture)){
                    table = table.Parent;
                    if(table.Parent == null)
                        throw new Exception("table.Parent is null");
                }
                return table;
            }
	    }
    }
}
