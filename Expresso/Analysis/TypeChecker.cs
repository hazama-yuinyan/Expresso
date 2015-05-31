using System;
using System.Collections.Generic;
using System.Linq;
using ICSharpCode.NRefactory;
using Expresso.TypeSystem;


namespace Expresso.Ast.Analysis
{
    /// <summary>
    /// A type checker is responsible for type validity check as well as type inference, if needed.
    /// All <see cref="Expresso.Ast.PlaceholderType"/> nodes are replaced with real types
    /// inferred from the context.
    /// </summary>
    partial class TypeChecker : IAstWalker<AstType>
    {
        static PlaceholderType PlaceholderTypeNode = new PlaceholderType(TextLocation.Empty);
        int scope_counter;
        Parser parser;
        SymbolTable symbols;  //keep a SymbolTable reference in a private field for convenience
        TypeInferenceRunner inference_runner;

        public TypeChecker(Parser parser)
        {
            this.parser = parser;
            symbols = parser.Symbols;
            inference_runner = new TypeInferenceRunner(parser, this);
        }

        public static void Check(ExpressoAst ast, Parser parser)
        {
            var checker = new TypeChecker(parser);
            ast.AcceptWalker(checker);
        }

        void DescendScope()
        {
            symbols = symbols.Children[scope_counter];
        }

        void AscendScope()
        {
            symbols = symbols.Parent;
        }

        #region IAstWalker implementation

        public AstType VisitAst(ExpressoAst ast)
        {
            foreach(var decl in ast.Declarations)
                decl.AcceptWalker(this);

            return null;
        }

        public AstType VisitBlock(BlockStatement block)
        {
            int tmp_counter = scope_counter;
            scope_counter = 0;
            DescendScope();
            foreach(var stmt in block.Statements)
                stmt.AcceptWalker(this);

            AscendScope();
            scope_counter = tmp_counter + 1;
            return null;
        }

        public AstType VisitBreakStatement(BreakStatement breakStmt)
        {
            if(breakStmt.Count.Value.GetType() != typeof(int) || (int)breakStmt.Count.Value < 0){
                parser.ReportSemanticError(
                    "`count` expression in a break statement has to be a positive integer",
                    breakStmt
                );
            }

            return null;
        }

        public AstType VisitContinueStatement(ContinueStatement continueStmt)
        {
            if(continueStmt.Count.Value.GetType() != typeof(int) || (int)continueStmt.Count.Value < 0){
                parser.ReportSemanticError(
                    "`count` expression in a continue statement has to be a positive integer",
                    continueStmt
                );
            }

            return null;
        }

        public AstType VisitEmptyStatement(EmptyStatement emptyStmt)
        {
            return AstType.Null;
        }

        public AstType VisitExpressionStatement(ExpressionStatement exprStmt)
        {
            exprStmt.Expression.AcceptWalker(this);
            return SimpleType.Null;
        }

        public AstType VisitForStatement(ForStatement forStmt)
        {
            int tmp_counter = scope_counter;
            scope_counter = 0;
            DescendScope();

            var left_type = forStmt.Left.AcceptWalker(this);
            if(IsPlaceholderType(left_type)){
                var inferred_type = forStmt.Target.AcceptWalker(inference_runner);
                left_type.ReplaceWith(inferred_type);
            }else{
                forStmt.Target.AcceptWalker(this);
            }
            forStmt.Body.AcceptWalker(this);

            AscendScope();
            scope_counter = tmp_counter + 1;
            return null;
        }

        public AstType VisitIfStatement(IfStatement ifStmt)
        {
            int tmp_counter = scope_counter;
            scope_counter = 0;
            DescendScope();
            ifStmt.Condition.AcceptWalker(this);
            ifStmt.TrueBlock.AcceptWalker(this);
            ifStmt.FalseBlock.AcceptWalker(this);
            AscendScope();
            scope_counter = tmp_counter + 1;
            return null;
        }

        public AstType VisitReturnStatement(ReturnStatement returnStmt)
        {
            returnStmt.Expression.AcceptWalker(this);
            return null;
        }

        public AstType VisitMatchStatement(MatchStatement matchStmt)
        {
            int tmp_counter = scope_counter;
            scope_counter = 0;
            DescendScope();
            matchStmt.Target.AcceptWalker(this);
            foreach(var clause in matchStmt.Clauses)
                clause.AcceptWalker(this);

            AscendScope();
            scope_counter = tmp_counter + 1;
            return null;
        }

        public AstType VisitWhileStatement(WhileStatement whileStmt)
        {
            int tmp_counter = scope_counter;
            scope_counter = 0;
            DescendScope();
            whileStmt.Condition.AcceptWalker(this);
            whileStmt.Body.AcceptWalker(this);
            AscendScope();
            scope_counter = tmp_counter + 1;
            return null;
        }

        public AstType VisitYieldStatement(YieldStatement yieldStmt)
        {
            yieldStmt.Expression.AcceptWalker(this);
            return null;
        }

        public AstType VisitVariableDeclarationStatement(VariableDeclarationStatement varDecl)
        {
            foreach(var variable in varDecl.Variables)
                variable.AcceptWalker(this);

            return null;
        }

        public AstType VisitAssignment(AssignmentExpression assignment)
        {
            var left_type = assignment.Left.AcceptWalker(this);
            if(IsPlaceholderType(left_type)){
                var inferred_type = assignment.Left.AcceptWalker(inference_runner);
                left_type.ReplaceWith(inferred_type);
                return inferred_type;
            }else{
                var right_type = assignment.Right.AcceptWalker(this);
                if(!IsCompatibleWith(left_type, right_type)){
                    parser.ReportSemanticErrorRegional(
                        "Type `{0}` on left-hand-side isn't compatible with type `{1}` on right-hand-side.",
                        assignment.Left, assignment.Right,
                        left_type, right_type
                    );
                }
                return left_type;
            }
        }

        public AstType VisitBinaryExpression(BinaryExpression binaryExpr)
        {
            var lhs_type = binaryExpr.Left.AcceptWalker(this);
            var rhs_type = binaryExpr.Right.AcceptWalker(this);
            if(!IsCompatibleWith(lhs_type, rhs_type)){
                parser.ReportSemanticErrorRegional(
                    "Can not apply the operator {0} on `{1}` and `{2}`.",
                    binaryExpr.Left, binaryExpr.Right,
                    binaryExpr.OperatorToken, lhs_type, rhs_type
                );
            }
            return null;
        }

        public AstType VisitCallExpression(CallExpression callExpr)
        {
            var func_type = callExpr.Target.AcceptWalker(this);
            if(IsPlaceholderType(func_type)){
                var inferred = inference_runner.VisitCallExpression(callExpr);
                func_type.ReplaceWith(inferred);
                return ((FunctionType)inferred).ReturnType;
            }
            return ((FunctionType)func_type).ReturnType;
        }

        public AstType VisitCastExpression(CastExpression castExpr)
        {
            var target_type = castExpr.ToExpression;
            var expression_type = castExpr.Target.AcceptWalker(this);
            if(!IsCastable(expression_type, target_type)){
                parser.ReportSemanticErrorRegional(
                    "Can not cast the type `{0}` to type `{1}`.",
                    castExpr.Target, castExpr.ToExpression,
                    expression_type, target_type
                );
            }

            return target_type;
        }

        public AstType VisitComprehensionExpression(ComprehensionExpression comp)
        {
            return comp.ObjectType;
        }

        public AstType VisitComprehensionForClause(ComprehensionForClause compFor)
        {
            return null;
        }

        public AstType VisitComprehensionIfClause(ComprehensionIfClause compIf)
        {
            return null;
        }

        public AstType VisitConditionalExpression(ConditionalExpression condExpr)
        {
            var true_type = condExpr.TrueExpression.AcceptWalker(this);
            var false_type = condExpr.FalseExpression.AcceptWalker(this);
            if(!IsCompatibleWith(true_type, false_type)){
                parser.ReportSemanticErrorRegional(
                    "`{0}` is not compatible with `{1}`.",
                    condExpr.Condition, condExpr.FalseExpression,
                    true_type, false_type
                );
            }

            return true_type;
        }

        public AstType VisitKeyValueLikeExpression(KeyValueLikeExpression keyValue)
        {
            return null;
        }

        public AstType VisitLiteralExpression(LiteralExpression literal)
        {
            return literal.Type;
        }

        public AstType VisitIdentifier(Identifier ident)
        {
            return ident.Type;
        }

        public AstType VisitIntegerSequenceExpression(IntegerSequenceExpression intSeq)
        {
            return new PrimitiveType("intseq", TextLocation.Empty);
        }

        public AstType VisitIndexerExpression(IndexerExpression indexExpr)
        {
            var type = indexExpr.Target.AcceptWalker(this);
            if(type is PlaceholderType)
                inference_runner.VisitIndexerExpression(indexExpr);

            var simple_type = type as SimpleType;
            if(simple_type != null){
                if(simple_type.Name != "array" && simple_type.Name != "vector"){
                    parser.ReportSemanticError(
                        "Can not apply the indexer operator on type `{0}`",
                        indexExpr,
                        simple_type
                    );
                }

                return simple_type;
            }
            return null;
        }

        public AstType VisitMemberReference(MemberReference memRef)
        {
            var type = memRef.Target.AcceptWalker(this);
            if(type is PlaceholderType)
                inference_runner.VisitMemberReference(memRef);

            var type_table = symbols.GetTypeTable(type.Name);
            if(type_table != null){
                var symbol = type_table.GetSymbol(memRef.Member.Name);
                if(symbol == null){
                    parser.ReportSemanticError(
                        "Type `{0}` doesn't have a field named {1}.",
                        memRef,
                        type.Name, memRef.Member.Name
                    );
                }else{
                    // Bind the name of the member here
                    memRef.Member.IdentifierId = symbol.IdentifierId;
                    return symbol.Type;
                }
            }

            return null;
        }

        public AstType VisitNewExpression(NewExpression newExpr)
        {
            return newExpr.CreationExpression.AcceptWalker(this);
        }

        public AstType VisitPathExpression(PathExpression pathExpr)
        {
            if(pathExpr.Items.Count == 1){
                return VisitIdentifier(pathExpr.AsIdentifier);
            }else{
                while(symbols.Parent != null)
                    symbols = symbols.Parent;

                AstType result = null;
                foreach(var item in pathExpr.Items){
                    result = VisitIdentifier(item);
                    symbols = symbols.GetTypeTable(item.Name);
                }

                return result;
            }
        }

        public AstType VisitParenthesizedExpression(ParenthesizedExpression parensExpr)
        {
            return parensExpr.Expression.AcceptWalker(this);
        }

        public AstType VisitObjectCreationExpression(ObjectCreationExpression creation)
        {
            var type_table = symbols.GetTypeTable(creation.TypePath.IdentifierNode.Name);
            if(type_table == null){
                parser.ReportSemanticError(
                    "Type `{0}` isn't found or accessible from the scope {1}.",
                    creation,
                    creation.TypePath, symbols.Name
                );
            }

            foreach(var key_value in creation.Items){
                var key_path = key_value.KeyExpression as PathExpression;
                if(key_path == null)
                    throw new InvalidOperationException();

                var key = type_table.GetSymbol(key_path.AsIdentifier.Name);
                if(key == null){
                    parser.ReportSemanticError(
                        "Type `{0}` doesn't have a field {1}.",
                        key_value.KeyExpression,
                        creation.TypePath, key_path.AsIdentifier.Name
                    );
                }else{
                    var value_type = key_value.ValueExpression.AcceptWalker(this);
                    if(!IsCastable(value_type, key.Type)){
                        parser.ReportSemanticErrorRegional(
                            "The field {0} expects the value to be of type `{1}`, but it actually is `{2}`.",
                            key_value.KeyExpression, key_value.ValueExpression,
                            key_path.AsIdentifier.Name, key.Type, value_type
                        );
                    }
                }
            }
            return creation.TypePath;
        }

        public AstType VisitSequenceInitializer(SequenceInitializer seqInitializer)
        {
            return null;
        }

        public AstType VisitMatchClause(MatchPatternClause matchClause)
        {
            AstType result = null;
            foreach(var pattern in matchClause.Patterns){
                var tmp = pattern.AcceptWalker(this);
                result = inference_runner.FigureOutCommonType(result, tmp);
            }
            return result;
        }

        public AstType VisitSequence(SequenceExpression seqExpr)
        {
            return null;
        }

        public AstType VisitUnaryExpression(UnaryExpression unaryExpr)
        {
            switch(unaryExpr.Operator){
            case OperatorType.Reference:
                return null;

            case OperatorType.Dereference:
                return null;

            case OperatorType.Plus:
            case OperatorType.Minus:
                {
                    var tmp = unaryExpr.Operand.AcceptWalker(this);
                    if(IsPlaceholderType(tmp)){
                        var inferred_type = inference_runner.VisitUnaryExpression(unaryExpr);
                        if(inferred_type.IsNull)
                            return inferred_type;

                        tmp.ReplaceWith(inferred_type);
                    }

                    var primitive_type = tmp as PrimitiveType;
                    if(primitive_type == null || tmp.IsNull || primitive_type.KnownTypeCode == KnownTypeCode.Char){
                        parser.ReportSemanticError(
                            "Can not apply the operator {0} on type `{1}`.",
                            unaryExpr,
                            unaryExpr.OperatorToken, tmp.Name
                        );
                    }
                    return tmp;
                }

            case OperatorType.Not:
                var operand_type = unaryExpr.Operand.AcceptWalker(this);
                if(!(operand_type is PrimitiveType) || ((PrimitiveType)operand_type).KnownTypeCode != Expresso.TypeSystem.KnownTypeCode.Bool){
                    parser.ReportSemanticError(
                        "Can not apply the '!' operator on type `{0}`!\nThe operand must be of type `bool`.",
                        operand_type
                    );
                }
                return operand_type;

            default:
                throw new FatalError("Unknown unary operator!");
            }
        }

        public AstType VisitSelfReferenceExpression(SelfReferenceExpression selfRef)
        {
            return selfRef.SelfIdentifier.Type;
        }

        public AstType VisitSuperReferenceExpression(SuperReferenceExpression superRef)
        {
            return superRef.SuperIdentifier.Type;
        }

        public AstType VisitCommentNode(CommentNode comment)
        {
            return null;
        }

        public AstType VisitTextNode(TextNode textNode)
        {
            return null;
        }

        public AstType VisitSimpleType(SimpleType simpleType)
        {
            BindTypeName(simpleType.IdentifierToken);
            // If the type arguments contain any unsubstituted arguments(placeholder nodes)
            // return the statically defined placeholder type node to indicate that it needs to be inferenced
            if(simpleType.TypeArguments.HasChildren && IsPlaceholderType(simpleType.TypeArguments.FirstOrNullObject()))
                return PlaceholderTypeNode;
            else
                return simpleType;
        }

        public AstType VisitPrimitiveType(PrimitiveType primitiveType)
        {
            return primitiveType;
        }

        public AstType VisitReferenceType(ReferenceType referenceType)
        {
            return referenceType.BaseType;
        }

        public AstType VisitMemberType(MemberType memberType)
        {
            return null;
        }

        public AstType VisitFunctionType(FunctionType funcType)
        {
            return funcType.ReturnType;
        }

        public AstType VisitPlaceholderType(PlaceholderType placeholderType)
        {
            return AstType.Null;
        }

        public AstType VisitAliasDeclaration(AliasDeclaration aliasDecl)
        {
            var original_type = aliasDecl.Path.AcceptWalker(this);
            aliasDecl.AliasToken.IdentifierId = original_type.IdentifierNode.IdentifierId;
            return null;
        }

        public AstType VisitImportDeclaration(ImportDeclaration importDecl)
        {
            if(!importDecl.AliasNameToken.IsNull){
                var module_type = importDecl.ModuleNameToken.AcceptWalker(this);
                importDecl.AliasNameToken.IdentifierId = module_type.IdentifierNode.IdentifierId;
            }
            return null;
        }

        public AstType VisitFunctionDeclaration(FunctionDeclaration funcDecl)
        {
            int tmp_counter = scope_counter;
            DescendScope();
            scope_counter = 0;

            foreach(var param in funcDecl.Parameters){
                var param_type = param.AcceptWalker(this);
                if(IsPlaceholderType(param_type)){
                    param.AcceptWalker(inference_runner);
                }else{
                    if(!param.Option.IsNull){
                        var option_type = param.Option.AcceptWalker(this);
                        if(!IsCastable(option_type, param_type)){
                            parser.ReportSemanticErrorRegional(
                                "Type mismatch; `{0}` is not compatible with `{1}`.",
                                param.NameToken, param.Option,
                                option_type, param_type
                            );
                        }
                    }
                }
            }

            funcDecl.Body.AcceptWalker(this);
            if(IsPlaceholderType(funcDecl.ReturnType)){
                var return_type = inference_runner.VisitFunctionDeclaration(funcDecl);
                funcDecl.ReturnType.ReplaceWith(return_type);
            }

            if(IsPlaceholderType(funcDecl.NameToken.Type)){
                var param_types =
                    from param in funcDecl.Parameters
                    select param.ReturnType.Clone();

                var return_type = funcDecl.ReturnType.Clone();
                var func_type = AstType.MakeFunctionType(funcDecl.Name, return_type, param_types);
                funcDecl.NameToken.Type.ReplaceWith(func_type);
            }

            AscendScope();
            scope_counter = tmp_counter + 1;
            return null;
        }

        public AstType VisitTypeDeclaration(TypeDeclaration typeDecl)
        {
            int tmp_counter = scope_counter;
            DescendScope();
            scope_counter = 0;

            foreach(var super_type in typeDecl.BaseTypes)
                super_type.AcceptWalker(this);

            foreach(var member in typeDecl.Members)
                member.AcceptWalker(this);

            AscendScope();
            scope_counter = tmp_counter + 1;
            return null;
        }

        public AstType VisitFieldDeclaration(FieldDeclaration fieldDecl)
        {
            foreach(var field in fieldDecl.Initializers){
                var field_type = field.AcceptWalker(this);
                if(IsPlaceholderType(field_type)){
                    var inferred_type = inference_runner.VisitVariableInitializer(field);
                    field_type.ReplaceWith(inferred_type);
                }else{
                    if(!field.Initializer.IsNull){
                        var init_type = field.Initializer.AcceptWalker(this);
                        if(!IsCastable(init_type, field_type)){
                            parser.ReportSemanticErrorRegional(
                                "Can not implicitly cast type `{0}` to type `{1}`.",
                                field.NameToken, field.Initializer,
                                init_type, field_type
                            );
                        }
                    }
                }
            }

            return null;
        }

        public AstType VisitParameterDeclaration(ParameterDeclaration parameterDecl)
        {
            // Don't check NameToken.Type is a placeholder type node.
            // It is the parent's job to resolve and replace this node with an actual type node.
            return parameterDecl.NameToken.Type;
        }

        public AstType VisitVariableInitializer(VariableInitializer initializer)
        {
            var left_type = initializer.NameToken.AcceptWalker(this);
            if(IsPlaceholderType(left_type)){
                var inferred_type = initializer.Initializer.AcceptWalker(inference_runner);
                left_type.ReplaceWith(inferred_type);
                return inferred_type;
            }else{
                var rhs_type = initializer.Initializer.AcceptWalker(this);
                if(!IsCompatibleWith(left_type, rhs_type)){
                    parser.ReportSemanticErrorRegional(
                        "Type `{0}` on the left-hand-side is not compatible with `{1}` on the right-hand-side.",
                        initializer.NameToken,
                        initializer.Initializer,
                        left_type, rhs_type
                    );
                }
                return left_type;
            }
        }

        public AstType VisitWildcardPattern(WildcardPattern wildcardPattern)
        {
            return SimpleType.Null;
        }

        public AstType VisitIdentifierPattern(IdentifierPattern identifierPattern)
        {
            return !identifierPattern.InnerPattern.IsNull ? identifierPattern.InnerPattern.AcceptWalker(this) : AstType.Null;
        }

        public AstType VisitValueBindingPattern(ValueBindingPattern valueBindingPattern)
        {
            return valueBindingPattern.Variables.AcceptWalker(this);
        }

        public AstType VisitCollectionPattern(CollectionPattern collectionPattern)
        {
            return null;
        }

        public AstType VisitDestructuringPattern(DestructuringPattern destructuringPattern)
        {
            return null;
        }

        public AstType VisitTuplePattern(TuplePattern tuplePattern)
        {
            return null;
        }

        public AstType VisitExpressionPattern(ExpressionPattern exprPattern)
        {
            return null;
        }

        public AstType VisitNullNode(AstNode nullNode)
        {
            return null;
        }

        public AstType VisitNewLine(NewLineNode newlineNode)
        {
            return null;
        }

        public AstType VisitWhitespace(WhitespaceNode whitespaceNode)
        {
            return null;
        }

        public AstType VisitExpressoTokenNode(ExpressoTokenNode tokenNode)
        {
            return null;
        }

        public AstType VisitPatternPlaceholder(AstNode placeholder, ICSharpCode.NRefactory.PatternMatching.Pattern child)
        {
            return null;
        }

        #endregion

        /// <summary>
        /// In Expresso, there are 3 valid cast cases.
        /// The first is the identity cast.
        /// </summary>
        /// <returns><c>true</c> if <c>fromType</c> can be casted to <c>totype</c>; otherwise, <c>false</c>.</returns>
        static bool IsCastable(AstType fromType, AstType toType)
        {
            // TODO: implement it
            return false;
        }

        static bool IsCompatibleWith(AstType first, AstType second)
        {
            // TODO: implement it
            return true;
        }

        static bool IsPlaceholderType(AstType type)
        {
            return type is PlaceholderType;
        }

        void BindTypeName(Identifier ident)
        {
            var table = symbols;
            while(table != null){
                var referenced = table.GetTypeSymbol(ident.Name);
                if(referenced != null){
                    ident.IdentifierId = referenced.IdentifierId;
                    return;
                }

                table = table.Parent;
            }

            if(ident.IdentifierId == 0){
                parser.ReportSemanticError(
                    "Type name `{0}` turns out not to be declared in the current scope {1}!",
                    ident,
                    ident.Name, table.Name
                );
            }
        }
    }
}

