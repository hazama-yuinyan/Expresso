using System;
using System.Collections.Generic;
using System.Linq;
using ICSharpCode.NRefactory;
using ICSharpCode.NRefactory.PatternMatching;
using System.Collections;
using System.Diagnostics;


namespace Expresso.Ast
{
    /// <summary>
    /// Represents the children of an AstNode that has a specific role.
    /// </summary>
    public class AstNodeCollection<T> : ICollection<T>
    #if NET4_5
    , IReadOnlyCollection
    #endif
        where T : AstNode
    {
        readonly AstNode node;
        readonly Role<T> role;

        /// <summary>
        /// Indicates whether the node doesn't have any child node.
        /// Just the inverse of HasChildren.
        /// </summary>
        public bool IsEmpty => !node.HasChildren;

        /// <summary>
        /// Indicates whether the node has children.
        /// </summary>
        public bool HasChildren => node.HasChildren;

        public AstNodeCollection(AstNode node, Role<T> role)
        {
            if(node == null)
                throw new ArgumentNullException("node");

            if(role == null)
                throw new ArgumentNullException("role");

            this.node = node;
            this.role = role;
        }

        public void AddRange(IEnumerable<T> nodes)
        {
            // Evaluate "nodes" first, since it might change when we add the new children
            // "e.g. collection.AddRange(collection);
            if(nodes != null){
                foreach(var node in nodes.ToList())
                    Add(node);
            }
        }

        public void AddRange(T[] nodes)
        {
            // Fast overload for arrays - we don't need to create a copy
            if(nodes != null){
                foreach(var node in nodes)
                    Add(node);
            }
        }

        #region ICollection implementation
        public void Add(T item)
        {
            node.AddChild(item, role);
        }

        public void Clear()
        {
            foreach(var item in this)
                item.Remove();
        }

        public bool Contains(T item)
        {
            return item != null && item.Parent == node && item.RoleIndex == role.Index;
        }

        public void CopyTo(T[] array, int arrayIndex)
        {
            foreach(var item in this)
                array[arrayIndex++] = item;
        }

        public bool Remove(T item)
        {
            if(Contains(item)){
                item.Remove();
                return true;
            }else{
                return false;
            }
        }

        public int Count{
            get{
                uint role_index = role.Index;
                return node.Children.Count(sibling => sibling.RoleIndex == role_index);
            }
        }

        public bool IsReadOnly => false;
        #endregion

        #region IEnumerable implementation
        public IEnumerator<T> GetEnumerator()
        {
            uint role_index = role.Index;
            AstNode next;
            for(var cur = node.FirstChild; cur != null; cur = next){
                Debug.Assert(cur.Parent == node);
                // store cur on next
                // This allows removing/inserting nodes while iterating through the list
                next = cur.NextSibling;
                if(cur.RoleIndex == role_index)
                    yield return (T)cur;
            }
        }
        #endregion

        #region IEnumerable implementation
        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
        #endregion

        public override int GetHashCode()
        {
            return node.GetHashCode() ^ role.GetHashCode();
        }

        public override bool Equals(object obj)
        {
            AstNodeCollection<T> other = obj as AstNodeCollection<T>;
            if(other == null)
                return false;

            return node == other.node && role == other.role;
        }

        public void ReplaceWith(IEnumerable<T> nodes)
        {
            // Evaluate "nodes" first, since it might change when we call Clear()
            // e.g. collection.ReplaceWith(collection);
            if(nodes != null)
                nodes = nodes.ToList();

            Clear();

            if(nodes != null){
                foreach(var item in nodes)
                    Add(item);
            }
        }

        /// <summary>
        /// Returns the first element for which the predicate returns true,
        /// or NullObject(AstNode with IsNull == true) if no such object is found.
        /// </summary>
        public T FirstOrNullObject(Func<T, bool> predicate = null)
        {
            foreach(var item in this){
                if(predicate == null || predicate(item))
                    return item;
            }

            return role.NullObject;
        }

        /// <summary>
        /// Returns the last element for which the predicate returns true,
        /// or NullObject(AstNode with IsNull == true) if no such object is found.
        /// </summary>
        public T LastOrNullObject(Func<T, bool> predicate = null)
        {
            T result = role.NullObject;
            foreach(var item in this){
                if(predicate == null || predicate(item))
                    result = item;
            }

            return result;
        }

        internal bool DoMatch(AstNodeCollection<T> other, Match match)
        {
            return Pattern.DoMatchCollection(role, node.FirstChild, other.node.FirstChild, match);
        }

        public void InsertAfter(T existingItem, T newItem)
        {
            node.InsertChildAfter(existingItem, newItem, role);
        }

        public void InsertBefore(T existingItem, T newItem)
        {
            node.InsertChildBefore(existingItem, newItem, role);
        }

        /// <summary>
        /// Applies <paramref name="walker"/> on all child nodes of this collection.
        /// </summary>
        public void AcceptWalker(IAstWalker walker)
        {
            uint role_index = role.Index;
            foreach(var child in node.Children.Where(child => child.RoleIndex == role_index))
                child.AcceptWalker(walker);
        }
    }
}

