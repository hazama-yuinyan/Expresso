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
using Expresso.TypeSystem;
using Expresso.CodeGen;

namespace Expresso.Ast.Analysis
{
    /// <summary>
    /// The name binding and name resolving:
    /// During name binding, we first define names on variables, fields, methods and types.
    /// And then bind names on those items except for types.
    /// For types we'll bind them during type validity check.
    /// By name binding, I mean that we give the same id defined on the name definition phase.
    /// That means that it is not guaranteed that the type of the target object is resolved.
    /// </summary>
    class ExpressoNameBinder : IAstWalker
	{
        int scope_counter;
        SymbolTable symbol_table;
		Parser parser;
		
		ExpressoNameBinder(Parser parentParser)
		{
			parser = parentParser;
            symbol_table = parentParser.Symbols;

            foreach(var pair in ExpressoSymbol.Identifiers){
                if(pair.Value.IdentifierId == 0)
                    UniqueIdGenerator.DefineNewId(pair.Value);
            }
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
            Console.WriteLine("We have given ids on total of {0} identifiers.", UniqueIdGenerator.CurrentId - 1);
            #endif
		}

        internal static void NameBindAst(Parser parser)
        {
            ExpressoNameBinder binder = new ExpressoNameBinder(parser);
            binder.OnlyBind(parser.TopmostAst);
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

        void OnlyBind(ExpressoAst unboundAst)
        {
            Debug.Assert(unboundAst != null);

            // Find all scopes and variables
            unboundAst.AcceptWalker(this);

            TypeChecker.Check(unboundAst, parser);
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
            Debug.Assert(symbol_table.Name == "programRoot", "When entering VisitAst, symbol_table should be programRoot");
            scope_counter = 0;

            ast.Imports.AcceptWalker(this);
            ast.Declarations.AcceptWalker(this);
            Debug.Assert(symbol_table.Name == "programRoot", "When exiting VisitAst, symbol_table should be programRoot");
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

        public void VisitValueBindingForStatement(ValueBindingForStatement valueBindingForStmt)
        {
            int tmp_counter = scope_counter;
            DecendScope();
            scope_counter = 0;

            valueBindingForStmt.Variables.AcceptWalker(this);
            valueBindingForStmt.Body.AcceptWalker(this);

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

        public void VisitThrowStatement(ThrowStatement throwStmt)
        {
            throwStmt.CreationExpression.AcceptWalker(this);
        }

        public void VisitTryStatement(TryStatement tryStmt)
        {
            tryStmt.EnclosingBlock.AcceptWalker(this);
            tryStmt.CatchClauses.AcceptWalker(this);
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

        public void VisitCatchClause(CatchClause catchClause)
        {
            catchClause.Pattern.AcceptWalker(this);
            catchClause.Body.AcceptWalker(this);
        }

        public void VisitClosureLiteralExpression(ClosureLiteralExpression closure)
        {
            int tmp_counter = scope_counter;
            DecendScope();
            scope_counter = 0;

            closure.Parameters.AcceptWalker(this);
            closure.Body.AcceptWalker(this);

            AscendScope();
            scope_counter = tmp_counter + 1;
        }

        public void VisitComprehensionExpression(ComprehensionExpression comp)
        {
            int tmp_counter = scope_counter;
            DecendScope();
            scope_counter = 0;

            comp.Body.AcceptWalker(this);
            comp.Item.AcceptWalker(this);

            AscendScope();
            scope_counter = tmp_counter + 1;
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

        public void VisitFinallyClause(FinallyClause finallyClause)
        {
            VisitBlock(finallyClause.Body);
        }

        public void VisitLiteralExpression(LiteralExpression literal)
        {
            //no op
        }

        public void VisitIdentifier(Identifier ident)
        {
            BindName(ident);
        }

        public void VisitIntegerSequenceExpression(IntegerSequenceExpression intSeq)
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

        public void VisitMemberReference(MemberReferenceExpression memRef)
        {
            // At this point we can't figure out which scope to use for the member expression
            // because we don't know anything about the type of the target expression.
            // So for now bind only the target expression.
            // Instead we'll do that in type check phase.
            memRef.Target.AcceptWalker(this);
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
            int tmp_counter = scope_counter;
            DecendScope();
            scope_counter = 0;

            matchClause.Patterns.AcceptWalker(this);
            matchClause.Guard.AcceptWalker(this);
            matchClause.Body.AcceptWalker(this);

            AscendScope();
            scope_counter = tmp_counter + 1;
        }

        public void VisitSequenceExpression(SequenceExpression seqExpr)
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
            BindTypeName(simpleType.IdentifierNode);
        }

        public void VisitPrimitiveType(PrimitiveType primitiveType)
        {
            // no op
        }

        public void VisitReferenceType(ReferenceType referenceType)
        {
            // no op
        }

        public void VisitMemberType(MemberType memberType)
        {
            // no op
        }

        public void VisitFunctionType(FunctionType funcType)
        {
            // no op
        }

        public void VisitParameterType(ParameterType paramType)
        {
            // no op
        }

        public void VisitPlaceholderType(PlaceholderType placeholderType)
        {
            // no op
        }

        public void VisitAliasDeclaration(AliasDeclaration aliasDecl)
        {
            // An alias name is another name for an item
            // so resolve the target name first and then bind the name to the alias.
            aliasDecl.Path.AcceptWalker(this);
            aliasDecl.AliasToken.IdentifierId = aliasDecl.Path.Items.Last().IdentifierId;
        }

        public void VisitFunctionDeclaration(FunctionDeclaration funcDecl)
        {
            UniqueIdGenerator.DefineNewId(funcDecl.NameToken);
            int tmp_counter = scope_counter;
            DecendScope();
            scope_counter = 0;

            funcDecl.Parameters.AcceptWalker(this);
            funcDecl.Body.AcceptWalker(this);

            AscendScope();
            scope_counter = tmp_counter + 1;
        }

        public void VisitFieldDeclaration(FieldDeclaration fieldDecl)
        {
            fieldDecl.Initializers.AcceptWalker(this);
        }

        public void VisitParameterDeclaration(ParameterDeclaration parameterDecl)
        {
            UniqueIdGenerator.DefineNewId(parameterDecl.NameToken);
        }

        public void VisitVariableInitializer(VariableInitializer initializer)
        {
            UniqueIdGenerator.DefineNewId(initializer.NameToken);
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
            keyValue.ValueExpression.AcceptWalker(this);
        }

        public void VisitPathExpression(PathExpression pathExpr)
        {
            // Side effect: After exiting this method, the symbol_table field will be set to
            // the type table corresponding to the most descendant type the path represents.
            if(pathExpr.Items.Count == 1){
                pathExpr.AsIdentifier.AcceptWalker(this);
            }else{
                var old_table = symbol_table;
                foreach(var ident in pathExpr.Items){
                    var tmp_table = symbol_table.GetTypeTable(ident.Name);
                    if(tmp_table == null)
                        VisitIdentifier(ident);
                    else
                        symbol_table = tmp_table;
                }

                symbol_table = old_table;
            }
        }

        public void VisitObjectCreationExpression(ObjectCreationExpression creation)
        {
            creation.TypePath.AcceptWalker(this);
            foreach(var keyvalue in creation.Items)
                keyvalue.ValueExpression.AcceptWalker(this);
        }

        public void VisitImportDeclaration(ImportDeclaration importDecl)
        {
            // Here's the good place to import names from other files
            // All external names will be imported into the module scope we are currently compiling
            var inner_parser = new Parser(parser.scanner.OpenChildFile(importDecl.ModuleName));
            inner_parser.Parse();

            //importDecl.ModuleNameToken.AcceptWalker(this);
            symbol_table.AddExternalSymbols(inner_parser.Symbols, importDecl.AliasName);
            ExpressoNameBinder.NameBindAst(inner_parser);
            UniqueIdGenerator.DefineNewId(importDecl.AliasNameToken);
            /*if(importDecl.AliasNameToken.IsNull){
                importDecl.ModuleNameToken.AcceptWalker(this);
                foreach(var ie in importDecl.ImportedEntities)
                    UniqueIdGenerator.DefineNewId(ie.AsIdentifier);
            }else{
                importDecl.ModuleNameToken.AcceptWalker(this);

                UniqueIdGenerator.DefineNewId(importDecl.AliasNameToken);
            }*/
        }

        public void VisitTypeDeclaration(TypeDeclaration typeDecl)
        {
            UniqueIdGenerator.DefineNewId(typeDecl.NameToken);

            // Add Return type node
            var type_node = AstType.MakeSimpleType(typeDecl.Name);
            typeDecl.AddChild(type_node, Roles.Type);

            int tmp_counter = scope_counter;
            DecendScope();
            scope_counter = 0;

            // Add self symbol to the scope of the class
            var self_ident = AstNode.MakeIdentifier("self");
            symbol_table.AddSymbol("self", self_ident);
            UniqueIdGenerator.DefineNewId(self_ident);
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
            UniqueIdGenerator.DefineNewId(identifierPattern.Identifier);
            identifierPattern.InnerPattern.AcceptWalker(this);
        }

        public void VisitValueBindingPattern(ValueBindingPattern valueBindingPattern)
        {
            valueBindingPattern.Variables.AcceptWalker(this);
        }

        public void VisitCollectionPattern(CollectionPattern collectionPattern)
        {
            collectionPattern.Items.AcceptWalker(this);
        }

        public void VisitDestructuringPattern(DestructuringPattern destructuringPattern)
        {
            destructuringPattern.TypePath.AcceptWalker(this);
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

        public void VisitIgnoringRestPattern(IgnoringRestPattern restPattern)
        {
            // no op
        }

        public void VisitKeyValuePattern(KeyValuePattern keyValuePattern)
        {
            keyValuePattern.Value.AcceptWalker(this);
        }
		#endregion

        void BindName(Identifier ident)
        {
            var referenced = symbol_table.GetSymbolInAnyScope(ident.Name);
            if(referenced != null)
                ident.IdentifierId = referenced.IdentifierId;

            if(ident.IdentifierId == 0){
                var native = SymbolTable.GetNativeSymbol(ident.Name);
                if(native == null){
                    parser.ReportSemanticError(
                        "Error ES0100: '{0}' turns out not to be declared or accessible in the current scope {1}!",
                        ident,
                        ident.Name, symbol_table.Name
                    );
                }else{
                    ident.IdentifierId = native.IdentifierId;
                }
            }
        }

        void BindTypeName(Identifier ident)
        {
            var symbol = symbol_table.GetTypeSymbolInAnyScope(ident.Name);
            if(symbol != null)
                ident.IdentifierId = symbol.IdentifierId;

            if(ident.IdentifierId == 0){
                parser.ReportSemanticError(
                    "Error ES0101: Type symbol '{0}' turns out not to be declared or accessible in the current scope {1}!",
                    ident,
                    ident.Name, symbol_table.Name
                );
            }
        }
	}
}


