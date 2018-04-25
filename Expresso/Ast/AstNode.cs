using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using ICSharpCode.NRefactory;
using ICSharpCode.NRefactory.PatternMatching;
using ICSharpCode.NRefactory.TypeSystem;
using Expresso.Formatting;

namespace Expresso.Ast
{
    using ExpressoModifiers = Expresso.Ast.Modifiers;

    /// <summary>
    /// 抽象構文木のノードの共通基底。
    /// The base class for all the abstract syntax trees.
    /// AstNodes should be cloned when absolutely needed to do that.
    /// </summary>
    public abstract class AstNode : IFreezable, INode, ICloneable
    {
        // the Root Role must be available when creating the null nodes, so we can't put it in the Roles class
        internal static readonly Role<AstNode> RootRole = new Role<AstNode>("Root");
        #region Null
        public static readonly AstNode Null = new NullAstNode();

        sealed class NullAstNode : AstNode
        {
            public override NodeType NodeType{
                get{
                    return NodeType.Unknown;
                }
            }

            public override bool IsNull{
                get{
                    return true;
                }
            }

            public override void AcceptWalker(IAstWalker walker)
            {
                walker.VisitNullNode(this);
            }

            public override TResult AcceptWalker<TResult>(IAstWalker<TResult> walker)
            {
                return walker.VisitNullNode(this);
            }

            public override TResult AcceptWalker<TResult, TData>(IAstWalker<TData, TResult> walker, TData data)
            {
                return walker.VisitNullNode(this, data);
            }

            internal protected override bool DoMatch(AstNode other, Match match)
            {
                return other == null || other.IsNull;
            }
        }
        #endregion

        #region PatternPlaceholder
        public static implicit operator AstNode(Pattern pattern)
        {
            return (pattern != null) ? new PatternPlaceholder(pattern) : null;
        }

        sealed class PatternPlaceholder : AstNode, INode
        {
            readonly Pattern child;

            public PatternPlaceholder(Pattern child)
            {
                this.child = child;
            }

            public override NodeType NodeType{
                get{return NodeType.Pattern;}
            }

            public override void AcceptWalker(IAstWalker walker)
            {
                walker.VisitPatternPlaceholder(this, child);
            }

            public override TResult AcceptWalker<TResult>(IAstWalker<TResult> walker)
            {
                return walker.VisitPatternPlaceholder(this, child);
            }

            public override TResult AcceptWalker<TResult, TData>(IAstWalker<TData, TResult> walker, TData data)
            {
                return walker.VisitPatternPlaceholder(this, child, data);
            }

            protected internal override bool DoMatch(AstNode other, Match match)
            {
                return child.DoMatch(other, match);
            }

            bool INode.DoMatchCollection(Role role, INode pos, Match match, BacktrackingInfo backtrackingInfo)
            {
                return child.DoMatchCollection(role, pos, match, backtrackingInfo);
            }
        }
        #endregion

        // Flags, from least significant to most significant bits:
        // - Role.RoleIndexBits: role index
        // - 1 bit: IsFrozen
        protected uint flags = RootRole.Index;
        // Derived classes may also use a few bits,
        // for example Identifier uses 1 bit for IsVerbatim

        const uint RoleIndexMask = (1u << Role.RoleIndexBits) - 1;
        const uint FrozenBits = 1u << Role.RoleIndexBits;
        protected const uint AstNodeFlagsUsedBits = Role.RoleIndexBits + 1;

        protected readonly TextLocation start_loc, end_loc;
        AstNode parent, prev_sibling, next_sibling, first_child, last_child;

        #region Properties
        /// <summary>
        /// Gets the start location.
        /// </summary>
        /// <remarks>
        /// Derived classes have opportunities to implement their own logics of determining
        /// the start location within the nodes.
        /// </remarks>
        /// <value>The start location in the source code.</value>
        public virtual TextLocation StartLocation{
            get{return start_loc;}
        }

        /// <summary>
        /// Gets the end location.
        /// </summary>
        /// <remarks>
        /// Derived classes have opportunities to implement their own logics of determining
        /// the end location within the nodes.
        /// </remarks>
        /// <value>The end location in the source code.</value>
        public virtual TextLocation EndLocation{
            get{return end_loc;}
        }

        /// <summary>
        /// Gets the parent node.
        /// </summary>
        /// <value>The parent AST node.</value>
        public AstNode Parent{
            get{return parent;}
        }

        /// <summary>
        /// Gets the next node.
        /// </summary>
        /// <value>The next AST node.</value>
        public AstNode NextSibling{
            get{return next_sibling;}
        }

        /// <summary>
        /// Gets the previous node.
        /// </summary>
        /// <value>The previous AST node.</value>
        public AstNode PrevSibling{
            get{return prev_sibling;}
        }

        /// <summary>
        /// Gets the first child node.
        /// </summary>
        /// <value>The first child AST node.</value>
        public AstNode FirstChild{
            get{return first_child;}
        }

        /// <summary>
        /// Gets the last child node.
        /// </summary>
        /// <value>The last child AST node.</value>
        public AstNode LastChild{
            get{return last_child;}
        }

        /// <summary>
        /// Determines whether this AST node has any child AST nodes.
        /// </summary>
        /// <value><c>true</c> if this node has children; otherwise, <c>false</c>.</value>
        public bool HasChildren{
            get{return first_child != null && !first_child.IsNull;}
        }

        public DomRegion Region{
            get{
                return new DomRegion(start_loc, end_loc);
            }
        }

        /// <summary>
        /// ノードタイプ。
		/// The node's type.
        /// </summary>
        public abstract NodeType NodeType{get;}

        /// <summary>
        /// Gets whether this node is a null node.
        /// </summary>
        /// <value><c>true</c> if this node is a null node; otherwise, <c>false</c>.</value>
        public virtual bool IsNull{
            get{return false;}
        }
        #endregion

        protected AstNode()
        {
        }

        protected AstNode(TextLocation startLoc, TextLocation endLoc)
        {
            start_loc = startLoc;
            end_loc = endLoc;
        }

        #region IFreezable implementation

        public void Freeze()
        {
            if(!IsFrozen){
                for(AstNode child = first_child; child != null; child = child.next_sibling)
                    child.Freeze();

                flags |= FrozenBits;
            }
        }

        public bool IsFrozen{
            get{
                return (flags & FrozenBits) != 0;
            }
        }

        #endregion

        protected void ThrowIfFrozen()
        {
            if(IsFrozen)
                throw new InvalidOperationException("Can not mutate frozen " + GetType().Name);
        }

        /// <summary>
        /// Clones the whole subtree rooted at this AST node.
        /// </summary>
        public AstNode Clone()
        {
            AstNode cloned = (AstNode)MemberwiseClone();
            // First, reset the shallow pointer copies
            cloned.parent = null;
            cloned.first_child = null;
            cloned.last_child = null;
            cloned.next_sibling = null;
            cloned.prev_sibling = null;
            cloned.flags &= ~FrozenBits; // unfreeze the copy

            // Then perform deep copy
            for(AstNode cur = first_child; cur != null; cur = cur.next_sibling)
                cloned.AddChildUnsafe(cur.Clone(), cur.Role);

            return cloned;
        }

        #region ICloneable implementation

        object ICloneable.Clone()
        {
            return Clone();
        }

        #endregion

        #region Role related members
        public Role Role{
            get{
                return Role.GetByIndex(flags & RoleIndexMask);
            }

            set{
                if(value == null)
                    throw new ArgumentNullException("value");

                if(!value.IsValid(this))
                    throw new ArgumentException("This node is invalid in the new role.");

                ThrowIfFrozen();
                SetRole(value);
            }
        }

        internal uint RoleIndex{
            get{return flags & RoleIndexMask;}
        }

        void SetRole(Role role)
        {
            flags = (flags & ~RoleIndexMask) | role.Index;
        }
        #endregion

        /// <summary>
        /// Accepts the ast walker.
        /// </summary>
        /// <param name="walker">Walker.</param>
        public abstract void AcceptWalker(IAstWalker walker);
        public abstract TResult AcceptWalker<TResult>(IAstWalker<TResult> walker);
        public abstract TResult AcceptWalker<TResult, TData>(IAstWalker<TData, TResult> walker, TData data);
		
        #region Common AstNode methods
        #region Node manipulation
        /// <summary>
        /// Adds a child to this node.
        /// </summary>
        /// <param name="child">The child node to be added.</param>
        /// <param name="role">The role that the child node will have.</param>
        /// <typeparam name="T">The type of the child node.</typeparam>
        public void AddChild<T>(T child, Role<T> role) where T : AstNode
        {
            if(role == null)
                throw new ArgumentNullException(nameof(role));

            if(child == null || child.IsNull)
                return;

            ThrowIfFrozen();
            if(child == this)
                throw new ArgumentException("Can not add a node to itself as a child", nameof(child));

            if(child.parent != null)
                throw new ArgumentException("Node is already used in another tree.", nameof(child));

            if(child.IsFrozen)
                throw new ArgumentException("Can not add a frozen node.", nameof(child));

            AddChildUnsafe(child, role);
        }

        public void AddChildWithExistingRole(AstNode child)
        {
            if(child == null || child.IsNull)
                return;

            ThrowIfFrozen();
            if(child == this)
                throw new ArgumentException("Can not add a node to itself as a child", nameof(child));

            if(child.parent != null)
                throw new ArgumentException("Node is already used in another truee.", nameof(child));

            if(child.IsFrozen)
                throw new ArgumentException("Can not add a frozen node.", nameof(child));

            AddChildUnsafe(child, child.Role);
        }

        /// <summary>
        /// Adds a child without performing any safety checks.
        /// </summary>
        internal void AddChildUnsafe(AstNode child, Role role)
        {
            child.parent = this;
            child.SetRole(role);

            if(first_child == null){
                last_child = first_child = child;
            }else{
                last_child.next_sibling = child;
                child.prev_sibling = last_child;
                last_child = child;
            }
        }

        /// <summary>
        /// Performs insertion on this node.
        /// </summary>
        /// <param name="nextSibling">The new node will be inserted just before this node.</param>
        /// <param name="child">The node to be inserted.</param>
        /// <param name="role">The role that the child node will have.</param>
        /// <typeparam name="T">The type of the child node.</typeparam>
        public void InsertChildBefore<T>(AstNode nextSibling, T child, Role<T> role) where T : AstNode
        {
            if(role == null)
                throw new ArgumentNullException(nameof(role));

            if(nextSibling == null || nextSibling.IsNull){
                AddChild(child, role);
                return;
            }

            if(child == null || child.IsNull)
                return;

            ThrowIfFrozen();
            if(child.parent != null)
                throw new ArgumentException("Node is already used in another tree.", nameof(child));

            if(child.IsFrozen)
                throw new ArgumentException("Can not add a frozen node", nameof(child));

            if(nextSibling.parent == null)
                throw new ArgumentException("NextSibling is not a child of this node.", nameof(nextSibling));

            // No need to test for "Can not add children to null nodes",
            // as there isn't any valid nextSibling in null nodes.
            InsertChildBeforeUnsafe(nextSibling, child, role);
        }

        internal void InsertChildBeforeUnsafe(AstNode nextSibling, AstNode child, Role role)
        {
            child.parent = this;
            child.SetRole(role);
            child.next_sibling = nextSibling;
            child.prev_sibling = nextSibling.prev_sibling;

            if(nextSibling.prev_sibling != null){
                Debug.Assert(nextSibling.prev_sibling.next_sibling == nextSibling);
                nextSibling.prev_sibling.next_sibling = child;
            }else{
                Debug.Assert(first_child == nextSibling);
                first_child = child;
            }
            nextSibling.prev_sibling = child;
        }

        /// <summary>
        /// Performs insertion on this node.
        /// </summary>
        /// <param name="prevSibling">The child node will be inserted right after this node.</param>
        /// <param name="child">The child node to be inserted.</param>
        /// <typeparam name="T">The type of the child node.</typeparam>
        public void InsertChildAfter<T>(AstNode prevSibling, T child, Role<T> role) where T : AstNode
        {
            InsertChildBefore((prevSibling == null) ? first_child : prevSibling.next_sibling, child, role);
        }

        /// <summary>
        /// Removes this node from its parent.
        /// </summary>
        public void Remove()
        {
            if(parent != null){
                ThrowIfFrozen();
                if(prev_sibling != null){
                    Debug.Assert(prev_sibling.next_sibling == this);
                    prev_sibling.next_sibling = next_sibling;
                }else{
                    Debug.Assert(parent.first_child == this);
                    parent.first_child = next_sibling;
                }

                if(next_sibling != null){
                    Debug.Assert(next_sibling.prev_sibling == this);
                    next_sibling.prev_sibling = prev_sibling;
                }else{
                    Debug.Assert(parent.last_child == this);
                    parent.last_child = prev_sibling;
                }

                parent = null;
                prev_sibling = null;
                next_sibling = null;
            }
        }

        /// <summary>
        /// Replaces this node with the new one.
        /// </summary>
        /// <param name="newNode">The new node.</param>
        public void ReplaceWith(AstNode newNode)
        {
            if(newNode == null || newNode.IsNull){
                Remove();
                return;
            }

            if(newNode == this)
                return;     // nothing to do...

            if(parent == null)
                throw new InvalidOperationException((IsNull ? "Can not replace the null nodes" : "Can not replace the root node"));

            ThrowIfFrozen();
            // Because this method doesn't statically check the new node's type with the role,
            // we perform a runtime test:
            if(!Role.IsValid(newNode)){
                throw new ArgumentException(
                    string.Format("The new node '{0}' is not valid in the role {1}",
                                  newNode.GetType().Name, Role.ToString()), nameof(newNode));
            }

            if(newNode.parent != null){
                // newNode is used within this tree?
                if(newNode.Ancestors.Contains(this)){
                    // e.g. "parenthesizedExpr.ReplaceWith(parenthesizedExpr.Expression);"
                    // enable automatic removal
                    newNode.Remove();
                }else{
                    throw new ArgumentException("Node is already used in another tree.", nameof(newNode));
                }
            }

            if(newNode.IsFrozen)
                throw new ArgumentException("Can not add a frozen node", nameof(newNode));

            newNode.parent = parent;
            newNode.SetRole(Role);
            newNode.prev_sibling = prev_sibling;
            newNode.next_sibling = next_sibling;

            if(prev_sibling != null){
                Debug.Assert(prev_sibling.next_sibling == this);
                prev_sibling.next_sibling = newNode;
            }else{
                Debug.Assert(parent.first_child == this);
                parent.first_child = newNode;
            }

            if(next_sibling != null){
                Debug.Assert(next_sibling.prev_sibling == this);
                next_sibling.prev_sibling = newNode;
            }else{
                Debug.Assert(parent.last_child == this);
                parent.last_child = newNode;
            }

            parent = null;
            prev_sibling = null;
            next_sibling = null;
        }

        /// <summary>
        /// Replaces this node with a new one using a delegate.
        /// </summary>
        /// <returns>The new <see cref="Expresso.Ast.AstNode"/>.</returns>
        /// <param name="replaceFunction">
        /// Replace function, which takes one <see cref="Expresso.Ast.AstNode"/> as input and
        /// returns the new <see cref="Expresso.Ast.AstNode"/> that will be replaced.
        /// </param>
        public AstNode ReplaceWith(Func<AstNode, AstNode> replaceFunction)
        {
            if(replaceFunction == null)
                throw new ArgumentException("replaceFunction");

            if(parent == null)
                throw new InvalidOperationException((IsNull ? "Can not replace the null nodes" : "Can not replace the root node"));

            AstNode old_parent = parent, old_successor = next_sibling;
            Role old_role = Role;
            Remove();

            AstNode replacement = replaceFunction(this);
            if(old_successor != null && old_successor.parent != old_parent)
                throw new InvalidOperationException("replace function changed nextSibling of node being replaced?");

            if(replacement != null){
                if(replacement.parent != null)
                    throw new InvalidOperationException("replace function must return the root of a tree");

                if(!old_role.IsValid(replacement)){
                    throw new InvalidOperationException(
                        string.Format("The new node '{0}' is invalid in the role {1}", replacement.GetType().Name, old_role.ToString()));
                }
                if(old_successor != null)
                    old_parent.InsertChildBeforeUnsafe(old_successor, replacement, old_role);
                else
                    old_parent.AddChildUnsafe(replacement, old_role);
            }

            return replacement;
        }

        protected void SetChildByRole<T>(Role<T> role, T newChild) where T : AstNode
        {
            AstNode old_child = GetChildByRole(role);
            if(old_child.IsNull)
                AddChild(newChild, role);
            else
                old_child.ReplaceWith(newChild);
        }
        #endregion

        #region Traversing nodes
        /// <summary>
        /// Traverses all child nodes of this one.
        /// </summary>
        public IEnumerable<AstNode> Children{
            get{
                AstNode next;
                for(AstNode cur = first_child; cur != null; cur = next){
                    Debug.Assert(cur.parent == this);
                    // Remember next before yielding cur
                    // This allows removing/replacing nodes while iterating through the list
                    next = cur.next_sibling;
                    yield return cur;
                }
            }
        }

        /// <summary>
        /// Traverses each sibling node including this one.
        /// </summary>
        /// <value>The siblings.</value>
        public IEnumerable<AstNode> Siblings{
            get{
                for(AstNode cur = this; cur != null; cur = cur.next_sibling)
                    yield return cur;
            }
        }

        /// <summary>
        /// Gets the ancestors of this node (excluding this node itself).
        /// </summary>
        public IEnumerable<AstNode> Ancestors{
            get{
                for(AstNode cur = parent; cur != null; cur = cur.parent)
                    yield return cur;
            }
        }

        /// <summary>
        /// Gets the ancestors of this node (including this node itself).
        /// </summary>
        public IEnumerable<AstNode> AncestorsAndSelf{
            get{
                for(AstNode cur = this; cur != null; cur = cur.parent)
                    yield return cur;
            }
        }

        #region Retrieving decendants
        /// <summary>
        /// Gets all decendant nodes of this node (excluding this node itself) in pre-order.
        /// </summary>
        public IEnumerable<AstNode> Descendants{
            get{
                return GetDescendantNodesImpl(false);
            }
        }

        /// <summary>
        /// Gets all decendant nodes of this node (including this node itself) in pre-order.
        /// </summary>
        public IEnumerable<AstNode> DescendantsAndSelf{
            get{
                return GetDescendantNodesImpl(true);
            }
        }

        static bool IsInsideRegion(DomRegion region, AstNode pos)
        {
            if(region.IsEmpty)
                return true;

            var node_region = pos.Region;
            return region.IntersectsWith(node_region) || region.OverlapsWith(node_region);
        }

        /// <summary>
        /// Gets all the descendant nodes specified by the predicate(exluding this node itself).
        /// </summary>
        /// <returns>All the descendant nodes found.</returns>
        /// <param name="descendIntoChidlren">
        /// A delegate which determines whether it should descend into the children of the input node.
        /// If it is null, this method gives back all descendant nodes of this node.
        /// </param>
        public IEnumerable<AstNode> GetDescendantNodes(Func<AstNode, bool> descendIntoChidlren = null)
        {
            return GetDescendantNodesImpl(false, new DomRegion(), descendIntoChidlren);
        }

        /// <summary>
        /// Gets all the descendant nodes specified by region and the predicate(exluding this node itself).
        /// </summary>
        /// <returns>All the descendant nodes found.</returns>
        /// <param name="region">The region within which we'll be searching for descendant nodes.</param>
        /// <param name="descendIntoChildren">
        /// A delegate which determines whether it should descend into the children of the input node.
        /// If it is null, this method gives back all descendant nodes of this node.
        /// </param>
        public IEnumerable<AstNode> GetDescendantNodes(DomRegion region, Func<AstNode, bool> descendIntoChildren = null)
        {
            return GetDescendantNodesImpl(false, region, descendIntoChildren);
        }

        /// <summary>
        /// Gets all the descendant nodes specified by the predicate(inluding this node itself).
        /// </summary>
        /// <returns>All the descendant nodes found.</returns>
        /// <param name="descendIntoChidlren">
        /// A delegate which determines whether it should descend into the children of the input node.
        /// If it is null, this method gives back all descendant nodes of this node.
        /// </param>
        public IEnumerable<AstNode> GetDescendantNodesAndSelf(Func<AstNode, bool> descendIntoChildren = null)
        {
            return GetDescendantNodesImpl(true, new DomRegion(), descendIntoChildren);
        }

        /// <summary>
        /// Gets all the descendant nodes specified by region and the predicate(inluding this node itself).
        /// </summary>
        /// <returns>All the descendant nodes found.</returns>
        /// <param name="region">The region within which we'll be searching for descendant nodes.</param>
        /// <param name="descendIntoChildren">
        /// A delegate which determines whether it should descend into the children of the input node.
        /// If it is null, this method gives back all descendant nodes of this node.
        /// </param>
        public IEnumerable<AstNode> GetDescendantNodesAndSelf(DomRegion region,
            Func<AstNode, bool> descendIntoChildren = null)
        {
            return GetDescendantNodesImpl(true, region, descendIntoChildren);
        }

        IEnumerable<AstNode> GetDescendantNodesImpl(bool includeSelf, DomRegion region = new DomRegion(),
            Func<AstNode, bool> descendIntoChildren = null)
        {
            if(includeSelf){
                if(IsInsideRegion(region, this))
                    yield return this;

                if(descendIntoChildren != null && !descendIntoChildren(this))
                    yield break;
            }

            var stack = new Stack<AstNode>();
            stack.Push(null);
            AstNode pos = first_child;
            while(pos != null){
                // Remember next before yielding pos.
                // This allows removing/replacing nodes while iterating through the list.
                if(pos.next_sibling != null)
                    stack.Push(pos.next_sibling);

                if(IsInsideRegion(region, pos))
                    yield return pos;

                if(pos.first_child != null && (descendIntoChildren == null || descendIntoChildren(pos)))
                    pos = pos.first_child;
                else
                    pos = stack.Pop();
            }
        }
        #endregion

        #region Retrieving children
        public T GetChildByRole<T>(Role<T> role) where T : AstNode
        {
            if(role == null)
                throw new ArgumentNullException(nameof(role));

            uint role_index = role.Index;
            for(var cur = first_child; cur != null; cur = cur.next_sibling){
                if((cur.flags & RoleIndexMask) == role_index)
                    return (T)cur;
            }

            return role.NullObject;
        }

        /// <summary>
        /// Traverses child nodes that match the predicate.
        /// </summary>
        /// <param name="pred">
        /// A delegate that determines which nodes must be traversed and which must not.
        /// If it is null, this method will traverses all the children including this one.
        /// </param>
        public IEnumerable<AstNode> GetChildren(Predicate<AstNode> pred)
        {
            if(pred == null)
                pred = n => true;

            AstNode node = this;
            while(node != null){
                if(pred(node))
                    yield return node;

                node = node.first_child;
            }
        }

        /// <summary>
        /// Gets the children by role.
        /// </summary>
        /// <returns>Child nodes having the specified role.</returns>
        /// <param name="role">Role.</param>
        /// <typeparam name="T">The type to which the target values will be casted.</typeparam>
        public AstNodeCollection<T> GetChildrenByRole<T>(Role<T> role)
            where T : AstNode
        {
            return new AstNodeCollection<T>(this, role);
        }

        /// <summary>
        /// Iterates over each child and tries to find the first match to the predicate.
        /// </summary>
        /// <returns>The found node, or null if none is found.</returns>
        /// <param name="pred">A delegate that determines which node is searched for.</param>
        /// <exception cref="InvalidOperationException">When multiple nodes match the predicate, it'll be thrown.</exception>
        public AstNode FindNode(Predicate<AstNode> pred)
        {
            AstNode node = this, result = null;
            var queue = new Queue<AstNode>();
            while(node != null){
                if(pred(node)){
                    if(result != null)
                        throw new InvalidOperationException("Multiple nodes found!");
                    else
                        result = node;
                }

                if(node.next_sibling != null){
                    if(node.first_child != null)
                        queue.Enqueue(node);

                    node = node.next_sibling;
                }else{
                    if(queue.Any() && node.first_child != null)
                        queue.Enqueue(node);

                    var next_parent = queue.Any() ? queue.Dequeue() : node;
                    node = next_parent.first_child;
                }
            }

            return result;
        }

        /// <summary>
        /// Iterates over each node and finds all matches to the predicate.
        /// </summary>
        /// <remarks>
        /// This method traverses all the sibling and child nodes rooted at this one.
        /// </remarks>
        /// <returns>An IEnumerable that enumerates the matched nodes.</returns>
        /// <param name="pred">A delegate that determines which node should be retrieved.</param>
        public IEnumerable<AstNode> FindNodes(Predicate<AstNode> pred)
        {
            AstNode node = this;
            var queue = new Queue<AstNode>();
            while(node != null){
                if(pred(node))
                    yield return node;

                if(node.next_sibling != null){
                    queue.Enqueue(node);
                    node = node.next_sibling;
                }else{
                    var next_parent = queue.Any() ? queue.Dequeue() : node;
                    node = next_parent.first_child;
                }
            }
        }
        #endregion
        #endregion

        #region Pattern Matching
        protected static bool MatchString(string pattern, string text)
        {
            return Pattern.MatchString(pattern, text);
        }

        protected internal abstract bool DoMatch(AstNode other, Match match);

        bool INode.DoMatch(INode other, Match match)
        {
            AstNode o = other as AstNode;
            // try matching if other is null, or other is an AstNode
            return (other == null || o != null) && DoMatch(o, match);
        }

        bool INode.DoMatchCollection(Role role, INode pos, Match match, BacktrackingInfo backtrakingInfo)
        {
            AstNode o = pos as AstNode;
            return (pos == null || o != null) && DoMatch(o, match);
        }

        INode INode.NextSibling{
            get{return next_sibling;}
        }

        INode INode.FirstChild{
            get{return first_child;}
        }
        #endregion

        public AstNode GetNextNode()
        {
            if(next_sibling != null)
                return next_sibling;

            if(parent != null)
                return parent.GetNextNode();

            return null;
        }

        /// <summary>
        /// Gets the next node which fulfills the given predicate.
        /// </summary>
        /// <returns>The next node.</returns>
        /// <param name="pred">The predicate.</param>
        public AstNode GetNextNode(Predicate<AstNode> pred)
        {
            var next = GetNextNode();
            while(next != null && !pred(next))
                next = next.GetNextNode();

            return next;
        }

        public AstNode GetPrevNode()
        {
            if(prev_sibling != null)
                return prev_sibling;

            if(parent != null)
                return parent.GetPrevNode();

            return null;
        }

        /// <summary>
        /// Gets the previous node which fulfills the given predicate.
        /// </summary>
        /// <returns>The previous node.</returns>
        /// <param name="pred">The predicate.</param>
        public AstNode GetPrevNode(Predicate<AstNode> pred)
        {
            var prev = GetPrevNode();
            while(prev != null && !pred(prev))
                prev = prev.prev_sibling;

            return prev;
        }

        /// <summary>
        /// Gets the next sibling node which fulfills the given predicate.
        /// </summary>
        /// <returns>The next sibling node.</returns>
        /// <param name="pred">The predicate.</param>
        public AstNode GetNextSibling(Predicate<AstNode> pred)
        {
            var next = next_sibling;
            while(next != null && !pred(next))
                next = next.next_sibling;

            return next;
        }

        /// <summary>
        /// Gets the previous sibling node which fulfills the given predicate.
        /// </summary>
        /// <returns>The previous sibling node.</returns>
        /// <param name="pred">The predicate.</param>
        public AstNode GetPrevSibling(Predicate<AstNode> pred)
        {
            var prev = prev_sibling;
            while(prev != null && !pred(prev))
                prev = prev.prev_sibling;

            return prev;
        }

        #region Retrieve node methods based on location
        #region GetNodeAt
        /// <summary>
        /// Gets the node specified by pred at the location line, column.
        /// This is useful for getting a specific node from the tree.
        /// For example, searching the current method declaration.
        /// (End exclusive)
        /// </summary>
        /// <returns>The <see cref="Expresso.Ast.AstNode"/>.</returns>
        public AstNode GetNodeAt(int line, int column, Predicate<AstNode> pred = null)
        {
            return GetNodeAt(new TextLocation(line, column), pred);
        }

        /// <summary>
        /// Gets the node specified by pred at location.
        /// This is useful for getting a specific node from the tree.
        /// For example, searching the current method declaration.
        /// </summary>
        /// <returns>The <see cref="Expresso.Ast.AstNode"/>.</returns>
        public AstNode GetNodeAt(TextLocation location, Predicate<AstNode> pred = null)
        {
            AstNode node = this, result = null;
            while(node.first_child != null){
                var child = node.first_child;
                while(child != null){
                    if(child.start_loc <= location && location < child.end_loc){
                        if(pred == null || pred(child))
                            result = child;

                        node = child;
                        break;
                    }
                    child = child.next_sibling;
                }
                // Found no better child node - therefore the parent is the right one.
                if(child == null)
                    break;
            }

            return result;
        }

        /// <summary>
        /// Gets the node specified by T at line and column.
        /// This is useful for getting a specific node from the tree.
        /// For example, searching the current method declaration.
        /// </summary>
        public T GetNodeAt<T>(int line, int column) where T : AstNode
        {
            return GetNodeAt<T>(new TextLocation(line, column));
        }

        /// <summary>
        /// Gets the node specified by T at location.
        /// This is useful for getting a specific node from the tree.
        /// For example, searching the current method declaration.
        /// </summary>
        public T GetNodeAt<T>(TextLocation location) where T : AstNode
        {
            T result = null;
            AstNode node = this;
            while(node.first_child != null){
                var child = node.first_child;
                while(child != null){
                    if(child.start_loc <= location && location < child.end_loc){
                        if(child is T)
                            result = (T)child;

                        node = child;
                        break;
                    }
                    child = child.next_sibling;
                }
                // Found no better child node - therefore the parent is the right one.
                if(child == null)
                    break;
            }

            return result;
        }
        #endregion

        #region GetAdjacentNodeAt
        /// <summary>
        /// Gets the node specified by pred at the location line, column.
        /// This is useful for getting a specific node from the tree.
        /// For example, searching the current method declaration.
        /// (End exclusive)
        /// </summary>
        /// <returns>The <see cref="Expresso.Ast.AstNode"/>.</returns>
        public AstNode GetAdjacentNodeAt(int line, int column, Predicate<AstNode> pred = null)
        {
            return GetAdjacentNodeAt(new TextLocation(line, column), pred);
        }

        /// <summary>
        /// Gets the node specified by pred at location.
        /// This is useful for getting a specific node from the tree.
        /// For example, searching the current method declaration.
        /// </summary>
        /// <returns>The <see cref="Expresso.Ast.AstNode"/>.</returns>
        public AstNode GetAdjacentNodeAt(TextLocation location, Predicate<AstNode> pred = null)
        {
            AstNode node = this, result = null;
            while(node.first_child != null){
                var child = node.first_child;
                while(child != null){
                    if(child.start_loc <= location && location <= child.end_loc){
                        if(pred == null || pred(child))
                            result = child;

                        node = child;
                        break;
                    }
                    child = child.next_sibling;
                }
                // Found no better child node - therefore the parent is the right one.
                if(child == null)
                    break;
            }

            return result;
        }

        /// <summary>
        /// Gets the node specified by T at line and column.
        /// This is useful for getting a specific node from the tree.
        /// For example, searching the current method declaration.
        /// </summary>
        public T GetAdjacentNodeAt<T>(int line, int column) where T : AstNode
        {
            return GetAdjacentNodeAt<T>(new TextLocation(line, column));
        }

        /// <summary>
        /// Gets the node specified by T at location.
        /// This is useful for getting a specific node from the tree.
        /// For example, searching the current method declaration.
        /// </summary>
        public T GetAdjacentNodeAt<T>(TextLocation location) where T : AstNode
        {
            T result = null;
            AstNode node = this;
            while(node.first_child != null){
                var child = node.first_child;
                while(child != null){
                    if(child.start_loc <= location && location <= child.end_loc){
                        if(child is T)
                            result = (T)child;

                        node = child;
                        break;
                    }
                    child = child.next_sibling;
                }
                // Found no better child node - therefore the parent is the right one.
                if(child == null)
                    break;
            }

            return result;
        }
        #endregion

        /// <summary>
        /// Gets the node that fully contains the range from startLocation to endLocation.
        /// </summary>
        public AstNode GetNodeContaining(TextLocation startLocation, TextLocation endLocation)
        {
            for(AstNode child = first_child; child != null; child = child.next_sibling){
                if(child.start_loc <= startLocation && endLocation <= child.end_loc)
                    return child.GetNodeContaining(startLocation, endLocation);
            }

            return this;
        }

        /// <summary>
        /// Gets the root nodes of all subtrees that are fully contained in the given region.
        /// </summary>
        public IEnumerable<AstNode> GetNodesBetween(int startLine, int startColumn,
            int endLine, int endColumn)
        {
            return GetNodesBetween(new TextLocation(startLine, startColumn), new TextLocation(endLine, endColumn));
        }

        /// <summary>
        /// Gets the root nodes of all subtrees that are fully contained between <paramref name="startLoc"/> and <paramref name="endLoc"/>.
        /// </summary>
        public IEnumerable<AstNode> GetNodesBetween(TextLocation startLoc, TextLocation endLoc)
        {
            AstNode node = this;
            while(node != null){
                AstNode next;
                if(startLoc <= node.start_loc && node.end_loc <= endLoc){
                    // Remember next before yielding node.
                    // This allows removing/replacing the node while iterating through the list.
                    next = node.GetNextNode();
                    yield return node;
                }else{
                    if(node.end_loc <= startLoc)
                        next = node.GetNextNode();
                    else
                        next = node.first_child;
                }

                if(next != null && next.start_loc > endLoc)
                    yield break;

                node = next;
            }
        }
        #endregion

        /// <summary>
        /// Gets the region from StartLocation to EndLocation for this node.
        /// The file name of the region is set based on the parent ExpressoAst's file name.
        /// If this node is not connected to a whole compilation, the file name will be null.
        /// </summary>
        /// <returns>The region.</returns>
        public DomRegion GetRegion()
        {
            var sytanx_tree = (Ancestors.LastOrDefault() ?? this) as ExpressoAst;
            string file_name = (sytanx_tree != null) ? sytanx_tree.Name : null;
            return new DomRegion(file_name, start_loc, end_loc);
        }

        /// <summary>
        /// Determines whether given coordinates(line and column) are in this node.
        /// </summary>
        /// <returns>
        /// <code>true</code>, if the given coordinates are between StartLocation and EndLocation(exclusive); otherwise, false.
        /// </returns>
        public bool Contains(int line, int column)
        {
            return Contains(new TextLocation(line, column));
        }

        /// <summary>
        /// Determines whether the specified location is inside this node.
        /// (StartLocation inclusive but EndLocation is exclusive)
        /// </summary>
        /// <returns>true, if the location is inside this node; otherwise false</returns>
        public bool Contains(TextLocation loc)
        {
            return start_loc <= loc && loc < end_loc;
        }

        /// <summary>
        /// Determines whether the given coordinates are between this node.
        /// </summary>
        /// <returns>
        /// <c>true</c>, if given coordinates are between the node; otherwise, <c>false</c>.</returns>
        public bool IsBetween(int line, int column)
        {
            return IsBetween(new TextLocation(line, column));
        }

        /// <summary>
        /// Determines whether the specified location points at a location in this node.
        /// (StartLocation and EndLocation inclusive)
        /// </summary>
        /// <returns><c>true</c> if this instance is between the specified loc; otherwise, <c>false</c>.</returns>
        public bool IsBetween(TextLocation loc)
        {
            return start_loc <= loc && loc <= end_loc;
        }
        #endregion

        public virtual string ToString(ExpressoFormattingOptions formattingOptions)
        {
            if(IsNull)
                return "";

            var sw = new StringWriter();
            //AcceptWalker(new ExpressoOutputWalker(sw, formattingOptions ?? FormattingOptionsFactory.CreateMono()));
            return sw.ToString();
        }

        public sealed override string ToString()
        {
            if(IsNull)
                return "<null>";

            var sw = new StringWriter();
            AcceptWalker(new DebugOutputWalker(sw));
            return sw.ToString();
        }

		#region AST node factory methods
		
        public static Identifier MakeIdentifier(string name, ExpressoModifiers modifiers = ExpressoModifiers.None, TextLocation loc = default)
		{
            return new Identifier(name, modifiers, loc);
		}

        public static Identifier MakeIdentifier(string name, AstType type, ExpressoModifiers modifiers = ExpressoModifiers.None, TextLocation loc = default)
        {
            return new Identifier(name, type, modifiers, loc);
        }

        public static ExpressoAst MakeModuleDef(string moduleName,
            IEnumerable<EntityDeclaration> decls, IEnumerable<ImportDeclaration> imports = null)
		{
            return new ExpressoAst(decls, imports, moduleName);
		}

        public static ImportDeclaration MakeImportDecl(Identifier path, string aliasName, TextLocation start = default, TextLocation end = default)
		{
            return new ImportDeclaration(new []{path}, new []{MakeIdentifier(aliasName)}, null, start, end);
		}

        public static ImportDeclaration MakeImportDecl(Identifier path, Identifier alias, TextLocation start = default, TextLocation end = default)
        {
            return new ImportDeclaration(new []{path}, new []{alias}, null, start, end);
        }

        public static ImportDeclaration MakeImportDecl(IEnumerable<Identifier> paths, IEnumerable<string> aliasNames, TextLocation start = default,
                                                       TextLocation end = default)
        {
            return new ImportDeclaration(paths, aliasNames.Select(n => MakeIdentifier(n)), null, start, end);
        }

        public static ImportDeclaration MakeImportDecl(IEnumerable<Identifier> paths, IEnumerable<Identifier> aliases, TextLocation start = default,
                                                       TextLocation end = default)
        {
            return new ImportDeclaration(paths, aliases, null, start, end);
        }

        public static ImportDeclaration MakeImportDecl(Identifier path, string alias, string fileName, TextLocation start = default,
                                                       TextLocation end = default)
        {
            return new ImportDeclaration(new []{path}, new []{MakeIdentifier(alias)}, MakeIdentifier(fileName), start, end);
        }

        public static ImportDeclaration MakeImportDecl(Identifier path, Identifier alias, Identifier file, TextLocation start = default,
                                                       TextLocation end = default)
        {
            return new ImportDeclaration(new []{path}, new []{alias}, file, start, end);
        }

        public static ImportDeclaration MakeImportDecl(IEnumerable<Identifier> paths, IEnumerable<string> aliasNames, string fileName, TextLocation start = default,
                                                       TextLocation end = default)
        {
            return new ImportDeclaration(paths, aliasNames.Select(n => MakeIdentifier(n)), MakeIdentifier(fileName), start, end);
        }

        public static ImportDeclaration MakeImportDecl(IEnumerable<Identifier> paths, IEnumerable<Identifier> aliases, Identifier file,
                                                       TextLocation start = default, TextLocation end = default)
        {
            return new ImportDeclaration(paths, aliases, file, start, end);
        }

        public static VariableInitializer MakeVariableInitializer(PatternWithType typedPattern, Expression expr)
        {
            return new VariableInitializer(typedPattern, expr);
        }
		#endregion
    }
}
