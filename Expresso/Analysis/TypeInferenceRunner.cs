using System;
using System.Linq;
using ICSharpCode.NRefactory;
using ICSharpCode.NRefactory.PatternMatching;
using Expresso.TypeSystem;
using System.Collections.Generic;


namespace Expresso.Ast.Analysis
{
    public partial class TypeChecker
    {
        /// <summary>
        /// The type inference runner is responsible for inferring types when asked.
        /// It does the job by inferring and replacing old nodes with the calculated type nodes
        /// in the symbol table. 
        /// </summary>
        /// <remarks>
        /// Currently, I must say it's just a temporal implementation since it messes around the AST itself
        /// in order to keep track of type relations.
        /// </remarks>
        public class TypeInferenceRunner : IAstWalker<AstType>
        {
            Parser parser;
            TypeChecker checker;

            public TypeInferenceRunner(Parser parser, TypeChecker checker)
            {
                this.parser = parser;
                this.checker = checker;
            }

            #region IAstWalker implementation

            public AstType VisitAst(ExpressoAst ast)
            {
                throw new NotImplementedException("Can not work on that node!");
            }

            public AstType VisitBlock(BlockStatement block)
            {
                throw new NotImplementedException("Can not work on that node!");
            }

            public AstType VisitBreakStatement(BreakStatement breakStmt)
            {
                throw new NotImplementedException("Can not work on that node!");
            }

            public AstType VisitContinueStatement(ContinueStatement continueStmt)
            {
                throw new NotImplementedException("Can not work on that node!");
            }

            public AstType VisitEmptyStatement(EmptyStatement emptyStmt)
            {
                throw new NotImplementedException("Can not work on that node!");
            }

            public AstType VisitExpressionStatement(ExpressionStatement exprStmt)
            {
                throw new NotImplementedException("Can not work on that node!");
            }

            public AstType VisitForStatement(ForStatement forStmt)
            {
                throw new NotImplementedException("Can not work on that node!");
            }

            public AstType VisitIfStatement(IfStatement ifStmt)
            {
                throw new NotImplementedException("Can not work on that node!");
            }

            public AstType VisitReturnStatement(ReturnStatement returnStmt)
            {
                throw new NotImplementedException("Can not work on that node!");
            }

            public AstType VisitMatchStatement(MatchStatement matchStmt)
            {
                throw new NotImplementedException("Can not work on that node!");
            }

            public AstType VisitWhileStatement(WhileStatement whileStmt)
            {
                throw new NotImplementedException("Can not work on that node!");
            }

            public AstType VisitYieldStatement(YieldStatement yieldStmt)
            {
                throw new NotImplementedException("Can not work on that node!");
            }

            public AstType VisitVariableDeclarationStatement(VariableDeclarationStatement varDecl)
            {
                throw new NotImplementedException("Can not work on that node!");
            }

            public AstType VisitAssignment(AssignmentExpression assignment)
            {
                // In an assignment, we want to know the type of the left-hand-side
                // So let's take a look at the right-hand-side
                var right_type = assignment.Right.AcceptWalker(this);
                return right_type.Clone();
            }

            public AstType VisitBinaryExpression(BinaryExpression binaryExpr)
            {
                return FigureOutCommonType(binaryExpr.Left.AcceptWalker(this), binaryExpr.Right.AcceptWalker(this));
            }

            public AstType VisitCallExpression(CallExpression callExpr)
            {
                var func_type = callExpr.Target.AcceptWalker(this);
                return ((FunctionType)func_type).ReturnType.Clone();
            }

            public AstType VisitCastExpression(CastExpression castExpr)
            {
                return castExpr.ToExpression.Clone();
            }

            public AstType VisitComprehensionExpression(ComprehensionExpression comp)
            {
                var obj_type = comp.ObjectType;
                if(obj_type.Name == "dictionary"){
                    var key_value = comp.Item as KeyValueLikeExpression;
                    var key_type = key_value.KeyExpression.AcceptWalker(this);
                    var value_type = key_value.ValueExpression.AcceptWalker(this);
                    obj_type.TypeArguments.FirstOrNullObject().ReplaceWith(key_type);
                    obj_type.TypeArguments.LastOrNullObject().ReplaceWith(value_type);
                }else if(obj_type.Name == "array" || obj_type.Name == "vector"){
                    var element_type = comp.Item.AcceptWalker(this);
                    obj_type.TypeArguments.FirstOrNullObject().ReplaceWith(element_type);
                }else{
                    throw new InvalidOperationException("Unreachable!");
                }

                return obj_type.Clone();
            }

            public AstType VisitComprehensionForClause(ComprehensionForClause compFor)
            {
                throw new NotImplementedException("Can not work on that node!");
            }

            public AstType VisitComprehensionIfClause(ComprehensionIfClause compIf)
            {
                throw new NotImplementedException("Can not work on that node!");
            }

            public AstType VisitConditionalExpression(ConditionalExpression condExpr)
            {
                return FigureOutCommonType(condExpr.TrueExpression.AcceptWalker(this), condExpr.FalseExpression.AcceptWalker(this));
            }

            public AstType VisitKeyValueLikeExpression(KeyValueLikeExpression keyValue)
            {
                throw new NotImplementedException("Can not work on that node!");
            }

            public AstType VisitLiteralExpression(LiteralExpression literal)
            {
                return literal.Type.Clone();
            }

            public AstType VisitIdentifier(Identifier ident)
            {
                if(ident.Type is PlaceholderType){
                    var symbol = checker.symbols.GetSymbolInAnyScope(ident.Name);
                    if(symbol == null){
                        parser.ReportSemanticError(
                            "The symbol '{0}' is not defined in the current scope {1}.",
                            ident,
                            ident.Name, checker.symbols.Name
                        );
                        return null;
                    }else{
                        return symbol.Type.Clone();
                    }
                }else{
                    return ident.Type.Clone();
                }
            }

            public AstType VisitIntegerSequenceExpression(IntegerSequenceExpression intSeq)
            {
                return new PrimitiveType("intseq", TextLocation.Empty);
            }

            public AstType VisitIndexerExpression(IndexerExpression indexExpr)
            {
                var target_type = indexExpr.Target.AcceptWalker(this);
                var simple_type = target_type as SimpleType;
                if(simple_type != null){
                    //TODO: get it to work for more general types 
                    if(simple_type.Identifier != "array" && simple_type.Identifier != "dictionary"){
                        parser.ReportSemanticErrorRegional(
                            "Can not apply the indexer expression on type `{0}`",
                            indexExpr.Target, indexExpr,
                            target_type
                        );
                    }

                    return simple_type.TypeArguments.LastOrNullObject().Clone();
                }

                return target_type;
            }

            public AstType VisitMemberReference(MemberReference memRef)
            {
                var target_type = memRef.Target.AcceptWalker(this);
                var type_table = checker.symbols.GetTypeTable(target_type.Name);
                if(type_table == null){
                    parser.ReportSemanticError(
                        "Although the expression {0} is evaluated to the type `{1}`, there isn't any type with that name.",
                        memRef.Target,
                        memRef.Target, target_type.Name
                    );
                }else{
                    var symbol = type_table.GetSymbol(memRef.Member.Name);
                    if(symbol == null){
                        parser.ReportSemanticError(
                            "Type `{0}` doesn't have a field {1}!",
                            memRef.Member,
                            target_type.Name, memRef.Member.Name
                        );
                    }else{
                        return symbol.Type.Clone();
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
                    var table = checker.symbols;
                    AstType result = null;
                    foreach(var item in pathExpr.Items){
                        if(table.HasTypeSymbol(item.Name)){
                            var tmp_type = table.GetTypeSymbol(item.Name);
                            result = tmp_type.Type;
                            table = table.Children[0];
                        }else if(table.HasSymbol(item.Name)){
                            var tmp = table.GetTypeSymbol(item.Name);
                            result = tmp.Type;
                            table = table.Children[0];
                        }else{
                            throw new ParserException(
                                "Type or symbol name '{0}' is not declared",
                                item.Name
                            );
                        }
                    }

                    return result.Clone();
                }
            }

            public AstType VisitParenthesizedExpression(ParenthesizedExpression parensExpr)
            {
                return parensExpr.Expression.AcceptWalker(this);
            }

            public AstType VisitObjectCreationExpression(ObjectCreationExpression creation)
            {
                return creation.TypePath.Clone();
            }

            public AstType VisitSequenceInitializer(SequenceInitializer seqInitializer)
            {
                // If the node given represents an empty sequence
                // then we are giving up inferring the type
                if(seqInitializer.Items.Count == 0)
                    return AstType.Null;

                // The type of the elements can be seen as the most restricted type
                // between all the elements.
                if(seqInitializer.ObjectType.Identifier == "dictionary"){
                    var first_elem = seqInitializer.Items.FirstOrNullObject() as KeyValueLikeExpression;
                    if(first_elem == null)
                        throw new InvalidOperationException();

                    AstType key_type, value_type;
                    key_type = first_elem.KeyExpression.AcceptWalker(this);
                    value_type = first_elem.ValueExpression.AcceptWalker(this);
                    foreach(var item in seqInitializer.Items.Skip(1).Cast<KeyValueLikeExpression>()){
                        var tmp_key = item.KeyExpression.AcceptWalker(this);
                        var tmp_value = item.ValueExpression.AcceptWalker(this);
                        key_type = FigureOutCommonType(key_type, tmp_key);
                        value_type = FigureOutCommonType(value_type, tmp_value);
                    }
                    seqInitializer.ObjectType.TypeArguments.FirstOrNullObject().ReplaceWith(key_type);
                    seqInitializer.ObjectType.TypeArguments.LastOrNullObject().ReplaceWith(value_type);

                    return new SimpleType("dictionary", new []{
                        key_type.Clone(), value_type.Clone()
                    }, TextLocation.Empty, TextLocation.Empty);
                }else{
                    AstType first = seqInitializer.Items.FirstOrNullObject().AcceptWalker(this);
                    var result = seqInitializer.Items.Skip(1)
                        .Aggregate(first, (accum, item) => FigureOutCommonType(accum, item.AcceptWalker(this)));
                    seqInitializer.ObjectType.TypeArguments.FirstOrNullObject().ReplaceWith(result.Clone());

                    return seqInitializer.ObjectType.Clone();
                }
            }

            public AstType VisitMatchClause(MatchPatternClause matchClause)
            {
                throw new NotImplementedException("Can not work on that node!");
            }

            public AstType VisitSequence(SequenceExpression seqExpr)
            {
                // The type of the element of a sequence can be seen as the most common type
                // of the whole sequence.
                var types = 
                    from item in seqExpr.Items
                    select item.AcceptWalker(this).Clone();

                return new SimpleType("tuple", types, seqExpr.StartLocation, seqExpr.EndLocation);
            }

            public AstType VisitUnaryExpression(UnaryExpression unaryExpr)
            {
                var tmp = unaryExpr.Operand.AcceptWalker(this);
                switch(unaryExpr.Operator){
                case OperatorType.Dereference:
                    return null;

                case OperatorType.Reference:
                    return null;

                case OperatorType.Plus:
                case OperatorType.Minus:
                    {
                        var primitive_type = tmp as PrimitiveType;
                        if(primitive_type != null){
                            if(primitive_type.KnownTypeCode == KnownTypeCode.Int
                                || primitive_type.KnownTypeCode == KnownTypeCode.UInt
                                || primitive_type.KnownTypeCode == KnownTypeCode.Float
                                || primitive_type.KnownTypeCode == KnownTypeCode.Double
                                || primitive_type.KnownTypeCode == KnownTypeCode.Byte
                                || primitive_type.KnownTypeCode == KnownTypeCode.BigInteger)
                                return primitive_type;
                        }

                        parser.ReportSemanticError(
                            "Can not apply operators '+' or '-' on type `{0}`.",
                            unaryExpr,
                            tmp.Name
                        );

                        return AstType.Null;
                    }

                case OperatorType.Not:
                    return new PrimitiveType("bool", TextLocation.Empty);

                default:
                    throw new ParserException("Unknown unary operator!");
                };
            }

            public AstType VisitSelfReferenceExpression(SelfReferenceExpression selfRef)
            {
                var decls =
                    from a in selfRef.Ancestors
                        where a.NodeType == NodeType.TypeDeclaration || a.NodeType == NodeType.Member
                    let entity = a as EntityDeclaration
                    select entity;

                return decls.First().ReturnType;
            }

            public AstType VisitSuperReferenceExpression(SuperReferenceExpression superRef)
            {
                var decls =
                    from a in superRef.Ancestors
                        where a.NodeType == NodeType.TypeReference || a.NodeType == NodeType.Member
                    let entity = a as EntityDeclaration
                    select entity;

                return decls.First().ReturnType;
            }

            public AstType VisitCommentNode(CommentNode comment)
            {
                throw new NotImplementedException("Can not work on that node!");
            }

            public AstType VisitTextNode(TextNode textNode)
            {
                throw new NotImplementedException("Can not work on that node!");
            }

            public AstType VisitSimpleType(SimpleType simpleType)
            {
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
                return memberType.ChildType;
            }

            public AstType VisitFunctionType(FunctionType funcType)
            {
                return funcType.ReturnType;
            }

            public AstType VisitPlaceholderType(PlaceholderType placeholderType)
            {
                return null;
            }

            public AstType VisitAliasDeclaration(AliasDeclaration aliasDecl)
            {
                return aliasDecl.Path.AcceptWalker(this);
            }

            public AstType VisitImportDeclaration(ImportDeclaration importDecl)
            {
                throw new NotImplementedException("Can not work on that node!");
            }

            public AstType VisitFunctionDeclaration(FunctionDeclaration funcDecl)
            {
                // TODO:Try to figure out the most common type between all the types that
                // all code paths return
                AstType type = AstType.Null;
                var last = funcDecl.Body.Statements.Last() as ReturnStatement;
                if(last != null){
                    if(last.Expression.IsNull)
                        return new SimpleType("tuple", TextLocation.Empty);
                    else
                        return last.Expression.AcceptWalker(this);
                }

                return type;
            }

            public AstType VisitTypeDeclaration(TypeDeclaration typeDecl)
            {
                throw new NotImplementedException("Can not work on that node!");
            }

            public AstType VisitFieldDeclaration(FieldDeclaration fieldDecl)
            {
                throw new NotImplementedException("Can not work on that node!");
            }

            public AstType VisitParameterDeclaration(ParameterDeclaration parameterDecl)
            {
                if(parameterDecl.Option.IsNull){
                    parser.ReportSemanticErrorRegional(
                        "Can not infer the expression {0} because it doesn't have any context.",
                        parameterDecl.NameToken, parameterDecl.Option,
                        parameterDecl
                    );
                }

                var option_type = parameterDecl.Option.AcceptWalker(this);
                parameterDecl.NameToken.Type.ReplaceWith(option_type);
                return option_type;
            }

            public AstType VisitVariableInitializer(VariableInitializer initializer)
            {
                if(initializer.Initializer.IsNull){
                    parser.ReportSemanticErrorRegional(
                        "Can not infer the expression {0} because it doesn't have any context.",
                        initializer.NameToken, initializer.Initializer,
                        initializer
                    );
                }

                var init_type = initializer.Initializer.AcceptWalker(this);
                initializer.NameToken.Type.ReplaceWith(init_type);
                return init_type;
            }

            public AstType VisitWildcardPattern(WildcardPattern wildcardPattern)
            {
                return SimpleType.Null;
            }

            public AstType VisitIdentifierPattern(IdentifierPattern identifierPattern)
            {
                if(identifierPattern.InnerPattern.IsNull)
                    return identifierPattern.Identifier.AcceptWalker(this);
                else
                    return identifierPattern.InnerPattern.AcceptWalker(this);
            }

            public AstType VisitValueBindingPattern(ValueBindingPattern valueBindingPattern)
            {
                var types = valueBindingPattern.Variables.Select(variable => variable.AcceptWalker(this));
                return AstType.MakeSimpleType("tuple", types, valueBindingPattern.StartLocation, valueBindingPattern.EndLocation);
            }

            public AstType VisitTuplePattern(TuplePattern tuplePattern)
            {
                var types =
                    from pattern in tuplePattern.Patterns
                    select pattern.AcceptWalker(this).Clone();

                return new SimpleType("tuple", types, tuplePattern.StartLocation, tuplePattern.EndLocation);
            }

            public AstType VisitCollectionPattern(CollectionPattern collectionPattern)
            {
                throw new NotImplementedException();
            }

            public AstType VisitDestructuringPattern(DestructuringPattern destructuringPattern)
            {
                throw new NotImplementedException();
            }

            public AstType VisitExpressionPattern(ExpressionPattern exprPattern)
            {
                return exprPattern.Expression.AcceptWalker(this);
            }

            public AstType VisitNullNode(AstNode nullNode)
            {
                throw new NotImplementedException("Can not work on that node!");
            }

            public AstType VisitNewLine(NewLineNode newlineNode)
            {
                throw new NotImplementedException("Can not work on that node!");
            }

            public AstType VisitWhitespace(WhitespaceNode whitespaceNode)
            {
                throw new NotImplementedException("Can not work on that node!");
            }

            public AstType VisitExpressoTokenNode(ExpressoTokenNode tokenNode)
            {
                throw new NotImplementedException("Can not work on that node!");
            }

            public AstType VisitPatternPlaceholder(AstNode placeholder, ICSharpCode.NRefactory.PatternMatching.Pattern child)
            {
                throw new NotImplementedException("Can not work on that node!");
            }

            #endregion

            /// <summary>
            /// Given 2 expressions, it tries to figure out the most common type.
            /// </summary>
            /// <returns>The common type between `lhs` and `rhs`.</returns>
            internal AstType FigureOutCommonType(AstType lhs, AstType rhs)
            {
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
                    return null;
                }

                parser.ReportWarning(
                    "Can not guess the common type between `{0}` and `{1}`.",
                    lhs,
                    lhs, rhs
                );

                return null;
            }

            static bool IsNumericalType(PrimitiveType type)
            {
                return (int)Expresso.TypeSystem.KnownTypeCode.Int <= (int)type.KnownTypeCode
                    && (int)type.KnownTypeCode <= (int)Expresso.TypeSystem.KnownTypeCode.BigInteger;
            }
        }
    }
}

