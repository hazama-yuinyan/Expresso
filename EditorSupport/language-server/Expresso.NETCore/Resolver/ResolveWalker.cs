//
// ResolveWalker.cs
//
// Author:
//       train12 <kotonechan@live.jp>
//
// Copyright (c) 2018 
//
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
//
// The above copyright notice and this permission notice shall be included in
// all copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
// THE SOFTWARE.
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using Expresso.Ast;
using Expresso.Ast.Analysis;
using Expresso.TypeSystem;
using Expresso.Utilities;
using ICSharpCode.NRefactory;
using ICSharpCode.NRefactory.PatternMatching;
using ICSharpCode.NRefactory.Semantics;
using ICSharpCode.NRefactory.TypeSystem;
using ICSharpCode.NRefactory.TypeSystem.Implementation;

namespace Expresso.Resolver
{
    using ExpressoKnownTypeReference = Expresso.TypeSystem.KnownTypeReference;

    public class ResolveWalker : IAstWalker<ResolveResult>
    {
        const uint PrintId = 1_000_000_000u;
        static readonly ResolveResult errorResult = ErrorResolveResult.UnknownError;
        static readonly Dictionary<uint, Symbol> Symbols = new Dictionary<uint, Symbol>();

        ExpressoResolver resolver;
        readonly ExpressoUnresolvedFile unresolved_file;
        readonly Dictionary<AstNode, ResolveResult> resolve_result_cache = new Dictionary<AstNode, ResolveResult>();
        readonly Dictionary<AstNode, ExpressoResolver> resolver_before_dict = new Dictionary<AstNode, ExpressoResolver>();
        readonly Dictionary<AstNode, ExpressoResolver> resolver_after_dict = new Dictionary<AstNode, ExpressoResolver>();

        IResolveVisitorNavigator navigator;
        bool resolver_enabled, request_type, request_property_or_field, request_method;
        IType context_type;
        IMember context_property_or_field;
        IMethod context_method;
        ITypeReference[] context_argument_types;

        #region Properties
        public CancellationToken CancellationToken{
            get; set;
        }
        #endregion

        #region Constructor
        static readonly IResolveVisitorNavigator skipAllNavigator = new ConstantModeResolveVisitorNavigator(ResolveVisitorNavigationMode.Skip, null);
        /// <summary>
        /// Creates a new ResolveVisitor instance.
        /// </summary>
        public ResolveWalker(ExpressoResolver resolver, ExpressoUnresolvedFile unresolvedFile)
        {
            this.resolver = resolver ?? throw new ArgumentNullException(nameof(resolver));
            unresolved_file = unresolvedFile;
            navigator = skipAllNavigator;

            if(!Symbols.Any())
                InitSymbols(resolver.Compilation);
        }

        static void InitSymbols(ICompilation compilation)
        {
            var console_type = compilation.FindType(typeof(Console));
            var string_type_ref = compilation.FindType(typeof(string)).ToTypeReference();
            var object_array_type_ref = compilation.FindType(typeof(object[])).ToTypeReference();

            var print_method = console_type.GetMethod("Write", new []{string_type_ref, object_array_type_ref});
            Symbols.Add(PrintId, new Symbol{Method = print_method, ResolveResult = new TypeResolveResult(console_type)});

            var println_method = console_type.GetMethod("WriteLine", new []{string_type_ref, object_array_type_ref});
            Symbols.Add(PrintId + 1u, new Symbol{Method = println_method, ResolveResult = new TypeResolveResult(console_type)});
        }

        #region Symbols
        static void AddSymbol(Identifier ident, Symbol symbol)
        {
            Symbols.Add(ident.IdentifierId, symbol);
        }

        static Symbol GetSymbol(Identifier ident)
        {
            if(Symbols.TryGetValue(ident.IdentifierId, out var symbol))
                return symbol;
            else
                return null;
        }
        #endregion

        internal void SetNavigator(IResolveVisitorNavigator navigator)
        {
            this.navigator = navigator ?? skipAllNavigator;
        }

        ResolveResult voidResult => new ResolveResult(resolver.Compilation.FindType(ICSharpCode.NRefactory.TypeSystem.KnownTypeCode.Void));
        #endregion

        #region ResetContext
        /// <summary>
        /// Resets the visitor to the stored position, runs the action, and then reverts the visitor to the previous position.
        /// </summary>
        void ResetContext(ExpressoResolver storedContext, Action action)
        {
            var old_resolver_enabled = resolver_enabled;
            var old_resolver = resolver;
            //var oldQueryResult = this.currentQueryResult;

            try{
                resolver_enabled = false;
                resolver = storedContext;
                //this.currentQueryResult = null;

                action();
            }
            finally{
                resolver_enabled = old_resolver_enabled;
                resolver = old_resolver;
                //this.currentQueryResult = oldQueryResult;
            }
        }
        #endregion

        #region Scan / Resolve
        /// <summary>
        /// Scans the AST rooted at the given node.
        /// </summary>
        public void Scan(AstNode node)
        {
            if(node == null || node.IsNull)
                return;

            switch(node.NodeType){
                case NodeType.Token:
                case NodeType.Whitespace:
                    return; // skip tokens, identifiers, comments, etc.
            }

            // don't Scan again if the node was already resolved
            if (resolve_result_cache.ContainsKey(node)) {
                // Restore state change caused by this node:
                if(resolver_before_dict.TryGetValue(node, out var new_resolver))
                    resolver = new_resolver;
                
                return;
            }

            var mode = navigator.Scan(node);
            switch(mode){
                case ResolveVisitorNavigationMode.Skip:
                    if(node is VariableDeclarationStatement /*|| node is SwitchSection*/){
                        // Enforce scanning of variable declarations.
                        goto case ResolveVisitorNavigationMode.Scan;
                    }
                    StoreCurrentState(node);
                    break;

                case ResolveVisitorNavigationMode.Scan:
                    bool old_resolver_enabled = resolver_enabled;
                    var old_resolver = resolver;
                    resolver_enabled = false;
                    StoreCurrentState(node);

                    ResolveResult result = node.AcceptWalker(this);
                    if(result != null){
                        // If the node was resolved, store the result even though it wasn't requested.
                        // This is necessary so that Visit-methods that decide to always resolve are
                        // guaranteed to get called only once.
                        // This is used for lambda registration.
                        StoreResult(node, result);

                        if(resolver != old_resolver){
                            // The node changed the resolver state:
                            resolver_after_dict.Add(node, resolver);
                        }
                        CancellationToken.ThrowIfCancellationRequested();
                    }
                    resolver_enabled = old_resolver_enabled;
                    break;

                case ResolveVisitorNavigationMode.Resolve:
                    Resolve(node);
                    break;

                default:
                    throw new InvalidOperationException("Invalid value for ResolveVisitorNavigationMode");
            }
        }

        /// <summary>
        /// Equivalent to 'Scan', but also resolves the node at the same time.
        /// This method should be only used if the <see cref="ExpressoResolver"/> passed to the <see cref="ResolveWalker"/> was manually set
        /// to the correct state.
        /// Otherwise, use <c>resolver.Scan(syntaxTree); var result = resolver.GetResolveResult(node);</c>
        /// instead.
        /// --
        /// This method now is internal, because it is difficult to use correctly.
        /// Users of the public API should use Scan()+GetResolveResult() instead.
        /// </summary>
        internal ResolveResult Resolve(AstNode node)
        {
            if(node == null || node.IsNull)
                return errorResult;
            
            bool old_resolver_enabled = resolver_enabled;
            resolver_enabled = true;

            if(!resolve_result_cache.TryGetValue(node, out var result)){
                CancellationToken.ThrowIfCancellationRequested();
                StoreCurrentState(node);

                var old_resolver = resolver;
                result = node.AcceptWalker(this) ?? errorResult;
                StoreResult(node, result);

                if(resolver != old_resolver){
                    // The node changed the resolver state:
                    resolver_after_dict.Add(node, resolver);
                }
            }

            resolver_enabled = old_resolver_enabled;
            return result;
        }

        IType ResolveType(AstType type)
        {
            return Resolve(type).Type;
        }

        void StoreCurrentState(AstNode node)
        {
            // It's possible that we re-visit an expression that we scanned over earlier,
            // so we might have to overwrite an existing state.

            #if DEBUG
            if(resolver_before_dict.TryGetValue(node, out var old_resolver))
                Debug.Assert(old_resolver.LocalVariables.SequenceEqual(resolver.LocalVariables));
            #endif

            resolver_before_dict[node] = resolver;
        }

        void StoreResult(AstNode node, ResolveResult result)
        {
            Debug.Assert(result != null);
            if(node.IsNull)
                return;
            
            Console.WriteLine("Resolved '{0}' to {1}", node, result);
            Debug.Assert(!ExpressoAstResolver.IsUnresolvableNode(node));
            // The state should be stored before the result is.
            Debug.Assert(resolver_before_dict.ContainsKey(node));
            // Don't store results twice.
            Debug.Assert(!resolve_result_cache.ContainsKey(node));
            // Don't use ConversionResolveResult as a result, because it can get
            // confused with an implicit conversion.
            Debug.Assert(!(result is ConversionResolveResult) /*|| result is CastResolveResult*/);
            resolve_result_cache[node] = result;

            if(navigator != null)
                navigator.Resolved(node, result);
        }

        void ScanChildren(AstNode node)
        {
            for(var child = node.FirstChild; child != null; child = child.NextSibling)
                Scan(child);
        }
        #endregion

        #region GetResolveResult
        /// <summary>
        /// Gets the resolve result for the specified node.
        /// If the node was not resolved by the navigator, this method will resolve it.
        /// </summary>
        public ResolveResult GetResolveResult(AstNode node)
        {
            Debug.Assert(!ExpressoAstResolver.IsUnresolvableNode(node));

            //MergeUndecidedLambdas();
            if(resolve_result_cache.TryGetValue(node, out var result))
                return result;

            var stored_resolver = GetPreviouslyScannedContext(node, out var parent);
            ResetContext(
                stored_resolver,
                () => {
                navigator = new NodeListResolveVisitorNavigator(node);
                Debug.Assert(!resolver_enabled);
                Scan(parent);
                navigator = skipAllNavigator;
            });

            //MergeUndecidedLambdas();
            return resolve_result_cache[node];
        }

        ExpressoResolver GetPreviouslyScannedContext(AstNode node, out AstNode parent)
        {
            parent = node;
            ExpressoResolver stored_resolver;
            while(!resolver_before_dict.TryGetValue(parent, out stored_resolver)){
                var tmp = parent.Parent;
                if(tmp == null)
                    throw new InvalidOperationException("Could not find a resolver state for any parent of the specified node. Are you trying to resolve a node that is not a descendant of the ExpressoAstResolver's root node?");

                if(tmp.NodeType == NodeType.Whitespace)
                    return resolver; // special case: resolve expression within preprocessor directive

                parent = tmp;
            }
            return stored_resolver;
        }

        /// <summary>
        /// Gets the resolver state in front of the specified node.
        /// If the node was not visited by a previous scanning process, the
        /// AST will be scanned again to determine the state.
        /// </summary>
        public ExpressoResolver GetResolverStateBefore(AstNode node)
        {
            //MergeUndecidedLambdas();
            if(resolver_before_dict.TryGetValue(node, out var r))
                return r;

            var stored_resolver = GetPreviouslyScannedContext(node, out var parent);
            ResetContext(
                stored_resolver,
                delegate {
                navigator = new NodeListResolveVisitorNavigator(new[] { node }, scanOnly: true);
                Debug.Assert(!resolver_enabled);
                // parent might already be resolved if 'node' is an unresolvable node
                Scan(parent);
                navigator = skipAllNavigator;
            });

            //MergeUndecidedLambdas();
            while(node != null){
                if(resolver_before_dict.TryGetValue(node, out r))
                    return r;
                
                node = node.Parent;
            }
            return null;
        }

        public ExpressoResolver GetResolverStateAfter(AstNode node)
        {
            // Resolve the node to fill the resolverAfterDict
            GetResolveResult(node);
            if(resolver_after_dict.TryGetValue(node, out var result))
                return result;
            else
                return GetResolverStateBefore(node);
        }

        ResolveResult IAstWalker<ResolveResult>.VisitAst(ExpressoAst ast)
        {
            var prev_resolver = resolver;
            /*try{
                if(unresolved_file != null){
                    
                }
            }finally{
                resolver = prev_resolver;
            }*/
            ScanChildren(ast);
            return voidResult;
        }

        ResolveResult IAstWalker<ResolveResult>.VisitBlock(BlockStatement block)
        {
            resolver = resolver.PushBlock();
            ScanChildren(block);
            resolver = resolver.PopBlock();
            return voidResult;
        }

        ResolveResult IAstWalker<ResolveResult>.VisitBreakStatement(BreakStatement breakStmt)
        {
            throw new NotImplementedException();
        }

        ResolveResult IAstWalker<ResolveResult>.VisitContinueStatement(ContinueStatement continueStmt)
        {
            throw new NotImplementedException();
        }

        ResolveResult IAstWalker<ResolveResult>.VisitDoWhileStatement(DoWhileStatement doWhileStmt)
        {
            throw new NotImplementedException();
        }

        ResolveResult IAstWalker<ResolveResult>.VisitEmptyStatement(EmptyStatement emptyStmt)
        {
            throw new NotImplementedException();
        }

        ResolveResult IAstWalker<ResolveResult>.VisitExpressionStatement(ExpressionStatement exprStmt)
        {
            ScanChildren(exprStmt);
            return voidResult;
        }

        ResolveResult IAstWalker<ResolveResult>.VisitForStatement(ForStatement forStmt)
        {
            throw new NotImplementedException();
        }

        ResolveResult IAstWalker<ResolveResult>.VisitValueBindingForStatement(ValueBindingForStatement valueBindingForStatment)
        {
            throw new NotImplementedException();
        }

        ResolveResult IAstWalker<ResolveResult>.VisitIfStatement(IfStatement ifStmt)
        {
            throw new NotImplementedException();
        }

        ResolveResult IAstWalker<ResolveResult>.VisitReturnStatement(ReturnStatement returnStmt)
        {
            throw new NotImplementedException();
        }

        ResolveResult IAstWalker<ResolveResult>.VisitMatchStatement(MatchStatement matchStmt)
        {
            throw new NotImplementedException();
        }

        ResolveResult IAstWalker<ResolveResult>.VisitWhileStatement(WhileStatement whileStmt)
        {
            throw new NotImplementedException();
        }

        ResolveResult IAstWalker<ResolveResult>.VisitYieldStatement(YieldStatement yieldStmt)
        {
            throw new NotImplementedException();
        }

        ResolveResult IAstWalker<ResolveResult>.VisitThrowStatement(ThrowStatement throwStmt)
        {
            throw new NotImplementedException();
        }

        ResolveResult IAstWalker<ResolveResult>.VisitTryStatement(TryStatement tryStmt)
        {
            throw new NotImplementedException();
        }

        ResolveResult IAstWalker<ResolveResult>.VisitVariableDeclarationStatement(VariableDeclarationStatement varDecl)
        {
            bool is_immutable = varDecl.Modifiers.HasFlag(Modifiers.Immutable);
            foreach(var variable in varDecl.Variables){
                var ast_type = RetrieveTypeFromPattern(variable.Pattern);
                var type = ResolveType(ast_type);
                IVariable v;
                if(is_immutable){
                    var rr = Resolve(variable.Initializer);
                    //rr = resolver.ResolveCast(type, rr);
                    v = MakeConstant(type, variable.NameToken, rr.ConstantValue);
                }else{
                    v = MakeVariable(type, variable.NameToken);
                }

                resolver = resolver.AddVariable(v);
                Scan(variable);
            }

            return voidResult;
        }

        ResolveResult IAstWalker<ResolveResult>.VisitAssignment(AssignmentExpression assignment)
        {
            throw new NotImplementedException();
        }

        ResolveResult IAstWalker<ResolveResult>.VisitBinaryExpression(BinaryExpression binaryExpr)
        {
            throw new NotImplementedException();
        }

        ResolveResult IAstWalker<ResolveResult>.VisitCallExpression(CallExpression call)
        {
            request_method = true;
            context_method = null;

            if(resolver_enabled){
                var target_rr = call.Target.AcceptWalker(this);

                var target_method = context_method;
                request_method = false;
                context_method = null;

                var rrs = call.Arguments.Select(arg => arg.AcceptWalker(this))
                              .ToList();
                var rr = new InvocationResolveResult(target_rr, target_method, rrs);
                return rr;
            }else{
                ScanChildren(call);

                request_method = false;
                context_method = null;

                return null;
            }
        }

        ResolveResult IAstWalker<ResolveResult>.VisitCastExpression(CastExpression castExpr)
        {
            throw new NotImplementedException();
        }

        ResolveResult IAstWalker<ResolveResult>.VisitCatchClause(CatchClause catchClause)
        {
            throw new NotImplementedException();
        }

        ResolveResult IAstWalker<ResolveResult>.VisitClosureLiteralExpression(ClosureLiteralExpression closure)
        {
            throw new NotImplementedException();
        }

        ResolveResult IAstWalker<ResolveResult>.VisitComprehensionExpression(ComprehensionExpression comp)
        {
            throw new NotImplementedException();
        }

        ResolveResult IAstWalker<ResolveResult>.VisitComprehensionForClause(ComprehensionForClause compFor)
        {
            throw new NotImplementedException();
        }

        ResolveResult IAstWalker<ResolveResult>.VisitComprehensionIfClause(ComprehensionIfClause compIf)
        {
            throw new NotImplementedException();
        }

        ResolveResult IAstWalker<ResolveResult>.VisitConditionalExpression(ConditionalExpression condExpr)
        {
            throw new NotImplementedException();
        }

        ResolveResult IAstWalker<ResolveResult>.VisitFinallyClause(FinallyClause finallyClause)
        {
            throw new NotImplementedException();
        }

        ResolveResult IAstWalker<ResolveResult>.VisitKeyValueLikeExpression(KeyValueLikeExpression keyValue)
        {
            throw new NotImplementedException();
        }

        ResolveResult IAstWalker<ResolveResult>.VisitLiteralExpression(LiteralExpression literal)
        {
            return resolver.ResolveLiteral(literal.Value);
        }

        ResolveResult IAstWalker<ResolveResult>.VisitIdentifier(Identifier ident)
        {
            var symbol = GetSymbol(ident);
            if(symbol != null){
                if(symbol.Variable != null){
                    if(request_type)
                        context_type = symbol.Variable.Type;
                    
                    var rr = new LocalResolveResult(symbol.Variable);
                    return rr;
                }else if(request_property_or_field && symbol.PropertyOrField != null){
                    context_property_or_field = symbol.PropertyOrField;
                    return symbol.ResolveResult;
                }else if(request_type && symbol.Type != null){
                    context_type = symbol.Type;
                    var rr = new LocalResolveResult(symbol.Variable);
                    return rr;
                }else if(request_method){
                    if(symbol.Method == null)
                        throw new FormattedException("The symbol '{0}' isn't defined.", ident.Name);

                    context_method = symbol.Method;
                    var rr = new MemberResolveResult(symbol.ResolveResult, symbol.Method);
                    return rr;
                }else{
                    throw new FormattedException("I can't guess what you want: {0}.", ident.Name);
                }
            }else{
                if(context_type != null && context_type.Kind == TypeKind.Enum){
                    var enum_field = context_type.GetField(ident.Name);
                    if(enum_field == null)
                        throw new FormattedException("It is found that the symbol '{0}' doesn't represent an enum field.", ident.Name);

                    context_property_or_field = enum_field;
                    AddSymbol(ident, new Symbol{PropertyOrField = enum_field});
                    return null;
                }else if(context_type != null && request_method){
                    var method = (context_argument_types != null) ? context_type.GetMethod(ident.Name, context_argument_types) : context_type.GetMethod(ident.Name);
                    if(method == null){
                        if(!request_property_or_field)
                            throw new FormattedException("It is found that the symbol '{0}' doesn't represent a method.", ident.Name);

                        var field = context_type.GetField(ident.Name);
                        if(field == null){
                            var property = context_type.GetProperty(ident.Name);
                            if(property == null)
                                throw new FormattedException("It is found that the symbol '{0}' doesn't resolve to either a field, a property or a method", ident.Name);

                            context_property_or_field = property;
                            AddSymbol(ident, new Symbol{PropertyOrField = property, ResolveResult = new TypeResolveResult(context_type)});
                            return null;
                        }

                        context_property_or_field = field;
                        AddSymbol(ident, new Symbol{PropertyOrField = field, ResolveResult = new TypeResolveResult(context_type)});
                    }

                    context_method = method;
                    var trr = new TypeResolveResult(context_type);
                    // Don't add the method symbol so that we can find other overloads of it
                    //return new MethodGroupResolveResult(trr, ident.Name, );
                    return null;
                }else if(context_type == null && request_type){
                    var type = TypeHelpers.GetNativeType(ident.Type);
                    context_type = type.ToTypeReference().Resolve(resolver.Compilation);
                    return new TypeResolveResult(context_type);
                }else{
                    throw new FormattedException("It is found that the symbol '{0}' isn't defined.", ident.Name);
                }
            }
        }

        ResolveResult IAstWalker<ResolveResult>.VisitIntegerSequenceExpression(IntegerSequenceExpression intSeq)
        {
            throw new NotImplementedException();
        }

        ResolveResult IAstWalker<ResolveResult>.VisitIndexerExpression(IndexerExpression indexExpr)
        {
            throw new NotImplementedException();
        }

        ResolveResult IAstWalker<ResolveResult>.VisitMemberReference(MemberReferenceExpression memRef)
        {
            if(resolver_enabled){
                request_type = true;
                context_type = null;
                memRef.Target.AcceptWalker(this);

                request_type = false;
                request_method = true;
                request_property_or_field = true;
                context_method = null;
                context_property_or_field = null;
                var rr = memRef.Member.AcceptWalker(this);
                StoreResult(memRef, rr);

                return rr;
            }else{
                ScanChildren(memRef);
                return null;
            }
        }

        ResolveResult IAstWalker<ResolveResult>.VisitPathExpression(PathExpression pathExpr)
        {
            if(pathExpr.AsIdentifier != null && resolver_enabled){
                var rr = pathExpr.AsIdentifier.AcceptWalker(this);
                if(rr != null)
                    return rr;
                
                //var type_args = ResolveTypeArguments(pa)
                var lookup_mode = GetNameLookupMode(pathExpr);
                return resolver.LookupSimpleNameOrTypeName(pathExpr.AsIdentifier.Name, EmptyList<IType>.Instance, lookup_mode);
            }else{
                ScanChildren(pathExpr);
                return null;
            }
        }

        ResolveResult IAstWalker<ResolveResult>.VisitParenthesizedExpression(ParenthesizedExpression parensExpr)
        {
            throw new NotImplementedException();
        }

        ResolveResult IAstWalker<ResolveResult>.VisitObjectCreationExpression(ObjectCreationExpression creation)
        {
            throw new NotImplementedException();
        }

        ResolveResult IAstWalker<ResolveResult>.VisitSequenceInitializer(SequenceInitializer seqInitializer)
        {
            throw new NotImplementedException();
        }

        ResolveResult IAstWalker<ResolveResult>.VisitMatchClause(MatchPatternClause matchClause)
        {
            throw new NotImplementedException();
        }

        ResolveResult IAstWalker<ResolveResult>.VisitSequenceExpression(SequenceExpression seqExpr)
        {
            ScanChildren(seqExpr);
            return voidResult;
        }

        ResolveResult IAstWalker<ResolveResult>.VisitUnaryExpression(UnaryExpression unaryExpr)
        {
            throw new NotImplementedException();
        }

        ResolveResult IAstWalker<ResolveResult>.VisitSelfReferenceExpression(SelfReferenceExpression selfRef)
        {
            throw new NotImplementedException();
        }

        ResolveResult IAstWalker<ResolveResult>.VisitSuperReferenceExpression(SuperReferenceExpression superRef)
        {
            throw new NotImplementedException();
        }

        ResolveResult IAstWalker<ResolveResult>.VisitNullReferenceExpression(NullReferenceExpression nullRef)
        {
            return resolver.ResolveLiteral(null);
        }

        ResolveResult IAstWalker<ResolveResult>.VisitCommentNode(CommentNode comment)
        {
            throw new NotImplementedException();
        }

        ResolveResult IAstWalker<ResolveResult>.VisitTextNode(TextNode textNode)
        {
            throw new NotImplementedException();
        }

        ResolveResult IAstWalker<ResolveResult>.VisitSimpleType(SimpleType simpleType)
        {
            throw new NotImplementedException();
        }

        ResolveResult IAstWalker<ResolveResult>.VisitPrimitiveType(PrimitiveType primitiveType)
        {
            if(!resolver_enabled)
                return null;

            var type_code = primitiveType.KnownTypeCode;
            var type = resolver.Compilation.FindType(type_code);
            return new TypeResolveResult(type);
        }

        ResolveResult IAstWalker<ResolveResult>.VisitReferenceType(ReferenceType referenceType)
        {
            throw new NotImplementedException();
        }

        ResolveResult IAstWalker<ResolveResult>.VisitMemberType(MemberType memberType)
        {
            throw new NotImplementedException();
        }

        ResolveResult IAstWalker<ResolveResult>.VisitFunctionType(FunctionType funcType)
        {
            throw new NotImplementedException();
        }

        ResolveResult IAstWalker<ResolveResult>.VisitParameterType(ParameterType paramType)
        {
            throw new NotImplementedException();
        }

        ResolveResult IAstWalker<ResolveResult>.VisitPlaceholderType(PlaceholderType placeholderType)
        {
            throw new NotImplementedException();
        }

        ResolveResult IAstWalker<ResolveResult>.VisitImportDeclaration(ImportDeclaration importDecl)
        {
            throw new NotImplementedException();
        }

        ResolveResult IAstWalker<ResolveResult>.VisitFunctionDeclaration(FunctionDeclaration funcDecl)
        {
            var old_resolver = resolver;
            try{
                IMember member = null;
                if(unresolved_file != null)
                    member = GetMemberFromLocation(funcDecl);

                if(member == null){
                    
                }

                resolver = resolver.WithCurrentMember(member);
                ScanChildren(funcDecl);

                if(member != null)
                    return new MemberResolveResult(null, member, false);
                else
                    return errorResult;
            }
            finally{
                resolver = old_resolver;
            }
        }

        ResolveResult IAstWalker<ResolveResult>.VisitTypeDeclaration(TypeDeclaration typeDecl)
        {
            throw new NotImplementedException();
        }

        ResolveResult IAstWalker<ResolveResult>.VisitAliasDeclaration(AliasDeclaration aliasDecl)
        {
            throw new NotImplementedException();
        }

        ResolveResult IAstWalker<ResolveResult>.VisitFieldDeclaration(FieldDeclaration fieldDecl)
        {
            throw new NotImplementedException();
        }

        ResolveResult IAstWalker<ResolveResult>.VisitParameterDeclaration(ParameterDeclaration parameterDecl)
        {
            throw new NotImplementedException();
        }

        ResolveResult IAstWalker<ResolveResult>.VisitVariableInitializer(VariableInitializer initializer)
        {
            throw new NotImplementedException();
        }

        ResolveResult IAstWalker<ResolveResult>.VisitWildcardPattern(WildcardPattern wildcardPattern)
        {
            throw new NotImplementedException();
        }

        ResolveResult IAstWalker<ResolveResult>.VisitIdentifierPattern(IdentifierPattern identifierPattern)
        {
            throw new NotImplementedException();
        }

        ResolveResult IAstWalker<ResolveResult>.VisitCollectionPattern(CollectionPattern collectionPattern)
        {
            throw new NotImplementedException();
        }

        ResolveResult IAstWalker<ResolveResult>.VisitDestructuringPattern(DestructuringPattern destructuringPattern)
        {
            throw new NotImplementedException();
        }

        ResolveResult IAstWalker<ResolveResult>.VisitTuplePattern(TuplePattern tuplePattern)
        {
            throw new NotImplementedException();
        }

        ResolveResult IAstWalker<ResolveResult>.VisitExpressionPattern(ExpressionPattern exprPattern)
        {
            throw new NotImplementedException();
        }

        ResolveResult IAstWalker<ResolveResult>.VisitIgnoringRestPattern(IgnoringRestPattern restPattern)
        {
            throw new NotImplementedException();
        }

        ResolveResult IAstWalker<ResolveResult>.VisitKeyValuePattern(KeyValuePattern keyValuePattern)
        {
            throw new NotImplementedException();
        }

        ResolveResult IAstWalker<ResolveResult>.VisitPatternWithType(PatternWithType pattern)
        {
            throw new NotImplementedException();
        }

        ResolveResult IAstWalker<ResolveResult>.VisitNullNode(AstNode nullNode)
        {
            throw new NotImplementedException();
        }

        ResolveResult IAstWalker<ResolveResult>.VisitNewLine(NewLineNode newlineNode)
        {
            throw new NotImplementedException();
        }

        ResolveResult IAstWalker<ResolveResult>.VisitWhitespace(WhitespaceNode whitespaceNode)
        {
            throw new NotImplementedException();
        }

        ResolveResult IAstWalker<ResolveResult>.VisitExpressoTokenNode(ExpressoTokenNode tokenNode)
        {
            throw new NotImplementedException();
        }

        ResolveResult IAstWalker<ResolveResult>.VisitPatternPlaceholder(AstNode placeholder, Pattern child)
        {
            throw new NotImplementedException();
        }

        /*public ConversionWithTargetType GetConversionWithTargetType(Expression expr)
        {
            GetResolverStateBefore(expr);
            ResolveParentForConversion(expr);
            ConversionWithTargetType result;
            if (conversionDict.TryGetValue(expr, out result)) {
                return result;
            } else {
                ResolveResult rr = GetResolveResult(expr);
                return new ConversionWithTargetType(Conversion.IdentityConversion, rr.Type);
            }
        }*/
        #endregion

        #region Related Methods
        DomRegion MakeRegion(AstNode node)
        {
            if(unresolved_file != null)
                return new DomRegion(unresolved_file.FileName, node.StartLocation, node.EndLocation);
            else
                return node.GetRegion();
        }

        IMember GetMemberFromLocation(AstNode node)
        {
            var type_def = resolver.CurrentTypeDefinition;
            if(type_def == null)
                return null;

            var location = node.StartLocation;
            //var location = TypeSystemConvertWalker.GetStartLocationAfterAttributes(node);
            return type_def.GetMembers(m => {
                var region = m.Region;
                return !region.IsEmpty && region.Begin <= location && location < region.End;
            }).FirstOrDefault();
        }

        List<IType> GetTypeArguments(IEnumerable<AstType> typeArguments)
        {
            var list = 
                from type_arg in typeArguments
                select ResolveType(type_arg);

            return list.ToList();
        }

        NameLookupMode GetNameLookupMode(Expression expr)
        {
            if(expr.Parent is CallExpression)
                return NameLookupMode.InvocationTarget;
            else
                return NameLookupMode.Expression;
        }

        #region Local variable type inference
        IVariable MakeVariable(IType type, Identifier identifier)
        {
            return new SimpleVariable(MakeRegion(identifier), type, identifier.Name);
        }

        IVariable MakeConstant(IType type, Identifier identifier, object constantValue)
        {
            return new SimpleConstant(MakeRegion(identifier), type, identifier.Name, constantValue);
        }

        class SimpleVariable : IVariable
        {
            readonly DomRegion region;
            readonly IType type;
            readonly string name;

            public SymbolKind SymbolKind => SymbolKind.Variable;
            public string Name => name;
            public DomRegion Region => region;
            public IType Type => type;
            public virtual bool IsConst => false;
            public virtual object ConstantValue => null;

            public SimpleVariable(DomRegion region, IType type, string name)
            {
                Debug.Assert(type != null);
                Debug.Assert(name != null);

                this.region = region;
                this.type = type;
                this.name = name;
            }

            public override string ToString()
            {
                return string.Format("{0} (- {1};", name, type);
            }

            public ISymbolReference ToReference()
            {
                return new VariableReference(type.ToTypeReference(), Name, Region, IsConst, ConstantValue);
            }
        }

        sealed class SimpleConstant : SimpleVariable
        {
            object constant_value;

            public override bool IsConst => true;
            public override object ConstantValue => constant_value;

            public SimpleConstant(DomRegion region, IType type, string name, object constantValue)
                : base(region, type, name)
            {
                constant_value = constantValue;
            }

            public override string ToString()
            {
                return string.Format("{0} (- {1} = {2};", Name, Type, ConstantValue);
            }
        }
        #endregion

        static AstType RetrieveTypeFromPattern(PatternConstruct patternConstruct)
        {
            if(patternConstruct is PatternWithType typed_pattern){
                if(!(typed_pattern.Type is PlaceholderType))
                    return typed_pattern.Type;
                else if(typed_pattern.Pattern is IdentifierPattern ident_pattern)
                    return ident_pattern.Identifier.Type;
            }

            return null;
        }
        #endregion
    }
}
