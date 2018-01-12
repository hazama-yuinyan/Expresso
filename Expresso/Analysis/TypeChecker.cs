﻿using System;
using System.Collections.Generic;
using System.Linq;
using ICSharpCode.NRefactory;
using ICSharpCode.NRefactory.PatternMatching;
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
        bool inspecting_immutability = false;
        int scope_counter;
        Parser parser;
        SymbolTable symbols;  //keep a SymbolTable reference in a private field for convenience
        TypeInferenceRunner inference_runner;
        ClosureParameterInferencer closure_parameter_inferencer;

        public TypeChecker(Parser parser)
        {
            this.parser = parser;
            symbols = parser.Symbols;
            inference_runner = new TypeInferenceRunner(parser, this);
            closure_parameter_inferencer = new ClosureParameterInferencer(parser, this);
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
            if(block.IsNull)
                return null;
            
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
                parser.ReportSemanticError(
                    "Error ES1301: `{0}` isn't a sequence type! A for statement can only be used for iterating over a sequence.",
                    forStmt.Target,
                    left_type
                );
            }else{
                var elem_type = MakeOutElementType(target_type);
                left_type.ReplaceWith(elem_type);
            }

            VisitBlock(forStmt.Body);

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
                        "Error ES1301: `{0}` isn't a sequence type! A for statemant can only be used for iterating over a sequence.",
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

            VisitBlock(valueBindingForStatment.Body);

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
            VisitBlock(ifStmt.TrueBlock);
            // We can't rewrite this to VisitBlock(ifStmt.FalseBlock);
            // because doing so can continue execution even if it is null.
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

            var target_type = matchStmt.Target.AcceptWalker(this);
            foreach(var clause in matchStmt.Clauses){
                var type = clause.AcceptWalker(this);
                if(IsCompatibleWith(target_type, type) == TriBool.False){
                    parser.ReportSemanticErrorRegional(
                        "Error ES1020: Mismatched types found! Expected {0}, found {1}.",
                        matchStmt,
                        clause,
                        target_type, type
                    );
                }
            }

            AscendScope();
            scope_counter = tmp_counter + 1;
            return null;
        }

        public AstType VisitThrowStatement(ThrowStatement throwStmt)
        {
            throwStmt.CreationExpression.AcceptWalker(this);
            return null;
        }

        public AstType VisitTryStatement(TryStatement tryStmt)
        {
            tryStmt.EnclosingBlock.AcceptWalker(this);
            foreach(var clause in tryStmt.CatchClauses)
                clause.AcceptWalker(this);

            // We can't rewrite this to directly calling VisitFinally
            // because it can be null.
            tryStmt.FinallyClause.AcceptWalker(this);
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

            inspecting_immutability = true;
            var left_type = assignment.Left.AcceptWalker(this);
            inspecting_immutability = false;
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
                        parser.ReportSemanticErrorRegional(
                            "Error ES1100: There is a type mismatch; left=`{0}`, right=`{1}`",
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
            case OperatorType.Modulus:
                if(!IsNumericalType(lhs_type) || !IsNumericalType(rhs_type)){
                    parser.ReportSemanticErrorRegional(
                        "Error ES1003: Can not apply the operator '{0}' on `{1}` and `{2}`",
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
                    parser.ReportSemanticError(
                        "Error ES1010: Can not apply the operator '{0}' on the left-hand-side '{1}'",
                        binaryExpr.Left,
                        binaryExpr.OperatorToken, binaryExpr.Left
                    );
                    return null;
                }else if(rhs_primitive.KnownTypeCode == KnownTypeCode.Float || rhs_primitive.KnownTypeCode == KnownTypeCode.Double){
                    parser.ReportSemanticError(
                        "Error ES1010: Can not apply the operator '{0}' on the right-hand-side '{1}'",
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
            var func_type = callExpr.Target.AcceptWalker(this) as FunctionType;
            // Don't call inference_runner.VisitCallExpression here
            // because doing so causes VisitIdentifier to be invoked two times
            // and show the same messages twice
            if(IsPlaceholderType(func_type)){
                var inferred = inference_runner.VisitCallExpression(callExpr);
                // Don't replace nodes here because the above code does that
                //func_type.ReplaceWith(inferred);
                return inferred;
            }

            // TODO: implement the type check on arguments.
            inference_runner.VisitCallExpression(callExpr);
            if(func_type == null){
                throw new ParserException(
                    "Error ES1805: {0} turns out not to be a function.",
                    callExpr,
                    callExpr.Target.ToString()
                );
            }
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

        public AstType VisitCatchClause(CatchClause catchClause)
        {
            int tmp_counter = scope_counter;
            DescendScope();
            scope_counter = 0;

            /*if(!(catchClause.Identifier is DestructuringPattern)){
                parser.ReportSemanticError(
                    "Error ES1005: The pattern in a catch clause must be a Destructuring pattern.",
                    catchClause.Identifier
                );
            }*/
            catchClause.Identifier.AcceptWalker(this);
            VisitBlock(catchClause.Body);

            AscendScope();
            scope_counter = tmp_counter + 1;

            return null;
        }

        public AstType VisitClosureLiteralExpression(ClosureLiteralExpression closure)
        {
            bool discovered_return_type = false;
            int tmp_counter = scope_counter;
            DescendScope();
            scope_counter = 0;

            foreach(var param in closure.Parameters){
                var param_type = param.AcceptWalker(this);
                if(IsPlaceholderType(param_type))
                    closure_parameter_inferencer.VisitParameterDeclaration(param);
            }

            VisitBlock(closure.Body);

            // Delay discovering the return type because the body statements should be type-aware
            // before the return type is started to be inferred
            if(IsPlaceholderType(closure.ReturnType)){
                // Because VisitBlock has made scope_counter one step forward
                --scope_counter;
                // Descend scopes 2 times because closure parameters have their own scope
                int tmp_counter2 = scope_counter;
                DescendScope();
                scope_counter = 0;

                inference_runner.InspectsClosure = false;
                var func_type = (FunctionType)inference_runner.VisitClosureLiteralExpression(closure);
                closure.ReturnType.ReplaceWith(func_type.ReturnType.Clone());

                AscendScope();
                scope_counter = tmp_counter2;
                discovered_return_type = true;
            }

            if(closure.LiftedIdentifiers == null){
                // The same reason as if(IsPlaceholderType(closure.ReturnType) block
                if(!discovered_return_type)
                    --scope_counter;

                /*int tmp_counter3 = scope_counter;
                DescendScope();
                scope_counter = 0;*/

                var inspecter = new ClosureInspecter(parser, this, closure);
                inspecter.VisitClosureLiteralExpression(closure);

                /*AscendScope();
                scope_counter = tmp_counter3;*/
            }

            var param_types = 
                from p in closure.Parameters
                                 select p.ReturnType.Clone();

            AscendScope();
            scope_counter = inference_runner.InspectsClosure ? tmp_counter + 1 : tmp_counter;
            
            return AstType.MakeFunctionType("closure", closure.ReturnType.Clone(), param_types);
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

        public AstType VisitFinallyClause(FinallyClause finallyClause)
        {
            return VisitBlock(finallyClause.Body);
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
            // Infer and spread the type of the identifier to this node
            inference_runner.VisitIdentifier(ident);
            if(inspecting_immutability && ident.Modifiers.HasFlag(Modifiers.Immutable)){
                parser.ReportSemanticError(
                    "Error ES1900: Re-assignment on an immutable variable '{0}'",
                    ident,
                    ident.Name
                );
            }
            return ident.Type;
        }

        public AstType VisitIntegerSequenceExpression(IntegerSequenceExpression intSeq)
        {
            var lower_type = intSeq.Lower.AcceptWalker(inference_runner);
            var upper_type = intSeq.Upper.AcceptWalker(inference_runner);
            var step_type = intSeq.Step.AcceptWalker(inference_runner);
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

            return AstType.MakePrimitiveType("intseq", intSeq.StartLocation);
        }

        public AstType VisitIndexerExpression(IndexerExpression indexExpr)
        {
            var type = indexExpr.Target.AcceptWalker(this);
            if(IsPlaceholderType(type)){
                var return_type = inference_runner.VisitIndexerExpression(indexExpr);
                return return_type;
            }

            if(type is SimpleType simple_type){
                if(simple_type.Name != "array" && simple_type.Name != "vector" && simple_type.Name != "dictionary"){
                    parser.ReportSemanticError(
                        "Error ES3011: Can not apply the indexer operator on type `{0}`",
                        indexExpr,
                        simple_type
                    );
                }

                if(indexExpr.Arguments.Count == 1){
                    var arg_type = indexExpr.Arguments.First().AcceptWalker(this);
                    if(arg_type is PrimitiveType primitive && primitive.KnownTypeCode == KnownTypeCode.IntSeq){
                        if(simple_type.Identifier == "dictionary"){
                            parser.ReportSemanticError(
                                "Error ES3012: Can not apply the indexer operator on a dictionary with an `intseq`",
                                indexExpr
                            );
                            return null;
                        }

                        // simple_type doesn't need to be cloned because it's already cloned
                        return AstType.MakeSimpleType("slice", new []{simple_type.Clone(), simple_type.TypeArguments.First().Clone()});
                    }
                }

                return simple_type.TypeArguments.Last();
            }
            return null;
        }

        public AstType VisitMemberReference(MemberReferenceExpression memRef)
        {
            inference_runner.VisitMemberReference(memRef);

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
                    // Don't bind the name of the member here because InferenceRunner has already done that
                    //memRef.Member.IdentifierId = symbol.IdentifierId;
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
                //inference_runner.VisitPathExpression(pathExpr);

                var old_table = symbols;
                AstType result = null;
                foreach(var item in pathExpr.Items){
                    var tmp_table = symbols.GetTypeTable(item.Name);
                    if(tmp_table == null)
                        result = VisitIdentifier(item);
                    else
                        symbols = tmp_table;
                }

                symbols = old_table;
                return result;
            }
        }

        public AstType VisitParenthesizedExpression(ParenthesizedExpression parensExpr)
        {
            var type = parensExpr.Expression.AcceptWalker(this);
            if(TemporaryTypes.Any()){
                var types = 
                    from t in TemporaryTypes
                    select t.Clone();
                type = AstType.MakeSimpleType("tuple", types, parensExpr.StartLocation, parensExpr.EndLocation);
            }

            return type;
        }

        public AstType VisitObjectCreationExpression(ObjectCreationExpression creation)
        {
            creation.AcceptWalker(inference_runner);
            var type_table = symbols.GetTypeTable(creation.TypePath.IdentifierNode.Name);
            if(type_table == null){
                parser.ReportSemanticError(
                    "Error ES1500: Type `{0}` isn't found or accessible from the scope {1}.",
                    creation,
                    creation.TypePath, symbols.Name
                );
                return null;
            }

            foreach(var key_value in creation.Items){
                var key_path = key_value.KeyExpression as PathExpression;
                if(key_path == null)
                    throw new InvalidOperationException();

                var key = type_table.GetSymbol(key_path.AsIdentifier.Name);
                if(key == null){
                    parser.ReportSemanticError(
                        "Error ES1501: Type `{0}` doesn't have a field named '{1}'.",
                        key_value.KeyExpression,
                        creation.TypePath, key_path.AsIdentifier.Name
                    );
                }else{
                    var value_type = key_value.ValueExpression.AcceptWalker(this);
                    if(IsCastable(value_type, key.Type) == TriBool.False){
                        parser.ReportSemanticErrorRegional(
                            "Error ES2000: The field '{0}' expects the value to be of type `{1}`, but it actually is `{2}`.",
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
            int tmp_counter = scope_counter;
            DescendScope();
            scope_counter = 0;

            AstType result = AstType.Null;
            foreach(var pattern in matchClause.Patterns){
                var tmp = pattern.AcceptWalker(this);
                result = FigureOutCommonType(result, tmp);
            }

            matchClause.Body.AcceptWalker(this);

            AscendScope();
            scope_counter = tmp_counter + 1;
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
                            "Error ES1201: Can not apply the operator '{0}' on type `{1}`.",
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
                        "Error ES1200: Can not apply the '!' operator on type `{0}`!\nThe operand must be of type `bool`.",
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
            // return the statically defined placeholder type node to indicate that it needs to be inferred
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
            var module_type = AstType.MakeSimpleType(importDecl.ModuleName);
            importDecl.ModuleNameToken.Type.ReplaceWith(module_type);

            if(!importDecl.AliasNameToken.IsNull){
                var module_type2 = importDecl.ModuleNameToken.AcceptWalker(this);
                importDecl.AliasNameToken.IdentifierId = module_type2.IdentifierNode.IdentifierId;
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
                                "Error ES1100: Type mismatch; `{0}` is not compatible with `{1}`.",
                                param.NameToken, param.Option,
                                option_type, param_type
                            );
                        }
                    }
                }
            }

            // In case of recursive calls, we first discover the function type.
            if(IsPlaceholderType(funcDecl.NameToken.Type)){
                var param_types =
                    from param in funcDecl.Parameters
                    select param.ReturnType.Clone();

                var return_type = funcDecl.ReturnType.Clone();
                var func_type = AstType.MakeFunctionType(funcDecl.Name, return_type, param_types);
                funcDecl.NameToken.Type.ReplaceWith(func_type);
            }

            if(funcDecl.Name == "main"){
                var next = funcDecl.GetNextNode();
                if(next != null && next is FunctionDeclaration){
                    parser.ReportSemanticError(
                        "Error ES1100: Can't define functions after the main function.",
                        next
                    );
                }
            }

            VisitBlock(funcDecl.Body);

            // Delay discovering the return type because the body statements should be type-aware
            // before the return type is started to be inferred
            if(IsPlaceholderType(funcDecl.ReturnType)){
                if(funcDecl.Parent is TypeDeclaration type_decl && type_decl.TypeKind == ClassType.Interface){
                    throw new ParserException(
                        "Error ES1602: The method signature '{0}' in an interface must make the return type explicit.",
                        funcDecl,
                        funcDecl.Name
                    );
                }

                if(funcDecl.Body.Statements.Count == 0){
                    throw new ParserException(
                        "Error ES1901: Can not infer the return type of '{0}' because the body is empty!",
                        funcDecl,
                        funcDecl.Name
                    );
                }
                // Descend scopes 2 times because a function name has its own scope
                int tmp_counter2 = scope_counter;
                --scope_counter;
                DescendScope();
                scope_counter = 0;

                var return_type = inference_runner.VisitFunctionDeclaration(funcDecl);
                funcDecl.ReturnType.ReplaceWith(return_type);
                // Replace the return type of the function type with an appropriate ast type object
                ((FunctionType)funcDecl.NameToken.Type).ReturnType.ReplaceWith(funcDecl.ReturnType.Clone());

                AscendScope();
                scope_counter = tmp_counter2;
            }

            if(funcDecl.Parent is TypeDeclaration){
                var type_name = ((TypeDeclaration)funcDecl.Parent).Name;
                if(funcDecl.Parameters.Any(p => p.ReturnType is SimpleType simple_type && simple_type.Name == type_name)){
                    parser.ReportSemanticError(
                        "Error ES1020: In Expresso you can't define a method that takes the self class as a parameter that contains the method.\nUse module-level functions instead.",
                        funcDecl.Parameters.Where(p => p.ReturnType is SimpleType simple_type && simple_type.Name == type_name).First()
                    );
                }else if(funcDecl.ReturnType is SimpleType simple_type && simple_type.Name == type_name){
                    parser.ReportSemanticError(
                        "Error ES1021: In Expresso you can't define a method that returns the self class that contains the method.\nUse module-level functions instead.",
                        funcDecl.ReturnType
                    );
                }
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

            var require_methods = new List<string>();

            foreach(var super_type in typeDecl.BaseTypes){
                var super_type_table = symbols.GetTypeTable(super_type.Name);
                if(super_type_table.TypeKind == ClassType.Interface)
                    require_methods.AddRange(super_type_table.Symbols.Select(s => s.Name));
                
                super_type.AcceptWalker(this);
            }

            while(require_methods.Contains("self"))
                require_methods.Remove("self");

            foreach(var member in typeDecl.Members){
                if(member is FunctionDeclaration method)
                    require_methods.Remove(method.Name);
                
                member.AcceptWalker(this);
            }

            if(require_methods.Any()){
                foreach(var require_method_name in require_methods){
                    parser.ReportSemanticError(
                        "Error ES1900: The class '{0}' doesn't implement '{1}' but an interface requires you to implement it.",
                        typeDecl,
                        typeDecl.Name, require_method_name
                    );
                }                          
            }

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
                inference_runner.InspectsClosure = true;
                var inferred_type = initializer.Initializer.AcceptWalker(inference_runner);
                inference_runner.InspectsClosure = true;
                if(IsCollectionType(inferred_type) && ((SimpleType)inferred_type).TypeArguments.Any(t => t is PlaceholderType)){
                    parser.ReportSemanticErrorRegional(
                        "Error ES1302: The left-hand-side lacks the inner type of the container `{0}`",
                        initializer.NameToken,
                        initializer.Initializer,
                        inferred_type.Name
                    );
                }
                left_type.ReplaceWith(inferred_type);
                left_type = inferred_type;
            }

            var rhs_type = initializer.Initializer.AcceptWalker(this);
            if(IsCollectionType(left_type) && ContainsPlaceholderType(left_type as SimpleType) || IsCollectionType(rhs_type) && ContainsPlaceholderType(rhs_type as SimpleType)){
                // The laft-hand-side lacks the types of the contents so infer them from the right-hand-side
                // Or the right-hand-side contains some placeholders, so infer them
                var lhs_simple = left_type as SimpleType;
                var rhs_simple = rhs_type as SimpleType;
                foreach(var pair in rhs_simple.TypeArguments.Zip(lhs_simple.TypeArguments,
                    (l, r) => new Tuple<AstType, AstType>(l, r))){
                    pair.Item1.ReplaceWith(pair.Item2.Clone());
                }
            }else if(rhs_type != null && IsCompatibleWith(left_type, rhs_type) == TriBool.False){
                parser.ReportSemanticErrorRegional(
                    "Error ES1300: Type `{0}` on the left-hand-side is not compatible with `{1}` on the right-hand-side.",
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
            var type = identifierPattern.AcceptWalker(inference_runner);
            identifierPattern.Identifier.Type.ReplaceWith(type);
            identifierPattern.InnerPattern.AcceptWalker(this);
            return type;
        }

        public AstType VisitValueBindingPattern(ValueBindingPattern valueBindingPattern)
        {
            var types = valueBindingPattern.Variables.Select(variable => variable.AcceptWalker(this));
            return AstType.MakeSimpleType("tuple", types, valueBindingPattern.StartLocation, valueBindingPattern.EndLocation);
        }

        public AstType VisitCollectionPattern(CollectionPattern collectionPattern)
        {
            AstType item_type = null;
            foreach(var item in collectionPattern.Items){
                var type = item.AcceptWalker(this);
                if(item_type == null && !type.IsNull)
                    item_type = type.Clone();
            }

            collectionPattern.CollectionType
                             .TypeArguments
                             .First()
                             .ReplaceWith(item_type);
            return collectionPattern.CollectionType;
        }

        public AstType VisitDestructuringPattern(DestructuringPattern destructuringPattern)
        {
            foreach(var item in destructuringPattern.Items)
                item.AcceptWalker(this);
            
            return destructuringPattern.TypePath;
        }

        public AstType VisitTuplePattern(TuplePattern tuplePattern)
        {
            var types = 
                from p in tuplePattern.Patterns
                select p.AcceptWalker(this).Clone();
            // TODO: consider the case that the tuple contains an IgnoringRestPattern
            return AstType.MakeSimpleType("tuple", types, tuplePattern.StartLocation, tuplePattern.EndLocation);
        }

        public AstType VisitExpressionPattern(ExpressionPattern exprPattern)
        {
            return exprPattern.Expression.AcceptWalker(this);
        }

        public AstType VisitIgnoringRestPattern(IgnoringRestPattern restPattern)
        {
            return SimpleType.Null;
        }

        public AstType VisitKeyValuePattern(KeyValuePattern keyValuePattern)
        {
            return keyValuePattern.Value.AcceptWalker(this);
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
        /// The first is the identity cast. An identity cast casts itself to itself.
        /// The second is the up cast. Up casts are usually valid as long as the types are derived.
        /// And the third is the down cast. Down casts are valid if and only if the target type is derived from the current type.
        /// </summary>
        /// <returns><c>true</c> if <c>fromType</c> can be casted to <c>totype</c>; otherwise, <c>false</c>.</returns>
        static TriBool IsCastable(AstType fromType, AstType toType)
        {
            if(fromType.Name == toType.Name)
                return TriBool.True;
            else
                return IsCompatibleWith(fromType, toType);
        }

        /// <summary>
        /// Determines whether <c>first</c> is compatible with the specified <c>second</c>.
        /// </summary>
        /// <returns><c>true</c> if is compatible with the specified first second; otherwise, <c>false</c>.</returns>
        /// <param name="first">First.</param>
        /// <param name="second">Second.</param>
        static TriBool IsCompatibleWith(AstType first, AstType second)
        {
            if(first == null)
                throw new ArgumentNullException(nameof(first));
            if(second == null)
                throw new ArgumentNullException(nameof(second));

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
                }else if(primitive1.IsMatch(primitive2)){
                    return TriBool.True;
                }else{
                    return TriBool.False;
                }
            }

            var simple1 = first as SimpleType;
            var simple2 = second as SimpleType;
            if(simple1 != null && simple2 != null){
                //TODO: implement it
                if(simple1.IsMatch(simple2)){
                    return TriBool.True;
                }
            }

            if(simple2 != null && simple2.IsNull){
                // This indicates that the right-hand-side represents, say, the wildcard pattern
                return TriBool.True;
            }

            return TriBool.False;
        }

        /// <summary>
        /// Given 2 expressions, it tries to figure out the most common type.
        /// </summary>
        /// <returns>The common type between `lhs` and `rhs`.</returns>
        AstType FigureOutCommonType(AstType lhs, AstType rhs)
        {
            if(lhs == null)
                throw new ArgumentNullException(nameof(lhs));

            if(rhs == null)
                throw new ArgumentNullException(nameof(rhs));
            
            if(lhs.IsNull)
                return rhs;

            if(rhs.IsNull)
                return lhs;

            var lhs_primitive = lhs as PrimitiveType;
            var rhs_primitive = rhs as PrimitiveType;
            if(lhs_primitive != null && rhs_primitive != null){
                // If both are primitives, check first if both are exactly the same type
                if(lhs_primitive.KnownTypeCode == rhs_primitive.KnownTypeCode){
                    return lhs_primitive;
                }else if(IsNumericalType(lhs_primitive) && IsNumericalType(rhs_primitive)){
                    // If not, then check if both are numeric types or not
                    var common_typecode = lhs_primitive.KnownTypeCode | rhs_primitive.KnownTypeCode;
                    return new PrimitiveType(common_typecode.ToString().ToLower(), TextLocation.Empty);
                }else{
                    // If both aren't the case, then we must say there is no common types between these 2 expressions
                    return AstType.Null;
                }
            }

            var lhs_simple = lhs as SimpleType;
            var rhs_simple = rhs as SimpleType;
            if(lhs_simple != null && rhs_simple != null){
                if(lhs_simple.IsMatch(rhs_simple)){
                    return lhs_simple;
                }
            }

            parser.ReportWarning(
                "Warning ES1200: Can not guess the common type between `{0}` and `{1}`.",
                lhs,
                lhs, rhs
            );

            return null;
        }

        /// <summary>
        /// Determines whether `type` is a number type.
        /// </summary>
        /// <returns><c>true</c> if `type` is a number type; otherwise, <c>false</c>.</returns>
        /// <param name="type">Type.</param>
        static bool IsNumericalType(AstType type)
        {
            return type.Name == "int" || type.Name == "uint" || type.Name == "float" || type.Name == "double" || type.Name == "bigint" || type.Name == "byte";
        }

        /// <summary>
        /// Determines whether `type` a small integer type.
        /// </summary>
        /// <returns><c>true</c>, if `type` is a small integer type, <c>false</c> otherwise.</returns>
        /// <param name="type">Type.</param>
        static bool IsSmallIntegerType(AstType type)
        {
            return type.Name == "int";
        }

        /// <summary>
        /// Determines whether `type` is a placeholder type.
        /// </summary>
        /// <returns><c>true</c>, if `type` is a placeholder type, <c>false</c> otherwise.</returns>
        /// <param name="type">Type.</param>
        static bool IsPlaceholderType(AstType type)
        {
            return type is PlaceholderType;
        }

        /// <summary>
        /// Determines whether `type` is a collection type.
        /// </summary>
        /// <returns><c>true</c>, if `type` is a collection type, <c>false</c> otherwise.</returns>
        /// <param name="type">Type.</param>
        static bool IsCollectionType(AstType type)
        {
            var simple = type as SimpleType;
            if(simple != null)
                return simple.Identifier == "array" || simple.Identifier == "vector" || simple.Identifier == "dictionary" || simple.Identifier == "tuple" || simple.Identifier == "slice";
            else
                return false;
        }

        /// <summary>
        /// Determines whether the `type` represents the tuple type
        /// </summary>
        /// <returns><c>true</c>, if `type` represents the tuple type, <c>false</c> otherwise.</returns>
        /// <param name="type">Type.</param>
        static bool IsTupleType(AstType type)
        {
            var simple = type as SimpleType;
            if(simple != null)
                return simple.Identifier == "tuple";
            else
                return false;
        }

        /// <summary>
        /// Determines whether the `type` represents a container type.
        /// </summary>
        /// <returns><c>true</c>, if `type` represents a container type, <c>false</c> otherwise.</returns>
        /// <param name="type">Type.</param>
        static bool IsContainerType(AstType type)
        {
            var simple = type as SimpleType;
            if(simple != null)
                return simple.Identifier == "array" || simple.Identifier == "vector";
            else
                return false;
        }

        /// <summary>
        /// Determines whether the `type` represents the dictionary type.
        /// </summary>
        /// <returns><c>true</c>, if `type` represents the dictionary type, <c>false</c> otherwise.</returns>
        /// <param name="type">Type.</param>
        static bool IsDictionaryType(AstType type)
        {
            var simple = type as SimpleType;
            if(simple != null)
                return simple.Identifier == "dictionary";
            else
                return false;
        }

        /// <summary>
        /// Determines whether `type` is a container type and whether it contains a placeholder type.
        /// </summary>
        /// <returns><c>true</c>, if placeholder type was contained, <c>false</c> otherwise.</returns>
        /// <param name="type">Type.</param>
        static bool ContainsPlaceholderType(SimpleType type)
        {
            if(type == null || !IsCollectionType(type))
                return false;
            
            return type.TypeArguments.Any(t => IsPlaceholderType(t));
        }

        /// <summary>
        /// Determines whether `type` is the void type.
        /// </summary>
        /// <returns><c>true</c>, if `type` is the void type, <c>false</c> otherwise.</returns>
        /// <param name="type">Type.</param>
        static bool IsVoidType(AstType type)
        {
            if(type is SimpleType simple)
                return simple.Name == "tuple" && !simple.TypeArguments.Any();
            else
                return false;
        }

        static bool IsSequenceType(AstType type)
        {
            var primitive = type as PrimitiveType;
            if(primitive != null && primitive.KnownTypeCode == KnownTypeCode.IntSeq)
                return true;
            else
                return IsCollectionType(type);
        }

        static AstType MakeOutElementType(AstType type)
        {
            var primitive = type as PrimitiveType;
            if(primitive != null && primitive.KnownTypeCode == KnownTypeCode.IntSeq)
                return AstType.MakePrimitiveType("int");

            var simple = type as SimpleType;
            if(simple != null){
                if(simple.Identifier == "slice")
                    return simple.TypeArguments.Last().Clone();
                else if(simple.TypeArguments.Count == 1)
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
                    "Error ES0101: Type name `{0}` turns out not to be declared in the current scope {1}!",
                    ident,
                    ident.Name, symbols.Name
                );
            }
        }
    }
}

