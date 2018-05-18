using System;
using System.Diagnostics;
using System.Linq;
using ICSharpCode.NRefactory.PatternMatching;


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
    /// During name binding, we first define names on variables, fields, methods and types.
    /// And then bind names on those items except for types.
    /// For types we'll bind them during type validity check.
    /// By name binding, I mean that we give the same id defined on the name definition phase.
    /// That means that it is not guaranteed that the type of the target object is resolved.
    /// </summary>
    class ExpressoNameBinder : IAstWalker
	{
        int scope_counter;
        bool check_shadowing;
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
            #if !NETCOREAPP2_0 && DEBUG
            Console.WriteLine("We have given ids on total of {0} identifiers.", UniqueIdGenerator.CurrentId - 1);
            #endif
		}
		#endregion
		
		void Bind(ExpressoAst unboundAst)
		{
            Debug.Assert(unboundAst != null, "We have to hold something to bind to");
			
            unboundAst.AcceptWalker(this);
			
            TypeChecker.Check(unboundAst, parser);
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
            Debug.Assert(symbol_table.Name == "programRoot", "When entering VisitAst, symbol_table should be programRoot");
            scope_counter = 0;

            #if !NETCOREAPP2_0
            Console.WriteLine("Resolving names in {0}...", ast.ModuleName);
            #endif

            // We don't call VisitAttributeSection directly so that we can avoid unnecessary method calls
            ast.Attributes.AcceptWalker(this);
            ast.Imports.AcceptWalker(this);
            ast.Declarations.AcceptWalker(this);
            Debug.Assert(symbol_table.Name == "programRoot", "When exiting VisitAst, symbol_table should be programRoot");
        }

        public void VisitBlock(BlockStatement block)
        {
            if(block.IsNull)
                return;
            
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

        public void VisitDoWhileStatement(DoWhileStatement doWhileStmt)
        {
            VisitWhileStatement(doWhileStmt.Delegator);
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
            VisitBlock(forStmt.Body);

            AscendScope();
            scope_counter = tmp_counter + 1;
        }

        public void VisitValueBindingForStatement(ValueBindingForStatement valueBindingForStmt)
        {
            int tmp_counter = scope_counter;
            DecendScope();
            scope_counter = 0;

            check_shadowing = true;
            valueBindingForStmt.Initializer.AcceptWalker(this);
            check_shadowing = false;
            VisitBlock(valueBindingForStmt.Body);

            AscendScope();
            scope_counter = tmp_counter + 1;
        }

        public void VisitIfStatement(IfStatement ifStmt)
        {
            int tmp_counter = scope_counter;
            DecendScope();
            scope_counter = 0;

            ifStmt.Condition.AcceptWalker(this);
            VisitBlock(ifStmt.TrueBlock);
            // We can't rewrite this to VisitBlock(ifStmt.FalseBlock);
            // because doing so can continue execution even if the node is null
            ifStmt.FalseStatement.AcceptWalker(this);

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
            VisitBlock(tryStmt.EnclosingBlock);
            tryStmt.CatchClauses.AcceptWalker(this);
            // Directly calling VisitFinally continues execution even if the node is null.
            tryStmt.FinallyClause.AcceptWalker(this);
        }

        public void VisitWhileStatement(WhileStatement whileStmt)
        {
            int tmp_counter = scope_counter;
            DecendScope();
            scope_counter = 0;

            whileStmt.Condition.AcceptWalker(this);
            VisitBlock(whileStmt.Body);

            AscendScope();
            scope_counter = tmp_counter + 1;
        }

        public void VisitYieldStatement(YieldStatement yieldStmt)
        {
            yieldStmt.Expression.AcceptWalker(this);
        }

        public void VisitVariableDeclarationStatement(VariableDeclarationStatement varDecl)
        {
            check_shadowing = true;
            varDecl.Variables.AcceptWalker(this);
            check_shadowing = false;
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
            int tmp_counter = scope_counter;
            DecendScope();
            scope_counter = 0;

            UniqueIdGenerator.DefineNewId(catchClause.Identifier);
            VisitBlock(catchClause.Body);

            AscendScope();
            scope_counter = tmp_counter + 1;
        }

        public void VisitClosureLiteralExpression(ClosureLiteralExpression closure)
        {
            int tmp_counter = scope_counter;
            DecendScope();
            scope_counter = 0;

            closure.Parameters.AcceptWalker(this);
            VisitBlock(closure.Body);

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
            // no op
        }

        public void VisitIdentifier(Identifier ident)
        {
            BindNameOrTypeName(ident);
        }

        public void VisitIntegerSequenceExpression(IntegerSequenceExpression intSeq)
        {
            intSeq.Start.AcceptWalker(this);
            intSeq.End.AcceptWalker(this);
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

        public void VisitNullReferenceExpression(NullReferenceExpression nullRef)
        {
            // no op
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
            var original_type_table = symbol_table;
            var type_table = symbol_table.GetTypeTable(memberType.Target.Name);
            symbol_table = type_table;
            VisitIdentifier(memberType.IdentifierNode);
            symbol_table = original_type_table;
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

        public void VisitAttributeSection(AttributeSection section)
        {
            section.Attributes.AcceptWalker(this);
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

            // We don't call VisitAttributeSection directly so that we can avoid unnecessary method calls
            funcDecl.Attribute.AcceptWalker(this);

            funcDecl.Parameters.AcceptWalker(this);
            VisitBlock(funcDecl.Body);

            AscendScope();
            scope_counter = tmp_counter + 1;
        }

        public void VisitFieldDeclaration(FieldDeclaration fieldDecl)
        {
            // We don't call VisitAttributeSection directly so that we can avoid unnecessary method calls
            fieldDecl.Attribute.AcceptWalker(this);
            fieldDecl.Initializers.AcceptWalker(this);
        }

        public void VisitParameterDeclaration(ParameterDeclaration parameterDecl)
        {
            // We don't call VisitAttributeSection directly so that we can avoid unnecessary method calls
            parameterDecl.Attribute.AcceptWalker(this);
            UniqueIdGenerator.DefineNewId(parameterDecl.NameToken);
        }

        public void VisitVariableInitializer(VariableInitializer initializer)
        {
            initializer.Pattern.AcceptWalker(this);
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

        public void VisitPatternPlaceholder(AstNode placeholder, Pattern child)
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
            // Side effect: Inside this method, the symbol_table field will be set to
            // the type table corresponding to the most descendant type the path represents.
            if(pathExpr.Items.Count == 1){
                VisitIdentifier(pathExpr.AsIdentifier);
            }else{
                var old_table = symbol_table;
                foreach(var ident in pathExpr.Items){
                    var tmp_table = symbol_table.GetTypeTable(ident.Name);
                    if(tmp_table == null){
                        VisitIdentifier(ident);
                    }else{
                        var type_symbol = symbol_table.GetTypeSymbolInAnyScope(ident.Name);
                        if(type_symbol == null){
                            parser.ReportSemanticError(
                                "`{0}` in the path '{1}' doesn't represent a type or a module.",
                                "ES1500",
                                ident,
                                ident.Name, pathExpr
                            );
                            return;
                        }
                        // MAYBE This part is not the bussiness of it. Move it to TypeChecker?
                        ident.IdentifierId = type_symbol.IdentifierId;
                        ident.Type = type_symbol.Type.Clone();
                        symbol_table = tmp_table;
                    }
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
            //foreach(var alias in importDecl.AliasTokens)
            //    alias.
        }

        public void VisitTypeDeclaration(TypeDeclaration typeDecl)
        {
            UniqueIdGenerator.DefineNewId(typeDecl.NameToken);

            // We don't call VisitAttributeSection directly so that we can avoid unnecessary method calls
            typeDecl.Attribute.AcceptWalker(this);

            // Add Return type node
            var type_node = AstType.MakeSimpleType(typeDecl.Name);
            typeDecl.AddChild(type_node, Roles.Type);

            foreach(var base_type in typeDecl.BaseTypes)
                base_type.AcceptWalker(this);

            int tmp_counter = scope_counter;
            DecendScope();
            scope_counter = 0;

            // Add self symbol to the scope of the class
            var self_ident = AstNode.MakeIdentifier("self");
            symbol_table.AddSymbol("self", self_ident);
            UniqueIdGenerator.DefineNewId(self_ident);
            typeDecl.Members.AcceptWalker(this);

            // Add the constructor here so that we can use it before the type is inspected in TypeChecker
            var field_types = typeDecl.Members
                                      .OfType<FieldDeclaration>()
                                      .SelectMany(fd => fd.Initializers.Select(init => init.NameToken.Type.Clone()));
            var name = "constructor";
            var return_type = AstType.MakeSimpleType("tuple");
            var ctor_type = AstType.MakeFunctionType(name, return_type, field_types);
            symbol_table.AddSymbol(name, ctor_type);

            AscendScope();
            scope_counter = tmp_counter + 1;
        }

        public void VisitWildcardPattern(WildcardPattern wildcardPattern)
        {
            // no op
        }

        public void VisitIdentifierPattern(IdentifierPattern identifierPattern)
        {
            if(check_shadowing){
                var tmp_table = symbol_table.Parent;
                while(tmp_table.Name != "programRoot")
                    tmp_table = tmp_table.Parent;
                
                var symbol = tmp_table.GetSymbol(identifierPattern.Identifier.Name);
                if(symbol != null){
                    throw new ParserException(
                        "Local bindings cannot shadow module variables: '{0}' tries to shadow {1}.",
                        "ES3100",
                        identifierPattern,
                        identifierPattern.Identifier.ToString(), symbol
                    );
                }
            }

            UniqueIdGenerator.DefineNewId(identifierPattern.Identifier);
            identifierPattern.InnerPattern.AcceptWalker(this);
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

        public void VisitPatternWithType(PatternWithType pattern)
        {
            pattern.Pattern.AcceptWalker(this);
        }
		#endregion

        void BindName(Identifier ident)
        {
            var referenced = symbol_table.GetSymbolInAnyScope(ident.Name);
            if(referenced != null){
                ident.IdentifierId = referenced.IdentifierId;
                ident.Modifiers = referenced.Modifiers;
            }

            if(ident.IdentifierId == 0){
                var native = SymbolTable.GetNativeSymbol(ident.Name);
                if(native == null){
                    if(referenced != null){
                        parser.ReportSemanticError(
                            "You can't use '{0}' before declared!",
                            "ES0120",
                            ident,
                            ident.Name
                        );
                    }else{
                        parser.ReportSemanticError(
                            "The name '{0}' turns out not to be declared or accessible in the current scope {1}!",
                            "ES0100",
                            ident,
                            ident.Name, symbol_table.Name
                        );
                    }
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
                if(symbol != null){
                    parser.ReportSemanticError(
                        "You can't use the type symbol '{0}' before defined!",
                        "ES0121",
                        ident,
                        ident.Name
                    );
                }else{
                    parser.ReportSemanticError(
                        "The type symbol '{0}' turns out not to be declared or accessible in the current scope {1}!",
                        "ES0101",
                        ident,
                        ident.Name, symbol_table.Name
                    );
                }
            }
        }

        void BindNameOrTypeName(Identifier ident)
        {
            var referenced = symbol_table.GetSymbolInAnyScope(ident.Name);
            if(referenced != null){
                ident.IdentifierId = referenced.IdentifierId;
                ident.Modifiers = referenced.Modifiers;
            }

            if(ident.IdentifierId == 0){
                var native = SymbolTable.GetNativeSymbol(ident.Name);
                if(native != null)
                    ident.IdentifierId = native.IdentifierId;
            }

            var symbol = symbol_table.GetTypeSymbolInAnyScope(ident.Name);
            if(symbol != null)
                ident.IdentifierId = symbol.IdentifierId;

            if(ident.IdentifierId == 0){
                if(referenced != null || symbol != null){
                    parser.ReportSemanticError(
                        "You can't use '{0}' before defined or declared!",
                        "ES0122",
                        ident,
                        ident.Name
                    );
                }else{
                    parser.ReportSemanticError(
                        "The symbol '{0}' turns out not to be declared or accessible in the current scope {1}!",
                        "ES0102",
                        ident,
                        ident.Name, symbol_table.Name
                    );
                }
            }
        }
	}
}


