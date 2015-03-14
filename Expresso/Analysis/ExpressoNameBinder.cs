using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;


/*
 * The name binding and type resolving:
 *
 * The name binding happens in 2 passes.
 * In the first pass (full recursive walk of the AST) we resolve locals and inference types,
 * meaning when reaching a variable whose type is expressed as inference type, it will try to
 * infer the type of that variable.
 * The second pass uses the "processed" list of all context statements (functions and class
 * bodies) and has each context statement resolve its free variables to determine whether
 * they are globals or references to lexically enclosing scopes.
 *
 * The second pass happens in post-order (the context statement is added into the "processed"
 * list after processing its nested functions/statements). This way, when the function is
 * processing its free variables, it also knows already which of its locals are being lifted
 * to the closure and can report error if such closure variable is being deleted.
 * 
 * The following list names constructs that will introduce new variables into the enclosing scope:
 *    TypeDeclaration,
 *    FunctionDeclaration,
 *    VariableDeclarationStatement,
 *    ImportDeclaration,
 *    Comprehension(ComprehensionForClause expression)
 */

namespace Expresso.Ast.Analysis
{
    /// <summary>
    /// The name binding and name resolving:
    /// During name binding, we first define names 
    /// </summary>
    class ExpressoNameBinder : IAstWalker
	{
        static uint id = 1;
        int scope_counter;
        SymbolTable symbol_table;
		Parser parser;
		
		ExpressoNameBinder(Parser parentParser)
		{
			parser = parentParser;
            symbol_table = parentParser.Symbols;
		}
		
		#region Public surface
        /// <summary>
        /// The public surface of post-parse processing.
        /// In this method, we are binding names, checking type validity and doing some flow analisys.
        /// Note that we are NOT doing any AST-wide optimizations here.
        /// </summary>
		internal static void BindAst(ExpressoAst ast, Parser parser)
		{
			ExpressoNameBinder binder = new ExpressoNameBinder(parser);
			binder.Bind(ast);
            #if DEBUG
            Console.WriteLine("We have given ids on total of {0} identifiers.", id - 1);
            #endif
		}
		#endregion
		
		void Bind(ExpressoAst unboundAst)
		{
            Debug.Assert(unboundAst != null);
			
			// Find all scopes and variables
            unboundAst.AcceptWalker(this);
			
            TypeChecker.Check(unboundAst, parser);
			// Run flow checker
            //FlowChecker.Check(unboundAst);
		}
		
        void DecendScope()
		{
            symbol_table = symbol_table.Children[scope_counter];
		}
		
		void AscendScope()
		{
            symbol_table = symbol_table.Parent;
		}
		
		#region IAstWalker implementation

        public void VisitAst(ExpressoAst ast)
        {
            Debug.Assert(symbol_table.Parent == null);
            scope_counter = 0;
            ast.Imports.AcceptWalker(this);
            ast.Declarations.AcceptWalker(this);
            Debug.Assert(symbol_table.Parent == null);
        }

        public void VisitBlock(BlockStatement block)
        {
            int tmp_counter = scope_counter;
            DecendScope();
            scope_counter = 0;
            block.Statements.AcceptWalker(this);
            AscendScope();
            scope_counter = tmp_counter + 1;
        }

        public void VisitBreakStatement(BreakStatement breakStmt)
        {
            //no op
        }

        public void VisitContinueStatement(ContinueStatement continueStmt)
        {
            //no op
        }

        public void VisitEmptyStatement(EmptyStatement emptyStmt)
        {
            //no op
        }

        public void VisitForStatement(ForStatement forStmt)
        {
            int tmp_counter = scope_counter;
            DecendScope();
            scope_counter = 0;
            forStmt.Left.AcceptWalker(this);
            forStmt.Target.AcceptWalker(this);
            forStmt.Body.AcceptWalker(this);
            AscendScope();
            scope_counter = tmp_counter + 1;
        }

        public void VisitIfStatement(IfStatement ifStmt)
        {
            int tmp_counter = scope_counter;
            DecendScope();
            scope_counter = 0;
            ifStmt.Condition.AcceptWalker(this);
            ifStmt.TrueBlock.AcceptWalker(this);
            ifStmt.FalseBlock.AcceptWalker(this);
            AscendScope();
            scope_counter = tmp_counter + 1;
        }

        public void VisitReturnStatement(ReturnStatement returnStmt)
        {
            returnStmt.Expression.AcceptWalker(this);
        }

        public void VisitMatchStatement(MatchStatement matchStmt)
        {
            int tmp_counter = scope_counter;
            DecendScope();
            scope_counter = 0;
            matchStmt.Target.AcceptWalker(this);
            matchStmt.Clauses.AcceptWalker(this);
            AscendScope();
            scope_counter = tmp_counter + 1;
        }

        public void VisitWhileStatement(WhileStatement whileStmt)
        {
            int tmp_counter = scope_counter;
            DecendScope();
            scope_counter = 0;
            whileStmt.Condition.AcceptWalker(this);
            whileStmt.Body.AcceptWalker(this);
            AscendScope();
            scope_counter = tmp_counter + 1;
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
            assignment.Left.AcceptWalker(this);
            assignment.Right.AcceptWalker(this);
        }

        public void VisitBinaryExpression(BinaryExpression binaryExpr)
        {
            binaryExpr.Left.AcceptWalker(this);
            binaryExpr.Right.AcceptWalker(this);
        }

        public void VisitCallExpression(CallExpression callExpr)
        {
            callExpr.Target.AcceptWalker(this);
            callExpr.Arguments.AcceptWalker(this);
        }

        public void VisitCastExpression(CastExpression castExpr)
        {
            castExpr.Target.AcceptWalker(this);
            castExpr.ToExpression.AcceptWalker(this);
        }

        public void VisitComprehensionExpression(ComprehensionExpression comp)
        {
            // Do not store scope counter because comprehensions contains only expressions.
            DecendScope();
            comp.Item.AcceptWalker(this);
            comp.Body.AcceptWalker(this);
            AscendScope();
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

        public void VisitLiteralExpression(LiteralExpression literal)
        {
            //no op
        }

        public void VisitIdentifier(Identifier ident)
        {
            BindName(ident);
        }

        public void VisitIntgerSequenceExpression(IntegerSequenceExpression intSeq)
        {
            intSeq.Lower.AcceptWalker(this);
            intSeq.Upper.AcceptWalker(this);
            intSeq.Step.AcceptWalker(this);
        }

        public void VisitIndexerExpression(IndexerExpression indexExpr)
        {
            indexExpr.Target.AcceptWalker(this);
            indexExpr.Arguments.AcceptWalker(this);
        }

        public void VisitMemberReference(MemberReference memRef)
        {
            memRef.Target.AcceptWalker(this);
            memRef.Subscription.AcceptWalker(this);
        }

        public void VisitNewExpression(NewExpression newExpr)
        {
            newExpr.CreationExpression.AcceptWalker(this);
        }

        public void VisitParenthesizedExpression(ParenthesizedExpression parensExpr)
        {
            parensExpr.Expression.AcceptWalker(this);
        }

        public void VisitSequenceInitializer(SequenceInitializer seqInitializer)
        {
            seqInitializer.Items.AcceptWalker(this);
        }

        public void VisitMatchClause(MatchPatternClause matchClause)
        {
            matchClause.Patterns.AcceptWalker(this);
            matchClause.Body.AcceptWalker(this);
        }

        public void VisitSequence(SequenceExpression seqExpr)
        {
            seqExpr.Items.AcceptWalker(this);
        }

        public void VisitUnaryExpression(UnaryExpression unaryExpr)
        {
            unaryExpr.Operand.AcceptWalker(this);
        }

        public void VisitSelfReferenceExpression(SelfReferenceExpression selfRef)
        {
            BindName(selfRef.SelfIdentifier);
        }

        public void VisitSuperReferenceExpression(SuperReferenceExpression superRef)
        {
            BindName(superRef.SuperIdentifier);
        }

        public void VisitCommentNode(CommentNode comment)
        {
            // no op
        }

        public void VisitTextNode(TextNode textNode)
        {
            // no op
        }

        public void VisitSimpleType(SimpleType simpleType)
        {
            BindTypeName(simpleType.IdentifierToken);
            simpleType.TypeArguments.AcceptWalker(this);
        }

        public void VisitPrimitiveType(PrimitiveType primitiveType)
        {
            // no op
        }

        public void VisitReferenceType(ReferenceType referenceType)
        {
            referenceType.BaseType.AcceptWalker(this);
        }

        public void VisitMemberType(MemberType memberType)
        {
            memberType.Target.AcceptWalker(this);
            BindTypeName(memberType.MemberNameToken);
            memberType.TypeArguments.AcceptWalker(this);
        }

        public void VisitPlaceholderType(PlaceholderType placeholderType)
        {
            // no op
        }

        public void VisitAliasDeclaration(AliasDeclaration aliasDecl)
        {
            // An alias name is another name for an item
            // so define a new name first and then resolve the name.
            DefineNewId(aliasDecl.AliasToken);
            /*var table = symbol_table;
            while(table != null){
                var symbol = table.GetSymbol(aliasDecl);
            }*/
        }

        public void VisitFunctionDeclaration(FunctionDeclaration funcDecl)
        {
            DefineNewId(funcDecl.NameToken);
            funcDecl.Parameters.AcceptWalker(this);
            funcDecl.Body.AcceptWalker(this);
        }

        public void VisitFieldDeclaration(FieldDeclaration fieldDecl)
        {
            foreach(var field in fieldDecl.Initializers){
                DefineNewId(field.NameToken);
                field.Initializer.AcceptWalker(this);
            }
        }

        public void VisitParameterDeclaration(ParameterDeclaration parameterDecl)
        {
            DefineNewId(parameterDecl.NameToken);
        }

        public void VisitVariableInitializer(VariableInitializer initializer)
        {
            DefineNewId(initializer.NameToken);
            initializer.Initializer.AcceptWalker(this);
        }

        public void VisitNullNode(AstNode nullNode)
        {
            // no op
        }

        public void VisitNewLine(NewLineNode newlineNode)
        {
            // no op
        }

        public void VisitWhitespace(WhitespaceNode whitespaceNode)
        {
            // no op
        }

        public void VisitExpressoTokenNode(ExpressoTokenNode tokenNode)
        {
            // no op
        }

        public void VisitPatternPlaceholder(AstNode placeholder, ICSharpCode.NRefactory.PatternMatching.Pattern child)
        {
            // no op
        }

        public void VisitExpressionStatement(ExpressionStatement exprStmt)
        {
            exprStmt.Expression.AcceptWalker(this);
        }

        public void VisitKeyValueLikeExpression(KeyValueLikeExpression keyValue)
        {
            keyValue.Value.AcceptWalker(this);
        }

        public void VisitPathExpression(PathExpression pathExpr)
        {
            foreach(var ident in pathExpr.Items){
                ident.AcceptWalker(this);
            }
        }

        public void VisitObjectCreationExpression(ObjectCreationExpression creation)
        {
            creation.TypePath.AcceptWalker(this);
            creation.Items.AcceptWalker(this);
        }

        public void VisitImportDeclaration(ImportDeclaration importDecl)
        {
            // Here's the good place to import names from other files
            importDecl.ModuleNameToken.AcceptWalker(this);
            DefineNewId(importDecl.AliasNameToken);
        }

        public void VisitTypeDeclaration(TypeDeclaration typeDecl)
        {
            DefineNewId(typeDecl.NameToken);
            typeDecl.BaseTypes.AcceptWalker(this);

            int tmp_counter = scope_counter;
            scope_counter = 0;
            DecendScope();
            typeDecl.Members.AcceptWalker(this);
            AscendScope();
            scope_counter = tmp_counter + 1;
        }

        public void VisitWildcardPattern(WildcardPattern wildcardPattern)
        {
            // no op
        }

        public void VisitIdentifierPattern(IdentifierPattern identifierPattern)
        {
            DefineNewId(identifierPattern.Identifier);
            identifierPattern.InnerPattern.AcceptWalker(this);
        }

        public void VisitValueBindingPattern(ValueBindingPattern valueBindingPattern)
        {
            valueBindingPattern.Pattern.AcceptWalker(this);
        }

        public void VisitCollectionPattern(CollectionPattern collectionPattern)
        {
            collectionPattern.Items.AcceptWalker(this);
        }

        public void VisitDestructuringPattern(DestructuringPattern destructuringPattern)
        {
            destructuringPattern.Items.AcceptWalker(this);
        }

        public void VisitTuplePattern(TuplePattern tuplePattern)
        {
            tuplePattern.Patterns.AcceptWalker(this);
        }

        public void VisitExpressionPattern(ExpressionPattern exprPattern)
        {
            exprPattern.Expression.AcceptWalker(this);
        }
		#endregion

        void DefineNewId(Identifier ident)
        {
            ident.IdentifierId = id++;
        }

        void BindName(Identifier ident)
        {
            var table = symbol_table;
            while(table != null){
                var referenced = table.GetSymbol(ident.Name);
                if(referenced != null){
                    ident.IdentifierId = referenced.IdentifierId;
                    break;
                }

                table = table.Parent;
            }

            if(ident.IdentifierId == 0){
                parser.ReportSemanticError("{0} turns out not to be declared or accessible in the current scope {1}!",
                    ident, ident.Name, symbol_table.Name
                );
            }
        }

        void BindTypeName(Identifier ident)
        {
            var table = symbol_table;
            while(table != null){
                var referenced = table.GetTypeSymbol(ident.Name);
                if(referenced != null){
                    ident.IdentifierId = referenced.IdentifierId;
                    break;
                }

                table = table.Parent;
            }

            if(ident.IdentifierId == 0){
                parser.ReportSemanticError("Type name `{0}` turns out not to be declared in the current scope {1}!",
                    ident, ident.Name, symbol_table.Name
                );
            }
        }
	}
}

