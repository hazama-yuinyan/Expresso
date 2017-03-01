﻿using System;
using System.Collections.Generic;
using System.Linq;
using ICSharpCode.NRefactory;
using Expresso.TypeSystem;


namespace Expresso.Ast.Analysis
{
    /// <summary>
    /// A type checker is responsible for type validity check as well as type inference, if needed.
    /// All <see cref="Expresso.Ast.PlaceholderType"/> nodes will be replaced with real types
    /// inferred from the context.
    /// </summary>
    partial class TypeChecker : IAstWalker<AstType>
    {
        static PlaceholderType PlaceholderTypeNode = new PlaceholderType(TextLocation.Empty);
        static List<AstType> TemporaryTypes = new List<AstType>();
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

        internal void DescendScope()
        {
            symbols = symbols.Children[scope_counter];
        }

        internal void AscendScope()
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
            DescendScope();
            scope_counter = 0;

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
                    "Error ES4000: `count` expression in a break statement has to be a positive integer",
                    breakStmt
                );
            }

            return null;
        }

        public AstType VisitContinueStatement(ContinueStatement continueStmt)
        {
            if(continueStmt.Count.Value.GetType() != typeof(int) || (int)continueStmt.Count.Value < 0){
                parser.ReportSemanticError(
                    "Error ES4001: `count` expression in a continue statement has to be a positive integer",
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
            return exprStmt.Expression.AcceptWalker(this);
        }

        public AstType VisitForStatement(ForStatement forStmt)
        {
            int tmp_counter = scope_counter;
            DescendScope();
            scope_counter = 0;

            var left_type = forStmt.Left.AcceptWalker(this);
            AstType target_type;
            if(IsPlaceholderType(left_type))
                target_type = forStmt.Target.AcceptWalker(inference_runner);
            else
                target_type = forStmt.Target.AcceptWalker(this);

            if(!IsSequenceType(target_type)){
                parser.ReportSemanticError("Error ES1300: `{0}` isn't a sequence type! A for statement can only be used for iterating over sequences",
                    forStmt.Target,
                    left_type
                );
            }else{
                var elem_type = MakeOutElementType(target_type);
                left_type.ReplaceWith(elem_type);
            }

            forStmt.Body.AcceptWalker(this);

            AscendScope();
            scope_counter = tmp_counter + 1;
            return null;
        }

        public AstType VisitValueBindingForStatement(ValueBindingForStatement valueBindingForStatment)
        {
            int tmp_counter = scope_counter;
            DescendScope();
            scope_counter = 0;

            foreach(var variable in valueBindingForStatment.Variables){
                var target_type = variable.Initializer.AcceptWalker(this);
                if(!IsSequenceType(target_type)){
                    parser.ReportSemanticError(
                        "Error ES1300: `{0}` isn't a sequence type! A for statemant can only be used for iterating over sequences.",
                        variable.Initializer,
                        target_type
                    );
                }else{
                    var elem_type = MakeOutElementType(target_type);
                    var left_type = variable.NameToken.AcceptWalker(this);
                    if(IsPlaceholderType(left_type))
                        left_type.ReplaceWith(elem_type);
                }
            }

            valueBindingForStatment.Body.AcceptWalker(this);

            AscendScope();
            scope_counter = tmp_counter + 1;
            return null;
        }

        public AstType VisitIfStatement(IfStatement ifStmt)
        {
            int tmp_counter = scope_counter;
            DescendScope();
            scope_counter = 0;

            var condition_type = ifStmt.Condition.AcceptWalker(this) as PrimitiveType;
            if(condition_type == null || condition_type.KnownTypeCode != KnownTypeCode.Bool){
                parser.ReportSemanticError(
                    "Error ES4000: The condition expression has to be of type `bool`",
                    ifStmt.Condition
                );
            }
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
            DescendScope();
            scope_counter = 0;

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
            DescendScope();
            scope_counter = 0;

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
            TemporaryTypes.Clear();
            assignment.AcceptWalker(inference_runner);

            var left_type = assignment.Left.AcceptWalker(this);
            if(left_type == SimpleType.Null){
                // We see the left-hand-side is a sequence expression so validate each item on both sides.
                var left_types = TemporaryTypes.ToList();
                TemporaryTypes.Clear();
                assignment.Right.AcceptWalker(this);
                // Don't validate the number of elements because we have already done that in parse phase.
                for(int i = 0; i < left_types.Count; ++i){
                    if(IsPlaceholderType(left_types[i])){
                        var inferred_type = TemporaryTypes[i].Clone();
                        left_types[i].ReplaceWith(inferred_type);
                    }else if(IsCompatibleWith(left_types[i], TemporaryTypes[i]) == TriBool.False){
                        var lhs_seq = assignment.Left as SequenceExpression;
                        var rhs_seq = assignment.Right as SequenceExpression;
                        parser.ReportSemanticErrorRegional("Error ES1100: There is a type mismatch; left=`{0}`, right=`{1}`",
                            lhs_seq.Items.ElementAt(i), rhs_seq.Items.ElementAt(i),
                            left_types[i], TemporaryTypes[i]
                        );
                    }
                }
                TemporaryTypes.Clear();
                return AstType.Null;
            }else if(IsPlaceholderType(left_type)){
                var inferred_type = assignment.Right.AcceptWalker(inference_runner);
                left_type.ReplaceWith(inferred_type);
                return inferred_type;
            }else{
                var right_type = assignment.Right.AcceptWalker(this);
                if(IsCompatibleWith(left_type, right_type) == TriBool.False){
                    parser.ReportSemanticErrorRegional(
                        "Error ES1002: Type `{0}` on left-hand-side isn't compatible with type `{1}` on right-hand-side.",
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
            if(IsPlaceholderType(lhs_type)){
                lhs_type = binaryExpr.Left.AcceptWalker(inference_runner);
                // Do not replace the type node because inference runner has already done that.
                //lhs_type.ReplaceWith(inferred_type);
                //lhs_type = inferred_type;
            }

            var rhs_type = binaryExpr.Right.AcceptWalker(this);
            if(IsPlaceholderType(rhs_type)){
                rhs_type = binaryExpr.Right.AcceptWalker(inference_runner);
                //rhs_type.ReplaceWith(inferred_type2);
                //rhs_type = inferred_type2;
            }

            if(IsCompatibleWith(lhs_type, rhs_type) == TriBool.False){
                parser.ReportSemanticErrorRegional(
                    "Error ES1003: Can not apply the operator '{0}' on `{1}` and `{2}`.",
                    binaryExpr.Left, binaryExpr.Right,
                    binaryExpr.OperatorToken, lhs_type, rhs_type
                );
            }

            var lhs_primitive = lhs_type as PrimitiveType;
            var rhs_primitive = rhs_type as PrimitiveType;
            switch(binaryExpr.Operator){
            case OperatorType.ConditionalAnd:
            case OperatorType.ConditionalOr:
            case OperatorType.Equality:
            case OperatorType.InEquality:
            case OperatorType.LessThan:
            case OperatorType.LessThanOrEqual:
            case OperatorType.GreaterThan:
            case OperatorType.GreaterThanOrEqual:
                return AstType.MakePrimitiveType("bool", binaryExpr.StartLocation);

            case OperatorType.Plus:
            case OperatorType.Minus:
            case OperatorType.Times:
            case OperatorType.Divide:
            case OperatorType.Power:
                if(!IsNumericalType(lhs_type) || !IsNumericalType(rhs_type)){
                    parser.ReportSemanticErrorRegional("Error ES1003: Can not apply the operator '{0}' on `{1}` and `{2}`",
                        binaryExpr.Left, binaryExpr.Right,
                        binaryExpr.OperatorToken, lhs_type, rhs_type
                    );
                    return null;
                }

                return lhs_type;

            case OperatorType.BitwiseAnd:
            case OperatorType.BitwiseOr:
            case OperatorType.BitwiseShiftLeft:
            case OperatorType.BitwiseShiftRight:
            case OperatorType.ExclusiveOr:
                if(lhs_primitive.KnownTypeCode == KnownTypeCode.Float || lhs_primitive.KnownTypeCode == KnownTypeCode.Double){
                    parser.ReportSemanticError("Error ES1010: Can not apply the operator '{0}' on the left-hand-side '{1}'",
                        binaryExpr.Left,
                        binaryExpr.OperatorToken, binaryExpr.Left
                    );
                    return null;
                }else if(rhs_primitive.KnownTypeCode == KnownTypeCode.Float || rhs_primitive.KnownTypeCode == KnownTypeCode.Double){
                    parser.ReportSemanticError("Error ES1010: Can not apply the operator '{0}' on the right-hand-side '{1}'",
                        binaryExpr.Right,
                        binaryExpr.OperatorToken, binaryExpr.Right
                    );
                    return null;
                }else{
                    return lhs_type;
                }

            default:
                throw new ArgumentException("Unknown operator found!");
            }
        }

        public AstType VisitCallExpression(CallExpression callExpr)
        {
            var func_type = callExpr.Target.AcceptWalker(this);
            if(IsPlaceholderType(func_type)){
                var inferred = inference_runner.VisitCallExpression(callExpr);
                // Don't replace nodes here because the above code does that
                //func_type.ReplaceWith(inferred);
                return inferred;
            }

            // TODO: implement the type check on arguments.
            inference_runner.VisitCallExpression(callExpr);
            return ((FunctionType)func_type).ReturnType;
        }

        public AstType VisitCastExpression(CastExpression castExpr)
        {
            var target_type = castExpr.ToExpression;
            var expression_type = castExpr.Target.AcceptWalker(this);
            if(IsCastable(expression_type, target_type) == TriBool.False){
                parser.ReportSemanticErrorRegional(
                    "Error ES1004: Can not cast the type `{0}` to type `{1}`.",
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
            if(IsCompatibleWith(true_type, false_type) == TriBool.False){
                parser.ReportSemanticErrorRegional(
                    "Error ES1005: `{0}` is not compatible with `{1}`.",
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
            var lower_type = intSeq.Lower.AcceptWalker(inference_runner);
            var upper_type = intSeq.Upper.AcceptWalker(inference_runner);
            var step_type = intSeq.Step.AcceptWalker(inference_runner);
            // TODO: implement uint and bigint version of the intseq type
            if(!IsSmallIntegerType(lower_type)){
                parser.ReportSemanticError(
                    "Error ES4001: `{0}` is not an `int` type! An integer sequence expression expects an `int`.",
                    intSeq.Lower,
                    lower_type
                );
            }

            if(!IsSmallIntegerType(upper_type)){
                parser.ReportSemanticError(
                    "Error ES4001: `{0}` is not an `int` type! An integer sequence expression expects an `int`.",
                    intSeq.Upper,
                    upper_type
                );
            }

            if(!IsSmallIntegerType(step_type)){
                parser.ReportSemanticError(
                    "Error ES4001: `{0}` is not an `int` type! An integer sequence expression expects an `int`.",
                    intSeq.Step,
                    step_type
                );
            }

            return new PrimitiveType("intseq", TextLocation.Empty);
        }

        public AstType VisitIndexerExpression(IndexerExpression indexExpr)
        {
            var type = indexExpr.Target.AcceptWalker(this);
            if(IsPlaceholderType(type))
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

        public AstType VisitMemberReference(MemberReferenceExpression memRef)
        {
            memRef.AcceptWalker(inference_runner);

            var type = memRef.Target.AcceptWalker(this);
            if(IsPlaceholderType(type)){
                var inferred = memRef.Target.AcceptWalker(inference_runner);
                // Do not replace the type node because ExpressoInferenceRunner has already done that
                //type.ReplaceWith(inferred);
                inference_runner.VisitMemberReference(memRef);
                type = inferred;
            }

            var type_table = symbols.GetTypeTable(type.Name);
            if(type_table != null){
                var symbol = type_table.GetSymbol(memRef.Member.Name);
                if(symbol == null){
                    // Don't report field missing error because InferenceRunner has already done that
                }else{
                    // Bind the name of the member here
                    memRef.Member.IdentifierId = symbol.IdentifierId;
                    return symbol.Type;
                }
            }

            return AstType.Null;
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
            creation.AcceptWalker(inference_runner);
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
                        "Type `{0}` doesn't have a field '{1}'.",
                        key_value.KeyExpression,
                        creation.TypePath, key_path.AsIdentifier.Name
                    );
                }else{
                    var value_type = key_value.ValueExpression.AcceptWalker(this);
                    if(IsCastable(value_type, key.Type) == TriBool.False){
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
            if(seqInitializer.ObjectType.TypeArguments.First() is PlaceholderType)
                seqInitializer.AcceptWalker(inference_runner);

            // Accepts each item as it replaces placeholder nodes with real type nodes
            // We don't validate the type of each item because inference phase do that
            AstType type = null;
            foreach(var item in seqInitializer.Items){
                if(type == null)
                    type = item.AcceptWalker(this);
                else
                    item.AcceptWalker(this);
            }

            return seqInitializer.ObjectType;
        }

        public AstType VisitMatchClause(MatchPatternClause matchClause)
        {
            AstType result = AstType.Null;
            foreach(var pattern in matchClause.Patterns){
                var tmp = pattern.AcceptWalker(this);
                result = inference_runner.FigureOutCommonType(result, tmp);
            }
            return result;
        }

        public AstType VisitSequenceExpression(SequenceExpression seqExpr)
        {
            TemporaryTypes.Clear();
            foreach(var item in seqExpr.Items)
                TemporaryTypes.Add(item.AcceptWalker(this));

            return SimpleType.Null;
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
                            "Can not apply the operator '{0}' on type `{1}`.",
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
            if(selfRef.SelfIdentifier.Type is PlaceholderType)
                return inference_runner.VisitSelfReferenceExpression(selfRef);
            else
                return selfRef.SelfIdentifier.Type;
        }

        public AstType VisitSuperReferenceExpression(SuperReferenceExpression superRef)
        {
            if(superRef.SuperIdentifier.Type is PlaceholderType)
                return inference_runner.VisitSuperReferenceExpression(superRef);
            else
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
                return PlaceholderTypeNode.Clone();
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

        public AstType VisitParameterType(ParameterType paramType)
        {
            return null;
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
                        if(IsCastable(option_type, param_type) == TriBool.False){
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
                // Descend scopes 2 times because a function name has its own scope
                int tmp_counter2 = scope_counter;
                --scope_counter;
                DescendScope();
                scope_counter = 0;

                var return_type = inference_runner.VisitFunctionDeclaration(funcDecl);
                funcDecl.ReturnType.ReplaceWith(return_type);
                AscendScope();
                scope_counter = tmp_counter2;
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
                        if(IsCastable(init_type, field_type) == TriBool.False){
                            parser.ReportSemanticErrorRegional(
                                "Error ES0110: Can not implicitly cast type `{0}` to type `{1}`.",
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
                if(IsContainerType(inferred_type) && ((SimpleType)inferred_type).TypeArguments.Any(t => t is PlaceholderType)){
                    parser.ReportSemanticErrorRegional(
                        "The left-hand-side lacks the inner type of the container `{0}`",
                        initializer.NameToken,
                        initializer.Initializer,
                        inferred_type.Name
                    );
                }
                left_type.ReplaceWith(inferred_type);
                left_type = inferred_type;
            }

            var rhs_type = initializer.Initializer.AcceptWalker(this);
            if(IsContainerType(rhs_type)){
                // The laft-hand-side lacks the types of the contents so infer them from the right-hand-side
                var lhs_simple = left_type as SimpleType;
                var rhs_simple = rhs_type as SimpleType;
                foreach(var pair in rhs_simple.TypeArguments.Zip(lhs_simple.TypeArguments,
                    (l, r) => new Tuple<AstType, AstType>(l, r))){
                    pair.Item1.ReplaceWith(pair.Item2.Clone());
                }
            }else if(rhs_type != null && IsCompatibleWith(left_type, rhs_type) == TriBool.False){
                parser.ReportSemanticErrorRegional(
                    "Type `{0}` on the left-hand-side is not compatible with `{1}` on the right-hand-side.",
                    initializer.NameToken,
                    initializer.Initializer,
                    left_type, rhs_type
                );
            }
            return left_type;
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
            var types = valueBindingPattern.Variables.Select(variable => variable.AcceptWalker(this));
            return AstType.MakeSimpleType("tuple", types, valueBindingPattern.StartLocation, valueBindingPattern.EndLocation);
        }

        public AstType VisitCollectionPattern(CollectionPattern collectionPattern)
        {
            return collectionPattern.CollectionType;
        }

        public AstType VisitDestructuringPattern(DestructuringPattern destructuringPattern)
        {
            return destructuringPattern.TypePath;
        }

        public AstType VisitTuplePattern(TuplePattern tuplePattern)
        {
            var types = 
                from p in tuplePattern.Patterns
                select p.AcceptWalker(this);
            return AstType.MakeSimpleType("tuple", types, tuplePattern.StartLocation, tuplePattern.EndLocation);
        }

        public AstType VisitExpressionPattern(ExpressionPattern exprPattern)
        {
            return exprPattern.Expression.AcceptWalker(this);
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
        /// The first is the identity cast. An identity cast casts itself.
        /// The second is the up cast. Up casts are usually valid as much as .
        /// </summary>
        /// <returns><c>true</c> if <c>fromType</c> can be casted to <c>totype</c>; otherwise, <c>false</c>.</returns>
        static TriBool IsCastable(AstType fromType, AstType toType)
        {
            if(fromType.Name == toType.Name)
                return TriBool.True;
            else
                return IsCompatibleWith(fromType, toType);
            // TODO: implement the cases of upcasts and downcasts
        }

        /// <summary>
        /// Determines if <c>first</c> is compatible with the specified <c>second</c>.
        /// </summary>
        /// <returns><c>true</c> if is compatible with the specified first second; otherwise, <c>false</c>.</returns>
        /// <param name="first">First.</param>
        /// <param name="second">Second.</param>
        static TriBool IsCompatibleWith(AstType first, AstType second)
        {
            if(first == null)
                throw new ArgumentNullException("first");
            if(second == null)
                throw new ArgumentNullException("second");

            var primitive1 = first as PrimitiveType;
            var primitive2 = second as PrimitiveType;
            if(primitive1 != null && primitive2 != null){
                if(IsNumericalType(first) && IsNumericalType(second)){
                    if(primitive1.KnownTypeCode == KnownTypeCode.Int && primitive2.KnownTypeCode == KnownTypeCode.UInt){
                        return TriBool.Intermmediate;
                    }else if(primitive1.KnownTypeCode == KnownTypeCode.Float && primitive2.KnownTypeCode == KnownTypeCode.Float){
                        return TriBool.True;
                    }else if(primitive1.KnownTypeCode == KnownTypeCode.Double && (primitive2.KnownTypeCode == KnownTypeCode.Float || primitive2.KnownTypeCode == KnownTypeCode.Double)){
                        return TriBool.True;
                    }else if((int)primitive1.KnownTypeCode >= (int)primitive2.KnownTypeCode && (int)primitive1.KnownTypeCode >= (int)KnownTypeCode.Byte
                        && (int)primitive1.KnownTypeCode <= (int)KnownTypeCode.BigInteger){
                        return TriBool.True;
                    }else{
                        return TriBool.False;
                    }
                }else if(first != AstType.Null && second != AstType.Null && primitive1.KnownTypeCode == primitive2.KnownTypeCode){
                    return TriBool.True;
                }else{
                    return TriBool.False;
                }
            }

            var simple1 = first as SimpleType;
            var simple2 = second as SimpleType;
            if(simple1 != null && simple2 != null){
                
            }

            return TriBool.False;
        }

        /// <summary>
        /// Determines if `type` is a number type.
        /// </summary>
        /// <returns><c>true</c> if `type` is a number type; otherwise, <c>false</c>.</returns>
        /// <param name="type">Type.</param>
        static bool IsNumericalType(AstType type)
        {
            return type.Name == "int" || type.Name == "uint" || type.Name == "float" || type.Name == "double" || type.Name == "bigint" || type.Name == "byte";
        }

        /// <summary>
        /// Determines if `type` a small integer type.
        /// </summary>
        /// <returns><c>true</c>, if `type` is a small integer type, <c>false</c> otherwise.</returns>
        /// <param name="type">Type.</param>
        static bool IsSmallIntegerType(AstType type)
        {
            return type.Name == "int";
        }

        /// <summary>
        /// Determines if `type` is a placeholder type.
        /// </summary>
        /// <returns><c>true</c>, if `type` is a placeholder type, <c>false</c> otherwise.</returns>
        /// <param name="type">Type.</param>
        static bool IsPlaceholderType(AstType type)
        {
            return type is PlaceholderType;
        }

        /// <summary>
        /// Determines if `type` is a container type.
        /// </summary>
        /// <returns><c>true</c>, if `type` is a container type, <c>false</c> otherwise.</returns>
        /// <param name="type">Type.</param>
        static bool IsContainerType(AstType type)
        {
            var simple = type as SimpleType;
            if(simple != null)
                return simple.Identifier == "array" || simple.Identifier == "vector" || simple.Identifier == "dictionary" || simple.Identifier == "tuple";
            else
                return false;
        }

        static bool IsSequenceType(AstType type)
        {
            var primitive = type as PrimitiveType;
            if(primitive != null && primitive.KnownTypeCode == KnownTypeCode.IntSeq)
                return true;
            else
                return IsContainerType(type);
        }

        static AstType MakeOutElementType(AstType type)
        {
            var primitive = type as PrimitiveType;
            if(primitive != null && primitive.KnownTypeCode == KnownTypeCode.IntSeq)
                return AstType.MakePrimitiveType("int");

            var simple = type as SimpleType;
            if(simple != null){
                if(simple.TypeArguments.Count == 1)
                    return simple.TypeArguments.FirstOrNullObject().Clone();
                else
                    return AstType.MakeSimpleType("tuple", simple.TypeArguments);
            }

            return AstType.Null;
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
                    ident.Name, symbols.Name
                );
            }
        }
    }
}

