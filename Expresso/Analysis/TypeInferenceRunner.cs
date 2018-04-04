using System;
using System.Linq;
using ICSharpCode.NRefactory;
using ICSharpCode.NRefactory.PatternMatching;
using Expresso.TypeSystem;


namespace Expresso.Ast.Analysis
{
    public partial class TypeChecker
    {
        /// <summary>
        /// The type inference runner is responsible for inferring types when asked.
        /// It does the job by inferring and replacing old nodes with the calculated type nodes
        /// in the symbol table. 
        /// Note that the AstType nodes returned won't be cloned. So be careful when using it.
        /// </summary>
        /// <remarks>
        /// Currently, I must say it's just a temporal implementation since it messes around the AST itself
        /// in order to keep track of type relations.
        /// </remarks>
        public class TypeInferenceRunner : IAstWalker<AstType>
        {
            Parser parser;
            TypeChecker checker;

            public bool InspectsClosure{
                get; set;
            }

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

            public AstType VisitDoWhileStatement(DoWhileStatement doWhileStmt)
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
                var arg_types = new AstType[callExpr.Arguments.Count];
                foreach(var pair in Enumerable.Range(0, callExpr.Arguments.Count).Zip(callExpr.Arguments, (l, r) => new Tuple<int, Expression>(l, r))){
                    var arg_type = pair.Item2.AcceptWalker(this);
                    arg_types[pair.Item1] = arg_type;
                }

                var parent_types = checker.argument_types;
                checker.argument_types = arg_types;

                var func_type = callExpr.Target.AcceptWalker(this) as FunctionType;
                if(func_type == null){
                    throw new ParserException(
                        "Error ES1805: {0} turns out not to be a function.",
                        callExpr,
                        callExpr.Target.ToString()
                    );
                }

                checker.argument_types = parent_types;
                
                return func_type.ReturnType;
            }

            public AstType VisitCastExpression(CastExpression castExpr)
            {
                return castExpr.ToExpression;
            }

            public AstType VisitCatchClause(CatchClause catchClause)
            {
                throw new InvalidOperationException("Can not work on that node");
            }

            public AstType VisitClosureLiteralExpression(ClosureLiteralExpression closure)
            {
                // Because there are times when the closure is not inspected beforehand
                // examine it here
                if(InspectsClosure){
                    InspectsClosure = false;
                    checker.VisitClosureLiteralExpression(closure);
                }

                // Usually we don't need to descend or ascend scopes in TypeInferenceRunner
                // we do need to do that here because TypeChecker.VisitVariableInitializer can directly call this method.
                // Descend scopes 2 times because closure parameters also have thier own scope.
                int tmp_counter = checker.scope_counter;
                bool has_descended = false;
                // FIXME: This condition would cause a problem if the closure is surrounded by another closure
                if(!checker.symbols.Parent.Name.StartsWith("closure", StringComparison.CurrentCulture)){
                    checker.DescendScope();
                    checker.scope_counter = 0;

                    checker.DescendScope();

                    has_descended = true;
                }

                var type = FigureOutReturnType(closure.Body);

                var param_types = 
                    from p in closure.Parameters
                                 select p.ReturnType.Clone();

                if(has_descended){
                    checker.AscendScope();
                    checker.AscendScope();
                    // Doesn't add 1 to tmp_counter because other methods also peek into that scope
                    checker.scope_counter = tmp_counter;
                }

                return AstType.MakeFunctionType("closure", type, param_types);
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
                    obj_type.TypeArguments.FirstOrNullObject().ReplaceWith(key_type.Clone());
                    obj_type.TypeArguments.LastOrNullObject().ReplaceWith(value_type.Clone());
                }else if(obj_type.Name == "array" || obj_type.Name == "vector"){
                    var element_type = comp.Item.AcceptWalker(this);
                    obj_type.TypeArguments.FirstOrNullObject().ReplaceWith(element_type.Clone());
                }else{
                    throw new InvalidOperationException("Unreachable!");
                }

                checker.AscendScope();
                checker.scope_counter = tmp_counter + 1;

                return obj_type;
            }

            public AstType VisitComprehensionForClause(ComprehensionForClause compFor)
            {
                var inferred = compFor.Target.AcceptWalker(this);
                var inferred_primitive = inferred as PrimitiveType;
                if(inferred_primitive != null){
                    // See if it is a IntSeq object
                    if(inferred_primitive.KnownTypeCode != KnownTypeCode.IntSeq){
                        parser.ReportSemanticError(
                            "Error ES1302: '{0}' isn't a sequence type! A comprehension expects a sequence object.",
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
                            "Error ES1302: '{0}' isn't a sequence type! A comprehension expects a sequence object.",
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

            public AstType VisitFinallyClause(FinallyClause finallyClause)
            {
                return VisitBlock(finallyClause.Body);
            }

            public AstType VisitKeyValueLikeExpression(KeyValueLikeExpression keyValue)
            {
                throw new InvalidOperationException("Can not work on that node!");
            }

            public AstType VisitLiteralExpression(LiteralExpression literal)
            {
                return literal.Type;
            }

            public AstType VisitIdentifier(Identifier ident)
            {
                if(!ident.Type.IsNull && !(ident.Type is PlaceholderType))
                    return ResolveType(ident);

                if(ident.Type is PlaceholderType){
                    var symbol = checker.symbols.GetSymbolInAnyScope(ident.Name);
                    if(symbol != null){
                        var cloned = symbol.Type.Clone();
                        ident.Type.ReplaceWith(cloned);
                        if(!ident.Type.IsNull && !(ident.Type is PlaceholderType))
                            return ResolveType(ident);
                        else
                            return symbol.Type;
                    }

                    var symbol2 = checker.symbols.GetTypeSymbolInAnyScope(ident.Name);
                    if(symbol2 == null){
                        throw new ParserException(
                            "Error ES1000: The symbol '{0}' at {1} is not defined in the current scope {2}.",
                            ident,
                            ident.Name, ident.StartLocation, checker.symbols.Name
                        );
                    }else{
                        var cloned = symbol2.Type.Clone();
                        ident.Type.ReplaceWith(cloned);
                        return symbol2.Type;
                    }
                }else{
                    return ident.Type;
                }
            }

            public AstType VisitIntegerSequenceExpression(IntegerSequenceExpression intSeq)
            {
                return AstType.MakePrimitiveType("intseq", intSeq.StartLocation);
            }

            public AstType VisitIndexerExpression(IndexerExpression indexExpr)
            {
                var target_type = indexExpr.Target.AcceptWalker(this);
                if(target_type is SimpleType simple_type){
                    if(simple_type.Name != "array" && simple_type.Name != "vector" && simple_type.Name != "dictionary"){
                        // We need to duplicate the following error messages on InferenceRunner and TypeChecker
                        // because we won't be reaching some code paths
                        throw new ParserException(
                            "Error ES3011: Can not apply the indexer operator on type `{0}`",
                            indexExpr,
                            simple_type.ToString()
                        );
                    }

                    if(indexExpr.Arguments.Count == 1){
                        var arg_type = indexExpr.Arguments.First().AcceptWalker(this);
                        if(arg_type is PrimitiveType primitive && primitive.KnownTypeCode == KnownTypeCode.IntSeq){
                            /*if(simple_type.Identifier == "dictionary"){
                                throw new ParserException(
                                    "Error ES3012: Can not apply the indexer operator on a dictionary with an `intseq`",
                                    indexExpr
                                );
                            }*/

                            return AstType.MakeSimpleType("slice", new []{simple_type.Clone(), simple_type.TypeArguments.First().Clone()});
                        }
                    }
                    return simple_type.TypeArguments.LastOrNullObject().Clone();
                }

                return target_type;
            }

            public AstType VisitMemberReference(MemberReferenceExpression memRef)
            {
                var target_type = memRef.Target.AcceptWalker(this);
                var name = (target_type is MemberType member) ? member.Target.Name + "::" + member.MemberName :
                                                                      !target_type.IdentifierNode.Type.IsNull ? target_type.IdentifierNode.Type.Name : target_type.Name;
                var type_table = checker.symbols.GetTypeTable(name);
                if(type_table == null){
                    throw new ParserException(
                        "Error ES2000: Although the expression '{0}' is evaluated to the type `{1}`, there isn't any type with that name in the scope '{2}'.",
                        memRef.Target,
                        memRef.Target.ToString(), target_type, checker.symbols.Name
                    );
                }
                    
                Identifier symbol = null;
                if(checker.argument_types != null){
                    var func_type = AstType.MakeFunctionType(memRef.Member.Name, AstType.MakePlaceholderType(), checker.argument_types.Select(arg => arg.Clone()));
                    var results = GetOverloadOfMethod(type_table, memRef.Member.Name, func_type);
                    if(results != null){
                        symbol = results.Item1;
                        ((CallExpression)memRef.Parent).OverloadSignature = results.Item2;
                    }
                }else{
                    symbol = type_table.GetSymbol(memRef.Member.Name);
                }

                if(symbol == null){
                    symbol = type_table.GetSymbol("get_" + memRef.Member.Name);
                    if(symbol == null){
                        throw new ParserException(
                            "Error ES2001: The type `{0}` doesn't have a field or a method '{1}'!",
                            memRef.Member,
                            target_type.ToString(), memRef.Member.Name
                        );
                    }
                }
                 
                var modifiers = symbol.Modifiers;
                bool reported_error = false;
                if(modifiers.HasFlag(Modifiers.Private)){
                    if(!(memRef.Target is SelfReferenceExpression)){
                        parser.ReportSemanticError(
                            "Error ES3300: A private field can't be accessed from outside its surrounding scope: `{0}`.",
                            memRef.Member,
                            memRef
                        );
                        reported_error = true;
                    }
                }

                if(modifiers.HasFlag(Modifiers.Protected)){
                    if(!(memRef.Target is SelfReferenceExpression) && !(memRef.Target is SuperReferenceExpression)){
                        parser.ReportSemanticError(
                            "Error ES3301: A protected field can't be accessed from outside its surrounding or derived classes: `{0}`.",
                            memRef.Member,
                            memRef
                        );
                        reported_error = true;
                    }
                }

                if(checker.inspecting_immutability && modifiers.HasFlag(Modifiers.Immutable)){
                    parser.ReportSemanticError(
                        "Error ES1902: Re-assignment on an immutable field: `{0}`.",
                        memRef.Member,
                        memRef
                    );
                    reported_error = true;
                }

                if(reported_error)
                    throw new FatalError("Accessibility or immutability error");
                        
                memRef.Member.Type.ReplaceWith(symbol.Type.Clone());
                // Resolve the name here because we defer it until this point
                memRef.Member.IdentifierId = symbol.IdentifierId;
                if(symbol.Name.StartsWith("get_", StringComparison.CurrentCulture) && symbol.Type is FunctionType func_type2)
                    return func_type2.ReturnType;
                else
                    return symbol.Type;
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
                            result = tmp_type.Type.Clone();
                            item.Type.ReplaceWith(result);
                            table = table.GetTypeTable(item.Name);
                        }else if(table.HasSymbolInAnyScope(item.Name)){
                            var tmp = table.GetSymbolInAnyScope(item.Name);
                            result = tmp.Type.Clone();
                            item.Type.ReplaceWith(result);
                        }else{
                            throw new ParserException(
                                "Error ES1700: The type or symbol name '{0}' is not declared.",
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
                var table = creation.TypePath is MemberType member ? checker.symbols.GetTypeTable(member.Target.Name) : checker.symbols;
                var referenced = table.GetTypeSymbolInAnyScope(creation.TypePath.Name);
                if(referenced == null){
                    throw new ParserException(
                        "Error ES1501: The type `{0}` isn't found or accessible from the scope {1}.",
                        creation,
                        creation.TypePath.ToString(),
                        creation.TypePath.Name
                    );
                }else{
                    creation.TypePath.IdentifierNode.Type = referenced.Type.Clone();
                }

                foreach(var keyvalue in creation.Items){
                    // Walk through creation.Items in order to make them type-aware
                    keyvalue.ValueExpression.AcceptWalker(this);
                }
                return creation.TypePath;
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
                    seqInitializer.ObjectType.TypeArguments.FirstOrNullObject().ReplaceWith(key_type.Clone());
                    seqInitializer.ObjectType.TypeArguments.LastOrNullObject().ReplaceWith(value_type.Clone());

                    return AstType.MakeSimpleType("dictionary", new []{
                        key_type.Clone(), value_type.Clone()
                    });
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

                return (seqExpr.Items.Count == 1) ? types.First() : AstType.MakeSimpleType("tuple", types, seqExpr.StartLocation, seqExpr.EndLocation);
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
                            "Error ES3200: Can not apply the unary operator '{0}' on type `{1}`.",
                            unaryExpr,
                            unaryExpr.OperatorToken,
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

                return decls.First().ReturnType;
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

                return decls.First();
            }

            public AstType VisitNullReferenceExpression(NullReferenceExpression nullRef)
            {
                return SimpleType.Null;
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
                throw new InvalidOperationException("Can not work on that node!");
            }

            public AstType VisitPrimitiveType(PrimitiveType primitiveType)
            {
                throw new InvalidOperationException("Can not work on that node!");
            }

            public AstType VisitReferenceType(ReferenceType referenceType)
            {
                throw new InvalidOperationException("Can not work on that node!");
            }

            public AstType VisitMemberType(MemberType memberType)
            {
                throw new InvalidOperationException("Can not work on that node!");
            }

            public AstType VisitFunctionType(FunctionType funcType)
            {
                throw new InvalidOperationException("Can not work on that node!");
            }

            public AstType VisitParameterType(ParameterType paramType)
            {
                throw new InvalidOperationException("Can not work on that node!");
            }

            public AstType VisitPlaceholderType(PlaceholderType placeholderType)
            {
                throw new InvalidOperationException("Can not work on that node!");
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
                return FigureOutReturnType(funcDecl.Body);
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
                        "Error ES1310: Can not infer the type of the parameter '{0}' because it doesn't have an optional value.",
                        parameterDecl.NameToken, parameterDecl.NameToken,
                        parameterDecl.Name
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
                        "Error ES1311: Can not infer the expression '{0}' because it doesn't have any context.",
                        initializer.Pattern, initializer.Initializer,
                        initializer
                    );
                }

                var init_type = initializer.Initializer.AcceptWalker(this);
                if(initializer.Pattern is PatternWithType pattern)
                    MakePatternTypeAware(pattern.Pattern, init_type);
                else
                    MakePatternTypeAware(initializer.Pattern, init_type);
                
                return init_type;
            }

            public AstType VisitWildcardPattern(WildcardPattern wildcardPattern)
            {
                return SimpleType.Null;
            }

            public AstType VisitIdentifierPattern(IdentifierPattern identifierPattern)
            {
                if(identifierPattern.Ancestors.Any(a => a is PatternWithType)){
                    return VisitIdentifier(identifierPattern.Identifier);
                }else if(identifierPattern.InnerPattern.IsNull){
                    var type = identifierPattern.Parent.AcceptWalker(this);
                    if(IsTupleType(type)){
                        var parent = identifierPattern.Parent;
                        int i = 0;
                        parent.Children.Any(node => {
                            if(node.IsMatch(identifierPattern)){
                                return true;
                            }else{
                                ++i;
                                return false;
                            }
                        });
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
                                "Error ES0101: The type symbol '{0}' turns out not to be declared or accessible in the current scope {1}!",
                                identifierPattern,
                                type.Name, checker.symbols.Name
                            );
                        }else{
                            var symbol = table.GetSymbol(identifierPattern.Identifier.Name);
                            type = symbol.Type;
                        }
                    }

                    return type;
                }else{
                    var type = identifierPattern.InnerPattern.AcceptWalker(this);
                    if(IsIntSeqType(type))
                        type = AstType.MakePrimitiveType("int", type.StartLocation);

                    return type;
                }
            }

            public AstType VisitTuplePattern(TuplePattern tuplePattern)
            {
                if(tuplePattern.Ancestors.Any(a => a is PatternWithType)){
                    var elem_types =
                        from elem in tuplePattern.Patterns
                                                 select elem.AcceptWalker(this).Clone();
                    return AstType.MakeSimpleType("tuple", elem_types);
                }else{
                    return tuplePattern.Parent.AcceptWalker(this);
                }
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

            public AstType VisitKeyValuePattern(KeyValuePattern keyValuePattern)
            {
                return keyValuePattern.Value.AcceptWalker(this);
            }

            public AstType VisitPatternWithType(PatternWithType pattern)
            {
                //TODO: consider the match statement
                var type = pattern.Pattern.AcceptWalker(this);
                if(pattern.Type is PlaceholderType)
                    return type;

                if(type is SimpleType val_tuple && pattern.Type is SimpleType tuple){
                    // TODO: take resolving type name into account
                    foreach(var pair in tuple.TypeArguments.Zip(val_tuple.TypeArguments, (l, r) => new Tuple<AstType, AstType>(l, r)))
                        pair.Item2.ReplaceWith(pair.Item1.Clone());

                    return pattern.Type;
                }else if(type is SimpleType val_tuple2 && !(pattern.Type is PlaceholderType) || pattern.Type is SimpleType tuple2 && !(type is PlaceholderType)){
                    throw new ParserException(
                        "Error ES1306: The type in the type annotation `{0}` conflicts with the expression type `{1}`",
                        pattern,
                        pattern.Type.ToString(), type
                    );
                }else{
                    type.ReplaceWith(pattern.Type.Clone());
                    if(pattern.Pattern is IdentifierPattern ident_pat)
                        ResolveType(ident_pat.Identifier);
                    else
                        throw new FatalError("Unrecognizable code path");
                    
                    return pattern.Type;
                }
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
                return type is PrimitiveType primitive && primitive.KnownTypeCode == KnownTypeCode.IntSeq;
            }

            AstType FigureOutReturnType(BlockStatement block)
            {
                var return_stmt = block.Statements.Last() as ReturnStatement;
                if(return_stmt != null)
                    return return_stmt.Expression.IsNull ? AstType.MakeSimpleType("tuple") : return_stmt.Expression.AcceptWalker(this);

                var last_if = block.Statements.Last() as IfStatement;
                if(last_if != null){
                    var func = block.Parent as FunctionDeclaration;
                    var closure = block.Parent as ClosureLiteralExpression;
                    if(func != null && IsVoidType(func.ReturnType)){
                        return func.ReturnType;
                    }else if(closure != null && IsVoidType(closure.ReturnType)){
                        return closure.ReturnType;
                    }

                    if(last_if.FalseBlock.IsNull){
                        var last_return = last_if.TrueBlock.Statements.Last() as ReturnStatement;
                        if(last_return != null)
                            return last_return.Expression.IsNull ? AstType.MakeSimpleType("tuple", last_return.StartLocation) : last_return.Expression.AcceptWalker(this);
                    }else{
                        // FIXME: This code can't take else if statements into account
                        // REVISE: A series of if statements becomes a tree construct
                        // so we may ignore the above statement
                        var true_return = last_if.TrueBlock.Statements.Last() as ReturnStatement;
                        var false_return = last_if.FalseBlock.Statements.Last() as ReturnStatement;
                        if(true_return != null && false_return != null){
                            var true_return_type = true_return.Expression.IsNull ? AstType.MakeSimpleType("tuple", true_return.StartLocation) : true_return.Expression.AcceptWalker(this);
                            var false_return_type = false_return.Expression.IsNull ? AstType.MakeSimpleType("tuple", false_return.StartLocation) : false_return.Expression.AcceptWalker(this);
                            return checker.FigureOutCommonType(true_return_type, false_return_type);
                        }else if(func != null && IsPlaceholderType(func.ReturnType) ||
                                 closure != null && IsPlaceholderType(closure.ReturnType)){
                            if(true_return == null && false_return == null)
                                return AstType.MakeSimpleType("tuple");
                            
                            checker.parser.ReportSemanticErrorRegional(
                                "Error ES1800: All code paths must return something. {0} returns nothing.",
                                last_if.TrueBlock, last_if.FalseBlock,
                                (true_return != null) ? "The false block" : "The true block"
                            );
                        }
                    }
                }

                return AstType.MakeSimpleType("tuple"); // The void type
            }

            void MakePatternTypeAware(PatternConstruct pattern, AstType type)
            {
                if(pattern is IdentifierPattern ident_pat){
                    ident_pat.Identifier.Type.ReplaceWith(type);
                }else if(pattern is TuplePattern tuple_pat && type is SimpleType tuple_type){
                    foreach(var pair in tuple_pat.Patterns.Zip(tuple_type.TypeArguments, (l, r) => new Tuple<PatternConstruct, AstType>(l, r))){
                        if(pair.Item1 is IdentifierPattern ident_pat2)
                            ident_pat2.Identifier.Type.ReplaceWith(pair.Item2);
                    }
                }
            }

            AstType ResolveType(Identifier ident)
            {
                if(!ident.Type.IdentifierNode.Type.IsNull)
                    return ident.Type;
                
                var type_symbol = checker.symbols.GetTypeSymbolInAnyScope(ident.Type.Name);
                if(type_symbol != null && !type_symbol.Type.IsNull && ident.Type.Name != type_symbol.Type.Name)
                    ident.Type.IdentifierNode.Type = type_symbol.Type.Clone();

                return ident.Type;
            }

            Tuple<Identifier, FunctionType> GetOverloadOfMethod(SymbolTable table, string methodName, FunctionType funcType)
            {
                if(funcType.Parameters.Count == 0){
                    var symbol = table.GetSymbol(methodName, funcType);
                    return new Tuple<Identifier, FunctionType>(symbol, funcType);
                }

                // TODO: make it so that it takes upcasts into account
                foreach(var pair in Enumerable.Range(0, funcType.Parameters.Count).Reverse().Zip(funcType.Parameters.Reverse(),
                                                                                                 (l, r) => new Tuple<int, AstType>(l, r))){
                    var new_parameters = funcType.Parameters
                                                 .Take(pair.Item1 + 1)
                                                 .Select(t => t.Clone());
                    var new_func_type = AstType.MakeFunctionType(funcType.Name, funcType.ReturnType.Clone(), new_parameters);
                    var symbol = table.GetSymbol(methodName, new_func_type);
                    if(symbol != null)
                        return new Tuple<Identifier, FunctionType>(symbol, new_func_type);

                    var new_parameters2 = funcType.Parameters
                                                  .Take(pair.Item1)
                                                  .Select(t => t.Clone())
                                                  .Concat(Enumerable.Range(0, funcType.Parameters.Count - pair.Item1)
                                                          .Select(_ => AstType.MakeSimpleType(AstNode.MakeIdentifier("Object", AstType.MakeSimpleType("System.Object"))))
                                                         );
                    var new_func_type2 = AstType.MakeFunctionType(funcType.Name, funcType.ReturnType.Clone(), new_parameters2);
                    var symbol2 = table.GetSymbol(methodName, new_func_type2);
                    if(symbol2 != null)
                        return new Tuple<Identifier, FunctionType>(symbol2, new_func_type2);

                    var new_parameters3 = funcType.Parameters
                                                  .Take(pair.Item1)
                                                  .Select(t => t.Clone())
                                                  .Concat(new []{AstType.MakeSimpleType(
                                                      "array",
                                                      TextLocation.Empty,
                                                      TextLocation.Empty,
                                                      // This is because objects from the outside starts with a capital O
                                                      AstType.MakeSimpleType(
                                                          AstNode.MakeIdentifier(
                                                              "Object",
                                                              AstType.MakeSimpleType("System.Object")
                                                             )
                                                         )
                                                     )});
                    var new_func_type3 = AstType.MakeFunctionType(funcType.Name, funcType.ReturnType.Clone(), new_parameters3);
                    var symbol3 = table.GetSymbol(methodName, new_func_type3);
                    if(symbol3 != null)
                        return new Tuple<Identifier, FunctionType>(symbol3, new_func_type3);
                }

                return null;
            }
        }
    }
}

