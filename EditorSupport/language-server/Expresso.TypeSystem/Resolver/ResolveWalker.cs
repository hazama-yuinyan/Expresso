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
using Expresso.TypeSystem;
using ICSharpCode.NRefactory.PatternMatching;
using ICSharpCode.NRefactory.Semantics;
using ICSharpCode.NRefactory.TypeSystem;

namespace Expresso.Resolver
{
    public class ResolveWalker : IAstWalker<ResolveResult>
    {
        static readonly ResolveResult errorResult = ErrorResolveResult.UnknownError;

        ExpressoResolver resolver;
        readonly ExpressoUnresolvedFile unresolved_file;
        readonly Dictionary<AstNode, ResolveResult> resolve_result_cache = new Dictionary<AstNode, ResolveResult>();
        readonly Dictionary<AstNode, ExpressoResolver> resolver_before_dict = new Dictionary<AstNode, ExpressoResolver>();
        readonly Dictionary<AstNode, ExpressoResolver> resolver_after_dict = new Dictionary<AstNode, ExpressoResolver>();

        IResolveVisitorNavigator navigator;
        bool resolver_enabled;

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
            if(resolver == null)
                throw new ArgumentNullException(nameof(resolver));
            
            this.resolver = resolver;
            this.unresolved_file = unresolvedFile;
            this.navigator = skipAllNavigator;
        }

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

                        if(resolver != old_resolver) {
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
            
            //Log.WriteLine("Resolved '{0}' to {1}", node, result);
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
                    throw new InvalidOperationException("Could not find a resolver state for any parent of the specified node. Are you trying to resolve a node that is not a descendant of the CSharpAstResolver's root node?");

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

        public ResolveResult VisitAliasDeclaration (AliasDeclaration aliasDecl)
        {
            throw new NotImplementedException ();
        }

        public ResolveResult VisitAssignment (AssignmentExpression assignment)
        {
            throw new NotImplementedException ();
        }

        public ResolveResult VisitAst (ExpressoAst ast)
        {
            throw new NotImplementedException ();
        }

        public ResolveResult VisitBinaryExpression (BinaryExpression binaryExpr)
        {
            throw new NotImplementedException ();
        }

        public ResolveResult VisitBlock (BlockStatement block)
        {
            throw new NotImplementedException ();
        }

        public ResolveResult VisitBreakStatement (BreakStatement breakStmt)
        {
            throw new NotImplementedException ();
        }

        public ResolveResult VisitCallExpression (CallExpression call)
        {
            throw new NotImplementedException ();
        }

        public ResolveResult VisitCastExpression (CastExpression castExpr)
        {
            throw new NotImplementedException ();
        }

        public ResolveResult VisitCatchClause (CatchClause catchClause)
        {
            throw new NotImplementedException ();
        }

        public ResolveResult VisitClosureLiteralExpression (ClosureLiteralExpression closure)
        {
            throw new NotImplementedException ();
        }

        public ResolveResult VisitCollectionPattern (CollectionPattern collectionPattern)
        {
            throw new NotImplementedException ();
        }

        public ResolveResult VisitCommentNode (CommentNode comment)
        {
            throw new NotImplementedException ();
        }

        public ResolveResult VisitComprehensionExpression (ComprehensionExpression comp)
        {
            throw new NotImplementedException ();
        }

        public ResolveResult VisitComprehensionForClause (ComprehensionForClause compFor)
        {
            throw new NotImplementedException ();
        }

        public ResolveResult VisitComprehensionIfClause (ComprehensionIfClause compIf)
        {
            throw new NotImplementedException ();
        }

        public ResolveResult VisitConditionalExpression (ConditionalExpression condExpr)
        {
            throw new NotImplementedException ();
        }

        public ResolveResult VisitContinueStatement (ContinueStatement continueStmt)
        {
            throw new NotImplementedException ();
        }

        public ResolveResult VisitDestructuringPattern (DestructuringPattern destructuringPattern)
        {
            throw new NotImplementedException ();
        }

        public ResolveResult VisitDoWhileStatement (DoWhileStatement doWhileStmt)
        {
            throw new NotImplementedException ();
        }

        public ResolveResult VisitEmptyStatement (EmptyStatement emptyStmt)
        {
            throw new NotImplementedException ();
        }

        public ResolveResult VisitExpressionPattern (ExpressionPattern exprPattern)
        {
            throw new NotImplementedException ();
        }

        public ResolveResult VisitExpressionStatement (ExpressionStatement exprStmt)
        {
            throw new NotImplementedException ();
        }

        public ResolveResult VisitExpressoTokenNode (ExpressoTokenNode tokenNode)
        {
            throw new NotImplementedException ();
        }

        public ResolveResult VisitFieldDeclaration (FieldDeclaration fieldDecl)
        {
            throw new NotImplementedException ();
        }

        public ResolveResult VisitFinallyClause (FinallyClause finallyClause)
        {
            throw new NotImplementedException ();
        }

        public ResolveResult VisitForStatement (ForStatement forStmt)
        {
            throw new NotImplementedException ();
        }

        public ResolveResult VisitFunctionDeclaration (FunctionDeclaration funcDecl)
        {
            throw new NotImplementedException ();
        }

        public ResolveResult VisitFunctionType (FunctionType funcType)
        {
            throw new NotImplementedException ();
        }

        public ResolveResult VisitIdentifier (Identifier ident)
        {
            throw new NotImplementedException ();
        }

        public ResolveResult VisitIdentifierPattern (IdentifierPattern identifierPattern)
        {
            throw new NotImplementedException ();
        }

        public ResolveResult VisitIfStatement (IfStatement ifStmt)
        {
            throw new NotImplementedException ();
        }

        public ResolveResult VisitIgnoringRestPattern (IgnoringRestPattern restPattern)
        {
            throw new NotImplementedException ();
        }

        public ResolveResult VisitImportDeclaration (ImportDeclaration importDecl)
        {
            throw new NotImplementedException ();
        }

        public ResolveResult VisitIndexerExpression (IndexerExpression indexExpr)
        {
            throw new NotImplementedException ();
        }

        public ResolveResult VisitIntegerSequenceExpression (IntegerSequenceExpression intSeq)
        {
            throw new NotImplementedException ();
        }

        public ResolveResult VisitKeyValueLikeExpression (KeyValueLikeExpression keyValue)
        {
            throw new NotImplementedException ();
        }

        public ResolveResult VisitKeyValuePattern (KeyValuePattern keyValuePattern)
        {
            throw new NotImplementedException ();
        }

        public ResolveResult VisitLiteralExpression (LiteralExpression literal)
        {
            throw new NotImplementedException ();
        }

        public ResolveResult VisitMatchClause (MatchPatternClause matchClause)
        {
            throw new NotImplementedException ();
        }

        public ResolveResult VisitMatchStatement (MatchStatement matchStmt)
        {
            throw new NotImplementedException ();
        }

        public ResolveResult VisitMemberReference (MemberReferenceExpression memRef)
        {
            throw new NotImplementedException ();
        }

        public ResolveResult VisitMemberType (MemberType memberType)
        {
            throw new NotImplementedException ();
        }

        public ResolveResult VisitNewLine (NewLineNode newlineNode)
        {
            throw new NotImplementedException ();
        }

        public ResolveResult VisitNullNode (AstNode nullNode)
        {
            throw new NotImplementedException ();
        }

        public ResolveResult VisitNullReferenceExpression (NullReferenceExpression nullRef)
        {
            throw new NotImplementedException ();
        }

        public ResolveResult VisitObjectCreationExpression (ObjectCreationExpression creation)
        {
            throw new NotImplementedException ();
        }

        public ResolveResult VisitParameterDeclaration (ParameterDeclaration parameterDecl)
        {
            throw new NotImplementedException ();
        }

        public ResolveResult VisitParameterType (ParameterType paramType)
        {
            throw new NotImplementedException ();
        }

        public ResolveResult VisitParenthesizedExpression (ParenthesizedExpression parensExpr)
        {
            throw new NotImplementedException ();
        }

        public ResolveResult VisitPathExpression (PathExpression pathExpr)
        {
            throw new NotImplementedException ();
        }

        public ResolveResult VisitPatternPlaceholder (AstNode placeholder, Pattern child)
        {
            throw new NotImplementedException ();
        }

        public ResolveResult VisitPatternWithType (PatternWithType pattern)
        {
            throw new NotImplementedException ();
        }

        public ResolveResult VisitPlaceholderType (PlaceholderType placeholderType)
        {
            throw new NotImplementedException ();
        }

        public ResolveResult VisitPrimitiveType (PrimitiveType primitiveType)
        {
            throw new NotImplementedException ();
        }

        public ResolveResult VisitReferenceType (ReferenceType referenceType)
        {
            throw new NotImplementedException ();
        }

        public ResolveResult VisitReturnStatement (ReturnStatement returnStmt)
        {
            throw new NotImplementedException ();
        }

        public ResolveResult VisitSelfReferenceExpression (SelfReferenceExpression selfRef)
        {
            throw new NotImplementedException ();
        }

        public ResolveResult VisitSequenceExpression (SequenceExpression seqExpr)
        {
            throw new NotImplementedException ();
        }

        public ResolveResult VisitSequenceInitializer (SequenceInitializer seqInitializer)
        {
            throw new NotImplementedException ();
        }

        public ResolveResult VisitSimpleType (SimpleType simpleType)
        {
            throw new NotImplementedException ();
        }

        public ResolveResult VisitSuperReferenceExpression (SuperReferenceExpression superRef)
        {
            throw new NotImplementedException ();
        }

        public ResolveResult VisitTextNode (TextNode textNode)
        {
            throw new NotImplementedException ();
        }

        public ResolveResult VisitThrowStatement (ThrowStatement throwStmt)
        {
            throw new NotImplementedException ();
        }

        public ResolveResult VisitTryStatement (TryStatement tryStmt)
        {
            throw new NotImplementedException ();
        }

        public ResolveResult VisitTuplePattern (TuplePattern tuplePattern)
        {
            throw new NotImplementedException ();
        }

        public ResolveResult VisitTypeDeclaration (TypeDeclaration typeDecl)
        {
            throw new NotImplementedException ();
        }

        public ResolveResult VisitUnaryExpression (UnaryExpression unaryExpr)
        {
            throw new NotImplementedException ();
        }

        public ResolveResult VisitValueBindingForStatement (ValueBindingForStatement valueBindingForStatment)
        {
            throw new NotImplementedException ();
        }

        public ResolveResult VisitVariableDeclarationStatement (VariableDeclarationStatement varDecl)
        {
            throw new NotImplementedException ();
        }

        public ResolveResult VisitVariableInitializer (VariableInitializer initializer)
        {
            throw new NotImplementedException ();
        }

        public ResolveResult VisitWhileStatement (WhileStatement whileStmt)
        {
            throw new NotImplementedException ();
        }

        public ResolveResult VisitWhitespace (WhitespaceNode whitespaceNode)
        {
            throw new NotImplementedException ();
        }

        public ResolveResult VisitWildcardPattern (WildcardPattern wildcardPattern)
        {
            throw new NotImplementedException ();
        }

        public ResolveResult VisitYieldStatement (YieldStatement yieldStmt)
        {
            throw new NotImplementedException ();
        }
    }
}
