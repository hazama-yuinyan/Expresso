//
// ExpressoAstResolver.cs
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
using System.Diagnostics;
using System.Threading;
using Expresso.Ast;
using Expresso.TypeSystem;
using ICSharpCode.NRefactory.Semantics;
using ICSharpCode.NRefactory.TypeSystem;

namespace Expresso.Resolver
{
    /// <summary>
    /// Resolves Expresso's ast.
    /// </summary>
    /// <remarks>This class is thread-safe.</remarks>
    public class ExpressoAstResolver
    {
        readonly ExpressoResolver initial_resolver_state;
        readonly AstNode root_node;
        readonly ExpressoUnresolvedFile unresolved_file;
        readonly ResolveWalker resolve_walker;
        bool resolver_initialized;

        /// <summary>
        /// Creates a new Expresso AST resolver.
        /// Use this overload if you are resolving within a complete Expresso file.
        /// </summary>
        /// <param name="compilation">The current compilation.</param>
        /// <param name="ast">The syntax tree to be resolved.</param>
        /// <param name="unresolvedFile">
        /// Optional: Result of <see cref="SyntaxTree.ToTypeSystem()"/> for the file being resolved.
        /// <para>
        /// This is used for setting up the context on the resolver. The unresolved file must be registered in the compilation.
        /// </para>
        /// <para>
        /// When a unresolvedFile is specified, the resolver will use the member's StartLocation/EndLocation to identify
        /// member declarations in the AST with members in the type system.
        /// When no unresolvedFile is specified (<c>null</c> value for this parameter), the resolver will instead compare the
        /// member's signature in the AST with the signature in the type system.
        /// </para>
        /// </param>
        public ExpressoAstResolver(ICompilation compilation, ExpressoAst ast, ExpressoUnresolvedFile unresolvedFile = null)
        {
            if(compilation == null)
                throw new ArgumentNullException(nameof(compilation));

            if(ast == null)
                throw new ArgumentNullException(nameof(ast));

            initial_resolver_state = new ExpressoResolver(compilation);
            root_node = ast;
            unresolved_file = unresolvedFile;
            resolve_walker = new ResolveWalker(initial_resolver_state, unresolvedFile);
        }

        /// <summary>
        /// Creates a new C# AST resolver.
        /// Use this overload if you are resolving code snippets (not necessarily complete files).
        /// </summary>
        /// <param name="resolver">The resolver state at the root node (to be more precise: just outside the root node).</param>
        /// <param name="rootNode">The root node of the tree to be resolved.</param>
        /// <param name="unresolvedFile">
        /// Optional: Result of <see cref="SyntaxTree.ToTypeSystem()"/> for the file being resolved.
        /// <para>
        /// This is used for setting up the context on the resolver. The unresolved file must be registered in the compilation.
        /// </para>
        /// <para>
        /// When a unresolvedFile is specified, the resolver will use the member's StartLocation/EndLocation to identify
        /// member declarations in the AST with members in the type system.
        /// When no unresolvedFile is specified (<c>null</c> value for this parameter), the resolver will instead compare the
        /// member's signature in the AST with the signature in the type system.
        /// </para>
        /// </param>
        public ExpressoAstResolver(ExpressoResolver resolver, AstNode rootNode, ExpressoUnresolvedFile unresolvedFile = null)
        {
            if(resolver == null)
                throw new ArgumentNullException(nameof(resolver));

            if(rootNode == null)
                throw new ArgumentNullException(nameof(rootNode));

            initial_resolver_state = resolver;
            root_node = rootNode;
            unresolved_file = unresolvedFile;
            resolve_walker = new ResolveWalker(resolver, unresolvedFile);
        }

        /// <summary>
        /// Gets the type resolve context for the root resolver.
        /// </summary>
        /// <value>The root type resolve context.</value>
        public ExpressoTypeResolveContext TypeResolveContext => initial_resolver_state.CurrentTypeResolveContext;

        /// <summary>
        /// Gets the compilation for this resolver.
        /// </summary>
        /// <value>The compilation.</value>
        public ICompilation Compilation => initial_resolver_state.Compilation;

        /// <summary>
        /// Gets the root node for which this CSharpAstResolver was created.
        /// </summary>
        public AstNode RootNode => root_node;

        /// <summary>
        /// Gets the unresolved file used by this CSharpAstResolver.
        /// Can return null.
        /// </summary>
        public ExpressoUnresolvedFile UnresolvedFile => unresolved_file;

        /// <summary>
        /// Applies a resolver navigator. This will resolve the nodes requested by the navigator, and will inform the
        /// navigator of the results.
        /// This method must be called as the first operation on the CSharpAstResolver, it is invalid to apply a navigator
        /// after a portion of the file was already resolved.
        /// </summary>
        public void ApplyNavigator(IResolveVisitorNavigator navigator, CancellationToken cancellationToken = default(CancellationToken))
        {
            if(navigator == null)
                throw new ArgumentNullException(nameof(navigator));

            lock(resolve_walker){
                if(resolver_initialized)
                    throw new InvalidOperationException("Applying a navigator is only valid as the first operation on the ExpressoAstResolver.");

                resolver_initialized = true;
                resolve_walker.CancellationToken = cancellationToken;
                resolve_walker.SetNavigator(navigator);
                try{
                    resolve_walker.Scan(root_node);
                }
                finally{
                    resolve_walker.SetNavigator(null);
                    resolve_walker.CancellationToken = CancellationToken.None;
                }
            }
        }

        /// <summary>
        /// Resolves the specified node.
        /// </summary>
        public ResolveResult Resolve(AstNode node, CancellationToken cancellationToken = default(CancellationToken))
        {
            if(node == null || node.IsNull || IsUnresolvableNode(node))
                return ErrorResolveResult.UnknownError;

            lock(resolve_walker){
                InitResolver();
                resolve_walker.CancellationToken = cancellationToken;
                try {
                    ResolveResult rr = resolve_walker.GetResolveResult(node);
                    if(rr == null)
                        Debug.Fail(node.GetType() + " resolved to null.", node.StartLocation + ":'" + node + "'");
                    
                    return rr;
                }
                finally{
                    resolve_walker.CancellationToken = CancellationToken.None;
                }
            }
        }

        /// <summary>
        /// Gets the resolver state immediately before the specified node.
        /// That is, if the node is a variable declaration, the returned state will not contain the newly declared variable.
        /// </summary>
        public ExpressoResolver GetResolverStateBefore(AstNode node, CancellationToken cancellationToken = default(CancellationToken))
        {
            if(node == null || node.IsNull)
                throw new ArgumentNullException(nameof(node));

            lock(resolve_walker){
                InitResolver();
                resolve_walker.CancellationToken = cancellationToken;
                try{
                    var resolver = resolve_walker.GetResolverStateBefore(node);
                    Debug.Assert(resolver != null);
                    return resolver;
                }
                finally{
                    resolve_walker.CancellationToken = CancellationToken.None;
                }
            }
        }

        /// <summary>
        /// Gets the resolver state immediately after the specified node.
        /// That is, if the node is a variable declaration, the returned state will include the newly declared variable.
        /// </summary>
        public ExpressoResolver GetResolverStateAfter(AstNode node, CancellationToken cancellationToken = default(CancellationToken))
        {
            if(node == null || node.IsNull)
                throw new ArgumentNullException(nameof(node));

            while(node != null && IsUnresolvableNode(node))
                node = node.Parent;

            if(node == null)
                return initial_resolver_state;
            
            lock(resolve_walker){
                InitResolver();
                resolve_walker.CancellationToken = cancellationToken;
                try{
                    var resolver = resolve_walker.GetResolverStateAfter(node);
                    Debug.Assert(resolver != null);
                    return resolver;
                }
                finally{
                    resolve_walker.CancellationToken = CancellationToken.None;
                }
            }
        }

        void InitResolver()
        {
            if(!resolver_initialized){
                resolver_initialized = true;
                resolve_walker.Scan(root_node);
            }
        }

        public static bool IsUnresolvableNode(AstNode node)
        {
            if(node == null)
                throw new ArgumentNullException(nameof(node));

            if(node.NodeType == NodeType.Token){
                // Most tokens cannot be resolved, but there are a couple of special cases:
                if(node is Identifier)
                    return false;
                else if (node.Role == Roles.Identifier)
                    return !(node.Parent is ForStatement || node.Parent is ValueBindingForStatement || node.Parent is CatchClause);
                
                return true;
            }

            return (node.NodeType == NodeType.Whitespace || node is SequenceInitializer);
        }
    }
}
