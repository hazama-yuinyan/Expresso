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
                throw new InvalidOperationException("Can not work on that node!");
            }

            public AstType VisitBlock(BlockStatement block)
            {
                throw new InvalidOperationException("Can not work on that node!");
            }

            public AstType VisitBreakStatement(BreakStatement breakStmt)
            {
                throw new InvalidOperationException("Can not work on that node!");
            }

            public AstType VisitContinueStatement(ContinueStatement continueStmt)
            {
                throw new InvalidOperationException("Can not work on that node!");
            }

            public AstType VisitEmptyStatement(EmptyStatement emptyStmt)
            {
                throw new InvalidOperationException("Can not work on that node!");
            }

            public AstType VisitExpressionStatement(ExpressionStatement exprStmt)
            {
                throw new InvalidOperationException("Can not work on that node!");
            }

            public AstType VisitForStatement(ForStatement forStmt)
            {
                throw new InvalidOperationException("Can not work on that node!");
            }

            public AstType VisitValueBindingForStatement(ValueBindingForStatement valueBindingForStatment)
            {
                throw new InvalidOperationException("Can not work on that node!");
            }

            public AstType VisitIfStatement(IfStatement ifStmt)
            {
                throw new InvalidOperationException("Can not work on that node!");
            }

            public AstType VisitReturnStatement(ReturnStatement returnStmt)
            {
                throw new InvalidOperationException("Can not work on that node!");
            }

            public AstType VisitMatchStatement(MatchStatement matchStmt)
            {
                return matchStmt.Target.AcceptWalker(this);
            }

            public AstType VisitThrowStatement(ThrowStatement throwStmt)
            {
                throw new InvalidOperationException("Can not work on that node!");
            }

            public AstType VisitTryStatement(TryStatement tryStmt)
            {
                throw new InvalidOperationException("Can not work on that node!");
            }

            public AstType VisitWhileStatement(WhileStatement whileStmt)
            {
                throw new InvalidOperationException("Can not work on that node!");
            }

            public AstType VisitYieldStatement(YieldStatement yieldStmt)
            {
                throw new InvalidOperationException("Can not work on that node!");
            }

            public AstType VisitVariableDeclarationStatement(VariableDeclarationStatement varDecl)
            {
                throw new InvalidOperationException("Can not work on that node!");
            }

            public AstType VisitAssignment(AssignmentExpression assignment)
            {
                // In an assignment, we usually see variables on the left-hand-side.
                // So let's take a look at the left-hand-side
                var type = assignment.Left.AcceptWalker(this);
                // In a compound assignment, we could see variables on the right-hand-side
                assignment.Right.AcceptWalker(this);
                return type.Clone();
            }

            public AstType VisitBinaryExpression(BinaryExpression binaryExpr)
            {
                return checker.FigureOutCommonType(binaryExpr.Left.AcceptWalker(this), binaryExpr.Right.AcceptWalker(this));
            }

            public AstType VisitCallExpression(CallExpression callExpr)
            {
                var func_type = callExpr.Target.AcceptWalker(this);
                foreach(var arg in callExpr.Arguments)
                    arg.AcceptWalker(this);
                
                return ((FunctionType)func_type).ReturnType.Clone();
            }

            public AstType VisitCastExpression(CastExpression castExpr)
            {
                return castExpr.ToExpression.Clone();
            }

            public AstType VisitCatchClause(CatchClause catchClause)
            {
                return catchClause.Pattern.AcceptWalker(this);
            }

            public AstType VisitClosureLiteralExpression(ClosureLiteralExpression closure)
            {
                AstType type = AstType.Null;
                var last = closure.Body.Statements.Last() as ReturnStatement;
                if(last != null)
                    type = last.Expression.IsNull ? AstType.MakeSimpleType("tuple", TextLocation.Empty) : last.Expression.AcceptWalker(this);
                else
                    type = AstType.MakeSimpleType("tuple", TextLocation.Empty);

                return type;
            }

            public AstType VisitComprehensionExpression(ComprehensionExpression comp)
            {
                int tmp_counter = checker.scope_counter;
                checker.DescendScope();
                checker.scope_counter = 0;

                var obj_type = comp.ObjectType;
                comp.Body.AcceptWalker(this);

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

                checker.AscendScope();
                checker.scope_counter = tmp_counter + 1;

                return obj_type.Clone();
            }

            public AstType VisitComprehensionForClause(ComprehensionForClause compFor)
            {
                var inferred = compFor.Target.AcceptWalker(this);
                var inferred_primitive = inferred as PrimitiveType;
                if(inferred_primitive != null){
                    // See if it is a IntSeq object
                    if(inferred_primitive.KnownTypeCode != KnownTypeCode.IntSeq){
                        parser.ReportSemanticError(
                            "Error ES1301: '{0}' isn't a sequence type! A comprehension expects a sequence object.",
                            inferred_primitive,
                            inferred_primitive
                        );
                    }

                    inferred = new PrimitiveType("int", inferred_primitive.StartLocation);
                }

                var inferred_simple = inferred as SimpleType;
                if(inferred_simple != null){
                    // See if it is a sequence object like array or vector
                    if(inferred_simple.Name != "array" && inferred_simple.Name != "vector"){
                        parser.ReportSemanticError(
                            "Error ES1301: '{0}' isn't a sequence type! A comprehension expects a sequence object.",
                            inferred_simple,
                            inferred_simple
                        );
                    }

                    inferred = inferred_simple.TypeArguments.FirstOrDefault();
                }

                var identifier_ptn = compFor.Left as IdentifierPattern;
                if(identifier_ptn != null)
                    identifier_ptn.Identifier.Type.ReplaceWith(inferred);

                if(compFor.Body != null)
                    compFor.Body.AcceptWalker(this);
                
                return inferred;
            }

            public AstType VisitComprehensionIfClause(ComprehensionIfClause compIf)
            {
                if(compIf.Body != null)
                    compIf.Body.AcceptWalker(this);

                return AstType.Null;
            }

            public AstType VisitConditionalExpression(ConditionalExpression condExpr)
            {
                return checker.FigureOutCommonType(condExpr.TrueExpression.AcceptWalker(this), condExpr.FalseExpression.AcceptWalker(this));
            }

            public AstType VisitKeyValueLikeExpression(KeyValueLikeExpression keyValue)
            {
                throw new InvalidOperationException("Can not work on that node!");
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
                            "Error ES1000: The symbol '{0}' is not defined in the current scope {1}.",
                            ident,
                            ident.Name, checker.symbols.Name
                        );
                        return null;
                    }else{
                        var cloned = symbol.Type.Clone();
                        ident.Type.ReplaceWith(cloned);
                        return symbol.Type.Clone();
                    }
                }else{
                    return ident.Type.Clone();
                }
            }

            public AstType VisitIntegerSequenceExpression(IntegerSequenceExpression intSeq)
            {
                return AstType.MakePrimitiveType("intseq", intSeq.StartLocation);
            }

            public AstType VisitIndexerExpression(IndexerExpression indexExpr)
            {
                var target_type = indexExpr.Target.AcceptWalker(this);
                var simple_type = target_type as SimpleType;
                if(simple_type != null){
                    //TODO: get it to work for more general types 
                    if(simple_type.Identifier != "array" && simple_type.Identifier != "dictionary" && simple_type.Identifier != "vector"){
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

            public AstType VisitMemberReference(MemberReferenceExpression memRef)
            {
                var target_type = memRef.Target.AcceptWalker(this);
                var type_table = checker.symbols.GetTypeTable(target_type.Name);
                if(type_table == null){
                    parser.ReportSemanticError(
                        "Error ES2000: Although the expression '{0}' is evaluated to the type `{1}`, there isn't any type with that name.",
                        memRef.Target,
                        memRef.Target, target_type.Name
                    );
                }else{
                    var symbol = type_table.GetSymbol(memRef.Member.Name);
                    if(symbol == null){
                        parser.ReportSemanticError(
                            "Error ES2001: Type `{0}` doesn't have a field {1}!",
                            memRef.Member,
                            target_type.Name, memRef.Member.Name
                        );
                    }else{
                        var type = symbol.Type.Clone();
                        memRef.Member.Type.ReplaceWith(type);
                        return type;
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
                    AstType result = AstType.Null;
                    foreach(var item in pathExpr.Items){
                        if(table.HasTypeSymbolInAnyScope(item.Name)){
                            var tmp_type = table.GetTypeSymbolInAnyScope(item.Name);
                            result = tmp_type.Type;
                            table = table.GetTypeTable(item.Name);
                        }else if(table.HasSymbolInAnyScope(item.Name)){
                            var tmp = table.GetSymbolInAnyScope(item.Name);
                            result = tmp.Type.Clone();
                            item.Type.ReplaceWith(result);
                        }else{
                            throw new ParserException(
                                "Type or symbol name '{0}' is not declared",
                                item,
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
                foreach(var keyvalue in creation.Items){
                    // Walk through creation.Items in order to make them type-aware
                    keyvalue.ValueExpression.AcceptWalker(this);
                }
                return creation.TypePath.Clone();
            }

            public AstType VisitSequenceInitializer(SequenceInitializer seqInitializer)
            {
                // If the node given represents an empty sequence
                // then we are giving up inferring the type of the elements
                if(seqInitializer.Items.Count == 0)
                    return seqInitializer.ObjectType;

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
                        key_type = checker.FigureOutCommonType(key_type, tmp_key);
                        value_type = checker.FigureOutCommonType(value_type, tmp_value);
                    }
                    seqInitializer.ObjectType.TypeArguments.FirstOrNullObject().ReplaceWith(key_type);
                    seqInitializer.ObjectType.TypeArguments.LastOrNullObject().ReplaceWith(value_type);

                    return new SimpleType("dictionary", new []{
                        key_type.Clone(), value_type.Clone()
                    }, TextLocation.Empty, TextLocation.Empty);
                }else{
                    AstType first = seqInitializer.Items.FirstOrNullObject().AcceptWalker(this);
                    var result = seqInitializer.Items.Skip(1)
                        .Aggregate(first, (accum, item) => checker.FigureOutCommonType(accum, item.AcceptWalker(this)));
                    seqInitializer.ObjectType.TypeArguments.FirstOrNullObject().ReplaceWith(result.Clone());

                    return seqInitializer.ObjectType.Clone();
                }
            }

            public AstType VisitMatchClause(MatchPatternClause matchClause)
            {
                return matchClause.Parent.AcceptWalker(this);
            }

            public AstType VisitSequenceExpression(SequenceExpression seqExpr)
            {
                // The type of the element of a sequence can be seen as the most common type
                // of the whole sequence.
                var types = 
                    from item in seqExpr.Items
                    select item.AcceptWalker(this).Clone();

                return (seqExpr.Items.Count == 1) ? types.First() : new SimpleType("tuple", types, seqExpr.StartLocation, seqExpr.EndLocation);
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
                    throw new ParserException("Unknown unary operator!", unaryExpr);
                };
            }

            public AstType VisitSelfReferenceExpression(SelfReferenceExpression selfRef)
            {
                var decls =
                    from a in selfRef.Ancestors
                        where a.NodeType == NodeType.TypeDeclaration
                    let entity = a as EntityDeclaration
                    select entity;

                return decls.First().ReturnType.Clone();
            }

            public AstType VisitSuperReferenceExpression(SuperReferenceExpression superRef)
            {
                var decls =
                    from a in superRef.Ancestors
                        where a.NodeType == NodeType.TypeDeclaration
                    let type = a as TypeDeclaration
                        where type != null
                    from pt in type.BaseTypes
                    select pt as AstType;

                return decls.First().Clone();
            }

            public AstType VisitCommentNode(CommentNode comment)
            {
                throw new InvalidOperationException("Can not work on that node!");
            }

            public AstType VisitTextNode(TextNode textNode)
            {
                throw new InvalidOperationException("Can not work on that node!");
            }

            public AstType VisitSimpleType(SimpleType simpleType)
            {
                return simpleType.Clone();
            }

            public AstType VisitPrimitiveType(PrimitiveType primitiveType)
            {
                return primitiveType.Clone();
            }

            public AstType VisitReferenceType(ReferenceType referenceType)
            {
                return referenceType.BaseType.Clone();
            }

            public AstType VisitMemberType(MemberType memberType)
            {
                return memberType.ChildType.Clone();
            }

            public AstType VisitFunctionType(FunctionType funcType)
            {
                return funcType.ReturnType.Clone();
            }

            public AstType VisitParameterType(ParameterType paramType)
            {
                return paramType.Clone();
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
                throw new InvalidOperationException("Can not work on that node!");
            }

            public AstType VisitFunctionDeclaration(FunctionDeclaration funcDecl)
            {
                // TODO: Try to figure out the most common type between all the types that
                // all code paths return
                AstType type = AstType.Null;
                var last = funcDecl.Body.Statements.Last() as ReturnStatement;
                if(last != null)
                    type = last.Expression.IsNull ? AstType.MakeSimpleType("tuple", TextLocation.Empty) : last.Expression.AcceptWalker(this);
                else
                    type = AstType.MakeSimpleType("tuple", TextLocation.Empty);

                return type;
            }

            public AstType VisitTypeDeclaration(TypeDeclaration typeDecl)
            {
                throw new InvalidOperationException("Can not work on that node!");
            }

            public AstType VisitFieldDeclaration(FieldDeclaration fieldDecl)
            {
                throw new InvalidOperationException("Can not work on that node!");
            }

            public AstType VisitParameterDeclaration(ParameterDeclaration parameterDecl)
            {
                if(parameterDecl.Option.IsNull){
                    parser.ReportSemanticErrorRegional(
                        "Error ES1301: Can not infer the expression '{0}' because it doesn't have any context.",
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
                        "Error ES1301: Can not infer the expression '{0}' because it doesn't have any context.",
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
                if(identifierPattern.InnerPattern.IsNull){
                    var type = identifierPattern.Parent.AcceptWalker(this);
                    if(IsTupleType(type)){
                        var parent = identifierPattern.Parent;
                        int i = 0;
                        parent.Children.Any(node => {
                            ++i;
                            return node.IsMatch(identifierPattern);
                        });
                        // decrement i before use because the above code always returns the index + 1
                        --i;
                        type = ((SimpleType)type).TypeArguments.ElementAt(i);
                    }else if(IsContainerType(type)){
                        type = MakeOutElementType(type);
                    }else if(IsIntSeqType(type)){
                        type = MakeOutElementType(type);
                    }else if(IsDictionaryType(type)){
                        
                    }else{
                        // type is a user defined type
                        var table = checker.symbols.GetTypeTable(type.Name);
                        if(table == null){
                            parser.ReportSemanticError(
                                "Error ES0101: Type symbol '{0}' turns out not to be declared or accessible in the current scope {1}!",
                                identifierPattern,
                                type.Name, checker.symbols.Name
                            );
                        }else{
                            var symbol = table.GetSymbol(identifierPattern.Identifier.Name);
                            type = symbol.Type;
                        }
                    }

                    return type.Clone();
                }else{
                    var type = identifierPattern.InnerPattern.AcceptWalker(this);
                    if(IsIntSeqType(type))
                        type = AstType.MakePrimitiveType("int", type.StartLocation);

                    return type.Clone();
                }
            }

            public AstType VisitValueBindingPattern(ValueBindingPattern valueBindingPattern)
            {
                var types = valueBindingPattern.Variables.Select(variable => variable.AcceptWalker(this));
                return AstType.MakeSimpleType("tuple", types, valueBindingPattern.StartLocation, valueBindingPattern.EndLocation);
            }

            public AstType VisitTuplePattern(TuplePattern tuplePattern)
            {
                return tuplePattern.Parent.AcceptWalker(this);
            }

            public AstType VisitCollectionPattern(CollectionPattern collectionPattern)
            {
                return collectionPattern.Parent.AcceptWalker(this);
            }

            public AstType VisitDestructuringPattern(DestructuringPattern destructuringPattern)
            {
                return destructuringPattern.TypePath;
            }

            public AstType VisitExpressionPattern(ExpressionPattern exprPattern)
            {
                return exprPattern.Expression.AcceptWalker(this);
            }

            public AstType VisitIgnoringRestPattern(IgnoringRestPattern restPattern)
            {
                return SimpleType.Null;
            }

            public AstType VisitNullNode(AstNode nullNode)
            {
                // Just ignore it.
                return AstType.Null;
            }

            public AstType VisitNewLine(NewLineNode newlineNode)
            {
                throw new InvalidOperationException("Can not work on that node!");
            }

            public AstType VisitWhitespace(WhitespaceNode whitespaceNode)
            {
                throw new InvalidOperationException("Can not work on that node!");
            }

            public AstType VisitExpressoTokenNode(ExpressoTokenNode tokenNode)
            {
                throw new InvalidOperationException("Can not work on that node!");
            }

            public AstType VisitPatternPlaceholder(AstNode placeholder, ICSharpCode.NRefactory.PatternMatching.Pattern child)
            {
                throw new InvalidOperationException("Can not work on that node!");
            }

            #endregion

            static bool IsNumericalType(PrimitiveType type)
            {
                return (int)Expresso.TypeSystem.KnownTypeCode.Int <= (int)type.KnownTypeCode
                    && (int)type.KnownTypeCode <= (int)Expresso.TypeSystem.KnownTypeCode.BigInteger;
            }

            static bool IsIntSeqType(AstType type)
            {
                return type is PrimitiveType && ((PrimitiveType)type).KnownTypeCode == KnownTypeCode.IntSeq;
            }
        }
    }
}

